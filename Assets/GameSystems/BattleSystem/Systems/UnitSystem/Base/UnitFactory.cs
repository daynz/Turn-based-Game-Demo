using BH.Framework.Infrastructure.DI.Attributes;
using BH.Framework.Infrastructure.Logging.Core;
using BH.GameSystems.BattleSystem.Interfaces;
using UnityEngine;

namespace BH.GameSystems.BattleSystem.Systems.UnitSystem.Base
{
    /// <summary>
    /// 战斗单位工厂
    /// </summary>
    public abstract class UnitFactory<TBattleUnit, TData>
        where TBattleUnit : BattleUnit<TData>
        where TData : IUnitData<IUnitSaveData, IUnitRuntimeData>
    {
        public string Name => GetType().Name;
        private readonly GameObject _prefab;
        [Inject] private LogService _logService;

        protected UnitFactory(GameObject prefab)
        {
            if (!prefab)
                throw new System.ArgumentNullException(nameof(prefab));

            _prefab = prefab;
        }

        public virtual TBattleUnit CreateFromData(TData data, Transform parent = null)
        {
            if (data == null)
            {
                _logService.Error("[UnitFactory] 传入的单位数据为 null", Name);
                return null;
            }

            var instance = _prefab.GetComponent<TBattleUnit>();
            if (!instance)
            {
                instance = _prefab.AddComponent<TBattleUnit>();
            }

            instance.Initialize(data);
            Object.Instantiate(_prefab, parent);

            if (instance)
                return instance;

            _logService.Error($"预制体 {_prefab.name} 未挂载 {typeof(TBattleUnit).Name} 组件", Name);
            Object.Destroy(instance.gameObject);
            return null;
        }

        public virtual TBattleUnit CreateFromId(string unitId, IUnitDatabase<TData> database, Transform parent = null)
        {
            if (string.IsNullOrEmpty(unitId))
            {
                Debug.LogError("UnitFactory: 单位 ID 为空");
                return null;
            }

            if (database == null)
            {
                Debug.LogError("UnitFactory: 数据库为 null");
                return null;
            }

            if (!database.TryGetUnit(unitId, out var data))
            {
                Debug.LogError($"UnitFactory: 未在数据库中找到单位 ID: {unitId}");
                return null;
            }

            return CreateFromData(data, parent);
        }
    }
}