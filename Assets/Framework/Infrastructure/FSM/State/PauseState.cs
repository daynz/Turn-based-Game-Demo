using BH.Framework.Infrastructure.Events.Interfaces;
using BH.Framework.Infrastructure.FSM.Base;
using BH.Framework.Infrastructure.Logging.Interfaces;

namespace BH.Framework.Infrastructure.FSM.State
{
    public class PausedState : StateBase
    {
        public PausedState(ILogService logService, IEventService eventService) : base(logService, eventService)
        {
        }
    }
}