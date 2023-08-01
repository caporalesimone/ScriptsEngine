using System;

namespace CSharpScriptsExamples
{
  class HelloWorld
  {
    public HelloWorld()
    {
        Console.WriteLine("HelloWorld - Constructor");
    }
    
    public void     Run   (   )
    {
        Console.WriteLine("HelloWorld - Run Method");
    }

    public void StopScript()
    {
        Console.WriteLine("HelloWorld - Stop Method");
    }

  }
}