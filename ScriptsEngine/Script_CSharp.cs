using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ScriptsEngine
{
    public class CSharpScript : AScript
    {
        public CSharpScript(string path) : base (path)
        {
            Type = ScriptType.CSHARP;
        }
        
        /// <summary>
        /// This function should validate the content of the script if is needed
        /// </summary>
        /// <returns>returns true if validates</returns>
        public override bool ValidateScript()
        {
// TODO
            return true;
        }
        public override bool Compile()
        {
            CSharpEngine.Instance.CompileFromFile(FullPath, true, out _, out _);
            return true;
        }
    }

}
