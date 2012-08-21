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
				"       <Strategy class=\"RandomStrategy\">" +
				"           <Param name=\"Seed\" value=\"12345\"/>" +
				"       </Strategy>" +
				"   </Test>" +
				"</Peach>";

			RunEngine(xml, 0, 1000);

			// Sanity check
			Assert.AreEqual(3000, actions.Count);

			var oldStrategies = strategies;
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
				"       <Strategy class=\"RandomStrategy\">" +
				"           <Param name=\"Seed\" value=\"12345\"/>" +
				"       </Strategy>" +
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

			oldStrategies.RemoveRange(0, oldStrategies.Count - strategies.Count);
			oldActions.RemoveRange(0, oldActions.Count - actions.Count);

			// Verify
			VerifySameResults(oldStrategies, oldActions);
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

			Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			dom.tests[0].includedMutators = new List<string>();
			dom.tests[0].includedMutators.Add("NumericalVarianceMutator");
			dom.tests[0].includedMutators.Add("NumericalEdgeCaseMutator");

			RunConfiguration config = new RunConfiguration();
			config.rangeStart = start;
			config.rangeStop = stop;
			config.range = true;

			Engine e = new Engine(null);
			e.config = config;
			e.startFuzzing(dom, config);
		}
	}
}

// end
