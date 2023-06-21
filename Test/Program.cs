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

            IScript csharp = factory.CreateScript("D:\\Progetti\\ScriptsEngine\\ScriptsEngine\\ScriptsEngine\\Script_CSharp.cs");

            IScript python = factory.CreateScript("py");

            IScript fail = factory.CreateScript("err");

            Console.ReadKey();

        }
    }
}
