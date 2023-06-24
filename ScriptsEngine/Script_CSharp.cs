using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace ScriptsEngine
{
    public class CSharpScript : IScript
    {
        private Assembly m_assembly = null;
        private object m_scriptInstance = null;

        public CSharpScript(string path) : base (path)
        {
            Type = ScriptType.CSHARP;
            m_assembly = null;
        }

        public override bool IsReady => m_assembly != null;

        /// <summary>
        /// This function should validate the content of the script if is needed
        /// </summary>
        /// <returns>returns true if validates</returns>
        public override bool ValidateScript()
        {
// TODO
            return true;
        }

        protected override bool CompileAsyncInternal()
        {
            if (m_assembly != null) { m_assembly = null; }
            new Thread(() => CSharpCompiler.CompileFromFile(FullPath, true, out _, out m_assembly)).Start();
            return true;
        }

        protected override bool ExecuteScritpAsycInternal()
        {
            if (m_assembly == null) return false;

            m_ScriptExecutionThread = new Thread(() => CSharpCompiler.ExecuteScript(m_assembly, "Run", out m_scriptInstance, out _));
            m_ScriptExecutionThread.Start();
            return true;
        }

        public override void StopScriptAsync()
        {
            new Thread(() =>
            {
                if (!IsRunning) return;

                var task = Task.Run(() => CSharpCompiler.CallScriptMethod(m_assembly, m_scriptInstance, "Stop", out _));
                task.Wait(TimeSpan.FromSeconds(10));
                m_ScriptExecutionThread.Abort();
            }).Start();
        }

    }

}
