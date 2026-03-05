using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using BH.Framework.Enums;
using BH.Framework.Infrastructure.DI.Attributes;
using BH.Framework.Infrastructure.DI.Interfaces;
using BH.Framework.Infrastructure.Logging.Interfaces;
using BH.Framework.Infrastructure.Logging.Output;
using BH.Framework.Interfaces;
using UnityEngine;
using IInitializable = Zenject.IInitializable;
using LogType = BH.Framework.Enums.LogType;

namespace BH.Framework.Infrastructure.Logging.Core
{
    /// <summary>
    /// 全局日志管理器
    /// </summary>
    [Serializable]
    public class LogService : ILogService, IInitializable
    {
        private ConcurrentQueue<LogEntry> _logQueue;
        private List<LogEntry> _logList = new();
        private static readonly object Lock = new();
        [SerializeField] private bool enableLogService = true;
        [SerializeField] private int maxCapacity = 5000;
        [SerializeField] private LogLevel minDisplayLevel = LogLevel.Info;

        private List<ILogOutput> _outputs;

        public int MaxCapacity
        {
            get => maxCapacity;
            set => maxCapacity = value;
        }

        public bool IsInitialized { get; private set; }

        //public int Priority => priority;
        //public string Name => GetType().Name;


        // public Task InitializeAsync()
        // {
        //     Init();
        //     return Task.CompletedTask;
        // }

        public LogService(IEnumerable<ILogOutput> outputs)
        {
            _outputs = new List<ILogOutput>(outputs);
        }

        public void Initialize()
        {
            if (IsInitialized)
                return;

            IsInitialized = true;
            _logQueue = new ConcurrentQueue<LogEntry>();
            _logList = new List<LogEntry>(maxCapacity);

            UnityEngine.Debug.Log("LogService 初始化完成");
            // 默认添加控制台输出器
            // _outputs.Add(new LogConsoleOutput());
            // _outputs.Add(new LogFileOutput());
        }

        /// <summary>
        /// 记录日志（带初始化状态检查）
        /// </summary>
        public void Log(LogEntry entry)
        {
            if (entry == null || !IsInitialized) return;
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
        public void Debug(string message, string owner, LogType type = LogType.General)
            => Log(new LogBuilder()
                .SetLevel(LogLevel.Debug)
                .SetCategory(type)
                .SetMessage(message)
                .SetOwner(owner)
                .Build()
            );

        public void Info(string message, string owner, LogType type = LogType.General)
            => Log(new LogBuilder()
                .SetLevel(LogLevel.Info)
                .SetCategory(type)
                .SetMessage(message)
                .SetOwner(owner)
                .Build());

        public void Warning(string message, string owner, LogType type = LogType.General)
            => Log(new LogBuilder()
                .SetLevel(LogLevel.Warning)
                .SetCategory(type)
                .SetMessage(message)
                .SetOwner(owner)
                .Build());

        public void Error(string message, string owner, LogType type = LogType.General, string stackTrace = null)
            => Log(new LogBuilder()
                .SetLevel(LogLevel.Error)
                .SetCategory(type)
                .SetMessage(message)
                .SetStackTrace(stackTrace ?? Environment.StackTrace)
                .SetOwner(owner)
                .Build()
            );

        public void Critical(string message, string owner, LogType type = LogType.General, string stackTrace = null)
            => Log(new LogBuilder()
                .SetLevel(LogLevel.Critical)
                .SetCategory(type)
                .SetMessage(message)
                .SetStackTrace(stackTrace ?? Environment.StackTrace)
                .SetOwner(owner)
                .Build()
            );

        public void EventLog(string message, string owner, bool enable = true, LogType type = LogType.Event)
        {
            if (!enable) return;
            Log(new LogBuilder()
                .SetLevel(LogLevel.Event)
                .SetCategory(type)
                .SetMessage(message)
                .SetOwner(owner)
                .Build()
            );
        }

        public void EventLogError(string message, string owner, bool enable = true, LogType type = LogType.Event,
            string stackTrace = null)
        {
            if (!enable) return;
            Log(new LogBuilder()
                .SetLevel(LogLevel.Error)
                .SetCategory(type)
                .SetMessage(message)
                .SetOwner(owner)
                .SetStackTrace(stackTrace ?? Environment.StackTrace)
                .Build()
            );
        }

        // public void AddOutput(ILogOutput output)
        // {
        //     if (!IsInitialized) return;
        //     if (!_outputs.Contains(output)) _outputs.Add(output);
        // }

        //public void RemoveOutput(ILogOutput output) => _outputs.Remove(output);

        /// <summary>
        /// 获取所有日志（用于导出）
        /// </summary>
        public IReadOnlyList<LogEntry> GetAllLogs()
        {
            if (!IsInitialized) return null;
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
            if (!IsInitialized) return;
            lock (_logList)
            {
                _logQueue?.Clear();
                _logList.Clear();
            }
        }

        [ContextMenu("导出到文件")]
        public void ExportLogToFile()
        {
            if (!IsInitialized) return;
            // TODO: 实现导出功能，可使用 LogExporter.ExportToHtml
        }

        public void Shutdown()
        {
            if (!IsInitialized) return;
            Clear();
        }
    }
}