using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using BH.Framework.Enums;
using BH.Framework.Infrastructure.Events.Interfaces;
using BH.Framework.Infrastructure.Logging.Core;
using UnityEngine;
using Zenject;
using EventType = BH.Framework.Enums.EventType;

namespace BH.Framework.Infrastructure.Events.Core
{
    /// <summary>
    /// 事件总线，负责管理通道和事件路由
    /// </summary>
    [Serializable]
    public class EventBus : IEventBus, IInitializable, IDisposable
    {
        #region 私有字段

        private readonly ConcurrentDictionary<EventType, EventChannel> _channels = new();
        private const EventType DefaultChannel = EventType.SystemEvent;

        [Inject] private LogService LOGService { get; set; }

        // todo: config加载

        #endregion

        #region 公共属性

        public bool IsInitialized { get; private set; } = true;

        public void Initialize()
        {
            // 默认初始化
            var allEventTypes = Enum.GetValues(typeof(EventType));
            foreach (var type in allEventTypes)
            {
                CreateChannel((EventType)type);
            }
        }

        public IEnumerable<EventChannel> GetAllChannels() => _channels.Values;

        #endregion

        #region 通道管理

        /// <summary>
        /// 创建通道
        /// </summary>
        public EventChannel CreateChannel(EventType channelType, EventPriority priority = EventPriority.Normal,
            int maxQueueSize = 1000)
        {
            var channel = new EventChannel(channelType, priority, maxQueueSize);
            _channels[channelType] = channel;

            return channel;
        }

        /// <summary>
        /// 获取通道
        /// </summary>
        public EventChannel GetChannel(EventType channelType)
        {
            _channels.TryGetValue(channelType, out var channel);
            return channel;
        }

        /// <summary>
        /// 启用所有通道
        /// </summary>
        public void EnableAllChannels()
        {
            foreach (var ch in _channels.Values)
                ch.IsEnabled = true;
        }

        /// <summary>
        /// 禁用所有通道
        /// </summary>
        public void DisableAllChannels()
        {
            foreach (var ch in _channels.Values)
                ch.IsEnabled = false;
        }

        #endregion

        #region 发布事件

        /// <summary>
        /// 发布事件到指定通道
        /// </summary>
        public void Publish<T>(T eventData, EventType eventType) where T : IEventData
        {
            var channel = GetChannel(eventType);
            if (channel == null)
            {
                LOGService.EventLog($"通道不存在: {eventType}");
                return;
            }

            channel.Enqueue(eventData);
            if (LOGService == null)
            {
                Debug.Log("LogService注入失败");
            }
            else
            {
                LOGService.EventLog($"发送事件{typeof(T).Name}");
            }
        }

        /// <summary>
        /// 发布事件（自动根据事件的目标通道路由）
        /// </summary>
        public void Publish<T>(T eventData) where T : IEventData
        {
            Publish(eventData, eventData.EventType == EventType.None ? DefaultChannel : eventData.EventType);
        }

        #endregion

        #region 订阅管理

        /// <summary>
        /// 订阅事件到指定通道（同步）
        /// </summary>
        public EventSubscription Subscribe<T>(EventType channelType, Action<T> handler, EventPriority priority = 0,
            object owner = null, bool isOnce = false) where T : IEventData
        {
            var channel = GetOrCreateChannel(channelType);
            return channel?.Subscribe(handler, priority, owner, isOnce);
        }

        /// <summary>
        /// 订阅异步事件到指定通道
        /// </summary>
        public EventSubscription SubscribeAsync<T>(EventType channelType, Func<T, Task> handler,
            EventPriority priority = 0, object owner = null, bool isOnce = false) where T : IEventData
        {
            var channel = GetOrCreateChannel(channelType);
            return channel?.SubscribeAsync(handler, priority, owner, isOnce);
        }

        /// <summary>
        /// 订阅事件到默认通道（根据事件类型自动选择）
        /// </summary>
        public EventSubscription Subscribe<T>(Action<T> handler, EventPriority priority = 0, object owner = null,
            bool isOnce = false) where T : IEventData
        {
            var channelType = EventSubscription.GetEventTypeFromData<T>();
            return Subscribe(channelType, handler, priority, owner, isOnce);
        }

        /// <summary>
        /// 订阅异步事件到默认通道
        /// </summary>
        public EventSubscription SubscribeAsync<T>(Func<T, Task> handler, EventPriority priority = 0,
            object owner = null,
            bool isOnce = false) where T : IEventData
        {
            var channelType = EventSubscription.GetEventTypeFromData<T>();
            return SubscribeAsync(channelType, handler, priority, owner, isOnce);
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        public void Unsubscribe(EventSubscription subscription, EventType? channelType = null)
        {
            if (subscription == null) return;

            if (channelType.HasValue)
            {
                var ch = GetChannel(channelType.Value);
                ch?.Unsubscribe(subscription);
            }
            else
            {
                foreach (var ch in _channels.Values)
                    ch.Unsubscribe(subscription);
            }
        }

        /// <summary>
        /// 取消某所有者所有订阅（在所有通道中）
        /// </summary>
        public void UnsubscribeAll(object owner)
        {
            if (owner == null) return;
            foreach (var ch in _channels.Values)
                ch.UnsubscribeAll(owner);
        }

        private EventChannel GetOrCreateChannel(EventType channelType)
        {
            if (!_channels.TryGetValue(channelType, out var channel))
            {
                channel = CreateChannel(channelType);
            }

            return channel;
        }

        #endregion

        #region 处理事件

        /// <summary>
        /// 处理所有已知通道上的事件。
        /// </summary>
        /// <param name="maxEventsPerChannel">每个通道要处理的最大事件数量。</param>
        public void ProcessAllChannels(int maxEventsPerChannel)
        {
            foreach (var channel in _channels.Values)
            {
                channel.ProcessQueue(maxEventsPerChannel);
            }
        }

        #endregion

        #region 资源释放

        public void Dispose()
        {
            foreach (var ch in _channels.Values)
                ch.Dispose();
            _channels.Clear();
            IsInitialized = false;
            LOGService.EventLog("[EventBus] 已释放");
        }

        #endregion
    }
}