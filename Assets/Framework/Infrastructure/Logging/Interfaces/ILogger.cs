using System.Collections.Generic;
using BH.Framework.Enums;
using BH.Framework.Infrastructure.Logging.Core;

namespace BH.Framework.Infrastructure.Logging.Interfaces
{
    public interface ILogService
    {
        int MaxCapacity { get; }
        void Debug(string message, string owner, LogType type = LogType.General);
        void Info(string message, string owner, LogType type = LogType.General);
        void Warning(string message, string owner, LogType type = LogType.General);
        void Error(string message, string owner, LogType type = LogType.General, string stackTrace = null);
        void Critical(string message, string owner, LogType type = LogType.General, string stackTrace = null);
        void EventLog(string message, string owner, bool enable = true, LogType type = LogType.Event);
        void EventLogError(string message, string owner, bool enable = true, LogType type = LogType.Event,
            string stackTrace = null);
        IReadOnlyList<LogEntry> GetAllLogs();
        void Clear();
        void ExportLogToFile();
    }
}