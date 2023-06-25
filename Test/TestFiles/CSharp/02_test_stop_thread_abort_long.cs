// This script, when stop is called will sleep 20 seconds and this is more than 10 seconds of abort.
// In any case after 10 seconds abort will kill the script and the message after the sleep of the stop will be not shown
// 

using System;

namespace CSharpScriptsExamples
{
    class TestStopThread_Abort_long
    {
        private bool m_Run = true;

        public TestStopThread_Abort_long()
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

        public void Stop()
        {
            Console.WriteLine("HelloWorld - Stop Method - Now will sleep 20 seconds, that is more then 10 of abort");
            int cnt = 100; // 20 seconds
            while (cnt-- > 0)
            {
                Console.WriteLine($"{cnt} - Sleep inside the stop call");
                System.Threading.Thread.Sleep(500);
            }
            
            Console.WriteLine("HelloWorld - Stop Method - This should not be reached");
        }

    }
}