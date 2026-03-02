using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BH.Framework.Infrastructure.Events.Interfaces;
using UnityEngine;
using EventType = BH.Framework.Enums.EventType;

namespace BH.Framework.Infrastructure.Events.Tools
{
    /// <summary>
    /// 事件记录器
    /// </summary>
    public class EventRecorder : IDisposable
    {
        private List<EventRecord> _records = new List<EventRecord>();
        private int _maxRecords = 10000;
        private bool _isRecording = true;
        
        public class EventRecord
        {
            public Guid EventId { get; set; }
            public EventType EventType { get; set; }
            public string Sender { get; set; }
            public DateTime Timestamp { get; set; }
            public bool IsHandled { get; set; }
            public string Data { get; set; }
            
            public override string ToString()
            {
                return $"{Timestamp:HH:mm:ss.fff} [{EventType}] 来自: {Sender}, 处理: {IsHandled}, 数据: {Data}";
            }
        }
        
        public EventRecorder()
        {
            _isRecording = true;
        }
        
        /// <summary>
        /// 记录事件
        /// </summary>
        public void RecordEvent(IEventData eventData)
        {
            if (!_isRecording || eventData == null) return;
            
            var record = new EventRecord
            {
                EventId = eventData.EventId,
                EventType = eventData.EventType,
                Sender = eventData.Sender?.GetType().Name ?? "Unknown",
                Timestamp = eventData.Timestamp,
                IsHandled = eventData.IsHandled,
                Data = eventData.Data?.ToString() ?? "null"
            };
            
            _records.Add(record);
            
            // 限制记录数量
            if (_records.Count > _maxRecords)
            {
                _records.RemoveAt(0);
            }
        }
        
        /// <summary>
        /// 开始记录
        /// </summary>
        public void StartRecording()
        {
            _isRecording = true;
        }
        
        /// <summary>
        /// 停止记录
        /// </summary>
        public void StopRecording()
        {
            _isRecording = false;
        }
        
        /// <summary>
        /// 清空记录
        /// </summary>
        public void ClearRecords()
        {
            _records.Clear();
        }
        
        /// <summary>
        /// 获取事件记录
        /// </summary>
        public List<EventRecord> GetRecords(int count = 100)
        {
            return _records.TakeLast(Math.Min(count, _records.Count)).ToList();
        }
        
        /// <summary>
        /// 导出记录到文件
        /// </summary>
        public void ExportToFile(string filePath)
        {
            try
            {
                var lines = new List<string>
                {
                    "时间,事件ID,事件类型,发送者,是否处理,数据"
                };
                
                foreach (var record in _records)
                {
                    var line = $"{record.Timestamp:yyyy-MM-dd HH:mm:ss.fff}," +
                              $"{record.EventId}," +
                              $"{record.EventType}," +
                              $"{record.Sender}," +
                              $"{record.IsHandled}," +
                              $"\"{record.Data.Replace("\"", "\"\"")}\"";
                    lines.Add(line);
                }
                
                File.WriteAllLines(filePath, lines);
                Debug.Log($"事件记录已导出到: {filePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"导出事件记录失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 查找特定事件
        /// </summary>
        public List<EventRecord> FindEvents(EventType eventType, string sender = null)
        {
            return _records.Where(r => 
                r.EventType == eventType &&  // 枚举类型使用==进行精确匹配
                (sender == null || r.Sender.Contains(sender))
            ).ToList();
        }
        
        /// <summary>
        /// 分析事件序列
        /// </summary>
        public void AnalyzeEventSequence()
        {
            if (_records.Count < 2) return;
            
            // 分析事件之间的时间间隔
            var intervals = new List<TimeSpan>();
            for (int i = 1; i < _records.Count; i++)
            {
                intervals.Add(_records[i].Timestamp - _records[i-1].Timestamp);
            }
            
            var avgInterval = intervals.Average(i => i.TotalMilliseconds);
            var maxInterval = intervals.Max(i => i.TotalMilliseconds);
            var minInterval = intervals.Min(i => i.TotalMilliseconds);
            
            Debug.Log($"事件序列分析: 平均间隔 {avgInterval:F2}ms, 最大间隔 {maxInterval:F2}ms, 最小间隔 {minInterval:F2}ms");
        }
        
        public void Dispose()
        {
            ClearRecords();
        }
    }
}