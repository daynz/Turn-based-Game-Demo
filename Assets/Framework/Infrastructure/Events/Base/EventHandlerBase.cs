using System;
using System.Collections.Generic;
using BH.Framework.Enums;
using BH.Framework.Infrastructure.Events.Core;
using BH.Framework.Infrastructure.Events.Interfaces;
using EventType = BH.Framework.Enums.EventType;

namespace BH.Framework.Infrastructure.Events.Base
{
    /// <summary>
    /// 事件处理器的基类。提供便捷的订阅方法和自动注销功能。
    /// </summary>
    public abstract class EventHandlerBase
    {
        private readonly HashSet<EventSubscription> _activeSubscriptions = new();
        private bool _disposedValue;
        protected virtual EventPriority EventPriority => EventPriority.Normal;

        private EventService EventService { get; }

        public EventHandlerBase(EventService eventService)
        {
            EventService = eventService;
        }

        /// <summary>
        /// 在子类的构造函数或初始化方法中，调用此方法来订阅事件。
        /// </summary>
        public void Subscribe<T>(
            Action<T> handler,
            EventPriority priority = EventPriority.Normal,
            bool isOnce = false) where T : IEventData
        {
            if (_disposedValue)
            {
                throw new ObjectDisposedException(GetType().Name, "无法订阅已释放的事件处理程序。");
            }

            if (EventService == null)
            {
                // 如果依赖注入失败，抛出异常比默默返回 null 更好
                throw new InvalidOperationException("EventService 尚未注入到此处理器中。");
            }

            priority = EventPriority == priority ? priority : EventPriority;
            var subscription = EventService.Subscribe(handler, priority, this, isOnce);
            if (subscription != null)
            {
                _activeSubscriptions.Add(subscription);
            }
        }

        /// <summary>
        /// 在子类的构造函数或初始化方法中，调用此方法来订阅事件到特定通道。
        /// </summary>
        protected void SubscribeToChannel<T>(
            EventType channelType,
            Action<T> handler,
            EventPriority priority = EventPriority.Normal,
            bool isOnce = false) where T : IEventData
        {
            if (_disposedValue)
            {
                throw new ObjectDisposedException(GetType().Name, "无法订阅已释放的事件处理程序。");
            }

            if (EventService == null)
            {
                throw new InvalidOperationException("EventService 尚未注入到此处理器中。");
            }

            priority = EventPriority == priority ? priority : EventPriority;
            var subscription = EventService.SubscribeToChannel(channelType, handler, priority, this, isOnce);
            if (subscription != null)
            {
                _activeSubscriptions.Add(subscription);
            }
        }

        /// <summary>
        /// 取消所有由本处理器创建的订阅。
        /// </summary>
        protected virtual void UnregisterAllSubscriptions()
        {
            // 让事件服务负责根据 owner (this) 来移除所有订阅
            EventService?.UnsubscribeAll(this);
            _activeSubscriptions.Clear();
        }

        // 提供一个公共方法供外部调用来清理资源
        public void Dispose()
        {
            Dispose(disposing: true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue) return;
            if (disposing)
            {
                // 释放托管状态 (managed objects)
                UnregisterAllSubscriptions();
            }

            // 释放未托管资源 (unmanaged objects) 并重写终结器
            // 将大型字段设置为 null
            _disposedValue = true;
        }
    }
}