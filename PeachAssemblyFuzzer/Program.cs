using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using Peach.Core.Language.DotNet;

namespace PeachAssemblyFuzzer
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.Out.WriteLine("\n] Peach .NET Assembly Fuzzer v0.1");
			Console.Out.WriteLine("] Copyright (c) Michael Eddington\n");

			if (args.Length == 0)
			{
				Console.Out.WriteLine("Syntax: PeachAssemblyFuzzer.exe MyDotNetAssembly.dll\n");
				return;
			}

			AssemblyFuzzer fuzzer = new AssemblyFuzzer();

			foreach (string a in args)
			{
				Console.Out.WriteLine("[*] Adding assembly: " + a);
				fuzzer.AddAssembly(Assembly.LoadFile(a));
			}

			Console.Out.WriteLine("[*] Starting fuzzer...");

			fuzzer.Run();

			Console.Out.WriteLine("\n -- All done! -- \n");
		}
	}
}
