using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScriptEngine.Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace UnitTests
{
    [TestClass]
    public class Tests_ScriptsEngineLogger
    {
        [TestMethod]
        public void LoggerInitialization()
        {
            SELogger logger = new SELogger(logToFile: false);
            Assert.IsNotNull(logger);

            // Check that filename dosn't change every time (Happened for my mistake)
            string filename1 = logger.LogFilePath;
            Thread.Sleep(1000);
            string filename2 = logger.LogFilePath;
            Assert.AreEqual(filename1, filename2);

            logger.Dispose();
        }

        [TestMethod]
        public void LoggerInitializationWithLogToFile()
        {
            SELogger logger = new SELogger(logToFile: true);
            Assert.IsNotNull(logger);

            // No logs added. I expect no files exists
            Assert.IsFalse(File.Exists(logger.LogFilePath));

            // Add a log and check if file exists
            logger.AddLog(LogLevel.Debug, "debug log");

            Thread.Sleep(1000); // Giving time to create the file because happen in a different thread
            Debug.WriteLine(logger.LogFilePath);
            Assert.IsTrue(File.Exists(logger.LogFilePath));

            File.Delete(logger.LogFilePath);

            logger.Dispose();
        }

        [TestMethod]
        public void AddLogsWithoutSubscriber()
        {
            SELogger logger = new SELogger(logToFile: false);

            for (int i = 0; i < 100; i++)
            {
                logger.AddLog(LogLevel.Debug, i.ToString());
            }
            Thread.Sleep(200);
            Debug.WriteLine("Subscribers: " + logger.Subscribers);
            Debug.WriteLine("Queue length: " + logger.LogQueueSize);
            Assert.IsTrue(logger.LogQueueSize == 0);
            Assert.IsFalse(logger.IsLoggingInProgress);

            logger.Dispose();
        }

        [TestMethod]
        public void AddLogsWithoutSubscriberButLogToFile()
        {
            SELogger logger = new SELogger(logToFile: true);
            Debug.WriteLine(logger.LogFilePath);

            for (int i = 0; i < 100; i++)
            {
                logger.AddLog(LogLevel.Debug, i.ToString());
            }
            Assert.IsFalse(logger.LogQueueSize == 0);
            Assert.IsTrue(logger.IsLoggingInProgress);
            Debug.WriteLine("Subscribers: " + logger.Subscribers);
            Debug.WriteLine("Queue length: " + logger.LogQueueSize);
            while(logger.LogQueueSize > 0)
            {
                Thread.Sleep(10);
            }
            Assert.IsTrue(logger.LogQueueSize == 0);
            Thread.Sleep(10);
            Assert.IsFalse(logger.IsLoggingInProgress);
            Assert.IsTrue(File.Exists(logger.LogFilePath));

            File.Delete(logger.LogFilePath);

            logger.Dispose();
        }



        [TestMethod]
        public void AddALog()
        {
            SELogger logger = new SELogger(logToFile: false);

            LogLevel expectedLogLevel = LogLevel.Info;
            string expectedMessage = "Test log message";
            ManualResetEventSlim eventReceived = new ManualResetEventSlim(false);

            logger.LogEvent += (sender, e) =>
            {
                Assert.AreEqual(expectedLogLevel, e.LogLevel);
                Assert.AreEqual(expectedMessage, e.Message);
                eventReceived.Set();
            };

            logger.AddLog(expectedLogLevel, expectedMessage);

            // Wait for the event to be received or timeout after 1 seconds
            bool eventReceivedResult = eventReceived.Wait(TimeSpan.FromSeconds(1));

            Assert.IsTrue(eventReceivedResult, "Log event notification was not received within the timeout.");

            logger.Dispose();
        }

        [TestMethod]
        public void AddLog_100000x()
        {
            SELogger logger = new SELogger(logToFile: false);

            int total_logs = 100000;
            int received_logs = 0;
            int sent_logs = 0;

            LogLevel expectedLogLevel = LogLevel.Info;
            ManualResetEventSlim eventReceived = new ManualResetEventSlim(false);

            logger.LogEvent += (sender, e) =>
            {
                Assert.AreEqual(expectedLogLevel, e.LogLevel);
                Assert.AreEqual(received_logs.ToString(), e.Message);

                received_logs++;
                if (received_logs == total_logs) 
                {
                    eventReceived.Set();
                }
            };

            for (int i = 0; i < total_logs; i++)
            {
                logger.AddLog(expectedLogLevel, i.ToString());
                sent_logs++;
            }

            // Wait for the event to be received or timeout after 1 seconds
            bool eventReceivedResult = eventReceived.Wait(TimeSpan.FromSeconds(1));

            Debug.WriteLine($"Sent: {sent_logs}");
            Debug.WriteLine($"Recv: {received_logs}");
            Assert.IsTrue(received_logs == total_logs);
            Assert.IsTrue(received_logs == sent_logs);
            Assert.IsTrue(eventReceivedResult, "Log event notification was not received within the timeout.");

            logger.Dispose();
        }

        [TestMethod]
        public void AddLog_100000x_2_Consumers()
        {
            SELogger logger = new SELogger(logToFile: false);

            int total_logs = 100000;
            int received_logs_1 = 0;
            int received_logs_2 = 0;
            int sent_logs = 0;

            LogLevel expectedLogLevel = LogLevel.Info;
            ManualResetEventSlim eventReceived = new ManualResetEventSlim(false);

            logger.LogEvent += (sender, e) =>
            {
                Assert.AreEqual(expectedLogLevel, e.LogLevel);
                Assert.AreEqual(received_logs_1.ToString(), e.Message);

                received_logs_1++;
                if (received_logs_1 == total_logs)
                {
                    eventReceived.Set();
                }
            };

            logger.LogEvent += (sender, e) =>
            {
                Assert.AreEqual(expectedLogLevel, e.LogLevel);
                Assert.AreEqual(received_logs_2.ToString(), e.Message);

                received_logs_2++;
                if (received_logs_2 == total_logs)
                {
                    eventReceived.Set();
                }
            };

            for (int i = 0; i < total_logs; i++)
            {
                logger.AddLog(expectedLogLevel, i.ToString());
                sent_logs++;
            }

            // Wait for the event to be received or timeout after 1 seconds
            bool eventReceivedResult = eventReceived.Wait(TimeSpan.FromSeconds(1));

            Debug.WriteLine($"Sent: {sent_logs}");
            Debug.WriteLine($"Recv_1: {received_logs_1}");
            Debug.WriteLine($"Recv_2: {received_logs_2}");
            Assert.IsTrue(received_logs_1 == total_logs);
            Assert.IsTrue(received_logs_2 == total_logs);
            Assert.IsTrue(received_logs_1 == sent_logs);
            Assert.IsTrue(received_logs_2 == sent_logs);
            Assert.IsTrue(eventReceivedResult, "Log event notification was not received within the timeout.");

            logger.Dispose();
        }

        /// <summary>
        /// Sends logs at maximum speed while the log consumer sleeps for each log
        /// in this case the logger should grow his queue
        /// </summary>
        [TestMethod]
        public void AddLog_10x_SlowQueue()
        {
            SELogger logger = new SELogger(logToFile: false);

            int total_logs = 10;
            int received_logs = 0;
            int sent_logs = 0;

            LogLevel expectedLogLevel = LogLevel.Info;
            ManualResetEventSlim eventReceived = new ManualResetEventSlim(false);

            DateTime start = DateTime.Now;

            logger.LogEvent += (sender, e) =>
            {
                Thread.Sleep(50);
                Assert.AreEqual(expectedLogLevel, e.LogLevel);
                Assert.AreEqual(received_logs.ToString(), e.Message);
                Debug.WriteLine($"Logger 2: {logger.LogQueueSize} - {DateTime.Now - start}");

                received_logs++;
                if (received_logs == total_logs)
                {
                    eventReceived.Set();
                }
            };

            
            for (int i = 0; i < total_logs; i++)
            {
                logger.AddLog(expectedLogLevel, i.ToString());
                sent_logs++;
            }
            Debug.WriteLine($"Sent {total_logs} in {DateTime.Now - start} seconds");

            Assert.IsTrue(logger.IsLoggingInProgress);
            Thread.Sleep(500);
            Assert.IsTrue(logger.IsLoggingInProgress);
            Assert.IsFalse(eventReceived.IsSet);

            // Wait for the event to be received or timeout after 1 seconds
            bool eventReceivedResult = eventReceived.Wait(TimeSpan.FromSeconds(15));

            Debug.WriteLine($"Sent: {sent_logs}");
            Debug.WriteLine($"Recv: {received_logs}");
            Assert.IsTrue(received_logs == total_logs);
            Assert.IsTrue(received_logs == sent_logs);
            Assert.IsTrue(eventReceivedResult, "Log event notification was not received within the timeout.");

            logger.Dispose();
        }

    }
}
