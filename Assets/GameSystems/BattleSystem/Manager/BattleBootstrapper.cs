using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BH.Framework.Interfaces;
using BH.Framework.Singleton;
using UnityEngine;

namespace BH.GameSystems.BattleSystem.Manager
{
    /// <summary>
    /// 初始化协调器，负责按优先级创建、初始化及销毁所有模块。
    /// </summary>
    public class Bootstrapper : MonoSingleton<Bootstrapper>
    {
        // [Serializable]
        // private class ModuleInfo
        // {
        //     public string typeName; // 类型的程序集限定名（用于反射）
        //     public MonoBehaviour instance; // 直接引用的实例（手动拖拽）
        //     public int priority; // 优先级（越小越先执行）
        //     public bool isInitialized;
        // }
        //
        // [SerializeField] private List<ModuleInfo> modules = new();
        // private bool _isInitializing;
        // private readonly List<IDestroy> _initializedModules = new();
        //
        // public event Action OnAllInitialized;
        // public bool IsAllInitialized { get; private set; }
        //
        // private void Start() => StartInitialization();
        //
        // /// <summary>
        // /// 通过类型注册模块（适用于自动创建的单例）
        // /// </summary>
        // public void RegisterModule<T>(int priority = 0) where T : MonoBehaviour, IInitializable
        // {
        //     var moduleInfo = new ModuleInfo
        //     {
        //         typeName = typeof(T).AssemblyQualifiedName,
        //         priority = priority,
        //         isInitialized = false
        //     };
        //     modules.Add(moduleInfo);
        //     modules = modules.OrderBy(info => info.priority).ToList();
        // }
        //
        // /// <summary>
        // /// 通过实例注册模块（适用于场景中手动放置的对象）
        // /// </summary>
        // public void RegisterModule(MonoBehaviour instance, int priority = 0)
        // {
        //     if (!instance) throw new ArgumentNullException(nameof(instance));
        //     if (instance is not IInitializable)
        //     {
        //         Debug.LogError($"[Bootstrapper] 实例 {instance.name} 未实现 IInitializable 接口");
        //         return;
        //     }
        //
        //     var moduleInfo = new ModuleInfo
        //     {
        //         typeName = instance.name,
        //         instance = instance,
        //         priority = priority,
        //         isInitialized = false
        //     };
        //     modules.Add(moduleInfo);
        //     modules = modules.OrderBy(info => info.priority).ToList();
        // }
        //
        // /// <summary>
        // /// 开始初始化流程
        // /// </summary>
        // public void StartInitialization()
        // {
        //     if (IsAllInitialized || _isInitializing)
        //     {
        //         Debug.LogWarning("[Bootstrapper] 已经初始化完成或正在初始化中");
        //         return;
        //     }
        //
        //     _isInitializing = true;
        //     
        //     StartCoroutine(InitializeCoroutine());
        // }
        //
        // private IEnumerator InitializeCoroutine()
        // {
        //     var count = 1;
        //     foreach (var module in modules.Where(m => !m.isInitialized))
        //     {
        //         // 确保实例存在（若不存在则自动创建）
        //         var targetInstance = EnsureInstance(module);
        //         module.priority = count++;
        //         if (!targetInstance)
        //         {
        //             Debug.LogError($"[Bootstrapper] 无法获取模块实例: {module.typeName ?? "unknown"}");
        //             continue;
        //         }
        //
        //         if (targetInstance is not IInitializable initTarget)
        //         {
        //             Debug.LogError($"[Bootstrapper] 实例 {targetInstance.name} 未实现 IInitializable");
        //             continue;
        //         }
        //
        //         Debug.Log($"[Bootstrapper] 正在初始化 {initTarget.GetType().Name} ...");
        //         module.typeName = module.instance.name;
        //         initTarget.Initialize();
        //         module.isInitialized = true;
        //
        //         if (targetInstance is IDestroy destroyTarget)
        //             _initializedModules.Add(destroyTarget);
        //         
        //         yield return null;
        //     }
        //     
        //     IsAllInitialized = true;
        //     _isInitializing = false;
        //     Debug.Log("[Bootstrapper] 所有模块初始化完成");
        //     OnAllInitialized?.Invoke();
        // }
        //
        // /// <summary>
        // /// 确保模块实例存在，若不存在则尝试通过反射创建
        // /// </summary>
        // private MonoBehaviour EnsureInstance(ModuleInfo module)
        // {
        //     if (module.instance)
        //         return module.instance;
        //
        //     if (string.IsNullOrEmpty(module.typeName))
        //     {
        //         Debug.LogError("[Bootstrapper] 模块信息无效：既无实例引用也无类型名称");
        //         return null;
        //     }
        //
        //     var type = Type.GetType(module.typeName);
        //     if (type == null)
        //     {
        //         Debug.LogError($"[Bootstrapper] 无法解析类型: {module.typeName}");
        //         return null;
        //     }
        //
        //     // 尝试通过静态 Instance 属性获取现有实例（适用于单例）
        //     MonoBehaviour existing = GetSingletonInstance(type);
        //     if (existing != null)
        //         return existing;
        //
        //     // 不存在则创建新对象
        //     Debug.Log($"[Bootstrapper] 为 {type.Name} 创建新实例");
        //     GameObject go = new GameObject(type.Name);
        //     DontDestroyOnLoad(go);
        //     return go.AddComponent(type) as MonoBehaviour;
        // }
        //
        // /// <summary>
        // /// 递归查找类型或其基类中的静态 Instance 属性
        // /// </summary>
        // private MonoBehaviour GetSingletonInstance(Type type)
        // {
        //     const BindingFlags flags = BindingFlags.Public | BindingFlags.Static;
        //     PropertyInfo prop = type.GetProperty("Instance", flags);
        //     if (prop == null && type.BaseType != null)
        //         prop = GetInstanceProperty(type.BaseType);
        //
        //     return prop?.GetValue(null) as MonoBehaviour;
        // }
        //
        // private PropertyInfo GetInstanceProperty(Type type)
        // {
        //     const BindingFlags flags = BindingFlags.Public | BindingFlags.Static;
        //     var prop = type.GetProperty("Instance", flags);
        //     if (prop != null) return prop;
        //
        //     return type.BaseType != null ? GetInstanceProperty(type.BaseType) : null;
        // }
        //
        // /// <summary>
        // /// 按初始化逆序销毁所有已初始化的模块
        // /// </summary>
        // public void Shutdown()
        // {
        //     if (!IsAllInitialized)
        //     {
        //         Debug.LogWarning("[Bootstrapper] 未完成初始化，跳过销毁流程");
        //         return;
        //     }
        //
        //     Debug.Log("[Bootstrapper] 开始销毁模块...");
        //     for (var i = _initializedModules.Count - 1; i >= 0; i--)
        //     {
        //         _initializedModules[i]?.Destroy();
        //     }
        //
        //     _initializedModules.Clear();
        //     IsAllInitialized = false;
        // }
        //
        // protected void OnDestroy()
        // {
        //     if (IsAllInitialized)
        //         Shutdown();
        //     //base.OnDestroy();
        // }
    }
}