using ScriptEngine.Logger;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace ScriptEngine
{
    public class CSharpScript : ScriptAbstraction
    {
        private Assembly m_assembly = null;
        private object m_scriptInstance = null;
        private MethodInfo m_run_method = null;

        private readonly bool m_compile_debug = true; // TODO Needs to be changed by the caller
        private readonly string assemblies_cfg_path = "Assemblies.cfg"; // TODO Needs to be set by the caller

        private SELogger m_Logger;

        public CSharpScript(string path, SELogger logger) : base(path)
        {
            Type = EScriptLanguage.CSharp;
            m_assembly = null;
            ScriptStatus = EScriptStatus.NotCompiled;
            m_Logger = logger;
        }

        public override bool ValidateScript()
        {
            return CSharpCompiler.ValidateMainScript(FullPath, ref m_Logger);
        }

        protected override void CompileAsyncInternal()
        {
            if (m_assembly != null) { m_assembly = null; }

            ScriptStatus = EScriptStatus.Compiling;

            new Thread(() =>
            {
                try
                {
                    bool compile = CSharpCompiler.CompileFromFile(FullPath, m_compile_debug, assemblies_cfg_path, ref m_Logger, out m_assembly, out m_run_method);
                    if (compile)
                    {
                        ScriptStatus = EScriptStatus.Ready;
                    }
                    else
                    {
                        ScriptStatus = EScriptStatus.Error;
                    }
                }
                catch (Exception ex)
                {
                    m_Logger.AddLog(LogLevel.Error, ex.ToString());
                    ScriptStatus = EScriptStatus.Error;
                }
            }).Start();
        }

        protected override void RunScritpAsycInternal()
        {
            if (m_assembly == null) return;

            m_scriptInstance = CSharpCompiler.CreateScriptInstance(m_run_method);
            if (m_scriptInstance == null) return;

            m_ScriptExecutionThread = new Thread(() =>
            {
                m_Logger.EnableConsoleOutputCapture(LogLevel.Script);
                ScriptStatus = EScriptStatus.Running;
                // This is a blocking call that ends when the Run method ends.
                CSharpCompiler.CallScriptMethod(m_scriptInstance, m_run_method, out _);
                StopScriptAsync();
                m_Logger.DisableConsoleOutputCapure();
            });

            m_ScriptExecutionThread.Start();
        }

        protected override void StopScriptAsyncInternal()
        {
            // Stop async will run a new thread that will call the Stop method of the script and will wait that main thread and stop thread
            // ends gracefully. If this not happen within 10 seconds, who is still running will be aborted

            if (m_ScriptExecutionThread == null) return;
            if (ScriptStatus != EScriptStatus.Running) return;

            ScriptStatus = EScriptStatus.ReaquestedTerminate;

            new Thread(() =>
            {
                const int MAX_TIMEOUT = 10000;

                // Now a new thread will be created and will call the Stop method of the script
                var stop_thread = new Thread(() => CSharpCompiler.CallStopScriptMethod(m_assembly, m_scriptInstance, out _));
                stop_thread.Start();

                // Let's wait gracefully the end of the thread for at least 10 seconds
                var timeout = new Stopwatch();
                timeout.Start();

                // Wait util the state is stopped of both threads and not more than 10 seconds
                while (
                            (timeout.ElapsedMilliseconds < MAX_TIMEOUT) &&
                            (
                                (m_ScriptExecutionThread.ThreadState != System.Threading.ThreadState.Stopped) ||
                                (stop_thread.ThreadState != System.Threading.ThreadState.Stopped)
                            )
                    )
                {
                    System.Threading.Thread.Sleep(100);
                }

                // If more than 10 seconds elapsed then the unstopped thread will be aborted
                if (timeout.ElapsedMilliseconds >= MAX_TIMEOUT)
                {
                    if (m_ScriptExecutionThread.ThreadState != System.Threading.ThreadState.Stopped)
                    {
                        m_Logger.AddLog(LogLevel.Error, $"Script took more than {TimeSpan.FromMilliseconds(MAX_TIMEOUT).Duration().TotalSeconds} seconds to terminate. It will be killed.");
                        m_ScriptExecutionThread.Abort();
                        m_ScriptExecutionThread.Join(1000); // Just to be shure thread ended
                    }

                    if (stop_thread.ThreadState != System.Threading.ThreadState.Stopped)
                    {
                        m_Logger.AddLog(LogLevel.Error, $"The stop function was unable to terminate in less than {TimeSpan.FromMilliseconds(MAX_TIMEOUT).Duration().TotalSeconds} seconds. It will be killed.");
                        stop_thread.Abort();
                        m_ScriptExecutionThread.Join(1000); // Just to be shure thread ended
                    }
                }

                timeout.Stop();

                m_ScriptExecutionThread = null;
                ScriptStatus = EScriptStatus.Ready;
            }).Start();
        }
    }

}
