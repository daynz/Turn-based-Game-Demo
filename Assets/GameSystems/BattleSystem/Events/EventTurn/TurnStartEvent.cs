using BH.Framework.Enums;

namespace BH.GameSystems.BattleSystem.Events.EventTurn
{
    public class TurnStartEvent : TurnEvent
    {
        private TurnStartEvent(object sender, EventPriority priority, bool allowMultipleHandlers, object data) 
            : base(sender, priority, allowMultipleHandlers, data)
        {
        }
    }
}