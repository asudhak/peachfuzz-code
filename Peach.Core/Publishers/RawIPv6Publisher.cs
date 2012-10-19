
//
// Copyright (c) Michael Eddington
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in	
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

// Authors:
//   Michael Eddington (mike@dejavusecurity.com)

// $Id$

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using Peach.Core.Dom;
using NLog;

namespace Peach.Core.Publishers
{
	[Publisher("RawV6", true)]
	[Publisher("Raw6")]
	[Publisher("raw.Raw6")]
	[Parameter("Host", typeof(string), "Hostname or IP address of remote host", true)]
	[Parameter("Interface", typeof(IPAddress), "IP of interface to bind to", false)]
	[Parameter("Protocol", typeof(ProtocolType), "IP protocol to use", true)]
	[Parameter("Timeout", typeof(int), "How many milliseconds to wait for data/connection (default 3000)", "3000")]
	public class RawV6Publisher : SocketPublisher
	{
		private static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		protected override NLog.Logger Logger { get { return logger; } }

		public RawV6Publisher(Dictionary<string, Variant> args)
			: base("RawV6", args)
		{
			// Protocol 'IP' is really 'Unspecified' and means the socket will include the IP header.
			// This publisher should not include the IP header.  Also, multiple enum values are '0'
			// so use the name passed in args when raising the error
			if (Protocol == ProtocolType.IP)
				throw new PeachException("Protocol \"" + (string)args["Protocol"] + "\" is not supported by the RawV4 publisher.");
		}

		protected override Socket OpenSocket()
		{
			Socket s = new Socket(AddressFamily.InterNetworkV6, SocketType.Raw, ProtocolType.Unspecified);
			if (Interface != null)
				s.Bind(new IPEndPoint(Interface, 0));
			s.Connect(Host, 0);
			return s;
		}
	}

	[Publisher("RawIPv6", true)]
	[Publisher("RawIp6")]
	[Publisher("raw.RawIp6")]
	[Parameter("Host", typeof(string), "Hostname or IP address of remote host", true)]
	[Parameter("Interface", typeof(IPAddress), "IP of interface to bind to", false)]
	[Parameter("Protocol", typeof(ProtocolType), "IP protocol to use", "Unspecified")]
	[Parameter("Timeout", typeof(int), "How many milliseconds to wait for data/connection (default 3000)", "3000")]
	public class RawIPv6Publisher : SocketPublisher
	{
		private static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		protected override NLog.Logger Logger { get { return logger; } }

		public RawIPv6Publisher(Dictionary<string, Variant> args)
			: base("RawIPv6", args)
		{
		}

		protected override Socket OpenSocket()
		{
			Socket s = new Socket(AddressFamily.InterNetworkV6, SocketType.Raw, ProtocolType.Unspecified);
			if (Interface != null)
				s.Bind(new IPEndPoint(Interface, 0));
			s.Connect(Host, 0);
			s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, 1);
			return s;
		}
	}
}
