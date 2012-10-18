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

namespace Peach.Core.Test.Publishers
{
	class SocketEcho
	{
		public EndPoint remoteEP;
		public byte[] RecvBuf;
		public Socket Socket;
		public int Max = 1;
		public int Count = 0;

		public SocketEcho()
		{
		}

		public void SendOnly(IPAddress remote)
		{
			Socket = new Socket(remote.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
			remoteEP = new IPEndPoint(remote, 5000);
			Socket.Connect(remoteEP);
			RecvBuf = Encoding.ASCII.GetBytes("SendOnly!");
			Socket.BeginSend(RecvBuf, 0, RecvBuf.Length, SocketFlags.None, new AsyncCallback(OnSend), null);
		}

		public void Start(IPAddress local, int count = 1)
		{
			Socket = new Socket(local.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
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
				System.Threading.Thread.Sleep(500);
				Socket.BeginSend(RecvBuf, 0, RecvBuf.Length, SocketFlags.None, new AsyncCallback(OnSend), null);
			}
			catch (ObjectDisposedException)
			{
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
					return new Tuple<string, IPAddress>("lo", IPAddress.Loopback);
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
		</State>
	</StateModel>

	<Test name=""Default"">
		<StateModel ref=""TheStateModel""/>
		<Publisher class=""{0}"">
			<Param name=""Host"" value=""{1}""/>
			<Param name=""Port"" value=""{2}""/>
		</Publisher>
	</Test>

</Peach>
";

		public string raw_template = @"
<Peach>

	<DataModel name=""TheDataModel"">
		<String name=""str"" value=""Hello World""/>
	</DataModel>

	<DataModel name=""IpDataModel"">
		<Number name=""ihl"" size=""8"" signed=""false""/>
		<Number name=""dscp"" size=""8"" signed=""false""/>
		<Number name=""tot_len"" size=""16"" signed=""false""/>
		<Number name=""id"" size=""16"" signed=""false""/>
		<Number name=""fragoff"" size=""16"" signed=""false""/>
		<Number name=""ttl"" size=""8"" signed=""false""/>
		<Number name=""protocol"" size=""8"" signed=""false""/>
		<Number name=""csum"" size=""16"" signed=""false""/>
		<Number name=""src"" size=""32"" signed=""false""/>
		<Number name=""dst"" size=""32"" signed=""false""/>
	</DataModel>

	<DataModel name=""UdpDataModel"">
		<Number name=""src_port"" size=""16"" signed=""false""/>
		<Number name=""dst_port"" size=""16"" signed=""false""/>
		<Number name=""len"" size=""16"" signed=""false""/>
		<Number name=""csum"" size=""16"" signed=""false""/>
	</DataModel>

	<DataModel name=""ip_packet"">
		<Block name=""ip"" ref=""IpDataModel""/>
		<Block name=""udp"" ref=""UdpDataModel""/>
		<Block name=""str"" ref=""TheDataModel""/>
	</DataModel>

	<DataModel name=""udp_packet"">
		<Block name=""ip"" ref=""IpDataModel""/>
		<Block name=""udp"" ref=""UdpDataModel""/>
		<Block name=""str"" ref=""TheDataModel""/>
	</DataModel>

	<StateModel name=""IpStateModel"" initialState=""InitialState"">
		<State name=""InitialState"">
			<Action name=""Send"" type=""output"">
				<DataModel ref=""TheDataModel""/>
			</Action>

			<Action name=""Recv"" type=""input"">
				<DataModel ref=""ip_packet""/>
			</Action>
		</State>
	</StateModel>

	<StateModel name=""UdpStateModel"" initialState=""InitialState"">
		<State name=""InitialState"">
			<Action name=""Send"" type=""output"">
				<DataModel ref=""TheDataModel""/>
			</Action>

			<Action name=""Recv"" type=""input"">
				<DataModel ref=""udp_packet""/>
			</Action>
		</State>
	</StateModel>

<Test name=""Default"">
		<StateModel ref=""{2}""/>
		<Publisher class=""{0}"">
			<Param name=""Host"" value=""{1}""/>
			<Param name=""Protocol"" value=""{3}""/>
		</Publisher>
	</Test>

</Peach>
";
		[Test]
		public void UdpTest()
		{
			SocketEcho echo = new SocketEcho();
			echo.Start(IPAddress.Loopback);
			IPEndPoint ep = echo.Socket.LocalEndPoint as IPEndPoint;

			string xml = string.Format(template, "Udp", IPAddress.Loopback, ep.Port, "Hello World");

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.config = config;
			e.startFuzzing(dom, config);

			Assert.AreEqual(2, actions.Count);

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
		public void Udp6Test()
		{
			SocketEcho echo = new SocketEcho();
			echo.Start(IPAddress.IPv6Loopback);
			IPEndPoint ep = echo.Socket.LocalEndPoint as IPEndPoint;

			string xml = string.Format(template, "Udp", IPAddress.IPv6Loopback, ep.Port, "Hello World");

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.config = config;
			e.startFuzzing(dom, config);

			Assert.AreEqual(2, actions.Count);

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
			string xml = string.Format(template, "Udp", IPAddress.Loopback, 1000, new string('a', 70000));

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.config = config;
			Assert.Throws<PeachException>(delegate() { e.startFuzzing(dom, config); });
		}


		[Test]
		public void UdpSizeMutateTest()
		{
			// If mutation makes the output too large, the socket publisher should truncate

			SocketEcho echo = new SocketEcho();
			echo.Start(IPAddress.Loopback, 2);
			IPEndPoint ep = echo.Socket.LocalEndPoint as IPEndPoint;

			string xml = string.Format(template, "Udp", IPAddress.Loopback, ep.Port, new string('a', 40000));

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			dom.tests[0].includedMutators = new List<string>();
			dom.tests[0].includedMutators.Add("DataElementDuplicateMutator");

			RunConfiguration config = new RunConfiguration();
			config.range = true;
			config.rangeStart = 0;
			config.rangeStop = 2;

			Engine e = new Engine(null);
			e.config = config;
			e.startFuzzing(dom, config);

			Assert.AreEqual(4, dataModels.Count);

			var de1 = dataModels[1].find("ResponseModel.str");
			Assert.NotNull(de1);
			string recv1 = (string)de1.DefaultValue;
			Assert.AreEqual("Recv 40000 bytes!", recv1);

			var de2 = dataModels[3].find("ResponseModel.str");
			Assert.NotNull(de2);
			string recv2 = (string)de2.DefaultValue;
			Assert.AreEqual("Recv 65000 bytes!", recv2);
		}

		[Test]
		public void RawIPv4Test()
		{
			SocketEcho echo = new SocketEcho();
			IPAddress self = GetFirstInterface(AddressFamily.InterNetwork).Item2;
			echo.SendOnly(self);

			string xml = string.Format(raw_template, "RawIPv4", self, "IpStateModel", "Unspecified");
			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.config = config;
			e.startFuzzing(dom, config);

			echo.Socket.Close();

			var de = actions[1].dataModel.find("ip_packet.str.str");
			Assert.NotNull(de);
			string str = (string)de.DefaultValue;
			Assert.AreEqual("SendOnly!", str);

		}

		[Test]
		public void RawTest()
		{
			SocketEcho echo = new SocketEcho();
			IPAddress self = GetFirstInterface(AddressFamily.InterNetwork).Item2;
			echo.SendOnly(self);

			string xml = string.Format(raw_template, "RawV4", self, "UdpStateModel", "Udp");
			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.config = config;
			e.startFuzzing(dom, config);

			echo.Socket.Close();

			var de = actions[1].dataModel.find("udp_packet.str.str");
			Assert.NotNull(de);
			string str = (string)de.DefaultValue;
			Assert.AreEqual("SendOnly!", str);

		}
	}
}
