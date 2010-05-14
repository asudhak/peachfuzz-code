
//
// Copyright (c) Michael Eddington
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in	
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

// Authors:
//   Michael Eddington (mike@phed.org)

// $Id$

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace DtdFuzzer
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				Console.WriteLine("\n[ Peach DTD XML Fuzzer v1.0 DEV");
				Console.WriteLine("[ Copyright (c) Michael Eddington\n");

				if (args.Length == 0 || args.Length > 1)
					syntax();

				Console.WriteLine(" * Using DTD '" + args[0] + "'.");

				TextReader reader = new StreamReader(args[0]);
				Parser parser = new Parser();
				parser.parse(reader);

				Generator generator = new Generator(null, parser.elements);
				XmlDocument doc = generator.GenerateXmlDocument();
				
				Console.WriteLine(doc.OuterXml);
				Console.WriteLine("\n [ Press Any Key To Continue ]");
				Console.ReadKey();
			}
			catch (SyntaxException)
			{
			}
		}

		static void syntax()
		{
			string syntax = @"
This is the experimental XML generation fuzzer.  It will consume a DTD
XML definition and use it to produce structurally correct XML documents.

Please submit any bugs to Michael Eddington <mike@phed.org>.

Syntax:

  DtdFuzzer.exe schema.dtd

";
			Console.WriteLine(syntax);
			throw new SyntaxException();
		}

		public class SyntaxException : Exception
		{
		}
	}
}

// end
