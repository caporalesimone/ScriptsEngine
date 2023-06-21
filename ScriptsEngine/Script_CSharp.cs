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
        }

        protected override ScriptType ValidateScript()
        {
            return ScriptType.CSHARP;
        }

    }

}
