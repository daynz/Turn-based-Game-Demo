using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using BH.Framework.Enums;
using BH.Framework.Infrastructure.Events.Interfaces;
using BH.Framework.Infrastructure.Logging.Interfaces;
using Zenject;
using EventType = BH.Framework.Enums.EventType;

namespace BH.Framework.Infrastructure.Events.Core
{
    [Serializable]
    public class EventBus : IEventBus
    {
        private readonly ConcurrentDictionary<EventType, EventChannel> _channels = new();
        private const EventType DefaultChannel = EventType.SystemEvent;

        [Inject] private ILogService LogService { get; set; }
        [Inject] private EventChannel.Factory _channelFactory;
        public bool IsInitialized { get; private set; }

        public void Initialize()
        {
            if (IsInitialized) return;
            // 初始化所有枚举类型的通道
            foreach (EventType type in Enum.GetValues(typeof(EventType)))
            {
                CreateChannel(type);
            }

            IsInitialized = true;
            LogService.EventLog("[EventBus] 初始化完成");
        }

        #region 通道管理

        public EventChannel CreateChannel(EventType channelType, EventPriority priority = EventPriority.Normal,
            int maxQueueSize = 1000)
        {
            if (_channels.TryGetValue(channelType, out var ch))
            {
                LogService.EventLog($"[EventBus] 通道已存在: {channelType}");
                return ch;
            }
            
            var channel = _channelFactory.Create(channelType);
            _channels[channelType] = channel;
            LogService.EventLog($"[EventBus] 创建通道: {channelType}");
            return channel;
        }

        public EventChannel GetChannel(EventType channelType)
        {
            _channels.TryGetValue(channelType, out var channel);
            return channel;
        }

        public EventChannel GetOrCreateChannel(EventType channelType)
        {
            return GetChannel(channelType) ?? CreateChannel(channelType);
        }

        public IEnumerable<EventChannel> GetAllChannels() => _channels.Values;
        public int ChannelCount => _channels.Values.Count;

        #endregion

        #region 事件发布

        public void Publish<T>(T eventData, EventType channelType) where T : IEvent<IEventData>
        {
            if (eventData == null) throw new ArgumentNullException(nameof(eventData));
            var channel = GetChannel(channelType);
            if (channel == null)
            {
                LogService.EventLog($"[EventBus] 发布失败：通道不存在 {channelType}");
                return;
            }

            if (!channel.IsEnabled)
            {
                LogService.EventLog($"[EventBus] 发布失败：通道已禁用 {channelType}");
                return;
            }

            channel.Enqueue(eventData);
            LogService.EventLog($"[EventBus] 发布事件 {typeof(T).Name} 到通道 {channelType}");
        }

        public void Publish<T>(T eventData) where T : IEvent<IEventData>
        {
            if (eventData == null) throw new ArgumentNullException(nameof(eventData));
            var targetChannel = eventData.EventType == EventType.None ? DefaultChannel : eventData.EventType;
            Publish(eventData, targetChannel);
        }

        #endregion

        #region 事件订阅

        public EventSubscription Subscribe<T>(EventType channelType, Action<T> handler,
            EventPriority priority = EventPriority.Normal, object owner = null, bool isOnce = false)
            where T : class, IEvent<IEventData>
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            var channel = GetOrCreateChannel(channelType);
            var subscription = channel.Subscribe(handler, priority, owner, isOnce);
            LogService.EventLog($"[EventBus] 订阅同步事件 {typeof(T).Name} 到通道 {channelType}");
            return subscription;
        }

        public EventSubscription SubscribeAsync<T>(EventType channelType, Func<T, Task> handler,
            EventPriority priority = EventPriority.Normal, object owner = null, bool isOnce = false)
            where T : class, IEvent<IEventData>
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            var channel = GetOrCreateChannel(channelType);
            var subscription = channel.SubscribeAsync(handler, priority, owner, isOnce);
            LogService.EventLog($"[EventBus] 订阅异步事件 {typeof(T).Name} 到通道 {channelType}");
            return subscription;
        }

        public EventSubscription Subscribe<T>(Action<T> handler, EventPriority priority = EventPriority.Normal,
            object owner = null, bool isOnce = false)
            where T : class, IEvent<IEventData>
        {
            var channelType = EventSubscription.GetEventTypeFromData<T>();
            return Subscribe(channelType, handler, priority, owner, isOnce);
        }

        public EventSubscription SubscribeAsync<T>(Func<T, Task> handler, EventPriority priority = EventPriority.Normal,
            object owner = null, bool isOnce = false)
            where T : class, IEvent<IEventData>
        {
            var channelType = EventSubscription.GetEventTypeFromData<T>();
            return SubscribeAsync(channelType, handler, priority, owner, isOnce);
        }

        #endregion

        #region 取消订阅

        public void Unsubscribe(EventSubscription subscription, EventType? channelType = null)
        {
            if (subscription == null || subscription.IsDisposed) return;

            if (channelType.HasValue)
            {
                var channel = GetChannel(channelType.Value);
                channel?.Unsubscribe(subscription);
                LogService.EventLog($"[EventBus] 取消订阅：通道 {channelType.Value} 中的 {subscription.GetType().Name}");
            }
            else
            {
                foreach (var channel in _channels.Values)
                    channel.Unsubscribe(subscription);
                LogService.EventLog($"[EventBus] 取消订阅：所有通道中的 {subscription.GetType().Name}");
            }

            subscription.Dispose();
        }

        public void UnsubscribeAll(object owner)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            foreach (var channel in _channels.Values)
                channel.UnsubscribeAll(owner);
            LogService.EventLog($"[EventBus] 取消所有者 {owner.GetType().Name} 的所有订阅");
        }

        #endregion

        #region 事件处理

        public void ProcessAllChannels(int maxEventsPerChannel)
        {
            if (maxEventsPerChannel < 0) throw new ArgumentOutOfRangeException(nameof(maxEventsPerChannel));
            foreach (var channel in _channels.Values)
            {
                if (channel.IsEnabled)
                    channel.ProcessQueue(maxEventsPerChannel);
            }
        }

        #endregion

        #region 资源释放

        public void Dispose()
        {
            foreach (var channel in _channels.Values)
                channel.Dispose();
            _channels.Clear();
            IsInitialized = false;
            LogService.EventLog("[EventBus] 已释放所有资源");
        }

        #endregion
    }
}