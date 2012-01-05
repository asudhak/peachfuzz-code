
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
//   Michael Eddington (mike@phed.org)

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
	[Publisher("RawIPv4")]
	[Publisher("RawIP")]
	[ParameterAttribute("Host", typeof(string), "Hostname or IP address of remote host", true)]
	[ParameterAttribute("Port", typeof(int), "Destination port #", true)]
	[ParameterAttribute("Timeout", typeof(int), "How long to wait for data/connection (default 3 seconds)", false)]
	[ParameterAttribute("Throttle", typeof(int), "Time in milliseconds to wait between connections", false)]
	public class RawIPv4Publisher : Publisher
	{
		protected string _host = null;
		protected int _port = 0;
		protected int _timeout = 3 * 1000;
		protected int _throttle = 0;
		protected Socket _socket = null;
		protected MemoryStream _buffer = new MemoryStream();
		protected int _pos = 0;
		protected EndPoint _remoteEndpoint = null;

		public RawIPv4Publisher(Dictionary<string, Variant> args)
			: base(args)
		{
			_host = (string)args["Host"];
			_port = (int)args["Port"];

			if (args.ContainsKey("Timeout"))
				_timeout = (int)args["Timeout"];
			if (args.ContainsKey("Throttle"))
				_throttle = (int)args["Throttle"];
		}

		public int Timeout
		{
			get { return _timeout; }
			set { _timeout = value; }
		}

		public int Throttle
		{
			get { return _throttle; }
			set { _throttle = value; }
		}

		protected Socket Socket
		{
			get { return _socket; }
			set
			{
				if (_socket != null)
					_socket.Close();

				_socket = value;
			}
		}

		/// <summary>
		/// Open or connect to a resource.  Will be called
		/// automatically if not called specifically.
		/// </summary>
		/// <param name="action">Action calling publisher</param>
		public override void open(Core.Dom.Action action)
		{
			// If socket is open, call close first.  This is what
			// we call an implicit action
			if (_socket != null)
				close(action);

			OnOpen(action);
			
			_socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
			_socket.Bind(new IPEndPoint(IPAddress.Parse("0.0.0.0"), 0));
			_socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, 1);

			_remoteEndpoint = new IPEndPoint(Dns.GetHostEntry(_host).AddressList[0], _port);
		}

		/// <summary>
		/// Close a resource.  Will be called automatically when
		/// state model exists.  Can also be called explicitly when
		/// needed.
		/// </summary>
		/// <param name="action">Action calling publisher</param>
		public override void close(Core.Dom.Action action)
		{
			OnClose(action);

			_socket.Close();
			_socket = null;
		}

		public override Variant input(Core.Dom.Action action)
		{
			OnInput(action);

			if (_socket == null)
				open(action);

			byte[] buff = new byte[1024];
			int len;

			// Always write to end of _buffer.
			_buffer.Seek(0, SeekOrigin.End);

			// Short read timeout
			//_tcpStream.ReadTimeout = 10;
			_socket.Blocking = false;
			while (true)
			{
				try
				{
					len = _socket.Receive(buff, buff.Length, SocketFlags.None);
				}
				catch
				{
					len = 0;
				}

				if (len == 0)
					break;

				_buffer.Write(buff, 0, len);
			}
			_socket.Blocking = true;

			Variant ret = new Variant(_buffer.ToArray());
			_buffer.Close();
			_buffer.Dispose();
			_buffer = new MemoryStream();

			return ret;
		}

		public override Variant input(Core.Dom.Action action, int size)
		{
			OnInput(action, size);

			// Open socket if not already, this is an implicit action
			if (_socket != null)
				open(action);

			byte[] buff;

			// Do we already have enough data?
			if (_buffer.Length - _pos >= size)
			{
				buff = new byte[size];
				_buffer.Position = _pos;
				_buffer.Read(buff, 0, size);

				_pos = (int)_buffer.Position;
				return new Variant(buff);
			}

			int neededLength = size - ((int)_buffer.Length - _pos);
			buff = new byte[size];
			int len;

			_buffer.Position = 0;
			len = _socket.Receive(buff, neededLength, SocketFlags.None);
			_buffer.Write(buff, 0, len);

			_buffer.Position = _pos;
			_buffer.Read(buff, 0, size);
			_pos = (int)_buffer.Position;

			return new Variant(buff);
		}

		public override void output(Core.Dom.Action action, Variant data)
		{
			if (_socket == null)
				open(action);

			OnOutput(action, data);
			byte[] buff = (byte[])data;
			_socket.SendTo(buff, _remoteEndpoint);
		}
	}
}

// end
