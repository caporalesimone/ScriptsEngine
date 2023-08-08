using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScriptEngine;
using ScriptEngine.Logger;
using System;

namespace UnitTests
{
    [TestClass]
    public class Tests_Script_CSharp
    {
        private readonly string base_path = @"TestScripts\CSharp\Script_CSharp\";

        [TestMethod]
        public void CompileScript()
        {
            SELogger logger = new SELogger(logToFile: false);
            ScriptAbstraction script = ScriptFactory.CreateScript($"{base_path}CompileScript_Test_01.cs", logger);
            Assert.IsNotNull(script);
            //script.CompileAsync();

        }
    }
}
