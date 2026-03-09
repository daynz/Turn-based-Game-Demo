using System;
using BH.Framework.Enums;

namespace BH.Framework.Infrastructure.Events.Interfaces
{
    /// <summary>
    /// 事件数据接口
    /// </summary>
    public interface IEvent<out TData> where TData : IEventData
    {
        /// <summary>
        /// 事件唯一标识
        /// </summary>
        Guid EventId { get; }
        
        /// <summary>
        /// 事件类型
        /// </summary>
        EventType EventType { get; }
        
        /// <summary>
        /// 事件发送者
        /// </summary>
        object Sender { get; }
        
        /// <summary>
        /// 事件发生时间
        /// </summary>
        DateTime Timestamp { get; }
        
        /// <summary>
        /// 事件是否已处理
        /// </summary>
        bool IsHandled { get; set; }
        
        /// <summary>
        /// 事件优先级
        /// </summary>
        EventPriority Priority { get; }
        
        /// <summary>
        /// 是否允许重复处理
        /// </summary>
        bool AllowMultipleHandlers { get; }
        
        /// <summary>
        /// 事件数据
        /// </summary>
        TData Data { get; }
        
        /// <summary>
        /// 获取泛型数据
        /// </summary>
        T GetData<T>();
    }
}