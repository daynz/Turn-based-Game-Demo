namespace BH.GameSystems.BattleSystem.Interfaces
{
    public interface IBattleUnit<TData> : IUnitId
        where TData : IUnitData<IUnitSaveData, IUnitRuntimeData>
    {
        TData Data { get; }
        bool IsAlive { get; }

        void Initialize(TData data);

        void Cleanup();
    }
}