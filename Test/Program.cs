using ScriptsEngine;
using System;

namespace Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //ScriptsEngine n = new ScriptsEngine();
            ScriptFactory factory = new ScriptFactory();

            AScript csharp = factory.CreateScript(@"TestFiles\hello_world.cs");
            csharp.Compile();


            AScript fail = factory.CreateScript("err");

            Console.ReadKey();

        }
    }
}
