using BH.Framework.Infrastructure.Events.Interfaces;
using BH.Framework.Infrastructure.FSM.Base;
using BH.Framework.Infrastructure.Logging.Interfaces;

namespace BH.GameSystems.BattleSystem.Systems.TurnSystem.State
{
    public class TurnCompletedState : StateBase
    {
        public TurnCompletedState(ILogService logService, IEventService eventService) : base(logService, eventService)
        {
        }

        public override void Enter()
        {
            //TurnManager.Instance.CurrentTurn.IsCompleted = true;
            //TODO: 回合完成 EventManager.Instance.Publish();
        }
    }
}