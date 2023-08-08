using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScriptEngine;
using System;
using System.Diagnostics;
using System.Reflection;
using ScriptEngine.Logger;
using System.Threading;

namespace UnitTests
{
    [TestClass]
    public class Tests_CSharpCompiler
    {
        private readonly string base_path = @"TestScripts\CSharp\CSharpCompiler_Scripts\";

        [TestMethod]
        public void ValidateMainScript_NotExistingScript()
        {
            SELogger logger = new SELogger(logToFile: false);

            logger.NewLog += (sender, e) =>
            {
                Debug.WriteLine(e.Message);
            };

            Assert.IsFalse(CSharpCompiler.ValidateMainScript("", ref logger));
            Thread.Sleep(10); // Wait for the logger thread
        }

        /// <summary>
        /// This test checks that validation for the test script passes but generates a log with a
        /// warning telling that the optional stop method is missing
        /// (
        /// </summary>
        [TestMethod]
        public void ValidateMainScript_ExistingScript()
        {
            SELogger logger = new SELogger(logToFile: false);

            ManualResetEventSlim eventReceived = new ManualResetEventSlim(false);
            logger.NewLog += (sender, e) =>
            {
                Debug.WriteLine(e.LogLevel.ToString() + " : " + e.Message);
                Assert.AreEqual(LogLevel.Warning, e.LogLevel);
                Assert.IsTrue(e.Message.Contains("Missing optional method"));
                eventReceived.Set();
            };

            Assert.IsTrue(CSharpCompiler.ValidateMainScript(base_path + "ValidateMainScript_Test_01.cs", ref logger));

            bool eventReceivedResult = eventReceived.Wait(TimeSpan.FromSeconds(1));
            Assert.IsTrue(eventReceivedResult, "Log warning message not sent");
        }
    }
}
