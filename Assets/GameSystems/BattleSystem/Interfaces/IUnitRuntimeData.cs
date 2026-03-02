namespace BH.GameSystems.BattleSystem.Interfaces
{
    public interface IUnitRuntimeData : IUnitId
    {
        float Health { get; set; }
        float MaxHealth { get; set; }
        float Attack { get; set; }
        float Defense { get; set; }
        float Speed { get; set; }
    }
}