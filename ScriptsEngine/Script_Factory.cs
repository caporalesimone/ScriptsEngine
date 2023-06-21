using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;
using System.Threading;

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
    /// Script Interface
    /// </summary>
    public interface IScript
    {
        ScriptType Type { get; }
        string FullPath { get; }
        string FileName { get; }
        DateTime LastModified { get; }
    }

    /// <summary>
    /// Abstract class of a script. All scripts must inherit from it
    /// </summary>
    public abstract class AScript : IScript
    {
        public ScriptType Type { get; protected set; }
        public string FullPath { get; }
        public string FileName { get => Path.GetFileName(FullPath); }
        public DateTime LastModified { get => File.GetLastWriteTime(FullPath); }

        /// <summary>
        /// Abstract Script Contructor
        /// </summary>
        /// <param name="path">Script path</param>
        public AScript(string path)
        {
            FullPath = path;
            Type = ScriptType.UNKNOWN;
        }

        /// <summary>
        /// Force a specific validation of the script
        /// </summary>
        /// <returns>true if validates</returns>
        public abstract bool ValidateScript();

        /// <summary>
        /// This method compile the script
        /// </summary>
        public abstract bool Compile();
    }



    /*
    public abstract class ScriptFactoryAbstract
    {
        public abstract IScript CreateScript(string path);
    }
    */

    public class ScriptFactory //: ScriptFactoryAbstract
    {
        //public override IScript CreateScript(string path)

        /// <summary>
        /// Factory Class to create a Script Class
        /// </summary>
        /// <param name="path">Path on filesystem of the script</param>
        /// <returns></returns>
        public AScript CreateScript(string path)
        {
            if (path == null) return null;
            if (!File.Exists(path)) return null;

            path = Path.GetFullPath(path); // Converts a possible relative path into an absolute path
            string ext = Path.GetExtension(path);

            AScript ret;
            switch (ext)
            {
                case ".cs":
                    ret = new CSharpScript(path);
                    break;
                case ".py":
                    //ret = new PythonScript(path);
                    throw new NotImplementedException();
#pragma warning disable CS0162 // È stato rilevato codice non raggiungibile
                    break;
#pragma warning restore CS0162 // È stato rilevato codice non raggiungibile
                case ".uos":
                    //ret = new UOSScript(path);
                    throw new NotImplementedException();
#pragma warning disable CS0162 // È stato rilevato codice non raggiungibile
                    break;
#pragma warning restore CS0162 // È stato rilevato codice non raggiungibile
                default:
                    ret = null;
                    break;
            }

            if (ret.ValidateScript()) return ret;
            return null;
        }
    }


}
