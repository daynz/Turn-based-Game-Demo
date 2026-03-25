using BH.Framework.Enums;
using BH.Framework.Infrastructure.Events.Attributes;
using BH.Framework.Infrastructure.Events.Base;
using BH.Framework.Infrastructure.Events.Data;

namespace BH.GameSystems.BattleSystem.Events
{
    [EventType(EventType.BattleEvent)]
    public class BattleLoadResourceCompletedEvent : Event<EmptyEventData>
    {
        protected BattleLoadResourceCompletedEvent(object sender, EmptyEventData data,
            EventPriority priority = EventPriority.Normal, bool allowMultipleHandlers = true) : base(sender, data,
            priority, allowMultipleHandlers)
        {
        }
    }
}