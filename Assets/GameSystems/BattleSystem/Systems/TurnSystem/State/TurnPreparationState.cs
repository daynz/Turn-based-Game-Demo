using BH.Framework.Infrastructure.DI.Attributes;
using BH.Framework.Infrastructure.Events.Core;
using BH.Framework.Infrastructure.Logging.Core;
using BH.GameSystems.BattleSystem.Enums;
using BH.GameSystems.BattleSystem.Events.EventDamageCalculator;
using BH.GameSystems.BattleSystem.Events.EventUnits;
using BH.GameSystems.BattleSystem.Manager;
using BH.GameSystems.BattleSystem.Systems.TurnSystem.Base;

namespace BH.GameSystems.BattleSystem.Systems.TurnSystem.State
{
    public class TurnPreparationState : BaseTurnState
    {
        [Inject] private LogService _logService;
        [Inject] private EventService _eventService;
        public override void Enter()
        {
            // _logService.Info($"[TurnPreparation] 回合准备");
            //
            // //var unit = TurnManager.Instance.CurrentTurn.Unit;
            //
            // _eventService.Publish(
            //     EventBuilder.Create<CalculateDotDamageEvent>()
            //         .WithSender(this)
            //         .Build()
            // );
            //
            // if (!unit.IsAlive)
            // {
            //     _eventService.Publish(
            //         EventBuilder.Create<UnitDataEvent>()
            //             .WithSender(this)
            //             .Build()
            //     );
            // }
            //
            // TurnManager.Instance.StateMachine.ChangeState(TurnPhase.ActionSelection);
        }
    }
}