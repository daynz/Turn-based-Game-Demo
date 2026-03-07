using BH.Framework.Enums;
using BH.Framework.Infrastructure.Events.Attributes;
using BH.Framework.Infrastructure.Events.Base;

namespace BH.GameSystems.BattleSystem.Events.EventUnits
{
    [EventType(EventType.UnitsEvent)]
    public class UnitsEvent : Event<>
    {
        protected UnitsEvent(object sender, EventPriority priority, bool allowMultipleHandlers, object data)
            : base(sender, priority, allowMultipleHandlers, data)
        {
        }
    }
}