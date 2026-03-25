using BH.Framework.Enums;
using BH.Framework.Infrastructure.Events.Attributes;
using BH.Framework.Infrastructure.Events.Base;
using BH.Framework.Infrastructure.Events.Data;

namespace BH.Framework.Infrastructure.Resource.Data.Events
{
    [EventType(EventType.GameEvent)]
    public class ResourceLoadFailedEvent : Event<EmptyEventData>
    {
        protected ResourceLoadFailedEvent(object sender, EmptyEventData data,
            EventPriority priority = EventPriority.Normal, bool allowMultipleHandlers = true) : base(sender, data,
            priority, allowMultipleHandlers)
        {
        }
    }
}