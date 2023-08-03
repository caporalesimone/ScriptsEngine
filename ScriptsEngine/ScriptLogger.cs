using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ScriptsEngine
{
    public class LogEntry
    {
        public enum ELogType
        {
            Error,
            Warning,
            Info,
        }

        private readonly string m_log_message;
        public ELogType LogType { get; }
        public string LogMessage
        {
            get
            {
                return LogType switch
                {
                    ELogType.Error => "Error: " + m_log_message,
                    ELogType.Warning => "Warning: " + m_log_message,
                    ELogType.Info => "Info: " + m_log_message,
                    _ => m_log_message,
                };
            }
        }
        public DateTime LogTime { get; }
        public LogEntry(ELogType log_type, string log_message)
        {
            LogType = log_type;
            m_log_message = log_message;
            LogTime = DateTime.Now;
        }
    }


    public class ScriptLogger : IEnumerable<LogEntry>
    {
        private ConcurrentQueue<LogEntry> m_logs = new();

        public void AddLog(LogEntry.ELogType logType, string message)
        {
            var logEntry = new LogEntry(logType, message);
            m_logs.Enqueue(logEntry);
        }
        public IEnumerator<LogEntry> GetEnumerator()
        {
            return m_logs.GetEnumerator();
        }
       
        public void Clear()
        {
            m_logs = new ConcurrentQueue<LogEntry>();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerable<LogEntry> GetLogsByType(LogEntry.ELogType logType)
        {
            foreach (var logEntry in m_logs)
            {
                if (logEntry.LogType == logType)
                {
                    yield return logEntry;
                }
            }
        }

        public LogEntry PopLog()
        {
            LogEntry log = null;
            m_logs.TryDequeue(out log);
            return log;
        }

        public void PopALogAndPrintIt()
        {
            if (m_logs.TryDequeue(out LogEntry logEntry))
            {
                Console.WriteLine($"[{logEntry.LogTime}] - {logEntry.LogMessage}");
            }
        }

    }


  }
