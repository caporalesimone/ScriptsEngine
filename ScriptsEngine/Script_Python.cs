using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ScriptsEngine
{
    public class PythonScript : AScript
    {
        public PythonScript(string path) : base (path)
        {
        }

        protected override ScriptType ValidateScript()
        {
            return ScriptType.PYTHON;
        }

    }

}
