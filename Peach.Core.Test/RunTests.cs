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

namespace Peach.Core.Test
{
	[TestFixture]
	class RunTests
	{
		DateTime iterationStarted;
		double iterationTimeSeconds = -1;

		[Test]
		public void TestWaitTime()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
				"<Peach>" +
				"   <DataModel name=\"TheDataModel\">" +
				"       <Blob name=\"blob1\"/>" +
				"   </DataModel>" +

				"   <StateModel name=\"TheState\" initialState=\"Initial\">" +
				"       <State name=\"Initial\">" +
				"           <Action type=\"output\">" +
				"               <DataModel ref=\"TheDataModel\"/>" +
				"           </Action>" +
				"       </State>" +
				"   </StateModel>" +

				"   <Test name=\"Default\" waitTime=\"5\">" +
				"       <StateModel ref=\"TheState\"/>" +
				"       <Publisher class=\"Null\"/>" +
				"       <Strategy class=\"Sequential\"/>" +
				"   </Test>" +
				"</Peach>";

			PitParser parser = new PitParser();

			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			dom.tests[0].includedMutators = new List<string>();
			dom.tests[0].includedMutators.Add("BlobMutator");

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.IterationStarting += new Engine.IterationStartingEventHandler(e_IterationStarting);
			e.IterationFinished += new Engine.IterationFinishedEventHandler(e_IterationFinished);
			e.startFuzzing(dom, config);

			// verify values
			Assert.GreaterOrEqual(5, iterationTimeSeconds);
		}
		void e_IterationFinished(RunContext context, uint currentIteration)
		{
			iterationTimeSeconds = (DateTime.Now - this.iterationStarted).TotalSeconds;
		}

		void e_IterationStarting(RunContext context, uint currentIteration, uint? totalIterations)
		{
			this.iterationStarted = DateTime.Now;
		}


		public void RunTest(uint start, uint replay, uint max = 100)
		{
			string template = @"
<Peach>
	<DataModel name='TheDataModel'>
		<String value='Hello World'/>
		<String value='Hello World'/>
		<String value='Hello World'/>
		<String value='Hello World'/>
	</DataModel>

	<StateModel name='TheState' initialState='Initial'>
		<State name='Initial'>
			<Action type='output'>
				<DataModel ref='TheDataModel'/>
			</Action>
		</State>
	</StateModel>

	<Agent name='LocalAgent'>
		<Monitor class='FaultingMonitor'>
			<Param name='Iteration' value='{0}'/>
			<Param name='Replay' value='true'/>
		</Monitor>
	</Agent>

	<Test name='Default' faultWaitTime='0' replayEnabled='true'>
		<Agent ref='LocalAgent'/>
		<StateModel ref='TheState'/>
		<Publisher class='Null'/>
	</Test>
</Peach>";

			iterationHistory.Clear();

			string xml = string.Format(template, replay);

			PitParser parser = new PitParser();

			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			dom.tests[0].includedMutators.Add("StringCaseMutator");

			RunConfiguration config = new RunConfiguration();
			config.range = true;
			config.rangeStart = start;
			config.rangeStop = uint.MaxValue;

			Engine e = new Engine(null);
			e.context.reproducingMaxBacksearch = max;
			e.IterationStarting += new Engine.IterationStartingEventHandler(r_IterationStarting);
			e.startFuzzing(dom, config);
		}

		List<uint> iterationHistory = new List<uint>();

		void r_IterationStarting(RunContext context, uint currentIteration, uint? totalIterations)
		{
			iterationHistory.Add(currentIteration);
		}

		[Test]
		public void TestMiddleSearch()
		{
			RunTest(1, 10);

			uint[] expected = new uint[] {
				1,  // Control
				1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
				10, // Initial replay
				9,  // Move back 1
				8,  // Move back 2
				6,  // Move back 4
				2,  // Move back 8
				1,  // Move back to beginning
				11, 12 };

			uint[] actual = iterationHistory.ToArray();
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void TestRangeSearch()
		{
			RunTest(6, 10);

			uint[] expected = new uint[] {
				6,  // Control
				6, 7, 8, 9, 10,
				10, // Initial replay
				9,  // Move back 1
				8,  // Move back 2
				6,  // Move back 4
				11, 12 };

			uint[] actual = iterationHistory.ToArray();
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void TestRangeBegin()
		{
			RunTest(6, 6);

			uint[] expected = new uint[] {
				6, // Control
				6, // Trigger replay
				6, // Only replay
				7, 8, 9, 10, 11, 12 };

			uint[] actual = iterationHistory.ToArray();
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void TestRangeMaxEqual()
		{
			RunTest(1, 10, 4);

			uint[] expected = new uint[] {
				1,  // Control
				1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
				10, // Initial replay
				9,  // Move back 1
				8,  // Move back 2
				6,  // Move back 4
				11, 12 };

			uint[] actual = iterationHistory.ToArray();
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void TestRangeMaxLess()
		{
			RunTest(1, 10, 5);

			uint[] expected = new uint[] {
				1,  // Control
				1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
				10, // Initial replay
				9,  // Move back 1
				8,  // Move back 2
				6,  // Move back 4
				11, 12 };

			uint[] actual = iterationHistory.ToArray();
			Assert.AreEqual(expected, actual);
		}
	}
}
