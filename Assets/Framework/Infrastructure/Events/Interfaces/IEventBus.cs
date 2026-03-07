using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BH.Framework.Enums;
using BH.Framework.Infrastructure.Events.Base;
using BH.Framework.Infrastructure.Events.Core;

namespace BH.Framework.Infrastructure.Events.Interfaces
{
    /// <summary>
    /// 事件总线核心接口：仅负责通道管理、事件发布/订阅的底层实现
    /// </summary>
    public interface IEventBus : IDisposable
    {
        /// <summary>
        /// 初始化所有默认通道
        /// </summary>
        void Initialize();

        #region 通道管理

        /// <summary>
        /// 创建指定类型的事件通道
        /// </summary>
        EventChannel CreateChannel(EventType channelType, EventPriority priority = EventPriority.Normal,
            int maxQueueSize = 1000);

        /// <summary>
        /// 获取指定类型的事件通道（不存在则返回null）
        /// </summary>
        EventChannel GetChannel(EventType channelType);

        /// <summary>
        /// 获取或创建指定类型的事件通道
        /// </summary>
        EventChannel GetOrCreateChannel(EventType channelType);

        /// <summary>
        /// 获取所有已创建的通道
        /// </summary>
        IEnumerable<EventChannel> GetAllChannels();

        public int ChannelCount { get; }

        #endregion

        #region 事件发布

        /// <summary>
        /// 发布事件到指定通道
        /// </summary>
        void Publish<T>(T eventData, EventType channelType) where T : IEvent<IEventData>;

        /// <summary>
        /// 发布事件到事件自身关联的默认通道
        /// </summary>
        void Publish<T>(T eventData) where T : IEvent<IEventData>;

        #endregion

        #region 事件订阅

        /// <summary>
        /// 订阅指定通道的同步事件
        /// </summary>
        EventSubscription Subscribe<T>(EventType channelType, Action<T> handler,
            EventPriority priority = EventPriority.Normal, object owner = null, bool isOnce = false)
            where T : class, IEvent<IEventData>;

        /// <summary>
        /// 订阅指定通道的异步事件
        /// </summary>
        EventSubscription SubscribeAsync<T>(EventType channelType, Func<T, Task> handler,
            EventPriority priority = EventPriority.Normal, object owner = null, bool isOnce = false)
            where T : class, IEvent<IEventData>;

        /// <summary>
        /// 订阅事件到默认通道（根据事件类型自动匹配）
        /// </summary>
        EventSubscription Subscribe<T>(Action<T> handler, EventPriority priority = EventPriority.Normal,
            object owner = null, bool isOnce = false)
            where T : class, IEvent<IEventData>;

        /// <summary>
        /// 订阅异步事件到默认通道（根据事件类型自动匹配）
        /// </summary>
        EventSubscription SubscribeAsync<T>(Func<T, Task> handler, EventPriority priority = EventPriority.Normal,
            object owner = null, bool isOnce = false)
            where T : class, IEvent<IEventData>;

        #endregion

        #region 取消订阅

        /// <summary>
        /// 取消指定订阅（可选指定通道，null则取消所有通道中的该订阅）
        /// </summary>
        void Unsubscribe(EventSubscription subscription, EventType? channelType = null);

        /// <summary>
        /// 取消指定所有者的所有订阅（所有通道）
        /// </summary>
        void UnsubscribeAll(object owner);

        #endregion

        #region 事件处理

        /// <summary>
        /// 处理所有通道的事件队列
        /// </summary>
        /// <param name="maxEventsPerChannel">每个通道单次处理的最大事件数</param>
        void ProcessAllChannels(int maxEventsPerChannel);

        #endregion
    }
}