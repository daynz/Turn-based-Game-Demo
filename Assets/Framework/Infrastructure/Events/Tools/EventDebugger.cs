using System;

namespace BH.Framework.Infrastructure.Events.Tools
{
    /// <summary>
    /// 事件调试器：监听所有通道的事件，统计和记录
    /// </summary>
    public class EventDebugger  : IDisposable
    {
        // #region 字段
        //
        // private readonly EventBus _eventBus;
        // private bool _isEnabled;
        // private readonly Dictionary<EventType, int> _eventCounts = new();
        // private readonly List<string> _eventLog = new();
        // private readonly HashSet<Guid> _processedEventIds = new(); // 用于去重
        // private readonly int _maxLogSize = 1000;
        // private readonly List<IDisposable> _subscriptions = new(); // 用于取消订阅通道事件
        // [Inject] private LogService _logService;
        //
        // #endregion
        //
        // #region 构造与启用
        //
        // /// <summary>
        // /// 构造函数
        // /// </summary>
        // /// <param name="eventBus">事件总线实例</param>
        // /// <param name="enableOnCreate">是否在创建后自动启用调试</param>
        // public EventDebugger(EventBus eventBus, bool enableOnCreate = true)
        // {
        //     _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        //     if (enableOnCreate)
        //         StartDebugging();
        // }
        //
        // /// <summary>
        // /// 启动调试
        // /// </summary>
        // public void StartDebugging()
        // {
        //     if (_isEnabled) return;
        //
        //     var channels = _eventBus.GetAllChannels();
        //     foreach (var channel in channels)
        //     {
        //         if (channel == null) continue;
        //
        //         // 订阅通道的入队事件和处理事件
        //         // EventHandler<IEventData> onQueued = OnEventPublished;
        //         // EventHandler<IEventData> onProcessed = OnEventPublished;
        //         //
        //         // channel.OnEventQueued += onQueued;
        //         // channel.OnEventProcessed += onProcessed;
        //
        //         // 保存委托引用以便取消订阅
        //         // _subscriptions.Add(new ChannelEventSubscription(channel, onQueued, onProcessed));
        //     }
        //
        //     _isEnabled = true;
        //     _logService.Info("[EventDebugger] 事件调试已启动");
        // }
        //
        // /// <summary>
        // /// 停止调试
        // /// </summary>
        // public void StopDebugging()
        // {
        //     if (!_isEnabled) return;
        //
        //     // 取消订阅所有通道事件
        //     foreach (var sub in _subscriptions)
        //     {
        //         sub.Dispose();
        //     }
        //
        //     _subscriptions.Clear();
        //     _processedEventIds.Clear();
        //     _isEnabled = false;
        //     _logService.Info("[EventDebugger] 事件调试已停止");
        // }
        //
        // #endregion
        //
        // #region 事件处理
        //
        // private void OnEventPublished(object sender, IEventData eventData)
        // {
        //     if (!_isEnabled || eventData == null) return;
        //
        //     // 去重：同一事件可能同时触发 Queued 和 Processed，只记录一次
        //     if (!_processedEventIds.Add(eventData.EventId))
        //         return;
        //
        //     var eventType = eventData.EventType;
        //
        //     // 统计事件数量
        //     _eventCounts.TryGetValue(eventType, out var count);
        //     _eventCounts[eventType] = count + 1;
        //
        //     // 记录事件日志
        //     var logEntry = $"[{DateTime.Now:HH:mm:ss.fff}] {eventType} " +
        //                    $"来自: {eventData.Sender?.GetType().Name ?? "null"} " +
        //                    $"处理: {eventData.IsHandled}";
        //     _eventLog.Add(logEntry);
        //
        //     // 限制日志大小
        //     if (_eventLog.Count > _maxLogSize)
        //         _eventLog.RemoveAt(0);
        //
        //     // 输出高频事件警告
        //     if (count + 1 > 1000)
        //     {
        //         _logService.Warning($"高频事件警告: {eventType} 已触发 {count + 1} 次，可能存在性能问题");
        //     }
        // }
        //
        // #endregion
        //
        // #region 统计输出
        //
        // /// <summary>
        // /// 打印事件统计
        // /// </summary>
        // public void PrintEventStats()
        // {
        //     Debug.Log("=== 事件调试统计 ===");
        //     var totalEvents = _eventCounts.Values.Sum();
        //     Debug.Log($"总事件数: {totalEvents}");
        //     Debug.Log($"事件类型数: {_eventCounts.Count}");
        //
        //     foreach (var kvp in _eventCounts.OrderByDescending(k => k.Value).Take(10))
        //     {
        //         Debug.Log($"{kvp.Key}: {kvp.Value} 次");
        //     }
        // }
        //
        // /// <summary>
        // /// 打印最近事件
        // /// </summary>
        // /// <param name="count">要打印的事件数量</param>
        // public void PrintRecentEvents(int count = 20)
        // {
        //     Debug.Log($"=== 最近 {Math.Min(count, _eventLog.Count)} 个事件 ===");
        //     var recentLogs = _eventLog.Skip(Math.Max(0, _eventLog.Count - count)).ToList();
        //     foreach (var log in recentLogs)
        //     {
        //         Debug.Log(log);
        //     }
        // }
        //
        // #endregion
        //
        // #region IDisposable 实现
        //
        public void Dispose()
        {
            // StopDebugging();
            // _eventCounts.Clear();
            // _eventLog.Clear();
            // _processedEventIds.Clear();
        }
        //
        // #endregion
        //
        // #region 辅助类
        //
        // /// <summary>
        // /// 辅助类：用于保存通道事件订阅，以便取消
        // /// </summary>
        // private class ChannelEventSubscription : IDisposable
        // {
        //     private readonly EventChannel _channel;
        //     private readonly EventHandler<IEventData> _onQueued;
        //     private readonly EventHandler<IEventData> _onProcessed;
        //
        //     public ChannelEventSubscription(EventChannel channel, EventHandler<IEventData> onQueued,
        //         EventHandler<IEventData> onProcessed)
        //     {
        //         _channel = channel;
        //         _onQueued = onQueued;
        //         _onProcessed = onProcessed;
        //     }
        //
        //     public void Dispose()
        //     {
        //         // _channel.OnEventQueued -= _onQueued;
        //         // _channel.OnEventProcessed -= _onProcessed;
        //     }
    // }
    //
    // #endregion
}

}