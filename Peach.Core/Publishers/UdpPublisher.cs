using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Net;
using NLog;

namespace Peach.Core.Publishers
{
	[Publisher("Udp", true)]
	[Parameter("Host", typeof(string), "Hostname or IP address of remote host", true)]
	[Parameter("Port", typeof(ushort), "Destination port number", true)]
	[Parameter("Timeout", typeof(int), "How many milliseconds to wait for data/connection (default 3000)", "3000")]
	[Parameter("Interface", typeof(IPAddress), "IP of interface to bind to", false)]
	[Parameter("SrcPort", typeof(ushort), "Source port number", "0")]
	public class UdpPublisher : SocketPublisher
	{
		private static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		protected override NLog.Logger Logger { get { return logger; } }

		public UdpPublisher(Dictionary<string, Variant> args)
			: base("Udp", args)
		{
		}

		protected override bool AddressFamilySupported(AddressFamily af)
		{
			return (af == AddressFamily.InterNetwork) || (af == AddressFamily.InterNetworkV6);
		}

		protected override Socket OpenSocket(EndPoint remote)
		{
			Socket s = new Socket(remote.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
			return s;
		}
	}
}
