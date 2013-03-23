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

			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			dom.tests[0].includedMutators = new List<string>();
			dom.tests[0].includedMutators.Add("UnicodeBomMutator");
			dom.tests[0].includedMutators.Add("StringCaseMutator");
			dom.tests[0].includedMutators.Add("StringMutator");

			RunConfiguration config = new RunConfiguration();

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			// verify values
			// 5 strings, BOM - 1413 mutations, StringCase - 3 mutations, String - 2379 mutations
			int expected = 5 * (1413 + 3 + 2379);
			Assert.AreEqual(expected, mutations.Count);

			// this strategy fuzzes elements in a consistently random order
			// that appears to be random.  should not run the same strategy twice
			// for > 85% of the time.
			Assert.Greater(strategies.Count, (expected * 85) / 100);
		}

	}
}
