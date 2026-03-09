using BH.Framework.Enums;
using BH.Framework.Infrastructure.Events.Attributes;
using BH.Framework.Infrastructure.Events.Base;

namespace BH.Framework.Infrastructure.Events.Data
{
    [EventType(EventType.GameEvent)]
    public class BattleStartEvent : Event<EmptyEventData>
    {
        protected BattleStartEvent(object sender, EmptyEventData data, EventPriority priority = EventPriority.Normal,
            bool allowMultipleHandlers = true) : base(sender, data, priority, allowMultipleHandlers)
        {
        }
    }
}