using ScriptEngine.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

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
    /// Class for Status Changed Notification 
    /// </summary>
    public class StatusChangedEventArgs : EventArgs
    {
        public Guid ScriptGuid { get; }
        public EScriptStatus NewStatus { get; }

        public StatusChangedEventArgs(Guid scriptGuid, EScriptStatus newStatus)
        {
            ScriptGuid = scriptGuid;
            NewStatus = newStatus;
        }
    }

    /// <summary>
    /// Abstract class of a script. All scripts must inherit from it
    /// </summary>
    public abstract class ScriptAbstraction
    {
        #region PrivatesVariables
        private EScriptStatus m_Status; // Script Status
        private Guid m_scriptGuid; // Unique script identifier
        private readonly FileSystemWatcher m_FileWatcher; // Watcher that monitors the script changes
        #endregion

        #region ProtectedVariables
        protected Thread m_ScriptExecutionThread = null; // Thread that contains the script execution
        protected SELogger m_Logger = null;
        #endregion

        #region Properties
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
        /// Unique identifier of the script
        /// </summary>
        public Guid UniqueID { get => m_scriptGuid; }

        /// <summary>
        /// Event handler of the Status Changed
        /// </summary>
        public event EventHandler<StatusChangedEventArgs> StatusChangedEvent;

        /// <summary>
        /// This property tells the status of the script. On each status change an event is notified
        /// </summary>
        public EScriptStatus ScriptStatus
        {
            get { return m_Status; }
            protected set
            {
                if (m_Status != value)
                {
                    m_Status = value;
                    StatusChangedEvent?.Invoke(this, new StatusChangedEventArgs(m_scriptGuid, m_Status));

                    switch (m_Status)
                    {
                        case EScriptStatus.Error:
                        case EScriptStatus.Ready:
                        case EScriptStatus.NotCompiled:
                            m_FileWatcher.EnableRaisingEvents = true;
                            break;
                        case EScriptStatus.Compiling:
                        case EScriptStatus.Running:
                        case EScriptStatus.ReaquestedTerminate:
                            m_FileWatcher.EnableRaisingEvents = false;
                            break;
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// Abstract Script Contructor
        /// </summary>
        /// <param name="path">Script path</param>
        public ScriptAbstraction(string path, SELogger logger)
        {
            Type = EScriptLanguage.Unknown;
            FullPath = path;
            m_Status = EScriptStatus.NotCompiled;
            m_ScriptExecutionThread = null;
            m_scriptGuid = Guid.NewGuid();
            m_Logger = logger;
            m_Logger.EnableConsoleOutputCapture(LogLevel.Script); // Enable the capture of the Console.Write and Console.Writeline

            m_FileWatcher = new(Path.GetDirectoryName(path), FileName)
            {
                EnableRaisingEvents = false,
            };
            m_FileWatcher.Changed += OnFileChanged;
        }

        #region PublicMethods
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
        #endregion


        #region PrivateMethods
        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                new Thread (() => {
                    if (!ValidateScript()) return;
                    CompileAsync();
                });
            }
        }
        #endregion
    }
}
