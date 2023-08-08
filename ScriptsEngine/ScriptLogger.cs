﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ScriptEngine.Logger
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading;

    public enum LogLevel
    {
        Debug,
        Error,
        Warning,
        Info,
        Script
    }

    public delegate void LogEventHandler(object sender, LogEventArgs e);

    public class LogEventArgs : EventArgs
    {
        public DateTime LogTime { get; private set; }
        public LogLevel LogLevel { get; private set; }
        public string Message { get; private set; }
        public string FullMessage { get => LogTime.ToString() + " - " + LogLevel.ToString() + " : " + Message; }

        public LogEventArgs(LogLevel logLevel, string message)
        {
            LogTime = DateTime.Now;
            LogLevel = logLevel;
            Message = message;
        }
    }

    public class SELogger
    {
        //private readonly List<Action<object, LogEventArgs>> logSubscribers = new();
        private readonly List<EventHandler<LogEventArgs>> logSubscribers = new();
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
        /// This is the handler for the New Log Event. Subscribe here to be notified of a new log
        /// </summary>
        public event EventHandler<LogEventArgs> NewLog
        {
            add
            {
                logSubscribers.Add(value);
            }
            remove
            {
                logSubscribers.Remove(value);
            }
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

        /// <summary>
        /// This will redirect any Console message into the logger
        /// </summary>
        /// <param name="logLevel"></param>
        public void EnableConsoleOutputCapture(LogLevel logLevel)
        {
            LogTextWriter logTextWriter = new (this, logLevel);
            Console.SetOut(logTextWriter);
        }

        /// <summary>
        /// This will recover the Console
        /// </summary>
        public void DisableConsoleOutputCapure ()
        {
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        }

    }

    public class LogTextWriter : TextWriter
    {
        private readonly SELogger logger;
        private readonly LogLevel logLevel;

        public LogTextWriter(SELogger logger, LogLevel logLevel)
        {
            this.logger = logger;
            this.logLevel = logLevel;
        }

        public override void WriteLine(string value)
        {
            logger.AddLog(logLevel, value);
        }

        public override void Write(string value)
        {
            logger.AddLog(logLevel, value);
        }

        public override Encoding Encoding => Encoding.Default;
    }

}