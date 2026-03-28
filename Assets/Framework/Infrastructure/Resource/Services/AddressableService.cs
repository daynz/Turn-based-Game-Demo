using System;
using System.Collections.Generic;
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
    /// </summary>
    [UsedImplicitly]
    public class AddressableService  : IAddressableService
    {
        #region 依赖注入 & 核心字段

        /// <summary>
        /// 日志服务依赖
        /// </summary>
        [Inject] private readonly ILogService _logService;

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
            _logService.Info("[AddressableService] 初始化完成");
        }

        /// <summary>
        /// 销毁服务并释放所有资源
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (_globalCts is { IsCancellationRequested: false })
                    _globalCts.Cancel();

                _globalCts?.Dispose();
                _globalCts = null;

                _logService.Info($"[AddressableService] 服务已销毁，资源已全部释放");
            }
            catch (Exception)
            {
                _logService.Error("[AddressableService] 销毁异常");
            }
        }

        #endregion

        #region 核心资源管理

        /// <summary>
        /// 异步加载指定地址的资源
        /// </summary>
        /// <typeparam name="T">资源类型（UnityEngine.Object 子类）</typeparam>
        /// <param name="addressableName">资源的 Addressables 名称</param>
        /// <param name="cancellationToken">取消令牌（可选）</param>
        /// <returns>加载成功返回原生操作句柄，失败返回 null</returns>
        public async Task<AsyncOperationHandle<T>> LoadAssetAsync<T>(string addressableName,
            CancellationToken cancellationToken = default)
            where T : Object
        {
            if (string.IsNullOrEmpty(addressableName))
            {
                _logService.Error($"[AddressableService] 加载地址为空");
                return default;
            }

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_globalCts.Token, cancellationToken);
            AsyncOperationHandle<T> handle = default;

            try
            {
                _logService.Info($"[AddressableService] 开始加载 → {addressableName}");
                handle = Addressables.LoadAssetAsync<T>(addressableName);

                await Task.WhenAny(handle.Task, Task.Delay(Timeout.Infinite, linkedCts.Token));

                if (linkedCts.IsCancellationRequested)
                {
                    _logService.Warning($"[AddressableService] 加载已取消 → {addressableName}");
                    Addressables.Release(handle);
                    return default;
                }

                if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result)
                {
                    _logService.Info($"[AddressableService] 加载成功 → {addressableName}");
                    return handle;
                }

                _logService.Error(
                    $"[AddressableService] 加载失败 → {addressableName} | 状态:{handle.Status} | 错误:{handle.OperationException?.Message}");

                Addressables.Release(handle);
                return default;
            }
            catch (OperationCanceledException)
            {
                _logService.Warning($"[AddressableService] 加载取消 → {addressableName}");
                if (handle.IsValid()) Addressables.Release(handle);
                return default;
            }
            catch (Exception)
            {
                _logService.Error($"[AddressableService] 加载异常 → {addressableName}");
                if (handle.IsValid()) Addressables.Release(handle);
                return default;
            }
        }

        /// <summary>
        /// 释放资源句柄
        /// </summary>
        /// <param name="handle">资源句柄</param>
        /// <typeparam name="T">资源类型</typeparam>
        public void ReleaseAsset(AsyncOperationHandle handle)
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
                _logService.Info($"[AddressableService] 资源已释放");
            }
        }

        /// <summary>
        /// 异步加载并实例化预制体
        /// </summary>
        /// <param name="address">预制体的 Addressables 地址</param>
        /// <param name="parent">实例化的父节点（可选）</param>
        /// <param name="worldSpace">是否为世界空间</param>
        /// <param name="cancellationToken">取消令牌（可选）</param>
        /// <returns>实例化后的游戏对象，失败返回 null</returns>
        public async Task<AsyncOperationHandle<GameObject>> InstantiateAsync(
            string address,
            Transform parent = null,
            bool worldSpace = false,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(address))
                return default;

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_globalCts.Token, cancellationToken);
            AsyncOperationHandle<GameObject> handle = default;

            try
            {
                handle = Addressables.InstantiateAsync(address, parent, worldSpace);
                await Task.WhenAny(handle.Task, Task.Delay(Timeout.Infinite, linkedCts.Token));

                if (linkedCts.IsCancellationRequested || handle.Status != AsyncOperationStatus.Succeeded)
                {
                    if (handle.IsValid()) Addressables.Release(handle);
                    return default;
                }

                return handle;
            }
            catch
            {
                if (handle.IsValid()) Addressables.Release(handle);
                return default;
            }
        }

        /// <summary>
        /// 释放预制体资源
        /// </summary>
        /// <param name="handle">资源句柄</param>
        public void ReleaseInstance(AsyncOperationHandle<GameObject> handle)
        {
            if (handle.IsValid())
                Addressables.Release(handle);
        }

        #endregion

        #region 批量标签加载

        /// <summary>
        /// 按单个标签异步批量加载资源
        /// </summary>
        /// <typeparam name="T">资源类型（UnityEngine.Object 子类）</typeparam>
        /// <param name="label">资源标签</param>
        /// <param name="mergeMode">合并模式</param>
        /// <param name="cancellationToken">取消令牌（可选）</param>
        /// <returns>加载成功的资源列表</returns>
        public async Task<AsyncOperationHandle<IList<T>>> LoadAssetsByLabelAsync<T>(
            string label,
            Addressables.MergeMode mergeMode = Addressables.MergeMode.Union,
            CancellationToken cancellationToken = default) where T : Object
        {
            if (string.IsNullOrEmpty(label)) return default;

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_globalCts.Token, cancellationToken);
            AsyncOperationHandle<IList<T>> handle = default;

            try
            {
                handle = Addressables.LoadAssetsAsync<T>(label, null, mergeMode);
                await Task.WhenAny(handle.Task, Task.Delay(Timeout.Infinite, linkedCts.Token));

                return linkedCts.IsCancellationRequested || handle.Status != AsyncOperationStatus.Succeeded
                    ? default
                    : handle;
            }
            catch
            {
                if (handle.IsValid()) Addressables.Release(handle);
                return default;
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
        public async Task<AsyncOperationHandle<IList<T>>> LoadAssetsByLabelsAsync<T>(
            List<string> labels,
            Addressables.MergeMode mergeMode = Addressables.MergeMode.Intersection,
            CancellationToken cancellationToken = default) where T : Object
        {
            if (labels == null || labels.Count == 0) return default;

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_globalCts.Token, cancellationToken);
            AsyncOperationHandle<IList<T>> handle = default;

            try
            {
                handle = Addressables.LoadAssetsAsync<T>(labels, null, mergeMode);
                await Task.WhenAny(handle.Task, Task.Delay(Timeout.Infinite, linkedCts.Token));

                return linkedCts.IsCancellationRequested || handle.Status != AsyncOperationStatus.Succeeded
                    ? default
                    : handle;
            }
            catch
            {
                if (handle.IsValid()) Addressables.Release(handle);
                return default;
            }
        }

        #endregion

        #region 场景管理

        /// <summary>
        /// 异步加载指定地址的场景（不自动激活）
        /// </summary>
        /// <param name="sceneName">场景的 Addressables 地址</param>
        /// <param name="loadMode">场景加载模式（默认叠加加载）</param>
        /// <param name="activateOnLoad">是否加载时激活</param>
        /// <param name="cancellationToken">取消令牌（可选）</param>
        /// <returns>场景实例，加载失败返回默认值</returns>
        public async Task<AsyncOperationHandle<SceneInstance>> LoadSceneAsync(
            string sceneName,
            LoadSceneMode loadMode = LoadSceneMode.Additive,
            bool activateOnLoad = false,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(sceneName)) return default;

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_globalCts.Token, cancellationToken);
            AsyncOperationHandle<SceneInstance> handle = default;

            try
            {
                handle = Addressables.LoadSceneAsync(sceneName, loadMode, activateOnLoad);
                await Task.WhenAny(handle.Task, Task.Delay(Timeout.Infinite, linkedCts.Token));

                if (linkedCts.IsCancellationRequested)
                {
                    if (handle.IsValid()) Addressables.Release(handle);
                    return default;
                }

                if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result.Scene.IsValid())
                {
                    _logService.Info($"[AddressableService] 场景加载完成 → {sceneName}");
                    return handle;
                }

                _logService.Error($"[AddressableService] 场景加载失败 → {sceneName}");
                if (handle.IsValid()) Addressables.Release(handle);
                return default;
            }
            catch
            {
                if (handle.IsValid()) Addressables.Release(handle);
                return default;
            }
        }

        /// <summary>
        /// 异步加载并激活指定场景
        /// </summary>
        /// <param name="sceneName">场景的 Addressables 地址</param>
        /// <param name="loadMode">场景加载模式（默认叠加加载）</param>
        /// <param name="cancellationToken">取消令牌（可选）</param>
        /// <returns>激活后的场景实例，失败返回默认值</returns>
        public async Task<AsyncOperationHandle<SceneInstance>> LoadAndActivateSceneAsync(
            string sceneName,
            LoadSceneMode loadMode = LoadSceneMode.Additive,
            CancellationToken cancellationToken = default)
        {
            var handle = await LoadSceneAsync(sceneName, loadMode, false, cancellationToken);

            if (!handle.IsValid() || !handle.Result.Scene.IsValid())
            {
                _logService.Error($"[AddressableService] 场景激活失败（无效句柄）→ {sceneName}");
                return default;
            }

            try
            {
                await handle.Result.ActivateAsync();
                _logService.Info($"[AddressableService] 场景激活成功 → {sceneName}");
                return handle;
            }
            catch (Exception)
            {
                _logService.Error($"[AddressableService] 场景激活异常 → {sceneName}");
                await UnloadSceneAsync(handle);
                return default;
            }
        }

        /// <summary>
        /// 异步卸载指定地址的场景
        /// </summary>
        /// <param name="sceneHandle">场景地址</param>
        /// <returns>卸载成功返回true，失败返回false</returns>
        public async Task<bool> UnloadSceneAsync(AsyncOperationHandle<SceneInstance> sceneHandle)
        {
            if (!sceneHandle.IsValid())
            {
                _logService.Warning("[AddressableService] 卸载无效场景句柄");
                return false;
            }

            try
            {
                var unloadHandle = Addressables.UnloadSceneAsync(sceneHandle);
                await unloadHandle.Task;
                _logService.Info("[AddressableService] 场景卸载完成");
                return true;
            }
            catch (Exception)
            {
                _logService.Error("[AddressableService] 场景卸载异常");
                return false;
            }
        }

        #endregion
    }
}