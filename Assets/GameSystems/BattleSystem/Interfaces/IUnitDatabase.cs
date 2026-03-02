namespace BH.GameSystems.BattleSystem.Interfaces
{
    /// <summary>
    /// 单位数据库接口
    /// </summary>
    public interface IUnitDatabase<TData>
        where TData : IUnitData<IUnitSaveData, IUnitRuntimeData>
    {
        /// <summary>
        /// 根据单位 ID 获取数据（未找到返回 null）
        /// </summary>
        TData GetUnit(string id);

        /// <summary>
        /// 尝试获取单位数据
        /// </summary>
        bool TryGetUnit(string id, out TData unit);
    }
}