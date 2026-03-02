using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BH.Framework.Infrastructure.DI.Attributes;
using BH.Framework.Infrastructure.Logging.Core;
using BH.GameSystems.BattleSystem.Interfaces;
using UnityEngine;

namespace BH.GameSystems.BattleSystem.Systems.UnitSystem.Base
{
    /// <summary>
    /// 泛型单位数据库
    /// </summary>
    /// <typeparam name="TData">必须实现 IUnitData&lt;IUnitSaveData, IUnitRuntimeData&gt;</typeparam>
    [Serializable]
    public class UnitDatabase<TData> : IUnitDatabase<TData>
        where TData : IUnitData<IUnitSaveData, IUnitRuntimeData>
    {
        public string Name => GetType().Name;
        [SerializeField] protected List<TData> units = new();

        protected Dictionary<string, TData> Cache;

        [Inject] protected LogService LOGService = null;

        private void BuildCache()
        {
            if (Cache != null) return;

            Cache = new Dictionary<string, TData>();
            foreach (var unit in units)
            {
                if (unit.UnitId == null)
                {
                    LOGService.Warning($"UnitDatabase<{typeof(TData).Name}> 包含无效单位（UnitId 为空）", Name);
                    continue;
                }

                var id = unit.UnitId;
                if (!Cache.TryAdd(id, unit))
                {
                    LOGService.Error($"UnitDatabase<{typeof(TData).Name}> 存在重复 UnitId: {id}", Name);
                }
            }
        }

        public virtual void LoadUnitDatabaseFromJson(string relativePath)
        {
            BuildCache();
        }

        /// <summary>
        /// 根据 ID 获取单位数据
        /// </summary>
        public TData GetUnit(string id)
        {
            return string.IsNullOrEmpty(id) ? default : Cache.GetValueOrDefault(id);
        }

        /// <summary>
        /// 尝试获取单位数据
        /// </summary>
        public bool TryGetUnit(string id, out TData unit)
        {
            unit = GetUnit(id);
            return unit != null && !string.IsNullOrEmpty(unit.SaveData?.UnitId);
        }

#if UNITY_EDITOR
        public void AddUnit(TData unit)
        {
            if (unit == null || string.IsNullOrEmpty(unit.SaveData?.UnitId))
            {
                LOGService.Error("无法添加无效单位到数据库", Name);
                return;
            }

            if (units.Any(u => u.SaveData.UnitId == unit.SaveData.UnitId))
            {
                LOGService.Warning($"单位 {unit.SaveData.UnitId} 已存在，跳过添加", Name);
                return;
            }

            units.Add(unit);
            Cache = null; // 重置缓存
        }

        public void RemoveUnit(string id)
        {
            units.RemoveAll(u => u.SaveData.UnitId == id);
            Cache = null;
        }
#endif
    }
}