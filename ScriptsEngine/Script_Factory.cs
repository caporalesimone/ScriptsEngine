using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;
using System.Threading;

namespace ScriptsEngine
{
    public class ScriptFactory //: ScriptFactoryAbstract
    {
        //public override IScript CreateScript(string path)

        /// <summary>
        /// Factory Class to create a Script Class
        /// </summary>
        /// <param name="path">Path on filesystem of the script</param>
        /// <returns></returns>
        public IScript CreateScript(string path)
        {
            if (path == null) return null;
            if (!File.Exists(path)) return null;

            path = Path.GetFullPath(path); // Converts a possible relative path into an absolute path
            string ext = Path.GetExtension(path);

            IScript ret;

            switch (ext)
            {
                case ".cs":
                    ret = new CSharpScript(path);
                    break;
/*
                case ".py":
                    ret = new PythonScript(path);
                    break;
                case ".uos":
                    ret = new UOSScript(path);
                    break;
*/
                default:
                    ret = null;
                    break;
            }

            if (ret.ValidateScript()) return ret;
            return null;
        }
    }


}
