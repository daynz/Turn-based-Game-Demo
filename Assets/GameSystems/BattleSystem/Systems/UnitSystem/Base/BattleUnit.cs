using System;
using BH.Framework.Infrastructure.Logging.Core;
using BH.GameSystems.BattleSystem.Enums;
using BH.GameSystems.BattleSystem.Interfaces;
using UnityEngine;

namespace BH.GameSystems.BattleSystem.Systems.UnitSystem.Base
{
    [Serializable]
    public class BattleUnit<TData> : MonoBehaviour, IBattleUnit<TData>
        where TData : IUnitData<IUnitSaveData, IUnitRuntimeData>
    {
        public string Name => GetType().Name;
        [SerializeField] protected string unitId = "";
        [SerializeField] protected TData data;
        [SerializeField] private UnitFaction faction;
        [SerializeField] private bool isInitialized;
        [SerializeField] private bool isAlive;

         protected LogService LOGService = null;

        // private Dictionary<Type, ICharacterModule> _modules;

        public string UnitId => unitId;

        public virtual TData Data
        {
            get => data;
            protected set => data = value;
        }

        public virtual UnitFaction Faction
        {
            get => faction;
            set => faction = value;
        }

        public virtual bool IsInitialized
        {
            get => isInitialized;
            private set => isInitialized = value;
        }

        public bool IsAlive
        {
            get => isAlive;
            set => isAlive = value;
        }

        //
        // /// <summary>
        // /// 角色模块字典
        // /// </summary>
        // protected virtual Dictionary<Type, ICharacterModule> Modules
        // {
        //     get => _modules;
        //     set => _modules = value;
        // }

        public virtual void Initialize(TData configData)
        {
            if (configData != null && !IsInitialized)
            {
                unitId = configData.SaveData.UnitId;
                data = configData;
                IsInitialized = true;
                if (data?.RuntimeData is UnitRuntimeData urd)
                {
                }
            }
            else
            {
                LOGService.Error("初始化失败：传入数据为 null",Name);
            }
        }

        public virtual void OnBattleStart()
        {
        }

        public virtual void OnBattleEnd()
        {
        }

        public virtual void Cleanup()
        {
            data = default;
            unitId = "";
        }

        // public void AddModule<T>() where T : ICharacterModule, new()
        // {
        //     AddModule(typeof(T));
        // }
        //
        // public void AddModule(Type moduleType)
        // {
        //     if (moduleType == null)
        //     {
        //         throw new ArgumentNullException(nameof(moduleType), "模块类型不能为null");
        //     }
        //
        //     // 校验类型是否实现了ICharacterModule接口
        //     if (!typeof(ICharacterModule).IsAssignableFrom(moduleType))
        //     {
        //         throw new ArgumentException($"类型 {moduleType.Name} 必须实现 ICharacterModule 接口", nameof(moduleType));
        //     }
        //
        //     // 校验是否已经存在该模块
        //     if (Modules.ContainsKey(moduleType)) return;
        //
        //     // 校验是否有无参构造函数（对应原泛型的 new() 约束）
        //     var constructor = moduleType.GetConstructor(Type.EmptyTypes);
        //     if (constructor == null)
        //     {
        //         throw new InvalidOperationException($"类型 {moduleType.Name} 必须包含无参构造函数");
        //     }
        //
        //     // 实例化模块并添加到字典
        //     var module = (ICharacterModule)Activator.CreateInstance(moduleType);
        //     Modules[moduleType] = module;
        // }
        //
        // public T GetModule<T>() where T : class, ICharacterModule
        // {
        //     var moduleType = typeof(T);
        //     if (Modules.TryGetValue(moduleType, out var module))
        //     {
        //         return module as T;
        //     }
        //
        //     return null;
        // }
        //
        // public void RemoveModule<T>() where T : ICharacterModule
        // {
        //     var moduleType = typeof(T);
        //     if (!Modules.TryGetValue(moduleType, out var module)) return;
        //     module.Cleanup();
        //     Modules.Remove(moduleType);
        // }
    }
}