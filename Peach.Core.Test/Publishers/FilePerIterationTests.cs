using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core;
using Peach.Core.Analyzers;
using System.IO;

namespace Peach.Core.Test.Publishers
{
	[TestFixture]
	class FilePerIterationTests
	{
		void RunTest(string tempFile, uint stop)
		{
			string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
				<Peach>
				   <DataModel name=""TheDataModel"">
				       <String name=""foo"" value=""Hello""/>
				   </DataModel>

				   <StateModel name=""TheStateModel"" initialState=""InitialState"">
				       <State name=""InitialState"">
				           <Action type=""output"">
				               <DataModel ref=""TheDataModel""/>
				           </Action>
				       </State>
				   </StateModel>

				   <Test name=""Default"">
				       <StateModel ref=""TheStateModel""/>
				       <Publisher class=""FilePerIteration"">
				           <Param name=""FileName"" value=""{0}""/>
				       </Publisher>
				       <Strategy class=""Sequential""/>
				   </Test>

				</Peach>";

			xml = string.Format(xml, tempFile);

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			dom.tests[0].includedMutators = new List<string>();
			dom.tests[0].includedMutators.Add("StringMutator");

			RunConfiguration config = new RunConfiguration();
			config.range = true;
			config.rangeStart = 1;
			config.rangeStop = stop;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);
		}

		[Test]
		public void Test1()
		{
			// Simple output of a single iteration

			string tempFile = Path.GetTempFileName() + ".{0}";
			RunTest(tempFile, 10);

			// From StringMutator
			string[] expected = {
				"Hello",
				"Peach",
				"abcdefghijklmnopqrstuvwxyz",
				"ABCDEFGHIJKLMNOPQRSTUVWXYZ",
				"0123456789",
				"",
				"10",
				"0.0",
				"1.0",
				"0.1",
			};

			for (uint i = 0; i < 10; ++i)
			{
				string file = string.Format(tempFile, i);

				if (i == 0)
					file = string.Format(tempFile, 1) + ".Control";

				string[] result = File.ReadAllLines(file);

				if (expected[i].Length > 0)
				{
					Assert.AreEqual(1, result.Length);
					Assert.AreEqual(expected[i], result[0]);
				}
				else
				{
					Assert.AreEqual(0, result.Length);
				}
			}
		}

		[Test,  ExpectedException(typeof(PeachException))]
		public void Test2()
		{
			// Ensure the FileName parameter actually contains a format identifier
			string tempFile = Path.GetTempFileName();
			RunTest(tempFile, 0);
		}

		[Test, ExpectedException(typeof(PeachException))]
		public void Test3()
		{
			// Ensure the FileName parameter contains a valid format identifier
			string tempFile = Path.GetTempFileName() + ".{0}.{1}";
			RunTest(tempFile, 0);
		}
	}
}
