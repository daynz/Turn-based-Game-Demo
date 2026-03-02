using BH.Framework.Enums;
using BH.Framework.Infrastructure.Events.Attributes;
using BH.Framework.Infrastructure.Events.Base;

namespace BH.GameSystems.BattleSystem.Events.EventDamageCalculator
{
    [EventType(EventType.ActionQueueEvent)]
    public class DamageCalculatorEvent : EventBase
    {
        protected DamageCalculatorEvent(object sender, EventPriority priority, bool allowMultipleHandlers, object data)
            : base(sender, priority, allowMultipleHandlers, data)
        {
        }
    }
}