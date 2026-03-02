using System;
using System.Reflection;
using BH.Framework.Enums;
using BH.Framework.Infrastructure.Events.Base;

namespace BH.Framework.Infrastructure.Events.Core
{
    /// <summary>
    /// 事件构建器
    /// 专门负责 EventBase 子类的实例创建
    /// </summary>
    /// <typeparam name="T">事件子类类型</typeparam>
    public class EventBuilder<T> where T : EventBase
    {
        /// <summary>
        /// 事件发送者参数
        /// </summary>
        private object _sender;

        /// <summary>
        /// 事件优先级参数
        /// 默认值为 EventPriority.Normal
        /// </summary>
        private EventPriority _priority = EventPriority.Normal;

        /// <summary>
        /// 多处理器允许参数
        /// 默认值为 true
        /// </summary>
        private bool _allowMultipleHandlers = true;

        /// <summary>
        /// 事件数据参数
        /// </summary>
        private object _data;

        /// <summary>
        /// 设置事件发送者
        /// </summary>
        /// <param name="sender">发送事件的对象实例</param>
        /// <returns>构建器实例，支持链式调用</returns>
        public EventBuilder<T> WithSender(object sender)
        {
            _sender = sender;
            return this;
        }

        /// <summary>
        /// 设置事件优先级
        /// </summary>
        /// <param name="priority">事件优先级枚举值</param>
        /// <returns>构建器实例，支持链式调用</returns>
        public EventBuilder<T> WithPriority(EventPriority priority)
        {
            _priority = priority;
            return this;
        }

        /// <summary>
        /// 设置多处理器允许状态
        /// </summary>
        /// <param name="allow">是否允许多个处理器处理</param>
        /// <returns>构建器实例，支持链式调用</returns>
        public EventBuilder<T> AllowMultipleHandlers(bool allow)
        {
            _allowMultipleHandlers = allow;
            return this;
        }

        /// <summary>
        /// 设置事件携带数据
        /// </summary>
        /// <param name="data">事件相关业务数据</param>
        /// <returns>构建器实例，支持链式调用</returns>
        public EventBuilder<T> WithData(object data)
        {
            _data = data;
            return this;
        }

        /// <summary>
        /// 构建事件实例
        /// 通过反射调用事件私有构造函数创建实例
        /// 自动校验事件类型特性，缺失将抛出异常
        /// </summary>
        /// <returns>构建完成的事件实例</returns>
        /// <exception cref="InvalidOperationException">事件类缺少指定构造函数时抛出</exception>
        public T Build()
        {
            try
            {
                // 获取事件私有构造函数
                var constructor = typeof(T).GetConstructor(
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    new[] { typeof(object), typeof(EventPriority), typeof(bool), typeof(object) },
                    null);

                if (constructor == null)
                {
                    throw new InvalidOperationException(
                        $"事件类 {typeof(T).Name} 缺少私有构造函数，需包含参数 object sender, EventPriority priority, bool allowMultipleHandlers, object data");
                }

                // 调用构造函数创建实例
                var instance = (T)constructor.Invoke(
                    new[] { _sender, _priority, _allowMultipleHandlers, _data });

                return instance;
            }
            catch (TargetInvocationException ex)
            {
                // 解包内部异常，暴露真实错误原因
                throw ex.InnerException ?? ex;
            }
        }

        /// <summary>
        /// 创建事件构建器实例
        /// 静态工厂方法，简化构建器创建
        /// </summary>
        /// <returns>事件构建器实例</returns>
        public static EventBuilder<T> Create()
        {
            return new EventBuilder<T>();
        }
    }

    /// <summary>
    /// 事件构建器静态工厂类
    /// 提供统一的构建器入口，无需手动指定泛型
    /// </summary>
    public static class EventBuilder
    {
        /// <summary>
        /// 创建指定事件类型的构建器
        /// </summary>
        /// <typeparam name="T">事件子类类型</typeparam>
        /// <returns>事件构建器实例</returns>
        public static EventBuilder<T> Create<T>() where T : EventBase
        {
            return EventBuilder<T>.Create();
        }
    }
}