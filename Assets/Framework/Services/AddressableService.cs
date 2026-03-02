using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace BH.Framework.Services
{
    /// <summary>
    /// Addressables 系统封装类。
    /// </summary>
    public class AddressableService
    {
        /// <summary>
        /// 存储所有已加载资源的异步操作句柄 (AsyncOperationHandle)<br/>
        /// Key: 资源地址 (Address)，Value: 对应的 AsyncOperationHandle。
        /// </summary>
        private readonly Dictionary<string, AsyncOperationHandle> _loadedHandles = new();

        /// <summary>
        /// 异步加载指定地址的资源。
        /// </summary>
        /// <typeparam name="T">期望加载的资源类型，必须继承自 UnityEngine.Object。</typeparam>
        /// <param name="address">资源在 Addressables 系统中注册的地址或标签。</param>
        /// <returns>一个 Task，完成后返回加载成功的资源实例；如果加载失败，则返回 null。</returns>
        public async Task<T> LoadAssetAsync<T>(string address) where T : class
        {
            if (string.IsNullOrEmpty(address))
            {
                Debug.LogError($"[AddressableService] 加载失败: 地址为空或null。");
                return null;
            }

            try
            {
                // 检查资源是否已被加载（通过检查内部句柄缓存）
                if (_loadedHandles.ContainsKey(address))
                {
                    Debug.LogWarning($"[AddressableService] 资源 '{address}' 已经被加载，正在从缓存中获取。");
                    // 注意：这里直接从句柄缓存取结果，假设资源未被外部释放。
                    // 在实际复杂场景下，可能需要更严谨的检查。
                    if (_loadedHandles[address].Result is T result)
                    {
                        return result;
                    }
                    else
                    {
                        Debug.LogError(
                            $"[AddressableService] 缓存中的资源 '{address}' 类型不匹配。期望 {typeof(T)}," +
                            $" 实际 {(_loadedHandles[address].Result?.GetType() ?? typeof(object))}");
                        return null;
                    }
                }

                // 开始异步加载
                var handle = Addressables.LoadAssetAsync<T>(address);
                var resultAsset = await handle.Task;

                if (resultAsset != null)
                {
                    // 加载成功，将句柄存储起来以便后续释放
                    _loadedHandles.Add(address, handle);
                    Debug.Log($"[AddressableService] 成功加载资源: {address} (Type: {typeof(T)})");
                    return resultAsset;
                }
                else
                {
                    Debug.LogError(
                        $"[AddressableService] 加载资源失败: {address}. 错误详情: {handle.Status}, {handle.OperationException}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AddressableService] 加载资源时发生异常: {address}\n{ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// 异步加载一个预制体 (Prefab) 并立即实例化为 GameObject。
        /// </summary>
        /// <param name="address">预制体在 Addressables 系统中注册的地址。</param>
        /// <param name="parent">新实例化的 GameObject 的父对象 (Transform)。可以为 null。</param>
        /// <param name="instantiateInWorldSpace">
        /// 如果为 true，则新实例的位置、旋转和缩放不会受父对象的影响；
        /// 如果为 false，则会相对于父对象进行定位。
        /// </param>
        /// <returns>一个 Task，完成后返回实例化成功的 GameObject；如果加载或实例化失败，则返回 null。</returns>
        public async Task<GameObject> InstantiatePrefabAsync(string address, Transform parent = null,
            bool instantiateInWorldSpace = false)
        {
            var prefab = await LoadAssetAsync<GameObject>(address);
            if (!prefab)
            {
                Debug.LogError($"[AddressableService] 无法实例化预制体，因为加载失败: {address}");
                return null;
            }

            var instance = Object.Instantiate(prefab, parent, instantiateInWorldSpace);
            Debug.Log($"[AddressableService] 成功实例化预制体: {address}");
            return instance;
        }

        /// <summary>
        /// 根据资源地址释放一个已加载的资源。
        /// 注意：此方法会从内部缓存中移除句柄，并通知 Addressables 系统释放该资源。
        /// 请确保没有其他地方再持有该资源的引用，否则可能导致未定义行为。
        /// </summary>
        /// <param name="address">要释放的资源的地址。</param>
        public void ReleaseAsset(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                Debug.LogError("[AddressableService] 释放失败: 地址为空或null。");
                return;
            }

            if (_loadedHandles.TryGetValue(address, out AsyncOperationHandle handle))
            {
                Addressables.Release(handle);
                _loadedHandles.Remove(address);
                Debug.Log($"[AddressableService] 已释放资源: {address}");
            }
            else
            {
                Debug.LogWarning($"[AddressableService] 尝试释放未加载的资源: {address}");
            }
        }

        /// <summary>
        /// 释放所有当前已加载的资源。
        /// 通常在应用程序关闭、场景切换或需要彻底清理内存时调用。
        /// 调用此方法后，内部的句柄缓存会被清空。
        /// </summary>
        public void ReleaseAllAssets()
        {
            foreach (var pair in _loadedHandles)
            {
                Addressables.Release(pair.Value);
                Debug.Log($"[AddressableService] 已释放资源: {pair.Key}");
            }

            _loadedHandles.Clear();
            Debug.Log("[AddressableService] 已清空所有资源引用。");
        }

        /// <summary>
        /// 获取当前已加载资源的数量。
        /// </summary>
        /// <returns>已加载资源的数量。</returns>
        public int GetLoadedAssetCount()
        {
            return _loadedHandles.Count;
        }

        /// <summary>
        /// 检查一个资源是否已经被加载（即其句柄是否存在于内部缓存中）。
        /// </summary>
        /// <param name="address">要检查的资源地址。</param>
        /// <returns>如果已加载则返回 true，否则返回 false。</returns>
        public bool IsAssetLoaded(string address)
        {
            return !string.IsNullOrEmpty(address) && _loadedHandles.ContainsKey(address);
        }

        /// <summary>
        /// 获取内部已加载句柄的副本字典。
        /// ResourceManager 可能需要此信息来管理其自身的缓存与 Addressables 句柄之间的关系。
        /// </summary>
        /// <returns>已加载句柄的副本字典。</returns>
        internal Dictionary<string, AsyncOperationHandle> GetLoadedHandlesCopy()
        {
            return new Dictionary<string, AsyncOperationHandle>(_loadedHandles);
        }
    }
}