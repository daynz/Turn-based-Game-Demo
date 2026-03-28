using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BH.Framework.Infrastructure.Resource.Data;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace BH.Framework.Infrastructure.Resource.Interfaces
{
    /// <summary>
    /// 资源管理高层服务接口
    /// 提供资源加载/卸载、配置加载、预制体实例化、资源预加载、LRU 缓存管理等统一入口
    /// </summary>
    public interface IResourceService : IInitializable, IDisposable
    {
        /// <summary>
        /// 启用资源服务
        /// </summary>
        void Enable();

        /// <summary>
        /// 禁用资源服务
        /// </summary>
        void Disable();

        #region 预加载管理

        /// <summary>
        /// 预加载所有注册的资源组
        /// </summary>
        Task PreloadAllResourcesAsync();

        /// <summary>
        /// 添加预加载资源组
        /// </summary>
        /// <param name="groupName">组名</param>
        /// <param name="resourceNames">资源地址列表</param>
        void AddPreloadGroup(string groupName, List<string> resourceNames);

        /// <summary>
        /// 异步预加载指定资源组
        /// </summary>
        /// <param name="groupName">组名</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task PreloadGroupAsync(string groupName, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取所有预加载组名称
        /// </summary>
        List<string> GetAllPreloadGroups();

        #endregion

        #region 常驻资源管理

        /// <summary>
        /// 异步加载常驻资源（带 LRU 缓存）
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="resourceName">资源地址</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>资源加载结果</returns>
        Task<ResourceLoadResult<T>> LoadPersistentAssetAsync<T>(string resourceName,
            CancellationToken cancellationToken = default) where T : Object;

        /// <summary>
        /// 卸载常驻资源
        /// </summary>
        /// <param name="resourceName">资源地址</param>
        /// <param name="forceRelease">是否强制释放</param>
        void UnloadPersistentAsset(string resourceName, bool forceRelease = false);

        /// <summary>
        /// 检查资源是否已缓存
        /// </summary>
        /// <param name="resourceName">资源地址</param>
        bool IsPersistentAssetCached(string resourceName);

        #endregion

        #region 配置加载

        /// <summary>
        /// 异步加载 JSON 配置
        /// </summary>
        /// <typeparam name="T">配置类型</typeparam>
        /// <param name="configName">配置地址</param>
        /// <param name="useCache">是否使用缓存</param>
        /// <returns>配置加载结果</returns>
        Task<ResourceLoadResult<T>> LoadJsonConfigAsync<T>(string configName, bool useCache = true);

        /// <summary>
        /// 清理配置缓存
        /// </summary>
        /// <param name="configName">配置名（为空则清理全部）</param>
        void ClearConfigCache(string configName = null);

        #endregion

        #region 预制体实例化

        /// <summary>
        /// 异步实例化预制体
        /// </summary>
        /// <param name="prefabName">预制体地址</param>
        /// <param name="parent">父物体</param>
        /// <param name="worldSpace">是否使用世界坐标</param>
        /// <returns>实例化结果</returns>
        Task<ResourceLoadResult<GameObject>> InstantiatePrefabAsync(string prefabName, Transform parent = null,
            bool worldSpace = false);

        #endregion

        #region 辅助方法

        /// <summary>
        /// 获取常驻缓存数量
        /// </summary>
        int GetPersistentCacheCount();

        #endregion
    }
}