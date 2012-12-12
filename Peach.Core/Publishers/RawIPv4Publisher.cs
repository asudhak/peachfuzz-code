
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
	internal static class RawHelpers
	{
		public const int IpHeaderLen = 20;

		public static void SetLength(MemoryStream ms)
		{
			if (ms.Length < IpHeaderLen)
				return;

			var buf = ms.GetBuffer();
			// Get in host order
			ushort ip_len = BitConverter.ToUInt16(buf, 2);
			ip_len += (ushort)(((ushort)(buf[0] & 0x0f)) << 2);
			// Set in network order
			buf[2] = (byte)(ip_len >> 8);
			buf[3] = (byte)(ip_len);
		}
	}

	/// <summary>
	/// Allows for input/output of raw IP packets.
	/// Protocol is the IP protocol number to send/receive.
	/// This publisher does not expect an IP header in the output buffer.
	/// The IP header is always included in the input buffer.
	/// </summary>
	/// <remarks>
	/// Mac raw sockets don't support TCP or UDP receptions.
	/// See the "b. FreeBSD" section at: http://sock-raw.org/papers/sock_raw
	/// </remarks>
	[Publisher("RawV4", true)]
	[Publisher("Raw")]
	[Publisher("raw.Raw")]
	[Parameter("Host", typeof(string), "Hostname or IP address of remote host")]
	[Parameter("Interface", typeof(IPAddress), "IP of interface to bind to", "")]
	[Parameter("Protocol", typeof(byte), "IP protocol to use")]
	[Parameter("Timeout", typeof(int), "How many milliseconds to wait for data/connection (default 3000)", "3000")]
	public class RawV4Publisher : SocketPublisher
	{
		private static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		protected override NLog.Logger Logger { get { return logger; } }

		public RawV4Publisher(Dictionary<string, Variant> args)
			: base("RawV4", args)
		{
		}

		protected override bool AddressFamilySupported(AddressFamily af)
		{
			return af == AddressFamily.InterNetwork;
		}

		protected override Socket OpenSocket(EndPoint remote)
		{
			Socket s = OpenRawSocket(AddressFamily.InterNetwork, Protocol);
			s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, 0);
			return s;
		}

		protected override void FilterInput(MemoryStream ms)
		{
			if (Platform.GetOS() != Platform.OS.OSX)
				return;

			// On OSX, ip_len is in host order and does not include the ip header
			// http://cseweb.ucsd.edu/~braghava/notes/freebsd-sockets.txt
			RawHelpers.SetLength(ms);
		}
	}

	/// <summary>
	/// Allows for input/output of raw IP packets.
	/// Protocol is the IP protocol number to send/receive.
	/// This publisher expects an IP header in the output buffer.
	/// The IP header is always included in the input buffer.
	/// </summary>
	/// <remarks>
	/// Mac raw sockets don't support TCP or UDP receptions.
	/// See the "b. FreeBSD" section at: http://sock-raw.org/papers/sock_raw
	/// </remarks>
	[Publisher("RawIPv4", true)]
	[Publisher("RawIp")]
	[Publisher("raw.RawIp")]
	[Parameter("Host", typeof(string), "Hostname or IP address of remote host")]
	[Parameter("Interface", typeof(IPAddress), "IP of interface to bind to", "")]
	[Parameter("Protocol", typeof(byte), "IP protocol to use")]
	[Parameter("Timeout", typeof(int), "How many milliseconds to wait for data/connection (default 3000)", "3000")]
	public class RawIPv4Publisher : SocketPublisher
	{
		private static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		protected override NLog.Logger Logger { get { return logger; } }

		public RawIPv4Publisher(Dictionary<string, Variant> args)
			: base("RawIPv4", args)
		{
		}

		protected override bool AddressFamilySupported(AddressFamily af)
		{
			return af == AddressFamily.InterNetwork;
		}

		protected override Socket OpenSocket(EndPoint remote)
		{
			Socket s = OpenRawSocket(AddressFamily.InterNetwork, Protocol);
			s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, 1);
			return s;
		}

		protected override void FilterInput(MemoryStream ms)
		{
			if (Platform.GetOS() != Platform.OS.OSX)
				return;

			// On OSX, ip_len is in host order and does not include the ip header
			// http://cseweb.ucsd.edu/~braghava/notes/freebsd-sockets.txt
			RawHelpers.SetLength(ms);
		}

		protected override void FilterOutput(MemoryStream ms)
		{
			if (Platform.GetOS() != Platform.OS.OSX)
				return;


			if (ms.Length < RawHelpers.IpHeaderLen)
				return;

			// On OSX, ip_len and ip_off need to be in host order
			// http://cseweb.ucsd.edu/~braghava/notes/freebsd-sockets.txt

			byte tmp;

			var buf = ms.GetBuffer();

			// Swap ip_len
			tmp = buf[2];
			buf[2] = buf[3];
			buf[3] = tmp;

			// Swap ip_off
			tmp = buf[6];
			buf[6] = buf[7];
			buf[7] = tmp;
		}
	}
}
