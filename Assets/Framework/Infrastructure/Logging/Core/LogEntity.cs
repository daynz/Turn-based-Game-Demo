using System;
using System.Collections.Generic;
using BH.Framework.Enums;
using BH.Framework.Infrastructure.Logging.Interfaces;

namespace BH.Framework.Infrastructure.Logging.Core
{
    /// <summary>
    /// 日志条目
    /// </summary>
    public class LogEntry : ILogEntry
    {
        /// <summary>
        /// 日志记录唯一标识
        /// </summary>
        public string Id { get; set; }

        public string Owner { get; set; }
        
        /// <summary>
        /// 日志记录时间（本地时间）
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// 日志记录时间戳（毫秒级Unix时间）
        /// </summary>
        public long TimestampUnixMs { get; set; }

        /// <summary>
        /// 日志级别
        /// </summary>
        public LogLevel Level { get; set; }

        /// <summary>
        /// 日志分类类型
        /// </summary>
        public LogType Type { get; set; }

        /// <summary>
        /// 日志消息内容
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 异常堆栈跟踪信息
        /// </summary>
        public string StackTrace { get; set; }

        /// <summary>
        /// 扩展上下文
        /// </summary>
        public Dictionary<string, object> Context { get; set; }

        public LogEntry()
        {
        }
    }
}