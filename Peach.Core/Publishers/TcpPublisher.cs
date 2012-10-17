using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Net.Sockets;

using Peach.Core.Dom;

using NLog;

namespace Peach.Core.Publishers
{
	public class TcpPublisher : Publisher
	{
		public ushort Port { get; set; }
		public int Timeout { get; set; }

		protected byte[] _recvBuf = new byte[1024];
		protected object _bufferLock = new object();
		protected object _clientLock = new object();
		protected ManualResetEvent _event = null;
		protected TcpClient _client = null;
		protected MemoryStream _buffer = null;

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
						logger.Debug("Read 0 bytes from {0}, closing client connection.", _client.Client.RemoteEndPoint);
						CloseClient();
					}
					else
					{
						logger.Debug("Read {0} bytes from {0}", len, _client.Client.RemoteEndPoint);

						lock (_bufferLock)
						{
							long pos = _buffer.Position;
							_buffer.Seek(0, SeekOrigin.End);
							_buffer.Write(_recvBuf, 0, len);
							_buffer.Position = pos;

                            logger.Debug("\n"+Utilities.FormatAsPrettyHex(_recvBuf, 0, len));
						}

						ScheduleRecv();
					}
				}
				catch (SocketException ex)
				{
					logger.Debug("Unable to receive on TCP socket.  " + ex.Message);
					CloseClient();
				}
			}
		}

		protected void StartClient()
		{
			System.Diagnostics.Debug.Assert(_client != null);
			System.Diagnostics.Debug.Assert(_buffer == null);

			_buffer = new MemoryStream();
			_event.Reset();
			ScheduleRecv();
		}

		protected void CloseClient()
		{
			lock (_clientLock)
			{
				System.Diagnostics.Debug.Assert(_client != null);
				logger.Debug("Closing connection to {0}", _client.Client.RemoteEndPoint);
				_client.Close();
				_client = null;
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
					logger.Debug("Shutting down connection to {0}", _client.Client.RemoteEndPoint);
					_client.Client.Shutdown(SocketShutdown.Send);
				}
			}

			if (!_event.WaitOne(TimeSpan.FromMilliseconds(Timeout)))
			{
				lock (_clientLock)
				{
					if (_client != null)
					{
						logger.Debug("Graceful shutdown of socket timed out.  Force closing...");
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
			data.CopyTo(this);
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

				try
				{
					_client.GetStream().Write(buffer, offset, count);
					logger.Debug("Write {0} bytes to {1}", count, _client.Client.RemoteEndPoint);

                    logger.Debug("\n" + Utilities.FormatAsPrettyHex(buffer, offset, count));
                }
				catch (SocketException ex)
				{
					logger.Debug("Failed to write {0} bytes to {1}.  {2}",
						count, _client.Client.RemoteEndPoint, ex.Message);

					throw new SoftException();
				}
			}
		}

		#endregion

	}
}

// end
