using UnityEngine;

namespace BH.GameSystems.BattleSystem.Interfaces
{
    /// <summary>
    /// 战斗单位工厂接口（泛型）
    /// </summary>
    public interface IUnitFactory<out TBattleUnit, TData>
        where TBattleUnit : MonoBehaviour, IBattleUnit<TData>
        where TData : IUnitData<IUnitSaveData, IUnitRuntimeData>
    {
        /// <summary>
        /// 从数据创建战斗单位
        /// </summary>
        TBattleUnit CreateFromData(TData data, Transform parent = null);

        /// <summary>
        /// 从 ID 创建战斗单位（需配合数据库）
        /// </summary>
        TBattleUnit CreateFromId(string unitId, IUnitDatabase<TData> database, Transform parent = null);
    }
}