using System;
using System.Reflection;
using System.Threading.Tasks;
using BH.Framework.Enums;
using BH.Framework.Infrastructure.Events.Attributes;
using BH.Framework.Infrastructure.Events.Interfaces;
using BH.Framework.Infrastructure.Logging.Interfaces;
using JetBrains.Annotations;
using Zenject;

namespace BH.Framework.Infrastructure.Events.Core
{
    /// <summary>
    /// 事件订阅项，表示一个事件处理程序的订阅关系
    /// </summary>
    [UsedImplicitly]
    public sealed class EventSubscription : IDisposable
    {
        #region 私有字段

        private readonly Type _handledEventType;
        private readonly bool _isAsync;
        private bool _disposed;

        [Inject] private readonly ILogService _logService;

        #endregion

        #region 公共属性

        /// <summary>
        /// 同步事件处理器
        /// </summary>
        public Action<IEvent<IEventData>> SyncHandler { get; }

        /// <summary>
        /// 异步事件处理器
        /// </summary>
        public Func<IEvent<IEventData>, Task> AsyncHandler { get; }

        /// <summary>
        /// 事件处理优先级
        /// </summary>
        public EventPriority Priority { get; }

        /// <summary>
        /// 订阅者拥有者
        /// </summary>
        public object Owner { get; }

        /// <summary>
        /// 是否一次性订阅（触发后自动释放）
        /// </summary>
        public bool IsOnce { get; }

        /// <summary>
        /// 是否已释放
        /// </summary>
        public bool IsDisposed => _disposed;

        /// <summary>
        /// 处理的事件类型
        /// </summary>
        public Type HandledEventType => _handledEventType;

        #endregion

        #region 构造函数

        /// <summary>
        /// 私有构造函数，禁止外部直接实例化
        /// </summary>
        /// <param name="syncHandler">同步处理器</param>
        /// <param name="asyncHandler">异步处理器</param>
        /// <param name="priority">优先级</param>
        /// <param name="owner">拥有者</param>
        /// <param name="isOnce">是否一次性</param>
        /// <param name="handledEventType">处理的事件类型</param>
        private EventSubscription(
            Action<IEvent<IEventData>> syncHandler,
            Func<IEvent<IEventData>, Task> asyncHandler,
            EventPriority priority,
            object owner,
            bool isOnce,
            Type handledEventType)
        {
            SyncHandler = syncHandler;
            AsyncHandler = asyncHandler;
            Priority = priority;
            Owner = owner;
            IsOnce = isOnce;
            _handledEventType = handledEventType ?? throw new ArgumentNullException(nameof(handledEventType));
            _isAsync = asyncHandler != null;
        }

        #endregion

        #region 静态工厂方法

        /// <summary>
        /// 创建同步事件订阅（泛型）
        /// </summary>
        /// <typeparam name="T">事件类型</typeparam>
        /// <param name="handler">同步事件处理器</param>
        /// <param name="priority">处理优先级</param>
        /// <param name="owner">订阅者拥有者</param>
        /// <param name="isOnce">是否一次性订阅</param>
        /// <returns>事件订阅实例</returns>
        /// <exception cref="ArgumentNullException">处理器为空时抛出</exception>
        public static EventSubscription Create<T>(
            Action<T> handler,
            EventPriority priority = EventPriority.Normal,
            object owner = null,
            bool isOnce = false)
            where T : class, IEvent<IEventData>
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler), "同步事件处理器不能为空");

            return new EventSubscription(
                syncHandler: e =>
                {
                    if (e is T typedEvent)
                        handler.Invoke(typedEvent);
                    else
                        throw new InvalidCastException($"无法将事件类型 {e.GetType().Name} 转换为 {typeof(T).Name}");
                },
                asyncHandler: null,
                priority: priority,
                owner: owner,
                isOnce: isOnce,
                handledEventType: typeof(T));
        }

        /// <summary>
        /// 创建异步事件订阅（泛型）
        /// </summary>
        /// <typeparam name="T">事件类型</typeparam>
        /// <param name="handler">异步事件处理器</param>
        /// <param name="priority">处理优先级</param>
        /// <param name="owner">订阅者拥有者</param>
        /// <param name="isOnce">是否一次性订阅</param>
        /// <returns>事件订阅实例</returns>
        /// <exception cref="ArgumentNullException">处理器为空时抛出</exception>
        public static EventSubscription CreateAsync<T>(
            Func<T, Task> handler,
            EventPriority priority = EventPriority.Normal,
            object owner = null,
            bool isOnce = false)
            where T : class, IEvent<IEventData>
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler), "异步事件处理器不能为空");

            return new EventSubscription(
                syncHandler: null,
                asyncHandler: async e =>
                {
                    if (e is T typedEvent)
                        await handler.Invoke(typedEvent).ConfigureAwait(false);
                    else
                        throw new InvalidCastException($"无法将事件类型 {e.GetType().Name} 转换为 {typeof(T).Name}");
                },
                priority: priority,
                owner: owner,
                isOnce: isOnce,
                handledEventType: typeof(T));
        }

        #endregion

        #region 事件触发方法

        /// <summary>
        /// 同步触发事件处理（异步处理器会被调度到线程池）
        /// </summary>
        /// <param name="eventData">事件数据</param>
        public void Invoke(IEvent<IEventData> eventData)
        {
            if (_disposed)
            {
                _logService.Warning("尝试调用已释放的事件订阅");
                return;
            }

            if (eventData == null)
            {
                _logService.Error("事件数据不能为空");
                return;
            }

            try
            {
                if (!_isAsync)
                {
                    // 同步处理
                    SyncHandler?.Invoke(eventData);
                }
                else
                {
                    // 异步处理：调度到线程池并捕获异常
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await AsyncHandler!.Invoke(eventData).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            _logService.Error($"异步事件处理失败: {_handledEventType.Name}, 错误: {ex.Message}");
                        }
                    });
                }

                // 一次性订阅触发后自动释放
                if (IsOnce)
                    Dispose();
            }
            catch (Exception ex)
            {
                _logService.Error($"事件处理失败: {_handledEventType.Name}, 错误: {ex.Message}");

                // 一次性订阅即使处理失败也释放
                if (IsOnce)
                    Dispose();
            }
        }

        /// <summary>
        /// 异步触发事件处理
        /// </summary>
        /// <param name="eventData">事件数据</param>
        /// <returns>处理任务</returns>
        public async Task InvokeAsync(IEvent<IEventData> eventData)
        {
            if (_disposed)
            {
                _logService.Warning("尝试调用已释放的事件订阅");
                return;
            }

            if (eventData == null)
            {
                _logService.Error("事件数据不能为空");
                return;
            }

            try
            {
                if (!_isAsync)
                {
                    // 同步处理器异步调用
                    SyncHandler?.Invoke(eventData);
                }
                else
                {
                    // 异步处理器直接调用
                    await AsyncHandler!.Invoke(eventData).ConfigureAwait(false);
                }

                // 一次性订阅触发后自动释放
                if (IsOnce)
                    Dispose();
            }
            catch (Exception ex)
            {
                _logService.Error($"异步事件处理失败: {_handledEventType.Name}, 错误: {ex.Message}");

                // 一次性订阅即使处理失败也释放
                if (IsOnce)
                    Dispose();
                throw; // 向上抛出异常，由调用方处理
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 获取事件类型
        /// </summary>
        /// <typeparam name="T">事件数据类型</typeparam>
        /// <returns>事件类型枚举</returns>
        public static EventType GetEventTypeFromData<T>() where T : IEvent<IEventData>
        {
            // 可根据实际业务逻辑扩展，比如从特性获取事件类型
            var eventTypeAttr = typeof(T).GetCustomAttribute<EventTypeAttribute>();
            return eventTypeAttr?.EventType ?? EventType.SystemEvent;
        }

        #endregion

        #region IDisposable 实现

        /// <summary>
        /// 释放订阅资源
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _logService?.Debug($"事件订阅已释放: {_handledEventType.Name}");
        }

        #endregion
    }
}