
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
using Peach.Core.Dom;

namespace Peach.Core.Publishers
{
	[Publisher("Tcp")]
	[Publisher("TcpClient")]
	[Publisher("tcp.Tcp")]
	[ParameterAttribute("Host", typeof(string), "Hostname or IP address of remote host", true)]
	[ParameterAttribute("Port", typeof(int), "Destination port #", true)]
	[ParameterAttribute("Timeout", typeof(int), "How long to wait for data/connection (default 3 seconds)", false)]
	[ParameterAttribute("Throttle", typeof(int), "Time in milliseconds to wait between connections", false)]
	public class TcpClientPublisher : Publisher
	{
		protected string _host = null;
		protected int _port = 0;
		protected int _timeout = 3 * 1000;
		protected int _throttle = 0;
		protected TcpClient _tcpClient = null;
		protected MemoryStream _buffer = new MemoryStream();
		protected int _pos = 0;

		protected byte[] receiveBuffer = new byte[1024];

		public TcpClientPublisher(Dictionary<string, Variant> args)
			: base(args)
		{
			_host = (string) args["Host"];
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

		protected TcpClient TcpClient
		{
			get { return _tcpClient; }
			set
			{
				_tcpClient = value;
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
			if (_tcpClient != null)
				close(action);

			OnOpen(action);

			for (int cnt = 0; cnt < 10 && _tcpClient == null; cnt++)
			{
				try
				{
					_tcpClient = new TcpClient(_host, _port);
				}
				catch (SocketException)
				{
					_tcpClient = null;
					Thread.Sleep(500);
				}
			}

			if (_tcpClient == null)
				throw new PeachException("Unable to connect to remote host " + _host + " on port " + _port);

			_tcpClient.Client.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None,
				new AsyncCallback(ReceiveData), null);
		}

		/// <summary>
		/// Send data
		/// </summary>
		/// <param name="action">Action calling publisher</param>
		/// <param name="data">Data to send/write</param>
		public override void output(Core.Dom.Action action, Variant data)
		{
			if (_tcpClient == null)
				open(action);

			OnOutput(action, data);

			try
			{
				_tcpClient.Client.Send((byte[])data);
			}
			catch (Exception ex)
			{
				// TODO - Log the exception, but continue!
			}
		}

		protected void ReceiveData(IAsyncResult iar)
		{
			try
			{
				Socket remote = (Socket)iar.AsyncState;
				int recv = remote.EndReceive(iar);

				lock (_buffer)
				{
					long pos = _buffer.Position;
					_buffer.Seek(0, SeekOrigin.End);
					_buffer.Write(receiveBuffer, 0, recv);
					_buffer.Position = pos;
				}

				_tcpClient.Client.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None,
					new AsyncCallback(ReceiveData), null);
			}
			catch (Exception ex)
			{
				// TODO -- Log this exception
			}
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

			if (_tcpClient != null)
				_tcpClient.Close();

			_tcpClient = null;
		}

		#region Stream

		public override bool CanRead
		{
			get
			{
				lock (_buffer)
				{
					return _buffer.CanRead;
				}
			}
		}

		public override bool CanSeek
		{
			get
			{
				lock (_buffer)
				{
					return _buffer.CanSeek;
				}
			}
		}

		public override bool CanWrite
		{
			get { return _tcpClient.GetStream().CanWrite; }
		}

		public override void Flush()
		{
			_tcpClient.GetStream().Flush();
		}

		public override long Length
		{
			get
			{
				lock (_buffer)
				{
					return _buffer.Length;
				}
			}
		}

		public override long Position
		{
			get
			{
				lock (_buffer)
				{
					return _buffer.Position;
				}
			}
			set
			{
				lock (_buffer)
				{
					_buffer.Position = value;
				}
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			lock (_buffer)
			{
				return _buffer.Read(buffer, offset, count);
			}
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			lock (_buffer)
			{
				return _buffer.Seek(offset, origin);
			}
		}

		public override void SetLength(long value)
		{
			lock (_buffer)
			{
				_buffer.SetLength(value);
			}
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			_tcpClient.GetStream().Write(buffer, offset, count);
		}

		#endregion

	}
}

// end
