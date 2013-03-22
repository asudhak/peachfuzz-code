
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
//   Michael Eddington (mike@dejavusecurity.com)

// $Id$

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Windows.Forms;

using Peach.Core.Xml;
using System.Reflection;

namespace PeachXmlGenerator
{
	class Program
	{
		static void DisplayTitle()
		{
			Console.WriteLine("");
			Console.WriteLine("[ Peach v3.0");
			Console.WriteLine("[ Peach DTD XML Fuzzer v{0}", Assembly.GetExecutingAssembly().GetName().Version);
			Console.WriteLine("[ Copyright (c) Michael Eddington\n");
		}

		static void Main(string[] args)
		{
			try
			{
				XmlDocument doc = null;
				string dtdFile = null;
				string rootElementName = null;
				string samplesFolder = null;
				int? iterations = null;
				string xmlns = null;

				var p = new OptionSet()
				{
					{ "h|?|help", v => syntax() },
					{ "x|xmlns=", v => xmlns = v },
					{ "d|dtd=", v => dtdFile = v },
					{ "c|count=", v => iterations = int.Parse(v)},
					{ "r|root=", v => rootElementName = v },
					{ "s|samples=", v => samplesFolder = v },
				};

				p.Parse(args);

				if (dtdFile == null || rootElementName == null)
				{
					Application.EnableVisualStyles();
					Application.SetCompatibleTextRenderingDefault(false);
					Application.Run(new FormMain());
					return;
				}

				DisplayTitle();

				Console.WriteLine(" * Using DTD '" + dtdFile + "'.");
				Console.WriteLine(" * Root element '" + rootElementName + "'.");

				TextReader reader = new StreamReader(dtdFile);
				Parser parser = new ParserDtd();
				parser.parse(reader);

				if (samplesFolder != null)
				{
					Console.Write(" * Loading Samples from '" + samplesFolder + "'...");
					Defaults defaults = new Defaults(parser.elements, true);
					defaults.ProcessFolder(samplesFolder);
					Console.WriteLine("done.");
				}

				Generator generator = new Generator(parser.elements[rootElementName], parser.elements);

				Console.Write(" * Generating XML files...");

				for (int i = 0; true; i++)
				{
					if (iterations != null && i >= iterations)
						break;

					if (i % 100 == 0)
						Console.Write(".");

					doc = generator.GenerateXmlDocument();

					if(xmlns != null)
						doc.DocumentElement.Attributes.Append(CreateXmlAttribute(doc, "xmlns", xmlns));

					if (File.Exists("fuzzed-" + i.ToString() + ".svg"))
						File.Delete("fuzzed-" + i.ToString() + ".svg");

					using (FileStream sout = File.OpenWrite("fuzzed-" + i.ToString() + ".svg"))
					{
						using (StreamWriter tout = new StreamWriter(sout))
						{
							tout.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
							tout.WriteLine(doc.OuterXml);
						}
					}
				}

				Console.WriteLine("done.\n");
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
			DisplayTitle();

			string syntax = @"
This is the experimental XML generation fuzzer.  It will consume a DTD
XML definition and use it to produce structurally correct XML documents.

Please submit any bugs to Michael Eddington <mike@dejavusecurity.com>.

Syntax:

  PeachXmlGenerator.exe -d schema.dtd -r root_element [-s samples folder] [-x namespace] [-c 100]

  -d/--dtd=     DTD Schema file [required]
  -r/--root=    Root XML element [required]
  -c/--count=   Number of XML files to produce [optional]
  -s/--samples= Sample XML folder [optional]
  -x/--xmlns=   Root XML namespace [optional]

Example:

  PeachXmlGenerator.exe -d svg.dtd -r svg
  PeachXmlGenerator.exe -d svg.dtd -r svg -x http://www.w3.org/2000/svg
  PeachXmlGenerator.exe -d svg.dtd -r svg -x http://www.w3.org/2000/svg -s c:\samples\svg
  PeachXmlGenerator.exe -d svg.dtd -r svg -x http://www.w3.org/2000/svg -s c:\samples\svg -c 100

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
