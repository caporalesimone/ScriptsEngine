using ScriptsEngine.Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ScriptsEngine
{
    public class CSharpScript : Script
    {
        private Assembly m_assembly = null;
        private object m_scriptInstance = null;
        private MethodInfo m_run_method = null;
        private EScriptStatus m_ScriptStatus = EScriptStatus.NotCompiled;

        private readonly bool m_compile_debug = true; // TODO Needs to be changed by the caller
        private readonly string assemblies_cfg_path = ""; // TODO Needs to be set by the caller

        private SELogger m_Logger;

        public CSharpScript(string path) : base (path)
        {
            Type = EScriptType.CSharp;
            m_assembly = null;
            m_ScriptStatus = EScriptStatus.NotCompiled;
            m_Logger = new SELogger(logToFile: false);
        }

        public override EScriptStatus ScriptStatus { get => m_ScriptStatus; }

        /// <summary>
        /// This function should validate the content of the script if is needed
        /// </summary>
        /// <returns>returns true if validates</returns>
        public override bool ValidateScript()
        {
            return CSharpCompiler.ValidateMainScript(FullPath, ref m_Logger);
        }

        protected override void CompileAsyncInternal()
        {
            if (m_assembly != null) { m_assembly = null; }

            m_ScriptStatus = EScriptStatus.Compiling;

            new Thread( () => {
                bool compile = CSharpCompiler.CompileFromFile(FullPath, m_compile_debug, assemblies_cfg_path, ref m_Logger, out m_assembly, out m_run_method);
                if (compile) 
                { 
                    m_ScriptStatus = EScriptStatus.Ready; 
                }
                else
                {
                    m_ScriptStatus = EScriptStatus.Error;
                }
            } ).Start();
        }

        protected override void RunScritpAsycInternal()
        {
            if (m_assembly == null) return;

            m_scriptInstance = CSharpCompiler.CreateScriptInstance(m_run_method);
            if (m_scriptInstance == null) return;

            m_ScriptExecutionThread = new Thread(() =>
            {
                m_ScriptStatus = EScriptStatus.Running;
                // This is a blocking call that ends when the Run method ends.
                CSharpCompiler.CallScriptMethod(m_scriptInstance, m_run_method, out _);
                StopScriptAsync();
            });

            m_ScriptExecutionThread.Start();
        }

        protected override void StopScriptAsyncInternal()
        {
            // Stop async will run a new thread that will call the Stop method of the script and will wait that main thread and stop thread
            // ends gracefully. If this not happen within 10 seconds, who is still running will be aborted

            if (m_ScriptExecutionThread == null) return;
            if (m_ScriptStatus != EScriptStatus.Running) return;

            m_ScriptStatus = EScriptStatus.ReaquestedTerminate;

            new Thread(() =>
            {
                // Now a new thread will be created and will call the Stop method of the script
                var stop_thread = new Thread(() => CSharpCompiler.CallStopScriptMethod(m_assembly, m_scriptInstance, out _));
                stop_thread.Start();

                // Let's wait gracefully the end of the thread for at least 10 seconds
                var timeout = new Stopwatch();
                timeout.Start();

                // Wait utill the state is stopped of both threads and not more than 10 seconds
                while ( 
                            (timeout.ElapsedMilliseconds < 10000) &&
                            (
                                (m_ScriptExecutionThread.ThreadState != System.Threading.ThreadState.Stopped) ||
                                (stop_thread.ThreadState != System.Threading.ThreadState.Stopped)
                            )
                    )
                {
                    System.Threading.Thread.Sleep(100);
                }

                // If more than 10 seconds elapsed then the unstopped thread will be aborted
                if (timeout.ElapsedMilliseconds >= 10000)
                {
                    if (m_ScriptExecutionThread.ThreadState != System.Threading.ThreadState.Stopped)
                    {
                        m_ScriptExecutionThread.Abort();
                        m_ScriptExecutionThread.Join(1000); // Just to be shure thread ended
                    }

                    if (stop_thread.ThreadState != System.Threading.ThreadState.Stopped)
                    {
                        stop_thread.Abort();
                        m_ScriptExecutionThread.Join(1000); // Just to be shure thread ended
                    }
                }

                timeout.Stop();

                m_ScriptExecutionThread = null;
                m_ScriptStatus = EScriptStatus.Ready;
            }).Start();
        }
    }

}
