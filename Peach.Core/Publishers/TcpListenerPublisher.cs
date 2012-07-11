using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Peach.Core.Publishers
{
	[Publisher("TcpListener", true)]
	[Publisher("tcp.TcpListener")]
	[ParameterAttribute("Interface", typeof(string), "Interface to bind to (0.0.0.0 for all)", true)]
	[ParameterAttribute("Port", typeof(int), "Local port to listen on", true)]
	[ParameterAttribute("Timeout", typeof(int), "How long to wait for data/connection", false)]
	public class TcpListenerPublisher : TcpClientPublisher
	{
		protected string _interface = "0.0.0.0";
		protected TcpListener _listener = null;

		public TcpListenerPublisher(Dictionary<string, Variant> args)
			: base(args)
		{
			if (args.ContainsKey("Interface"))
				_interface = (string)args["Interface"];

			_port = (short)args["Port"];

			if (args.ContainsKey("Timeout"))
				_timeout = (int)args["Timeout"];
		}

		public override void accept(Dom.Action action)
		{
			try
			{
				_buffer = new MemoryStream();
				TcpClient = _listener.AcceptTcpClient();
			}
			catch (SocketException e)
			{
				throw new PeachException("Error, unable to accept incoming connection: " + e.Message);
			}

			TcpClient.Client.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None,
				new AsyncCallback(ReceiveData), null);

		}

		public override void close(Dom.Action action)
		{
			TcpClient.Close();
			base.close(action);
		}

		public override void start(Dom.Action action)
		{
			base.start(action);

			try
			{
				_listener = new TcpListener(IPAddress.Parse(_interface), _port);
				_listener.Start();
			}
			catch (SocketException e)
			{
				throw new PeachException("Error, unable to bind to interface " + 
					_interface + " on port " + _port + ": " + e.Message);
			}
		}

		public override void stop(Dom.Action action)
		{
			base.stop(action);

			_listener.Stop();
			_listener = null;
		}

		public override void open(Dom.Action action)
		{
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
			get { return TcpClient.GetStream().CanWrite; }
		}

		public override void Flush()
		{
			TcpClient.GetStream().Flush();
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
			OnInput(currentAction, count);

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
			OnOutput(currentAction, new Variant(buffer));

			TcpClient.GetStream().Write(buffer, offset, count);
		}

		#endregion

	}
}

// end
