using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScriptsEngine;
using System;

namespace UnitTests
{
    [TestClass]
    public class Tests_ScriptFactory
    {
        [TestMethod]
        public void CSharpScriptNotExisting()
        {
            ScriptFactory factory = new ScriptFactory();
            Script csharp = factory.CreateScript(@"TestFiles\CSharp\01_hello_world.cs");
            Assert.IsNull(csharp);
        }

        [TestMethod]
        public void ScriptPathEmpty()
        {
            ScriptFactory factory = new ScriptFactory();
            Script csharp = factory.CreateScript("");
            Assert.IsNull(csharp);
        }
    }
}
