using BH.Framework.Enums;
using BH.Framework.Infrastructure.Events.Base;

namespace BH.Framework.Infrastructure.Events.Data
{
    public class GameStartData : Event<EmptyEventData>
    {
        protected GameStartData(object sender, EmptyEventData data, EventPriority priority = EventPriority.Normal,
            bool allowMultipleHandlers = true) : base(sender, data, priority, allowMultipleHandlers)
        {
        }
    }
}