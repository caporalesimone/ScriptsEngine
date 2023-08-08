using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScriptEngine;
using ScriptEngine.Logger;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting.Channels;
using System.Threading;
using System.Threading.Tasks;

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
            Assert.AreEqual(script.FileName, "CompileScript_Test_01.cs");
            Assert.IsTrue(File.Exists(script.FullPath));
            Assert.AreEqual(script.ScriptStatus, EScriptStatus.NotCompiled);
            Assert.IsTrue(script.LastExecutionTime < DateTime.Now);

            logger.NewLog += (sender, e) =>
            {
                Debug.WriteLine(e.FullMessage);
            };

            script.StatusChanged += (sender, e) =>
            {
                Debug.WriteLine($"Event Change from {e.ScriptGuid}: {e.NewStatus}" );
            };

            script.CompileAsync();

            var startTime = DateTime.Now;
            while (script.ScriptStatus != EScriptStatus.Compiling)
            {
                if (DateTime.Now - startTime > TimeSpan.FromSeconds(1)) Assert.Fail("Elapsed too much for enter in Compiling state");
            }

            startTime = DateTime.Now;
            while (script.ScriptStatus != EScriptStatus.Ready)
            {
                if (DateTime.Now - startTime > TimeSpan.FromSeconds(15)) Assert.Fail("Elapsed too much for exit from Compiling state");

                if (script.ScriptStatus == EScriptStatus.Error) Assert.Fail("Failed to compile the script");
            }

            script.RunScriptAsync();

            while (script.ScriptStatus != EScriptStatus.Ready)
            {
                Thread.Sleep(100);
            }

            //Thread.Sleep(1000);
        }
    }
}
