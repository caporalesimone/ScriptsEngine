using ScriptEngine.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;
using System.Threading;

namespace ScriptEngine
{
    public static class ScriptFactory
    {
        /// <summary>
        /// Factory Class to create a Script Class
        /// </summary>
        /// <param name="path">Path on filesystem of the script</param>
        /// <returns></returns>
        public static ScriptAbstraction CreateScript(string path, SELogger logger)
        {
            if (path == null) return null;
            if (!File.Exists(path)) return null;

            path = Path.GetFullPath(path); // Converts a possible relative path into an absolute path
            string ext = Path.GetExtension(path);
            ScriptAbstraction ret = ext switch
            {
                ".cs" => new CSharpScript(path, logger),
                ".py" => new IronPythonScript(path, logger),
                //".uos" => new UOSScript(path),
                _ => null,
            };

            if (ret == null) return null;

            return ret.ValidateScript() == true ? ret : null;
        }
    }


}
