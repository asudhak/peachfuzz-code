using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

using Peach.Core;
using Peach.Core.Agent.Channels;

using NUnit;
using NUnit.Framework;
using System.Threading;
using Peach.Core.Analyzers;
using System.IO;
using Peach.Core.Agent;

namespace Peach.Core.Test.Agent
{
	[TestFixture]
	public class AgentTests
	{
		SingleInstance si;

		[SetUp]
		public void SetUp()
		{
			si = SingleInstance.CreateInstance("Peach.Core.Test.Agent.AgentTests");
			si.Lock();
		}

		[TearDown]
		public void TearDown()
		{
			si.Dispose();
			si = null;
		}

		public System.Diagnostics.Process process;

		[Publisher("AgentKiller", true)]
		public class AgentKillerPublisher : Peach.Core.Publisher
		{
			public AgentTests owner;

			static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

			public AgentKillerPublisher(Dictionary<string, Variant> args)
				: base(args)
			{
			}

			protected override NLog.Logger Logger
			{
				get { return logger; }
			}

			protected RunContext Context
			{
				get
				{
					Dom.Dom dom = this.Test.parent as Dom.Dom;
					return dom.context;
				}
			}

			protected override void OnOpen()
			{
				base.OnOpen();

				if (!this.IsControlIteration && (this.Iteration % 2) == 1)
				{
					// Lame hack to make sure CrashableServer gets stopped
					Context.agentManager.IterationFinished();

					owner.StopAgent();
					owner.StartAgent();
				}
			}
		}

		ManualResetEvent startEvent;

		public void StartAgent()
		{
			process = new System.Diagnostics.Process();

			if (Platform.GetOS() == Platform.OS.Windows)
			{
				process.StartInfo.FileName = "Peach.exe";
				process.StartInfo.Arguments = "-a tcp";
			}
			else
			{
				List<string> paths = new List<string>();
				paths.Add(Environment.CurrentDirectory);
				paths.AddRange(process.StartInfo.EnvironmentVariables["PATH"].Split(Path.PathSeparator));
				string peach = "peach.exe";
				foreach (var dir in paths)
				{
					var candidate = Path.Combine(dir, peach);
					if (File.Exists(candidate))
					{
						peach = candidate;
						break;
					}
				}

				process.StartInfo.FileName = "mono";
				process.StartInfo.Arguments = "--debug {0} -a tcp".Fmt(peach);
			}

			process.StartInfo.RedirectStandardInput = true;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.UseShellExecute = false;
			process.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(process_OutputDataReceived);

			try
			{
				using (startEvent = new ManualResetEvent(false))
				{
					process.Start();
					if (Platform.GetOS() == Platform.OS.Windows)
						process.BeginOutputReadLine();

					startEvent.WaitOne(5000);
				}
			}
			catch
			{
				process = null;
				throw;
			}
			finally
			{
				startEvent = null;
			}
		}

		void process_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
		{
			if (e.Data == null)
				return;

			if (e.Data.Contains("Press ENTER to quit agent"))
				startEvent.Set();
		}

		public void StopAgent()
		{
			if (!process.HasExited)
			{
				process.Kill();
				process.WaitForExit();
			}

			process.Close();
			process = null;
		}

		[Test]
		public void TestReconnect()
		{
			ushort port = TestBase.MakePort(20000, 21000);

			string agent = @"
	<Agent name='RemoteAgent' location='tcp://127.0.0.1:9001'>
		<Monitor class='WindowsDebugger'>
			<Param name='CommandLine' value='CrashableServer.exe 127.0.0.1 {0}'/>
			<Param name='RestartOnEachTest' value='true'/>
			<Param name='FaultOnEarlyExit' value='true'/>
		</Monitor>
	</Agent>
";
			if (Platform.GetOS() != Platform.OS.Windows)
			{
				agent = @"
	<Agent name='RemoteAgent' location='tcp://127.0.0.1:9001'>
		<Monitor class='Process'>
			<Param name='Executable' value='CrashableServer'/>
			<Param name='Arguments' value='127.0.0.1 {0}'/>
			<Param name='RestartOnEachTest' value='true'/>
			<Param name='FaultOnEarlyExit' value='true'/>
		</Monitor>
	</Agent>
";
			}
			else
			{
				if (!Environment.Is64BitProcess && Environment.Is64BitOperatingSystem)
					Assert.Ignore("Cannot run the 32bit version of this test on a 64bit operating system.");

				if (Environment.Is64BitProcess && !Environment.Is64BitOperatingSystem)
					Assert.Ignore("Cannot run the 64bit version of this test on a 32bit operating system.");
			}

			agent = agent.Fmt(port);

			string xml = @"
<Peach>
	<DataModel name='TheDataModel'>
		<String value='Hello'/>
	</DataModel>

	<StateModel name='TheState' initialState='Initial'>
		<State name='Initial'>
			<Action type='output' publisher='Remote'>
				<DataModel ref='TheDataModel'/>
			</Action>

			<Action type='open' publisher='Killer'/>
		</State>
	</StateModel>

{1}

	<Test name='Default' replayEnabled='false'>
		<Agent ref='RemoteAgent'/>
		<StateModel ref='TheState'/>
		<Publisher name='Remote' class='Remote'>
			<Param name='Agent' value='RemoteAgent' />
			<Param name='Class' value='Tcp'/>
			<Param name='Host' value='127.0.0.1' />
			<Param name='Port' value='{0}' />
		</Publisher>
		<Publisher name='Killer' class='AgentKiller'/>
		<Strategy class='RandomDeterministic'/>
	</Test>
</Peach>".Fmt(port, agent);

			try
			{
				StartAgent();

				PitParser parser = new PitParser();
				Dom.Dom dom = parser.asParser(null, new MemoryStream(Encoding.ASCII.GetBytes(xml)));

				var pub = dom.tests[0].publishers[1] as AgentKillerPublisher;
				pub.owner = this;

				RunConfiguration config = new RunConfiguration();
				config.range = true;
				config.rangeStart = 1;
				config.rangeStop = 5;

				Engine e = new Engine(null);
				e.Fault += new Engine.FaultEventHandler(e_Fault);
				e.startFuzzing(dom, config);

				Assert.Greater(faults.Count, 0);
			}
			finally
			{
				if (process != null)
					StopAgent();
			}
		}

		Dictionary<uint, Fault[]> faults = new Dictionary<uint, Fault[]>();

		void e_Fault(RunContext context, uint currentIteration, Dom.StateModel stateModel, Fault[] faultData)
		{
			faults[currentIteration] = faultData;
		}

		[Test]
		public void TestSoftException()
		{
			ushort port = TestBase.MakePort(20000, 21000);

			string agent = @"
	<Agent name='RemoteAgent' location='tcp://127.0.0.1:9001'>
		<Monitor class='WindowsDebugger'>
			<Param name='CommandLine' value='CrashableServer.exe 127.0.0.1 {0}'/>
			<Param name='FaultOnEarlyExit' value='true'/>
		</Monitor>
	</Agent>
";
			if (Platform.GetOS() != Platform.OS.Windows)
			{
				agent = @"
	<Agent name='RemoteAgent' location='tcp://127.0.0.1:9001'>
		<Monitor class='Process'>
			<Param name='Executable' value='CrashableServer'/>
			<Param name='Arguments' value='127.0.0.1 {0}'/>
			<Param name='FaultOnEarlyExit' value='true'/>
		</Monitor>
	</Agent>
";
			}
			else
			{
				if (!Environment.Is64BitProcess && Environment.Is64BitOperatingSystem)
					Assert.Ignore("Cannot run the 32bit version of this test on a 64bit operating system.");

				if (Environment.Is64BitProcess && !Environment.Is64BitOperatingSystem)
					Assert.Ignore("Cannot run the 64bit version of this test on a 32bit operating system.");
			}

			agent = agent.Fmt(port);

			string xml = @"
<Peach>
	<DataModel name='TheDataModel'>
		<String value='Hello'/>
	</DataModel>

	<StateModel name='TheState' initialState='Initial'>
		<State name='Initial'>
			<Action type='output' publisher='Remote'>
				<DataModel ref='TheDataModel'/>
			</Action>
			<Action type='output' publisher='Remote'>
				<DataModel ref='TheDataModel'/>
			</Action>
		</State>
	</StateModel>

{1}

	<Test name='Default' replayEnabled='false'>
		<Agent ref='RemoteAgent'/>
		<StateModel ref='TheState'/>
		<Publisher name='Remote' class='Remote'>
			<Param name='Agent' value='RemoteAgent' />
			<Param name='Class' value='Tcp'/>
			<Param name='Host' value='127.0.0.1' />
			<Param name='Port' value='{0}' />
		</Publisher>
		<Strategy class='RandomDeterministic'/>
	</Test>
</Peach>".Fmt(port, agent);

			try
			{
				StartAgent();

				PitParser parser = new PitParser();
				Dom.Dom dom = parser.asParser(null, new MemoryStream(Encoding.ASCII.GetBytes(xml)));

				RunConfiguration config = new RunConfiguration();
				config.range = true;
				config.rangeStart = 1;
				config.rangeStop = 5;

				Engine e = new Engine(null);
				e.Fault += new Engine.FaultEventHandler(e_Fault);
				e.startFuzzing(dom, config);

				Assert.Greater(faults.Count, 0);
			}
			finally
			{
				if (process != null)
					StopAgent();
			}
		}

		[Test]
		public void TestBadProcess()
		{
			string error = "System debugger could not start process 'MissingProgram'.";
			string agent = @"
	<Agent name='RemoteAgent' location='tcp://127.0.0.1:9001'>
		<Monitor class='WindowsDebugger'>
			<Param name='CommandLine' value='MissingProgram'/>
		</Monitor>
	</Agent>
";
			if (Platform.GetOS() != Platform.OS.Windows)
			{
				error = "Could not start process 'MissingProgram'.";
				agent = @"
	<Agent name='RemoteAgent' location='tcp://127.0.0.1:9001'>
		<Monitor class='Process'>
			<Param name='Executable' value='MissingProgram'/>
		</Monitor>
	</Agent>
";
			}
			else
			{
				if (!Environment.Is64BitProcess && Environment.Is64BitOperatingSystem)
					Assert.Ignore("Cannot run the 32bit version of this test on a 64bit operating system.");

				if (Environment.Is64BitProcess && !Environment.Is64BitOperatingSystem)
					Assert.Ignore("Cannot run the 64bit version of this test on a 32bit operating system.");
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

{0}

	<Test name='Default' replayEnabled='false'>
		<Agent ref='RemoteAgent'/>
		<StateModel ref='TheState'/>
		<Publisher class='Null'/>
		<Strategy class='RandomDeterministic'/>
	</Test>
</Peach>".Fmt(agent);

			try
			{
				StartAgent();

				PitParser parser = new PitParser();
				Dom.Dom dom = parser.asParser(null, new MemoryStream(Encoding.ASCII.GetBytes(xml)));

				RunConfiguration config = new RunConfiguration();

				Engine e = new Engine(null);

				try
				{
					e.startFuzzing(dom, config);
					Assert.Fail("Should throw!");
				}
				catch (PeachException pe)
				{
					Assert.True(pe.Message.StartsWith(error));
				}
			}
			finally
			{
				if (process != null)
					StopAgent();
			}
		}

		[Monitor("LoggingMonitor", true, IsTest = true)]
		public class LoggingMonitor : Peach.Core.Agent.Monitor
		{
			public LoggingMonitor(IAgent agent, string name, SerializableDictionary<string, Variant> args)
				: base(agent, name, args)
			{
				history.Add(Name + ".LoggingMonitor");
			}

			public override void StopMonitor()
			{
				history.Add(Name + ".StopMonitor");
			}

			public override void SessionStarting()
			{
				history.Add(Name + ".SessionStarting");
			}

			public override void SessionFinished()
			{
				history.Add(Name + ".SessionFinished");
			}

			public override void IterationStarting(uint iterationCount, bool isReproduction)
			{
				history.Add(Name + ".IterationStarting");
			}

			public override bool IterationFinished()
			{
				history.Add(Name + ".IterationFinished");
				return false;
			}

			public override bool DetectedFault()
			{
				history.Add(Name + ".DetectedFault");
				return false;
			}

			public override Fault GetMonitorData()
			{
				history.Add(Name + ".GetMonitorData");
				return null;
			}

			public override bool MustStop()
			{
				history.Add(Name + ".MustStop");
				return false;
			}

			public override Variant Message(string name, Variant data)
			{
				history.Add(Name + ".Message." + name + "." + (string)data);
				return null;
			}
		}

		static List<string> history = new List<string>();

		[Test]
		public void TestAgentOrder()
		{
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
			<Action type='call' method='Foo' publisher='Peach.Agent'/>
		</State>
	</StateModel>

	<Agent name='Local1'>
		<Monitor name='Local1.mon1' class='LoggingMonitor'/>
		<Monitor name='Local1.mon2' class='LoggingMonitor'/>
	</Agent>

	<Agent name='Local2'>
		<Monitor name='Local2.mon1' class='LoggingMonitor'/>
		<Monitor name='Local2.mon2' class='LoggingMonitor'/>
	</Agent>

	<Test name='Default' replayEnabled='false'>
		<Agent ref='Local1'/>
		<Agent ref='Local2'/>
		<StateModel ref='TheState'/>
		<Publisher class='Null'/>
		<Strategy class='Random'/>
	</Test>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(Encoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			string[] expected =
			{
				"Local1.mon1.LoggingMonitor",
				"Local1.mon2.LoggingMonitor",
				"Local1.mon1.SessionStarting",
				"Local1.mon2.SessionStarting",
				"Local2.mon1.LoggingMonitor",
				"Local2.mon2.LoggingMonitor",
				"Local2.mon1.SessionStarting",
				"Local2.mon2.SessionStarting",
				"Local1.mon1.IterationStarting",
				"Local1.mon2.IterationStarting",
				"Local2.mon1.IterationStarting",
				"Local2.mon2.IterationStarting",
				"Local1.mon1.Message.Action.Call.Foo",
				"Local1.mon2.Message.Action.Call.Foo",
				"Local2.mon1.Message.Action.Call.Foo",
				"Local2.mon2.Message.Action.Call.Foo",
				"Local2.mon2.IterationFinished",
				"Local2.mon1.IterationFinished",
				"Local1.mon2.IterationFinished",
				"Local1.mon1.IterationFinished",
				"Local1.mon1.DetectedFault",
				"Local1.mon2.DetectedFault",
				"Local2.mon1.DetectedFault",
				"Local2.mon2.DetectedFault",
				"Local1.mon1.MustStop",
				"Local1.mon2.MustStop",
				"Local2.mon1.MustStop",
				"Local2.mon2.MustStop",
				"Local2.mon2.SessionFinished",
				"Local2.mon1.SessionFinished",
				"Local1.mon2.SessionFinished",
				"Local1.mon1.SessionFinished",
				"Local2.mon2.StopMonitor",
				"Local2.mon1.StopMonitor",
				"Local1.mon2.StopMonitor",
				"Local1.mon1.StopMonitor",
			};

			Assert.AreEqual(expected, history.ToArray());

		}
	}
}
