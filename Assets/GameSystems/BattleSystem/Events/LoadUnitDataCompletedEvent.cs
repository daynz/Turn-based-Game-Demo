using BH.Framework.Enums;
using BH.Framework.Infrastructure.Events.Base;
using BH.Framework.Infrastructure.Events.Data;

namespace BH.GameSystems.BattleSystem.Events
{
    // Data存储完成加载的阵营
    public class LoadUnitDataCompletedEvent : Event<EmptyEventData>
    {
        protected LoadUnitDataCompletedEvent(object sender, EmptyEventData data,
            EventPriority priority = EventPriority.Normal, bool allowMultipleHandlers = true) : base(sender, data,
            priority, allowMultipleHandlers)
        {
        }
    }
}