using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BH.Framework.Infrastructure.Resource.Data;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Zenject;
using Object = UnityEngine.Object;

namespace BH.Framework.Infrastructure.Resource.Interfaces
{
    /// <summary>
    /// 游戏资源管理服务接口
    /// 定义资源缓存、配置加载、场景管理、预加载、类型安全的资源操作核心契约
    /// </summary>
    public interface IResourceService : IInitializable, IDisposable
    {
        #region 生命周期管理

        /// <summary>
        /// 启用资源服务
        /// </summary>
        void Enable();

        /// <summary>
        /// 禁用资源服务
        /// </summary>
        void Disable();

        #endregion

        #region 预加载管理

        /// <summary>
        /// 预加载所有注册的资源组和核心配置
        /// </summary>
        Task PreloadAllResourcesAsync();

        /// <summary>
        /// 添加预加载组（资源名称为 AssetKeys.AddressableNames 常量）
        /// </summary>
        /// <param name="groupName">预加载组名称</param>
        /// <param name="resourceNames">AssetKeys.AddressableNames 常量列表</param>
        void AddPreloadGroup(string groupName, List<string> resourceNames);

        /// <summary>
        /// 异步加载指定预加载组
        /// </summary>
        /// <param name="groupName">预加载组名称</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task PreloadGroupAsync(string groupName, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取所有预加载组名称
        /// </summary>
        List<string> GetAllPreloadGroups();

        #endregion

        #region 常驻资源管理

        /// <summary>
        /// 异步加载常驻资源（适配 AssetKeys.AddressableNames 常量）
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="resourceName">AssetKeys.AddressableNames 常量</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>资源加载结果</returns>
        Task<ResourceLoadResult<T>> LoadPersistentAssetAsync<T>(string resourceName,
            CancellationToken cancellationToken = default)
            where T : Object;

        /// <summary>
        /// 卸载常驻资源
        /// </summary>
        /// <param name="resourceName">AssetKeys.AddressableNames 常量</param>
        /// <param name="forceRelease">强制释放（忽略引用计数）</param>
        void UnloadPersistentAsset(string resourceName, bool forceRelease = false);

        /// <summary>
        /// 检查资源是否在常驻缓存中
        /// </summary>
        /// <param name="resourceName">AssetKeys.AddressableNames 常量</param>
        /// <returns>是否存在于缓存</returns>
        bool IsPersistentAssetCached(string resourceName);

        #endregion

        #region 配置加载

        /// <summary>
        /// 异步加载 JSON 配置（适配 AssetKeys.AddressableNames 配置常量）
        /// </summary>
        /// <typeparam name="T">配置类型</typeparam>
        /// <param name="configName">AssetKeys.AddressableNames 配置常量</param>
        /// <param name="useCache">是否使用缓存</param>
        /// <returns>配置加载结果</returns>
        Task<ResourceLoadResult<T>> LoadJsonConfigAsync<T>(string configName, bool useCache = true);

        /// <summary>
        /// 清理配置缓存
        /// </summary>
        /// <param name="configName">配置名称（null 清理全部）</param>
        void ClearConfigCache(string configName = null);

        #endregion

        #region 场景管理

        /// <summary>
        /// 异步加载场景（适配 AssetKeys.AddressableNames 场景常量）
        /// </summary>
        /// <param name="sceneLogicalName">场景逻辑名称</param>
        /// <param name="sceneAddress">AssetKeys.AddressableNames 场景常量</param>
        /// <param name="loadMode">加载模式</param>
        /// <returns>场景加载结果</returns>
        Task<ResourceLoadResult<SceneInstance>> LoadSceneAsync(string sceneLogicalName, string sceneAddress,
            LoadSceneMode loadMode = LoadSceneMode.Additive);

        /// <summary>
        /// 异步加载并激活场景
        /// </summary>
        /// <param name="sceneLogicalName">场景逻辑名称</param>
        /// <param name="sceneAddress">AssetKeys.AddressableNames 场景常量</param>
        /// <param name="loadMode">加载模式</param>
        /// <returns>场景加载结果</returns>
        Task<ResourceLoadResult<SceneInstance>> LoadAndActivateSceneAsync(string sceneLogicalName,
            string sceneAddress, LoadSceneMode loadMode = LoadSceneMode.Additive);

        /// <summary>
        /// 异步卸载场景
        /// </summary>
        /// <param name="sceneLogicalName">场景逻辑名称</param>
        /// <param name="forceRelease">强制释放</param>
        /// <returns>是否卸载成功</returns>
        Task<bool> UnloadSceneAsync(string sceneLogicalName, bool forceRelease = false);

        /// <summary>
        /// 检查场景是否已加载
        /// </summary>
        /// <param name="sceneLogicalName">场景逻辑名称</param>
        /// <returns>是否已加载</returns>
        bool IsSceneLoaded(string sceneLogicalName);

        #endregion

        #region 预制体实例化

        /// <summary>
        /// 异步实例化预制体（适配 AssetKeys.AddressableNames 预制体常量）
        /// </summary>
        /// <param name="prefabName">AssetKeys.AddressableNames 预制体常量</param>
        /// <param name="parent">父节点</param>
        /// <param name="instantiateInWorldSpace">是否使用世界空间坐标</param>
        /// <returns>预制体实例化结果</returns>
        Task<ResourceLoadResult<GameObject>> InstantiatePrefabAsync(string prefabName,
            Transform parent = null, bool instantiateInWorldSpace = false);

        #endregion

        #region 辅助方法

        /// <summary>
        /// 获取已加载场景列表
        /// </summary>
        /// <returns>场景逻辑名称列表</returns>
        List<string> GetLoadedScenes();

        /// <summary>
        /// 获取常驻缓存资源数量
        /// </summary>
        /// <returns>缓存资源数量</returns>
        int GetPersistentCacheCount();

        #endregion
    }
}