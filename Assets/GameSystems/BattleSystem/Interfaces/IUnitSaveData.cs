namespace BH.GameSystems.BattleSystem.Interfaces
{
    public interface IUnitSaveData : IUnitId
    {
        float Health { get; set; }
        float Attack { get; set; }
        float Defense { get; set; }
        float Speed { get; set; }
        string ToJson();
        void FromJson(string json);
    }
}