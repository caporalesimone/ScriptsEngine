using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using static ScriptsEngine.LogEntry;

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
        private readonly ConcurrentQueue<LogEntry> m_logs = new();

        public void AddLog(ELogType logType, string message)
        {
            var logEntry = new LogEntry(logType, message);
            m_logs.Enqueue(logEntry);
        }
        public IEnumerator<LogEntry> GetEnumerator()
        {
            return m_logs.GetEnumerator();
        }
       
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public IEnumerable<LogEntry> GetLogsByType(ELogType logType)
        {
            foreach (var logEntry in m_logs)
            {
                if (logEntry.LogType == logType)
                {
                    yield return logEntry;
                }
            }
        }

        public void PopALogAndPrintIt()
        {
            if (m_logs.TryDequeue(out LogEntry logEntry))
            {
                Console.WriteLine($"[{logEntry.LogTime}] - {logEntry.LogMessage}");
            }
        }

    }




    /*
    public class ScriptLogger : IEnumerable<LogEntry>
    {
        //private readonly List<LogEntry> logs;
        private readonly Queue<LogEntry> m_logs;
        private readonly object lockObj = new();

        public ScriptLogger()
        {
            //logs = new List<LogEntry>();
            m_logs = new Queue<LogEntry>();
        }
        // Adds a new log in thread-safe
        public void AddLog(ELogType type, string message)
        {
            lock (lockObj)
            {
                m_logs.Enqueue(new LogEntry(type, message));
            }
        }
        // IEnumerable interface for iteates through all the log in a thread-safe way
        public IEnumerator<LogEntry> GetEnumerator()
        {
            List<LogEntry> copyLogs;
            lock (lockObj)
            {
                copyLogs = new List<LogEntry>(m_logs);
            }
            return copyLogs.GetEnumerator();
        }
        /// <summary>
        /// Retruns the Enumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        /// <summary>
        /// Iterates only through a specific log type. Is Thread-Safe
        /// </summary>
        /// <param name="type">Log Type</param>
        /// <returns>Returns the list of all filtered logs </returns>
        public IEnumerable<LogEntry> GetLogsByType(ELogType type)
        {
            List<LogEntry> filteredLogs;
            lock (lockObj)
            {
                filteredLogs = m_logs.FindAll(entry => entry.LogType == type);
            }
            return filteredLogs;
        }
        /// <summary>
        /// Checks if a specific LogType exists. Is Thread-Safe
        /// </summary>
        /// <param name="type">Log Type</param>
        /// <returns>returns true if logs are found</returns>
        public bool HasLogs(ELogType type)
        {
            lock (lockObj)
            {
                return m_logs.Exists(entry => entry.LogType == type);
            }
        }
        /// <summary>
        /// Deletes all the stored logs. Is Thread-Safe
        /// </summary>
        public void DeleteLogs()
        {
            lock(lockObj)
            {
                m_logs.Clear();
            }
        }

        /// <summary>
        /// Prints all logs into the Console. Is Thread-Safe
        /// </summary>
        public void PrintAllLogsInConsole()
        {
            List<LogEntry> log_copy;
            lock (lockObj)
            {
                log_copy = new List<LogEntry>(m_logs);
            }

            foreach (var item in log_copy)
            {
                Console.WriteLine($"[{item.LogTime.ToString()}] - {item.LogMessage}");
            }
        }

        public void PopALogAndPrint()
        {
            LogEntry entry = null;
            lock ( lockObj)
            {
                if (m_logs.Count > 0)
                    entry = m_logs.Dequeue();
            }

            if (entry != null)
            {
                Console.WriteLine($"[{entry.LogTime.ToString()}] - {entry.LogMessage}");
            }
            
        }
    */
}
