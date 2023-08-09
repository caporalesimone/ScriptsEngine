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

        private void ExecuteScriptCompile(ScriptAbstraction script)
        {
            script.CompileAsync();

            var startTime = DateTime.Now;
            while (script.ScriptStatus != EScriptStatus.Compiling)
            {
                if (DateTime.Now - startTime > TimeSpan.FromSeconds(1)) Assert.Fail("Elapsed too much for enter in Compiling state");
            }

            startTime = DateTime.Now;
            while (script.ScriptStatus != EScriptStatus.Ready)
            {
                if (DateTime.Now - startTime > TimeSpan.FromSeconds(10)) Assert.Fail("Elapsed too much for exit from Compiling state");

                if (script.ScriptStatus == EScriptStatus.Error) Assert.Fail("Failed to compile the script");
            }
        }

        [TestMethod]
        public void CompileError()
        {
            SELogger logger = new SELogger(logToFile: false);
            ScriptAbstraction script = ScriptFactory.CreateScript($"{base_path}CompileScript_Test_00_no_compile.cs", logger);
            Assert.IsNotNull(script);
            Assert.AreEqual(script.FileName, "CompileScript_Test_00_no_compile.cs");
            Assert.IsTrue(File.Exists(script.FullPath));
            Assert.AreEqual(script.ScriptStatus, EScriptStatus.NotCompiled);
            Assert.IsTrue(script.LastExecutionTime < DateTime.Now);

            try
            {
                ExecuteScriptCompile(script);
                Assert.Fail("Script compile should fail");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("Failed to compile the script"));
                Debug.WriteLine(ex);
            }
        }

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

            script.StatusChanged += (sender, e) =>
            {
                Assert.AreEqual(script.UniqueID, e.ScriptGuid);
                Debug.WriteLine($"Event change by {e.ScriptGuid}: {e.NewStatus}");
            };

            ExecuteScriptCompile(script);
        }

        [TestMethod]
        public void ExecuteScript()
        {
            SELogger logger = new SELogger(logToFile: false);
            ScriptAbstraction script = ScriptFactory.CreateScript($"{base_path}CompileScript_Test_02.cs", logger);

            logger.LogEvent += (sender, e) =>
            {
                Debug.WriteLine(e.FullMessage);
            };

            script.StatusChanged += (sender, e) =>
            {
                Debug.WriteLine($"Event change by {e.ScriptGuid}: {e.NewStatus}");
            };
            Debug.WriteLine("Script terminated");
            ExecuteScriptCompile(script);

            script.RunScriptAsync();

            var startTime = DateTime.Now;
            while (script.ScriptStatus != EScriptStatus.Ready)
            {
                if (DateTime.Now - startTime > TimeSpan.FromSeconds(1)) Assert.Fail("Script elapsed too much. Something wrong");
            }

            while (logger.IsLoggingInProgress)
            {
                Thread.Sleep(100);
            }

            Thread.Sleep(100);
        }
    }
}
