using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScriptsEngine;
using System;

namespace UnitTests
{
    [TestClass]
    public class Tests_ScriptLogger
    {
        [TestMethod]
        public void TestMethod1()
        {
            ScriptLogger logEntries = new ScriptLogger();
            Assert.IsNotNull(logEntries);
        }
    }
}
