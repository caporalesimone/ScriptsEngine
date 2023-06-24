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

            //IScript csharp = factory.CreateScript(@"TestFiles\CSharp\01_hello_world.cs");
            IScript csharp = factory.CreateScript(@"TestFiles\CSharp\02_test_stop_thread.cs");
            //IScript csharp = factory.CreateScript(@"TestFiles\CSharp\03_missing_run.cs");
            csharp.CompileAsync();
            var a = csharp.GetType();


            IScript fail = factory.CreateScript("err");
            if (fail == null) { Console.WriteLine("Error!"); }

            //Console.ReadKey();
            Console.WriteLine("Waiting...");
            System.Threading.Thread.Sleep(2000);
            Console.WriteLine("Executing script");
            csharp.ExecuteScriptAsync();
            Console.ReadKey();
            csharp.StopScriptAsync();
            Console.ReadKey();
            Console.ReadKey();
            Console.WriteLine($"Thread is running: {csharp.IsRunning}");
            Console.ReadKey();
        }
    }
}
