using System;
using BH.Framework.Infrastructure.DI.Attributes;
using BH.Framework.Infrastructure.Logging.Core;
using BH.GameSystems.BattleSystem.Interfaces;
using Newtonsoft.Json;
using UnityEngine;

namespace BH.GameSystems.BattleSystem.Systems.UnitSystem.Base
{
    /// <summary>
    /// 单位数据基类
    /// </summary>
    /// <typeparam name="TRuntime">专属运行时数据类型（需实现IUnitRuntimeData）</typeparam>
    /// <typeparam name="TSave">专属存档数据类型（需实现IUnitSaveData）</typeparam>
    [Serializable]
    public class UnitData<TSave, TRuntime> : IUnitData<TSave, TRuntime>
        where TSave : IUnitSaveData
        where TRuntime : IUnitRuntimeData
    {
        public string ClassName => GetType().Name;

        [Header("单位数据")] [SerializeField] [Tooltip("id")] [JsonProperty]
        protected string unitId;

        [SerializeField] [Tooltip("名称")] [JsonProperty]
        protected string unitName = "Unknown";

        [SerializeField] [Min(1)] [Tooltip("等级")]
        protected int level = 1;

        [SerializeField] [JsonProperty] protected TSave saveData;
        [SerializeField] [JsonProperty] protected TRuntime runtimeData;

        [SerializeField] private GameObject prefab;
        [Inject] protected LogService LOGService = null;

        public string UnitId
        {
            get => unitId;
            set => unitId = value;
        }

        /// <summary>
        /// 单位名称
        /// </summary>
        public string Name
        {
            get => unitName;
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                    unitName = value;
                else
                    LOGService.Warning("单位名称不能为空或仅包含空格！", ClassName);
            }
        }

        /// <summary>
        /// 等级
        /// </summary>
        public virtual int Level
        {
            get => level;
            set
            {
                if (value >= 1)
                    level = value;
                else
                    LOGService.Warning($"等级必须大于0，当前值：{value}", ClassName);
            }
        }

        public TSave SaveData => saveData;

        public TRuntime RuntimeData => runtimeData;

        public GameObject Prefab
        {
            get => prefab;
            set => prefab = value;
        }
    }
}