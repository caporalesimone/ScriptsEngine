// This script, when stop is called will not stop and an abort will be required

using System;

namespace CSharpScriptsExamples
{
    class TestStopThread_Abort
    {
        private bool m_Run = true;

        public TestStopThread_Abort()
        {
            Console.WriteLine("HelloWorld - Constructor");
        }

        public void Run()
        {
            while (m_Run)
            {
                Console.WriteLine("HelloWorld - Run Method");
                System.Threading.Thread.Sleep(1000);
            }
        }

        public void StopScript()
        {
            Console.WriteLine("HelloWorld - Stop Method called. Now stop ends immediately without terminating Run");
        }

    }
}