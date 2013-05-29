using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core.Analyzers;
using Proc = System.Diagnostics.Process;
using System.Diagnostics;

namespace Peach.Core.Test.Monitors
{
	[TestFixture]
	class ProcessKillerMonitorTests
	{
		SingleInstance si;

		[SetUp]
		public void SetUp()
		{
			si = SingleInstance.CreateInstance("Peach.Core.Test.Monitors.ProcessKillerMonitorTests");
			si.Lock();
		}

		[TearDown]
		public void TearDown()
		{
			si.Dispose();
			si = null;
		}

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
		<Monitor class='ProcessKiller'>
			<Param name='ProcessNames' value='{0}'/>
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

		[Test]
		public void TestBadProcss()
		{
			// Specify a process name that is not running
			Run("some_invalid_process");
		}

		[Test]
		public void TestProcss()
		{
			// Specify a process name that is not running
			Run(testProcess, IterationStarting);

			var procs = Proc.GetProcessesByName(testProcess);
			foreach (var p in procs)
				p.Close();

			Assert.True(madeProcess);
			Assert.AreEqual(0, procs.Length);
		}

		string testProcess = GetTestProcess();
		bool madeProcess = false;

		void IterationStarting(RunContext context, uint currentIteration, uint? totalIterations)
		{
			Proc p = new Proc();
			p.StartInfo = new ProcessStartInfo(testProcess);
			madeProcess = p.Start();
			System.Threading.Thread.Sleep(1000);
			p.Close();
		}

		static string GetTestProcess()
		{
			if (Platform.GetOS() == Platform.OS.Windows)
				return "notepad";
			else
				return "tail";
		}
	}
}