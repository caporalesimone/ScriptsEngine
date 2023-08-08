using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScriptEngine.Logger;
using System;
using System.IO;
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
            Thread.Sleep(100); // Giving time to create the file because happen in a different thread
            Assert.IsTrue(File.Exists(logger.LogFilePath));

            File.Delete(logger.LogFilePath);
        }

        [TestMethod]
        public void AddALog()
        {
            SELogger logger = new SELogger(logToFile: false);

            LogLevel expectedLogLevel = LogLevel.Info;
            string expectedMessage = "Test log message";
            ManualResetEventSlim eventReceived = new ManualResetEventSlim(false);

            logger.NewLog += (sender, e) =>
            {
                Assert.AreEqual(expectedLogLevel, e.LogLevel);
                Assert.AreEqual(expectedMessage, e.Message);
                eventReceived.Set();
            };

            logger.AddLog(expectedLogLevel, expectedMessage);

            // Wait for the event to be received or timeout after 1 seconds
            bool eventReceivedResult = eventReceived.Wait(TimeSpan.FromSeconds(1));

            Assert.IsTrue(eventReceivedResult, "Log event notification was not received within the timeout.");
        }

    }
}
