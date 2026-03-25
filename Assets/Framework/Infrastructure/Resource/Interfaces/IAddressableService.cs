using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Zenject;
using Object = UnityEngine.Object;

namespace BH.Framework.Infrastructure.Resource.Interfaces
{
    /// <summary>
    /// Addressables 资源管理服务接口
    /// 定义线程安全、支持引用计数、取消操作的 Addressables 统一操作契约
    /// </summary>
    public interface IAddressableService : IInitializable, IDisposable
    {
        #region 核心资源加载

        /// <summary>
        /// 异步加载指定地址的资源
        /// </summary>
        /// <typeparam name="T">资源类型（UnityEngine.Object 子类）</typeparam>
        /// <param name="address">资源的 Addressables 地址</param>
        /// <param name="cancellationToken">取消令牌（可选）</param>
        /// <returns>加载成功返回资源实例，失败返回 null</returns>
        Task<T> LoadAssetAsync<T>(string address, CancellationToken cancellationToken = default)
            where T : Object;

        /// <summary>
        /// 异步加载并实例化预制体
        /// </summary>
        /// <param name="address">预制体的 Addressables 地址</param>
        /// <param name="parent">实例化的父节点（可选）</param>
        /// <param name="instantiateInWorldSpace">是否使用世界空间坐标</param>
        /// <param name="cancellationToken">取消令牌（可选）</param>
        /// <returns>实例化后的游戏对象，失败返回 null</returns>
        Task<GameObject> InstantiatePrefabAsync(string address, Transform parent = null,
            bool instantiateInWorldSpace = false, CancellationToken cancellationToken = default);

        #endregion

        #region 批量标签加载

        /// <summary>
        /// 按单个标签异步批量加载资源
        /// </summary>
        /// <typeparam name="T">资源类型（UnityEngine.Object 子类）</typeparam>
        /// <param name="label">资源标签</param>
        /// <param name="onAssetLoaded">单个资源加载完成回调（可选）</param>
        /// <param name="onProgress">加载进度回调（0-1，可选）</param>
        /// <param name="cancellationToken">取消令牌（可选）</param>
        /// <returns>加载成功的资源列表</returns>
        Task<List<T>> LoadAssetsByLabelAsync<T>(string label,
            CancellationToken cancellationToken = default) where T : Object;

        /// <summary>
        /// 按多个标签异步批量加载资源
        /// </summary>
        /// <typeparam name="T">资源类型（UnityEngine.Object 子类）</typeparam>
        /// <param name="labels">标签列表</param>
        /// <param name="mergeMode">标签合并模式（交集/并集）</param>
        /// <param name="onAssetLoaded">单个资源加载完成回调（可选）</param>
        /// <param name="onProgress">加载进度回调（0-1，可选）</param>
        /// <param name="cancellationToken">取消令牌（可选）</param>
        /// <returns>加载成功的资源列表</returns>
        Task<List<T>> LoadAssetsByLabelsAsync<T>(List<string> labels,
            Addressables.MergeMode mergeMode = Addressables.MergeMode.Intersection,
            CancellationToken cancellationToken = default) where T : Object;

        #endregion

        #region 资源释放

        /// <summary>
        /// 释放指定地址的资源（引用计数减1，计数为0时实际释放）
        /// </summary>
        /// <param name="address">资源地址</param>
        /// <param name="forceRelease">是否强制释放（忽略引用计数）</param>
        void ReleaseAsset(string address, bool forceRelease = false);

        /// <summary>
        /// 释放所有已加载的资源
        /// </summary>
        /// <param name="forceRelease">是否强制释放（忽略引用计数）</param>
        void ReleaseAllAssets(bool forceRelease = false);

        /// <summary>
        /// 获取指定资源的引用计数
        /// </summary>
        /// <param name="address">资源地址</param>
        /// <returns>引用计数值，未加载返回0</returns>
        int GetAssetRefCount(string address);

        #endregion

        #region 场景管理

        /// <summary>
        /// 异步加载指定地址的场景（不自动激活）
        /// </summary>
        /// <param name="sceneAddress">场景的 Addressables 地址</param>
        /// <param name="loadMode">场景加载模式（默认叠加加载）</param>
        /// <param name="cancellationToken">取消令牌（可选）</param>
        /// <returns>场景实例，加载失败返回默认值</returns>
        Task<SceneInstance> LoadSceneAsync(string sceneAddress,
            LoadSceneMode loadMode = LoadSceneMode.Additive,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 异步加载并激活指定场景
        /// </summary>
        /// <param name="sceneAddress">场景的 Addressables 地址</param>
        /// <param name="loadMode">场景加载模式（默认叠加加载）</param>
        /// <param name="cancellationToken">取消令牌（可选）</param>
        /// <returns>激活后的场景实例，失败返回默认值</returns>
        Task<SceneInstance> LoadAndActivateSceneAsync(string sceneAddress,
            LoadSceneMode loadMode = LoadSceneMode.Additive,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 异步卸载指定地址的场景
        /// </summary>
        /// <param name="sceneAddress">场景地址</param>
        /// <param name="forceRelease">是否强制卸载（忽略引用计数）</param>
        /// <returns>卸载成功返回true，失败返回false</returns>
        Task<bool> UnloadSceneAsync(string sceneAddress, bool forceRelease = false);

        /// <summary>
        /// 异步释放所有已加载的场景
        /// </summary>
        /// <param name="forceRelease">是否强制释放（忽略引用计数）</param>
        /// <returns>异步任务</returns>
        Task ReleaseAllScenes(bool forceRelease = false);

        /// <summary>
        /// 检查指定场景是否已加载并有效
        /// </summary>
        /// <param name="sceneAddress">场景地址</param>
        /// <returns>已加载且有效返回true，否则返回false</returns>
        bool IsSceneLoaded(string sceneAddress);

        #endregion

        #region 状态查询 & 辅助方法

        /// <summary>
        /// 获取已加载资源和场景的数量
        /// </summary>
        /// <returns>元组(已加载资源数, 已加载场景数)</returns>
        (int assetCount, int sceneCount) GetLoadedCount();

        /// <summary>
        /// 检查指定地址的资源是否已加载
        /// </summary>
        /// <param name="address">资源地址</param>
        /// <returns>已加载返回true，否则返回false</returns>
        bool IsAssetLoaded(string address);

        /// <summary>
        /// 清理所有无效的资源/场景句柄
        /// </summary>
        void CleanInvalidHandles();

        #endregion
    }
}