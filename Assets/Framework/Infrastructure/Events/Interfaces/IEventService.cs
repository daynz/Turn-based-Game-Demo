using System;
using System.Threading.Tasks;
using BH.Framework.Enums;
using BH.Framework.Infrastructure.Events.Core;
using Zenject;

namespace BH.Framework.Infrastructure.Events.Interfaces
{
    /// <summary>
    /// 事件服务接口：业务层入口，封装事件系统生命周期+全局策略，底层委托给IEventBus
    /// </summary>
    public interface IEventService : IInitializable, IDisposable
    {
        /// <summary>
        /// 事件系统是否启用
        /// </summary>
        bool IsEnable { get; }

        #region 生命周期管理

        /// <summary>
        /// 初始化事件系统（含EventBus初始化）
        /// </summary>
        void Initialize();

        /// <summary>
        /// 启用事件系统（所有通道启用）
        /// </summary>
        void Enable();

        /// <summary>
        /// 禁用事件系统（所有通道禁用）
        /// </summary>
        void Disable();

        #endregion

        #region 业务层事件操作（封装IEventBus）

        /// <summary>
        /// 发布事件到默认通道
        /// </summary>
        void Publish<T>(T eventData) where T : class, IEvent<IEventData>;

        /// <summary>
        /// 发布事件到指定通道
        /// </summary>
        void Publish<T>(T eventData, EventType channelType) where T : class, IEvent<IEventData>;

        /// <summary>
        /// 订阅默认通道的同步事件
        /// </summary>
        EventSubscription Subscribe<T>(Action<T> handler, EventPriority priority = EventPriority.Normal,
            object owner = null, bool isOnce = false)
            where T : class, IEvent<IEventData>;

        /// <summary>
        /// 订阅默认通道的异步事件
        /// </summary>
        EventSubscription SubscribeAsync<T>(Func<T, Task> handler, EventPriority priority = EventPriority.Normal,
            object owner = null, bool isOnce = false)
            where T : class, IEvent<IEventData>;

        /// <summary>
        /// 订阅指定通道的同步事件
        /// </summary>
        EventSubscription SubscribeToChannel<T>(EventType channelType, Action<T> handler,
            EventPriority priority = EventPriority.Normal, object owner = null, bool isOnce = false)
            where T : class, IEvent<IEventData>;

        /// <summary>
        /// 取消订阅
        /// </summary>
        void Unsubscribe(EventSubscription subscription, EventType? channelType = null);

        /// <summary>
        /// 取消指定所有者的所有订阅
        /// </summary>
        void UnsubscribeAll(object owner);

        #endregion

        #region 全局事件处理策略

        /// <summary>
        /// 处理所有通道事件（封装全局策略，如每帧最大处理数）
        /// </summary>
        /// <param name="maxEventsPerFrame">每帧处理的事件总数上限（而非每个通道）</param>
        void ProcessChannels(int maxEventsPerFrame);

        #endregion

        #region 通道管理（只读封装）

        /// <summary>
        /// 获取指定通道（业务层仅只读，创建/修改由底层IEventBus管理）
        /// </summary>
        EventChannel GetChannel(EventType channelType);

        #endregion
    }
}