using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BH.Framework.Enums;
using BH.Framework.Infrastructure.DI.Attributes;
using BH.Framework.Infrastructure.Events.Interfaces;
using BH.Framework.Infrastructure.Logging.Core;
using JetBrains.Annotations;

namespace BH.Framework.Infrastructure.Events.Core
{
    /// <summary>
    /// 事件通道，维护独立订阅列表和队列
    /// </summary>
    public class EventChannel : IDisposable
    {
        #region 私有字段

        private readonly EventType _channelType;
        private readonly int _maxQueueSize;
        private readonly ConcurrentQueue<IEventData> _eventQueue;
        private readonly ConcurrentDictionary<EventPriority, List<EventSubscription>> _subscriptionsByPriority;
        private bool _isProcessing;
        private readonly object _lock = new();

        #endregion

        #region 属性

        public string Name => GetType().Name;
        public EventType ChannelType { get; }
        public EventPriority ChannelPriority { get; }
        public bool IsEnabled { get; set; } = true;
        [field: Inject] private LogService LogService { get; set; }

        #endregion

        #region 构造函数

        public EventChannel(EventType channelType, EventPriority priority, int maxQueueSize = 1000)
        {
            ChannelType = channelType;
            ChannelPriority = priority;
            _maxQueueSize = maxQueueSize;
            _eventQueue = new ConcurrentQueue<IEventData>();
            _subscriptionsByPriority = new ConcurrentDictionary<EventPriority, List<EventSubscription>>();
        }

        #endregion

        #region 订阅管理

        /// <summary>
        /// 订阅事件到当前通道（同步）
        /// </summary>
        public EventSubscription Subscribe<T>(Action<T> handler, EventPriority priority = 0,
            [CanBeNull] object owner = null,
            bool isOnce = false)
            where T : IEventData
        {
            var subscription = EventSubscription.Create(handler, priority, owner, isOnce);
            AddSubscription(priority, subscription);
            return subscription;
        }

        /// <summary>
        /// 订阅事件到当前通道（异步）
        /// </summary>
        public EventSubscription SubscribeAsync<T>(Func<T, Task> handler, EventPriority priority = 0,
            [CanBeNull] object owner = null, bool isOnce = false)
            where T : IEventData
        {
            var subscription = EventSubscription.CreateAsync(handler, priority, owner, isOnce);
            AddSubscription(priority, subscription);
            return subscription;
        }

        private void AddSubscription(EventPriority priority, EventSubscription subscription)
        {
            lock (_lock)
            {
                if (!_subscriptionsByPriority.ContainsKey(priority))
                {
                    _subscriptionsByPriority[priority] = new List<EventSubscription>();
                }

                _subscriptionsByPriority[priority].Add(subscription);
            }
        }

        /// <summary>
        /// 取消所有属于某所有者的订阅
        /// </summary>
        public void UnsubscribeAll(object owner)
        {
            if (owner == null) return;
            foreach (var kv in _subscriptionsByPriority)
            {
                kv.Value.RemoveAll(sub => sub.Owner == owner);
            }
        }

        public void Unsubscribe(EventSubscription subscription)
        {
            lock (_lock)
            {
                foreach (var kvp in _subscriptionsByPriority)
                {
                    kvp.Value.RemoveAll(s => s.Id == subscription.Id);
                }
            }
        }

        /// <summary>
        /// 清空所有订阅
        /// </summary>
        private void ClearSubscriptions()
        {
            foreach (var list in _subscriptionsByPriority.Values)
            {
                foreach (var sub in list)
                    sub.Dispose();
                list.Clear();
            }

            _subscriptionsByPriority.Clear();
        }

        #endregion

        #region 事件入队

        /// <summary>
        /// 将事件加入通道队列
        /// </summary>
        public void Enqueue(IEventData eventData)
        {
            if (!IsEnabled || eventData == null) return;

            if (_eventQueue.Count >= _maxQueueSize)
            {
                LogService.EventLog($"通道 '{ChannelType}' 队列已满，丢弃事件: {eventData.GetType().Name}", Name);
                return;
            }

            _eventQueue.Enqueue(eventData);
        }

        #endregion

        #region 队列处理

        /// <summary>
        /// 处理队列中的事件（最多处理指定数量）
        /// </summary>
        public void ProcessQueue(int maxEvents = 10)
        {
            if (!IsEnabled || _isProcessing) return;

            lock (_lock)
            {
                _isProcessing = true;
                var processed = 0;
                try
                {
                    while (_eventQueue.TryDequeue(out var eventData) && processed < maxEvents)
                    {
                        ProcessSingleEvent(eventData);
                        processed++;
                    }
                }
                finally
                {
                    _isProcessing = false;
                }
            }
        }

        /// <summary>
        /// 立即处理事件（不入队）
        /// </summary>
        private void ProcessSingleEvent(IEventData eventData)
        {
            if (eventData.IsHandled && !eventData.AllowMultipleHandlers)
            {
                return;
            }

            var subscriptions = new List<EventSubscription>();
            lock (_lock)
            {
                var sortedPriorities =
                    _subscriptionsByPriority.Keys.OrderByDescending(p => p).ToList();
                foreach (var subsForPriority in
                         sortedPriorities.Select(priority => _subscriptionsByPriority[priority]))
                {
                    subscriptions.AddRange(subsForPriority);
                }
            }

            foreach (var subscription in subscriptions)
            {
                try
                {
                    subscription.Invoke(eventData);

                    if (subscription.IsOnce)
                    {
                        Unsubscribe(subscription);
                    }
                }
                catch (Exception ex)
                {
                    LogService.EventLogError(
                        $"调用事件处理程序时出错 '{eventData.GetType().Name}': {ex.Message}", Name);
                }
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 获取当前队列大小
        /// </summary>
        public int GetQueueSize() => _eventQueue.Count;

        /// <summary>
        /// 清空队列
        /// </summary>
        private void ClearQueue()
        {
            _eventQueue.Clear();
        }

        #endregion

        #region IDisposable 实现

        public void Dispose()
        {
            ClearQueue();
            ClearSubscriptions();
        }

        #endregion
    }
}