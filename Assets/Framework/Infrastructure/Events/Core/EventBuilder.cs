using System;
using System.Reflection;
using BH.Framework.Enums;
using BH.Framework.Infrastructure.Events.Base;
using BH.Framework.Infrastructure.Events.Data;
using BH.Framework.Infrastructure.Events.Interfaces;

namespace BH.Framework.Infrastructure.Events.Core
{
    /// <summary>
    /// 事件构建器
    /// 专门负责 EventBase 子类的实例创建，强类型约束保证数据安全
    /// </summary>
    /// <typeparam name="TEvent">事件子类类型（继承 EventBase）</typeparam>
    /// <typeparam name="TData">事件数据类型（实现 IEventData）</typeparam>
    public class EventBuilder<TEvent, TData>
        where TEvent : Event<TData>
        where TData : IEventData
    {
        #region 构建参数

        /// <summary>
        /// 事件发送者参数
        /// </summary>
        private object _sender;

        /// <summary>
        /// 事件优先级参数（默认 Normal）
        /// </summary>
        private EventPriority _priority = EventPriority.Normal;

        /// <summary>
        /// 多处理器允许参数（默认允许）
        /// </summary>
        private bool _allowMultipleHandlers = true;

        /// <summary>
        /// 事件数据参数（强类型）
        /// </summary>
        private TData _data;

        #endregion

        #region 链式配置方法

        /// <summary>
        /// 设置事件发送者
        /// </summary>
        /// <param name="sender">发送事件的对象实例</param>
        /// <returns>构建器实例，支持链式调用</returns>
        public EventBuilder<TEvent, TData> WithSender(object sender)
        {
            _sender = sender;
            return this;
        }

        /// <summary>
        /// 设置事件优先级
        /// </summary>
        /// <param name="priority">事件优先级枚举值</param>
        /// <returns>构建器实例，支持链式调用</returns>
        public EventBuilder<TEvent, TData> WithPriority(EventPriority priority)
        {
            _priority = priority;
            return this;
        }

        /// <summary>
        /// 设置是否允许多处理器处理当前事件
        /// </summary>
        /// <param name="allow">是否允许</param>
        /// <returns>构建器实例，支持链式调用</returns>
        public EventBuilder<TEvent, TData> AllowMultipleHandlers(bool allow)
        {
            _allowMultipleHandlers = allow;
            return this;
        }

        /// <summary>
        /// 设置事件携带的强类型数据
        /// </summary>
        /// <param name="data">事件相关业务数据</param>
        /// <returns>构建器实例，支持链式调用</returns>
        /// <exception cref="ArgumentNullException">数据为空时抛出</exception>
        public EventBuilder<TEvent, TData> WithData(TData data)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data), "事件数据不能为空");
            return this;
        }

        /// <summary>
        /// 快捷设置空事件数据（适配无业务数据的事件）
        /// </summary>
        /// <returns>构建器实例，支持链式调用</returns>
        /// <exception cref="InvalidOperationException">数据类型非 EmptyEventData 时抛出</exception>
        public EventBuilder<TEvent, TData> WithEmptyData()
        {
            if (typeof(TData) != typeof(EmptyEventData))
            {
                throw new InvalidOperationException($"当前事件数据类型为 {typeof(TData).Name}，仅 EmptyEventData 支持此方法");
            }

            _data = (TData)(object)EmptyEventData.Instance;
            return this;
        }

        #endregion

        #region 构建核心逻辑

        /// <summary>
        /// 构建事件实例
        /// 通过反射调用 EventBase 子类的 protected 构造函数创建实例
        /// 自动校验事件类型特性，缺失将抛出异常
        /// </summary>
        /// <returns>构建完成的强类型事件实例</returns>
        /// <exception cref="InvalidOperationException">事件类缺少指定构造函数时抛出</exception>
        public TEvent Build()
        {
            try
            {
                var eventTypeAttr = GlobalEventTypeCache.GetEventTypeFromCache(typeof(TEvent));
                if (eventTypeAttr == null)
                {
                    throw new InvalidOperationException($"事件类 {typeof(TEvent).Name} 未标记 [EventType] 特性，无法构建");
                }

                var constructor = typeof(TEvent).GetConstructor(
                    bindingAttr: BindingFlags.NonPublic | BindingFlags.Instance,
                    binder: null,
                    types: new[] { typeof(object), typeof(TData), typeof(EventPriority), typeof(bool) },
                    modifiers: null);

                if (constructor == null)
                {
                    throw new InvalidOperationException(
                        $"事件类 {typeof(TEvent).Name} 缺少符合规范的构造函数，需定义：\n" +
                        $"protected {typeof(TEvent).Name}(object sender, {typeof(TData).Name} data, EventPriority priority, bool allowMultipleHandlers)");
                }

                var eventInstance = (TEvent)constructor.Invoke(
                    parameters: new[] { _sender, _data, _priority, _allowMultipleHandlers });

                return eventInstance;
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException ?? ex;
            }
        }

        #endregion

        #region 静态工厂方法

        /// <summary>
        /// 创建事件构建器实例（强类型）
        /// </summary>
        /// <returns>事件构建器实例</returns>
        public static EventBuilder<TEvent, TData> Create()
        {
            return new EventBuilder<TEvent, TData>();
        }

        #endregion
    }

    /// <summary>
    /// 事件构建器静态工厂类
    /// 提供统一的构建器入口，简化泛型声明
    /// </summary>
    public static class EventBuilder
    {
        /// <summary>
        /// 创建带业务数据的事件构建器
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <typeparam name="TData">事件数据类型</typeparam>
        /// <returns>强类型事件构建器</returns>
        public static EventBuilder<TEvent, TData> Create<TEvent, TData>()
            where TEvent : Event<TData>
            where TData : IEventData
        {
            return EventBuilder<TEvent, TData>.Create();
        }

        /// <summary>
        /// 创建无业务数据的事件构建器（快捷方法）
        /// </summary>
        /// <typeparam name="TEvent">事件类型（需继承 EventBase&lt;EmptyEventData&gt;）</typeparam>
        /// <returns>事件构建器实例</returns>
        public static EventBuilder<TEvent, EmptyEventData> CreateForEmptyData<TEvent>()
            where TEvent : Event<EmptyEventData>
        {
            return EventBuilder<TEvent, EmptyEventData>.Create();
        }
    }
}