using BH.Framework.Enums;
using BH.Framework.Infrastructure.Events.Attributes;
using BH.Framework.Infrastructure.Events.Base;

namespace BH.GameSystems.BattleSystem.Events.EventTurn
{
    [EventType(EventType.TurnEvent)]
    public class TurnEvent : Event<>
    {
        protected TurnEvent(object sender, EventPriority priority, bool allowMultipleHandlers, object data)
            : base(sender, priority, allowMultipleHandlers, data)
        {
        }
    }
}