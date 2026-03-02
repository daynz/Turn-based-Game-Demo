using BH.Framework.Enums;
using BH.Framework.Infrastructure.Events.Attributes;
using BH.Framework.Infrastructure.Events.Base;

namespace BH.GameSystems.BattleSystem.Events.EventBattle
{
    [EventType(EventType.BattleEvent)]
    public class BattleEvent : EventBase
    {
        protected BattleEvent(object sender, EventPriority priority, bool allowMultipleHandlers, object data) :
            base(sender, priority, allowMultipleHandlers, data)
        {
        }
    }
}