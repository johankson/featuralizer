using System;
using Mono.Addins;

namespace Addin2000
{
	[TypeExtensionPoint]
	public interface ICommand
	{
		void Run();
	}

	[Extension]
	public class HelloCommand : ICommand
	{
		public void Run()
		{
			Console.WriteLine("Hello World");
		}
	}
}
