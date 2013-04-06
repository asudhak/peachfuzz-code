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

			protected override void OnOpen()
			{
				base.OnOpen();

				if (!this.IsControlIteration && (this.Iteration % 2) == 1)
				{
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

			agent = agent.Fmt(port);

			string xml = @"
<Peach>
	<Import import='code'/>

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
	}
}
