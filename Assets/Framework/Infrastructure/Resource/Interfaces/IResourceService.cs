using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace BH.Framework.Infrastructure.Resource.Interfaces
{
    public interface IResourceService : IInitializable, IDisposable
    {
        string GetResourceAddress(string resourceName);
        void AddOrReplaceResourcePath(string logicalName, string address);
        Task<T> LoadPersistentAssetAsync<T>(string resourceName) where T : Object;
        void UnloadPersistentAsset(string resourceName);
        int GetPersistentCacheCount();
        bool IsPersistentAssetLoaded(string resourceName);

        Task<Object> InstantiatePrefabAsync(string resourceName, Transform parent = null,
            bool instantiateInWorldSpace = false);

        Task<T> LoadProtobufDataAsync<T>(string resourceName, bool useCache = true)
            where T : class, IMessage<T>, new();

        Task<T> LoadJsonConfigAsync<T>(string configResourceName, bool useCache = true);
        Task<string> LoadTextConfigAsStringAsync(string configResourceName);
        void AddToPreloadGroup(string groupName, List<string> resourceNames);
        Task PreloadGroupAsync(string groupName);
    }
}