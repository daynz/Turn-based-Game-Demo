using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BH.Framework.Infrastructure.Events.Interfaces;
using BH.Framework.Infrastructure.Logging.Interfaces;
using BH.Framework.Infrastructure.Resource.Data;
using BH.Framework.Infrastructure.Resource.Interfaces;
using BH.Framework.Utilities.Collections;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Zenject;

namespace BH.Framework.Infrastructure.Resource.Services
{
    /// <summary>
    /// 场景管理服务
    /// </summary>
    [UsedImplicitly]
    public class SceneService : ISceneService
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

        #endregion

        #region 核心缓存 & 映射

        /// <summary>
        /// 线程安全锁对象，保护多线程下的缓存/映射操作
        /// </summary>
        private readonly object _lockObj = new();

        /// <summary>
        /// 已加载场景映射表
        /// </summary>
        /// <remarks>
        /// Key: 场景名称<br/>
        /// Value: Addressables 加载场景后返回的句柄实例
        /// </remarks>
        private readonly ExpiredLruCache<string, AsyncOperationHandle<SceneInstance>> _sceneCache = new(8);

        /// <summary>
        /// 全局取消令牌源，用于取消所有未完成的异步资源操作
        /// </summary>
        private CancellationTokenSource _globalCts = new();

        #endregion

        public void Initialize()
        {
            _sceneCache.OnItemEvicted += OnSceneCacheEvicted;
            _logService.Info("[SceneService] 初始化完成");
        }

        public void Dispose()
        {
            try
            {
                lock (_lockObj)
                {
                    _sceneCache.Clear();
                }

                _globalCts?.Cancel();
                _globalCts?.Dispose();
                _globalCts = null;

                _logService.Info("[SceneService] 服务已销毁，所有场景已释放");
            }
            catch (Exception)
            {
                _logService.Error("[SceneService] 销毁异常");
            }
        }

        #region 场景管理

        /// <summary>
        /// 异步加载场景（不自动激活）
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <param name="loadMode">场景加载模式（默认叠加加载）</param>
        /// <returns>场景加载结果对象 <see cref="ResourceLoadResult{SceneInstance}"/></returns>
        /// <exception cref="ArgumentException">逻辑名称/地址为空时返回失败结果</exception>
        public async Task<ResourceLoadResult<SceneInstance>> LoadSceneAsync(string sceneName,
            LoadSceneMode loadMode = LoadSceneMode.Additive)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                const string msg = "[SceneService] 场景地址为空";
                _logService.Error(msg);
                return new ResourceLoadResult<SceneInstance> { Status = ResourceLoadStatus.Failed, ErrorMessage = msg };
            }

            if (IsSceneLoaded(sceneName))
            {
                _logService.Warning($"[SceneService] 场景已加载: {sceneName}");
                return new ResourceLoadResult<SceneInstance> { Status = ResourceLoadStatus.AlreadyLoaded };
            }

            try
            {
                var handle = await _addressableService.LoadSceneAsync(sceneName, loadMode, false, _globalCts.Token);

                if (!handle.IsValid() || !handle.Result.Scene.IsValid())
                {
                    var msg = $"[SceneService] 场景加载无效: {sceneName}";
                    _logService.Error(msg);
                    return new ResourceLoadResult<SceneInstance>
                        { Status = ResourceLoadStatus.Failed, ErrorMessage = msg };
                }

                lock (_lockObj)
                {
                    _sceneCache.Add(sceneName, handle);
                }

                _logService.Info($"[SceneService] 场景加载成功: {sceneName}");
                return new ResourceLoadResult<SceneInstance>
                {
                    Asset = handle.Result,
                    Status = ResourceLoadStatus.Success
                };
            }
            catch (Exception ex)
            {
                _logService.Error($"[SceneService] 场景加载异常: {sceneName}", ex.StackTrace);
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
        /// <param name="sceneName">场景逻辑名称（业务层标识）</param>
        /// <param name="loadMode">场景加载模式（默认叠加加载）</param>
        /// <returns>场景加载结果对象 <see cref="ResourceLoadResult{SceneInstance}"/></returns>
        /// <remarks>
        /// 执行逻辑：<br/>
        /// 1. 调用 <see cref="LoadSceneAsync"/> 加载场景<br/>
        /// 2. 激活场景<br/>
        /// 3. 激活失败时自动卸载场景并返回失败结果
        /// </remarks>
        public async Task<ResourceLoadResult<SceneInstance>> LoadAndActivateSceneAsync(string sceneName,
            LoadSceneMode loadMode = LoadSceneMode.Additive)
        {
            var loadResult = await LoadSceneAsync(sceneName, loadMode);
            if (loadResult.Status != ResourceLoadStatus.Success) return loadResult;

            try
            {
                AsyncOperationHandle<SceneInstance> handle;
                lock (_lockObj)
                {
                    _sceneCache.TryGet(sceneName, out handle);
                }

                if (!handle.IsValid())
                {
                    return new ResourceLoadResult<SceneInstance>
                        { Status = ResourceLoadStatus.Failed, ErrorMessage = "无效场景句柄" };
                }

                await handle.Result.ActivateAsync();
                _logService.Info($"[SceneService] 场景激活成功: {sceneName}");
                return loadResult;
            }
            catch (Exception ex)
            {
                _logService.Error($"[SceneService] 场景激活异常: {sceneName}");
                await UnloadSceneAsync(sceneName);
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
        /// <param name="sceneName">场景逻辑名称（业务层标识）</param>
        /// <returns>卸载成功返回 true，否则返回 false</returns>
        /// <remarks>线程安全操作，卸载成功后移除场景映射记录</remarks>
        public async Task UnloadSceneAsync(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return;

            AsyncOperationHandle<SceneInstance> handle;
            lock (_lockObj)
            {
                if (!_sceneCache.TryGet(sceneName, out handle))
                {
                    _logService.Warning($"[SceneService] 场景未加载: {sceneName}");
                    return;
                }
            }

            bool success = await _addressableService.UnloadSceneAsync(handle);
            if (success)
            {
                lock (_lockObj)
                {
                    _sceneCache.Remove(sceneName);
                }

                _logService.Info($"[SceneService] 场景卸载成功: {sceneName}");
            }
            else
            {
                _logService.Error($"[SceneService] 场景卸载失败: {sceneName}");
            }
        }

        /// <summary>
        /// 检查场景是否已加载且有效
        /// </summary>
        /// <param name="sceneName">场景逻辑名称（业务层标识）</param>
        /// <returns>已加载且有效返回 true，否则返回 false</returns>
        public bool IsSceneLoaded(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return false;

            lock (_lockObj)
            {
                if (_sceneCache.TryGet(sceneName, out var handle))
                {
                    return handle.IsValid() && handle.Result.Scene.IsValid() && handle.Result.Scene.isLoaded;
                }
            }

            return false;
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 获取已加载场景的逻辑名称列表
        /// </summary>
        /// <returns>已加载场景逻辑名称列表</returns>
        public List<string> GetScenesCache()
        {
            lock (_lockObj)
            {
                return _sceneCache.GetAllCacheName();
            }
        }

        /// <summary>
        /// 清理所有无效的场景句柄
        /// </summary>
        public void CleanInvalidHandles()
        {
            lock (_lockObj)
            {
                var keys = _sceneCache.GetAllCacheName();
                foreach (var key in keys)
                {
                    if (!_sceneCache.TryGet(key, out var handle)) continue;
                    if (!handle.IsValid())
                    {
                        _sceneCache.Remove(key);
                    }
                }
            }

            _logService.Info("[SceneService] 无效句柄清理完成");
        }

        /// <summary>
        /// LRU 缓存自动淘汰时自动卸载场景
        /// </summary>
        private async void OnSceneCacheEvicted(string key, AsyncOperationHandle<SceneInstance> handle)
        {
            try
            {
                await _addressableService.UnloadSceneAsync(handle);
                _logService.Info($"[SceneService] LRU 自动卸载场景: {key}");
            }
            catch (Exception)
            {
                _logService.Error($"[SceneService] LRU 卸载异常: {key}");
            }
        }

        #endregion
    }
}