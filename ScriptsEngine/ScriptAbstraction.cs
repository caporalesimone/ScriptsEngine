using System;
using System.IO;
using System.Threading;

namespace ScriptEngine
{
    public enum EScriptLanguage : byte
    {
        Unknown,
        CSharp,
        Python,
        UOSteam,
    }

    public enum EScriptStatus : byte
    {
        NotCompiled,
        Compiling,
        Ready,
        Running,
        ReaquestedTerminate,
        Error,
    }

    /// <summary>
    /// Abstract class of a script. All scripts must inherit from it
    /// </summary>
    public abstract class ScriptAbstraction
    {
        #region properties
        /// <summary>
        /// Type of the script
        /// </summary>
        public EScriptLanguage Type { get; protected set; }
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
		#endregion
        /// <summary>
        /// Abstract Script Contructor
        /// </summary>
        /// <param name="path">Script path</param>
        public ScriptAbstraction(string path)
        {
            Type = EScriptLanguage.Unknown;
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
        public void RunScriptAsync()
        {
            LastExecutionTime = DateTime.Now;
            RunScritpAsycInternal();
        }

        /// <summary>
        /// Internal implementation of the ExecuteScriptAsync Method
        /// </summary>
        /// <returns></returns>
        protected abstract void RunScritpAsycInternal();

        /// <summary>
        /// Stops the execution of the script
        /// </summary>
        public void StopScriptAsync()
        {
            StopScriptAsyncInternal();
        }

        /// <summary>
        /// Internal implementation of the StopScriptAsync Method
        /// </summary>
        protected abstract void StopScriptAsyncInternal();
    }
}
