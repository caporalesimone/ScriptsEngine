using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ScriptsEngine
{
    public class CSharpScript : IScript
    {
        private Assembly m_assembly = null;
        private object m_scriptInstance = null;
        private EScriptStatus m_ScriptStatus = EScriptStatus.NOT_COMPILED;

        public CSharpScript(string path) : base (path)
        {
            Type = EScriptType.CSHARP;
            m_assembly = null;
            m_ScriptStatus = EScriptStatus.NOT_COMPILED;
        }

        public override EScriptStatus ScriptStatus { get => m_ScriptStatus; }

        /// <summary>
        /// This function should validate the content of the script if is needed
        /// </summary>
        /// <returns>returns true if validates</returns>
        public override bool ValidateScript()
        {
// TODO
            return true;
        }

        protected override void CompileAsyncInternal()
        {
            if (m_assembly != null) { m_assembly = null; }

            m_ScriptStatus = EScriptStatus.COMPILING;

            new Thread( () => { 
                bool compile = CSharpCompiler.CompileFromFile(FullPath, true, out List<string> errorwarnings, out m_assembly);
                if (compile) 
                { 
                    m_ScriptStatus = EScriptStatus.READY; 
                }
                else
                {
                    m_ScriptStatus = EScriptStatus.ERROR;
                }
            } ).Start();
        }

        protected override void ExecuteScritpAsycInternal()
        {
            if (m_assembly == null) return;

            m_ScriptExecutionThread = new Thread(() => 
                {
                    bool execute = CSharpCompiler.ExecuteScript(m_assembly, out m_scriptInstance, out _);
                    if (execute) 
                    {
                        m_ScriptStatus = EScriptStatus.RUNNING;
                    }
                    else
                    {
                        m_ScriptStatus = EScriptStatus.ERROR;
                    }
                }
            );
            m_ScriptExecutionThread.Start();
        }

        public override void StopScriptAsync()
        {
            // Stop async will run a new thread that will call the Stop method of the script and will wait that main thread and stop thread
            // ends gracefully. If this not happen within 10 seconds, who is still running will be aborted

            new Thread(() =>
            {
                if (m_ScriptStatus != EScriptStatus.RUNNING) return;

                // Now a new thread will be created and will call the Stop method of the script
                var stop_thread = new Thread(() => CSharpCompiler.CallScriptMethod(m_assembly, m_scriptInstance, "Stop", out _));
                stop_thread.Start();

                /*
                Console.WriteLine($"--------------------------------------");
                Console.WriteLine($"Main thread: {m_ScriptExecutionThread.ThreadState}");
                Console.WriteLine($"Stop thread: {stop_thread.ThreadState}");
                Console.WriteLine($"--------------------------------------");
                */

                // Let's wait gracefully the end of the thread for at least 10 secons
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
                    //Console.WriteLine($"Main thread: {m_ScriptExecutionThread.ThreadState} - Elapsed {timeout.ElapsedMilliseconds}");
                    //Console.WriteLine($"Stop thread: {stop_thread.ThreadState}");
                    System.Threading.Thread.Sleep(100);
                }

                /*
                Console.WriteLine($"--------------------------------------");
                Console.WriteLine($"Elapsed: {timeout.ElapsedMilliseconds}");
                Console.WriteLine($"Main thread: {m_ScriptExecutionThread.ThreadState}");
                Console.WriteLine($"Stop thread: {stop_thread.ThreadState}");
                Console.WriteLine($"--------------------------------------");
                */

                // If more than 10 seconds elapsed then the unstopped thread will be aborted
                if (timeout.ElapsedMilliseconds >= 10000)
                {
                    if (m_ScriptExecutionThread.ThreadState != System.Threading.ThreadState.Stopped)
                    {
                        m_ScriptExecutionThread.Abort();
                        m_ScriptExecutionThread.Join(1000); // Just to be shure thread ended
                        //Console.WriteLine("Aborted main thread");
                    }

                    if (stop_thread.ThreadState != System.Threading.ThreadState.Stopped)
                    {
                        stop_thread.Abort();
                        m_ScriptExecutionThread.Join(1000); // Just to be shure thread ended
                        //Console.WriteLine("Aborted stop thread");
                    }
                }

                /*
                Console.WriteLine($"--------------------------------------");
                Console.WriteLine($"Elapsed: {timeout.ElapsedMilliseconds}");
                Console.WriteLine($"Main thread: {m_ScriptExecutionThread.ThreadState}");
                Console.WriteLine($"Stop thread: {stop_thread.ThreadState}");
                Console.WriteLine($"--------------------------------------");
                */
                timeout.Stop();
                //Console.WriteLine("Stop procedure terminated");

                m_ScriptStatus = EScriptStatus.READY;
            }).Start();
        }

    }

}
