using System;
using System.Threading.Tasks;
using BH.Framework.Enums;
using BH.Framework.Infrastructure.Events.Interfaces;
using BH.Framework.Infrastructure.Logging.Core;
using Zenject;
using EventType = BH.Framework.Enums.EventType;

namespace BH.Framework.Infrastructure.Events.Core
{
    [Serializable]
    public class EventService : IEventService
    {
        #region 字段与属性

        //private EventConfig _config;
        // private EventDebugger _debugger;
        // private EventRecorder _recorder;
        // private EventProfiler _profiler;
        private bool _isEnable = true;

        [Inject] private LogService LogService { get; set; }
        [Inject] private EventBus EventBus { get; set; }

        public bool IsEnable
        {
            get => _isEnable;
            private set => _isEnable = value;
        }

        #endregion

        #region 初始化

        public void Initialize()
        {
            //var config = new EventConfig();
            //_config = config;

// #if UNITY_EDITOR || DEVELOPMENT_BUILD
//             _debugger = new EventDebugger();
//             LogService.EventLog("事件调试器开启", IsEnable);
// #endif
//
//             if (_config.recordEventHistory)
//             {
//                 _recorder = new EventRecorder();
//                 LogService.EventLog("事件记录器开启", IsEnable);
//             }
//
//             if (_config.enableProfiling)
//             {
//                 _profiler = new EventProfiler();
//                 LogService.EventLog("事件性能分析器开启", IsEnable);
//             }

            LogService.EventLog("事件系统初始化完成", IsEnable);
        }

        #endregion

        #region 控制接口

        public void Enable()
        {
            EventBus?.EnableAllChannels();
            IsEnable = true;

            LogService.EventLog("事件系统已启用", IsEnable);
        }

        public void Disable()
        {
            EventBus?.DisableAllChannels();
            LogService.EventLog("事件系统已禁用", IsEnable);
            IsEnable = false;
        }

        #endregion

        #region 事件发布

        public void Publish<T>(T eventData) where T : IEventData
        {
            if (eventData == null) throw new ArgumentNullException(nameof(eventData));
            EventBus.Publish(eventData);
        }

        public void Publish<T>(T eventData, EventType channelType) where T : IEventData
        {
            if (eventData == null) throw new ArgumentNullException(nameof(eventData));
            EventBus.Publish(eventData, channelType);
        }

        #endregion

        #region 事件订阅

        public EventSubscription Subscribe<T>(Action<T> handler, EventPriority p = 0, object owner = null,
            bool isOnce = false) where T : IEventData
        {
            return EventBus?.Subscribe(handler, p, owner, isOnce);
        }

        public EventSubscription SubscribeAsync<T>(Func<T, Task> handler, EventPriority p = 0,
            object owner = null, bool isOnce = false) where T : IEventData
        {
            return EventBus?.SubscribeAsync(handler, p, owner, isOnce);
        }

        public EventSubscription SubscribeToChannel<T>(EventType channelType, Action<T> handler,
            EventPriority p = 0, object owner = null, bool isOnce = false) where T : IEventData
        {
            return EventBus?.Subscribe(channelType, handler, p, owner, isOnce);
        }

        public void Unsubscribe(EventSubscription subscription, EventType? channelType = null)
        {
            EventBus?.Unsubscribe(subscription, channelType);
        }

        public void UnsubscribeAll(object owner)
        {
            EventBus?.UnsubscribeAll(owner);
        }

        #endregion

        #region 通道管理

        public EventChannel GetChannel(EventType channelType)
        {
            return EventBus?.GetChannel(channelType);
        }

        public EventChannel CreateChannel(EventType channelType, EventPriority p = EventPriority.Normal,
            int maxQueueSize = 1000)
        {
            return EventBus?.CreateChannel(channelType, p, maxQueueSize);
        }

        #endregion

        public void ProcessChannels(int maxEventsPerFrame)
        {
            EventBus?.ProcessAllChannels(maxEventsPerFrame);
        }

        public void Dispose()
        {
            EventBus?.Dispose();
            // _debugger?.Dispose();
            // _recorder?.Dispose();
            // _profiler?.Dispose();
            LogService.EventLog("事件系统已关闭");
        }
    }
}