using System;
using BH.Framework.Enums;
using BH.Framework.Infrastructure.Logging.Interfaces;
using UnityEngine;

namespace BH.Framework.Infrastructure.Logging.Output
{
    /// <summary>
    /// Unity Console 输出器
    /// </summary>
    public class LogConsoleOutput : ILogOutput
    {
        public void Output(ILogEntry entry)
        {
            var prefix = $"[{entry.Timestamp:HH:mm:ss.fff}][{entry.Type}]";
            switch (entry.Level)
            {
                case LogLevel.Event:
                    Debug.Log($"<color=#FAC7FA>{entry.Message}</color> \n {prefix} {entry.TimestampUnixMs}");
                    break;
                case LogLevel.Debug:
                case LogLevel.Info:
                    Debug.Log($"<color=green>{entry.Message}</color> \n {prefix}");
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning($"<color=yellow>{entry.Message}</color> \n {prefix} \n {entry.StackTrace}");
                    break;
                case LogLevel.Error:
                case LogLevel.Critical:
                    Debug.LogError($"<color=red>{entry.Message}</color> \n {prefix} \n {entry.StackTrace}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}