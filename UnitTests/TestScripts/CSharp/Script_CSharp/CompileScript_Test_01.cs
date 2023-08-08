using System;
using System.Diagnostics;
using System.Threading;

namespace Script_CSharp_TestScripts
{
    class CompileScript_Test_01
    {
        public CompileScript_Test_01()
        {
            Console.WriteLine("Constructor");
        }

        // Keep Run written in this way for testing the regex that will search the Run method
        public     void          Run   (   ) 
        {
            Console.WriteLine("Run Method");

            for (int i = 0; i < 10; i++)
            {
                /*
                Console.Write($"A{i}");
                Console.Write($"B{i}");
                Console.WriteLine($"C{i}");
                Thread.Sleep(10);
                */
                System.Diagnostics.Debug.WriteLine($"{i}");
            }

        }
    }
}