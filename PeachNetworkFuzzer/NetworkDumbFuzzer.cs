using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Peach.Core;
using Peach.Core.IO;
using Peach.Core.Dom;
using Peach.Core.Analyzers;
using Peach.Core.Proxy.Net;
using Peach.Core.Mutators;

namespace PeachNetworkFuzzer
{
	public class NetworkDumbFuzzer
	{
		NetProxy proxy;

		public NetworkDumbFuzzer(string listenAddress, int listenPort, string remoteAddress, int remotePort)
		{
			proxy = new NetProxy(listenAddress, listenPort, remoteAddress, remotePort);
			proxy.ClientDataReceived += new ClientDataReceivedEventHandler(proxy_ClientDataReceived);
			proxy.ServerDataReceived += new ServerDataReceivedEventHandler(proxy_ServerDataReceived);

			proxy.Run();
		}

		void proxy_ServerDataReceived(Peach.Core.Proxy.Connection conn)
		{
			MemoryStream sin = conn.ServerInputStream;
			byte[] buff = new byte[sin.Length - sin.Position];
			int len = sin.Read(buff, 0, buff.Length);

			// TODO - Fuzz the data!

			conn.ClientStream.Write(buff, 0, len);
		}

		void proxy_ClientDataReceived(Peach.Core.Proxy.Connection conn)
		{
			MemoryStream sin = conn.ClientInputStream;
			byte[] buff = new byte[sin.Length - sin.Position];
			int len = sin.Read(buff, 0, buff.Length);

			// TODO - Fuzz the data!

			conn.ServerStream.Write(buff, 0, len);
		}
	}
}
