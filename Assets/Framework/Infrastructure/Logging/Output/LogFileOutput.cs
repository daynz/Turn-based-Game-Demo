using System;
using System.IO;
using BH.Framework.Infrastructure.Logging.Interfaces;
using UnityEngine;

namespace BH.Framework.Infrastructure.Logging.Output
{
    public class LogFileOutput : ILogOutput
    {
        private readonly string _path;
        public LogFileOutput(string fileName = "BH6Log.log")
        {
            _path = Path.Combine(Application.persistentDataPath, fileName);
        }
    
        public void Output(ILogEntry entry)
        {
            string line = $"{entry.Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{entry.Level}] [{entry.Type}] {entry.Message}";
            if (entry.StackTrace != null) line += $"\n{entry.StackTrace}";
            File.AppendAllText(_path, line + Environment.NewLine);
        }
    }
}