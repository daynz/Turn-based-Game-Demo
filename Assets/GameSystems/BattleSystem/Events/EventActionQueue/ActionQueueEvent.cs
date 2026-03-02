using BH.Framework.Enums;
using BH.Framework.Infrastructure.Events.Attributes;
using BH.Framework.Infrastructure.Events.Base;

namespace BH.GameSystems.BattleSystem.Events.EventActionQueue
{
    [EventType(EventType.ActionQueueEvent)]
    public class ActionQueueEvent : EventBase
    {
        protected ActionQueueEvent(object sender, EventPriority priority, bool allowMultipleHandlers, object data)
            : base(sender, priority, allowMultipleHandlers, data)
        {
        }
    }
}