using System;
using System.Collections.Generic;
using BH.Framework.Enums;
using BH.Framework.Infrastructure.Logging.Core;
using Zenject;

namespace BH.Framework.Infrastructure.Logging.Interfaces
{
    public interface ILogService : IInitializable, IDisposable
    {
        int MaxCapacity { get; }

        void Debug(string message, LogType type = LogType.General);

        void Info(string message, LogType type = LogType.General);

        void Warning(string message, LogType type = LogType.General);

        void Error(string message, string stackTrace = null, LogType type = LogType.General);

        void Critical(string message, string stackTrace = null, LogType type = LogType.General);

        void EventLog(string message, bool enable = true, LogType type = LogType.Event);

        void EventLogError(string message, bool enable = true, LogType type = LogType.Event, string stackTrace = null);

        IReadOnlyList<LogEntry> GetAllLogs();
        void Clear();
        void ExportLogToFile();
        void UpdateConfig(LogConfig newConfig);
    }
}