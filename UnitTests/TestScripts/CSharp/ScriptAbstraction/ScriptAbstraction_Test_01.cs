using System;
using System.Diagnostics;
using System.Threading;

namespace Script_CSharp_TestScripts
{
    class ScriptAbstraction_Test_01
    {
        public ScriptAbstraction_Test_01()
        {
            Console.WriteLine("Constructor");
        }

        // Keep Run written in this way for testing the regex that will search the Run method
        public void Run()
        {
            Console.WriteLine("Run Method");
        }

        public void Stop() 
        {
            Console.WriteLine("Stop Method");
        }
    }
}