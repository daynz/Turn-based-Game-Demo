namespace BH.GameSystems.BattleSystem.Interfaces
{
    public interface IUnitData<out TSave, out TRuntime> : IUnitId
        where TSave : IUnitSaveData
        where TRuntime : IUnitRuntimeData
    {
        string Name { get; }
        int Level { get; }
        TSave SaveData { get; }
        TRuntime RuntimeData { get; }
    }
}