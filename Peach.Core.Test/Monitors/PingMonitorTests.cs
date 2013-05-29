using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core.Analyzers;

namespace Peach.Core.Test.Monitors
{
	[TestFixture]
	class PingMonitorTests
	{
		class Params : Dictionary<string, string> { }

		private uint faultIteration;
		private Fault[] faults;

		[SetUp]
		public void SetUp()
		{
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
		<Monitor class='Ping'>
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

			faults = null;

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
		public void TestSuccess()
		{
			Run(new Params { { "Host", "127.0.0.1" } }, false);
			Assert.Null(faults);
		}

		[Test]
		public void TestFailure()
		{
			Run(new Params { { "Host", "234.5.6.7" } }, true);
			Assert.NotNull(faults);
			Assert.AreEqual(1, faults.Length);
			Assert.AreEqual("PingMonitor", faults[0].detectionSource);
			Assert.AreEqual(FaultType.Fault, faults[0].type);
		}

		[Test]
		public void TestFaultSuccess()
		{
			Run(new Params { { "Host", "234.5.6.7" }, { "FaultOnSuccess", "true" } }, false);
			Assert.Null(faults);
		}

		[Test]
		public void TestFaultFailure()
		{
			Run(new Params { { "Host", "127.0.0.1" }, { "FaultOnSuccess", "true" } }, true);
			Assert.NotNull(faults);
			Assert.AreEqual(1, faults.Length);
			Assert.AreEqual("PingMonitor", faults[0].detectionSource);
			Assert.AreEqual(FaultType.Fault, faults[0].type);
			Assert.True(faults[0].description.Contains("RoundTrip time"));
		}

		[Test]
		public void TestSuccessData()
		{
			faultIteration = 1;
			Run(new Params { { "Host", "127.0.0.1" } }, true);
			Assert.NotNull(faults);
			Assert.AreEqual(2, faults.Length);
			Assert.AreEqual("FaultingMonitor", faults[0].detectionSource);
			Assert.AreEqual(FaultType.Fault, faults[0].type);
			Assert.AreEqual("PingMonitor", faults[1].detectionSource);
			Assert.AreEqual(FaultType.Data, faults[1].type);
			Assert.True(faults[1].description.Contains("RoundTrip time"));
		}

		[Test]
		public void TestFaultSuccessData()
		{
			faultIteration = 1;
			Run(new Params { { "Host", "234.5.6.7" }, { "FaultOnSuccess", "true" } }, true);
			Assert.NotNull(faults);
			Assert.AreEqual(2, faults.Length);
			Assert.AreEqual("FaultingMonitor", faults[0].detectionSource);
			Assert.AreEqual(FaultType.Fault, faults[0].type);
			Assert.AreEqual("PingMonitor", faults[1].detectionSource);
			Assert.AreEqual(FaultType.Data, faults[1].type);
			Assert.False(faults[1].description.Contains("RoundTrip time"));
		}

		[Test]
		public void TestBadHost()
		{
			Run(new Params { { "Host", "some.bad.host" } }, true);
			Assert.NotNull(faults);
			Assert.AreEqual(1, faults.Length);
			Assert.AreEqual("PingMonitor", faults[0].detectionSource);
			Assert.AreEqual(FaultType.Fault, faults[0].type);
		}

		[Test]
		public void TestTimeout()
		{
			int start, stop, diff;

			// Run once to ensure all static objects are initialized
			Run(new Params { { "Host", "234.5.6.7" } }, true);

			start = Environment.TickCount;
			Run(new Params { { "Host", "234.5.6.7" }, { "Timeout", "5000" } }, true);
			stop = Environment.TickCount;

			diff = stop - start;
			Assert.Greater(diff, 4500);
			Assert.Less(diff, 6000);

			Assert.NotNull(faults);
			Assert.AreEqual(1, faults.Length);
			Assert.AreEqual("PingMonitor", faults[0].detectionSource);
			Assert.AreEqual(FaultType.Fault, faults[0].type);

			start = Environment.TickCount;
			Run(new Params { { "Host", "234.5.6.7" }, { "Timeout", "1000" } }, true);
			stop = Environment.TickCount;

			diff = stop - start;
			Assert.Greater(diff, 500);
			Assert.Less(diff, 2000);

			Assert.NotNull(faults);
			Assert.AreEqual(1, faults.Length);
			Assert.AreEqual("PingMonitor", faults[0].detectionSource);
			Assert.AreEqual(FaultType.Fault, faults[0].type);
		}

		[Test]
		public void TestData()
		{
			string data = new string('a', 70);
			Run(new Params { { "Host", "127.0.0.1" }, { "FaultOnSuccess", "true" }, { "Data", data } }, true);
			Assert.NotNull(faults);
			Assert.AreEqual(1, faults.Length);
			Assert.AreEqual("PingMonitor", faults[0].detectionSource);
			Assert.AreEqual(FaultType.Fault, faults[0].type);
			Assert.True(faults[0].description.Contains("Buffer size: 70"));
		}
	}
}
