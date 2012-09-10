using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Analyzers;
using Peach.Core.IO;
using NUnit.Framework;

namespace Peach.Core.Test.MutationStrategies
{
	[TestFixture]
	class RandomDeterministicTest : DataModelCollector
	{
		[Test]
		public void Test1()
		{
			// testing expansion of the buffer, expand the buffer size by 10

			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
				"<Peach>" +
				"   <DataModel name=\"TheDataModel\">" +
				"       <String name=\"str1\" value=\"Hello World!\"/>" +
				"       <String name=\"str2\" value=\"Hello World!\"/>" +
				"       <String name=\"str3\" value=\"Hello World!\"/>" +
				"       <String name=\"str4\" value=\"Hello World!\"/>" +
				"       <String name=\"str5\" value=\"Hello World!\"/>" +
				"   </DataModel>" +

				"   <StateModel name=\"TheState\" initialState=\"Initial\">" +
				"       <State name=\"Initial\">" +
				"           <Action type=\"output\">" +
				"               <DataModel ref=\"TheDataModel\"/>" +
				"           </Action>" +
				"       </State>" +
				"   </StateModel>" +

				"   <Test name=\"Default\">" +
				"       <StateModel ref=\"TheState\"/>" +
				"       <Publisher class=\"Null\"/>" +
				"       <Strategy class=\"RandomDeterministic\"/>" +
				"   </Test>" +

				"</Peach>";

			PitParser parser = new PitParser();

			Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			dom.tests[0].includedMutators = new List<string>();
			dom.tests[0].includedMutators.Add("UnicodeBomMutator");
			dom.tests[0].includedMutators.Add("StringCaseMutator");
			dom.tests[0].includedMutators.Add("StringMutator");

			RunConfiguration config = new RunConfiguration();

			Engine e = new Engine(null);
			e.config = config;
			e.startFuzzing(dom, config);

			// verify values
			// 5 strings, BOM - 1413 mutations, StringCase - 3 mutations, String - 2379 mutations
			Assert.AreEqual(5 * (1413 + 3 + 2379), mutations.Count);
			Assert.AreEqual(5 * 3, strategies.Count);

			// this strategy fuzzes elements in a consistently random order
			string[] expected = {
									"UnicodeBomMutator | TheDataModel.str5",
									"StringCaseMutator | TheDataModel.str1",
									"StringMutator | TheDataModel.str3",
									"StringMutator | TheDataModel.str2",
									"StringCaseMutator | TheDataModel.str5",
									"StringMutator | TheDataModel.str1",
									"StringMutator | TheDataModel.str5",
									"StringCaseMutator | TheDataModel.str2",
									"UnicodeBomMutator | TheDataModel.str3",
									"UnicodeBomMutator | TheDataModel.str2",
									"UnicodeBomMutator | TheDataModel.str1",
									"StringCaseMutator | TheDataModel.str3",
									"StringCaseMutator | TheDataModel.str4",
									"UnicodeBomMutator | TheDataModel.str4",
									"StringMutator | TheDataModel.str4",
								};

			for (int i = 0; i < expected.Length; ++i)
				Assert.AreEqual(expected[i], strategies[i]);
		}

	}
}
