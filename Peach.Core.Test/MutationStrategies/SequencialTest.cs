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
	class SequencialTest : DataModelCollector
	{
		[Test]
		public void Test1()
		{
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
				"       <Strategy class=\"Sequencial\"/>" +
				"   </Test>" +

				"   <Run name=\"DefaultRun\">" +
				"       <Test ref=\"TheTest\"/>" +
				"   </Run>" +
				"</Peach>";

			PitParser parser = new PitParser();

			Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			dom.tests[0].includedMutators = new List<string>();
			dom.tests[0].includedMutators.Add("UnicodeBomMutator");
			dom.tests[0].includedMutators.Add("StringMutator");
			dom.tests[0].includedMutators.Add("StringCaseMutator");

			RunConfiguration config = new RunConfiguration();

			Engine e = new Engine(null);
			e.config = config;
			e.startFuzzing(dom, config);

			// verify values
			// 5 strings, BOM - 1413 mutations, StringCase - 3 mutations, String - 2379 mutations
			Assert.AreEqual(5 * (1413 + 3 + 2379), mutations.Count);
			Assert.AreEqual(5 * 3, strategies.Count);

			// this strategy fuzzes elements in a sequencial order
			string[] expected = {
									"StringCaseMutator | TheDataModel.str1",
									"StringMutator | TheDataModel.str1",
									"UnicodeBomMutator | TheDataModel.str1",
									"StringCaseMutator | TheDataModel.str2",
									"StringMutator | TheDataModel.str2",
									"UnicodeBomMutator | TheDataModel.str2",
									"StringCaseMutator | TheDataModel.str3",
									"StringMutator | TheDataModel.str3",
									"UnicodeBomMutator | TheDataModel.str3",
									"StringCaseMutator | TheDataModel.str4",
									"StringMutator | TheDataModel.str4",
									"UnicodeBomMutator | TheDataModel.str4",
									"StringCaseMutator | TheDataModel.str5",
									"StringMutator | TheDataModel.str5",
									"UnicodeBomMutator | TheDataModel.str5",
								};

			for (int i = 0; i < expected.Length; ++i)
				Assert.AreEqual(expected[i], strategies[i]);
		}

		public void FuzzSequencial(RunConfiguration config)
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
				"<Peach>" +
				"   <DataModel name=\"TheDataModel\">" +
				"       <String name=\"str1\" value=\"String 1\"/>" +
				"       <String name=\"str2\" value=\"String 2\"/>" +
				"       <String name=\"str3\" value=\"String 3\"/>" +
				"       <String name=\"str4\" value=\"String 4\"/>" +
				"       <String name=\"str5\" value=\"String 5\"/>" +
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
				"       <Strategy class=\"Sequencial\"/>" +
				"   </Test>" +

				"   <Run name=\"DefaultRun\">" +
				"       <Test ref=\"TheTest\"/>" +
				"   </Run>" +
				"</Peach>";

			PitParser parser = new PitParser();

			Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			dom.tests[0].includedMutators = new List<string>();
			dom.tests[0].includedMutators.Add("StringCaseMutator");

			Engine e = new Engine(null);
			e.config = config;
			e.startFuzzing(dom, config);
		}

		[Test]
		public void Test2()
		{
			// Tests skipping to the 0th iteration
			RunConfiguration config = new RunConfiguration();
			config.skipToIteration = 0;
			FuzzSequencial(config);
			Assert.AreEqual(16, values.Count);
			Assert.AreEqual(15, mutations.Count);
		}

		[Test]
		public void Test3()
		{
			// Tests skipping to the 1st iteration
			RunConfiguration config = new RunConfiguration();
			config.skipToIteration = 1;
			FuzzSequencial(config);
			Assert.AreEqual(16, values.Count);
			Assert.AreEqual(15, mutations.Count);
		}

		[Test]
		public void Test4()
		{
			// Tests skipping a middle iteration
			RunConfiguration config = new RunConfiguration();
			config.skipToIteration = 12;
			FuzzSequencial(config);
			Assert.AreEqual(5, values.Count);
			Assert.AreEqual(4, mutations.Count);
		}

		[Test]
		public void Test5()
		{
			// Tests skipping past the last iteration
			RunConfiguration config = new RunConfiguration();
			config.skipToIteration = 16;
			FuzzSequencial(config);
			Assert.AreEqual(1, values.Count);
			Assert.AreEqual(0, mutations.Count);
		}

		[Test]
		public void Test6()
		{
			// Tests skipping way past the last iteration
			RunConfiguration config = new RunConfiguration();
			config.skipToIteration = 30;
			FuzzSequencial(config);
			Assert.AreEqual(1, values.Count);
			Assert.AreEqual(0, mutations.Count);
		}

		[Test]
		public void Test7()
		{
			RunConfiguration config = new RunConfiguration();
			config.range = true;
			config.rangeStart = 5;
			config.rangeStop = 5;
			FuzzSequencial(config);
			Assert.AreEqual(1, values.Count);
			Assert.AreEqual(0, mutations.Count);
		}

		[Test]
		public void Test8()
		{
			RunConfiguration config = new RunConfiguration();
			config.range = true;
			config.rangeStart = 5;
			config.rangeStop = 10;
			FuzzSequencial(config);
			Assert.AreEqual(6, values.Count);
			Assert.AreEqual(5, mutations.Count);
		}
	}
}
