using System;
using System.Threading.Tasks;
using UnityEngine;
using IInitializable = Zenject.IInitializable;

namespace BH.Framework.Infrastructure.Resource.Interfaces
{
    /// <summary>
    /// Addressables 资源服务接口，定义核心操作契约
    /// </summary>
    public interface IAddressableService : IInitializable, IDisposable
    {
        /// <summary>
        /// 异步加载指定地址的资源
        /// </summary>
        /// <typeparam name="T">期望加载的资源类型，必须继承自 UnityEngine.Object</typeparam>
        /// <param name="address">资源在 Addressables 系统中注册的地址或标签</param>
        /// <returns>加载成功的资源实例；加载失败则返回 null</returns>
        Task<T> LoadAssetAsync<T>(string address) where T : class;

        /// <summary>
        /// 异步加载预制体并实例化为 GameObject
        /// </summary>
        /// <param name="address">预制体地址</param>
        /// <param name="parent">父对象Transform（可为null）</param>
        /// <param name="instantiateInWorldSpace">是否使用世界空间坐标实例化</param>
        /// <returns>实例化后的GameObject；失败则返回 null</returns>
        Task<GameObject> InstantiatePrefabAsync(string address, Transform parent = null,
            bool instantiateInWorldSpace = false);

        /// <summary>
        /// 根据地址释放已加载的资源
        /// </summary>
        /// <param name="address">资源地址</param>
        void ReleaseAsset(string address);

        /// <summary>
        /// 释放所有已加载的资源
        /// </summary>
        void ReleaseAllAssets();

        /// <summary>
        /// 获取当前已加载资源的数量
        /// </summary>
        /// <returns>已加载资源数量</returns>
        int GetLoadedAssetCount();

        /// <summary>
        /// 检查指定地址的资源是否已加载
        /// </summary>
        /// <param name="address">资源地址</param>
        /// <returns>已加载返回true，否则返回false</returns>
        bool IsAssetLoaded(string address);
    }
}