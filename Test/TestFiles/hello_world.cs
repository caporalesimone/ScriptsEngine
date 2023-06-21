using System;

namespace HelloWorld
{
  class Hello
  {
    public Hello()
    {
		Console.WriteLine("Constructor");
    }
	
	public void Run()
	{
		Console.WriteLine("Premi un tasto per terminare il programma");
		Console.ReadKey();
	}
  }
}