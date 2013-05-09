using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Peach.Core.Publishers
{
	/// <summary>
	/// Helper class for creating stream based publishers.
	/// This class is used when the publisher implementation
	/// has a non-seekable stream interface.
	/// 
	/// Most derived classes should only need to override OnOpen()
	/// and in the implementation open _client and call StartClient()
	/// to begin async reads from _client to _buffer.
	/// </summary>
	public abstract class BufferedStreamPublisher : Publisher
	{
		public int Timeout { get; set; }

		protected byte[] _recvBuf = new byte[1024];
		protected object _bufferLock = new object();
		protected object _clientLock = new object();
		protected string _clientName = null;
		protected ManualResetEvent _event = null;
		protected Stream _client = null;
		protected MemoryStream _buffer = null;
		protected bool _timeout = false;

		public BufferedStreamPublisher(Dictionary<string, Variant> args)
			: base(args)
		{
		}

		#region Async Read Operations

		protected void ScheduleRead()
		{
			lock (_clientLock)
			{
				try
				{
					System.Diagnostics.Debug.Assert(_client != null);
					ClientBeginRead(_recvBuf, 0, _recvBuf.Length, OnReadComplete, _client);
				}
				catch (Exception ex)
				{
					Logger.Debug("Unable to start reading data from {0}.  {1}", _clientName, ex.Message);
					CloseClient();
				}
			}
		}

		protected void OnReadComplete(IAsyncResult ar)
		{
			lock (_clientLock)
			{
				// Already closed!
				if (_client == null)
					return;

				try
				{
					int len = ClientEndRead(ar);

					if (len == 0)
					{
						Logger.Debug("Read 0 bytes from {0}, closing client connection.", _clientName);
						CloseClient();
					}
					else
					{
						Logger.Debug("Read {0} bytes from {1}", len, _clientName);

						lock (_bufferLock)
						{
							long pos = _buffer.Position;
							_buffer.Seek(0, SeekOrigin.End);
							_buffer.Write(_recvBuf, 0, len);
							_buffer.Position = pos;

							// Reset any timeout value
							_timeout = false;

							if (Logger.IsDebugEnabled)
								Logger.Debug("\n\n" + Utilities.HexDump(_buffer));
						}

						ScheduleRead();
					}
				}
				catch (Exception ex)
				{
					Logger.Debug("Unable to complete reading data from {0}.  {1}", _clientName, ex.Message);
					CloseClient();
				}
			}
		}

		#endregion

		#region Base Client/Buffer Sync Implementations

		protected virtual void StartClient()
		{
			System.Diagnostics.Debug.Assert(_clientName != null);
			System.Diagnostics.Debug.Assert(_client != null);
			System.Diagnostics.Debug.Assert(_buffer == null);

			_buffer = new MemoryStream();
			_event.Reset();
			ScheduleRead();
		}

		protected virtual void CloseClient()
		{
			lock (_clientLock)
			{
				System.Diagnostics.Debug.Assert(_client != null);
				System.Diagnostics.Debug.Assert(_clientName != null);
				Logger.Debug("Closing connection to {0}", _clientName);
				ClientClose();
				_client = null;
				_clientName = null;
				_event.Set();
			}
		}

		protected virtual IAsyncResult ClientBeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			return _client.BeginRead(buffer, offset, count, callback, state);
		}

		protected virtual int ClientEndRead(IAsyncResult asyncResult)
		{
			return _client.EndRead(asyncResult);
		}

		protected virtual void ClientWrite(byte[] buffer, int offset, int count)
		{
			_client.Write(buffer, offset, count);
		}

		protected virtual void ClientShutdown()
		{
			_client.Close();
		}

		protected virtual void ClientClose()
		{
			_client.Close();
		}

		#endregion

		#region Publisher Overrides

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
					Logger.Debug("Shutting down connection to {0}", _clientName);

					try
					{
						ClientShutdown();
					}
					catch (Exception e)
					{
						Logger.Debug("Failed to gracefully shutdown connection to {0}.  {1}", _clientName, e.Message);
					}
				}
			}

			if (!_event.WaitOne(TimeSpan.FromMilliseconds(Timeout)))
			{
				lock (_clientLock)
				{
					if (_client != null)
					{
						Logger.Debug("Graceful shutdown of connection timed out.  Force closing...");
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

		protected override void OnOutput(byte[] buffer, int offset, int count)
		{
			lock (_clientLock)
			{
				try
				{
                    if (Logger.IsDebugEnabled)
                        Logger.Debug("\n\n" + Utilities.HexDump(buffer, offset, count));

					ClientWrite(buffer, offset, count);
				}
				catch (Exception ex)
				{
					Logger.Error("output: Error during send.  " + ex.Message);
					throw new SoftException(ex);
				}
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

				// If we have already timed out, 
			}

			DateTime start = DateTime.Now;

			// Wait up to Timeout milliseconds to see if count bytes become available
			while (true)
			{
				lock (_bufferLock)
				{
					if ((_buffer.Length - _buffer.Position) >= count || _timeout)
						return;

					if ((DateTime.Now - start) >= TimeSpan.FromMilliseconds(Timeout))
					{
						_timeout = true;
						return;
					}
				}

				Thread.Sleep(100);
			}
		}

		#endregion

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

		public override void Flush()
		{
			lock (_clientLock)
			{
				if (_client == null)
					throw new NotSupportedException();

				_client.Flush();
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

		#endregion
	}
}
