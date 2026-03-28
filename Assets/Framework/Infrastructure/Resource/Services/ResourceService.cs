using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BH.Framework.Infrastructure.Events.Core;
using BH.Framework.Infrastructure.Events.Interfaces;
using BH.Framework.Infrastructure.Logging.Interfaces;
using BH.Framework.Infrastructure.Resource.Data;
using BH.Framework.Infrastructure.Resource.Data.Events;
using BH.Framework.Infrastructure.Resource.Interfaces;
using BH.Framework.Utilities.Collections;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;
using Object = UnityEngine.Object;
using ResourceLoadStatus = BH.Framework.Infrastructure.Resource.Data.ResourceLoadStatus;

namespace BH.Framework.Infrastructure.Resource.Services
{
    /// <summary>
    /// 游戏资源管理服务类
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
        private readonly ExpiredLruCache<string, AsyncOperationHandle> _persistentCache = new(50);

        /// <summary>
        /// 配置文件缓存字典（LRU 淘汰策略）
        /// </summary>
        /// <remarks>
        /// Key: AssetKeys.AddressableNames 常量定义的配置地址<br/>
        /// Value: (配置实例, 最后访问时间戳) - 时间戳用于 LRU 淘汰
        /// </remarks>
        private readonly ExpiredLruCache<string, object> _configCache = new(10);

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
        private CancellationTokenSource _globalCts = new();

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
            _persistentCache.OnItemEvicted += OnPersistentAssetEvicted;
            Enable();
            _logService.Info($"[ResourceService] 初始化完成（缓存容量: {_config.PersistentCacheMaxCount}）");
        }

        /// <summary>
        /// 启用资源服务
        /// </summary>
        public void Enable()
        {
            // 注册事件监听
            SubscribeEvents();

            _logService.Info("[ResourceService] 已启用");
        }

        /// <summary>
        /// 禁用资源服务
        /// </summary>
        public void Disable()
        {
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
        /// 释放资源服务
        /// </summary>
        public void Dispose()
        {
            Disable();
            // 取消全局操作
            _globalCts?.Dispose();
            // 清理缓存
            lock (_lockObj)
            {
                _persistentCache?.Clear();
                _configCache?.Clear();
                _preloadGroups?.Clear();
            }

            _logService.Info("[ResourceService] 已释放所有资源");
        }

        /// <summary>
        /// 注册资源服务相关事件监听
        /// </summary>
        /// <remarks>当前注册 <see cref="ResourcePreLoadEvent"/> 事件，触发预加载流程</remarks>
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
                await PreloadAllResourcesAsync();
                _logService.Info("[ResourceService] 资源配置预加载流程执行完毕");
            }
            catch (OperationCanceledException)
            {
                _logService.Warning("[ResourceService] 预加载流程被取消");
            }
            catch (Exception)
            {
                _logService.Error("[ResourceService] 预加载失败");
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
                AddPreloadGroup("DefaultPreloadGroup", new List<string>());
                AddPreloadGroup("UIPreloadGroup", new List<string>
                {
                    AssetKeys.AddressableNames.UIMainMenu,
                });

                var allGroups = GetAllPreloadGroups();
                _logService.Info($"[ResourceService] 开始预加载 {allGroups.Count} 个资源组");

                foreach (var groupName in allGroups.TakeWhile(_ => !_globalCts.Token.IsCancellationRequested))
                {
                    await PreloadGroupAsync(groupName);
                }

                _eventService.Publish(EventBuilder.CreateForEmptyData<GamePreloadCompleteEvent>().WithSender(this)
                    .Build());
                _logService.Info("[ResourceService] 资源和配置预加载完成");
            }
            catch (OperationCanceledException)
            {
                _logService.Warning("[ResourceService] 预加载流程被取消");
                throw;
            }
            catch (Exception)
            {
                _logService.Error("[ResourceService] 预加载异常");
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

            lock (_lockObj)
            {
                if (!_preloadGroups.ContainsKey(groupName))
                    _preloadGroups[groupName] = new List<string>();

                var newResources = resourceNames
                    .Where(name => !string.IsNullOrEmpty(name))
                    .Except(_preloadGroups[groupName])
                    .ToList();

                _preloadGroups[groupName].AddRange(newResources);

                _logService.Info(
                    $"[ResourceService] 预加载组 '{groupName}' 新增 {newResources.Count} 个资源（总计: {_preloadGroups[groupName].Count}）");
            }
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
            if (string.IsNullOrEmpty(groupName))
            {
                _logService.Error("[ResourceService] 预加载组名称为空，无法加载");
                return;
            }

            List<string> resourceList;
            lock (_lockObj)
            {
                if (!_preloadGroups.TryGetValue(groupName, out resourceList))
                {
                    _logService.Warning($"[ResourceService] 预加载组 '{groupName}' 不存在");
                    return;
                }

                resourceList = new List<string>(resourceList);
            }

            if (resourceList.Count == 0)
            {
                _logService.Warning($"[ResourceService] 预加载组 '{groupName}' 无资源可加载");
                return;
            }

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_globalCts.Token, cancellationToken);
            var totalCount = resourceList.Count;
            var completedCount = 0f;

            try
            {
                _logService.Info($"[ResourceService] 开始预加载组 '{groupName}'（{totalCount} 个资源）");
                const int batchSize = 5;

                for (var i = 0; i < totalCount; i += batchSize)
                {
                    linkedCts.Token.ThrowIfCancellationRequested();
                    var batch = resourceList.Skip(i).Take(batchSize).ToList();
                    var tasks = batch.Select(name => LoadPersistentAssetAsync<Object>(name, linkedCts.Token)).ToList();
                    await Task.WhenAll(tasks);
                    completedCount += batch.Count;
                }

                _logService.Info($"[ResourceService] 预加载组 '{groupName}' 完成，共 {completedCount} 个资源");
            }
            catch (OperationCanceledException)
            {
                _logService.Warning($"[ResourceService] 预加载组 '{groupName}' 被取消");
            }
            catch (Exception)
            {
                _logService.Error($"[ResourceService] 预加载组 '{groupName}' 异常");
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
        public async Task<ResourceLoadResult<T>> LoadPersistentAssetAsync<T>(string resourceName,
            CancellationToken cancellationToken = default) where T : Object
        {
            if (string.IsNullOrEmpty(resourceName))
            {
                const string msg = "[ResourceService] 资源名称为空，无法加载";
                _logService.Error(msg);
                return new ResourceLoadResult<T> { Status = ResourceLoadStatus.Failed, ErrorMessage = msg };
            }

            lock (_lockObj)
            {
                if (_persistentCache.TryGet(resourceName, out var handle))
                {
                    if (handle.IsValid() && handle.Result is T asset)
                    {
                        _logService.Info($"[ResourceService] 缓存命中: {resourceName}");
                        return new ResourceLoadResult<T> { Asset = asset, Status = ResourceLoadStatus.Success };
                    }

                    _persistentCache.Remove(resourceName);
                }
            }

            try
            {
                var handle = await _addressableService.LoadAssetAsync<T>(resourceName, cancellationToken);

                if (!handle.IsValid() || !handle.Result)
                {
                    var msg = $"[ResourceService] 资源加载失败: {resourceName}";
                    _logService.Error(msg);
                    return new ResourceLoadResult<T> { Status = ResourceLoadStatus.Failed, ErrorMessage = msg };
                }

                _persistentCache.Add(resourceName, handle);

                _logService.Info($"[ResourceService] 资源加载成功: {resourceName}");
                return new ResourceLoadResult<T> { Asset = handle.Result, Status = ResourceLoadStatus.Success };
            }
            catch (OperationCanceledException)
            {
                _logService.Warning($"[ResourceService] 加载取消: {resourceName}");
                return new ResourceLoadResult<T> { Status = ResourceLoadStatus.Cancelled };
            }
            catch (Exception ex)
            {
                _logService.Error($"[ResourceService] 加载异常: {resourceName}");
                return new ResourceLoadResult<T>
                    { Status = ResourceLoadStatus.Failed, ErrorMessage = ex.Message, Exception = ex };
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
                _logService.Error("[ResourceService] 资源名称为空");
                return;
            }

            lock (_lockObj)
            {
                if (_persistentCache.TryGet(resourceName, out var handle))
                {
                    _addressableService.ReleaseAsset(handle);
                    _persistentCache.Remove(resourceName);
                    _logService.Info($"[ResourceService] 资源已释放: {resourceName}");
                }
                else
                {
                    _logService.Warning($"[ResourceService] 资源未缓存: {resourceName}");
                }
            }
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
                return _persistentCache.TryGet(resourceName, out var handle) && handle.IsValid();
            }
        }

        #endregion

        #region LRU 自动释放

        private void OnPersistentAssetEvicted(string key, AsyncOperationHandle handle)
        {
            try
            {
                _addressableService.ReleaseAsset(handle);
                _logService.Info($"[ResourceService] LRU 自动释放: {key}");
            }
            catch (Exception)
            {
                _logService.Error($"[ResourceService] LRU 释放失败: {key}");
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
            if (string.IsNullOrEmpty(configName))
            {
                var msg = "[ResourceService] 配置名称为空";
                _logService.Error(msg);
                return new ResourceLoadResult<T> { Status = ResourceLoadStatus.Failed, ErrorMessage = msg };
            }

            if (useCache)
            {
                lock (_lockObj)
                {
                    if (_configCache.TryGet(configName, out var data) && data is T config)
                    {
                        _logService.Info($"[ResourceService] 配置缓存命中: {configName}");
                        return new ResourceLoadResult<T> { Asset = config, Status = ResourceLoadStatus.AlreadyLoaded };
                    }
                }
            }

            try
            {
                var textResult = await LoadPersistentAssetAsync<TextAsset>(configName, _globalCts.Token);
                if (textResult.Status != ResourceLoadStatus.Success || textResult.Asset == null)
                {
                    var msg = $"[ResourceService] 配置加载失败: {configName}";
                    _logService.Error(msg);
                    return new ResourceLoadResult<T> { Status = ResourceLoadStatus.Failed, ErrorMessage = msg };
                }

                var cfg = JsonConvert.DeserializeObject<T>(textResult.Asset.text, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

                if (cfg == null)
                {
                    var msg = $"[ResourceService] 配置反序列化失败: {configName}";
                    _logService.Error(msg);
                    return new ResourceLoadResult<T> { Status = ResourceLoadStatus.Failed, ErrorMessage = msg };
                }

                if (useCache)
                {
                    lock (_lockObj)
                    {
                        _configCache.Add(configName, cfg);
                    }
                }

                _logService.Info($"[ResourceService] 配置加载成功: {configName}");
                return new ResourceLoadResult<T> { Asset = cfg, Status = ResourceLoadStatus.Success };
            }
            catch (Exception ex)
            {
                _logService.Error($"[ResourceService] 配置加载异常: {configName}");
                return new ResourceLoadResult<T>
                    { Status = ResourceLoadStatus.Failed, ErrorMessage = ex.Message, Exception = ex };
            }
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
                    _logService.Info("[ResourceService] 已清空所有配置缓存");
                }
                else
                {
                    if (_configCache.Remove(configName))
                        _logService.Info($"[ResourceService] 配置缓存已清理: {configName}");
                    else
                        _logService.Warning($"[ResourceService] 配置不存在: {configName}");
                }
            }
        }

        #endregion


        #region 预制体实例化

        /// <summary>
        /// 异步实例化预制体
        /// </summary>
        /// <param name="prefabName">AssetKeys.AddressableNames 常量定义的预制体地址</param>
        /// <param name="parent">实例化的父节点（可选）</param>
        /// <param name="worldSpace">是否使用世界空间坐标（默认 false）</param>
        /// <returns>预制体实例化结果对象 <see cref="ResourceLoadResult{GameObject}"/></returns>
        /// <exception cref="ArgumentException">预制体名称为空时返回失败结果</exception>
        public async Task<ResourceLoadResult<GameObject>> InstantiatePrefabAsync(string prefabName,
            Transform parent = null, bool worldSpace = false)
        {
            if (string.IsNullOrEmpty(prefabName))
            {
                const string msg = "[ResourceService] 预制体名称为空";
                _logService.Error(msg);
                return new ResourceLoadResult<GameObject> { Status = ResourceLoadStatus.Failed, ErrorMessage = msg };
            }

            try
            {
                var handle =
                    await _addressableService.InstantiateAsync(prefabName, parent, worldSpace, _globalCts.Token);

                if (!handle.IsValid() || !handle.Result)
                {
                    var msg = $"[ResourceService] 预制体实例化失败: {prefabName}";
                    _logService.Error(msg);
                    return new ResourceLoadResult<GameObject>
                        { Status = ResourceLoadStatus.Failed, ErrorMessage = msg };
                }

                _logService.Info($"[ResourceService] 预制体实例化成功: {prefabName}");
                return new ResourceLoadResult<GameObject>
                    { Asset = handle.Result, Status = ResourceLoadStatus.Success };
            }
            catch (Exception ex)
            {
                _logService.Error($"[ResourceService] 实例化异常: {prefabName}");
                return new ResourceLoadResult<GameObject>
                    { Status = ResourceLoadStatus.Failed, ErrorMessage = ex.Message, Exception = ex };
            }
        }

        #endregion

        #region 辅助方法

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