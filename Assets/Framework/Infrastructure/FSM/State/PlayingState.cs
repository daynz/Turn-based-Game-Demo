using BH.Framework.Infrastructure.Events.Interfaces;
using BH.Framework.Infrastructure.FSM.Base;
using BH.Framework.Infrastructure.FSM.Interfaces;
using BH.Framework.Infrastructure.Logging.Interfaces;
using UnityEngine;

namespace BH.Framework.Infrastructure.FSM.State
{
    public class PlayingState : StateBase
    {
        public PlayingState(ILogService logService, IEventService eventService) : base(logService, eventService)
        {
        }
        
    }
}