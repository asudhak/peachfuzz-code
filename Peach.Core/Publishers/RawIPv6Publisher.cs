
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

namespace Peach.Core.Publishers
{
	[Publisher("RawIPv6", true)]
	[Parameter("Host", typeof(string), "Hostname or IP address of remote host", true)]
	[Parameter("Port", typeof(ushort), "Destination port #", true)]
	[Parameter("Timeout", typeof(int), "How long to wait for data/connection (default 3 seconds)", "3")]
	[Parameter("SrcPort", typeof(ushort), "Source port number", "0")]
	public class RawIPv6Publisher : SocketPublisher
	{
		public RawIPv6Publisher(Dictionary<string, Variant> args)
			: base("RawIPv6", args)
		{
		}

		protected override Socket OpenSocket()
		{
			Socket s = new Socket(AddressFamily.InterNetworkV6, SocketType.Raw, ProtocolType.IP);
			s.Bind(new IPEndPoint(IPAddress.IPv6Any, SrcPort));
			s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, 1);
			s.Connect(Host, Port);
			return s;
		}
	}
}
