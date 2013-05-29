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

		void _AppendFault(RunContext context, uint currentIteration, Dom.StateModel stateModel, Fault[] faults)
		{
			List<Fault> tmp = new List<Fault>();
			if (this.faults != null)
				tmp.AddRange(this.faults);

			tmp.AddRange(faults);
			this.faults = tmp.ToArray();
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

			RunConfiguration config = new RunConfiguration();
			config.range = true;
			config.rangeStart = 2;
			config.rangeStop = 2;

			Engine e = new Engine(null);
			e.Fault += _Fault;
			e.startFuzzing(dom, config);

			Assert.NotNull(this.faults);
			Assert.AreEqual(1, this.faults.Length);
			Assert.AreEqual(FaultType.Fault, this.faults[0].type);
			Assert.AreEqual("WindowsDebuggerHybrid", this.faults[0].detectionSource);
		}

		[Test]
		public void TestEarlyExit()
		{
			string pit = @"
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
			<Param name='CommandLine' value='CrashingFileConsumer.exe'/>
			<Param name='FaultOnEarlyExit' value='true'/>
		</Monitor>
	</Agent>

	<Test name='Default' replayEnabled='true'>
		<Agent ref='LocalAgent'/>
		<StateModel ref='TheState'/>
		<Publisher class='Null'/>
	</Test>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(pit)));

			RunConfiguration config = new RunConfiguration();
			config.range = true;
			config.rangeStart = 1;
			config.rangeStop = 1;

			Engine e = new Engine(null);
			e.Fault += _AppendFault;
			e.ReproFault += _AppendFault;

			try
			{
				e.startFuzzing(dom, config);
				Assert.Fail("Should throw!");
			}
			catch (PeachException ex)
			{
				Assert.AreEqual("Fault detected on control iteration.", ex.Message);
			}

			Assert.NotNull(this.faults);
			Assert.AreEqual(2, this.faults.Length);
			Assert.AreEqual(FaultType.Fault, this.faults[0].type);
			Assert.AreEqual("SystemDebugger", this.faults[0].detectionSource);
			Assert.AreEqual(FaultType.Fault, this.faults[1].type);
			Assert.AreEqual("WindowsDebugEngine", this.faults[1].detectionSource);
		}

		[Test]
		public void TestExitEarlyFault()
		{
			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["CommandLine"] = new Variant("CrashingFileConsumer.exe");
			args["FaultOnEarlyExit"] = new Variant("true");

			var w = new WindowsDebuggerHybrid(null, "name", args);
			w.IterationStarting(1, false);

			System.Threading.Thread.Sleep(1000);

			w.IterationFinished();

			Assert.AreEqual(true, w.DetectedFault());
			Fault f = w.GetMonitorData();
			Assert.NotNull(f);
			Assert.AreEqual("ProcessExitedEarly", f.folderName);

			w.SessionFinished();
			w.StopMonitor();
		}

		[Test]
		public void TestExitEarlyFault1()
		{
			Variant foo = new Variant("foo");
			Variant bar = new Variant("bar");

			// FaultOnEarlyExit doesn't fault when stop message is sent

			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["CommandLine"] = new Variant("CrashingFileConsumer.exe");
			args["StartOnCall"] = foo;
			args["WaitForExitOnCall"] = bar;
			args["FaultOnEarlyExit"] = new Variant("true");

			var w = new WindowsDebuggerHybrid(null, "name", args);
			w.SessionStarting();
			w.IterationStarting(1, false);

			w.Message("Action.Call", foo);
			w.Message("Action.Call", bar);

			w.IterationFinished();

			Assert.AreEqual(false, w.DetectedFault());

			w.SessionFinished();
			w.StopMonitor();
		}

		[Test]
		public void TestExitEarlyFault2()
		{
			Variant foo = new Variant("foo");

			// FaultOnEarlyExit faults when StartOnCall is used and stop message is not sent

			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["CommandLine"] = new Variant("CrashingFileConsumer.exe");
			args["StartOnCall"] = foo;
			args["FaultOnEarlyExit"] = new Variant("true");

			var w = new WindowsDebuggerHybrid(null, "name", args);
			w.SessionStarting();
			w.IterationStarting(1, false);

			w.Message("Action.Call", foo);

			System.Threading.Thread.Sleep(1000);

			w.IterationFinished();

			Assert.AreEqual(true, w.DetectedFault());
			Fault f = w.GetMonitorData();
			Assert.NotNull(f);
			Assert.AreEqual("ProcessExitedEarly", f.folderName);


			w.SessionFinished();
			w.StopMonitor();
		}

		[Test]
		public void TestExitEarlyFault3()
		{
			Variant foo = new Variant("foo");

			// FaultOnEarlyExit doesn't fault when StartOnCall is used

			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["CommandLine"] = new Variant("CrashableServer.exe 127.0.0.1 6789");
			args["StartOnCall"] = foo;
			args["FaultOnEarlyExit"] = new Variant("true");

			var w = new WindowsDebuggerHybrid(null, "name", args);
			w.SessionStarting();
			w.IterationStarting(1, false);

			w.Message("Action.Call", foo);

			w.IterationFinished();

			Assert.AreEqual(false, w.DetectedFault());

			w.SessionFinished();
			w.StopMonitor();
		}

		[Test]
		public void TestExitEarlyFault4()
		{
			// FaultOnEarlyExit doesn't fault when restart every iteration is true

			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["CommandLine"] = new Variant("CrashableServer.exe 127.0.0.1 6789");
			args["RestartOnEachTest"] = new Variant("true");
			args["FaultOnEarlyExit"] = new Variant("true");

			var w = new WindowsDebuggerHybrid(null, "name", args);
			w.SessionStarting();
			w.IterationStarting(1, false);

			w.IterationFinished();

			Assert.AreEqual(false, w.DetectedFault());

			w.SessionFinished();
			w.StopMonitor();
		}
	}
}
