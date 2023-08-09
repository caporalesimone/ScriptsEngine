using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScriptEngine;
using ScriptEngine.Logger;
using System;

namespace UnitTests
{
    [TestClass]
    public class Tests_ScriptFactory
    {
        private readonly SELogger m_logger = new SELogger(logToFile: false);

        [TestMethod]
        public void CSharpScriptNotExisting()
        {
            ScriptAbstraction csharp = ScriptFactory.CreateScript(@"TestFiles\CSharp\not_existing_script.cs", m_logger);
            Assert.IsNull(csharp);
        }

        [TestMethod]
        public void ScriptPathEmpty()
        {
            ScriptAbstraction csharp = ScriptFactory.CreateScript("", m_logger);
            Assert.IsNull(csharp);
        }
    }
}
