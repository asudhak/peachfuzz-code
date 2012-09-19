using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Analyzers;
using Peach.Core.IO;
using Peach.Core.MutationStrategies;

namespace Peach.Core.Test.MutationStrategies
{
	[TestFixture]
	class RandomStrategyTests : DataModelCollector
	{
		[Test]
		public void Test1()
		{
			// Test fuzzing does something

			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
				"<Peach>" +
				"   <DataModel name=\"TheDataModel\">" +
				"       <Number name=\"num1\" size=\"32\" value=\"100\" signed=\"false\"/>" +
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
				"       <Strategy class=\"RandomStrategy\"/>" +
				"   </Test>" +
				"</Peach>";

			RunEngine(xml, 0, 1000);

			// verify values
			Assert.AreEqual(999, mutations.Count);
			Assert.AreEqual(999, allStrategies.Count);
		}

		[Test]
		public void Test2()
		{
			// Random strategy picks one data model to fuzz each iteration, make sure this is working

			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
				"<Peach>" +
				"   <DataModel name=\"TheDataModel1\">" +
				"       <Number name=\"num1\" size=\"32\" value=\"100\" signed=\"false\"/>" +
				"   </DataModel>" +

				"   <DataModel name=\"TheDataModel2\">" +
				"       <Number name=\"num1\" size=\"32\" value=\"100\" signed=\"false\"/>" +
				"   </DataModel>" +

				"   <StateModel name=\"TheState\" initialState=\"Initial\">" +
				"       <State name=\"Initial\">" +
				"           <Action name=\"Action1\" type=\"output\">" +
				"               <DataModel ref=\"TheDataModel1\"/>" +
				"           </Action>" +
				"           <Action name=\"Action2\" type=\"output\">" +
				"               <DataModel ref=\"TheDataModel2\"/>" +
				"           </Action>" +
				"       </State>" +
				"   </StateModel>" +

				"   <Test name=\"Default\">" +
				"       <StateModel ref=\"TheState\"/>" +
				"       <Publisher class=\"Null\"/>" +
				"       <Strategy class=\"RandomStrategy\"/>" +
				"   </Test>" +
				"</Peach>";

			RunEngine(xml, 0, 1000);

			// verify values
			int dm1 = 0;
			int dm2 = 0;
			foreach (var item in allStrategies)
			{
				if (item.Contains("TheDataModel1"))
					dm1 += 1;
				else if (item.Contains("TheDataModel2"))
					dm2 += 1;
			}

			Assert.AreEqual(2000, actions.Count);
			Assert.AreEqual(999, allStrategies.Count);
			Assert.AreEqual(allStrategies.Count, dm1 + dm2);

			// Make sure each data model was fuzzed about half the time
			Assert.Greater(dm1, 450);
			Assert.Less(dm1, 550);
			Assert.Greater(dm2, 450);
			Assert.Less(dm2, 550);
		}

		[Test]
		public void Test3()
		{
			// Test strategy only mutates a random number between 1 and MaxFieldsToMutate every iteration

			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
				"<Peach>" +
				"   <DataModel name=\"TheDataModel\">" +
				"       <Number name=\"num1\" size=\"32\" value=\"100\" signed=\"false\"/>" +
				"       <Number name=\"num2\" size=\"32\" value=\"100\" signed=\"false\"/>" +
				"       <Number name=\"num3\" size=\"32\" value=\"100\" signed=\"false\"/>" +
				"       <Number name=\"num4\" size=\"32\" value=\"100\" signed=\"false\"/>" +
				"       <Number name=\"num5\" size=\"32\" value=\"100\" signed=\"false\"/>" +
				"       <Number name=\"num6\" size=\"32\" value=\"100\" signed=\"false\"/>" +
				"       <Number name=\"num7\" size=\"32\" value=\"100\" signed=\"false\"/>" +
				"       <Number name=\"num8\" size=\"32\" value=\"100\" signed=\"false\"/>" +
				"       <Number name=\"num9\" size=\"32\" value=\"100\" signed=\"false\"/>" +
				"       <Number name=\"num10\" size=\"32\" value=\"100\" signed=\"false\"/>" +
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
				"       <Strategy class=\"RandomStrategy\">" +
				"           <Param name=\"MaxFieldsToMutate\" value=\"5\"/>" +
				"       </Strategy>" +
				"   </Test>" +
				"</Peach>";

			RunEngine(xml, 0, 1000);

			// verify values
			// Random number between 1 and 5 is on average 3, for 1000 iterations is 3000 mutations
			Assert.AreEqual(1000, actions.Count);
			Assert.Greater(allStrategies.Count, 2900);
			Assert.Less(allStrategies.Count, 3100);
		}

		[Test]
		public void Test4()
		{
			// Test that subsequent runs of the same seed produce identical results

			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
				"<Peach>" +
				"   <DataModel name=\"TheDataModel1\">" +
				"       <Number name=\"num1\" size=\"32\" value=\"100\" signed=\"false\"/>" +
				"       <Number name=\"num2\" size=\"32\" value=\"100\" signed=\"false\"/>" +
				"       <Number name=\"num3\" size=\"32\" value=\"100\" signed=\"false\"/>" +
				"       <Number name=\"num4\" size=\"32\" value=\"100\" signed=\"false\"/>" +
				"   </DataModel>" +

				"   <DataModel name=\"TheDataModel2\">" +
				"       <Number name=\"num1\" size=\"32\" value=\"100\" signed=\"false\"/>" +
				"       <Number name=\"num2\" size=\"32\" value=\"100\" signed=\"false\"/>" +
				"       <Number name=\"num3\" size=\"32\" value=\"100\" signed=\"false\"/>" +
				"       <Number name=\"num4\" size=\"32\" value=\"100\" signed=\"false\"/>" +
				"   </DataModel>" +

				"   <DataModel name=\"TheDataModel3\">" +
				"       <Number name=\"num1\" size=\"32\" value=\"100\" signed=\"false\"/>" +
				"       <Number name=\"num2\" size=\"32\" value=\"100\" signed=\"false\"/>" +
				"       <Number name=\"num3\" size=\"32\" value=\"100\" signed=\"false\"/>" +
				"       <Number name=\"num4\" size=\"32\" value=\"100\" signed=\"false\"/>" +
				"   </DataModel>" +

				"   <StateModel name=\"TheState\" initialState=\"Initial\">" +
				"       <State name=\"Initial\">" +
				"           <Action name=\"Action1\" type=\"output\">" +
				"               <DataModel ref=\"TheDataModel1\"/>" +
				"           </Action>" +
				"           <Action name=\"Action2\" type=\"output\">" +
				"               <DataModel ref=\"TheDataModel2\"/>" +
				"           </Action>" +
				"           <Action name=\"Action3\" type=\"output\">" +
				"               <DataModel ref=\"TheDataModel3\"/>" +
				"           </Action>" +
				"       </State>" +
				"   </StateModel>" +

				"   <Test name=\"Default\">" +
				"       <StateModel ref=\"TheState\"/>" +
				"       <Publisher class=\"Null\"/>" +
				"       <Strategy class=\"RandomStrategy\"/>" +
				"   </Test>" +
				"</Peach>";

			RunEngine(xml, 0, 1000);

			// Sanity check
			Assert.AreEqual(3000, actions.Count);

			var oldStrategies = allStrategies;
			var oldActions = actions;

			// Reset the DataModelCollector
			ResetContainers();

			RunEngine(xml, 0, 1000);

			// Verify
			VerifySameResults(oldStrategies, oldActions);
		}

		[Test]
		public void Test5()
		{
			// Test that subsequent runs of the same seed produce identical results
			// when the second run includes a subset of iterations of the first

			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
				"<Peach>" +
				"   <DataModel name=\"TheDataModel1\">" +
				"       <Number name=\"num1\" size=\"32\" value=\"100\" signed=\"false\"/>" +
				"       <Number name=\"num2\" size=\"32\" value=\"100\" signed=\"false\"/>" +
				"       <Number name=\"num3\" size=\"32\" value=\"100\" signed=\"false\"/>" +
				"       <Number name=\"num4\" size=\"32\" value=\"100\" signed=\"false\"/>" +
				"   </DataModel>" +

				"   <DataModel name=\"TheDataModel2\">" +
				"       <Number name=\"num1\" size=\"32\" value=\"100\" signed=\"false\"/>" +
				"       <Number name=\"num2\" size=\"32\" value=\"100\" signed=\"false\"/>" +
				"       <Number name=\"num3\" size=\"32\" value=\"100\" signed=\"false\"/>" +
				"       <Number name=\"num4\" size=\"32\" value=\"100\" signed=\"false\"/>" +
				"   </DataModel>" +

				"   <DataModel name=\"TheDataModel3\">" +
				"       <Number name=\"num1\" size=\"32\" value=\"100\" signed=\"false\"/>" +
				"       <Number name=\"num2\" size=\"32\" value=\"100\" signed=\"false\"/>" +
				"       <Number name=\"num3\" size=\"32\" value=\"100\" signed=\"false\"/>" +
				"       <Number name=\"num4\" size=\"32\" value=\"100\" signed=\"false\"/>" +
				"   </DataModel>" +

				"   <StateModel name=\"TheState\" initialState=\"Initial\">" +
				"       <State name=\"Initial\">" +
				"           <Action name=\"Action1\" type=\"output\">" +
				"               <DataModel ref=\"TheDataModel1\"/>" +
				"           </Action>" +
				"           <Action name=\"Action2\" type=\"output\">" +
				"               <DataModel ref=\"TheDataModel2\"/>" +
				"           </Action>" +
				"           <Action name=\"Action3\" type=\"output\">" +
				"               <DataModel ref=\"TheDataModel3\"/>" +
				"           </Action>" +
				"       </State>" +
				"   </StateModel>" +

				"   <Test name=\"Default\">" +
				"       <StateModel ref=\"TheState\"/>" +
				"       <Publisher class=\"Null\"/>" +
				"       <Strategy class=\"RandomStrategy\"/>" +
				"   </Test>" +
				"</Peach>";

			RunEngine(xml, 0, 1000);

			// Sanity check
			Assert.AreEqual(3000, actions.Count);

			var oldStrategies = allStrategies;
			var oldActions = actions;

			// Reset the DataModelCollector
			ResetContainers();

			RunEngine(xml, 501, 1000);

			// Sanity check
			Assert.AreEqual(1500, actions.Count);

			oldStrategies.RemoveRange(0, oldStrategies.Count - allStrategies.Count);
			oldActions.RemoveRange(0, oldActions.Count - actions.Count);

			// Verify
			VerifySameResults(oldStrategies, oldActions);
		}

		[Test]
		public void Test6()
		{
			// Test that the random strategy properly cycles through data models on the specified switch count
			string temp1 = Path.GetTempFileName();
			string temp2 = Path.GetTempFileName();

			File.WriteAllBytes(temp1, Encoding.ASCII.GetBytes("Hello\x00World\x00"));
			File.WriteAllBytes(temp2, Encoding.ASCII.GetBytes("Foo\x00"));

			// Test loading a dataset from a file
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
				"<Peach>" +
				"   <DataModel name=\"TheDataModel\">" +
				"       <String name=\"str1\" value=\"Initial\" maxOccurs=\"100\" nullTerminated=\"true\"/>" +
				"   </DataModel>" +

				"   <StateModel name=\"TheState\" initialState=\"Initial\">" +
				"       <State name=\"Initial\">" +
				"           <Action type=\"output\">" +
				"               <DataModel ref=\"TheDataModel\"/>" +
				"               <Data fileName=\"" + temp1 + "\"/>" +
				"               <Data fileName=\"" + temp2 + "\"/>" +
				"           </Action>" +
				"       </State>" +
				"   </StateModel>" +

				"   <Test name=\"Default\">" +
				"       <StateModel ref=\"TheState\"/>" +
				"       <Publisher class=\"Null\"/>" +
				"       <Strategy class=\"RandomStrategy\">" +
				"           <Param name=\"SwitchCount\" value=\"10\"/>" +
				"       </Strategy>" +
				"   </Test>" +
				"</Peach>";

			PitParser parser = new PitParser();

			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			dom.tests[0].includedMutators = new List<string>();
			dom.tests[0].includedMutators.Add("StringCaseMutator");

			RunConfiguration config = new RunConfiguration();
			config.rangeStart = 0;
			config.rangeStop = 50;
			config.range = true;
			config.randomSeed = 12345;

			Engine e = new Engine(null);
			e.config = config;
			e.startFuzzing(dom, config);

			Assert.AreEqual(49, mutations.Count);
			Assert.AreEqual(50, dataModels.Count);

			int lastSize = 0;

			// Skip data model 0, its the magical 1st pass w/o mutations
			for (int i = 1; i < 50; ++i)
			{
				Assert.AreEqual(1, dataModels[i].Count);
				Dom.Array item = dataModels[i][0] as Dom.Array;

				// Its either an array of 1 or an array of 2
				Assert.GreaterOrEqual(item.Count, 1);
				Assert.LessOrEqual(item.Count, 2);

				if (lastSize != item.Count)
				{
					// Change of data model should only occur at iteration 1, 11, 21, 31, 41
					Assert.AreEqual(1, i % 10);
					lastSize = item.Count;
				}

			}
		}

		[Test]
		public void Test7()
		{
			// Test that the random strategy is reproducable when starting at an
			// arbitrary iteration when configured to cycles through data models
			// with multiple actions
			string temp1 = Path.GetTempFileName();
			string temp2 = Path.GetTempFileName();
			string temp3 = Path.GetTempFileName();
			string temp4 = Path.GetTempFileName();

			File.WriteAllBytes(temp1, Encoding.ASCII.GetBytes("Foo\u0000"));
			File.WriteAllBytes(temp2, Encoding.ASCII.GetBytes("Foo\u0000Bar\u0000"));
			File.WriteAllBytes(temp3, Encoding.ASCII.GetBytes("Foo\u0000Bar\u0000Baz\u0000"));
			File.WriteAllBytes(temp4, Encoding.ASCII.GetBytes("Foo\u0000Bar\u0000Baz\u0000Qux\u0000"));

			// Test loading a dataset from a file
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
				"<Peach>" +
				"   <DataModel name=\"TheDataModel1\">" +
				"       <String name=\"str1\" value=\"Initial\" maxOccurs=\"100\" nullTerminated=\"true\"/>" +
				"   </DataModel>" +

				"   <DataModel name=\"TheDataModel2\">" +
				"       <String name=\"str1\" value=\"Initial\" maxOccurs=\"100\" nullTerminated=\"true\"/>" +
				"   </DataModel>" +

				"   <StateModel name=\"TheState\" initialState=\"Initial\">" +
				"       <State name=\"Initial\">" +
				"           <Action type=\"output\">" +
				"               <DataModel ref=\"TheDataModel1\"/>" +
				"               <Data fileName=\"" + temp1 + "\"/>" +
				"               <Data fileName=\"" + temp2 + "\"/>" +
				"           </Action>" +
				"           <Action type=\"output\">" +
				"               <DataModel ref=\"TheDataModel2\"/>" +
				"               <Data fileName=\"" + temp3 + "\"/>" +
				"               <Data fileName=\"" + temp4 + "\"/>" +
				"           </Action>" +
				"       </State>" +
				"   </StateModel>" +

				"   <Test name=\"Default\">" +
				"       <StateModel ref=\"TheState\"/>" +
				"       <Publisher class=\"Null\"/>" +
				"       <Strategy class=\"RandomStrategy\">" +
				"           <Param name=\"SwitchCount\" value=\"10\"/>" +
				"       </Strategy>" +
				"   </Test>" +
				"</Peach>";

			RunSwitchTest(xml, 0, 100);
			Assert.AreEqual(200, dataModels.Count);
			var oldDataModels = dataModels;

			ResetContainers();
			Assert.AreEqual(0, dataModels.Count);

			RunSwitchTest(xml, 47, 100);
			Assert.AreEqual(108, dataModels.Count);

			oldDataModels.RemoveRange(0, oldDataModels.Count - dataModels.Count);
			Assert.AreEqual(dataModels.Count, oldDataModels.Count);

			// Because there are two actions, the first two entries in dataModels are the 0th iteration
			for (int i = 2; i < dataModels.Count; ++i)
			{
				Assert.AreEqual(1, dataModels[i].Count);
				Assert.AreEqual(1, oldDataModels[i].Count);

				Dom.Array item = dataModels[i][0] as Dom.Array;
				Dom.Array oldItem = oldDataModels[i][0] as Dom.Array;

				Assert.AreNotEqual(null, item);
				Assert.AreNotEqual(null, oldItem);

				Assert.AreEqual(item.Count, oldItem.Count);

				for (int j = 0; j < item.Count; ++j)
				{
					Dom.String str = item[j] as Dom.String;
					Dom.String oldStr = oldItem[j] as Dom.String;

					Assert.AreNotEqual(null, str);
					Assert.AreNotEqual(null, oldStr);
					Assert.AreEqual(str.InternalValue, oldStr.InternalValue);
				}
			}
		}

		private static void RunSwitchTest(string xml, uint start, uint stop)
		{
			PitParser parser = new PitParser();

			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			dom.tests[0].includedMutators = new List<string>();
			dom.tests[0].includedMutators.Add("StringCaseMutator");

			RunConfiguration config = new RunConfiguration();
			config.rangeStart = start;
			config.rangeStop = stop;
			config.range = true;
			config.randomSeed = 12345;

			Engine e = new Engine(null);
			e.config = config;
			e.startFuzzing(dom, config);
		}

		private void VerifySameResults(List<string> oldStrategies, List<Dom.Action> oldActions)
		{
			Assert.AreEqual(allStrategies.Count, oldStrategies.Count);
			Assert.AreEqual(actions.Count, oldActions.Count);

			for (int i = 0; i < allStrategies.Count; ++i)
			{
				Assert.AreEqual(allStrategies[i], oldStrategies[i]);
			}

			for (int i = 0; i < actions.Count; ++i)
			{
				Assert.AreEqual(actions[i].name, oldActions[i].name);
				Assert.AreEqual(actions[i].dataModel.name, oldActions[i].dataModel.name);
				var oldDataModel = oldActions[i].dataModel;
				var dataModel = actions[i].dataModel;

				Assert.AreEqual(4, oldDataModel.Count);
				Assert.AreEqual(4, dataModel.Count);

				for (int j = 0; j < 4; ++j)
				{
					var lhs = oldDataModel[j].InternalValue;
					var rhs = dataModel[j].InternalValue;
					Assert.AreEqual(lhs, rhs);
				}
			}
		}

		private void RunEngine(string xml, uint start, uint stop)
		{
			PitParser parser = new PitParser();

			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			dom.tests[0].includedMutators = new List<string>();
			dom.tests[0].includedMutators.Add("NumericalVarianceMutator");
			dom.tests[0].includedMutators.Add("NumericalEdgeCaseMutator");

			RunConfiguration config = new RunConfiguration();
			config.rangeStart = start;
			config.rangeStop = stop;
			config.range = true;
			config.randomSeed = 12345;

			Engine e = new Engine(null);
			e.config = config;
			e.startFuzzing(dom, config);
		}
	}
}

// end
