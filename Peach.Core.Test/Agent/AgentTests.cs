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

					System.Threading.Thread.Sleep(1000);
				}
			}
		}

		public void StartAgent()
		{
			process = new System.Diagnostics.Process();

			if (Platform.GetOS() == Platform.OS.Windows)
			{
				process.StartInfo.FileName = "Peach.exe";
				process.StartInfo.Arguments = "-a tcp --debug";
			}
			else
			{
				process.StartInfo.FileName = "mono";
				process.StartInfo.Arguments = "--debug peach.exe -a tcp --debug";
			}

			foreach (var x in process.StartInfo.EnvironmentVariables.Keys)
				Console.WriteLine("{0} = {1}", x, process.StartInfo.EnvironmentVariables[x.ToString()]);

			process.Start();
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
				StopAgent();
			}
		}

		Dictionary<uint, Fault[]> faults = new Dictionary<uint, Fault[]>();

		void e_Fault(RunContext context, uint currentIteration, Dom.StateModel stateModel, Fault[] faultData)
		{
			faults[currentIteration] = faultData;
		}

	}
}
