using System;

namespace CSharpScriptsExamples
{
    class TestStopThread
    {
        private bool m_Run = true;

        public TestStopThread()
        {
            Console.WriteLine("HelloWorld - Constructor");
        }

        public void Run()
        {
            while (m_Run)
            {
                Console.WriteLine("HelloWorld - Run Method");
                System.Threading.Thread.Sleep(500);
            }
            Console.WriteLine("HelloWorld - Terminated");
        }

        public void Stop()
        {
            Console.WriteLine("HelloWorld - Stop Method - sending a stop to main loop and wait 3 seconds for test");
            m_Run = false;
            System.Threading.Thread.Sleep(3 * 1000);
            Console.WriteLine("HelloWorld - Stop Method - terminated after 3 seconds");
        }

    }
}