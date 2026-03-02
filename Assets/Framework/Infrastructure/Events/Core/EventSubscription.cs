using System;
using System.Reflection;
using System.Threading.Tasks;
using BH.Framework.Enums;
using BH.Framework.Infrastructure.DI.Attributes;
using BH.Framework.Infrastructure.Events.Attributes;
using BH.Framework.Infrastructure.Events.Interfaces;
using BH.Framework.Infrastructure.Logging.Core;
using JetBrains.Annotations;

namespace BH.Framework.Infrastructure.Events.Core
{
    /// <summary>
    /// 事件订阅项，表示一个事件处理程序的订阅关系
    /// </summary>
    public class EventSubscription : IDisposable
    {
        #region 私有字段

        private readonly Action<IEventData> _syncHandler;
        private readonly Func<IEventData, Task> _asyncHandler;
        private readonly bool _isAsync;
        private bool _isDisposed;
        private EventType? _eventType;

        [Inject] private LogService _logService;

        #endregion

        #region 公共属性

        public string Name => GetType().Name;
        public Guid Id { get; }

        /// <summary>
        /// 处理的具体事件类型
        /// </summary>
        public Type HandledType { get; }


        /// <summary>
        /// 事件类型
        /// </summary>
        public EventType EventType
        {
            get
            {
                if (_eventType != null) return _eventType.Value;
                var attr = HandledType.GetCustomAttribute<EventTypeAttribute>();
                _eventType = attr?.EventType ?? EventType.None;

                return _eventType.Value;
            }
        }

        /// <summary>
        /// 处理优先级
        /// </summary>
        public EventPriority Priority { get; }

        /// <summary>
        /// 订阅者所有者对象
        /// </summary>
        public object Owner { get; }

        /// <summary>
        /// 是否为一次性订阅
        /// </summary>
        public bool IsOnce { get; }

        /// <summary>
        /// 是否为异步处理
        /// </summary>
        public bool IsAsync => _isAsync;

        #endregion

        #region 构造函数

        private EventSubscription(Type handledType, Action<IEventData> handler, EventPriority priority,
            [CanBeNull] object owner, bool isOnce)
        {
            Id = Guid.NewGuid();
            HandledType = handledType ?? throw new ArgumentNullException(nameof(handledType));
            _syncHandler = handler ?? throw new ArgumentNullException(nameof(handler));
            _asyncHandler = null;
            _isAsync = false;
            Priority = priority;
            Owner = owner;
            IsOnce = isOnce;
        }

        private EventSubscription(Type handledType, Func<IEventData, Task> handler, EventPriority priority,
            [CanBeNull] object owner, bool isOnce)
        {
            HandledType = handledType ?? throw new ArgumentNullException(nameof(handledType));
            _syncHandler = null;
            _asyncHandler = handler ?? throw new ArgumentNullException(nameof(handler));
            _isAsync = true;
            Priority = priority;
            Owner = owner;
            IsOnce = isOnce;
        }

        #endregion

        #region 静态工厂方法

        /// <summary>
        /// 创建同步事件订阅（泛型）
        /// </summary>
        public static EventSubscription Create<T>(Action<T> handler, EventPriority priority = 0,
            [CanBeNull] object owner = null,
            bool isOnce = false) where T : IEventData
        {
            return handler == null
                ? throw new ArgumentNullException(nameof(handler))
                : new EventSubscription(typeof(T), Wrap, priority, owner, isOnce);
            void Wrap(IEventData data) => handler((T)data);
        }

        /// <summary>
        /// 创建异步事件订阅（泛型）
        /// </summary>
        public static EventSubscription CreateAsync<T>(Func<T, Task> handler, EventPriority priority = 0,
            [CanBeNull] object owner = null, bool isOnce = false) where T : IEventData
        {
            return handler == null
                ? throw new ArgumentNullException(nameof(handler))
                : new EventSubscription(typeof(T), WrapAsync, priority, owner, isOnce);
            async Task WrapAsync(IEventData data) => await handler((T)data);
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 触发事件处理
        /// </summary>
        public void Invoke(IEventData eventData)
        {
            if (_isDisposed) return;

            try
            {
                if (!_isAsync)
                {
                    _syncHandler?.Invoke(eventData);
                }
                else
                {
                    if (_asyncHandler == null)
                    {
                        _logService.Error($"异步事件标记为异步但异步处理器为空: {HandledType.Name}", Name);
                    }
                    else
                    {
                        // 异步任务调度到线程池，注意：Unity API 需在主线程调用
                        Task.Run(async () =>
                        {
                            try
                            {
                                await _asyncHandler(eventData);
                            }
                            catch (Exception ex)
                            {
                                _logService.Error($"异步事件处理失败: {HandledType.Name}, Error: {ex.Message}", Name);
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logService.Error($"事件处理失败: {HandledType.Name}, Error: {ex.Message}", Name);
            }
        }

        #endregion

        #region IDisposable 实现

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
        }

        #endregion

        #region 静态方法

        public static EventType GetEventTypeFromData<T>() where T : IEventData
        {
            var attr = typeof(T).GetCustomAttribute<EventTypeAttribute>();
            return attr?.EventType ??
                   throw new InvalidOperationException(
                       $"类型 {typeof(T).Name} 缺少 EventTypeAttribute 特性，无法确定通道路由。"
                   );
        }

        #endregion
    }
}