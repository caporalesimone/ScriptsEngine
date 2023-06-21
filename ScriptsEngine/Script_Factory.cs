using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ScriptsEngine
{
    public enum ScriptType : byte {
        CSHARP = 0,
        PYTHON = 1,
        UOSTEAM = 2,

        UNKNOWN = 255
    }

    /// <summary>
    /// Script Interface
    /// </summary>
    public interface IScript
    {
        string Path { get; }
        ScriptType Type { get; }
        Assembly Assembly { get; }
    }

    /// <summary>
    /// Abstract class of a script. All scripts must inherit from it
    /// </summary>
    public abstract class AScript : IScript
    {
        public string Path { get; }
        public ScriptType Type { get; }

        public Assembly Assembly { get; private set; }

        public AScript(string path)
        {
            Path = path;
            Assembly = null;
            Type = ValidateScript();
        }

        /// <summary>
        /// Force implementation of a ValidateScript function specific for the script
        /// </summary>
        /// <returns></returns>
        protected abstract ScriptType ValidateScript();
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
        public IScript CreateScript(string path)
        {
            if (path == null) return null;
            if (!File.Exists(path)) return null;

            string ext = Path.GetExtension(path);
            switch (ext)
            {
                case ".cs":
                    var cs = new CSharpScript(path);
                    return cs;
                case ".py":
                    var py = new PythonScript(path);
                    return py;
                default:
                    return null;
            }
        }
    }

    
}
