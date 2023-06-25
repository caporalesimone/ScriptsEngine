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
    public enum EScriptType : byte
    {
        UNKNOWN = 0,
        CSHARP = 1,
        PYTHON = 2,
        UOSTEAM = 3,
    }

    public enum EScriptStatus : byte
    {
        NOT_COMPILED,
        COMPILING,
        READY,
        RUNNING,
        REQUESTED_TERMINATE,
        ERROR,
    }

    /// <summary>
    /// Abstract class of a script. All scripts must inherit from it
    /// </summary>
    public abstract class IScript // : IScript
    {
        /// <summary>
        /// Type of the script
        /// </summary>
        public EScriptType Type { get; protected set; }
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
        /// This property tells the status of the script
        /// </summary>
        public abstract EScriptStatus ScriptStatus { get; }

        /// <summary>
        /// Thread that contains the script execution
        /// </summary>
        protected Thread m_ScriptExecutionThread = null;

        /// <summary>
        /// Abstract Script Contructor
        /// </summary>
        /// <param name="path">Script path</param>
        public IScript(string path)
        {
            Type = EScriptType.UNKNOWN;
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
        public void CompileAsync()
        {
            LastCompiledTime = DateTime.Now;
            CompileAsyncInternal();
        }

        /// <summary>
        /// Internal implementation of the CompileAsync Method
        /// </summary>
        /// <returns></returns>
        protected abstract void CompileAsyncInternal();

        /// <summary>
        /// This method executes the script in a sepatate thread
        /// </summary>
        /// <returns></returns>
        public void ExecuteScriptAsync()
        {
            LastExecutionTime = DateTime.Now;
            ExecuteScritpAsycInternal();
        }

        /// <summary>
        /// Internal implementation of the ExecuteScriptAsync Method
        /// </summary>
        /// <returns></returns>
        protected abstract void ExecuteScritpAsycInternal();

        /// <summary>
        /// Stops the execution of the script
        /// </summary>
        public abstract void StopScriptAsync();
    }
}
