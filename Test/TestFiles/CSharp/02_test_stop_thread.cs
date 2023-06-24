using System;

namespace CSharpScriptsExamples
{
    class TestStopThread
    {
        public TestStopThread()
        {
            Console.WriteLine("HelloWorld - Constructor");
        }

        public void Run()
        {
            while (true)
            {
                Console.WriteLine("HelloWorld - Run Method");
                System.Threading.Thread.Sleep(1000);
            }
        }

        public void Stop()
        {
            Console.WriteLine("HelloWorld - Stop Method - Adding a sleep of 20 seconds. Thread kill should occour before the 20 seconds");
            System.Threading.Thread.Sleep(20 * 1000);
            Console.WriteLine("HelloWorld - Stop Method - This should not be reached");
        }

    }
}