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

namespace Peach.Core.Test.Publishers
{
	class SocketEcho
	{
		public EndPoint remoteEP;
		public byte[] RecvBuf;
		public Socket Socket;

		public SocketEcho()
		{
		}

		public void Start()
		{
			Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			Socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
			RecvBuf = new byte[Socket.ReceiveBufferSize];
			remoteEP = new IPEndPoint(IPAddress.Any, 0);
			Socket.BeginReceiveFrom(RecvBuf, 0, RecvBuf.Length, SocketFlags.None, ref remoteEP, new AsyncCallback(OnRecv), null);
		}

		private void OnRecv(IAsyncResult ar)
		{
			var len = Socket.EndReceiveFrom(ar, ref remoteEP);

			byte[] response = Encoding.ASCII.GetBytes(string.Format("Recv {0} bytes!", len));
			Socket.SendTo(response, remoteEP);
			Socket.Close();
			Socket = null;
		}
	}

	[TestFixture]
	class SocketPublisherTests : DataModelCollector
	{
		public string template = @"
<Peach>

	<DataModel name=""TheDataModel"">
		<String name=""str"" value=""Hello World""/>
	</DataModel>

	<StateModel name=""TheStateModel"" initialState=""InitialState"">
		<State name=""InitialState"">
			<Action name=""Send"" type=""output"">
				<DataModel ref=""TheDataModel""/>
			</Action>

			<Action name=""Recv"" type=""input"">
				<DataModel ref=""TheDataModel""/>
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
		[Test]
		public void UdpTest()
		{
			SocketEcho echo = new SocketEcho();
			echo.Start();
			IPEndPoint ep = echo.Socket.LocalEndPoint as IPEndPoint;

			string xml = string.Format(template, "Udp", "127.0.0.1", ep.Port);

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
			var de2 = actions[1].dataModel.find("TheDataModel.str");
			Assert.NotNull(de2);

			string send = (string)de1.DefaultValue;
			string recv = (string)de2.DefaultValue;

			Assert.AreEqual("Hello World", send);
			Assert.AreEqual("Recv 11 bytes!", recv);
			
		}
	}
}
