using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BH.Framework.Enums;
using BH.Framework.Infrastructure.Events.Interfaces;
using Debug = UnityEngine.Debug;

namespace BH.Framework.Infrastructure.Events.Tools
{
    /// <summary>
    /// 事件性能分析器
    /// </summary>
    public class EventProfiler : IDisposable
    {
        private Dictionary<EventType, ProfilerData> _profilerData = new();
        private Stopwatch _stopwatch = new Stopwatch();

        private class ProfilerData
        {
            public EventType EventType { get; set; }
            public long TotalCalls { get; set; }
            public long TotalMilliseconds { get; set; }
            public long MaxMilliseconds { get; set; }
            public long MinMilliseconds { get; set; } = long.MaxValue;
            public DateTime LastCallTime { get; set; }
            
            public double AverageMilliseconds => TotalCalls > 0 ? (double)TotalMilliseconds / TotalCalls : 0;
        }
        
        public EventProfiler()
        {
            _stopwatch.Start();
        }
        
        public void OnEventPublished(IEventData eventData)
        {
            if (eventData == null) return;
            
            var eventType = eventData.EventType;
            
            if (!_profilerData.TryGetValue(eventType, out var data))
            {
                data = new ProfilerData { EventType = eventType };
                _profilerData[eventType] = data;
            }
            
            // 这里可以记录事件处理时间
            // 需要在EventBus中配合使用
        }
        
        /// <summary>
        /// 获取性能报告
        /// </summary>
        public string GetPerformanceReport()
        {
            if (_profilerData.Count == 0) return "无性能数据";
            
            var report = "=== 事件性能报告 ===\n";
            
            foreach (var kvp in _profilerData.OrderByDescending(p => p.Value.TotalMilliseconds))
            {
                var data = kvp.Value;
                report += $"{data.EventType}:\n" +
                         $"  调用次数: {data.TotalCalls}\n" +
                         $"  总耗时: {data.TotalMilliseconds}ms\n" +
                         $"  平均耗时: {data.AverageMilliseconds:F2}ms\n" +
                         $"  最大耗时: {data.MaxMilliseconds}ms\n" +
                         $"  最小耗时: {(data.MinMilliseconds == long.MaxValue ? 0 : data.MinMilliseconds)}ms\n";
            }
            
            return report;
        }
        
        /// <summary>
        /// 打印性能报告
        /// </summary>
        public void PrintPerformanceReport()
        {
            Debug.Log(GetPerformanceReport());
        }
        
        /// <summary>
        /// 检查性能问题
        /// </summary>
        public List<string> CheckPerformanceIssues(double thresholdMs = 10.0)
        {
            var issues = new List<string>();
            
            foreach (var kvp in _profilerData)
            {
                var data = kvp.Value;
                
                if (data.AverageMilliseconds > thresholdMs)
                {
                    issues.Add($"事件 '{data.EventType}' 平均处理时间 {data.AverageMilliseconds:F2}ms 超过阈值 {thresholdMs}ms");
                }
                
                if (data.TotalCalls > 1000 && data.TotalMilliseconds > 1000)
                {
                    issues.Add($"事件 '{data.EventType}' 高频调用 ({data.TotalCalls}次)，总耗时 {data.TotalMilliseconds}ms");
                }
            }
            
            return issues;
        }
        
        public void Dispose()
        {
            _profilerData.Clear();
            _stopwatch.Stop();
        }
    }
}