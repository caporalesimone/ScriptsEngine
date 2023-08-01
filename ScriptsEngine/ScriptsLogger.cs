using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ScriptsEngine.LogEntry;

namespace ScriptsEngine
{
    public class LogEntry
    {
        public enum E_LogType
        {
            Error,
            Warning,
            Info,
        }

        private readonly string m_log_message;
        public E_LogType LogType { get; }
        public string LogMessage
        {
            get
            {
                return LogType switch
                {
                    E_LogType.Error => "Error: " + m_log_message,
                    E_LogType.Warning => "Warning: " + m_log_message,
                    E_LogType.Info => "Info: " + m_log_message,
                    _ => m_log_message,
                };
            }
        }

        public DateTime LogTime { get; }

        public LogEntry(E_LogType log_type, string log_message)
        {
            LogType = log_type;
            m_log_message = log_message;
            LogTime = DateTime.Now;
        }

    }

    public class ScriptsLogger : IEnumerable<LogEntry>
    {
        private readonly List<LogEntry> logs;
        private readonly object lockObj = new();

        public ScriptsLogger()
        {
            logs = new List<LogEntry>();
        }

        // Adds a new log in thread-safe
        public void AddLog(E_LogType type, string message)
        {
            lock (lockObj)
            {
                logs.Add(new LogEntry(type, message));
            }
        }

        // IEnumerable interface for iteates through all the log in a thread-safe way
        public IEnumerator<LogEntry> GetEnumerator()
        {
            List<LogEntry> copyLogs;
            lock (lockObj)
            {
                copyLogs = new List<LogEntry>(logs);
            }
            return copyLogs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        // Iterates only specific log type in a thread-safe way
        public IEnumerable<LogEntry> GetLogsByType(E_LogType type)
        {
            List<LogEntry> filteredLogs;
            lock (lockObj)
            {
                filteredLogs = logs.FindAll(entry => entry.LogType == type);
            }
            return filteredLogs;
        }

        // Checks if a specific LogType exists in a thread-safe way
        public bool HasLogs(E_LogType type)
        {
            lock (lockObj)
            {
                return logs.Exists(entry => entry.LogType == type);
            }
        }
    }
}
