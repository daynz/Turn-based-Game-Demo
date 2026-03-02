using System;
using System.Threading.Tasks;
using BH.Framework.Enums;
using BH.Framework.Infrastructure.Events.Core;

namespace BH.Framework.Infrastructure.Events.Interfaces
{
    /// <summary>
    /// 事件系统的核心操作接口
    /// </summary>
    public interface IEventService
    {
        /// <summary>
        /// 事件系统是否已完成初始化。
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// 启用事件系统。
        /// </summary>
        void Enable();

        /// <summary>
        /// 禁用事件系统。
        /// </summary>
        void Disable();

        /// <summary>
        /// 关闭并清理事件系统的所有资源。
        /// </summary>
        void Shutdown();

        /// <summary>
        /// 发布一个事件到其默认关联的通道。
        /// </summary>
        /// <typeparam name="T">事件数据的类型。</typeparam>
        /// <param name="eventData">要发布的事件数据实例。</param>
        void Publish<T>(T eventData) where T : IEventData;

        /// <summary>
        /// 发布一个事件到指定的通道。
        /// </summary>
        /// <typeparam name="T">事件数据的类型。</typeparam>
        /// <param name="eventData">要发布的事件数据实例。</param>
        /// <param name="channelType">目标事件通道的类型。</param>
        void Publish<T>(T eventData, EventType channelType) where T : IEventData;

        /// <summary>
        /// 订阅一个事件（同步处理器）到其默认关联的通道。
        /// </summary>
        /// <typeparam name="T">事件数据的类型。</typeparam>
        /// <param name="handler">处理事件的同步方法。</param>
        /// <param name="priority">处理器的优先级。</param>
        /// <param name="owner">订阅者对象，可用于批量取消订阅。</param>
        /// <param name="isOnce">是否为一次性订阅。</param>
        /// <returns>订阅的句柄，可用于取消订阅。</returns>
        EventSubscription Subscribe<T>(
            Action<T> handler,
            EventPriority priority = EventPriority.Normal,
            object owner = null,
            bool isOnce = false) where T : IEventData;

        /// <summary>
        /// 订阅一个事件（异步处理器）到其默认关联的通道。
        /// </summary>
        /// <typeparam name="T">事件数据的类型。</typeparam>
        /// <param name="handler">处理事件的异步方法。</param>
        /// <param name="priority">处理器的优先级。</param>
        /// <param name="owner">订阅者对象，可用于批量取消订阅。</param>
        /// <param name="isOnce">是否为一次性订阅。</param>
        /// <returns>订阅的句柄，可用于取消订阅。</returns>
        EventSubscription SubscribeAsync<T>(
            Func<T, Task> handler,
            EventPriority priority = EventPriority.Normal,
            object owner = null,
            bool isOnce = false) where T : IEventData;

        /// <summary>
        /// 订阅一个事件（同步处理器）到指定的通道。
        /// </summary>
        /// <typeparam name="T">事件数据的类型。</typeparam>
        /// <param name="channelType">目标事件通道的类型。</param>
        /// <param name="handler">处理事件的同步方法。</param>
        /// <param name="priority">处理器的优先级。</param>
        /// <param name="owner">订阅者对象，可用于批量取消订阅。</param>
        /// <param name="isOnce">是否为一次性订阅。</param>
        /// <returns>订阅的句柄，可用于取消订阅。</returns>
        EventSubscription SubscribeToChannel<T>(
            EventType channelType,
            Action<T> handler,
            EventPriority priority = EventPriority.Normal,
            object owner = null,
            bool isOnce = false) where T : IEventData;

        /// <summary>
        /// 取消指定的订阅。
        /// </summary>
        /// <param name="subscription">要取消的订阅句柄。</param>
        /// <param name="channelType">可选，指定在哪个通道中取消订阅。如果为 null，则在所有通道中取消。</param>
        void Unsubscribe(EventSubscription subscription, EventType? channelType = null);

        /// <summary>
        /// 取消指定所有者的所有订阅。
        /// </summary>
        /// <param name="owner">订阅者对象。</param>
        void UnsubscribeAll(object owner);

        /// <summary>
        /// 获取指定类型的事件通道。
        /// </summary>
        /// <param name="channelType">事件通道的类型。</param>
        /// <returns>对应的事件通道实例，如果不存在则返回 null。</returns>
        EventChannel GetChannel(EventType channelType);

        /// <summary>
        /// 创建一个指定类型的事件通道。
        /// </summary>
        /// <param name="channelType">事件通道的类型。</param>
        /// <param name="priority">通道的默认优先级。</param>
        /// <param name="maxQueueSize">通道队列的最大容量。</param>
        /// <returns>新创建的事件通道实例。</returns>
        EventChannel CreateChannel(EventType channelType, EventPriority priority = EventPriority.Normal,
            int maxQueueSize = 1000);

        /// <summary>
        /// 处理所有通道中的待处理事件。
        /// </summary>
        /// <param name="maxEventsPerFrame">每帧最多处理的事件总数。</param>
        void ProcessChannels(int maxEventsPerFrame);
    }
}