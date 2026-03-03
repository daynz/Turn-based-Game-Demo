using System;
using System.Threading.Tasks;
using BH.Framework.Enums;
using BH.Framework.Infrastructure.DI.Attributes;
using BH.Framework.Infrastructure.DI.Interfaces;
using BH.Framework.Infrastructure.Events.Config;
using BH.Framework.Infrastructure.Events.Interfaces;
using BH.Framework.Infrastructure.Events.Tools;
using BH.Framework.Infrastructure.Logging.Core;
using UnityEngine;
using EventType = BH.Framework.Enums.EventType;

namespace BH.Framework.Infrastructure.Events.Core
{
    [Serializable]
    [AutoRegisterService]
    public class EventService : IService, IEventService
    {
        #region 字段与属性

        public string Name => GetType().Name;
        private EventConfig _config;
        private EventDebugger _debugger;
        private EventRecorder _recorder;
        private EventProfiler _profiler;
        private bool _isEnable;
        [SerializeField] private int priority = (int)PriorityOrder.EventService;
        [field: Inject] private LogService LogService { get; set; }
        [field: Inject] private EventBus EventBus { get; set; }

        public bool IsEnable
        {
            get => _isEnable;
            private set => _isEnable = value;
        }

        public bool IsInitialized { get; private set; }

        public int Priority => priority;

        #endregion

        #region 初始化

        public Task InitializeAsync()
        {
            IsInitialized = true;
            return Task.CompletedTask;
        }

        private void PerformInitialization(EventConfig config)
        {
            try
            {
                _config = config;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                //_debugger = new EventDebugger(EventBus);
                //LogService.EventLog("事件调试器开启", Name, IsEnable);
#endif

                if (_config.recordEventHistory)
                {
                    _recorder = new EventRecorder();
                    LogService.EventLog("事件记录器开启", Name, IsEnable);
                }

                if (_config.enableProfiling)
                {
                    _profiler = new EventProfiler();
                    LogService.EventLog("事件性能分析器开启", Name, IsEnable);
                }


                IsInitialized = true;
                LogService.EventLog("事件系统初始化完成", Name, IsEnable);
            }
            catch (Exception ex)
            {
                LogService.EventLogError($"初始化失败: {ex.Message}", Name, IsEnable);
                throw;
            }
        }

        #endregion

        #region 控制接口

        public void Enable()
        {
            if (!IsInitialized)
            {
                LogService.EventLog("EventManager 未初始化", Name, IsEnable);
                throw new InvalidOperationException("EventManager 未初始化");
            }

            EventBus?.EnableAllChannels();
            IsEnable = true;

            LogService.EventLog("事件系统已启用", Name, IsEnable);
        }

        public void Disable()
        {
            if (!IsInitialized) return;
            EventBus?.DisableAllChannels();
            LogService.EventLog("事件系统已禁用", Name, IsEnable);
            IsEnable = false;
        }

        public void Shutdown()
        {
            if (!IsInitialized) return;
            EventBus?.Dispose();
            //_debugger?.Dispose();
            _recorder?.Dispose();
            _profiler?.Dispose();
            EventBus = null;
            IsInitialized = false;

            LogService.EventLog("事件系统已关闭", Name);
        }

        #endregion

        #region 事件发布

        public void Publish<T>(T eventData) where T : IEventData
        {
            if (!IsInitialized) throw new InvalidOperationException("EventManager 未初始化");
            if (eventData == null) throw new ArgumentNullException(nameof(eventData));
            EventBus.Publish(eventData);
        }

        public void Publish<T>(T eventData, EventType channelType) where T : IEventData
        {
            if (!IsInitialized) throw new InvalidOperationException("EventManager 未初始化");
            if (eventData == null) throw new ArgumentNullException(nameof(eventData));
            EventBus.Publish(eventData, channelType);
        }

        #endregion

        #region 事件订阅

        public EventSubscription Subscribe<T>(Action<T> handler, EventPriority p = 0, object owner = null,
            bool isOnce = false) where T : IEventData
        {
            if (!IsInitialized) throw new InvalidOperationException("EventManager 未初始化");
            return EventBus?.Subscribe(handler, p, owner, isOnce);
        }

        public EventSubscription SubscribeAsync<T>(Func<T, Task> handler, EventPriority p = 0,
            object owner = null, bool isOnce = false) where T : IEventData
        {
            if (!IsInitialized) throw new InvalidOperationException("EventManager 未初始化");
            return EventBus?.SubscribeAsync(handler, p, owner, isOnce);
        }

        public EventSubscription SubscribeToChannel<T>(EventType channelType, Action<T> handler,
            EventPriority p = 0, object owner = null, bool isOnce = false) where T : IEventData
        {
            if (!IsInitialized) throw new InvalidOperationException("EventManager 未初始化");
            return EventBus?.Subscribe(channelType, handler, p, owner, isOnce);
        }

        public void Unsubscribe(EventSubscription subscription, EventType? channelType = null)
        {
            if (!IsInitialized) return;
            EventBus?.Unsubscribe(subscription, channelType);
        }

        public void UnsubscribeAll(object owner)
        {
            if (!IsInitialized) return;
            EventBus?.UnsubscribeAll(owner);
        }

        #endregion

        #region 通道管理

        public EventChannel GetChannel(EventType channelType)
        {
            return !IsInitialized
                ? throw new InvalidOperationException("EventManager 未初始化")
                : EventBus?.GetChannel(channelType);
        }

        public EventChannel CreateChannel(EventType channelType, EventPriority p = EventPriority.Normal,
            int maxQueueSize = 1000)
        {
            return !IsInitialized
                ? throw new InvalidOperationException("EventManager 未初始化")
                : EventBus?.CreateChannel(channelType, p, maxQueueSize);
        }

        #endregion

        #region 内部处理

        public void ProcessChannels(int maxEventsPerFrame)
        {
            if (!IsInitialized || !IsEnable) return;
            EventBus?.ProcessAllChannels(maxEventsPerFrame);
        }

        #endregion
    }
}