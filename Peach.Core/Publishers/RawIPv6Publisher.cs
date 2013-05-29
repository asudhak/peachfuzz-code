
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
	/// <summary>
	/// Allows for input/output of raw IP packets.
	/// Protocol is the IP protocol number to send/receive.
	/// This publisher does not expect an IP header in the output buffer.
	/// The IP header is always included in the input buffer.
	/// </summary>
	[Publisher("RawV6", true)]
	[Publisher("Raw6")]
	[Publisher("raw.Raw6")]
	[Parameter("Host", typeof(string), "Hostname or IP address of remote host")]
	[Parameter("Interface", typeof(IPAddress), "IP of interface to bind to", "")]
	[Parameter("Protocol", typeof(byte), "IP protocol to use")]
	[Parameter("Timeout", typeof(int), "How many milliseconds to wait for data/connection (default 3000)", "3000")]
	[Parameter("MinMTU", typeof(uint), "Minimum allowable MTU property value", DefaultMinMTU)]
	[Parameter("MaxMTU", typeof(uint), "Maximum allowable MTU property value", DefaultMaxMTU)]
	public class RawV6Publisher : SocketPublisher
	{
		private static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		protected override NLog.Logger Logger { get { return logger; } }

		public RawV6Publisher(Dictionary<string, Variant> args)
			: base("RawV6", args)
		{
		}

		protected override bool AddressFamilySupported(AddressFamily af)
		{
			return af == AddressFamily.InterNetworkV6;
		}

		protected override Socket OpenSocket(EndPoint remote)
		{
			Socket s = OpenRawSocket(AddressFamily.InterNetworkV6, Protocol);
			return s;
		}
	}
}
