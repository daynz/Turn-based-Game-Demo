using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BH.Framework.Infrastructure.Logging.Interfaces;
using BH.Framework.Infrastructure.Resource.Interfaces;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Zenject;
using Object = UnityEngine.Object;

namespace BH.Framework.Infrastructure.Resource.Services
{
    /// <summary>
    /// Addressables 资源管理服务封装类
    /// 提供线程安全、引用计数、取消支持、错误处理的 Addressables 统一操作接口
    /// </summary>
    [UsedImplicitly]
    public class AddressableService : IAddressableService
    {
        #region 依赖注入 & 核心字段

        /// <summary>
        /// 日志服务依赖
        /// </summary>
        [Inject] private readonly ILogService _logService;

        /// <summary>
        /// 通用线程锁对象，保证多线程下字典操作安全
        /// </summary>
        private readonly object _lockObj = new();

        /// <summary>
        /// 普通资源句柄缓存字典
        /// Key: 资源地址 | Value: (加载句柄, 引用计数)
        /// </summary>
        private readonly Dictionary<string, (AsyncOperationHandle handle, int refCount)> _loadedHandles = new();

        /// <summary>
        /// 场景资源句柄缓存字典
        /// Key: 场景地址 | Value: (场景加载句柄, 引用计数)
        /// </summary>
        private readonly Dictionary<string, (AsyncOperationHandle<SceneInstance> handle, int refCount)> _sceneHandles =
            new();

        /// <summary>
        /// 全局取消令牌源，用于取消所有未完成的异步操作
        /// </summary>
        private CancellationTokenSource _globalCts = new();

        #endregion

        #region 初始化 & 销毁

        /// <summary>
        /// 初始化 Addressables 服务
        /// </summary>
        public void Initialize()
        {
            _logService?.Info("[AddressableService] 初始化完成");
        }

        /// <summary>
        /// 销毁服务并释放所有资源
        /// </summary>
        public async void Dispose()
        {
            try
            {
                // 取消并释放全局取消令牌
                if (_globalCts is { IsCancellationRequested: false })
                {
                    _globalCts.Cancel();
                }

                _globalCts?.Dispose();
                _globalCts = null;

                // 释放所有普通资源
                ReleaseAllAssets(true);

                // 异步释放所有场景资源
                if (_sceneHandles.Count > 0)
                {
                    await ReleaseAllScenes(true);
                }

                // 清空缓存字典
                lock (_lockObj)
                {
                    _loadedHandles.Clear();
                    _sceneHandles.Clear();
                }

                _logService.Info("[AddressableService] 已释放所有资源并完成清理");
            }
            catch (Exception ex)
            {
                _logService.Error($"[AddressableService] 销毁过程中发生异常: {ex.Message}\n{ex.StackTrace}");
            }
        }

        #endregion

        #region 核心资源加载

        /// <summary>
        /// 异步加载指定地址的资源
        /// </summary>
        /// <typeparam name="T">资源类型（UnityEngine.Object 子类）</typeparam>
        /// <param name="address">资源的 Addressables 地址</param>
        /// <param name="cancellationToken">取消令牌（可选）</param>
        /// <returns>加载成功返回资源实例，失败返回 null</returns>
        public async Task<T> LoadAssetAsync<T>(string address, CancellationToken cancellationToken = default)
            where T : Object
        {
            // 入参合法性校验
            if (string.IsNullOrEmpty(address))
            {
                _logService.Error($"[AddressableService] 资源加载失败: 地址为空 [Type: {typeof(T).Name}]");
                return null;
            }

            // 合并全局和局部取消令牌
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                _globalCts.Token,
                cancellationToken);

            try
            {
                lock (_lockObj)
                {
                    // 检查缓存，存在则更新引用计数
                    if (_loadedHandles.TryGetValue(address, out var cacheItem))
                    {
                        var newRefCount = cacheItem.refCount + 1;
                        _loadedHandles[address] = (cacheItem.handle, newRefCount);
                        _logService.Info(
                            $"[AddressableService] 资源已缓存，引用计数+1: {address} (当前计数: {newRefCount}) [Type: {typeof(T).Name}]");

                        // 类型校验并返回结果
                        if (cacheItem.handle.Result is T result)
                        {
                            return result;
                        }

                        _logService.Error(
                            $"[AddressableService] 缓存资源类型不匹配: {address} 期望[{typeof(T).Name}] 实际[{cacheItem.handle.Result?.GetType().Name ?? "Null"}]");
                        return null;
                    }
                }

                // 异步加载资源
                _logService.Info($"[AddressableService] 开始加载资源: {address} [Type: {typeof(T).Name}]");
                var loadHandle = Addressables.LoadAssetAsync<T>(address);

                // 等待加载完成或取消
                var completedTask = await Task.WhenAny(
                    loadHandle.Task,
                    Task.Delay(Timeout.InfiniteTimeSpan, linkedCts.Token));

                // 检查是否为加载完成且未取消
                if (completedTask == loadHandle.Task && !linkedCts.Token.IsCancellationRequested)
                {
                    // 加载成功处理
                    if (loadHandle.Status == AsyncOperationStatus.Succeeded && loadHandle.Result)
                    {
                        lock (_lockObj)
                        {
                            _loadedHandles[address] = (loadHandle, 1);
                        }

                        _logService.Info($"[AddressableService] 资源加载成功: {address} [Type: {typeof(T).Name}]");
                        return loadHandle.Result;
                    }

                    // 加载失败处理
                    _logService.Error(
                        $"[AddressableService] 资源加载失败: {address} 状态[{loadHandle.Status}] 错误[{loadHandle.OperationException?.Message}] [Type: {typeof(T).Name}]");
                    loadHandle.Release();
                    return null;
                }

                // 取消操作处理
                _logService.Warning($"[AddressableService] 资源加载被取消: {address} [Type: {typeof(T).Name}]");
                loadHandle.Release();
                linkedCts.Token.ThrowIfCancellationRequested();
                return null;
            }
            catch (OperationCanceledException)
            {
                _logService.Warning($"[AddressableService] 资源加载取消: {address} [Type: {typeof(T).Name}]");
                return null;
            }
            catch (Exception ex)
            {
                _logService.Error(
                    $"[AddressableService] 资源加载异常: {address} [Type: {typeof(T).Name}]\n{ex.Message}\n{ex.StackTrace}");
                return null;
            }
            finally
            {
                linkedCts.Dispose();
            }
        }

        /// <summary>
        /// 异步加载并实例化预制体
        /// </summary>
        /// <param name="address">预制体的 Addressables 地址</param>
        /// <param name="parent">实例化的父节点（可选）</param>
        /// <param name="instantiateInWorldSpace">是否使用世界空间坐标</param>
        /// <param name="cancellationToken">取消令牌（可选）</param>
        /// <returns>实例化后的游戏对象，失败返回 null</returns>
        public async Task<GameObject> InstantiatePrefabAsync(string address, Transform parent = null,
            bool instantiateInWorldSpace = false, CancellationToken cancellationToken = default)
        {
            var prefab = await LoadAssetAsync<GameObject>(address, cancellationToken);
            if (prefab == null)
            {
                _logService.Error($"[AddressableService] 预制体实例化失败: 加载失败 {address}");
                return null;
            }

            var instance = Object.Instantiate(prefab, parent, instantiateInWorldSpace);
            _logService.Info($"[AddressableService] 预制体实例化成功: {address}");
            return instance;
        }

        #endregion

        #region 批量标签加载

        /// <summary>
        /// 按单个标签异步批量加载资源
        /// </summary>
        /// <typeparam name="T">资源类型（UnityEngine.Object 子类）</typeparam>
        /// <param name="label">资源标签</param>
        /// <param name="cancellationToken">取消令牌（可选）</param>
        /// <returns>加载成功的资源列表</returns>
        public async Task<List<T>> LoadAssetsByLabelAsync<T>(string label,
            CancellationToken cancellationToken = default) where T : Object
        {
            // 入参校验
            if (string.IsNullOrEmpty(label))
            {
                _logService.Error($"[AddressableService] 按标签加载失败：标签为空 [Type: {typeof(T).Name}]");
                return new List<T>();
            }

            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_globalCts.Token, cancellationToken);
            var loadedAssets = new List<T>();

            try
            {
                _logService.Info($"[AddressableService] 开始加载标签「{label}」下的{typeof(T).Name}类型资源");

                // 使用原生批量加载接口提升效率
                var loadHandle = Addressables.LoadAssetsAsync<T>(
                    label,
                    asset =>
                    {
                        if (asset != null)
                        {
                            loadedAssets.Add(asset);
                        }
                    },
                    Addressables.MergeMode.Union);

                // 等待加载完成或取消
                var completedTask = await Task.WhenAny(
                    loadHandle.Task,
                    Task.Delay(Timeout.InfiniteTimeSpan, linkedCts.Token));

                if (completedTask == loadHandle.Task && !linkedCts.Token.IsCancellationRequested)
                {
                    if (loadHandle.Status == AsyncOperationStatus.Succeeded)
                    {
                        _logService.Info(
                            $"[AddressableService] 标签「{label}」加载完成，共{loadedAssets.Count}个{typeof(T).Name}类型资源");
                    }
                    else
                    {
                        _logService.Error(
                            $"[AddressableService] 标签「{label}」加载失败: {loadHandle.OperationException?.Message}");
                    }
                }
                else
                {
                    _logService.Warning($"[AddressableService] 标签「{label}」加载被取消 [Type: {typeof(T).Name}]");
                }

                // 释放Location句柄并返回结果
                Addressables.Release(loadHandle);
                return loadedAssets;
            }
            catch (OperationCanceledException)
            {
                _logService.Warning($"[AddressableService] 标签「{label}」加载被取消 [Type: {typeof(T).Name}]");
                return loadedAssets;
            }
            catch (Exception ex)
            {
                _logService.Error(
                    $"[AddressableService] 标签「{label}」加载异常 [Type: {typeof(T).Name}]\n{ex.Message}\n{ex.StackTrace}");
                return loadedAssets;
            }
            finally
            {
                linkedCts.Dispose();
            }
        }

        /// <summary>
        /// 按多个标签异步批量加载资源
        /// </summary>
        /// <typeparam name="T">资源类型（UnityEngine.Object 子类）</typeparam>
        /// <param name="labels">标签列表</param>
        /// <param name="mergeMode">标签合并模式（交集/并集）</param>
        /// <param name="cancellationToken">取消令牌（可选）</param>
        /// <returns>加载成功的资源列表</returns>
        public async Task<List<T>> LoadAssetsByLabelsAsync<T>(List<string> labels,
            Addressables.MergeMode mergeMode = Addressables.MergeMode.Intersection,
            CancellationToken cancellationToken = default) where T : Object
        {
            // 入参校验
            if (labels == null || labels.Count == 0)
            {
                _logService.Error($"[AddressableService] 多标签加载失败：标签列表为空 [Type: {typeof(T).Name}]");
                return new List<T>();
            }

            var labelStr = string.Join(" + ", labels);
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_globalCts.Token, cancellationToken);
            var loadedAssets = new List<T>();

            try
            {
                _logService.Info($"[AddressableService] 开始加载多标签「{labelStr}」下的{typeof(T).Name}类型资源（合并模式：{mergeMode}）");

                // 使用原生批量加载接口
                var loadHandle = Addressables.LoadAssetsAsync<T>(
                    labels,
                    asset =>
                    {
                        if (asset)
                        {
                            loadedAssets.Add(asset);
                        }
                    },
                    mergeMode);

                // 等待加载完成或取消
                var completedTask = await Task.WhenAny(
                    loadHandle.Task,
                    Task.Delay(Timeout.InfiniteTimeSpan, linkedCts.Token));

                if (completedTask == loadHandle.Task && !linkedCts.Token.IsCancellationRequested)
                {
                    if (loadHandle.Status == AsyncOperationStatus.Succeeded)
                    {
                        _logService.Info(
                            $"[AddressableService] 多标签「{labelStr}」加载完成，共{loadedAssets.Count}个{typeof(T).Name}类型资源");
                    }
                    else
                    {
                        _logService.Error(
                            $"[AddressableService] 多标签「{labelStr}」加载失败: {loadHandle.OperationException?.Message}");
                    }
                }
                else
                {
                    _logService.Warning($"[AddressableService] 多标签「{labelStr}」加载被取消 [Type: {typeof(T).Name}]");
                }

                // 释放句柄并返回结果
                Addressables.Release(loadHandle);
                return loadedAssets;
            }
            catch (OperationCanceledException)
            {
                _logService.Warning($"[AddressableService] 多标签「{labelStr}」加载被取消 [Type: {typeof(T).Name}]");
                return loadedAssets;
            }
            catch (Exception ex)
            {
                _logService.Error(
                    $"[AddressableService] 多标签「{labelStr}」加载异常 [Type: {typeof(T).Name}]\n{ex.Message}\n{ex.StackTrace}");
                return loadedAssets;
            }
            finally
            {
                linkedCts.Dispose();
            }
        }

        #endregion

        #region 资源释放

        /// <summary>
        /// 释放指定地址的资源（引用计数减1，计数为0时实际释放）
        /// </summary>
        /// <param name="address">资源地址</param>
        /// <param name="forceRelease">是否强制释放（忽略引用计数）</param>
        public void ReleaseAsset(string address, bool forceRelease = false)
        {
            if (string.IsNullOrEmpty(address))
            {
                _logService.Error("[AddressableService] 资源释放失败: 地址为空");
                return;
            }

            lock (_lockObj)
            {
                if (!_loadedHandles.TryGetValue(address, out var cacheItem))
                {
                    _logService.Warning($"[AddressableService] 尝试释放未加载的资源: {address}");
                    return;
                }

                // 计算新引用计数
                var newRefCount = forceRelease ? 0 : cacheItem.refCount - 1;

                // 计数清零或强制释放时执行实际释放
                if (newRefCount <= 0 || forceRelease)
                {
                    // 校验句柄有效性后释放
                    if (cacheItem.handle.IsValid())
                    {
                        Addressables.Release(cacheItem.handle);
                    }

                    _loadedHandles.Remove(address);
                    _logService.Info($"[AddressableService] 资源释放成功: {address} (引用计数清零)");
                }
                else
                {
                    // 仅更新引用计数
                    _loadedHandles[address] = (cacheItem.handle, newRefCount);
                    _logService.Info($"[AddressableService] 资源引用计数-1: {address} (当前计数: {newRefCount})");
                }
            }
        }

        /// <summary>
        /// 释放所有已加载的资源
        /// </summary>
        /// <param name="forceRelease">是否强制释放（忽略引用计数）</param>
        public void ReleaseAllAssets(bool forceRelease = false)
        {
            lock (_lockObj)
            {
                var addresses = new List<string>(_loadedHandles.Keys);
                foreach (var address in addresses)
                {
                    ReleaseAsset(address, forceRelease);
                }
            }

            _logService.Info($"[AddressableService] 已处理所有资源释放（强制模式: {forceRelease}）");
        }

        /// <summary>
        /// 获取指定资源的引用计数
        /// </summary>
        /// <param name="address">资源地址</param>
        /// <returns>引用计数值，未加载返回0</returns>
        public int GetAssetRefCount(string address)
        {
            lock (_lockObj)
            {
                return _loadedHandles.TryGetValue(address, out var item) ? item.refCount : 0;
            }
        }

        #endregion

        #region 场景管理

        /// <summary>
        /// 异步加载指定地址的场景（不自动激活）
        /// </summary>
        /// <param name="sceneAddress">场景的 Addressables 地址</param>
        /// <param name="loadMode">场景加载模式（默认叠加加载）</param>
        /// <param name="cancellationToken">取消令牌（可选）</param>
        /// <returns>场景实例，加载失败返回默认值</returns>
        public async Task<SceneInstance> LoadSceneAsync(string sceneAddress,
            LoadSceneMode loadMode = LoadSceneMode.Additive,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(sceneAddress))
            {
                _logService.Error($"[AddressableService] 场景加载失败: 地址为空");
                return default;
            }

            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_globalCts.Token, cancellationToken);

            try
            {
                lock (_lockObj)
                {
                    // 检查缓存并更新引用计数
                    if (_sceneHandles.TryGetValue(sceneAddress, out var cacheItem))
                    {
                        var newRefCount = cacheItem.refCount + 1;
                        _sceneHandles[sceneAddress] = (cacheItem.handle, newRefCount);
                        _logService.Info($"[AddressableService] 场景已缓存，引用计数+1: {sceneAddress} (当前计数: {newRefCount})");
                        return cacheItem.handle.Result;
                    }
                }

                // 异步加载场景
                _logService.Info($"[AddressableService] 开始加载场景: {sceneAddress} (模式: {loadMode})");
                var loadHandle = Addressables.LoadSceneAsync(sceneAddress, loadMode, activateOnLoad: false);

                // 等待加载完成或取消
                var completedTask = await Task.WhenAny(
                    loadHandle.Task,
                    Task.Delay(Timeout.InfiniteTimeSpan, linkedCts.Token));

                if (completedTask == loadHandle.Task && !linkedCts.Token.IsCancellationRequested)
                {
                    // 加载成功校验
                    if (loadHandle.Status == AsyncOperationStatus.Succeeded &&
                        loadHandle.Result.Equals(null) &&
                        loadHandle.Result.Scene.IsValid())
                    {
                        lock (_lockObj)
                        {
                            _sceneHandles[sceneAddress] = (loadHandle, 1);
                        }

                        _logService.Info($"[AddressableService] 场景加载成功: {sceneAddress} (模式: {loadMode})");
                        return loadHandle.Result;
                    }

                    // 加载失败处理
                    _logService.Error(
                        $"[AddressableService] 场景加载失败: {sceneAddress} 状态[{loadHandle.Status}] 错误[{loadHandle.OperationException?.Message}]");
                    loadHandle.Release();
                    return default;
                }

                // 取消处理
                _logService.Warning($"[AddressableService] 场景加载被取消: {sceneAddress}");
                loadHandle.Release();
                linkedCts.Token.ThrowIfCancellationRequested();
                return default;
            }
            catch (OperationCanceledException)
            {
                _logService.Warning($"[AddressableService] 场景加载取消: {sceneAddress}");
                return default;
            }
            catch (Exception ex)
            {
                _logService.Error($"[AddressableService] 场景加载异常: {sceneAddress}\n{ex.Message}\n{ex.StackTrace}");
                return default;
            }
            finally
            {
                linkedCts.Dispose();
            }
        }

        /// <summary>
        /// 异步加载并激活指定场景
        /// </summary>
        /// <param name="sceneAddress">场景的 Addressables 地址</param>
        /// <param name="loadMode">场景加载模式（默认叠加加载）</param>
        /// <param name="cancellationToken">取消令牌（可选）</param>
        /// <returns>激活后的场景实例，失败返回默认值</returns>
        public async Task<SceneInstance> LoadAndActivateSceneAsync(string sceneAddress,
            LoadSceneMode loadMode = LoadSceneMode.Additive,
            CancellationToken cancellationToken = default)
        {
            var sceneInstance = await LoadSceneAsync(sceneAddress, loadMode, cancellationToken);

            // 校验场景有效性
            if (sceneInstance.Equals(null) || !sceneInstance.Scene.IsValid())
            {
                _logService.Error($"[AddressableService] 场景激活失败: 场景实例无效 {sceneAddress}");
                return default;
            }

            // 激活场景并处理异常
            try
            {
                await sceneInstance.ActivateAsync();
                _logService.Info($"[AddressableService] 场景已激活: {sceneAddress}");
                return sceneInstance;
            }
            catch (Exception ex)
            {
                _logService.Error($"[AddressableService] 场景激活异常: {sceneAddress}\n{ex.Message}\n{ex.StackTrace}");
                // 激活失败时清理缓存
                await UnloadSceneAsync(sceneAddress, true);
                return default;
            }
        }

        /// <summary>
        /// 异步卸载指定地址的场景
        /// </summary>
        /// <param name="sceneAddress">场景地址</param>
        /// <param name="forceRelease">是否强制卸载（忽略引用计数）</param>
        /// <returns>卸载成功返回true，失败返回false</returns>
        public async Task<bool> UnloadSceneAsync(string sceneAddress, bool forceRelease = false)
        {
            // 入参校验
            if (string.IsNullOrEmpty(sceneAddress))
            {
                _logService.Error($"[AddressableService] 【场景卸载】失败：地址为空");
                return false;
            }

            AsyncOperationHandle<SceneInstance> targetHandle;
            bool needActualUnload;

            // 锁内处理引用计数和句柄提取
            lock (_lockObj)
            {
                if (!_sceneHandles.TryGetValue(sceneAddress, out var cacheItem))
                {
                    _logService.Warning($"[AddressableService] 【场景卸载】警告：尝试卸载未加载的场景 -> {sceneAddress}");
                    return false;
                }

                // 计算新引用计数
                int newRefCount = forceRelease ? 0 : cacheItem.refCount - 1;

                // 引用计数仍大于0，仅更新计数
                if (newRefCount > 0)
                {
                    _sceneHandles[sceneAddress] = (cacheItem.handle, newRefCount);
                    _logService.Info($"[AddressableService] 【场景卸载】引用计数减1 -> {sceneAddress} (当前计数: {newRefCount})");
                    return true;
                }

                // 标记需要实际卸载并提取句柄
                targetHandle = cacheItem.handle;
                needActualUnload = true;
            }

            // 执行实际卸载逻辑
            if (needActualUnload)
            {
                try
                {
                    // 句柄有效性校验
                    if (targetHandle.IsValid())
                    {
                        // 异步卸载场景
                        AsyncOperationHandle unloadHandle = Addressables.UnloadSceneAsync(targetHandle);
                        await unloadHandle.Task;

                        // 加锁清理缓存
                        lock (_lockObj)
                        {
                            if (_sceneHandles.ContainsKey(sceneAddress))
                            {
                                targetHandle.Release();
                                _sceneHandles.Remove(sceneAddress);
                            }
                        }

                        _logService.Info($"[AddressableService] 【场景卸载】成功 -> {sceneAddress}");
                        return true;
                    }
                    else
                    {
                        // 句柄无效时清理缓存
                        lock (_lockObj)
                        {
                            _sceneHandles.Remove(sceneAddress);
                        }

                        _logService.Warning($"[AddressableService] 【场景卸载】警告：句柄无效，已清理缓存 -> {sceneAddress}");
                    }
                }
                catch (Exception ex)
                {
                    // 异常时清理缓存
                    lock (_lockObj)
                    {
                        _sceneHandles.Remove(sceneAddress);
                    }

                    _logService.Error(
                        $"[AddressableService] 【场景卸载】异常 -> {sceneAddress}\n异常信息：{ex.Message}\n堆栈：{ex.StackTrace}");
                }
            }

            return false;
        }

        /// <summary>
        /// 异步释放所有已加载的场景
        /// </summary>
        /// <param name="forceRelease">是否强制释放（忽略引用计数）</param>
        /// <returns>异步任务</returns>
        public async Task ReleaseAllScenes(bool forceRelease = false)
        {
            List<string> addresses;
            lock (_lockObj)
            {
                addresses = new List<string>(_sceneHandles.Keys);
                if (addresses.Count == 0)
                {
                    _logService.Info("[AddressableService] 没有已加载的场景需要释放");
                    return;
                }

                _logService.Info($"[AddressableService] 开始释放所有场景，共{addresses.Count}个（强制模式: {forceRelease}）");
            }

            // 批量异步卸载场景
            var unloadTasks = addresses.Select(address => UnloadSceneAsync(address, forceRelease)).ToList();
            var results = await Task.WhenAll(unloadTasks);

            // 统计卸载结果
            var successCount = results.Count(r => r);
            var failCount = addresses.Count - successCount;

            _logService.Info(
                $"[AddressableService] 所有场景释放触发完成 - 成功: {successCount} 失败: {failCount}（强制模式: {forceRelease}）");
        }

        /// <summary>
        /// 检查指定场景是否已加载并有效
        /// </summary>
        /// <param name="sceneAddress">场景地址</param>
        /// <returns>已加载且有效返回true，否则返回false</returns>
        public bool IsSceneLoaded(string sceneAddress)
        {
            if (string.IsNullOrEmpty(sceneAddress)) return false;

            lock (_lockObj)
            {
                if (_sceneHandles.TryGetValue(sceneAddress, out var handleItem))
                {
                    return handleItem.handle.IsValid()
                           && handleItem.handle.Result.Equals(null)
                           && handleItem.handle.Result.Scene.IsValid()
                           && handleItem.handle.Result.Scene.isLoaded;
                }
            }

            return false;
        }

        #endregion

        #region 状态查询 & 辅助方法

        /// <summary>
        /// 获取已加载资源和场景的数量
        /// </summary>
        /// <returns>元组(已加载资源数, 已加载场景数)</returns>
        public (int assetCount, int sceneCount) GetLoadedCount()
        {
            lock (_lockObj)
            {
                return (_loadedHandles.Count, _sceneHandles.Count);
            }
        }

        /// <summary>
        /// 检查指定地址的资源是否已加载
        /// </summary>
        /// <param name="address">资源地址</param>
        /// <returns>已加载返回true，否则返回false</returns>
        public bool IsAssetLoaded(string address)
        {
            lock (_lockObj)
            {
                return !string.IsNullOrEmpty(address) && _loadedHandles.ContainsKey(address);
            }
        }

        /// <summary>
        /// 清理所有无效的资源/场景句柄
        /// </summary>
        public void CleanInvalidHandles()
        {
            lock (_lockObj)
            {
                // 清理无效资源句柄
                var invalidAssets = new List<string>();
                foreach (var pair in _loadedHandles)
                {
                    if (!pair.Value.handle.IsValid() || pair.Value.handle.Status != AsyncOperationStatus.Succeeded)
                    {
                        invalidAssets.Add(pair.Key);
                    }
                }

                foreach (var address in invalidAssets)
                {
                    _loadedHandles.Remove(address);
                    _logService.Info($"[AddressableService] 清理无效资源句柄: {address}");
                }

                // 清理无效场景句柄
                var invalidScenes = new List<string>();
                foreach (var pair in _sceneHandles)
                {
                    if (!pair.Value.handle.IsValid() ||
                        pair.Value.handle.Result.Equals(null) ||
                        !pair.Value.handle.Result.Scene.IsValid())
                    {
                        invalidScenes.Add(pair.Key);
                    }
                }

                foreach (var address in invalidScenes)
                {
                    _sceneHandles.Remove(address);
                    _logService.Info($"[AddressableService] 清理无效场景句柄: {address}");
                }
            }
        }

        #endregion
    }
}