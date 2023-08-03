using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScriptsEngine;
using System;
using System.Diagnostics;
using System.Reflection;
using static ScriptsEngine.LogEntry;

namespace UnitTests
{
    [TestClass]
    public class Tests_CSharpCompiler
    {
        private readonly string base_path = @"TestScripts\CSharp\CSharpCompiler_Scripts\";

        [TestMethod]
        public void ValidateMainScript_NotExistingScript()
        {
            ScriptLogger logger = new ScriptLogger();
            Assert.IsFalse(CSharpCompiler.ValidateMainScript("", ref logger));
            Debug.WriteLine(logger.PopLog().LogMessage);
        }

        [TestMethod]
        public void ValidateMainScript_ExistingScript()
        {
            ScriptLogger logger = new ScriptLogger();
            Assert.IsTrue(CSharpCompiler.ValidateMainScript(base_path + "ValidateMainScript_Test_01.cs", ref logger));

            // Expected a warning log telling that optional method public void StopScript() is missing
            var log = logger.PopLog();
            Assert.IsNotNull(log);
            Assert.AreEqual(log.LogType, LogEntry.ELogType.Warning);
            Debug.WriteLine(log.LogMessage);
        }

    }
}
