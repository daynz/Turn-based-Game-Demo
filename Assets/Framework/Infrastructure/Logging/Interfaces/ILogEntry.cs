using System;
using System.Collections.Generic;
using BH.Framework.Enums;

namespace BH.Framework.Infrastructure.Logging.Interfaces
{
    public interface ILogEntry
    {
        public string Id { get; set; }
        public string Owner { get; set; }
        public DateTime Timestamp { get; set; }
        public long TimestampUnixMs { get; set; }
        public LogLevel Level { get; set; }
        public LogType Type { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public Dictionary<string, object> Context { get; set; }
    }
}