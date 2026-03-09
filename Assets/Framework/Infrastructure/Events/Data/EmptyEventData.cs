using BH.Framework.Infrastructure.Events.Interfaces;

namespace BH.Framework.Infrastructure.Events.Data
{
    /// <summary>
    /// 空事件数据载体：专用于无需传输业务数据的事件
    /// </summary>
    public class EmptyEventData : IEventData
    {
        /// <summary>
        /// 单例实例（避免重复创建空对象）
        /// </summary>
        public static readonly EmptyEventData Instance = new EmptyEventData();

        /// <summary>
        /// 私有化构造函数，强制使用单例
        /// </summary>
        private EmptyEventData()
        {
        }
    }
}