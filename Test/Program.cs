using ScriptsEngine;
using System;

namespace Test
{
    // Nuget Package Microsoft.CodeDom.Providers.DotNetCompilerPlatform is required for add Roslyn compiler into the output folder

    internal class Program
    {
        static void Main(string[] args)
        {
            //ScriptsEngine n = new ScriptsEngine();
            ScriptFactory factory = new ScriptFactory();

            AScript csharp = factory.CreateScript(@"TestFiles\hello_world.cs");
            csharp.Compile();


            AScript fail = factory.CreateScript("err");

            //Console.ReadKey();

        }
    }
}
