using BH.Framework.Enums;

namespace BH.GameSystems.BattleSystem.Events.EventDamageCalculator
{
    public class CalculateDotDamageEvent : DamageCalculatorEvent
    {
        private CalculateDotDamageEvent(object sender, EventPriority priority, bool allowMultipleHandlers, object data)
            : base(sender, priority, allowMultipleHandlers, data)
        {
        }
    }
}