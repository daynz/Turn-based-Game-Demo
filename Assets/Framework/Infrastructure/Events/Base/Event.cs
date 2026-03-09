using System;
using System.Collections.Generic;
using System.Reflection;
using BH.Framework.Enums;
using BH.Framework.Infrastructure.Events.Attributes;
using BH.Framework.Infrastructure.Events.Core;
using BH.Framework.Infrastructure.Events.Interfaces;
using BH.Framework.Infrastructure.Logging.Interfaces;
using UnityEngine;
using Zenject;
using EventType = BH.Framework.Enums.EventType;

namespace BH.Framework.Infrastructure.Events.Base
{
    /// <summary>
    /// 事件系统基础抽象类
    /// </summary>
    [Serializable]
    public class Event<TData> : IEvent<TData> where TData : IEventData
    {
        [NonSerialized] private Guid _eventId;
        [NonSerialized] private object _sender;
        [SerializeField] private EventPriority priority;
        [NonSerialized] private TData _data;
        [SerializeField] private bool allowMultipleHandlers;
        [SerializeField] private bool isHandled;
        [NonSerialized] private DateTime _timestamp;
        [NonSerialized] private EventType? _cachedEventType;
        [NonSerialized] private Dictionary<string, object> _metadata;
        [NonSerialized] private string _callStack;

        [Inject] private ILogService _logService;

        public Guid EventId => _eventId;

        public object Sender => _sender;

        public DateTime Timestamp => _timestamp;

        public TData Data => _data;

        public Dictionary<string, object> Metadata => _metadata;

        public bool IsHandled
        {
            get => isHandled;
            set => isHandled = value;
        }

        public EventPriority Priority => priority;
        public bool AllowMultipleHandlers => allowMultipleHandlers;

        public string CallStack
        {
            get => _callStack;
            private set => _callStack = value;
        }

        public EventType EventType
        {
            get
            {
                if (_cachedEventType.HasValue)
                {
                    return _cachedEventType.Value;
                }

                var eventType = GlobalEventTypeCache.GetEventTypeFromCache(GetType());
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
        protected Event(object sender, TData data, EventPriority priority = EventPriority.Normal,
            bool allowMultipleHandlers = true)
        {
            _eventId = Guid.NewGuid();
            _sender = sender ?? throw new ArgumentNullException(nameof(sender), "事件发送者不可为null");
            _data = data;
            _timestamp = DateTime.UtcNow; // 使用 UTC 时间保证一致性
            this.priority = priority;
            this.allowMultipleHandlers = allowMultipleHandlers;
            _metadata = new Dictionary<string, object>();

            // 开发模式下记录调用堆栈，便于调试
#if DEBUG
            CallStack = Environment.StackTrace;
#endif
        }

        #endregion

        #region 数据相关工具方法

        /// <summary>
        /// 获取强类型事件数据
        /// 尝试将事件原始数据转换为指定类型
        /// </summary>
        /// <typeparam name="T">目标数据类型</typeparam>
        /// <returns>转换后的强类型数据，失败返回默认值</returns>
        public T GetData<T>()
        {
            if (Data is T typedData)
                return typedData;
            if (!typeof(T).IsValueType && !typeof(T).IsAssignableFrom(typeof(TData))) return default;
            try
            {
                return (T)Convert.ChangeType(Data, typeof(T));
            }
            catch
            {
                _logService.Error("类型转换失败");
            }

            return default;
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