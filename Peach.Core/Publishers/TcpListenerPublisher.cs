using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using NLog;

namespace Peach.Core.Publishers
{
	[Publisher("TcpListener", true)]
	[Publisher("tcp.TcpListener")]
	[ParameterAttribute("Interface", typeof(string), "Interface to bind to (0.0.0.0 for all)", true)]
	[ParameterAttribute("Port", typeof(int), "Local port to listen on", true)]
	[ParameterAttribute("Timeout", typeof(int), "How long to wait for data/connection", false)]
	public class TcpListenerPublisher : Publisher
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		protected string _interface = "0.0.0.0";
		protected TcpListener _listener = null;

		protected int _port = 0;
		protected int _timeout = 3 * 1000;
		protected int _throttle = 0;
		protected TcpClient _tcpClient = null;
		protected MemoryStream _buffer = new MemoryStream();
		protected int _pos = 0;
		protected Thread _workThread = null;

		protected byte[] receiveBuffer = new byte[1];

		public TcpListenerPublisher(Dictionary<string, Variant> args)
			: base(args)
		{
			if (args.ContainsKey("Interface"))
				_interface = (string)args["Interface"];

			_port = (short)args["Port"];

			if (args.ContainsKey("Timeout"))
				_timeout = (int)args["Timeout"] * 1000;
		}

		public string Interface
		{
			get { return _interface; }
		}

		public int Port
		{
			get { return _port; }
		}

		protected TcpClient TcpClient
		{
			get { return _tcpClient; }
			set
			{
				_tcpClient = value;
			}
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
				new AsyncCallback(ReceiveData), TcpClient.Client);

			//if (_workThread != null && _workThread.IsAlive)
			//{
			//    _workThread.Abort();
			//    _workThread.Join();
			//    _workThread = null;
			//}

			//ThreadStart threadStart = new ThreadStart(WorkerReceiveData);
			//_workThread = new Thread(threadStart);
			//_workThread.Start();
		}

		protected void WorkerReceiveData()
		{
			TcpClient.Client.Blocking = false;
			TcpClient.Client.ReceiveTimeout = 0;
			
			while (true)
			{
				try
				{
					int recv = TcpClient.Client.Receive(receiveBuffer);
					if (recv == 0)
						Thread.Sleep(100);
					if (recv == -1)
						return;

					lock (_buffer)
					{
						long pos = _buffer.Position;
						_buffer.Seek(0, SeekOrigin.End);
						_buffer.Write(receiveBuffer, 0, recv);
						_buffer.Position = pos;
					}
				}
				catch (SocketException sx)
				{
					if (sx.Message.IndexOf("A non-blocking socket operation could not be completed immediately") == -1)
						throw sx;
				}
				catch
				{
					return;
				}
			}
		}

		protected void ReceiveData(IAsyncResult iar)
		{
			try
			{
				Socket remote = (Socket)iar.AsyncState;

				if (remote == null)
					return;

				int recv = remote.EndReceive(iar);

				lock (_buffer)
				{
					long pos = _buffer.Position;
					_buffer.Seek(0, SeekOrigin.End);
					_buffer.Write(receiveBuffer, 0, recv);
					_buffer.Position = pos;
				}

				_tcpClient.Client.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None,
					new AsyncCallback(ReceiveData), remote);
			}
			catch (ObjectDisposedException)
			{
			}
			catch (Exception ex)
			{
				logger.Error("ReceiveData: Ignoring error: " + ex.ToString());
				//throw new ActionException();
			}
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
				logger.Error("output: Ignoring error from send.: " + ex.ToString());
				//throw new ActionException();
			}
		}


		public override void close(Dom.Action action)
		{
			OnClose(action);
			IsOpen = false;

			try
			{
				if (_tcpClient != null)
				{
					_tcpClient.Close();
				}
			}
			catch
			{
				// Ignore any errors on close, they should just
				// indicate we are already closed :)
			}
			finally
			{
				_tcpClient = null;
			}
		}

		public override void start(Dom.Action action)
		{
			base.start(action);

			try
			{
				if (_listener == null)
				{
					_listener = new TcpListener(IPAddress.Parse(_interface), _port);
					_listener.Start();
				}
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

			if (_listener != null)
			{
				_listener.Stop();
				_listener = null;
			}
		}

		public override void open(Dom.Action action)
		{
			OnOpen(action);
			IsOpen = true;
		}

		public override void WantBytes(long count)
		{
			DateTime start = DateTime.Now;

			while ((_buffer.Length - _buffer.Position) < count && (DateTime.Now - start).TotalMilliseconds < _timeout)
			{
				Thread.Sleep(100);
			}
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
