using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ScriptEngine.Logger
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;

    public enum LogLevel
    {
        Debug,
        Error,
        Warning,
        Info
    }

    public delegate void LogEventHandler(object sender, LogEventArgs e);

    public class LogEventArgs : EventArgs
    {
        public DateTime LogTime { get; private set; }
        public LogLevel LogLevel { get; private set; }
        public string Message { get; private set; }

        public LogEventArgs(LogLevel logLevel, string message)
        {
            LogTime = DateTime.Now;
            LogLevel = logLevel;
            Message = message;
        }
    }

    public class SELogger
    {
        private readonly List<Action<object, LogEventArgs>> logSubscribers = new();
        private readonly bool logToFile;
        private readonly string logFilePath;

        /// <summary>
        /// Property to get the log file name.
        /// </summary>
        public string LogFilePath => logFilePath;

        /// <summary>
        /// Constructor for the SELogger class.
        /// </summary>
        /// <param name="logToFile">Flag to enable log storage to file.</param>
        public SELogger(bool logToFile)
        {
            this.logToFile = logToFile;
            if (logToFile)
            {
                logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{DateTime.Now:yyyyMMddHHmmss}.log");
            }
        }

        /// <summary>
        /// Subscribe a subscriber to log events.
        /// </summary>
        /// <param name="subscriber">The subscriber action to add.</param>
        public void Subscribe(Action<object, LogEventArgs> subscriber)
        {
            logSubscribers.Add(subscriber);
        }

        /// <summary>
        /// Unsubscribe a subscriber from log events.
        /// </summary>
        /// <param name="subscriber">The subscriber action to remove.</param>
        public void Unsubscribe(Action<object, LogEventArgs> subscriber)
        {
            logSubscribers.Remove(subscriber);
        }

        /// <summary>
        /// Add a new log with the specified log level and message.
        /// </summary>
        /// <param name="logLevel">The log level of the log entry.</param>
        /// <param name="message">The log message.</param>
        public void AddLog(LogLevel logLevel, string message)
        {
            // If there are no subscribers and not logging to file, exit immediately
            if (logSubscribers.Count == 0 && !logToFile)
            {
                return;
            }

            // Create a new log event
            LogEventArgs logEvent = new(logLevel, message);

            // If there are subscribers, notify them asynchronously
            if (logSubscribers.Count > 0)
            {
                ThreadPool.QueueUserWorkItem((state) =>
                {
                    foreach (var subscriber in logSubscribers)
                    {
                        subscriber.BeginInvoke(this, logEvent, null, null);
                    }
                });
            }

            // If logging to file is enabled, write the log to file
            if (logToFile)
            {
                string logText = $"{logEvent.LogTime:G} - [{logEvent.LogLevel}] - {logEvent.Message}{Environment.NewLine}";
                ThreadPool.QueueUserWorkItem((fileState) =>
                {
                    File.AppendAllText(logFilePath, logText);
                });
            }
        }
    }
}


/*
 class Program
    {
        static void Main(string[] args)
        {
            SELogger logger = new SELogger(logToFile: true);

            // Subscribe two consumers
            logger.Subscribe(ConsoleLogConsumer);
            logger.Subscribe(FileLogConsumer);

            logger.AddLog(LogLevel.Info, "This is an information log.");
            logger.AddLog(LogLevel.Error, "Error! Something went wrong.");
        }

        static void ConsoleLogConsumer(object sender, LogEventArgs e)
        {
            Console.WriteLine($"[{e.LogLevel}] {e.Message}");
        }

        static void FileLogConsumer(object sender, LogEventArgs e)
        {
            // Implement the logic to write to file, if necessary
        }
    }
*/