using BH.Framework.Enums;

namespace BH.GameSystems.BattleSystem.Events.EventUnits
{
    public class UnitDataEvent : UnitsEvent
    {
        protected UnitDataEvent(object sender, EventPriority priority, bool allowMultipleHandlers, object data)
            : base(sender, priority, allowMultipleHandlers, data)
        {
        }
    }
}