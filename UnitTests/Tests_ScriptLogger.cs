using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScriptsEngine;
using System;
using System.Linq;

namespace UnitTests
{
    [TestClass]
    public class Tests_ScriptLogger
    {
        [TestMethod]
        public void LoggerInstantiate()
        {
            ScriptLogger log = new ScriptLogger();
            Assert.IsNotNull(log);
        }

        [TestMethod]
        public void LoggerEnqueueAllTypesOfLogs()
        {
            ScriptLogger log = new ScriptLogger();
            Assert.IsNotNull(log);

            int log_count = 1000;

            // Iterate for each enum, adds logs and then checks
            foreach (var logtype in Enum.GetValues(typeof(LogEntry.ELogType)))
            {
                for (int i = 0; i < log_count; i++)
                {
                    log.AddLog((LogEntry.ELogType)logtype, $"Log {i}");
                }

                Assert.AreEqual(log.Count(), log_count);

                foreach (var item in log)
                {
                    Assert.AreEqual(item.LogType, (LogEntry.ELogType)logtype);
                }

                log.Clear();

                Assert.AreEqual(log.Count(), 0);
            }
        }

        [TestMethod]
        public void LoggerDequeueLogs()
        {
            ScriptLogger log = new ScriptLogger();
            Assert.IsNotNull(log);

            int log_count_per_each_level = 1000;

            // Iterate for each enum, adds logs and then checks
            foreach (var logtype in Enum.GetValues(typeof(LogEntry.ELogType)))
            {
                for (int i = 0; i < log_count_per_each_level; i++)
                {
                    log.AddLog((LogEntry.ELogType)logtype, $"Log {i}");
                }
            }

            var log_levels_count = Enum.GetValues(typeof(LogEntry.ELogType)).Length;

            Assert.AreEqual(log.Count(), log_count_per_each_level * log_levels_count);

            for (int i = 0; i < log_count_per_each_level * log_levels_count; i++)
            {
                Assert.IsNotNull(log.PopLog());
            }

            Assert.IsNull(log.PopLog());
        }
    }
}
