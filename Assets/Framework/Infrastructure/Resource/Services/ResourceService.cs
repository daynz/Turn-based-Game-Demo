using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BH.Framework.Events;
using BH.Framework.Infrastructure.Events.Core;
using BH.Framework.Infrastructure.Events.Interfaces;
using BH.Framework.Infrastructure.Logging.Interfaces;
using BH.Framework.Infrastructure.Resource.Data;
using BH.Framework.Infrastructure.Resource.Data.Events;
using BH.Framework.Infrastructure.Resource.Interfaces;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Zenject;
using Object = UnityEngine.Object;
using ResourceLoadStatus = BH.Framework.Infrastructure.Resource.Data.ResourceLoadStatus;

namespace BH.Framework.Infrastructure.Resource.Services
{
    /// <summary>
    /// 游戏资源管理高层服务类
    /// <para>核心能力：</para>
    /// <list type="bullet">
    /// <item>基于 LRU 策略的常驻资源缓存管理</item>
    /// <item>JSON 配置文件加载与缓存</item>
    /// <item>场景加载/卸载/激活的统一管理</item>
    /// <item>资源预加载分组管理</item>
    /// <item>类型安全的资源操作与异常处理</item>
    /// </list>
    /// <para>依赖：<see cref="IAddressableService"/> 底层资源服务、<see cref="ILogService"/> 日志服务、<see cref="IEventService"/> 事件服务</para>
    /// </summary>
    [UsedImplicitly]
    public class ResourceService : IResourceService
    {
        #region 依赖注入 & 配置

        /// <summary>
        /// 底层 Addressables 资源服务（依赖注入）
        /// </summary>
        [Inject] private readonly IAddressableService _addressableService;

        /// <summary>
        /// 日志服务（依赖注入）
        /// </summary>
        [Inject] private readonly ILogService _logService;

        /// <summary>
        /// 事件服务（依赖注入）
        /// </summary>
        [Inject] private readonly IEventService _eventService;

        /// <summary>
        /// 资源服务配置项（依赖注入）
        /// </summary>
        private readonly ResourceServiceConfig _config = new();

        #endregion

        #region 核心缓存 & 映射

        /// <summary>
        /// 线程安全锁对象，保护多线程下的缓存/映射操作
        /// </summary>
        private readonly object _lockObj = new();

        /// <summary>
        /// 常驻资源缓存字典（LRU 淘汰策略）
        /// </summary>
        /// <remarks>
        /// Key: AssetKeys.AddressableNames 常量定义的资源地址<br/>
        /// Value: (资源实例, 最后访问时间戳) - 时间戳用于 LRU 淘汰
        /// </remarks>
        private readonly Dictionary<string, (Object Asset, long LastAccessTick)> _persistentCache = new();

        /// <summary>
        /// 配置文件缓存字典（LRU 淘汰策略）
        /// </summary>
        /// <remarks>
        /// Key: AssetKeys.AddressableNames 常量定义的配置地址<br/>
        /// Value: (配置实例, 最后访问时间戳) - 时间戳用于 LRU 淘汰
        /// </remarks>
        private readonly Dictionary<string, (object Config, long LastAccessTick)> _configCache = new();

        /// <summary>
        /// 已加载场景映射表
        /// </summary>
        /// <remarks>
        /// Key: 场景逻辑名称（业务层使用）<br/>
        /// Value: AssetKeys.AddressableNames 常量定义的场景地址（底层使用）
        /// </remarks>
        private readonly Dictionary<string, string> _loadedScenes = new();

        /// <summary>
        /// 预加载资源组映射表
        /// </summary>
        /// <remarks>
        /// Key: 预加载组名称<br/>
        /// Value: AssetKeys.AddressableNames 常量列表
        /// </remarks>
        private readonly Dictionary<string, List<string>> _preloadGroups = new();

        #endregion

        #region 取消令牌 & 生命周期

        /// <summary>
        /// 全局取消令牌源，用于取消所有未完成的异步资源操作
        /// </summary>
        private CancellationTokenSource _globalCts;

        /// <summary>
        /// 服务初始化状态标记
        /// </summary>
        private bool _isInitialized;

        /// <summary>
        /// 服务释放状态标记（防止重复释放）
        /// </summary>
        private bool _isDisposed;

        #endregion

        #region 生命周期管理

        /// <summary>
        /// 初始化资源服务
        /// </summary>
        /// <remarks>
        /// 执行逻辑：<br/>
        /// 1. 初始化全局取消令牌<br/>
        /// 2. 初始化底层 Addressables 服务<br/>
        /// 3. 注册事件监听<br/>
        /// 4. 标记初始化完成状态
        /// </remarks>
        public void Initialize()
        {
            if (_isInitialized)
            {
                _logService.Warning("[ResourceService] 已初始化，跳过重复执行");
                return;
            }

            _globalCts = new CancellationTokenSource();

            _isInitialized = true;
            _logService.Info($"[ResourceService] 初始化完成（缓存容量: {_config.PersistentCacheMaxCount}）");
        }

        /// <summary>
        /// 启用资源服务
        /// </summary>
        /// <exception cref="InvalidOperationException">服务未初始化时抛出警告日志</exception>
        public void Enable()
        {
            if (!_isInitialized)
            {
                _logService.Error("[ResourceService] 未初始化，无法启用");
                return;
            }

            // 注册事件监听
            SubscribeEvents();

            _logService.Info("[ResourceService] 已启用");
        }

        /// <summary>
        /// 禁用资源服务
        /// </summary>
        /// <remarks>
        /// 执行逻辑：<br/>
        /// 1. 取消所有未完成的异步操作<br/>
        /// 2. 清理预加载组临时状态<br/>
        /// 3. 输出禁用日志
        /// </remarks>
        public void Disable()
        {
            if (!_isInitialized) return;

            // 取消所有未完成操作
            _globalCts.Cancel();
            UnsubscribeEvents();
            // 清理临时状态
            lock (_lockObj)
            {
                _preloadGroups.Clear();
            }

            _logService.Info("[ResourceService] 已禁用");
        }

        /// <summary>
        /// 释放资源服务（实现 IDisposable 接口）
        /// </summary>
        /// <remarks>
        /// 托管资源释放逻辑：<br/>
        /// 1. 取消并释放全局取消令牌<br/>
        /// 2. 清理所有缓存/映射表<br/>
        /// 3. 释放底层 Addressables 服务<br/>
        /// 4. 取消事件监听<br/>
        /// 5. 标记释放完成状态
        /// </remarks>
        public void Dispose()
        {
            if (_isDisposed) return;
            Disable();
            // 取消全局操作
            _globalCts?.Dispose();
            // 清理缓存
            lock (_lockObj)
            {
                _persistentCache?.Clear();
                _configCache?.Clear();
                _loadedScenes?.Clear();
                _preloadGroups?.Clear();
            }

            _isDisposed = true;
            _logService.Info("[ResourceService] 已释放所有资源");
        }

        /// <summary>
        /// 注册资源服务相关事件监听
        /// </summary>
        /// <remarks>当前注册 <see cref="GameCheckModuleStatusEvent"/> 事件，触发预加载流程</remarks>
        private void SubscribeEvents()
        {
            _eventService.Subscribe<ResourcePreLoadEvent>(HandleResourcePreload);
        }

        /// <summary>
        /// 取消资源服务相关事件监听
        /// </summary>
        private void UnsubscribeEvents()
        {
            //_eventService.Unsubscribe<ResourcePreLoadEvent>();
        }

        #endregion

        #region 核心事件处理

        /// <summary>
        /// 处理模块状态检查事件，触发资源预加载流程
        /// </summary>
        /// <param name="event">模块状态检查事件参数</param>
        /// <exception cref="OperationCanceledException">预加载流程被取消时抛出</exception>
        /// <exception cref="Exception">预加载流程异常时捕获并记录日志</exception>
        private async void HandleResourcePreload(ResourcePreLoadEvent @event)
        {
            try
            {
                if (_globalCts.Token.IsCancellationRequested)
                {
                    _logService.Warning("[ResourceService] 预加载流程已取消，跳过执行");
                    return;
                }

                _logService.Info("[ResourceService] 开始执行资源配置预加载流程");

                // 执行预加载
                await PreloadAllResourcesAsync();

                _logService.Info("[ResourceService] 资源配置预加载流程执行完毕");
            }
            catch (OperationCanceledException)
            {
                _logService.Warning("[ResourceService] 预加载流程被取消");
            }
            catch (Exception ex)
            {
                _logService.Error($"预加载失败: {ex.Message}");

                // 触发失败回调
                // //OnResourceLoadFailed?.Invoke("PreloadAll", ex);

                // 发布模块错误事件
                // _eventService.Publish(EventBuilder.Create<GameModuleErrorEvent>()
                //     .WithSender(this)
                //     .WithData(new GameModuleErrorData { ModuleName = "ResourceService", ErrorMsg = errorMsg })
                //     .Build());
            }
        }

        #endregion

        #region 预加载管理

        /// <summary>
        /// 预加载所有已注册的资源组和核心配置文件
        /// </summary>
        /// <returns>异步任务</returns>
        /// <exception cref="OperationCanceledException">预加载被取消时抛出</exception>
        /// <exception cref="Exception">预加载异常时捕获并记录日志，重新抛出异常</exception>
        public async Task PreloadAllResourcesAsync()
        {
            if (_globalCts.Token.IsCancellationRequested)
            {
                _logService.Warning("[ResourceService] 预加载已取消，跳过执行");
                return;
            }

            try
            {
                AddPreloadGroup("DefaultPreloadGroup", new List<string>
                {
                });
                AddPreloadGroup("UIPreloadGroup", new List<string>
                {
                    AssetKeys.AddressableNames.MainMenuUI
                });

                var allGroups = GetAllPreloadGroups();
                var totalGroups = allGroups.Count;

                _logService.Info($"[ResourceService] 开始预加载 {totalGroups} 个资源组");

                // 批量预加载资源组
                foreach (var groupName in allGroups.TakeWhile(_ => !_globalCts.Token.IsCancellationRequested))
                {
                    await PreloadGroupAsync(groupName);
                }

                // 发布预加载完成事件
                _eventService.Publish(EventBuilder.CreateForEmptyData<GamePreloadCompleteEvent>()
                    .WithSender(this)
                    .Build());

                _logService.Info("[ResourceService] 资源和配置预加载完成");
            }
            catch (OperationCanceledException)
            {
                _logService.Warning("[ResourceService] 预加载流程被取消");
                throw;
            }
            catch (Exception ex)
            {
                _logService.Error($"预加载异常: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 添加预加载资源组
        /// </summary>
        /// <param name="groupName">预加载组名称（非空）</param>
        /// <param name="resourceNames">AssetKeys.AddressableNames 常量定义的资源地址列表</param>
        /// <exception cref="ArgumentNullException">资源列表为 null 时输出警告日志</exception>
        /// <exception cref="ArgumentException">组名称为空时输出错误日志</exception>
        public void AddPreloadGroup(string groupName, List<string> resourceNames)
        {
            if (string.IsNullOrEmpty(groupName))
            {
                _logService.Error("[ResourceService] 预加载组名称为空，无法添加");
                return;
            }

            if (resourceNames == null || resourceNames.Count == 0)
            {
                _logService.Warning($"[ResourceService] 预加载组 '{groupName}' 传入空资源列表");
                return;
            }

            List<string> newResources;
            lock (_lockObj)
            {
                if (!_preloadGroups.ContainsKey(groupName))
                {
                    _preloadGroups[groupName] = new List<string>();
                }

                // 去重添加
                newResources = resourceNames
                    .Where(name => !string.IsNullOrEmpty(name))
                    .Except(_preloadGroups[groupName])
                    .ToList();

                _preloadGroups[groupName].AddRange(newResources);
            }

            _logService.Info(
                $"[ResourceService] 预加载组 '{groupName}' 新增 {newResources.Count} 个资源（总计: {_preloadGroups[groupName].Count}）");
        }

        /// <summary>
        /// 异步加载指定预加载组的所有资源
        /// </summary>
        /// <param name="groupName">预加载组名称（非空）</param>
        /// <param name="cancellationToken">取消令牌（可选）</param>
        /// <returns>异步任务</returns>
        /// <remarks>
        /// 执行逻辑：<br/>
        /// 1. 参数校验 and 线程安全获取资源列表<br/>
        /// 2. 分批并行加载资源（优化性能）<br/>
        /// 3. 单个资源加载异常不影响整组加载<br/>
        /// 4. 支持取消操作
        /// </remarks>
        public async Task PreloadGroupAsync(string groupName, CancellationToken cancellationToken = default)
        {
            // 基础校验
            if (string.IsNullOrEmpty(groupName))
            {
                _logService.Error("[ResourceService] 预加载组名称为空，无法加载");
                return;
            }

            // 线程安全获取资源列表
            List<string> resourceList;
            lock (_lockObj)
            {
                if (!_preloadGroups.TryGetValue(groupName, out resourceList))
                {
                    _logService.Warning($"[ResourceService] 预加载组 '{groupName}' 不存在");
                    return;
                }

                resourceList = new List<string>(resourceList); // 复制避免遍历中修改
            }

            if (resourceList.Count == 0)
            {
                _logService.Warning($"[ResourceService] 预加载组 '{groupName}' 无资源可加载");
                return;
            }

            // 合并取消令牌
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_globalCts.Token, cancellationToken);
            var totalCount = resourceList.Count;
            var completedCount = 0f;

            try
            {
                _logService.Info($"[ResourceService] 开始预加载组 '{groupName}'（{totalCount} 个资源）");

                //todo:
                int preloadBatchSize = 0;
                //preloadBatchSize = _config.PreloadBatchSize;
                // 分批并行加载（优化性能）
                for (int i = 0; i < totalCount; i += preloadBatchSize)
                {
                    linkedCts.Token.ThrowIfCancellationRequested();

                    var batch = resourceList.Skip(i).Take(preloadBatchSize).ToList();
                    var batchTasks = new List<Task>();

                    foreach (var resourceName in batch)
                    {
                        // 加载为常驻资源
                        batchTasks.Add(LoadPersistentAssetAsync<Object>(resourceName, linkedCts.Token)
                            .ContinueWith(task =>
                            {
                                lock (_lockObj) completedCount++;
                                //OnPreloadProgress?.Invoke(completedCount / totalCount);

                                // 捕获单资源加载异常
                                if (task.Exception != null)
                                {
                                    _logService.Error(
                                        $"[ResourceService] 预加载组 '{groupName}' 资源 '{resourceName}' 加载失败: {task.Exception.Message}");
                                    //OnResourceLoadFailed?.Invoke(resourceName, task.Exception);
                                }
                            }, linkedCts.Token));
                    }

                    await Task.WhenAll(batchTasks);
                }

                _logService.Info($"[ResourceService] 预加载组 '{groupName}' 完成（{completedCount}/{totalCount}）");
            }
            catch (OperationCanceledException)
            {
                _logService.Warning($"[ResourceService] 预加载组 '{groupName}' 被取消");
            }
            catch (Exception ex)
            {
                _logService.Error($"[ResourceService] 预加载组 '{groupName}' 异常: {ex.Message}\n{ex.StackTrace}");
                //OnResourceLoadFailed?.Invoke(groupName, ex);
            }
            finally
            {
                linkedCts.Dispose();
            }
        }

        /// <summary>
        /// 获取所有已注册的预加载组名称
        /// </summary>
        /// <returns>预加载组名称列表</returns>
        public List<string> GetAllPreloadGroups()
        {
            lock (_lockObj)
            {
                return _preloadGroups.Keys.ToList();
            }
        }

        #endregion

        #region 常驻资源管理

        /// <summary>
        /// 异步加载常驻资源（带 LRU 缓存）
        /// </summary>
        /// <typeparam name="T">资源类型（继承自 <see cref="Object"/>）</typeparam>
        /// <param name="resourceName">AssetKeys.AddressableNames 常量定义的资源地址</param>
        /// <param name="cancellationToken">取消令牌（可选）</param>
        /// <returns>资源加载结果对象 <see cref="ResourceLoadResult{T}"/></returns>
        /// <remarks>
        /// 执行逻辑：<br/>
        /// 1. 参数校验 → 2. 缓存命中检查（更新访问时间） → 3. 底层加载 → 4. 缓存存储（触发 LRU 淘汰）<br/>
        /// 缓存淘汰策略：当缓存数量超过 <see cref="ResourceServiceConfig.PersistentCacheMaxCount"/> 时，淘汰最久未访问的资源
        /// </remarks>
        public async Task<ResourceLoadResult<T>> LoadPersistentAssetAsync<T>(string resourceName,
            CancellationToken cancellationToken = default)
            where T : Object
        {
            if (string.IsNullOrEmpty(resourceName))
            {
                var errorMsg = $"[ResourceService] 资源名称 {resourceName} 为空，无法加载常驻资源";
                _logService.Error(errorMsg);

                return new ResourceLoadResult<T>
                {
                    Status = ResourceLoadStatus.Failed,
                    ErrorMessage = errorMsg
                };
            }

            // 检查缓存（LRU 更新访问时间）
            lock (_lockObj)
            {
                if (_persistentCache.TryGetValue(resourceName, out var cacheItem))
                {
                    if (cacheItem.Asset is T typedAsset)
                    {
                        // 更新访问时间（使用 Tick 提升性能，避免 DateTime 装箱）
                        _persistentCache[resourceName] = (typedAsset, DateTime.UtcNow.Ticks);
                        _logService.Info($"[ResourceService] 常驻缓存命中: {resourceName}（类型: {typeof(T).Name}）");
                        return new ResourceLoadResult<T>
                        {
                            Asset = typedAsset,
                            Status = ResourceLoadStatus.AlreadyLoaded
                        };
                    }

                    var errorMsg =
                        $"[ResourceService] 常驻缓存类型不匹配: {resourceName} 期望 {typeof(T).Name}，实际 {cacheItem.Asset.GetType().Name}";
                    _logService.Error(errorMsg);
                    return new ResourceLoadResult<T>
                    {
                        Status = ResourceLoadStatus.Failed,
                        ErrorMessage = errorMsg
                    };
                }
            }

            // 从 Addressables 加载
            try
            {
                var asset = await _addressableService.LoadAssetAsync<T>(resourceName, cancellationToken);
                if (!asset)
                {
                    var errorMsg = $"[ResourceService] 加载常驻资源失败: {resourceName}";
                    _logService.Error(errorMsg);
                    return new ResourceLoadResult<T>
                    {
                        Status = ResourceLoadStatus.Failed,
                        ErrorMessage = errorMsg
                    };
                }

                // 加入缓存（触发 LRU 淘汰）
                lock (_lockObj)
                {
                    // LRU 淘汰优化：仅当超过容量时执行一次排序
                    if (_persistentCache.Count >= _config.PersistentCacheMaxCount)
                    {
                        EvictLRUPersistentAsset();
                    }

                    _persistentCache[resourceName] = (asset, DateTime.UtcNow.Ticks);
                }

                _logService.Info($"[ResourceService] 常驻资源加载成功: {resourceName}（类型: {typeof(T).Name}）");
                return new ResourceLoadResult<T>
                {
                    Asset = asset,
                    Status = ResourceLoadStatus.Success
                };
            }
            catch (OperationCanceledException)
            {
                _logService.Warning($"[ResourceService] 常驻资源加载取消: {resourceName}");
                return new ResourceLoadResult<T>
                {
                    Status = ResourceLoadStatus.Cancelled,
                    ErrorMessage = "加载被取消"
                };
            }
            catch (Exception ex)
            {
                _logService.Error($"[ResourceService] 常驻资源加载异常: {resourceName}\n{ex.StackTrace}");
                return new ResourceLoadResult<T>
                {
                    Status = ResourceLoadStatus.Failed,
                    ErrorMessage = ex.Message,
                    Exception = ex
                };
            }
        }

        /// <summary>
        /// 卸载常驻资源
        /// </summary>
        /// <param name="resourceName">AssetKeys.AddressableNames 常量定义的资源地址</param>
        /// <param name="forceRelease">是否强制释放（忽略引用计数）</param>
        /// <exception cref="ArgumentException">资源名称为空时输出错误日志</exception>
        public void UnloadPersistentAsset(string resourceName, bool forceRelease = false)
        {
            if (string.IsNullOrEmpty(resourceName))
            {
                _logService.Error("[ResourceService] 资源名称为空，无法卸载常驻资源");
                return;
            }

            lock (_lockObj)
            {
                if (_persistentCache.Remove(resourceName, out var cacheItem))
                {
                    // 释放底层资源
                    _addressableService.ReleaseAsset(resourceName, forceRelease);
                    _logService.Info($"[ResourceService] 常驻资源卸载成功: {resourceName}");
                }
                else
                {
                    _logService.Warning($"[ResourceService] 常驻缓存中未找到资源: {resourceName}");
                }
            }
        }

        /// <summary>
        /// LRU 淘汰最久未使用的常驻资源
        /// </summary>
        /// <remarks>
        /// 淘汰逻辑：<br/>
        /// 1. 按最后访问时间戳升序排序<br/>
        /// 2. 移除第一个元素（最久未访问）<br/>
        /// 3. 调用底层服务释放该资源
        /// </remarks>
        private void EvictLRUPersistentAsset()
        {
            var lruKey = _persistentCache.OrderBy(x => x.Value.LastAccessTick).First().Key;
            if (!_persistentCache.Remove(lruKey, out var lruItem)) return;
            _addressableService.ReleaseAsset(lruKey, true);
            _logService.Info($"[ResourceService] LRU 淘汰常驻资源: {lruKey}");
        }

        /// <summary>
        /// 检查指定资源是否在常驻缓存中
        /// </summary>
        /// <param name="resourceName">AssetKeys.AddressableNames 常量定义的资源地址</param>
        /// <returns>存在返回 true，否则返回 false</returns>
        /// <remarks>线程安全检查</remarks>
        public bool IsPersistentAssetCached(string resourceName)
        {
            if (string.IsNullOrEmpty(resourceName)) return false;

            lock (_lockObj)
            {
                return _persistentCache.ContainsKey(resourceName);
            }
        }

        #endregion

        #region 配置加载

        /// <summary>
        /// 异步加载 JSON 配置文件（带缓存）
        /// </summary>
        /// <typeparam name="T">配置对象类型</typeparam>
        /// <param name="configName">AssetKeys.AddressableNames 常量定义的配置地址</param>
        /// <param name="useCache">是否使用缓存（默认 true）</param>
        /// <returns>配置加载结果对象 <see cref="ResourceLoadResult{T}"/></returns>
        /// <remarks>
        /// 执行逻辑：<br/>
        /// 1. 参数校验 → 2. 缓存命中检查 → 3. 加载 TextAsset → 4. JSON 反序列化 → 5. 缓存存储（触发 LRU 淘汰）<br/>
        /// 反序列化配置：忽略空值、缺失成员、循环引用
        /// </remarks>
        public async Task<ResourceLoadResult<T>> LoadJsonConfigAsync<T>(string configName, bool useCache = true)
        {
            // 入参校验
            if (string.IsNullOrEmpty(configName))
            {
                var errorMsg = "[ResourceService] 配置名称为空，无法加载 JSON 配置";
                _logService.Error(errorMsg);
                return new ResourceLoadResult<T>
                {
                    Status = ResourceLoadStatus.Failed,
                    ErrorMessage = errorMsg
                };
            }

            // 1. 检查缓存
            if (useCache)
            {
                lock (_lockObj)
                {
                    if (_configCache.TryGetValue(configName, out var cacheItem))
                    {
                        if (cacheItem.Config is T typedConfig)
                        {
                            _configCache[configName] = (typedConfig, DateTime.UtcNow.Ticks);
                            _logService.Info($"[ResourceService] 配置缓存命中: {configName}（类型: {typeof(T).Name}）");
                            return new ResourceLoadResult<T>
                            {
                                Asset = typedConfig,
                                Status = ResourceLoadStatus.AlreadyLoaded
                            };
                        }

                        var errorMsg =
                            $"[ResourceService] 配置缓存类型不匹配: {configName} 期望 {typeof(T).Name}，实际 {cacheItem.Config.GetType().Name}";
                        _logService.Error(errorMsg);
                        return new ResourceLoadResult<T>
                        {
                            Status = ResourceLoadStatus.Failed,
                            ErrorMessage = errorMsg
                        };
                    }
                }
            }

            // 2. 加载配置文件
            try
            {
                var textAssetResult = await LoadPersistentAssetAsync<TextAsset>(configName, _globalCts.Token);
                if (textAssetResult.Status != ResourceLoadStatus.Success || !textAssetResult.Asset)
                {
                    var errorMsg = $"[ResourceService] 加载配置文件失败: {configName}";
                    _logService.Error(errorMsg);
                    return new ResourceLoadResult<T>
                    {
                        Status = ResourceLoadStatus.Failed,
                        ErrorMessage = errorMsg
                    };
                }

                // 3. 反序列化
                var jsonContent = textAssetResult.Asset.text;
                var config = JsonConvert.DeserializeObject<T>(jsonContent, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

                if (config == null)
                {
                    var errorMsg = $"[ResourceService] 配置反序列化失败: {configName}";
                    _logService.Error(errorMsg);
                    return new ResourceLoadResult<T>
                    {
                        Status = ResourceLoadStatus.Failed,
                        ErrorMessage = errorMsg
                    };
                }

                // 4. 缓存配置
                if (useCache && _config.EnableConfigCacheAutoClean)
                {
                    lock (_lockObj)
                    {
                        if (_configCache.Count >= _config.ConfigCacheMaxCount)
                        {
                            EvictLRUConfig();
                        }

                        _configCache[configName] = (config, DateTime.UtcNow.Ticks);
                    }
                }

                _logService.Info($"[ResourceService] JSON 配置加载成功: {configName}（类型: {typeof(T).Name}）");
                return new ResourceLoadResult<T>
                {
                    Asset = config,
                    Status = ResourceLoadStatus.Success
                };
            }
            catch (Exception ex)
            {
                _logService.Error($"[ResourceService] 配置加载异常: {configName}\n{ex.StackTrace}");
                //OnResourceLoadFailed?.Invoke(configName, ex);
                return new ResourceLoadResult<T>
                {
                    Status = ResourceLoadStatus.Failed,
                    ErrorMessage = ex.Message,
                    Exception = ex
                };
            }
        }

        /// <summary>
        /// LRU 淘汰最久未使用的配置缓存
        /// </summary>
        /// <remarks>
        /// 淘汰逻辑：<br/>
        /// 1. 按最后访问时间戳升序排序<br/>
        /// 2. 移除第一个元素（最久未访问）<br/>
        /// 3. 输出淘汰日志
        /// </remarks>
        private void EvictLRUConfig()
        {
            var lruKey = _configCache.OrderBy(x => x.Value.LastAccessTick).First().Key;
            _configCache.Remove(lruKey);
            _logService.Info($"[ResourceService] LRU 淘汰配置缓存: {lruKey}");
        }

        /// <summary>
        /// 清理配置缓存
        /// </summary>
        /// <param name="configName">配置名称（null 或空字符串表示清理全部）</param>
        /// <remarks>线程安全清理</remarks>
        public void ClearConfigCache(string configName = null)
        {
            lock (_lockObj)
            {
                if (string.IsNullOrEmpty(configName))
                {
                    _configCache.Clear();
                    _logService.Info("[ResourceService] 配置缓存已清空");
                }
                else if (_configCache.Remove(configName))
                {
                    _logService.Info($"[ResourceService] 配置缓存清理成功: {configName}");
                }
                else
                {
                    _logService.Warning($"[ResourceService] 配置缓存中未找到: {configName}");
                }
            }
        }

        #endregion

        #region 场景管理

        /// <summary>
        /// 异步加载场景（不自动激活）
        /// </summary>
        /// <param name="sceneLogicalName">场景逻辑名称（业务层标识）</param>
        /// <param name="sceneAddress">AssetKeys.AddressableNames 常量定义的场景地址</param>
        /// <param name="loadMode">场景加载模式（默认叠加加载）</param>
        /// <returns>场景加载结果对象 <see cref="ResourceLoadResult{SceneInstance}"/></returns>
        /// <exception cref="ArgumentException">逻辑名称/地址为空时返回失败结果</exception>
        public async Task<ResourceLoadResult<SceneInstance>> LoadSceneAsync(string sceneLogicalName,
            string sceneAddress,
            LoadSceneMode loadMode = LoadSceneMode.Additive)
        {
            // 入参校验
            if (string.IsNullOrEmpty(sceneLogicalName) || string.IsNullOrEmpty(sceneAddress))
            {
                var errorMsg = "[ResourceService] 场景逻辑名称/地址为空，无法加载";
                _logService.Error(errorMsg);
                return new ResourceLoadResult<SceneInstance>
                {
                    Status = ResourceLoadStatus.Failed,
                    ErrorMessage = errorMsg
                };
            }

            // 检查是否已加载
            if (IsSceneLoaded(sceneLogicalName))
            {
                _logService.Warning($"[ResourceService] 场景已加载: {sceneLogicalName}（地址: {sceneAddress}）");
                return new ResourceLoadResult<SceneInstance>
                {
                    Status = ResourceLoadStatus.AlreadyLoaded
                };
            }

            // 加载场景
            try
            {
                var sceneInstance = await _addressableService.LoadSceneAsync(sceneAddress, loadMode, _globalCts.Token);
                if (!sceneInstance.Scene.IsValid())
                {
                    var errorMsg = $"[ResourceService] 场景加载无效: {sceneLogicalName}（地址: {sceneAddress}）";
                    _logService.Error(errorMsg);
                    return new ResourceLoadResult<SceneInstance>
                    {
                        Status = ResourceLoadStatus.Failed,
                        ErrorMessage = errorMsg
                    };
                }

                // 记录已加载场景
                lock (_lockObj)
                {
                    _loadedScenes[sceneLogicalName] = sceneAddress;
                }

                _logService.Info($"[ResourceService] 场景加载成功: {sceneLogicalName}（地址: {sceneAddress}）");
                return new ResourceLoadResult<SceneInstance>
                {
                    Asset = sceneInstance,
                    Status = ResourceLoadStatus.Success
                };
            }
            catch (Exception ex)
            {
                _logService.Error($"[ResourceService] 场景加载异常: {sceneLogicalName}\n{ex.StackTrace}");
                //OnResourceLoadFailed?.Invoke(sceneLogicalName, ex);
                return new ResourceLoadResult<SceneInstance>
                {
                    Status = ResourceLoadStatus.Failed,
                    ErrorMessage = ex.Message,
                    Exception = ex
                };
            }
        }

        /// <summary>
        /// 异步加载并激活场景
        /// </summary>
        /// <param name="sceneLogicalName">场景逻辑名称（业务层标识）</param>
        /// <param name="sceneAddress">AssetKeys.AddressableNames 常量定义的场景地址</param>
        /// <param name="loadMode">场景加载模式（默认叠加加载）</param>
        /// <returns>场景加载结果对象 <see cref="ResourceLoadResult{SceneInstance}"/></returns>
        /// <remarks>
        /// 执行逻辑：<br/>
        /// 1. 调用 <see cref="LoadSceneAsync"/> 加载场景<br/>
        /// 2. 激活场景<br/>
        /// 3. 激活失败时自动卸载场景并返回失败结果
        /// </remarks>
        public async Task<ResourceLoadResult<SceneInstance>> LoadAndActivateSceneAsync(string sceneLogicalName,
            string sceneAddress, LoadSceneMode loadMode = LoadSceneMode.Additive)
        {
            var loadResult = await LoadSceneAsync(sceneLogicalName, sceneAddress, loadMode);
            if (loadResult.Status != ResourceLoadStatus.Success || !loadResult.Asset.Scene.IsValid())
            {
                return loadResult;
            }

            // 激活场景
            try
            {
                await loadResult.Asset.ActivateAsync();
                _logService.Info($"[ResourceService] 场景已激活: {sceneLogicalName}");
                return loadResult;
            }
            catch (Exception ex)
            {
                _logService.Error($"[ResourceService] 场景激活异常: {sceneLogicalName}\n{ex.StackTrace}");
                //OnResourceLoadFailed?.Invoke(sceneLogicalName, ex);

                // 激活失败自动卸载
                await UnloadSceneAsync(sceneLogicalName, true);

                return new ResourceLoadResult<SceneInstance>
                {
                    Status = ResourceLoadStatus.Failed,
                    ErrorMessage = ex.Message,
                    Exception = ex
                };
            }
        }

        /// <summary>
        /// 异步卸载场景
        /// </summary>
        /// <param name="sceneLogicalName">场景逻辑名称（业务层标识）</param>
        /// <param name="forceRelease">是否强制卸载（忽略引用计数）</param>
        /// <returns>卸载成功返回 true，否则返回 false</returns>
        /// <remarks>线程安全操作，卸载成功后移除场景映射记录</remarks>
        public async Task<bool> UnloadSceneAsync(string sceneLogicalName, bool forceRelease = false)
        {
            if (string.IsNullOrEmpty(sceneLogicalName))
            {
                _logService.Error("[ResourceService] 场景逻辑名称为空，无法卸载");
                return false;
            }

            // 获取场景地址
            string sceneAddress;
            lock (_lockObj)
            {
                if (!_loadedScenes.TryGetValue(sceneLogicalName, out sceneAddress))
                {
                    _logService.Warning($"[ResourceService] 场景未加载，无需卸载: {sceneLogicalName}");
                    return false;
                }
            }

            // 卸载场景
            var unloadSuccess = await _addressableService.UnloadSceneAsync(sceneAddress, forceRelease);
            if (unloadSuccess)
            {
                lock (_lockObj)
                {
                    _loadedScenes.Remove(sceneLogicalName);
                }

                _logService.Info($"[ResourceService] 场景卸载成功: {sceneLogicalName}（地址: {sceneAddress}）");
            }
            else
            {
                _logService.Error($"[ResourceService] 场景卸载失败: {sceneLogicalName}（地址: {sceneAddress}）");
            }

            return unloadSuccess;
        }

        /// <summary>
        /// 检查场景是否已加载且有效
        /// </summary>
        /// <param name="sceneLogicalName">场景逻辑名称（业务层标识）</param>
        /// <returns>已加载且有效返回 true，否则返回 false</returns>
        /// <remarks>
        /// 检查逻辑：<br/>
        /// 1. 检查场景映射表是否存在<br/>
        /// 2. 调用底层服务检查场景是否真的加载且有效
        /// </remarks>
        public bool IsSceneLoaded(string sceneLogicalName)
        {
            if (string.IsNullOrEmpty(sceneLogicalName)) return false;

            lock (_lockObj)
            {
                if (!_loadedScenes.TryGetValue(sceneLogicalName, out var sceneAddress))
                {
                    return false;
                }

                return _addressableService.IsSceneLoaded(sceneAddress);
            }
        }

        #endregion

        #region 预制体实例化

        /// <summary>
        /// 异步实例化预制体
        /// </summary>
        /// <param name="prefabName">AssetKeys.AddressableNames 常量定义的预制体地址</param>
        /// <param name="parent">实例化的父节点（可选）</param>
        /// <param name="instantiateInWorldSpace">是否使用世界空间坐标（默认 false）</param>
        /// <returns>预制体实例化结果对象 <see cref="ResourceLoadResult{GameObject}"/></returns>
        /// <exception cref="ArgumentException">预制体名称为空时返回失败结果</exception>
        public async Task<ResourceLoadResult<GameObject>> InstantiatePrefabAsync(string prefabName,
            Transform parent = null, bool instantiateInWorldSpace = false)
        {
            if (string.IsNullOrEmpty(prefabName))
            {
                var errorMsg = "[ResourceService] 预制体名称为空，无法实例化";
                _logService.Error(errorMsg);
                return new ResourceLoadResult<GameObject>
                {
                    Status = ResourceLoadStatus.Failed,
                    ErrorMessage = errorMsg
                };
            }

            try
            {
                var prefabInstance = await _addressableService.InstantiatePrefabAsync(
                    prefabName, parent, instantiateInWorldSpace, _globalCts.Token);

                if (prefabInstance == null)
                {
                    var errorMsg = $"[ResourceService] 预制体实例化失败: {prefabName}";
                    _logService.Error(errorMsg);
                    return new ResourceLoadResult<GameObject>
                    {
                        Status = ResourceLoadStatus.Failed,
                        ErrorMessage = errorMsg
                    };
                }

                _logService.Info($"[ResourceService] 预制体实例化成功: {prefabName}");
                return new ResourceLoadResult<GameObject>
                {
                    Asset = prefabInstance,
                    Status = ResourceLoadStatus.Success
                };
            }
            catch (Exception ex)
            {
                _logService.Error($"[ResourceService] 预制体实例化异常: {prefabName}\n{ex.StackTrace}");
                //OnResourceLoadFailed?.Invoke(prefabName, ex);
                return new ResourceLoadResult<GameObject>
                {
                    Status = ResourceLoadStatus.Failed,
                    ErrorMessage = ex.Message,
                    Exception = ex
                };
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 获取已加载场景的逻辑名称列表
        /// </summary>
        /// <returns>已加载场景逻辑名称列表（线程安全）</returns>
        public List<string> GetLoadedScenes()
        {
            lock (_lockObj)
            {
                return _loadedScenes.Keys.ToList();
            }
        }

        /// <summary>
        /// 获取常驻缓存中的资源数量
        /// </summary>
        /// <returns>常驻缓存资源数量（线程安全）</returns>
        public int GetPersistentCacheCount()
        {
            lock (_lockObj)
            {
                return _persistentCache.Count;
            }
        }

        #endregion
    }
}