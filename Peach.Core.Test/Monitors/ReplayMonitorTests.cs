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

namespace Peach.Core.Test.Monitors
{
    [TestFixture]
    class ReplayMonitorTests : DataModelCollector
    {
        [Test]
        public void Test1()
        {
            // Test that the repeated iterations are producing the same values.

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <String name=\"str1\" value=\"Hello, World!\"/>" +
                "   </DataModel>" +

                "   <StateModel name=\"TheState\" initialState=\"Initial\">" +
                "       <State name=\"Initial\">" +
                "           <Action type=\"output\">" +
                "               <DataModel ref=\"TheDataModel\"/>" +
                "           </Action>" +
                "       </State>" +
                "   </StateModel>" +

                "   <Agent name=\"LocalAgent\">" +
                "       <Monitor class=\"FaultingMonitor\">" +
                "           <Param name=\"FaultAlways\" value=\"true\"/>" +
                "       </Monitor>" +
                "   </Agent>" +

                "   <Test name=\"Default\" replayEnabled=\"true\">" +
                "       <Agent ref=\"LocalAgent\"/>" +
                "       <StateModel ref=\"TheState\"/>" +
                "       <Publisher class=\"Null\"/>" +
                "       <Strategy class=\"RandomDeterministic\"/>" +
                "   </Test>" +
                "</Peach>";

            PitParser parser = new PitParser();

            Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
            dom.tests[0].includedMutators = new List<string>();
            dom.tests[0].includedMutators.Add("StringCaseMutator");

            RunConfiguration config = new RunConfiguration();

            Engine e = new Engine(null);
            e.startFuzzing(dom, config);

            // verify values
            Assert.AreEqual(6, mutations.Count);
            Assert.AreEqual((string)mutations[0], (string)mutations[1]);
            Assert.AreEqual((string)mutations[2], (string)mutations[3]);
            Assert.AreEqual((string)mutations[4], (string)mutations[5]);
        }


		[Test]
		public void ReplayControl()
		{
			string xml = @"
<Peach>
	<DataModel name='TheDataModel'>
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
			<Param name='Iteration' value='1'/>
		</Monitor>
	</Agent>

	<Test name='Default' faultWaitTime='0' replayEnabled='true'>
		<Agent ref='LocalAgent'/>
		<StateModel ref='TheState'/>
		<Publisher class='Null'/>
		<Strategy class='RandomDeterministic'/>
	</Test>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.Fault += _Fault;
			e.ReproFault += _ReproFault;

			try
			{
				e.startFuzzing(dom, config);
				Assert.Fail("Should throw!");
			}
			catch (PeachException ex)
			{
				Assert.AreEqual("Fault detected on control iteration.", ex.Message);
			}

			Assert.NotNull(faults);
			Assert.AreEqual(1, faults.Length);

			Assert.NotNull(reproFaults);
			Assert.AreEqual(1, reproFaults.Length);
		}

		Fault[] faults = null;
		Fault[] reproFaults = null;

		void _ReproFault(RunContext context, uint currentIteration, Dom.StateModel stateModel, Fault[] faultData)
		{
			Assert.Null(reproFaults);
			reproFaults = faultData;
		}

		void _Fault(RunContext context, uint currentIteration, Dom.StateModel stateModel, Fault[] faultData)
		{
			Assert.Null(faults);
			faults = faultData;
		}
    }
}

// end
