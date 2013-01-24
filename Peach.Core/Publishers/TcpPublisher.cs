using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Net.Sockets;

using Peach.Core.Dom;

using NLog;
using System.Net;

namespace Peach.Core.Publishers
{
	public abstract class TcpPublisher : Publisher
	{
		public ushort Port { get; set; }
		public int Timeout { get; set; }

		protected byte[] _recvBuf = new byte[1024];
		protected object _bufferLock = new object();
		protected object _clientLock = new object();
		protected ManualResetEvent _event = null;
		protected TcpClient _client = null;
		protected MemoryStream _buffer = null;
		protected EndPoint _localEp = null;
		protected EndPoint _remoteEp = null;

		public TcpPublisher(Dictionary<string, Variant> args)
			: base(args)
		{
		}

		protected void ScheduleRecv()
		{
			lock (_clientLock)
			{
				System.Diagnostics.Debug.Assert(_client != null);
				_client.Client.BeginReceive(_recvBuf, 0, _recvBuf.Length, SocketFlags.None, OnRecvComplete, _client);
			}
		}

		protected void OnRecvComplete(IAsyncResult ar)
		{
			lock (_clientLock)
			{
				// Already closed!
				if (_client == null)
					return;

				try
				{
					int len = _client.Client.EndReceive(ar);

					if (len == 0)
					{
						Logger.Debug("Read 0 bytes from {0}, closing client connection.", _remoteEp);
						CloseClient();
					}
					else
					{
						Logger.Debug("Read {0} bytes from {0}", len, _remoteEp);

						lock (_bufferLock)
						{
							long pos = _buffer.Position;
							_buffer.Seek(0, SeekOrigin.End);
							_buffer.Write(_recvBuf, 0, len);
							_buffer.Position = pos;

							if (Logger.IsDebugEnabled)
								Logger.Debug("\n" + Utilities.HexDump(_buffer));
						}

						ScheduleRecv();
					}
				}
				catch (Exception ex)
				{
					Logger.Debug("Unable to receive on TCP socket.  " + ex.Message);
					CloseClient();
				}
			}
		}

		protected void StartClient()
		{
			System.Diagnostics.Debug.Assert(_client != null);
			System.Diagnostics.Debug.Assert(_buffer == null);
			System.Diagnostics.Debug.Assert(_localEp == null);
			System.Diagnostics.Debug.Assert(_remoteEp == null);

			_buffer = new MemoryStream();
			_event.Reset();
			_localEp = _client.Client.LocalEndPoint;
			_remoteEp = _client.Client.RemoteEndPoint;
			ScheduleRecv();
		}

		protected void CloseClient()
		{
			lock (_clientLock)
			{
				System.Diagnostics.Debug.Assert(_client != null);
				System.Diagnostics.Debug.Assert(_localEp != null);
				System.Diagnostics.Debug.Assert(_remoteEp != null);
				Logger.Debug("Closing connection to {0}", _remoteEp);
				_client.Close();
				_client = null;
				_remoteEp = null;
				_localEp = null;
				_event.Set();
			}
		}

		protected override void OnStart()
		{
			System.Diagnostics.Debug.Assert(_event == null);
			_event = new ManualResetEvent(true);
		}

		protected override void OnStop()
		{
			if (_event != null)
			{
				_event.Close();
				_event = null;
			}
		}

		protected override void OnOpen()
		{
			System.Diagnostics.Debug.Assert(_client == null);
			System.Diagnostics.Debug.Assert(_buffer == null);
		}

		protected override void OnClose()
		{
			lock (_clientLock)
			{
				if (_client != null)
				{
					Logger.Debug("Shutting down connection to {0}", _remoteEp);

					try
					{
						_client.Client.Shutdown(SocketShutdown.Send);
					}
					catch (SocketException e)
					{
						Logger.Debug("Failed to gracefully shutdown connection to {0}.  {1}", _remoteEp, e.Message);
					}
				}
			}

			if (!_event.WaitOne(TimeSpan.FromMilliseconds(Timeout)))
			{
				lock (_clientLock)
				{
					if (_client != null)
					{
						Logger.Debug("Graceful shutdown of socket timed out.  Force closing...");
						CloseClient();
					}
				}
			}

			if (_buffer != null)
			{
				_buffer.Close();
				_buffer = null;
			}
		}

		protected override void OnInput()
		{
			// Try to make sure 1 byte is available for reading.  Without doing this,
			// state models with an initial state of input can miss the message.
			WantBytes(1);
		}

		protected override void OnOutput(Stream data)
		{
			try
			{
				data.CopyTo(this);
			}
			catch (Exception ex)
			{
				Logger.Error("output: Ignoring error during send.  " + ex.Message);
			}
		}

		public override void WantBytes(long count)
		{
			if (count == 0)
				return;

			lock (_clientLock)
			{
				// If the connection has been closed, we are not going to get anymore bytes.
				if (_client == null)
					return;
			}

			DateTime start = DateTime.Now;

			// Wait up to Timeout milliseconds to see if count bytes become available
			do
			{
				lock (_bufferLock)
				{
					if ((_buffer.Length - _buffer.Position) >= count)
						break;
				}

				Thread.Sleep(100);
			}
			while ((DateTime.Now - start) < TimeSpan.FromMilliseconds(Timeout));
		}

		#region Stream

		public override bool CanRead
		{
			get
			{
				lock (_bufferLock)
				{
					return _buffer.CanRead;
				}
			}
		}

		public override bool CanSeek
		{
			get
			{
				lock (_bufferLock)
				{
					return _buffer.CanSeek;
				}
			}
		}

		public override bool CanWrite
		{
			get
			{
				lock (_clientLock)
				{
					return _client != null && _client.GetStream().CanWrite;
				}
			}
		}

		public override void Flush()
		{
			lock (_clientLock)
			{
				if (_client == null)
					throw new NotSupportedException();

				_client.GetStream().Flush();
			}
		}

		public override long Length
		{
			get
			{
				lock (_bufferLock)
				{
					return _buffer.Length;
				}
			}
		}

		public override long Position
		{
			get
			{
				lock (_bufferLock)
				{
					return _buffer.Position;
				}
			}
			set
			{
				lock (_bufferLock)
				{
					_buffer.Position = value;
				}
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			lock (_bufferLock)
			{
				return _buffer.Read(buffer, offset, count);
			}
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			lock (_bufferLock)
			{
				return _buffer.Seek(offset, origin);
			}
		}

		public override void SetLength(long value)
		{
			lock (_bufferLock)
			{
				_buffer.SetLength(value);
			}
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			lock (_clientLock)
			{
				if (_client == null)
					throw new NotSupportedException();

				_client.GetStream().Write(buffer, offset, count);
			}
		}

		#endregion

	}
}

// end
