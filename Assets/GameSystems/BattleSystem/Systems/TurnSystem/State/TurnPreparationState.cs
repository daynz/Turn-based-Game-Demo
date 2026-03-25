using BH.Framework.Infrastructure.Events.Interfaces;
using BH.Framework.Infrastructure.FSM.Base;
using BH.Framework.Infrastructure.Logging.Interfaces;

namespace BH.GameSystems.BattleSystem.Systems.TurnSystem.State
{
    public class TurnPreparationState : StateBase
    {
        public TurnPreparationState(ILogService logService, IEventService eventService) 
            : base(logService, eventService)
        {
        }

        public override void Enter()
        {
            LogService.Info($"回合准备");

            // EventService.Publish(
            //     EventBuilder.Create<CalculateDotDamageEvent>()
            //         .WithSender(this)
            //         .Build()
            // );

            // if (!unit.IsAlive)
            // {
            //     EventService.Publish(
            //         EventBuilder.Create<UnitDataEvent>()
            //             .WithSender(this)
            //             .Build()
            //     );
            // }

            //TurnManager.Instance.StateMachine.ChangeState(TurnPhase.ActionSelection);
        }
    }
}