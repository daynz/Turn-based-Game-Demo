using BH.Framework.Infrastructure.Events.Interfaces;
using BH.Framework.Infrastructure.FSM.Base;
using BH.Framework.Infrastructure.Logging.Interfaces;

namespace BH.GameSystems.BattleSystem.Systems.TurnSystem.State
{
    public class TurnEndState : StateBase
    {
        public TurnEndState(ILogService logService, IEventService eventService)
            : base(logService, eventService)
        {
        }
    }
}