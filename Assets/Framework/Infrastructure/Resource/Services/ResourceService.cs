using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BH.Framework.Infrastructure.Logging.Interfaces;
using BH.Framework.Infrastructure.Resource.Interfaces;
using Google.Protobuf;
using Newtonsoft.Json;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace BH.Framework.Infrastructure.Resource.Services
{
    /// <summary>
    /// 游戏资源管理器
    /// </summary>
    [Serializable]
    public class ResourceService : IResourceService
    {
        #region 依赖注入字段

        [Inject]private readonly IAddressableService _addressableService;
        [Inject]private readonly ILogService _logService;

        /// <summary>
        /// 与 Addressables 系统交互的底层管理器。
        /// </summary>
        private ProtobufService _protobufService;

        #endregion

        #region 私有字段 (缓存/映射表)

        /// <summary>
        /// 资源地址映射表。将资源的逻辑名称（如 "PlayerAvatar"）映射到 Addressables 系统中的具体地址。
        /// 此映射表可以从外部配置文件加载。
        /// </summary>
        private readonly Dictionary<string, string> _resourcePaths = new();

        /// <summary>
        /// 常驻内存缓存。存储需要长期驻留在内存中的资源对象。
        /// Key: 资源的逻辑名称, Value: 资源对象实例 (UnityEngine.Object)。
        /// </summary>
        private readonly Dictionary<string, Object> _persistentCache = new();

        /// <summary>
        /// 预加载资源组。允许将资源按功能或场景分组，以便提前批量加载。
        /// Key: 预加载组的名称 (如 "Scene_Level1"), Value: 该组包含的资源逻辑名称列表。
        /// </summary>
        private readonly Dictionary<string, List<string>> _preloadGroups = new();

        /// <summary>
        /// 配置数据缓存。缓存已加载并反序列化的配置对象，避免重复加载和解析。
        /// Key: 配置文件的逻辑名称, Value: 反序列化后的配置对象。
        /// </summary>
        private readonly Dictionary<string, object> _configCache = new();

        #endregion

        #region 接口实现

        public void Initialize()
        {
            _logService.Info("初始化");
        }

        #endregion

        #region 核心工具方法

        /// <summary>
        /// 根据资源的逻辑名称获取其在 Addressables 系统中的地址。
        /// </summary>
        /// <param name="resourceName">资源的逻辑名称。</param>
        /// <returns>对应的地址字符串；如果未找到映射则返回 null。</returns>
        public string GetResourceAddress(string resourceName)
        {
            if (_resourcePaths.TryGetValue(resourceName, out var address))
            {
                return address;
            }

            _logService.Error($"未找到资源名称 '{resourceName}' 的映射地址。");
            return null;
        }

        /// <summary>
        /// 从配置文件加载 _resourcePaths 映射表。
        /// 此方法在构造函数中被调用，尝试初始化资源路径映射。
        /// </summary>
        private void LoadResourcePathsFromConfig()
        {
            // 示例路径 (需要在 Addressables Groups 中设置)
            // const string configPath = "Assets/Addressables/Local/Config/ResourcePaths.json";

            // 由于在构造函数中难以进行异步加载，这里先使用硬编码的默认路径作为回退。
            // 理想情况下，这个映射文件也应该通过 Addressables 加载。
            // 例如，你可以为映射文件创建一个 Addressable 标签或地址，如 "ResourcePathMapping"。

            // 尝试从预设的映射文件加载（这里使用硬编码的键名作为示例）
            const string mappingResourceName = "ResourcePathMapping";

            // 这里是一个简化的回退逻辑。
            // 更好的方式是在游戏启动流程中异步加载 ResourcePathMapping 文件，
            // 然后调用一个方法（如 InitResourcePaths(Dictionary<string, string> mapping)）来设置。
            // 目前，如果没有找到 "ResourcePathMapping"，则使用硬编码。

            // 假设 _resourcePaths 在初始化时已经包含了 "ResourcePathMapping" 的地址
            // if (_resourcePaths.ContainsKey(mappingResourceName))
            // {
            //     string mappingContent = await LoadTextConfigAsStringAsync(mappingResourceName);
            //     if (!string.IsNullOrEmpty(mappingContent))
            //     {
            //         // Parse mappingContent (JSON) into _resourcePaths dictionary
            //         // e.g., using JsonUtility or JsonConvert
            //     }
            // }

            // 硬编码默认路径（用于开发或作为回退）
            _resourcePaths["Characters"] = "Assets/Addressables/Local/Config/CharacterData/Characters.json";
            // ... 添加更多默认路径 ...

            _logService.Info("资源路径已从硬编码默认值加载。");
        }

        /// <summary>
        /// 手动添加或替换一个资源路径映射。
        /// 适用于运行时动态添加资源路径，或用于测试和热重载。
        /// </summary>
        /// <param name="logicalName">资源的逻辑名称。</param>
        /// <param name="address">资源在 Addressables 系统中的地址。</param>
        public void AddOrReplaceResourcePath(string logicalName, string address)
        {
            _resourcePaths[logicalName] = address;
            _logService.Info($"添加/替换资源路径: {logicalName} -> {address}");
        }

        #endregion

        #region 常驻资源加载/卸载

        /// <summary>
        /// 根据逻辑名称异步加载一个资源，并将其存储在常驻内存缓存中。
        /// 如果资源已在缓存中，则直接返回缓存的对象。
        /// </summary>
        /// <typeparam name="T">期望加载的资源类型，必须继承自 UnityEngine.Object (如 GameObject, Sprite, AudioClip 等)。</typeparam>
        /// <param name="resourceName">资源的逻辑名称（例如 "PlayerAvatar"），需在 _resourcePaths 中有对应映射。</param>
        /// <returns>一个 Task，完成后返回加载或从缓存获取的资源实例；如果加载失败或找不到映射则返回 null。</returns>
        public async Task<T> LoadPersistentAssetAsync<T>(string resourceName) where T : Object
        {
            if (!_resourcePaths.TryGetValue(resourceName, out string address))
            {
                _logService.Error($"未找到资源名称 '{resourceName}' 的映射地址。");
                return null;
            }

            // 检查资源是否已在常驻缓存中
            if (_persistentCache.TryGetValue(resourceName, out Object cachedObject))
            {
                _logService.Info($"从常驻缓存加载资源: {resourceName} ({address})");
                if (cachedObject is T typedObject)
                {
                    return typedObject;
                }
                else
                {
                    _logService.Error(
                        $"常驻缓存中的资源 '{resourceName}' 类型不匹配。期望 {typeof(T)}, 实际 {cachedObject.GetType()}");
                    return null;
                }
            }

            // 从 Addressables 系统加载资源
            T asset = await _addressableService.LoadAssetAsync<T>(address);
            if (!asset) return asset; // 如果加载失败，直接返回 null

            // 加载成功后，将其放入常驻缓存
            _persistentCache[resourceName] = asset;
            _logService.Info($"成功加载并缓存常驻资源: {resourceName} ({address})");
            return asset;
        }

        /// <summary>
        /// 从常驻缓存中移除一个资源。
        /// </summary>
        /// <param name="resourceName">要从常驻缓存中卸载的资源名称。</param>
        public void UnloadPersistentAsset(string resourceName)
        {
            if (_persistentCache.Remove(resourceName, out _))
            {
                _logService.Info($"已从常驻缓存卸载资源: {resourceName}");
                // 注意：对象本身可能仍有其他引用，只有当所有引用都被移除时，GC 才会回收其内存。
                // Addressables 的句柄需要通过 _addressableService.ReleaseAsset 来释放。
                // 如果需要彻底释放，可能需要 ResourceService 记录加载地址，并调用 _addressableManager.ReleaseAsset。
                // 当前实现仅从缓存移除。
            }
            else
            {
                _logService.Warning($"尝试卸载未在常驻缓存中的资源: {resourceName}");
            }
        }

        /// <summary>
        /// 获取常驻缓存中资源的数量。
        /// </summary>
        /// <returns>当前在常驻缓存中的资源数量。</returns>
        public int GetPersistentCacheCount()
        {
            return _persistentCache.Count;
        }

        /// <summary>
        /// 检查一个资源是否已经在常驻缓存中。
        /// </summary>
        /// <param name="resourceName">要检查的资源名称。</param>
        /// <returns>如果资源在常驻缓存中则返回 true，否则返回 false。</returns>
        public bool IsPersistentAssetLoaded(string resourceName)
        {
            return _persistentCache.ContainsKey(resourceName);
        }

        #endregion

        #region 预制体实例化

        /// <summary>
        /// 根据逻辑名称异步加载一个预制体并实例化为 GameObject。
        /// 注意：此方法加载的预制体不会被放入常驻缓存。
        /// </summary>
        /// <param name="resourceName">预制体的逻辑名称（例如 "PlayerAvatar"），需在 _resourcePaths 中有对应映射。</param>
        /// <param name="parent">新实例化 GameObject 的父对象 (Transform)。可以为 null。</param>
        /// <param name="instantiateInWorldSpace">
        /// 如果为 true，则新实例的位置、旋转和缩放不会受父对象的影响；
        /// 如果为 false，则会相对于父对象进行定位。
        /// </param>
        /// <returns>一个 Task，完成后返回实例化的 GameObject；如果加载或实例化失败则返回 null。</returns>
        public async Task<Object> InstantiatePrefabAsync(string resourceName, Transform parent = null,
            bool instantiateInWorldSpace = false)
        {
            if (!_resourcePaths.TryGetValue(resourceName, out string address))
            {
                _logService.Error($"未找到预制体名称 '{resourceName}' 的映射地址。");
                return null;
            }

            // 加载预制体模板
            Object prefab = await _addressableService.LoadAssetAsync<Object>(address);
            if (prefab)
            {
                Object instance = Object.Instantiate(prefab, parent, instantiateInWorldSpace);
                _logService.Info($"成功实例化预制体: {resourceName} ({address})");
                return instance;
            }

            _logService.Error($"加载预制体失败: {resourceName} ({address})");
            return null;
        }

        #endregion

        #region 配置文件加载 (Protobuf/JSON/Text)

        /// <summary>
        /// 异步加载一个二进制 Protobuf 文件 (.bytes)，将其反序列化为目标 Protobuf 消息类型 T，
        /// 并可选择性地缓存结果。
        /// </summary>
        /// <typeparam name="T">期望反序列化后得到的 Protobuf 消息类型，必须实现 IMessage&lt;T&gt;。</typeparam>
        /// <param name="resourceName">Protobuf 文件的逻辑名称（例如 "PlayerData"），需在 _resourcePaths 中有对应映射。</param>
        /// <param name="useCache">如果为 true，则将加载和反序列化的结果缓存起来，后续调用会优先返回缓存值。默认为 true。</param>
        /// <returns>一个 Task，完成后返回反序列化后的 Protobuf 消息对象；如果加载或反序列化失败则返回 null。</returns>
        public async Task<T> LoadProtobufDataAsync<T>(string resourceName, bool useCache = true)
            where T : class, IMessage<T>, new()
        {
            // 1. 检查缓存
            if (useCache && _configCache.TryGetValue(resourceName, out object cachedData))
            {
                if (cachedData is T typedCachedData)
                {
                    _logService.Info($"从缓存加载 Protobuf 数据: {resourceName}");
                    return typedCachedData;
                }
                else
                {
                    _logService.Error(
                        $"配置缓存中的数据 '{resourceName}' 类型不匹配。期望 {typeof(T)}, 实际 {cachedData.GetType()}");
                }
            }

            _logService.Info($"开始加载 Protobuf 数据: {resourceName}");

            // 2. 从 Addressables 加载二进制文件 (Byte Asset)
            string address = GetResourceAddress(resourceName);
            if (string.IsNullOrEmpty(address))
            {
                _logService.Error($"无法获取 Protobuf 文件 '{resourceName}' 的地址。");
                return null;
            }

            // 假设您的 .bytes 文件在 Addressables 中被当作 TextAsset 或 Binary ScriptableObject 处理。
            // 如果是 .bytes 文件，通常用 TextAsset 加载其 raw bytes。
            TextAsset textAsset = await _addressableService.LoadAssetAsync<TextAsset>(address);
            if (!textAsset)
            {
                _logService.Error($"加载 TextAsset 失败，Protobuf 文件名称: {resourceName}");
                return null;
            }

            // 3. 使用 ProtobufService 反序列化
            T protobufData = _protobufService.DeserializeFromBytes<T>(textAsset.bytes);

            if (protobufData != null && useCache)
            {
                // 4. 如果成功且启用缓存，则存入缓存
                _configCache[resourceName] = protobufData;
                _logService.Info($"成功加载并缓存 Protobuf 数据: {resourceName}");
            }
            else if (protobufData != null)
            {
                _logService.Info($"成功加载 Protobuf 数据 (未缓存): {resourceName}");
            }

            return protobufData;
        }

        /// <summary>
        /// 异步加载一个 JSON 格式的配置文件，将其反序列化为目标类型 T，并可选择性地缓存结果。
        /// </summary>
        /// <typeparam name="T">期望反序列化后得到的数据类型（例如 CharacterData[], SkillData 等）。</typeparam>
        /// <param name="configResourceName">配置文件的逻辑名称（例如 "Characters"），需在 _resourcePaths 中有对应映射。</param>
        /// <param name="useCache">如果为 true，则将加载和反序列化的结果缓存起来，后续调用会优先返回缓存值。默认为 true。</param>
        /// <returns>一个 Task，完成后返回反序列化后的对象；如果加载或反序列化失败则返回类型的默认值 (default(T))。</returns>
        public async Task<T> LoadJsonConfigAsync<T>(string configResourceName, bool useCache = true)
        {
            // 如果启用缓存，首先检查缓存
            if (useCache && _configCache.TryGetValue(configResourceName, out object cachedData))
            {
                if (cachedData is T typedCachedData)
                {
                    _logService.Info($"从缓存加载 JSON 配置: {configResourceName}");
                    return typedCachedData;
                }
                else
                {
                    _logService.Error(
                        $"配置缓存中的数据 '{configResourceName}' 类型不匹配。期望 {typeof(T)}, 实际 {cachedData.GetType()}");
                    return default;
                }
            }

            _logService.Info($"尝试加载 JSON 配置: {configResourceName}");

            // 使用底层的 _addressableService 加载 TextAsset
            var textAsset =
                await _addressableService.LoadAssetAsync<TextAsset>(GetResourceAddress(configResourceName));
            if (!textAsset)
            {
                _logService.Error($"加载 TextAsset 失败，配置名称: {configResourceName}");
                return default;
            }

            string jsonContent = textAsset.text;
            if (string.IsNullOrEmpty(jsonContent))
            {
                _logService.Error($"加载的 TextAsset 内容为空，配置名称: {configResourceName}");
                return default;
            }

            try
            {
                var configData = JsonConvert.DeserializeObject<T>(jsonContent, new JsonSerializerSettings
                {
                    // 根据需要配置反序列化选项 (例如，NullValueHandling, MissingMemberHandling)
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                });

                // 如果启用缓存，将结果存入缓存
                if (useCache)
                {
                    _configCache[configResourceName] = configData;
                    _logService.Info($"成功加载并缓存 JSON 配置: {configResourceName}");
                }
                else
                {
                    _logService.Info($"成功加载 JSON 配置 (未缓存): {configResourceName}");
                }

                return configData;
            }
            catch (Exception ex)
            {
                _logService.Error(
                    $"解析 JSON 配置时出错 '{configResourceName}'");
                return default;
            }
        }

        /// <summary>
        /// 异步加载一个 TextAsset（通常用于 JSON 或纯文本格式的配置文件）并返回其内容字符串。
        /// </summary>
        /// <param name="configResourceName">配置文件的逻辑名称。</param>
        /// <returns>一个 Task，完成后返回 TextAsset 的文本内容；如果加载失败则返回 null。</returns>
        public async Task<string> LoadTextConfigAsStringAsync(string configResourceName)
        {
            TextAsset textAsset =
                await _addressableService.LoadAssetAsync<TextAsset>(GetResourceAddress(configResourceName));
            return textAsset?.text; // 如果 textAsset 为 null，?. 操作符会返回 null
        }

        #endregion

        #region 预加载组管理

        /// <summary>
        /// 将一组资源添加到指定的预加载组中。
        /// 这些资源将在后续调用 PreloadGroupAsync 时被批量加载。
        /// </summary>
        /// <param name="groupName">预加载组的名称（例如 "Scene_Level1"）。</param>
        /// <param name="resourceNames">要添加到该组的资源名称列表。</param>
        public void AddToPreloadGroup(string groupName, List<string> resourceNames)
        {
            if (!_preloadGroups.ContainsKey(groupName))
            {
                _preloadGroups[groupName] = new List<string>();
            }

            _preloadGroups[groupName].AddRange(resourceNames);
            _logService.Info($"已将 {resourceNames.Count} 个资源添加到预加载组 '{groupName}'。");
        }

        /// <summary>
        /// 异步加载指定预加载组中的所有资源，并将它们放入常驻缓存。
        /// 通常用于在进入新场景或开启新功能前，提前加载所需的资源。
        /// </summary>
        /// <param name="groupName">要加载的预加载组名称。</param>
        /// <returns>一个 Task，当组内所有资源都加载完成时该 Task 完成。</returns>
        public async Task PreloadGroupAsync(string groupName)
        {
            if (!_preloadGroups.TryGetValue(groupName, out List<string> resourceList))
            {
                _logService.Warning($"尝试加载不存在的预加载组: {groupName}");
                return;
            }

            _logService.Info($"开始预加载组: {groupName}，共 {resourceList.Count} 个资源。");

            // 使用 Task.WhenAll 并发加载所有资源，以提高效率
            List<Task> tasks = new List<Task>();
            foreach (string resourceName in resourceList)
            {
                if (_resourcePaths.TryGetValue(resourceName, out _))
                {
                    // 将预加载的资源加入常驻缓存
                    tasks.Add(LoadPersistentAssetAsync<Object>(resourceName));
                }
                else
                {
                    _logService.Error($"预加载组 '{groupName}' 中包含无效资源名称: {resourceName}");
                }
            }

            await Task.WhenAll(tasks);
            _logService.Info($"预加载组 '{groupName}' 完成。");
        }

        #endregion

        #region 缓存清理

        public void Dispose()
        {
            _persistentCache.Clear();
            _configCache.Clear();
            _addressableService.ReleaseAllAssets(); // 通知底层系统释放所有资源
            _logService.Info("已清除所有缓存和底层资源。");
        }

        #endregion
    }
}