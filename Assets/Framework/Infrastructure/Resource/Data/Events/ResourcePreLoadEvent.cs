using BH.Framework.Enums;
using BH.Framework.Infrastructure.Events.Attributes;
using BH.Framework.Infrastructure.Events.Base;
using BH.Framework.Infrastructure.Events.Data;
using JetBrains.Annotations;

namespace BH.Framework.Infrastructure.Resource.Data.Events
{
    [UsedImplicitly]
    [EventType(EventType.GameEvent)]
    public class ResourcePreLoadEvent : Event<EmptyEventData>
    {
        protected ResourcePreLoadEvent(object sender, EmptyEventData data,
            EventPriority priority = EventPriority.Normal, bool allowMultipleHandlers = true) : base(sender, data,
            priority, allowMultipleHandlers)
        {
        }
    }
}