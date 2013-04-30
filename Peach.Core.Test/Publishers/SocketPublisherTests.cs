using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core;
using Peach.Core.Analyzers;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace Peach.Core.Test.Publishers
{
	class SocketEcho
	{
		public EndPoint remoteEP;
		public byte[] RecvBuf;
		public Socket Socket;
		public int Max = 1;
		public int Count = 0;
		public int WaitTime = 500;
		public IPAddress localIp;

		#region OSX Multicast Goo
		[DllImport("libc")]
		static extern int setsockopt(int socket, int level, int optname, ref IntPtr opt, int optlen);

		[DllImport("libc")]
		static extern uint if_nametoindex(string ifname);

		const int IPPROTO_IPV6 = 0x29;
		const int IPV6_MULTICAST_IF = 0x9;
		#endregion

		public SocketEcho()
		{
		}

		public void SendOnly(IPAddress remote, int port = 5000, string payload = "SendOnly!")
		{
			Socket = new Socket(remote.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

			if (remote.IsIPv6Multicast && Platform.GetOS() == Platform.OS.OSX)
			{
				Assert.AreEqual(localIp, IPAddress.IPv6Loopback);
				IntPtr ptr = new IntPtr(if_nametoindex("lo0"));
				setsockopt(Socket.Handle.ToInt32(), IPPROTO_IPV6, IPV6_MULTICAST_IF, ref ptr, Marshal.SizeOf(typeof(IntPtr)));
			}

			remoteEP = new IPEndPoint(remote, port);
			RecvBuf = Encoding.ASCII.GetBytes(payload);
			Socket.BeginSendTo(RecvBuf, 0, RecvBuf.Length, SocketFlags.None, remoteEP, new AsyncCallback(OnSend), null);
		}

		public void SendRaw(IPAddress remote, int port = 5000)
		{
			Socket = new Socket(remote.AddressFamily, SocketType.Raw, ProtocolType.Pup);
			remoteEP = new IPEndPoint(remote, port);
			RecvBuf = Encoding.ASCII.GetBytes("SendOnly!");
			Socket.BeginSendTo(RecvBuf, 0, RecvBuf.Length, SocketFlags.None, remoteEP, new AsyncCallback(OnSend), null);
		}

		public void Start(IPAddress local, int count = 1)
		{
			Socket = new Socket(local.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
			Socket.ReceiveBufferSize = 65535;
			Socket.Bind(new IPEndPoint(local, 0));
			RecvBuf = new byte[65535];
			remoteEP = new IPEndPoint(local, 0);
			Max = count;
			Socket.BeginReceiveFrom(RecvBuf, 0, RecvBuf.Length, SocketFlags.None, ref remoteEP, new AsyncCallback(OnRecv), null);
		}

		private void OnSend(IAsyncResult ar)
		{
			try
			{
				Socket.EndSend(ar);
			}
			catch (ObjectDisposedException)
			{
				return;
			}
			catch (SocketException ex)
			{
				// Fail
				if (ex.SocketErrorCode != SocketError.ConnectionRefused)
					Assert.Null(ex.Message);
			}
			catch (Exception ex)
			{
				// Fail
				Assert.Null(ex.Message);
			}

			System.Threading.Thread.Sleep(WaitTime);

			try
			{
				Socket.BeginSendTo(RecvBuf, 0, RecvBuf.Length, SocketFlags.None, remoteEP, new AsyncCallback(OnSend), null);
			}
			catch (ObjectDisposedException)
			{
				return;
			}
			catch (Exception ex)
			{
				// Fail
				Assert.Null(ex.Message);
			}

		}

		private void OnRecv(IAsyncResult ar)
		{
			try
			{
				var len = Socket.EndReceiveFrom(ar, ref remoteEP);

				byte[] response = Encoding.ASCII.GetBytes(string.Format("Recv {0} bytes!", len));
				Socket.SendTo(response, remoteEP);
			}
			catch (Exception)
			{
				Socket.Close();
				Socket = null;
				throw;
			}

			if (++Count == Max)
			{
				Socket.Close();
				Socket = null;
			}
			else
			{
				Socket.BeginReceiveFrom(RecvBuf, 0, RecvBuf.Length, SocketFlags.None, ref remoteEP, new AsyncCallback(OnRecv), null);
			}
		}
	}

	[TestFixture]
	class SocketPublisherTests : DataModelCollector
	{
		private static Tuple<string, IPAddress> GetFirstInterface(AddressFamily af)
		{
			if (Platform.GetOS() != Platform.OS.Windows)
			{
				if (af == AddressFamily.InterNetwork)
					return new Tuple<string, IPAddress>("lo", IPAddress.Parse("127.0.0.1"));
				else
					return new Tuple<string, IPAddress>("lo", IPAddress.IPv6Loopback);
			}

			NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
			foreach (NetworkInterface adapter in nics)
			{
				if (adapter.OperationalStatus != OperationalStatus.Up)
					continue;

				foreach (var addr in adapter.GetIPProperties().UnicastAddresses)
				{
					if (addr.Address.AddressFamily != af)
						continue;

					return new Tuple<string, IPAddress>(adapter.Name, addr.Address);
				}
			}

			throw new Exception("Couldn't find a valid network adapter");
		}

		public string template = @"
<Peach>

	<DataModel name=""TheDataModel"">
		<String name=""str"" value=""{3}""/>
	</DataModel>

	<DataModel name=""ResponseModel"">
		<String name=""str"" mutable=""false""/>
	</DataModel>

	<StateModel name=""TheStateModel"" initialState=""InitialState"">
		<State name=""InitialState"">
			<Action name=""Send"" type=""output"">
				<DataModel ref=""TheDataModel""/>
			</Action>

			<Action name=""Recv"" type=""input"">
				<DataModel ref=""ResponseModel""/>
			</Action>

			<Action name=""Addr"" type=""getProperty"" property=""LastRecvAddr"">
				<DataModel name=""LastRecvAddr""/>
			</Action>

		</State>
	</StateModel>

	<Test name=""Default"">
		<StateModel ref=""TheStateModel""/>
		<Publisher class=""{0}"">
			<Param name=""Host"" value=""{1}""/>
			<Param name=""Port"" value=""{2}""/>
			<Param name=""SrcPort"" value=""{4}""/>
		</Publisher>
		<Strategy class=""RandomDeterministic""/>
	</Test>

</Peach>
";

		public string raw_template = @"
<Peach>

	<DataModel name=""TheDataModel"">
		<String name=""str"" value=""Hello World""/>
	</DataModel>

	<DataModel name=""IpDataModel"">
		<Number name=""ihl"" valueType=""hex"" value=""45"" size=""8"" signed=""false""/>
		<Number name=""dscp"" size=""8"" signed=""false""/>
		<Number name=""tot_len"" value=""31"" endian=""big"" size=""16"" signed=""false""/>
		<Number name=""id"" size=""16"" signed=""false""/>
		<Number name=""fragoff"" size=""16"" signed=""false""/>
		<Number name=""ttl"" value= ""1"" size=""8"" signed=""false""/>
		<Number name=""protocol"" value=""13"" size=""8"" signed=""false""/>
		<Number name=""csum"" size=""16"" signed=""false""/>
		<Number name=""src"" valueType=""hex"" value=""7f 00 00 01"" size=""32"" signed=""false"" endian=""big""/>
		<Number name=""dst"" valueType=""hex"" value=""7f 00 00 01"" size=""32"" signed=""false"" endian=""big""/>
	</DataModel>

	<DataModel name=""UdpDataModel"">
		<Number name=""src_port"" size=""16"" signed=""false""/>
		<Number name=""dst_port"" size=""16"" signed=""false""/>
		<Number name=""len"" size=""16"" signed=""false""/>
		<Number name=""csum"" size=""16"" signed=""false""/>
	</DataModel>

	<DataModel name=""ip_packet"">
		<Block name=""ip"" ref=""IpDataModel""/>
		<Block name=""str"" ref=""TheDataModel""/>
	</DataModel>

	<DataModel name=""pup_packet"">
		<Block name=""str"" ref=""TheDataModel""/>
	</DataModel>

	<StateModel name=""IpStateModel"" initialState=""InitialState"">
		<State name=""InitialState"">
			<Action name=""Recv"" type=""input"">
				<DataModel ref=""ip_packet""/>
			</Action>

			<Action name=""Send"" type=""output"">
				<DataModel ref=""ip_packet""/>
			</Action>
		</State>
	</StateModel>

	<StateModel name=""PupStateModel"" initialState=""InitialState"">
		<State name=""InitialState"">
			<Action name=""Recv"" type=""input"">
				<DataModel ref=""ip_packet""/>
			</Action>

			<Action name=""Send"" type=""output"">
				<DataModel ref=""pup_packet""/>
			</Action>
		</State>
	</StateModel>

	<StateModel name=""OspfStateModel"" initialState=""InitialState"">
		<State name=""InitialState"">
			<Action name=""Send"" type=""output"">
				<DataModel ref=""ip_packet""/>
			</Action>
		</State>
	</StateModel>

<Test name=""Default"">
		<StateModel ref=""{2}""/>
		<Publisher class=""{0}"">
			<Param name=""Host"" value=""{1}""/>
			<Param name=""Protocol"" value=""{3}""/>
		</Publisher>
		<Strategy class=""RandomDeterministic""/>
	</Test>

</Peach>
";
		[Test]
		public void UdpTest()
		{
			SocketEcho echo = new SocketEcho();
			echo.Start(IPAddress.Loopback);
			IPEndPoint ep = echo.Socket.LocalEndPoint as IPEndPoint;

			string xml = string.Format(template, "Udp", IPAddress.Loopback, ep.Port, "Hello World", "0");

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			Assert.AreEqual(3, actions.Count);

			var de1 = actions[0].dataModel.find("TheDataModel.str");
			Assert.NotNull(de1);
			var de2 = actions[1].dataModel.find("ResponseModel.str");
			Assert.NotNull(de2);

			string send = (string)de1.DefaultValue;
			string recv = (string)de2.DefaultValue;

			Assert.AreEqual("Hello World", send);
			Assert.AreEqual("Recv 11 bytes!", recv);
			
		}

		[Test]
		public void MulticastUdpTest()
		{
			ushort dstport = TestBase.MakePort(53000, 54000);
			ushort srcport = TestBase.MakePort(54000, 55000);

			SocketEcho echo = new SocketEcho();
			echo.SendOnly(IPAddress.Parse("234.5.6.7"), dstport);

			try
			{
				string xml = string.Format(template, "Udp", "234.5.6.7", srcport.ToString(), "Hello World", dstport.ToString());

				PitParser parser = new PitParser();
				Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

				RunConfiguration config = new RunConfiguration();
				config.singleIteration = true;

				Engine e = new Engine(null);
				e.startFuzzing(dom, config);

				Assert.AreEqual(3, actions.Count);

				var de1 = actions[0].dataModel.find("TheDataModel.str");
				Assert.NotNull(de1);
				var de2 = actions[1].dataModel.find("ResponseModel.str");
				Assert.NotNull(de2);
				var addr = actions[2].dataModel.DefaultValue;
				Assert.NotNull(addr);

				IPAddress ip = new IPAddress((byte[])addr);
				Assert.NotNull(ip);

				string send = (string)de1.DefaultValue;
				string recv = (string)de2.DefaultValue;

				Assert.AreEqual("Hello World", send);
				Assert.AreEqual("SendOnly!", recv);

			}
			finally
			{
				echo.Socket.Close();
			}
		}

		[Test]
		public void MulticastUdp6Test()
		{
			ushort dstport = TestBase.MakePort(53000, 54000);
			ushort srcport = TestBase.MakePort(54000, 55000);
			var local = GetFirstInterface(AddressFamily.InterNetworkV6).Item2;

			SocketEcho echo = new SocketEcho() { localIp = local };
			echo.SendOnly(IPAddress.Parse("ff02::22"), dstport);

			try
			{
				string xml = string.Format(template, "Udp", "ff02::22", srcport.ToString(), "Hello World", dstport.ToString());

				PitParser parser = new PitParser();
				Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
				Peach.Core.Publishers.UdpPublisher pub = dom.tests[0].publishers[0] as Peach.Core.Publishers.UdpPublisher;
				pub.Interface = local; 

				RunConfiguration config = new RunConfiguration();
				config.singleIteration = true;

				Engine e = new Engine(null);
				e.startFuzzing(dom, config);

				Assert.AreEqual(3, actions.Count);

				var de1 = actions[0].dataModel.find("TheDataModel.str");
				Assert.NotNull(de1);
				var de2 = actions[1].dataModel.find("ResponseModel.str");
				Assert.NotNull(de2);

				string send = (string)de1.DefaultValue;
				string recv = (string)de2.DefaultValue;

				Assert.AreEqual("Hello World", send);
				Assert.AreEqual("SendOnly!", recv);
			}
			finally
			{
				echo.Socket.Close();
			}
		}

		[Test]
		public void Udp6Test()
		{
			SocketEcho echo = new SocketEcho();
			echo.Start(IPAddress.IPv6Loopback);
			IPEndPoint ep = echo.Socket.LocalEndPoint as IPEndPoint;

			string xml = string.Format(template, "Udp", IPAddress.IPv6Loopback, ep.Port, "Hello World", "0");

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			Assert.AreEqual(3, actions.Count);

			var de1 = actions[0].dataModel.find("TheDataModel.str");
			Assert.NotNull(de1);
			var de2 = actions[1].dataModel.find("ResponseModel.str");
			Assert.NotNull(de2);

			string send = (string)de1.DefaultValue;
			string recv = (string)de2.DefaultValue;

			Assert.AreEqual("Hello World", send);
			Assert.AreEqual("Recv 11 bytes!", recv);

		}

		[Test]
		public void UdpSizeTest()
		{
			// If the data model is too large, the publisher should throw a PeachException
			string xml = string.Format(template, "Udp", IPAddress.Loopback, 1000, new string('a', 70000), "0");

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			Assert.Throws<PeachException>(delegate() { e.startFuzzing(dom, config); });
		}


		[Test]
		public void UdpSizeMutateTest()
		{
			// If mutation makes the output too large, the socket publisher should skip iteration

			SocketEcho echo = new SocketEcho();
			echo.Start(IPAddress.Loopback, 2);
			IPEndPoint ep = echo.Socket.LocalEndPoint as IPEndPoint;

			string xml = string.Format(template, "Udp", IPAddress.Loopback, ep.Port, new string('a', 40000), "0");

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			dom.tests[0].includedMutators = new List<string>();
			dom.tests[0].includedMutators.Add("DataElementDuplicateMutator");

			RunConfiguration config = new RunConfiguration();
			config.range = true;
			config.rangeStart = 0;
			config.rangeStop = 1;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			Assert.AreEqual(4, dataModels.Count);

			var de1 = dataModels[1].find("ResponseModel.str");
			Assert.NotNull(de1);
			string recv1 = (string)de1.DefaultValue;
			Assert.AreEqual("Recv 40000 bytes!", recv1);
		}

		[Test]
		public void RawIPv4Test()
		{
			SocketEcho echo = new SocketEcho();
			IPAddress self = GetFirstInterface(AddressFamily.InterNetwork).Item2;
			echo.SendRaw(self);

			try
			{
				string xml = string.Format(raw_template, "RawIPv4", self, "IpStateModel", "12");
				PitParser parser = new PitParser();
				Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

				RunConfiguration config = new RunConfiguration();
				config.singleIteration = true;

				Engine e = new Engine(null);
				e.startFuzzing(dom, config);

				Assert.AreEqual(2, actions.Count);
				var de = actions[0].dataModel.find("ip_packet.str.str");
				Assert.NotNull(de);
				string str = (string)de.DefaultValue;
				Assert.AreEqual("SendOnly!", str);
			}
			finally
			{
				echo.Socket.Close();
			}
		}

		[Test]
		public void RawTest()
		{
			SocketEcho echo = new SocketEcho();
			IPAddress self = GetFirstInterface(AddressFamily.InterNetwork).Item2;
			echo.SendRaw(self);

			try
			{
				string xml = string.Format(raw_template, "RawV4", self, "PupStateModel", "12");
				PitParser parser = new PitParser();
				Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

				RunConfiguration config = new RunConfiguration();
				config.singleIteration = true;

				Engine e = new Engine(null);
				e.startFuzzing(dom, config);

				Assert.AreEqual(2, actions.Count);
				var de = actions[0].dataModel.find("ip_packet.str.str");
				Assert.NotNull(de);
				string str = (string)de.DefaultValue;
				Assert.AreEqual("SendOnly!", str);
			}
			finally
			{
				echo.Socket.Close();
			}
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "The resolved IP '127.0.0.1:0' for host '127.0.0.1' is not compatible with the RawV6 publisher.")]
		public void BadAddressFamily()
		{
			// Tests what happens when we give an ipv4 address to an ipv6 publisher.
			string xml = string.Format(raw_template, "RawV6", IPAddress.Loopback, "PupStateModel", "17");
			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);
		}

		static IPAddress GetLinkLocalIPv6()
		{
			NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
			foreach (NetworkInterface adapter in nics)
			{
				if (adapter.OperationalStatus != OperationalStatus.Up)
					continue;

				if (adapter.NetworkInterfaceType != NetworkInterfaceType.Ethernet)
					continue;

				foreach (var ip in adapter.GetIPProperties().UnicastAddresses)
				{
					if (ip.Address.AddressFamily != AddressFamily.InterNetworkV6)
						continue;

					if (!ip.Address.IsIPv6LinkLocal)
						continue;

					return ip.Address;
				}
			}

			return null;
		}

		private string udp6_xml_template = @"
<Peach>
	<DataModel name=""TheDataModel"">
		<String name=""str"" value=""Hello World""/>
	</DataModel>

	<StateModel name=""StateModel"" initialState=""InitialState"">
		<State name=""InitialState"">
			<Action name=""Send"" type=""output"">
				<DataModel ref=""TheDataModel""/>
			</Action>
		</State>
	</StateModel>

<Test name=""Default"">
		<StateModel ref=""StateModel""/>
		<Publisher class=""Udp"">
			<Param name=""Host"" value=""{0}""/>
			<Param name=""Port"" value=""8080""/>
			<Param name=""Interface"" value=""{0}""/>
		</Publisher>
	</Test>
</Peach>
";

		[Test]
		public void TestUdp6Send()
		{
			IPAddress ip = GetLinkLocalIPv6();

			if (ip == null)
				Assert.Ignore("No interface with a link-locak IPv6 address was found.");

			Assert.AreNotEqual(0, ip.ScopeId);
			ip.ScopeId = 0;

			string xml = string.Format(udp6_xml_template, ip);

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			Assert.AreEqual(1, actions.Count);

		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Could not resolve scope id for interface with address 'fe80::'.")]
		public void TestBadUdp6Send()
		{
			string xml = string.Format(udp6_xml_template, "fe80::");

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);
		}

		public void TestMtu(string iface, int mtu)
		{
			string xml = @"
<Peach>
	<DataModel name=""TheDataModel"">
		<Number size=""16"" value=""{1}""/>
	</DataModel>

	<StateModel name=""StateModel"" initialState=""InitialState"">
		<State name=""InitialState"">
			<Action type=""getProperty"" property=""MTU"">
				<DataModel ref=""TheDataModel""/>
			</Action>
			<Action type=""setProperty"" property=""MTU"">
				<DataModel ref=""TheDataModel""/>
			</Action>
			<Action type=""getProperty"" property=""MTU"">
				<DataModel ref=""TheDataModel""/>
			</Action>
		</State>

	</StateModel>

<Test name=""Default"">
		<StateModel ref=""StateModel""/>
		<Publisher class=""Udp"">
			<Param name=""Host"" value=""{0}""/>
			<Param name=""Port"" value=""8080""/>
			<Param name=""Interface"" value=""{0}""/>
		</Publisher>
	</Test>
</Peach>
";

			xml = xml.Fmt(iface, mtu);

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			Assert.AreEqual(3, this.actions.Count);
			Assert.AreEqual(mtu, (int)this.actions[2].dataModel.DefaultValue);
		}

		[Test]
		public void TestMtuInterface()
		{
			IPAddress self = GetFirstInterface(AddressFamily.InterNetwork).Item2;
			TestMtu(self.ToString(), 1280);
		}

		[Test]
		public void TestMtuLoopback()
		{
			try
			{
				TestMtu("127.0.0.1", 1280);
				if (Platform.GetOS() == Platform.OS.Windows)
					Assert.Fail("Should throw");
				Assert.AreEqual(3, this.actions.Count);
				Assert.NotNull(this.actions[0].dataModel.DefaultValue);
			}
			catch (PeachException pe)
			{
				if (Platform.GetOS() != Platform.OS.Windows)
					Assert.Fail("Should not throw");

				Assert.True(pe.Message.Contains("MTU changes are not supported on interface"));
				Assert.AreEqual(2, this.actions.Count);
				Assert.Null(this.actions[0].dataModel.DefaultValue);
			}
		}

		[Test]
		public void TestUdpNoPortSend()
		{
			string xml = string.Format(template, "Udp", IPAddress.Loopback, 0, "Hello World", "0");

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);

			try
			{
				e.startFuzzing(dom, config);
				Assert.Fail("Should throw");
			}
			catch (PeachException pe)
			{
				Assert.AreEqual("Error sending a Udp packet to 127.0.0.1, the port was not specified.", pe.Message);
			}
		}

		[Test]
		public void TestUdpNoPort()
		{
			ushort srcport = TestBase.MakePort(24000, 25000);

			string xml = @"
<Peach>

	<DataModel name=""TheDataModel"">
		<String name=""str""/>
	</DataModel>

	<StateModel name=""TheStateModel"" initialState=""InitialState"">
		<State name=""InitialState"">
			<Action name=""Recv"" type=""input"">
				<DataModel ref=""TheDataModel""/>
			</Action>

			<Action name=""Send"" type=""output"">
				<DataModel ref=""TheDataModel""/>
			</Action>

			<Action name=""Recv"" type=""input"">
				<DataModel ref=""TheDataModel""/>
			</Action>

			<Action name=""Recv"" type=""input"">
				<DataModel ref=""TheDataModel""/>
			</Action>

		</State>
	</StateModel>

	<Test name=""Default"">
		<StateModel ref=""TheStateModel""/>
		<Publisher class=""Udp"">
			<Param name=""Host"" value=""127.0.0.1""/>
			<Param name=""SrcPort"" value=""{0}""/>
		</Publisher>
	</Test>

</Peach>
".Fmt(srcport);

			this.cloneActions = true;

			SocketEcho echo1 = new SocketEcho() { WaitTime = 100 };
			echo1.SendOnly(IPAddress.Loopback, srcport, "Echo1");

			SocketEcho echo2 = new SocketEcho() { WaitTime = 66 };
			echo2.SendOnly(IPAddress.Loopback, srcport, "Echo2");

			try
			{
				PitParser parser = new PitParser();
				Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

				RunConfiguration config = new RunConfiguration();
				config.range = true;
				config.rangeStart = 1;
				config.rangeStop = 200;

				Engine e = new Engine(null);
				e.IterationFinished += new Engine.IterationFinishedEventHandler(e_IterationFinished);
				e.startFuzzing(dom, config);

				int num1 = 0;
				int num2 = 0;

				for (int i = 0; i < this.actions.Count; i += 4)
				{
					var exp = (string)actions[i + 0].dataModel[0].DefaultValue;
					if (exp != "Echo1")
					{
						Assert.AreEqual("Echo2", exp);
						++num2;
					}
					else
					{
						++num1;
					}

					Assert.AreEqual(exp, (string)actions[i + 2].dataModel[0].DefaultValue);
					Assert.AreEqual(exp, (string)actions[i + 3].dataModel[0].DefaultValue);
				}

				Assert.Greater(num1, 0);
				Assert.Greater(num2, 0);
			}
			finally
			{
				echo1.Socket.Close();
				echo2.Socket.Close();
			}
		}

		int numEcho1 = 0;
		int numEcho2 = 0;

		void e_IterationFinished(RunContext context, uint currentIteration)
		{
			if (currentIteration < 2)
				return;

			var v1 = (string)actions[actions.Count - 2].dataModel[0].DefaultValue;
			var v2 = (string)actions[actions.Count - 1].dataModel[0].DefaultValue;

			if (v1 != "Echo1")
			{
				Assert.AreEqual("Echo2", v1);
				++numEcho2;
			}
			else
			{
				++numEcho1;
			}

			Assert.AreEqual(v1, v2);

			if (numEcho1 > 0 && numEcho2 > 0)
				context.config.shouldStop = new RunConfiguration.StopHandler(delegate() { return true; });
		}
	}
}
