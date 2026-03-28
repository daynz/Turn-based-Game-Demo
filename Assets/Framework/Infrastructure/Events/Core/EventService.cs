using System;
using System.Threading.Tasks;
using BH.Framework.Enums;
using BH.Framework.Infrastructure.Events.Interfaces;
using BH.Framework.Infrastructure.Logging.Interfaces;
using Zenject;

namespace BH.Framework.Infrastructure.Events.Core
{
    [Serializable]
    public class EventService : IEventService
    {
        private bool _isEnable = true;

        [Inject] private ILogService LogService { get; set; }
        [Inject] private IEventBus EventBus { get; set; }

        public bool IsEnable => _isEnable;

        #region 生命周期

        public void Initialize()
        {
            LogService.Info("[EventService] 事件系统初始化完成");
        }

        public void Enable()
        {
            if (_isEnable) return;
            _isEnable = true;
            LogService.Info("[EventService] 事件系统已启用");
        }

        public void Disable()
        {
            if (!_isEnable) return;
            _isEnable = false;
            LogService.Info("[EventService] 事件系统已禁用");
        }

        #endregion

        #region 事件发布

        public void Publish<T>(T eventData) where T : class, IEvent<IEventData>
        {
            if (!_isEnable)
            {
                LogService.Info($"[EventService] 事件系统已禁用，发布失败：{typeof(T).Name}");
                return;
            }

            EventBus.Publish(eventData);
        }

        public void Publish<T>(T eventData, EventType channelType) where T : class, IEvent<IEventData>
        {
            if (!_isEnable)
            {
                LogService.Info($"[EventService] 事件系统已禁用，发布失败：{typeof(T).Name} 到通道 {channelType}");
                return;
            }

            EventBus.Publish(eventData, channelType);
        }

        #endregion

        #region 事件订阅

        public EventSubscription Subscribe<T>(Action<T> handler, EventPriority priority = EventPriority.Normal,
            object owner = null, bool isOnce = false)
            where T : class, IEvent<IEventData>
        {
            if (_isEnable) return EventBus.Subscribe(handler, priority, owner, isOnce);
            LogService.Warning($"[EventService] 事件系统已禁用，订阅失败：{typeof(T).Name}");
            return null;
        }

        public EventSubscription SubscribeAsync<T>(Func<T, Task> handler, EventPriority priority = EventPriority.Normal,
            object owner = null, bool isOnce = false)
            where T : class, IEvent<IEventData>
        {
            if (_isEnable) return EventBus.SubscribeAsync(handler, priority, owner, isOnce);
            LogService.Warning($"[EventService] 事件系统已禁用，订阅失败：{typeof(T).Name}");
            return null;
        }

        public EventSubscription SubscribeToChannel<T>(EventType channelType, Action<T> handler,
            EventPriority priority = EventPriority.Normal, object owner = null, bool isOnce = false)
            where T : class, IEvent<IEventData>
        {
            if (_isEnable) return EventBus.Subscribe(channelType, handler, priority, owner, isOnce);
            LogService.Warning($"[EventService] 事件系统已禁用，订阅失败：{typeof(T).Name} 到通道 {channelType}");
            return null;

        }

        #endregion

        #region 取消订阅

        public void Unsubscribe(EventSubscription subscription, EventType? channelType = null)
        {
            EventBus.Unsubscribe(subscription, channelType);
            LogService.Info($"[EventService] 取消订阅：{subscription?.GetType().Name}");
        }

        public void UnsubscribeAll(object owner)
        {
            EventBus.UnsubscribeAll(owner);
            LogService.Info($"[EventService] 取消所有者 {owner?.GetType().Name} 的所有订阅");
        }

        #endregion

        #region 事件处理

        public void ProcessChannels(int maxEventsPerFrame)
        {
            if (!_isEnable)
            {
                LogService.Warning("[EventService] 事件系统已禁用，跳过事件处理");
                return;
            }

            var maxPerChannel = Math.Max(1, maxEventsPerFrame / EventBus.ChannelCount);
            EventBus.ProcessAllChannels(maxPerChannel);
        }

        #endregion

        #region 通道管理

        public EventChannel GetChannel(EventType channelType)
        {
            return EventBus.GetChannel(channelType);
        }

        #endregion

        #region 资源释放

        public void Dispose()
        {
            EventBus.Dispose();
            _isEnable = false;
            LogService.Info("[EventService] 事件系统已关闭");
        }

        #endregion
    }
}