using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Peach.Core;
using Peach.Core.Agent.Monitors;
using NUnit.Framework;
using System.Threading;
using Peach.Core.Analyzers;
using System.IO;
using System.Text;

namespace Peach.Core.Test.Agent.Monitors
{
	[TestFixture]
	public class WindowsDebuggerHybridTest
	{
		Fault[] faults = null;

		[SetUp]
		public void SetUp()
		{
			faults = null;

			if (!Environment.Is64BitProcess && Environment.Is64BitOperatingSystem)
				Assert.Ignore("Cannot run the 32bit version of this test on a 64bit operating system.");

			if (Environment.Is64BitProcess && !Environment.Is64BitOperatingSystem)
				Assert.Ignore("Cannot run the 64bit version of this test on a 32bit operating system.");
		}

		void _Fault(RunContext context, uint currentIteration, Dom.StateModel stateModel, Fault[] faults)
		{
			Assert.Null(this.faults);
			Assert.True(context.reproducingFault);
			Assert.AreEqual(1, context.reproducingInitialIteration);
			this.faults = faults;
		}

		string xml = @"
<Peach>
	<DataModel name='TheDataModel'>
		<String value='Hello'/>
	</DataModel>

	<StateModel name='TheState' initialState='Initial'>
		<State name='Initial'>
			<Action type='output'>
				<DataModel ref='TheDataModel'/>
			</Action>
		</State>
	</StateModel>

	<Agent name='LocalAgent'>
		<Monitor class='WindowsDebugger'>
			<Param name='CommandLine' value='CrashableServer.exe 127.0.0.1 44444'/>
		</Monitor>
	</Agent>

	<Test name='Default'>
		<Agent ref='LocalAgent'/>
		<StateModel ref='TheState'/>
		<Publisher class='Tcp'>
			<Param name='Host' value='127.0.0.1'/>
			<Param name='Port' value='44444'/>
		</Publisher>
	</Test>
</Peach>";

		[Test, Ignore]
		public void TestNoFault()
		{
			PitParser parser = new PitParser();

			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			dom.tests[0].includedMutators = new List<string>();
			dom.tests[0].includedMutators.Add("StringCaseMutator");

			RunConfiguration config = new RunConfiguration();

			Engine e = new Engine(null);
			e.Fault += _Fault;
			e.startFuzzing(dom, config);

			Assert.Null(this.faults);
		}

		[Test, Ignore]
		public void TestFault()
		{
			PitParser parser = new PitParser();

			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			dom.tests[0].includedMutators = new List<string>();
			dom.tests[0].includedMutators.Add("UnicodeUtf8ThreeCharMutator");

			RunConfiguration config = new RunConfiguration();
			config.range = true;
			config.rangeStart = 1;
			config.rangeStop = 1;

			Engine e = new Engine(null);
			e.Fault += _Fault;
			e.startFuzzing(dom, config);

			Assert.NotNull(this.faults);
			Assert.AreEqual(1, this.faults.Length);
			Assert.AreEqual(FaultType.Fault, this.faults[0].type);
			Assert.AreEqual("WindowsDebuggerHybrid", this.faults[0].detectionSource);
		}
	}
}
