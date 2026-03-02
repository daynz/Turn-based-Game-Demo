using System;
using System.Collections.Generic;
using BH.Framework.Enums;

namespace BH.Framework.Infrastructure.Logging.Core
{
    public class LogBuilder
    {
        private readonly LogEntry _entry = new();

        public LogBuilder SetOwner(string owner)
        {
            _entry.Owner = owner;
            return this;
        }
        
        public LogBuilder SetLevel(LogLevel level)
        {
            _entry.Level = level;
            return this;
        }

        public LogBuilder SetCategory(LogType category)
        {
            _entry.Type = category;
            return this;
        }

        public LogBuilder SetMessage(string message)
        {
            _entry.Message = message;
            return this;
        }

        public LogBuilder SetTimestamp(DateTime time)
        {
            _entry.Timestamp = time;
            return this;
        }

        public LogBuilder SetStackTrace(string stackTrace)
        {
            _entry.StackTrace = stackTrace;
            return this;
        }

        public LogBuilder AddContext(string key, object value)
        {
            if (_entry.Context == null) _entry.Context = new Dictionary<string, object>();
            _entry.Context[key] = value;
            return this;
        }

        public LogEntry Build()
        {
            _entry.Id = Guid.NewGuid().ToString();
            if (_entry.Timestamp == DateTime.MinValue)
                _entry.Timestamp = DateTime.Now;
            _entry.TimestampUnixMs = new DateTimeOffset(_entry.Timestamp).ToUnixTimeMilliseconds();
            return _entry;
        }
    }
}