using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScriptEngine;
using ScriptEngine.Logger;
using System;

namespace UnitTests
{
    [TestClass]
    public class Tests_Script_IronPython
    {
        private readonly SELogger m_logger = new SELogger(logToFile: false);

        [TestMethod]
        public void CreateScript()
        {
            ScriptAbstraction python = ScriptFactory.CreateScript(@"TestScripts\Python\CreateScript_Test_01.py", m_logger);
            Console.WriteLine(python.FullPath);
        }
    }
}
