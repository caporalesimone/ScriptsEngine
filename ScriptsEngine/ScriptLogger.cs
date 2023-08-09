using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ScriptEngine.Logger
{
    using System;
    using System.Collections.Concurrent;
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
        public string FullMessage { get => $"{LogTime:G}-[{LogLevel}]:{Message}"; }
        
        public LogEventArgs(LogLevel logLevel, string message)
        {
            LogTime = DateTime.Now;
            LogLevel = logLevel;
            Message = message;
        }
    }

    public class SELogger : IDisposable
    {
        private readonly ConcurrentQueue<LogEventArgs> logQueue = new(); // Queue of logs
        public event EventHandler<LogEventArgs> LogEvent;

        private readonly object queueLock = new();
        private bool isProcessingQueue;

        private readonly bool logToFile;
        private readonly string logFilePath;

        private readonly bool run_logger_dequeuer;

        private readonly Thread thread_logger_dequeuer;

        public string LogFilePath => logFilePath;
        public bool IsLoggingInProgress => isProcessingQueue;
        public int LogQueueSize => logQueue.Count;
        public int Subscribers => LogEvent?.GetInvocationList().Length ?? 0;

        /// <summary>
        /// Constructor for the SELogger class.
        /// </summary>
        /// <param name="logToFile">Flag to enable log storage to file.</param>
        public SELogger(bool logToFile)
        {
            this.logToFile = logToFile;
            if (logToFile)
            {
                logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{DateTime.Now:yyyyMMdd_HHmmss}.log");
            }

            run_logger_dequeuer = true;

            thread_logger_dequeuer = new(ProcessLogQueue);
            thread_logger_dequeuer.Start();
        }

        public void Dispose()
        {
            thread_logger_dequeuer.Abort();
        }

        /// <summary>
        /// Add a new log with the specified log level and message.
        /// </summary>
        /// <param name="logLevel">The log level of the log entry.</param>
        /// <param name="message">The log message.</param>
        public void AddLog(LogLevel logLevel, string message)
        {
            // If there are no subscribers and not logging to file, exit immediately
            if ( (LogEvent == null || LogEvent?.GetInvocationList().Length == 0) && !logToFile)
            {
                return;
            }

            LogEventArgs newEvent = new(logLevel, message);

            lock (queueLock)
            {
                isProcessingQueue = true;
                logQueue.Enqueue(newEvent);
                Monitor.Pulse(queueLock);
            }
        }

        /// <summary>
        /// This function is a thread that processes the queue.
        /// It's syncronized with AddLog function
        /// </summary>
        private void ProcessLogQueue()
        {
            while (run_logger_dequeuer)
            {
                LogEventArgs newEvent;
                bool bDequeued = false;

                lock (queueLock)
                {
                    while (logQueue.Count == 0)
                    {
                        isProcessingQueue = false;
                        Monitor.Wait(queueLock);
                    }
                    bDequeued = logQueue.TryDequeue(out newEvent);
                }
                if (bDequeued)
                {
                    LogEvent?.Invoke(this, newEvent);

                    if (logToFile)
                    {
                        File.AppendAllText(logFilePath, newEvent.FullMessage + Environment.NewLine);
                    }
                }
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