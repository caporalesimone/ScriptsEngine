using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScriptsEngine;
using System;
using System.Diagnostics;
using System.Reflection;
using ScriptsEngine.Logger;

namespace UnitTests
{
    [TestClass]
    public class Tests_CSharpCompiler
    {
        private readonly string base_path = @"TestScripts\CSharp\CSharpCompiler_Scripts\";

        [TestMethod]
        [Ignore]
        public void ValidateMainScript_NotExistingScript()
        {
            SELogger logger = new SELogger(logToFile: false);
            Assert.IsFalse(CSharpCompiler.ValidateMainScript("", ref logger));
            //Debug.WriteLine(logger.PopLog().LogMessage);
        }

        [TestMethod]
        [Ignore]
        public void ValidateMainScript_ExistingScript()
        {
            SELogger logger = new SELogger(logToFile: false);
            Assert.IsTrue(CSharpCompiler.ValidateMainScript(base_path + "ValidateMainScript_Test_01.cs", ref logger));

            // Expected a warning log telling that optional method public void StopScript() is missing
            //var log = logger.PopLog();
            //Assert.IsNotNull(log);
            //Assert.AreEqual(log.LogType, LogLevel.Warning);
            //Debug.WriteLine(log.LogMessage);
        }

    }
}
