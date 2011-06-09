
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
using System.Net.Sockets;
using Peach.Core.Dom;

namespace Peach.Core.Publishers
{
	[Publisher("Tcp")]
	[Publisher("tcp.Tcp")]
	[ParameterAttribute("Host", typeof(string), "Hostname or IP address of remote host", true)]
	[ParameterAttribute("Port", typeof(int), "Destination port #", true)]
	[ParameterAttribute("Timeout", typeof(int), "How long to wait for data/connection (default 3 seconds)", false)]
	[ParameterAttribute("Throttle", typeof(int), "Time in milliseconds to wait between connections", false)]
	public class TcpPublisher : Publisher
	{
		string _host = null;
		int _port = 0;
		int _timeout = 3 * 1000;
		int _throttle = 0;
		TcpClient _tcpClient = null;
		NetworkStream _tcpStream = null;
		MemoryStream _buffer = new MemoryStream();
		int _pos = 0;

		public TcpPublisher(Dictionary<string, Variant> args)
			: base(args)
		{
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
			if (_tcpClient != null)
				close(action);

			OnOpen(action);

			try
			{
				_tcpClient = new TcpClient(_host, _port);
			}
			catch (SocketException)
			{
				throw new SoftException();
			}

			_tcpStream = _tcpClient.GetStream();
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

			if (_tcpStream != null)
				_tcpStream.Close();
			if (_tcpClient != null)
				_tcpClient.Close();

			_tcpStream = null;
			_tcpClient = null;
		}

		public override Variant input(Core.Dom.Action action)
		{
			OnInput(action);

			if (_tcpClient == null || _tcpStream == null)
				throw new PeachException("Error, socket not open!");

			if (!_tcpClient.Connected)
				throw new SoftException();

			byte[] buff = new byte[1024];
			int len;

			// Always write to end of _buffer.
			_buffer.Seek(0, SeekOrigin.End);

			// Short read timeout
			_tcpStream.ReadTimeout = 10;
			while (true)
			{
				try
				{
					len = _tcpStream.Read(buff, 0, buff.Length);
				}
				catch
				{
					len = 0;
				}

				if (len == 0)
					break;

				_buffer.Write(buff, 0, len);
			}

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
			if (_tcpClient == null || _tcpStream == null)
				open(action);

			if (!_tcpClient.Connected)
				throw new SoftException();

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

			_tcpStream.ReadTimeout = _timeout;
			_buffer.Position = 0;
			len = _tcpStream.Read(buff, 0, neededLength);
			_buffer.Write(buff, 0, len);

			_buffer.Position = _pos;
			_buffer.Read(buff, 0, size);
			_pos = (int)_buffer.Position;

			return new Variant(buff);
		}

		public override void output(Core.Dom.Action action, Variant data)
		{
			OnOutput(action, data);
			byte [] buff = (byte[]) data;
			_tcpStream.Write(buff, 0, buff.Length);
		}
	}

	[Publisher("TcpListener")]
	[Publisher("tcp.TcpListener")]
	[ParameterAttribute("Interface", typeof(string), "Interface to bind to (0.0.0.0 for all)", true)]
	[ParameterAttribute("Port", typeof(int), "Local port to listen on", true)]
	[ParameterAttribute("Timeout", typeof(int), "How long to wait for data/connection", false)]
	public class TcpListenerPublisher : Publisher
	{
		string _interface = null;
		short _port = 0;
		int _timeout = 0;

		public TcpListenerPublisher(Dictionary<string, Variant> args)
			: base(args)
		{
		}
	}
}

// end
