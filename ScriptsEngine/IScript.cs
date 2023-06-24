using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScriptsEngine
{
    public enum ScriptType : byte
    {
        UNKNOWN = 0,
        CSHARP = 1,
        PYTHON = 2,
        UOSTEAM = 3,
    }

    /// <summary>
    /// Abstract class of a script. All scripts must inherit from it
    /// </summary>
    public abstract class IScript // : IScript
    {
        /// <summary>
        /// Type of the script
        /// </summary>
        public ScriptType Type { get; protected set; }
        /// <summary>
        /// Contains the full path of the script into the filesystem
        /// </summary>
        public string FullPath { get; }
        /// <summary>
        /// Contains the filename of the script
        /// </summary>
        public string FileName { get => Path.GetFileName(FullPath); }
        /// <summary>
        /// Last time the script has been written into filesystem
        /// </summary>
        public DateTime LastModified { get => File.GetLastWriteTime(FullPath); }
        /// <summary>
        /// Last time the script has been compiled
        /// </summary>
        public DateTime LastCompiledTime { get; private set; }
        /// <summary>
        /// Last time the script has been executed
        /// </summary>
        public DateTime LastExecutionTime { get; private set; }
        /// <summary>
        /// This property tells if the script is ready to be executed
        /// </summary>
        public abstract bool IsReady { get; }

        /// <summary>
        /// Return the status of the script. True means the thread of the script is not stopped.
        /// </summary>
        public bool IsRunning
        {
            get => (m_ScriptExecutionThread != null && m_ScriptExecutionThread.ThreadState != ThreadState.Stopped);
        }

        protected Thread m_ScriptExecutionThread = null;

        /// <summary>
        /// Abstract Script Contructor
        /// </summary>
        /// <param name="path">Script path</param>
        public IScript(string path)
        {
            Type = ScriptType.UNKNOWN;
            FullPath = path;
            m_ScriptExecutionThread = null;
        }

        /// <summary>
        /// Force a specific validation of the script
        /// </summary>
        /// <returns>true if validates</returns>
        public abstract bool ValidateScript();

        /// <summary>
        /// This method compile the script in a separate thread
        /// </summary>
        public bool CompileAsync()
        {
            LastCompiledTime = DateTime.Now;
            return CompileAsyncInternal();
        }

        /// <summary>
        /// Internal implementation of the CompileAsync Method
        /// </summary>
        /// <returns></returns>
        protected abstract bool CompileAsyncInternal();

        /// <summary>
        /// This method executes the script in a sepatate thread
        /// </summary>
        /// <returns></returns>
        public bool ExecuteScriptAsync()
        {
            LastExecutionTime = DateTime.Now;
            return ExecuteScritpAsycInternal();
        }

        /// <summary>
        /// Internal implementation of the ExecuteScriptAsync Method
        /// </summary>
        /// <returns></returns>
        protected abstract bool ExecuteScritpAsycInternal();

        /// <summary>
        /// Stops the execution of the script
        /// </summary>
        public abstract void StopScriptAsync();

    }
}
