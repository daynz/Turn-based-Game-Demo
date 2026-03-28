using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Zenject;
using Object = UnityEngine.Object;

namespace BH.Framework.Infrastructure.Resource.Interfaces
{
    /// <summary>
    /// Addressables 资源管理核心服务接口
    /// </summary>
    public interface IAddressableService : IInitializable, IDisposable
    {
        #region 资源加载 & 释放

        /// <summary>
        /// 异步加载单个资源，返回原生操作句柄（用于上层缓存与释放）
        /// </summary>
        /// <typeparam name="T">资源类型，需继承 UnityEngine.Object</typeparam>
        /// <param name="addressableName">Addressable 地址</param>
        /// <param name="cancellationToken">异步取消令牌</param>
        /// <returns>资源操作句柄</returns>
        Task<AsyncOperationHandle<T>> LoadAssetAsync<T>(string addressableName, CancellationToken cancellationToken = default) where T : Object;

        /// <summary>
        /// 主动释放资源句柄，递减引用计数
        /// </summary>
        /// <param name="handle">资源加载句柄</param>
        void ReleaseAsset(AsyncOperationHandle handle);

        #endregion

        #region 预制体实例化

        /// <summary>
        /// 异步实例化预制体，返回实例句柄
        /// </summary>
        /// <param name="address">预制体地址</param>
        /// <param name="parent">父物体</param>
        /// <param name="worldSpace">是否使用世界坐标</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>实例操作句柄</returns>
        Task<AsyncOperationHandle<GameObject>> InstantiateAsync(string address, Transform parent = null, bool worldSpace = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// 销毁实例并释放句柄
        /// </summary>
        /// <param name="handle">实例句柄</param>
        void ReleaseInstance(AsyncOperationHandle<GameObject> handle);

        #endregion

        #region 批量标签加载

        /// <summary>
        /// 根据单个标签批量加载资源
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="label">资源标签</param>
        /// <param name="mergeMode">合并模式</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>批量操作句柄</returns>
        Task<AsyncOperationHandle<IList<T>>> LoadAssetsByLabelAsync<T>(string label, Addressables.MergeMode mergeMode = Addressables.MergeMode.Union, CancellationToken cancellationToken = default) where T : Object;

        /// <summary>
        /// 根据多标签组合批量加载资源
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="labels">标签列表</param>
        /// <param name="mergeMode">合并模式（交集/并集）</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>批量操作句柄</returns>
        Task<AsyncOperationHandle<IList<T>>> LoadAssetsByLabelsAsync<T>(List<string> labels, Addressables.MergeMode mergeMode = Addressables.MergeMode.Intersection, CancellationToken cancellationToken = default) where T : Object;

        #endregion

        #region 场景管理

        /// <summary>
        /// 异步加载场景（不自动激活）
        /// </summary>
        /// <param name="sceneName">场景地址</param>
        /// <param name="loadMode">加载模式</param>
        /// <param name="activateOnLoad">是否加载后自动激活</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>场景操作句柄</returns>
        Task<AsyncOperationHandle<SceneInstance>> LoadSceneAsync(string sceneName, LoadSceneMode loadMode = LoadSceneMode.Additive, bool activateOnLoad = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// 异步加载并激活场景
        /// </summary>
        /// <param name="sceneName">场景地址</param>
        /// <param name="loadMode">加载模式</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>场景操作句柄</returns>
        Task<AsyncOperationHandle<SceneInstance>> LoadAndActivateSceneAsync(string sceneName, LoadSceneMode loadMode = LoadSceneMode.Additive, CancellationToken cancellationToken = default);

        /// <summary>
        /// 异步卸载场景
        /// </summary>
        /// <param name="sceneHandle">场景操作句柄</param>
        /// <returns>是否卸载成功</returns>
        Task<bool> UnloadSceneAsync(AsyncOperationHandle<SceneInstance> sceneHandle);

        #endregion
    }
}