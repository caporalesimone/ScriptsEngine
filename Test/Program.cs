using ScriptsEngine;
using System;

namespace Test
{
    // Nuget Package Microsoft.CodeDom.Providers.DotNetCompilerPlatform is required for add Roslyn compiler into the output folder

    internal class Program
    {
        static void Main(string[] args)
        {
            ScriptsLogger a = new ScriptsLogger();

            a.AddLog(LogEntry.E_LogType.Warning, "AAAAAA");
            a.AddLog(LogEntry.E_LogType.Warning, "BBBBBB");

            foreach (var it in a) 
            {
                Console.WriteLine(it.LogMessage);
            }





            ScriptFactory factory = new ScriptFactory();

            /*
            Script fail = factory.CreateScript("err");
            if (fail == null) { Console.WriteLine("Error!"); }
            */

            //Script csharp = factory.CreateScript(@"TestFiles\CSharp\01_hello_world.cs");
            //Script csharp = factory.CreateScript(@"TestFiles\CSharp\02_test_stop_thread_abort.cs");
            //Script csharp = factory.CreateScript(@"TestFiles\CSharp\02_test_stop_thread_abort_long.cs");
            //Script csharp = factory.CreateScript(@"TestFiles\CSharp\02_test_stop_thread_graceful.cs");
            Script csharp = factory.CreateScript(@"TestFiles\CSharp\03_missing_run.cs");
            
            csharp.CompileAsync();
            while (!Console.KeyAvailable)
            {
                if (csharp.ScriptStatus == EScriptStatus.ERROR)
                {
                    Console.WriteLine("Compile failed. Press a key to terminate.");
                    Console.ReadKey();
                    return;
                }

                if (csharp.ScriptStatus == EScriptStatus.READY) 
                {
                    break;
                }
                System.Threading.Thread.Sleep(200);
            }

            Console.WriteLine("Executing script");
            csharp.RunScriptAsync();
            while (!Console.KeyAvailable)
            {
                Console.WriteLine($"Script State: {csharp.ScriptStatus}");
                System.Threading.Thread.Sleep(200);
            }
            Console.ReadKey();

            Console.WriteLine("Stopping script");
            csharp.StopScriptAsync();
            while (!Console.KeyAvailable)
            {
                Console.WriteLine($"Script State: {csharp.ScriptStatus}");
                if (csharp.ScriptStatus == EScriptStatus.READY) break;
                System.Threading.Thread.Sleep(200);
            }

            Console.WriteLine("Script Terminated. Press enter to end test.");
            Console.ReadKey();
        }
    }
}
