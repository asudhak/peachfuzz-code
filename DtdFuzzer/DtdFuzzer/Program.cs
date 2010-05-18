
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

				if (args.Length == 0 || args.Length > 3)
					syntax();

				Console.WriteLine(" * Using DTD '" + args[0] + "'.");
				Console.WriteLine(" * Root element '" + args[1] + "'.");

				TextReader reader = new StreamReader(args[0]);
				Parser parser = new Parser();
				parser.parse(reader);

				if (args.Length > 2)
				{
					Console.Write(" * Loading Samples from '" + args[2] + "'");
					Defaults defaults = new Defaults(parser.elements, true);
					defaults.ProcessFolder(args[2]);
					Console.WriteLine("done.");
				}

				Generator generator = new Generator(parser.elements[args[1]], parser.elements);

				for (int i = 0; i < 100; i++)
				{
					Console.WriteLine("\n---vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv---");
					
					XmlDocument doc = generator.GenerateXmlDocument();
					Console.WriteLine(doc.OuterXml);
					if(File.Exists("fuzzed-" + i.ToString() + ".svg"))
						File.Delete("fuzzed-" + i.ToString() + ".svg");

					doc.DocumentElement.Attributes.Append(CreateXmlAttribute(doc, "xml:id", "svg-root"));
					doc.DocumentElement.Attributes.Append(CreateXmlAttribute(doc, "xmlns", "http://www.w3.org/2000/svg"));
					doc.DocumentElement.Attributes.Append(CreateXmlAttribute(doc, "viewbox", "0 0 480 360"));
					doc.DocumentElement.Attributes.Append(CreateXmlAttribute(doc, "width", "100%"));
					doc.DocumentElement.Attributes.Append(CreateXmlAttribute(doc, "height", "100%"));

					FileStream sout = File.OpenWrite("fuzzed-" + i.ToString() + ".svg");
					StreamWriter tout = new StreamWriter(sout);
					tout.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
					tout.WriteLine(doc.OuterXml);
					tout.Close();
					sout.Close();

					Console.WriteLine("\n---^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^---");
				}

				Console.WriteLine("\n [ Press Any Key To Continue ]");
				Console.ReadKey();
			}
			catch (SyntaxException)
			{
			}
		}

		protected static XmlAttribute CreateXmlAttribute(XmlDocument doc, string name, string value)
		{
			XmlAttribute a = doc.CreateAttribute(name);
			a.InnerText = value;
			return a;
		}

		static void syntax()
		{
			string syntax = @"
This is the experimental XML generation fuzzer.  It will consume a DTD
XML definition and use it to produce structurally correct XML documents.

Please submit any bugs to Michael Eddington <mike@phed.org>.

Syntax:

  DtdFuzzer.exe schema.dtd root_element [samples folder]

Example:

  DtdFuzzer.exe svg.dtd svg
  DtdFuzzer.exe svg.dtd svg c:\samples\svg

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
