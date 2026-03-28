using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using BH.Framework.Enums;
using BH.Framework.Infrastructure.Logging.Interfaces;
using BH.Framework.Infrastructure.Logging.Output;
using UnityEngine;
using LogType = BH.Framework.Enums.LogType;

namespace BH.Framework.Infrastructure.Logging.Core
{
    /// <summary>
    /// 全局日志管理器
    /// </summary>
    [Serializable]
    public class LogService : ILogService
    {
        private ConcurrentQueue<LogEntry> _logQueue = new();
        private List<LogEntry> _logList;
        [SerializeField] private LogConfig config = new();
        [SerializeField] private LogConfig defaultConfig = new();
        [SerializeField] private bool enableLogService = true;
        [SerializeField] private int maxCapacity = 5000;
        [SerializeField] private LogLevel minDisplayLevel = LogLevel.Info;

        private List<ILogOutput> _outputs = new();
        private static readonly object Lock = new();

        public int MaxCapacity
        {
            get => maxCapacity;
            set => maxCapacity = value;
        }

        public void Initialize()
        {
            _outputs.Add(new LogConsoleOutput());
            _logList = new List<LogEntry>(maxCapacity);
            config = defaultConfig;
            Info("[LogService] 初始化完成");
        }

        /// <summary>
        /// 记录日志（带初始化状态检查）
        /// </summary>
        public void Log(LogEntry entry)
        {
            if (entry == null) return;
            if (_logQueue == null)
            {
                UnityEngine.Debug.LogWarning($"[LogService] 日志系统尚未初始化，消息被忽略: {entry.Message}");
                return;
            }

            if (entry.Level < minDisplayLevel)
            {
                return;
            }

            lock (Lock)
            {
                _logQueue.Enqueue(entry);

                foreach (var output in _outputs)
                {
                    try
                    {
                        output.Output(entry);
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogException(e);
                    }
                }
            }

            lock (_logList)
            {
                _logList.Add(entry);
                if (_logList.Count > maxCapacity)
                {
                    _logList.RemoveRange(0, _logList.Count - maxCapacity);
                }
            }
        }

        // ----- 便捷方法（链式调用）-----
        public void Debug(string message, LogType type = LogType.General)
            => Log(new LogBuilder()
                .SetLevel(LogLevel.Debug)
                .SetCategory(type)
                .SetMessage(message)
                .Build()
            );

        public void Info(string message,
            LogType type = LogType.General)
            => Log(new LogBuilder()
                .SetLevel(LogLevel.Info)
                .SetCategory(type)
                .SetMessage(message)
                .Build());

        public void Warning(string message,
            LogType type = LogType.General)
            => Log(new LogBuilder()
                .SetLevel(LogLevel.Warning)
                .SetCategory(type)
                .SetMessage(message)
                .Build());

        public void Error(string message, string stackTrace = null, LogType type = LogType.General)
            => Log(new LogBuilder()
                .SetLevel(LogLevel.Error)
                .SetCategory(type)
                .SetMessage(message)
                .SetStackTrace(stackTrace ?? Environment.StackTrace)
                .Build()
            );

        public void Critical(string message, string stackTrace = null, LogType type = LogType.General)
            => Log(new LogBuilder()
                .SetLevel(LogLevel.Critical)
                .SetCategory(type)
                .SetMessage(message)
                .SetStackTrace(stackTrace ?? Environment.StackTrace)
                .Build()
            );

        public void EventLog(string message, bool enable = true,
            LogType type = LogType.Event)
        {
            if (!enable) return;
            Log(new LogBuilder()
                .SetLevel(LogLevel.Event)
                .SetCategory(type)
                .SetMessage(message)
                .Build()
            );
        }

        public void EventLogError(string message, bool enable = true,
            LogType type = LogType.Event,
            string stackTrace = null)
        {
            if (!enable) return;
            Log(new LogBuilder()
                .SetLevel(LogLevel.Error)
                .SetCategory(type)
                .SetMessage(message)
                .SetStackTrace(stackTrace ?? Environment.StackTrace)
                .Build()
            );
        }

        /// <summary>
        /// 获取所有日志（用于导出）
        /// </summary>
        public IReadOnlyList<LogEntry> GetAllLogs()
        {
            lock (_logList)
            {
                return _logList.AsReadOnly();
            }
        }

        /// <summary>
        /// 清除所有日志
        /// </summary>
        public void Clear()
        {
            lock (_logList)
            {
                _logQueue?.Clear();
                _logList.Clear();
            }
        }

        public void ExportLogToFile()
        {
            // TODO: 实现导出功能，可使用 LogExporter.ExportToHtml
        }

        public void UpdateConfig(LogConfig newConfig)
        {
            if (newConfig == null)
            {
                Debug("尝试用空值更新LogConfig");
                return;
            }

            config = newConfig;
        }

        public void Dispose()
        {
            Clear();
        }
    }
}