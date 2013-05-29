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
	[Parameter("Host", typeof(string), "Hostname or IP address of remote host")]
	[Parameter("Port", typeof(ushort), "Destination port number", "0")]
	[Parameter("Timeout", typeof(int), "How many milliseconds to wait for data/connection (default 3000)", "3000")]
	[Parameter("Interface", typeof(IPAddress), "IP of interface to bind to", "")]
	[Parameter("SrcPort", typeof(ushort), "Source port number", "0")]
	[Parameter("MinMTU", typeof(uint), "Minimum allowable MTU property value", DefaultMinMTU)]
	[Parameter("MaxMTU", typeof(uint), "Maximum allowable MTU property value", DefaultMaxMTU)]
	public class UdpPublisher : SocketPublisher
	{
		private static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		protected override NLog.Logger Logger { get { return logger; } }
		private IPEndPoint _remote;

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
			_remote = (IPEndPoint)remote;
			Socket s = new Socket(remote.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
			return s;
		}

		protected override void FilterOutput(byte[] buffer, int offset, int count)
		{
			base.FilterOutput(buffer, offset, count);

			if (_remote.Port == 0)
				throw new PeachException("Error sending a Udp packet to " + _remote.Address + ", the port was not specified.");
		}
	}
}
