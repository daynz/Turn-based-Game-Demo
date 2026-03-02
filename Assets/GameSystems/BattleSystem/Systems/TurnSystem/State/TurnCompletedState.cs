using BH.GameSystems.BattleSystem.Manager;
using BH.GameSystems.BattleSystem.Systems.TurnSystem.Base;

namespace BH.GameSystems.BattleSystem.Systems.TurnSystem.State
{
    public class TurnCompletedState : BaseTurnState
    {
        public override void Enter()
        {
            //TurnManager.Instance.CurrentTurn.IsCompleted = true;
            //TODO: 回合完成 EventManager.Instance.Publish();
        }
    }
}