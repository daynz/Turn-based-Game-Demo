using BH.Framework.Enums;

namespace BH.GameSystems.BattleSystem.Events.EventUnits
{
    // Data存储完成加载的阵营
    public class LoadUnitDataCompletedEvent : UnitsEvent
    {
        private LoadUnitDataCompletedEvent(object sender, EventPriority priority, bool allowMultipleHandlers,
            object data)
            : base(sender, priority, allowMultipleHandlers, data)
        {
        }
    }
}