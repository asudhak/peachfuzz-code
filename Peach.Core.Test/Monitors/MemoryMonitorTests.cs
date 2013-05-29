using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Analyzers;
using Peach.Core.IO;
using Peach.Core.Agent;
using System.Collections;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;

namespace Peach.Core.Test.Monitors
{
	[TestFixture]
	class MemoryMonitorTests
	{
		class Params : Dictionary<string, string> { }

		int thisPid;
		private uint faultIteration;
		private Fault[] faults;

		[SetUp]
		public void SetUp()
		{
			using (var p = Process.GetCurrentProcess())
			{
				thisPid = p.Id;
			}

			faultIteration = 0;
			faults = null;
		}

		void _Fault(RunContext context, uint currentIteration, Dom.StateModel stateModel, Fault[] faults)
		{
			Assert.Null(this.faults);
			this.faults = faults;
		}

		string MakeXml(Params parameters)
		{
			string fmt = "<Param name='{0}' value='{1}'/>";

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
		<Monitor class='FaultingMonitor'>
			<Param name='Iteration' value='{0}'/>
		</Monitor>
		<Monitor class='Memory'>
{1}
		</Monitor>
	</Agent>

	<Test name='Default' replayEnabled='false'>
		<Agent ref='LocalAgent'/>
		<StateModel ref='TheState'/>
		<Publisher class='Null'/>
		<Strategy class='RandomDeterministic'/>
	</Test>
</Peach>";

			var items = parameters.Select(kv => string.Format(fmt, kv.Key, kv.Value));
			var joined = string.Join(Environment.NewLine, items);
			var ret = string.Format(template, faultIteration, joined);

			return ret;
		}

		private void Run(Params parameters, bool shouldFault)
		{
			string xml = MakeXml(parameters);

			PitParser parser = new PitParser();

			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			dom.tests[0].includedMutators = new List<string>();
			dom.tests[0].includedMutators.Add("StringCaseMutator");

			RunConfiguration config = new RunConfiguration();

			Engine e = new Engine(null);
			e.Fault += _Fault;

			if (!shouldFault)
			{
				e.startFuzzing(dom, config);
				return;
			}

			try
			{
				e.startFuzzing(dom, config);
				Assert.Fail("Should throw.");
			}
			catch (PeachException ex)
			{
				Assert.AreEqual("Fault detected on control iteration.", ex.Message);
			}
		}

		[Test]
		public void TestBadPid()
		{
			Run(new Params { { "Pid", "2147483647" } }, true);

			// verify values
			Assert.NotNull(faults);
			Assert.AreEqual(1, faults.Length);
			Assert.AreEqual("Unable to locate process with Pid 2147483647.", faults[0].title);
		}

		[Test]
		public void TestBadProcName()
		{
			Run(new Params { { "ProcessName", "some_invalid_process" } }, true);

			// verify values
			Assert.NotNull(faults);
			Assert.AreEqual(1, faults.Length);
			Assert.AreEqual("Unable to locate process \"some_invalid_process\".", faults[0].title);
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Could not start monitor \"Memory\".  Either pid or process name is required.")]
		public void TestNoParams()
		{
			Run(new Params(), false);
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Could not start monitor \"Memory\".  Only specify pid or process name, not both.")]
		public void TestAllParams()
		{
			Run(new Params { { "Pid", "1" }, { "ProcessName", "name" } }, false);
		}

		[Test]
		public void TestNoFault()
		{
			// If no fault occurs, no data should be returned
			Run(new Params { { "Pid", thisPid.ToString() } }, false);

			// verify values
			Assert.Null(faults);
		}

		[Test]
		public void TestFaultData()
		{
			// If fault occurs, monitor should always return data
			faultIteration = 1;

			Run(new Params { { "Pid", thisPid.ToString() } }, true);

			// verify values
			Assert.NotNull(faults);
			Assert.AreEqual(2, faults.Length);
			Assert.AreEqual("FaultingMonitor", faults[0].detectionSource);
			Assert.AreEqual(FaultType.Fault, faults[0].type);
			Assert.AreEqual("MemoryMonitor", faults[1].detectionSource);
			Assert.AreEqual(FaultType.Data, faults[1].type);
		}

		[Test]
		public void TestMemoryLimit()
		{
			// If memory limit is exceeded, monitor should generate a fault
			Run(new Params { { "Pid", thisPid.ToString() }, { "MemoryLimit", "1" } }, true);

			// verify values
			Assert.NotNull(faults);
			Assert.AreEqual(1, faults.Length);
			Assert.AreEqual("MemoryMonitor", faults[0].detectionSource);
			Assert.AreEqual(FaultType.Fault, faults[0].type);
		}

		[Test]
		public void TestProcessName()
		{
			string proc = Platform.GetOS() == Platform.OS.Windows ? "explorer.exe" : "sshd";

			Run(new Params { { "ProcessName", proc }, { "MemoryLimit", "1" } }, true);

			// verify values
			Assert.NotNull(faults);
			Assert.AreEqual(1, faults.Length);
			Assert.AreEqual("MemoryMonitor", faults[0].detectionSource);
			Assert.AreEqual(FaultType.Fault, faults[0].type);

			Console.WriteLine("{0}\n{1}\n", faults[0].title, faults[0].description);
		}
	}
}
