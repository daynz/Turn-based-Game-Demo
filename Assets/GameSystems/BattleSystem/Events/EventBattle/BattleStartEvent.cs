using BH.Framework.Enums;

namespace BH.GameSystems.BattleSystem.Events.EventBattle
{
    public class BattleStartEvent : BattleEvent
    {
        private BattleStartEvent(object sender, EventPriority priority, bool allowMultipleHandlers, object data) :
            base(sender, priority, allowMultipleHandlers, data)
        {
        }
    }
}