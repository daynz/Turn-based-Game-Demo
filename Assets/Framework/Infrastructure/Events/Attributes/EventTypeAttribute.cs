using System;
using BH.Framework.Enums;

namespace BH.Framework.Infrastructure.Events.Attributes
{
    /// <summary>
    /// 用于标记事件数据类对应的 EventType 枚举值
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class EventTypeAttribute : Attribute
    {
        /// <summary>
        /// 特性存储的枚举值
        /// </summary>
        public EventType EventType { get; }

        /// <summary>
        /// 特性构造函数
        /// </summary>
        public EventTypeAttribute(EventType eventType)
        {
            EventType = eventType;
        }
    }
}