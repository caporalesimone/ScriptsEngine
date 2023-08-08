using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting;
using ScriptEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using ScriptEngine.Logger;

namespace ScriptEngine
{
    internal class IronPythonScript : ScriptAbstraction
    {
        private Microsoft.Scripting.Hosting.ScriptEngine engine;
        private ScriptSource source;
        private dynamic scriptInstance;
        private string status;

        private SELogger m_logger;
        public IronPythonScript(string path, SELogger logger) : base(path)
        {
            m_logger = logger;
            /*
            engine = Python.CreateEngine();
            source = engine.CreateScriptSourceFromFile(path);
            ScriptScope scope = engine.CreateScope();
            try
            {
                source.Execute(scope);
                dynamic script = scope.GetVariable("Script");
                scriptInstance = script();
                status = "Compile Success";
            }
            catch (Exception ex)
            {
                status = "Compile Error: " + ex.Message;
            }
            */
        }

        public override EScriptStatus ScriptStatus { get => ScriptStatus; }

        public override bool ValidateScript()
        {
            return true;
        }

        protected override void CompileAsyncInternal()
        {
            throw new NotImplementedException();
        }

        protected override void RunScritpAsycInternal()
        {
            throw new NotImplementedException();
        }

        protected override void StopScriptAsyncInternal()
        {
            throw new NotImplementedException();
        }
    }
}
