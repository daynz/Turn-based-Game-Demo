using BH.Framework.Enums;

namespace BH.Framework.Interfaces
{
    public interface IPrioritized
    {
        /// <summary>
        /// 优先级值，数值越小优先级越高
        /// </summary>
        PriorityOrder Priority { get; }
    }
}