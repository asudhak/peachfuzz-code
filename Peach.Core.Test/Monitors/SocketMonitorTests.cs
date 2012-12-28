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
	class SocketMonitorTests
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
		<Monitor class='SocketMonitor'>
{1}
		</Monitor>
	</Agent>

	<Test name='Default'>
		<Agent ref='LocalAgent'/>
		<StateModel ref='TheState'/>
		<Publisher class='Null'/>
	</Test>
</Peach>";

			var items = parameters.Select(kv => string.Format(fmt, kv.Key, kv.Value));
			var joined = string.Join(Environment.NewLine, items);
			var ret = string.Format(template, faultIteration, joined);

			return ret;
		}

		private void Run(Params parameters)
		{
			string xml = MakeXml(parameters);

			faults = null;

			PitParser parser = new PitParser();

			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			dom.tests[0].includedMutators = new List<string>();
			dom.tests[0].includedMutators.Add("StringCaseMutator");

			RunConfiguration config = new RunConfiguration();

			Engine e = new Engine(null);
			e.config = config;
			e.Fault += _Fault;
			e.startFuzzing(dom, config);
		}

		[Test]
		public void TestNoConnNoFault()
		{
			// No connections, no faults
			Run(new Params());
			Assert.Null(faults);
		}

		[Test]
		public void TestNoConnOtherFault()
		{
			// Different monitor faults, SocketMonitor returns FaultType.Data
			faultIteration = 1;

			Run(new Params());
			Assert.NotNull(faults);
			Assert.AreEqual(2, faults.Length);
			Assert.AreEqual("FaultingMonitor", faults[0].detectionSource);
			Assert.AreEqual(FaultType.Fault, faults[0].type);
			Assert.AreEqual("SocketMonitor", faults[1].detectionSource);
			Assert.AreEqual(FaultType.Data, faults[1].type);
			Assert.AreEqual("No connections recorded.", faults[1].description);
		}

		[Test]
		public void TestNoConnFault()
		{
			// No connection, FaultOnSuccess = true results in fault
			Run(new Params { { "FaultOnSuccess", "true" } });
			Assert.NotNull(faults);
			Assert.AreEqual(1, faults.Length);
			Assert.AreEqual("SocketMonitor", faults[0].detectionSource);
			Assert.AreEqual(FaultType.Fault, faults[0].type);
			Assert.AreEqual("No connections recorded.", faults[0].description);
		}

		[Test]
		public void TestNoInterface()
		{
			// What to do if 'Interface' property is not specified
			// If 'Host' is specified, use the interface that has the best route to 'Host'
			// Otherwise, use "0.0.0.0".

			faultIteration = 1;

			Run(new Params());
			Assert.NotNull(faults);
			Assert.AreEqual(2, faults.Length);
			Assert.AreEqual("FaultingMonitor", faults[0].detectionSource);
			Assert.AreEqual(FaultType.Fault, faults[0].type);
			Assert.AreEqual("SocketMonitor", faults[1].detectionSource);
			Assert.AreEqual(FaultType.Data, faults[1].type);
			Assert.AreEqual("Monitoring 0.0.0.0:8080", faults[1].title);

			Run(new Params { { "Host", "::1" } });
			Assert.NotNull(faults);
			Assert.AreEqual(2, faults.Length);
			Assert.AreEqual("FaultingMonitor", faults[0].detectionSource);
			Assert.AreEqual(FaultType.Fault, faults[0].type);
			Assert.AreEqual("SocketMonitor", faults[1].detectionSource);
			Assert.AreEqual(FaultType.Data, faults[1].type);
			Assert.AreEqual("Monitoring [::1]:8080", faults[1].title);

			Run(new Params { { "Host", "127.0.0.2" } });
			Assert.NotNull(faults);
			Assert.AreEqual(2, faults.Length);
			Assert.AreEqual("FaultingMonitor", faults[0].detectionSource);
			Assert.AreEqual(FaultType.Fault, faults[0].type);
			Assert.AreEqual("SocketMonitor", faults[1].detectionSource);
			Assert.AreEqual(FaultType.Data, faults[1].type);
			Assert.AreEqual("Monitoring 127.0.0.1:8080", faults[1].title);

			string addr = null;
			using (var u = new System.Net.Sockets.UdpClient("1.1.1.1", 1))
			{
				addr = ((System.Net.IPEndPoint)u.Client.LocalEndPoint).Address.ToString();
			}

			Run(new Params { { "Host", "1.1.1.1" } });
			Assert.NotNull(faults);
			Assert.AreEqual(2, faults.Length);
			Assert.AreEqual("FaultingMonitor", faults[0].detectionSource);
			Assert.AreEqual(FaultType.Fault, faults[0].type);
			Assert.AreEqual("SocketMonitor", faults[1].detectionSource);
			Assert.AreEqual(FaultType.Data, faults[1].type);
			Assert.AreEqual("Monitoring " + addr + ":8080", faults[1].title);
		}

		[Test, ExpectedException(ExpectedException=typeof(PeachException), ExpectedMessage="Interface '::' is not compatible with the address family for Host '1.1.1.1'.")]
		public void TestBadHostInterface()
		{
			// Deal with IPv4/IPv6 mismatched Host & Interface parameters
			Run(new Params { { "Host", "1.1.1.1" }, { "Interface", "::" } });
		}

		[Test]
		[Ignore]
		public void TestConnNoFault()
		{
			// receive connection, FaultOnSuccess = true results in no fault
		}

		[Test]
		[Ignore]
		public void TestMulticast()
		{
			// Support 'Host' of 234.5.6.7
		}

		[Test]
		[Ignore]
		public void TestTcpHost()
		{
			// Only accept TCP connections from specified host
		}

		[Test]
		[Ignore]
		public void TestUdpHost()
		{
			// Only accept UDP connections from specified host
		}

		[Test]
		[Ignore]
		public void TestUdp4()
		{
		}

		[Test]
		[Ignore]
		public void TestUdp6()
		{
		}

		[Test]
		[Ignore]
		public void TestTcp4()
		{
		}

		[Test]
		[Ignore]
		public void TestTcp6()
		{
		}
	}
}
