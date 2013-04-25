using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core.Analyzers;
using Proc = System.Diagnostics.Process;
using Peach.Core.Agent.Monitors;
using TheAgent = Peach.Core.Agent.Agent;

namespace Peach.Core.Test.Monitors
{
	[TestFixture]
	class ProcessMonitorTests
	{
		string MakeXml(string folder)
		{
			string template = @"
<Peach>
	<DataModel name='TheDataModel'>
		<String value='Hello' mutable='false'/>
	</DataModel>

	<StateModel name='TheState' initialState='Initial'>
		<State name='Initial'>
			<Action type='output'>
				<DataModel ref='TheDataModel'/>
			</Action>
		</State>
	</StateModel>

	<Agent name='LocalAgent'>
		<Monitor class='Process'>
			<Param name='Executable' value='{0}'/>
		</Monitor>
	</Agent>

	<Test name='Default' replayEnabled='false'>
		<Agent ref='LocalAgent'/>
		<StateModel ref='TheState'/>
		<Publisher class='Null'/>
		<Strategy class='RandomDeterministic'/>
	</Test>
</Peach>";

			var ret = string.Format(template, folder);
			return ret;
		}

		void Run(string proccessNames, Engine.IterationStartingEventHandler OnIterStart = null)
		{
			string xml = MakeXml(proccessNames);

			PitParser parser = new PitParser();

			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			dom.tests[0].includedMutators = new List<string>();
			dom.tests[0].includedMutators.Add("StringCaseMutator");

			RunConfiguration config = new RunConfiguration();

			Engine e = new Engine(null);
			if (OnIterStart != null)
				e.IterationStarting += OnIterStart;
			e.startFuzzing(dom, config);
		}

		[Test, ExpectedException(typeof(PeachException))]
		public void TestBadProcss()
		{
			// Specify a process name that is not running
			Run("some_invalid_process");
		}

		[Test]
		public void TestStartOnCall()
		{
			Variant foo = new Variant("foo");

			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["Executable"] = new Variant("CrashableServer");
			args["Arguments"] = new Variant("127.0.0.1 {0}".Fmt(TestBase.MakePort(60000, 61000)));
			args["StartOnCall"] = foo;
			args["WaitForExitTimeout"] = new Variant("2000");
			args["NoCpuKill"] = new Variant("true");

			Process p = new Process(new TheAgent("agent"), "name", args);

			p.Message("Action.Call", foo);
			System.Threading.Thread.Sleep(1000);

			var before = DateTime.Now;
			p.IterationFinished();
			var after = DateTime.Now;

			var span = (after - before);

			Assert.AreEqual(false, p.DetectedFault());

			p.SessionFinished();
			p.StopMonitor();

			Assert.GreaterOrEqual(span.TotalSeconds, 1.9);
			Assert.LessOrEqual(span.TotalSeconds, 2.1);
		}

		[Test]
		public void TestCpuKill()
		{
			Variant foo = new Variant("foo");

			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["Executable"] = new Variant("CrashableServer");
			args["Arguments"] = new Variant("127.0.0.1 {0}".Fmt(TestBase.MakePort(61000, 62000)));
			args["StartOnCall"] = foo;

			Process p = new Process(new TheAgent("agent"), "name", args);

			p.Message("Action.Call", foo);
			System.Threading.Thread.Sleep(1000);

			var before = DateTime.Now;
			p.IterationFinished();
			var after = DateTime.Now;

			var span = (after - before);

			Assert.AreEqual(false, p.DetectedFault());

			p.SessionFinished();
			p.StopMonitor();

			Assert.GreaterOrEqual(span.TotalSeconds, 0.0);
			Assert.LessOrEqual(span.TotalSeconds, 0.5);
		}

		[Test]
		public void TestExitOnCallNoFault()
		{
			Variant foo = new Variant("foo");
			Variant bar = new Variant("bar");

			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["Executable"] = new Variant("CrashingFileConsumer");
			args["StartOnCall"] = foo;
			args["WaitForExitOnCall"] = bar;
			args["NoCpuKill"] = new Variant("true");

			Process p = new Process(new TheAgent("agent"), "name", args);

			p.Message("Action.Call", foo);
			p.Message("Action.Call", bar);

			p.IterationFinished();

			Assert.AreEqual(false, p.DetectedFault());

			p.SessionFinished();
			p.StopMonitor();
		}

		[Test]
		public void TestExitOnCallFault()
		{
			Variant foo = new Variant("foo");
			Variant bar = new Variant("bar");

			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["Executable"] = new Variant("CrashableServer");
			args["Arguments"] = new Variant("127.0.0.1 {0}".Fmt(TestBase.MakePort(62000, 63000)));
			args["StartOnCall"] = foo;
			args["WaitForExitOnCall"] = bar;
			args["WaitForExitTimeout"] = new Variant("2000");
			args["NoCpuKill"] = new Variant("true");

			Process p = new Process(new TheAgent("agent"), "name", args);

			p.Message("Action.Call", foo);
			p.Message("Action.Call", bar);

			p.IterationFinished();

			Assert.AreEqual(true, p.DetectedFault());
			Fault f = p.GetMonitorData();
			Assert.NotNull(f);
			Assert.AreEqual("ProcessFailedToExit", f.folderName);

			p.SessionFinished();
			p.StopMonitor();
		}

		[Test]
		public void TestExitTime()
		{
			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["Executable"] = new Variant("CrashableServer");
			args["Arguments"] = new Variant("127.0.0.1 {0}".Fmt(TestBase.MakePort(63000, 64000)));
			args["RestartOnEachTest"] = new Variant("true");

			Process p = new Process(new TheAgent("agent"), "name", args);
			p.SessionStarting();
			p.IterationStarting(1, false);

			var before = DateTime.Now;
			p.IterationFinished();
			var after = DateTime.Now;

			var span = (after - before);

			Assert.AreEqual(false, p.DetectedFault());

			p.SessionFinished();
			p.StopMonitor();

			Assert.GreaterOrEqual(span.TotalSeconds, 0.0);
			Assert.LessOrEqual(span.TotalSeconds, 0.1);
		}

		[Test]
		public void TestExitEarlyFault()
		{
			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["Executable"] = new Variant("CrashingFileConsumer");
			args["FaultOnEarlyExit"] = new Variant("true");

			Process p = new Process(new TheAgent("agent"), "name", args);
			p.SessionStarting();
			p.IterationStarting(1, false);

			System.Threading.Thread.Sleep(1000);

			p.IterationFinished();

			Assert.AreEqual(true, p.DetectedFault());
			Fault f = p.GetMonitorData();
			Assert.NotNull(f);
			Assert.AreEqual("ProcessExitedEarly", f.folderName);

			p.SessionFinished();
			p.StopMonitor();
		}

		[Test]
		public void TestExitEarlyFault1()
		{
			Variant foo = new Variant("foo");
			Variant bar = new Variant("bar");

			// FaultOnEarlyExit doesn't fault when stop message is sent

			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["Executable"] = new Variant("CrashingFileConsumer");
			args["StartOnCall"] = foo;
			args["WaitForExitOnCall"] = bar;
			args["FaultOnEarlyExit"] = new Variant("true");

			Process p = new Process(new TheAgent("agent"), "name", args);
			p.SessionStarting();
			p.IterationStarting(1, false);

			p.Message("Action.Call", foo);
			p.Message("Action.Call", bar);

			p.IterationFinished();

			Assert.AreEqual(false, p.DetectedFault());

			p.SessionFinished();
			p.StopMonitor();
		}

		[Test]
		public void TestExitEarlyFault2()
		{
			Variant foo = new Variant("foo");

			// FaultOnEarlyExit faults when StartOnCall is used and stop message is not sent

			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["Executable"] = new Variant("CrashingFileConsumer");
			args["StartOnCall"] = foo;
			args["FaultOnEarlyExit"] = new Variant("true");

			Process p = new Process(new TheAgent("agent"), "name", args);
			p.SessionStarting();
			p.IterationStarting(1, false);

			p.Message("Action.Call", foo);

			System.Threading.Thread.Sleep(1000);

			p.IterationFinished();

			Assert.AreEqual(true, p.DetectedFault());
			Fault f = p.GetMonitorData();
			Assert.NotNull(f);
			Assert.AreEqual("ProcessExitedEarly", f.folderName);


			p.SessionFinished();
			p.StopMonitor();
		}

		[Test]
		public void TestExitEarlyFault3()
		{
			Variant foo = new Variant("foo");

			// FaultOnEarlyExit doesn't fault when StartOnCall is used

			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["Executable"] = new Variant("CrashableServer");
			args["Arguments"] = new Variant("127.0.0.1 {0}".Fmt(TestBase.MakePort(63000, 64000)));
			args["StartOnCall"] = foo;
			args["FaultOnEarlyExit"] = new Variant("true");

			Process p = new Process(new TheAgent("agent"), "name", args);
			p.SessionStarting();
			p.IterationStarting(1, false);

			p.Message("Action.Call", foo);

			p.IterationFinished();

			Assert.AreEqual(false, p.DetectedFault());
			
			p.SessionFinished();
			p.StopMonitor();
		}

		[Test]
		public void TestExitEarlyFault4()
		{
			// FaultOnEarlyExit doesn't fault when restart every iteration is true

			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["Executable"] = new Variant("CrashableServer");
			args["Arguments"] = new Variant("127.0.0.1 {0}".Fmt(TestBase.MakePort(63000, 64000)));
			args["RestartOnEachTest"] = new Variant("true");
			args["FaultOnEarlyExit"] = new Variant("true");

			Process p = new Process(new TheAgent("agent"), "name", args);
			p.SessionStarting();
			p.IterationStarting(1, false);

			p.IterationFinished();

			Assert.AreEqual(false, p.DetectedFault());

			p.SessionFinished();
			p.StopMonitor();
		}
	}
}