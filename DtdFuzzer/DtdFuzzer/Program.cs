using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DtdFuzzer
{
	class Program
	{
		static void Main(string[] args)
		{
			TextReader reader = new StreamReader(args[0]);
			Parser parser = new Parser();
			parser.parse(reader);
			string s = "A";
		}
	}
}
