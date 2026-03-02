using BH.Framework.Enums;
using BH.Framework.Infrastructure.Events.Attributes;
using BH.Framework.Infrastructure.Events.Base;

namespace BH.Framework.Infrastructure.Events.Data
{
    [EventType(EventType.GameEvent)]
    public class GameStartEvent : EventBase
    {
        protected GameStartEvent(object sender, EventPriority priority = EventPriority.Normal,
            bool allowMultipleHandlers = true, object data = null) : base(sender, priority, allowMultipleHandlers, data)
        {
        }
    }
}