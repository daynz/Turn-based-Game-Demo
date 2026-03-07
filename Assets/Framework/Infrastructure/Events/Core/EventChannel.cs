using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BH.Framework.Enums;
using BH.Framework.Infrastructure.Events.Interfaces;
using BH.Framework.Infrastructure.Logging.Interfaces;
using JetBrains.Annotations;
using Zenject;

namespace BH.Framework.Infrastructure.Events.Core
{
    /// <summary>
    /// 事件通道，负责事件的订阅、取消订阅和分发
    /// </summary>
    [UsedImplicitly]
    public sealed class EventChannel : IDisposable
    {
        #region 私有字段

        // 按事件类型存储订阅项，按优先级排序
        private readonly ConcurrentDictionary<Type, SortedSet<EventSubscription>> _subscriptions = new();

        private EventType DefaultEventType => EventType.GameEvent;

        // 线程安全锁
        private readonly object _lockObj = new();

        // 标记是否已释放
        private bool _disposed;

        // 事件队列（用于帧内分批处理）
        private readonly ConcurrentQueue<IEvent<IEventData>> _eventQueue = new();

        // 通道最大队列大小
        private readonly int _maxQueueSize;

        // 通道是否启用
        private bool _isEnabled = true;

        [Inject] private ILogService _logService;

        #endregion

        #region 公共属性

        /// <summary>
        /// 通道名称
        /// </summary>
        public EventType Type { get; }

        /// <summary>
        /// 当前订阅数量
        /// </summary>
        public int SubscriptionCount
        {
            get
            {
                lock (_lockObj)
                {
                    return _subscriptions.Sum(kv => kv.Value.Count);
                }
            }
        }

        /// <summary>
        /// 通道是否启用
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        /// <summary>
        /// 事件队列当前长度
        /// </summary>
        public int QueueCount => _eventQueue.Count;

        /// <summary>
        /// 最大队列大小
        /// </summary>
        public int MaxQueueSize => _maxQueueSize;

        #endregion

        #region 构造函数

        /// <summary>
        /// 初始化事件通道（兼容原有多参数构造）
        /// </summary>
        /// <param name="channelType">通道类型（作为名称）</param>
        /// <param name="priority">预留优先级参数</param>
        /// <param name="maxQueueSize">最大队列大小</param>
        public EventChannel(EventType channelType, EventPriority priority = EventPriority.Normal,
            int maxQueueSize = 1000)
            : this(channelType)
        {
            _maxQueueSize = maxQueueSize;
        }

        /// <summary>
        /// 初始化事件通道
        /// </summary>
        /// <param name="channelType">通道类型（用于日志和标识）</param>
        public EventChannel(EventType channelType)
        {
            Type = channelType == EventType.None ? DefaultEventType : channelType;
            _maxQueueSize = 1000; // 默认队列大小
        }

        #endregion

        #region 队列管理

        /// <summary>
        /// 将事件加入队列
        /// </summary>
        /// <param name="eventData">事件数据</param>
        /// <exception cref="ArgumentNullException">事件数据为空时抛出</exception>
        /// <exception cref="InvalidOperationException">队列已满时抛出</exception>
        public void Enqueue(IEvent<IEventData> eventData)
        {
            if (eventData == null)
                throw new ArgumentNullException(nameof(eventData), "事件数据不能为空");

            if (!_isEnabled)
            {
                _logService.Warning($"尝试向禁用的通道加入事件: 通道={Type}, 事件类型={eventData.GetType()}");
                return;
            }

            if (_eventQueue.Count >= _maxQueueSize)
            {
                throw new InvalidOperationException(
                    $"事件队列已满: 通道={Type}, 当前大小={_eventQueue.Count}, 最大大小={_maxQueueSize}");
            }

            _eventQueue.Enqueue(eventData);
            _logService.Debug($"事件已加入队列: 通道={Type}, 事件类型={eventData.GetType()}, 队列当前大小={_eventQueue.Count}");
        }

        /// <summary>
        /// 处理队列中的事件
        /// </summary>
        /// <param name="maxEvents">本次处理的最大事件数</param>
        public void ProcessQueue(int maxEvents)
        {
            if (!_isEnabled)
            {
                _logService.Debug($"通道已禁用，跳过事件处理: {Type}");
                return;
            }

            if (maxEvents <= 0)
                maxEvents = 1;

            int processedCount = 0;
            while (processedCount < maxEvents && _eventQueue.TryDequeue(out var eventData))
            {
                try
                {
                    // 根据事件类型分发
                    var eventType = eventData.GetType();
                    PublishInternal(eventType, eventData);
                    processedCount++;
                }
                catch (Exception ex)
                {
                    _logService.Error($"处理队列事件失败: 通道={Type}, 事件类型={eventData.GetType()}, 错误={ex.Message}");
                }
            }

            if (processedCount > 0)
            {
                _logService.Debug($"处理队列事件完成: 通道={Type}, 处理数量={processedCount}, 剩余队列大小={_eventQueue.Count}");
            }
        }

        /// <summary>
        /// 清空事件队列
        /// </summary>
        public void ClearQueue()
        {
            _eventQueue.Clear();
            _logService.Debug($"事件队列已清空: 通道={Type}");
        }

        #endregion

        #region 订阅管理

        /// <summary>
        /// 订阅同步事件
        /// </summary>
        /// <typeparam name="T">事件类型</typeparam>
        /// <param name="handler">事件处理器</param>
        /// <param name="priority">处理优先级（默认Normal）</param>
        /// <param name="owner">订阅者拥有者（用于批量取消）</param>
        /// <param name="isOnce">是否一次性订阅（触发后自动取消）</param>
        /// <returns>事件订阅实例</returns>
        /// <exception cref="ArgumentNullException">处理器为空时抛出</exception>
        public EventSubscription Subscribe<T>(
            Action<T> handler,
            EventPriority priority = EventPriority.Normal,
            object owner = null,
            bool isOnce = false)
            where T : class, IEvent<IEventData>
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler), "同步事件处理器不能为空");

            if (_disposed)
                throw new ObjectDisposedException(Type.ToString(), "事件通道已释放，无法添加订阅");

            var subscription = EventSubscription.Create(handler, priority, owner, isOnce);

            lock (_lockObj)
            {
                var eventType = typeof(T);
                if (!_subscriptions.ContainsKey(eventType))
                {
                    // 创建按优先级排序的订阅集合（高优先级先执行）
                    _subscriptions[eventType] = new SortedSet<EventSubscription>(
                        Comparer<EventSubscription>.Create((x, y) =>
                            y.Priority.CompareTo(x.Priority)));
                }

                _subscriptions[eventType].Add(subscription);
                _logService.Debug($"事件订阅添加成功: 通道={Type}, 事件类型={eventType.Name}, 优先级={priority}");
            }

            return subscription;
        }

        /// <summary>
        /// 订阅异步事件
        /// </summary>
        /// <typeparam name="T">事件类型</typeparam>
        /// <param name="handler">异步事件处理器</param>
        /// <param name="priority">处理优先级（默认Normal）</param>
        /// <param name="owner">订阅者拥有者（用于批量取消）</param>
        /// <param name="isOnce">是否一次性订阅（触发后自动取消）</param>
        /// <returns>事件订阅实例</returns>
        /// <exception cref="ArgumentNullException">处理器为空时抛出</exception>
        public EventSubscription SubscribeAsync<T>(
            Func<T, Task> handler,
            EventPriority priority = EventPriority.Normal,
            object owner = null,
            bool isOnce = false)
            where T : class, IEvent<IEventData>
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler), "异步事件处理器不能为空");

            if (_disposed)
                throw new ObjectDisposedException(Type.ToString(), "事件通道已释放，无法添加订阅");

            var subscription = EventSubscription.CreateAsync(handler, priority, owner, isOnce);

            lock (_lockObj)
            {
                var eventType = typeof(T);
                if (!_subscriptions.ContainsKey(eventType))
                {
                    _subscriptions[eventType] = new SortedSet<EventSubscription>(
                        Comparer<EventSubscription>.Create((x, y) =>
                            y.Priority.CompareTo(x.Priority)));
                }

                _subscriptions[eventType].Add(subscription);
                _logService.Debug($"异步事件订阅添加成功: 通道={Type}, 事件类型={eventType.Name}, 优先级={priority}");
            }

            return subscription;
        }

        /// <summary>
        /// 取消指定订阅
        /// </summary>
        /// <param name="subscription">要取消的订阅实例</param>
        public void Unsubscribe(EventSubscription subscription)
        {
            if (subscription == null || _disposed)
                return;

            lock (_lockObj)
            {
                foreach (var kv in _subscriptions)
                {
                    if (kv.Value.Remove(subscription))
                    {
                        subscription.Dispose();
                        _logService.Debug($"事件订阅已取消: 通道={Type}, 事件类型={kv.Key.Name}");

                        // 移除空的订阅集合
                        if (kv.Value.Count == 0)
                            _subscriptions.Remove(kv.Key, out _);

                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 取消指定类型的所有订阅
        /// </summary>
        /// <typeparam name="T">事件类型</typeparam>
        public void UnsubscribeAll<T>() where T : IEvent<IEventData>
        {
            if (_disposed)
                return;

            var eventType = typeof(T);
            lock (_lockObj)
            {
                if (_subscriptions.TryGetValue(eventType, out var subscriptions))
                {
                    foreach (var subscription in subscriptions)
                        subscription.Dispose();

                    _subscriptions.Remove(eventType, out _);
                    _logService.Debug($"事件类型所有订阅已取消: 通道={Type}, 事件类型={eventType.Name}");
                }
            }
        }

        /// <summary>
        /// 取消指定拥有者的所有订阅
        /// </summary>
        /// <param name="owner">订阅者拥有者</param>
        public void UnsubscribeByOwner(object owner)
        {
            if (owner == null || _disposed)
                return;

            lock (_lockObj)
            {
                var toRemove = new List<(Type, EventSubscription)>();

                // 收集需要移除的订阅
                foreach (var kv in _subscriptions)
                {
                    foreach (var subscription in kv.Value)
                    {
                        if (ReferenceEquals(subscription.Owner, owner))
                            toRemove.Add((kv.Key, subscription));
                    }
                }

                // 执行移除和释放
                foreach (var (eventType, subscription) in toRemove)
                {
                    _subscriptions[eventType].Remove(subscription);
                    subscription.Dispose();

                    // 移除空集合
                    if (_subscriptions[eventType].Count == 0)
                        _subscriptions.Remove(eventType, out _);
                }

                _logService.Debug($"指定拥有者的订阅已取消: 通道={Type}, 拥有者={owner.GetType().Name}, 取消数量={toRemove.Count}");
            }
        }

        /// <summary>
        /// 清空所有订阅（兼容EventBus的UnsubscribeAll调用）
        /// </summary>
        /// <param name="owner">订阅者拥有者</param>
        public void UnsubscribeAll(object owner)
        {
            UnsubscribeByOwner(owner);
        }

        /// <summary>
        /// 清空所有订阅
        /// </summary>
        public void Clear()
        {
            if (_disposed)
                return;

            lock (_lockObj)
            {
                foreach (var subscriptions in _subscriptions.Values)
                {
                    foreach (var subscription in subscriptions)
                        subscription.Dispose();
                }

                _subscriptions.Clear();
                _logService.Debug($"事件通道所有订阅已清空: 通道={Type}");
            }
        }

        #endregion

        #region 事件分发

        /// <summary>
        /// 内部事件分发逻辑
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <param name="eventData">事件数据</param>
        private void PublishInternal(Type eventType, IEvent<IEventData> eventData)
        {
            lock (_lockObj)
            {
                if (!_subscriptions.TryGetValue(eventType, out var subscriptions))
                    return;

                // 复制订阅集合防止遍历中集合变更
                var subscriptionsCopy = subscriptions.ToList();

                foreach (var subscription in subscriptionsCopy)
                {
                    if (subscription.IsDisposed)
                    {
                        // 清理已释放的订阅
                        subscriptions.Remove(subscription);
                        continue;
                    }

                    try
                    {
                        // 同步触发订阅处理
                        subscription.Invoke(eventData);

                        // 一次性订阅触发后自动取消
                        if (subscription.IsOnce)
                            Unsubscribe(subscription);
                    }
                    catch (Exception ex)
                    {
                        _logService.Error($"同步事件分发失败: 通道={Type}, 事件类型={eventType.Name}, 错误={ex.Message}");
                    }
                }

                // 移除空集合
                if (subscriptions.Count == 0)
                    _subscriptions.Remove(eventType, out _);
            }
        }

        /// <summary>
        /// 同步分发事件
        /// </summary>
        /// <typeparam name="T">事件类型</typeparam>
        /// <param name="eventData">事件数据</param>
        public void Publish<T>(T eventData) where T : class, IEvent<IEventData>
        {
            if (eventData == null)
                throw new ArgumentNullException(nameof(eventData), "事件数据不能为空");

            if (_disposed)
            {
                _logService.Warning($"尝试向已释放的事件通道发布事件: 通道={Type}, 事件类型={typeof(T).Name}");
                return;
            }

            var eventType = typeof(T);
            _logService.Debug($"开始分发同步事件: 通道={Type}, 事件类型={eventType.Name}");

            PublishInternal(eventType, eventData);
        }

        /// <summary>
        /// 异步分发事件
        /// </summary>
        /// <typeparam name="T">事件类型</typeparam>
        /// <param name="eventData">事件数据</param>
        /// <returns>分发任务</returns>
        public async Task PublishAsync<T>(T eventData) where T : class, IEvent<IEventData>
        {
            if (eventData == null)
                throw new ArgumentNullException(nameof(eventData), "事件数据不能为空");

            if (_disposed)
            {
                _logService.Warning($"尝试向已释放的事件通道发布事件: 通道={Type}, 事件类型={typeof(T).Name}");
                return;
            }

            var eventType = typeof(T);
            _logService.Debug($"开始分发异步事件: 通道={Type}, 事件类型={eventType.Name}");

            List<EventSubscription> subscriptionsCopy = null;

            lock (_lockObj)
            {
                if (_subscriptions.TryGetValue(eventType, out var subscriptions))
                {
                    // 复制订阅集合防止遍历中集合变更
                    subscriptionsCopy = subscriptions.ToList();
                }
            }

            if (subscriptionsCopy == null || subscriptionsCopy.Count == 0)
                return;

            // 并行执行所有异步订阅（按优先级排序）
            var tasks = new List<Task>();
            foreach (var subscription in subscriptionsCopy)
            {
                if (subscription.IsDisposed)
                {
                    // 后续清理已释放的订阅
                    continue;
                }

                try
                {
                    tasks.Add(subscription.InvokeAsync(eventData));

                    // 一次性订阅触发后自动取消
                    if (subscription.IsOnce)
                        Unsubscribe(subscription);
                }
                catch (Exception ex)
                {
                    _logService.Error($"异步事件分发失败: 通道={Type}, 事件类型={eventType.Name}, 错误={ex.Message}");
                }
            }

            // 等待所有异步任务完成
            if (tasks.Count > 0)
                await Task.WhenAll(tasks).ConfigureAwait(false);

            // 清理已释放的订阅
            lock (_lockObj)
            {
                if (_subscriptions.TryGetValue(eventType, out var subscriptions))
                {
                    var disposedSubscriptions = subscriptions.Where(s => s.IsDisposed).ToList();
                    foreach (var sub in disposedSubscriptions)
                        subscriptions.Remove(sub);

                    if (subscriptions.Count == 0)
                        _subscriptions.Remove(eventType, out _);
                }
            }
        }

        #endregion

        #region IDisposable 实现

        /// <summary>
        /// 释放事件通道资源
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            // 清空所有订阅
            Clear();

            // 清空事件队列
            ClearQueue();

            _disposed = true;
            _logService.Debug($"事件通道已释放: {Type}");
        }

        #endregion
    }
}