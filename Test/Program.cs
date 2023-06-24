using ScriptsEngine;
using System;

namespace Test
{
    // Nuget Package Microsoft.CodeDom.Providers.DotNetCompilerPlatform is required for add Roslyn compiler into the output folder

    internal class Program
    {
        static void Main(string[] args)
        {
            ScriptFactory factory = new ScriptFactory();

            IScript csharp = factory.CreateScript(@"TestFiles\hello_world.cs");
            csharp.CompileAsync();

            IScript fail = factory.CreateScript("err");
            if (fail == null) { Console.WriteLine("Error!"); }

            Console.ReadKey();
            csharp.ExecuteScriptAsync();
            Console.ReadKey();

        }
    }
}
