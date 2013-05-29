using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core.Analyzers;
using System.Net;
using System.Net.Sockets;

namespace Peach.Core.Test.Monitors
{
	[TestFixture]
	class SocketMonitorTests
	{
		class TcpSender : IDisposable
		{
			public byte[] buffer;
			public IPEndPoint remoteEP;
			public IPEndPoint localEP;
			public Socket socket;

			public TcpSender(string ip, ushort port, string payload)
			{
				remoteEP = new IPEndPoint(IPAddress.Parse(ip), port);
				localEP = new IPEndPoint(remoteEP.Address, 0);
				buffer = Encoding.ASCII.GetBytes(payload);

				Connect();
			}

			private void Connect()
			{
				socket = new Socket(remoteEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
				socket.Bind(localEP);
				if (localEP.Port == 0)
					localEP.Port = ((IPEndPoint)socket.LocalEndPoint).Port;
				socket.BeginConnect(remoteEP.Address, remoteEP.Port, OnConnect, null);
			}

			void OnConnect(IAsyncResult ar)
			{
				try
				{
					socket.EndConnect(ar);
				}
				catch (ObjectDisposedException)
				{
					return;
				}
				catch (Exception ex)
				{
					SocketException se = ex as SocketException;
					if (se == null || se.SocketErrorCode != SocketError.ConnectionRefused)
						Assert.Null(ex.Message);
				}

				if (!socket.Connected)
				{
					socket.Close();
					System.Threading.Thread.Sleep(500);
					Connect();
					return;
				}

				socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, OnSend, null);
			}

			void OnSend(IAsyncResult ar)
			{
				try
				{
					socket.EndSend(ar);
					socket.Shutdown(SocketShutdown.Both);
					socket.Close();
				}
				catch (SocketException se)
				{
					if (se.SocketErrorCode != SocketError.ConnectionReset)
						Assert.Null(se.Message);
				}
				catch (ObjectDisposedException)
				{
					return;
				}
				catch (Exception ex)
				{
					Assert.Null(ex.Message);
				}
			}

			public void Dispose()
			{
				socket.Dispose();
			}
		}

		class UdpSender : UdpClient
		{
			byte[] buffer;
			IPEndPoint remoteEP;

			public UdpSender(IPAddress localIp, string ip, ushort port, string payload)
				: base(localIp.AddressFamily)
			{
				remoteEP = new IPEndPoint(IPAddress.Parse(ip), port);
				buffer = Encoding.ASCII.GetBytes(payload);

				if (remoteEP.Address.IsMulticast())
				{
					if (remoteEP.Address.AddressFamily == AddressFamily.InterNetwork)
					{
						Client.Bind(new IPEndPoint(localIp, 0));
						Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, localIp.GetAddressBytes());
					}
					else
					{
						throw new NotSupportedException();
					}
				}
				else
				{
					Client.Bind(new IPEndPoint(localIp, 0));
				}

				BeginSend(buffer, buffer.Length, remoteEP, OnSend, null);
			}

			public UdpSender(string ip, ushort port, string payload)
				: this(IPAddress.Parse(ip), ip, port, payload)
			{
			}

			void OnSend(IAsyncResult ar)
			{
				try
				{
					EndSend(ar);
				}
				catch (ObjectDisposedException)
				{
					return;
				}
				catch (Exception ex)
				{
					SocketException se = ex as SocketException;
					if (se == null || se.SocketErrorCode != SocketError.ConnectionRefused)
						Assert.Null(ex.Message);
				}

				System.Threading.Thread.Sleep(500);

				try
				{
					BeginSend(buffer, buffer.Length, remoteEP, OnSend, null);
				}
				catch (ObjectDisposedException)
				{
					return;
				}
				catch (Exception ex)
				{
					Assert.Null(ex.Message);
				}
			}
		}

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
		<Monitor class='Socket'>
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
		public void TestNoConnNoFault()
		{
			// No connections, no faults
			ushort port = TestBase.MakePort(40000, 41000);

			Run(new Params { { "Timeout", "1" }, { "Port", port.ToString() } }, false);
			Assert.Null(faults);
		}

		[Test]
		public void TestNoConnOtherFault()
		{
			// Different monitor faults, SocketMonitor returns FaultType.Data
			faultIteration = 1;

			ushort port = TestBase.MakePort(41000, 42000);

			Run(new Params { { "Timeout", "1" }, { "Port", port.ToString() } }, true);
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
			ushort port = TestBase.MakePort(42000, 43000);

			// No connection, FaultOnSuccess = true results in fault
			Run(new Params { { "Timeout", "1" }, { "FaultOnSuccess", "true" }, { "Port", port.ToString() } }, true);
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

			ushort port = TestBase.MakePort(43000, 44000);

			Run(new Params { { "Timeout", "1" }, { "Port", port.ToString() } }, true);
			Assert.NotNull(faults);
			Assert.AreEqual(2, faults.Length);
			Assert.AreEqual("FaultingMonitor", faults[0].detectionSource);
			Assert.AreEqual(FaultType.Fault, faults[0].type);
			Assert.AreEqual("SocketMonitor", faults[1].detectionSource);
			Assert.AreEqual(FaultType.Data, faults[1].type);
			Assert.AreEqual("Monitoring 0.0.0.0:" + port, faults[1].title);

			Run(new Params { { "Timeout", "1" }, { "Host", "::1" }, { "Port", port.ToString() } }, true);
			Assert.NotNull(faults);
			Assert.AreEqual(2, faults.Length);
			Assert.AreEqual("FaultingMonitor", faults[0].detectionSource);
			Assert.AreEqual(FaultType.Fault, faults[0].type);
			Assert.AreEqual("SocketMonitor", faults[1].detectionSource);
			Assert.AreEqual(FaultType.Data, faults[1].type);
			if (Platform.GetOS() == Platform.OS.Windows)
				Assert.AreEqual("Monitoring [::1]:" + port, faults[1].title);
			else
				Assert.AreEqual("Monitoring ::0.0.0.1:" + port, faults[1].title);

			Run(new Params { { "Timeout", "1" }, { "Host", "127.0.0.2" }, { "Port", port.ToString() } }, true);
			Assert.NotNull(faults);
			Assert.AreEqual(2, faults.Length);
			Assert.AreEqual("FaultingMonitor", faults[0].detectionSource);
			Assert.AreEqual(FaultType.Fault, faults[0].type);
			Assert.AreEqual("SocketMonitor", faults[1].detectionSource);
			Assert.AreEqual(FaultType.Data, faults[1].type);
			Assert.AreEqual("Monitoring 127.0.0.1:" + port, faults[1].title);

			string addr = null;
			using (var u = new System.Net.Sockets.UdpClient("1.1.1.1", 1))
			{
				addr = ((System.Net.IPEndPoint)u.Client.LocalEndPoint).Address.ToString();
			}

			Run(new Params { { "Timeout", "1" }, { "Host", "1.1.1.1" }, { "Port", port.ToString() } }, true);
			Assert.NotNull(faults);
			Assert.AreEqual(2, faults.Length);
			Assert.AreEqual("FaultingMonitor", faults[0].detectionSource);
			Assert.AreEqual(FaultType.Fault, faults[0].type);
			Assert.AreEqual("SocketMonitor", faults[1].detectionSource);
			Assert.AreEqual(FaultType.Data, faults[1].type);
			Assert.AreEqual("Monitoring " + addr + ":" + port, faults[1].title);
		}

		[Test, ExpectedException(ExpectedException=typeof(PeachException), ExpectedMessage="Could not start monitor \"Socket\".  Interface '::' is not compatible with the address family for Host '1.1.1.1'.")]
		public void TestBadHostInterface()
		{
			// Deal with IPv4/IPv6 mismatched Host & Interface parameters
			Run(new Params { { "Host", "1.1.1.1" }, { "Interface", "::" } }, false);
		}

		[Test]
		public void TestConnNoFault()
		{
			// receive connection, FaultOnSuccess = true results in no fault
			ushort port = TestBase.MakePort(44000, 45000);

			using (var sender = new UdpSender("127.0.0.1", port, "Hello"))
			{
				Run(new Params { { "FaultOnSuccess", "true" }, { "Protocol", "udp" }, { "Interface", "127.0.0.1" }, { "Port", port.ToString() } }, false);
			}

			Assert.Null(faults);
		}

		[Test]
		public void TestConnNoFaultOtherFault()
		{
			// receive connection, FaultOnSuccess = true results in fault data when other monitor faults
			faultIteration = 1;

			ushort port = TestBase.MakePort(45000, 46000);
			string desc;

			using (var sender = new UdpSender("127.0.0.1", port, "Hello"))
			{
				desc = string.Format("Received 5 bytes from '{0}'.", sender.Client.LocalEndPoint);
				Run(new Params { { "FaultOnSuccess", "true" }, { "Protocol", "udp" }, { "Interface", "127.0.0.1" }, { "Port", port.ToString() } }, true);
			}

			Assert.NotNull(faults);
			Assert.AreEqual(2, faults.Length);
			Assert.AreEqual("FaultingMonitor", faults[0].detectionSource);
			Assert.AreEqual(FaultType.Fault, faults[0].type);
			Assert.AreEqual("SocketMonitor", faults[1].detectionSource);
			Assert.AreEqual(FaultType.Data, faults[1].type);
			Assert.AreEqual(desc, faults[1].description);
		}

		[Test]
		public void TestMulticast()
		{
			IPAddress addr;

			using (Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
			{
				s.Connect(new IPEndPoint(IPAddress.Parse("1.1.1.1"), 80));
				addr = ((IPEndPoint)s.LocalEndPoint).Address;
			}

			// Support 'Host' of 234.5.6.7
			ushort port = TestBase.MakePort(46000, 47000);
			string host = "234.5.6.7";
			string desc;

			using (var sender = new UdpSender(addr, host, port, "Hello"))
			{
				desc = string.Format("Received 5 bytes from '{0}'.", sender.Client.LocalEndPoint);
				Run(new Params { { "Protocol", "udp" }, { "Interface", addr.ToString() }, { "Host", host }, { "Port", port.ToString() } }, true);
			}

			Assert.NotNull(faults);
			Assert.AreEqual(1, faults.Length);
			Assert.AreEqual("SocketMonitor", faults[0].detectionSource);
			Assert.AreEqual(FaultType.Fault, faults[0].type);
			Assert.AreEqual(desc, faults[0].description);
			Assert.True(faults[0].collectedData.ContainsKey("Response"));
			Assert.AreEqual(Encoding.ASCII.GetBytes("Hello"), faults[0].collectedData["Response"]);
		}

		[Test, ExpectedException(ExpectedException = typeof(PeachException), ExpectedMessage = "Could not start monitor \"Socket\".  Multicast hosts are not supported with the tcp protocol.")]
		public void TestMulticastTcp()
		{
			// Multicast is not supported when Protocol is tcp
			Run(new Params { { "Host", "234.5.6.7" } }, false);
		}

		[Test]
		public void TestTcpHost()
		{
			// Only accept TCP connections from specified host
			ushort port = TestBase.MakePort(47000, 48000);
			string desc;

			using (var sender = new TcpSender("127.0.0.1", port, "Hello"))
			{
				Run(new Params { { "Host", "127.0.0.2" }, { "Timeout", "1000" }, { "Interface", "127.0.0.1" }, { "Port", port.ToString() } }, false);
			}

			Assert.Null(faults);

			using (var sender = new TcpSender("127.0.0.1", port, "Hello"))
			{
				desc = string.Format("Received 5 bytes from '{0}'.", sender.localEP);
				Run(new Params { { "Host", "127.0.0.1" }, { "Timeout", "1000" }, { "Interface", "127.0.0.1" }, { "Port", port.ToString() } }, true);
			}

			Assert.NotNull(faults);
			Assert.AreEqual(1, faults.Length);
			Assert.AreEqual("SocketMonitor", faults[0].detectionSource);
			Assert.AreEqual(FaultType.Fault, faults[0].type);
			Assert.AreEqual(desc, faults[0].description);
			Assert.True(faults[0].collectedData.ContainsKey("Response"));
			Assert.AreEqual(Encoding.ASCII.GetBytes("Hello"), faults[0].collectedData["Response"]);
		}

		[Test]
		public void TestUdpHost()
		{
			// Only accept UDP connections from specified host
			ushort port = TestBase.MakePort(48000, 49000);
			string desc;

			using (var sender = new UdpSender("127.0.0.1", port, "Hello"))
			{
				Run(new Params { { "Protocol", "udp" }, { "Host", "127.0.0.2" }, { "Timeout", "1000" }, { "Interface", "127.0.0.1" }, { "Port", port.ToString() } }, false);
			}

			Assert.Null(faults);

			using (var sender = new UdpSender("127.0.0.1", port, "Hello"))
			{
				desc = string.Format("Received 5 bytes from '{0}'.", sender.Client.LocalEndPoint);
				Run(new Params { { "Protocol", "udp" }, { "Host", "127.0.0.1" }, { "Timeout", "1000" }, { "Interface", "127.0.0.1" }, { "Port", port.ToString() } }, true);
			}

			Assert.NotNull(faults);
			Assert.AreEqual(1, faults.Length);
			Assert.AreEqual("SocketMonitor", faults[0].detectionSource);
			Assert.AreEqual(FaultType.Fault, faults[0].type);
			Assert.AreEqual(desc, faults[0].description);
			Assert.True(faults[0].collectedData.ContainsKey("Response"));
			Assert.AreEqual(Encoding.ASCII.GetBytes("Hello"), faults[0].collectedData["Response"]);
		}

		[Test]
		public void TestUdp4()
		{
			ushort port = TestBase.MakePort(49000, 50000);
			string desc;

			using (var sender = new UdpSender("127.0.0.1", port, "Hello"))
			{
				desc = string.Format("Received 5 bytes from '{0}'.", sender.Client.LocalEndPoint);
				Run(new Params { { "Protocol", "udp" }, { "Interface", "127.0.0.1" }, { "Port", port.ToString() } }, true);
			}

			Assert.NotNull(faults);
			Assert.AreEqual(1, faults.Length);
			Assert.AreEqual("SocketMonitor", faults[0].detectionSource);
			Assert.AreEqual(FaultType.Fault, faults[0].type);
			Assert.AreEqual(desc, faults[0].description);
			Assert.True(faults[0].collectedData.ContainsKey("Response"));
			Assert.AreEqual(Encoding.ASCII.GetBytes("Hello"), faults[0].collectedData["Response"]);
		}

		[Test]
		public void TestUdp6()
		{
			ushort port = TestBase.MakePort(50000, 51000);
			string desc;

			using (var sender = new UdpSender("::1", port, "Hello"))
			{
				desc = string.Format("Received 5 bytes from '{0}'.", sender.Client.LocalEndPoint);
				Run(new Params { { "Protocol", "udp" }, { "Interface", "::1" }, { "Port", port.ToString() } }, true);
			}

			Assert.NotNull(faults);
			Assert.AreEqual(1, faults.Length);
			Assert.AreEqual("SocketMonitor", faults[0].detectionSource);
			Assert.AreEqual(FaultType.Fault, faults[0].type);
			Assert.AreEqual(desc, faults[0].description);
			Assert.True(faults[0].collectedData.ContainsKey("Response"));
			Assert.AreEqual(Encoding.ASCII.GetBytes("Hello"), faults[0].collectedData["Response"]);
		}

		[Test]
		public void TestTcp4()
		{
			ushort port = TestBase.MakePort(51000, 52000);
			string desc;

			using (var sender = new TcpSender("127.0.0.1", port, "Hello"))
			{
				desc = string.Format("Received 5 bytes from '{0}'.", sender.localEP);
				Run(new Params { { "Interface", "127.0.0.1" }, { "Port", port.ToString() } }, true);
			}

			Assert.NotNull(faults);
			Assert.AreEqual(1, faults.Length);
			Assert.AreEqual("SocketMonitor", faults[0].detectionSource);
			Assert.AreEqual(FaultType.Fault, faults[0].type);
			Assert.AreEqual(desc, faults[0].description);
			Assert.True(faults[0].collectedData.ContainsKey("Response"));
			Assert.AreEqual(Encoding.ASCII.GetBytes("Hello"), faults[0].collectedData["Response"]);
		}

		[Test]
		public void TestTcp6()
		{
			ushort port = TestBase.MakePort(52000, 53000);
			string desc;

			using (var sender = new TcpSender("::1", port, "Hello"))
			{
				desc = string.Format("Received 5 bytes from '{0}'.", sender.localEP);
				Run(new Params { { "Interface", "::1" }, { "Port", port.ToString() } }, true);
			}

			Assert.NotNull(faults);
			Assert.AreEqual(1, faults.Length);
			Assert.AreEqual("SocketMonitor", faults[0].detectionSource);
			Assert.AreEqual(FaultType.Fault, faults[0].type);
			Assert.AreEqual(desc, faults[0].description);
			Assert.True(faults[0].collectedData.ContainsKey("Response"));
			Assert.AreEqual(Encoding.ASCII.GetBytes("Hello"), faults[0].collectedData["Response"]);
		}
	}
}
