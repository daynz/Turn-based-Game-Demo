namespace BH.GameSystems.BattleSystem.Interfaces
{
    public interface ITurnState
    {
        void Enter();
        void Exit();
        void Update();
        void HandleInput();
    }
}