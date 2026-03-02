using System;
using System.Collections.Generic;
using System.Reflection;
using BH.Framework.Enums;
using BH.Framework.Infrastructure.Events.Attributes;
using BH.Framework.Infrastructure.Events.Interfaces;
using UnityEngine;
using EventType = BH.Framework.Enums.EventType;

namespace BH.Framework.Infrastructure.Events.Base
{
    /// <summary>
    /// 事件系统基础抽象类
    /// </summary>
    [Serializable]
    public abstract class EventBase : IEventData
    {
        private static readonly Dictionary<Type, EventType> TypeToEventTypeCache = new();

        [SerializeField] private EventPriority priority;
        [SerializeField] private bool allowMultipleHandlers;
        [SerializeField] private bool isHandled;

        public Guid EventId { get; }
        public object Sender { get; }
        public DateTime Timestamp { get; }
        public object Data { get; }
        public Dictionary<string, object> Metadata { get; }

        public bool IsHandled
        {
            get => isHandled;
            set => isHandled = value;
        }

        public EventPriority Priority => priority;
        public bool AllowMultipleHandlers => allowMultipleHandlers;

        // 用于存储调用堆栈（仅在 DEBUG 模式下）
        public string CallStack { get; private set; }

        private EventType? _cachedEventType;

        public EventType EventType
        {
            get
            {
                if (_cachedEventType.HasValue)
                {
                    return _cachedEventType.Value;
                }

                var eventType = GetEventTypeFromCache(GetType());
                if (eventType == null)
                {
                    throw new InvalidOperationException(
                        $"事件类 '{GetType().Name}' 必须标记 [EventType] 特性。");
                }

                _cachedEventType = eventType.Value;
                return _cachedEventType.Value;
            }
        }


        #region 私有化构造函数

        /// <summary>
        /// 事件核心构造函数
        /// 私有化禁止外部直接实例化，仅通过 EventBuilder 调用
        /// 完成事件基础属性初始化、特性校验、堆栈记录等操作
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="priority">事件优先级</param>
        /// <param name="allowMultipleHandlers">是否允许多处理器</param>
        /// <param name="data">事件携带数据</param>
        protected EventBase(object sender, EventPriority priority = EventPriority.Normal,
            bool allowMultipleHandlers = true, object data = null)
        {
            EventId = Guid.NewGuid();
            Sender = sender;
            Timestamp = DateTime.UtcNow; // 使用 UTC 时间保证一致性
            this.priority = priority;
            this.allowMultipleHandlers = allowMultipleHandlers;
            Data = data;
            Metadata = new Dictionary<string, object>();

            // 开发模式下记录调用堆栈，便于调试
#if DEBUG
            CallStack = Environment.StackTrace;
#endif
        }

        #endregion

        #region 数据相关工具方法

        /// <summary>
        /// 从缓存获取事件类型
        /// 优先从缓存读取，缓存未命中时反射获取并加入缓存
        /// 线程安全设计，支持异步事件创建场景
        /// </summary>
        /// <param name="eventType">事件类类型</param>
        /// <returns>事件类型枚举值，无特性时返回 null</returns>
        private EventType? GetEventTypeFromCache(Type eventType)
        {
            lock (TypeToEventTypeCache)
            {
                if (TypeToEventTypeCache.TryGetValue(eventType, out var type))
                {
                    return type;
                }

                // 反射获取事件类型特性
                var attr = eventType.GetCustomAttribute<EventTypeAttribute>(inherit: true);
                if (attr == null)
                {
                    return null;
                }

                // 将获取到的事件类型加入缓存
                TypeToEventTypeCache.Add(eventType, attr.EventType);
                return attr.EventType;
            }
        }

        /// <summary>
        /// 获取强类型事件数据
        /// 尝试将事件原始数据转换为指定类型
        /// 转换失败时返回类型默认值，不抛出异常
        /// </summary>
        /// <typeparam name="T">目标数据类型</typeparam>
        /// <returns>转换后的强类型数据，失败返回默认值</returns>
        public T GetData<T>()
        {
            if (Data is T typedData)
                return typedData;
            try
            {
                return (T)Convert.ChangeType(Data, typeof(T));
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// 添加事件元数据
        /// 向元数据字典中添加或更新键值对
        /// </summary>
        /// <param name="key">元数据键</param>
        /// <param name="value">元数据值</param>
        public void AddMetadata(string key, object value)
        {
            Metadata[key] = value;
        }

        /// <summary>
        /// 获取事件元数据
        /// 根据键从元数据字典中获取对应值，无对应键时返回 null
        /// </summary>
        /// <param name="key">元数据键</param>
        /// <returns>元数据值，无对应键返回 null</returns>
        public object GetMetadata(string key)
        {
            return Metadata.GetValueOrDefault(key);
        }

        #endregion
    }
}