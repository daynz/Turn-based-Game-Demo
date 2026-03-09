using BH.Framework.Enums;
using BH.Framework.Infrastructure.Events.Base;
using BH.Framework.Infrastructure.Events.Data;

namespace BH.GameSystems.BattleSystem.Events
{
    public class TurnStartEvent : Event<EmptyEventData>
    {
        protected TurnStartEvent(object sender, EmptyEventData data, EventPriority priority = EventPriority.Normal,
            bool allowMultipleHandlers = true) : base(sender, data, priority, allowMultipleHandlers)
        {
        }
    }
}