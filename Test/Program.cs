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

            /*
            IScript fail = factory.CreateScript("err");
            if (fail == null) { Console.WriteLine("Error!"); }
            */


            IScript csharp = factory.CreateScript(@"TestFiles\CSharp\01_hello_world.cs");
            //IScript csharp = factory.CreateScript(@"TestFiles\CSharp\02_test_stop_thread_abort.cs");
            //IScript csharp = factory.CreateScript(@"TestFiles\CSharp\02_test_stop_thread_abort_long.cs");
            //IScript csharp = factory.CreateScript(@"TestFiles\CSharp\02_test_stop_thread_graceful.cs");
            //IScript csharp = factory.CreateScript(@"TestFiles\CSharp\03_missing_run.cs");
            csharp.CompileAsync();

            while (csharp.ScriptStatus != EScriptStatus.READY)
            {
                System.Threading.Thread.Sleep(10);
                if (csharp.ScriptStatus == EScriptStatus.ERROR)
                {
                    Console.WriteLine("Compile failed. Press a key to terminate.");
                    Console.ReadKey();
                    return;
                }
            }

            Console.WriteLine("Executing script");
            csharp.ExecuteScriptAsync();
            System.Threading.Thread.Sleep(100);
            Console.WriteLine($"Script State: {csharp.ScriptStatus}");
            Console.ReadKey();

            csharp.StopScriptAsync();
            while (csharp.ScriptStatus == EScriptStatus.RUNNING)
            {
                System.Threading.Thread.Sleep(10);
            }
            Console.WriteLine("Script Terminated. Press enter to end test.");
            Console.ReadKey();
        }
    }
}
