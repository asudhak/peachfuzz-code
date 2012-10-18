using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Net;

namespace Peach.Core.Publishers
{
	public abstract class SocketPublisher : Publisher
	{
		public ProtocolType Protocol { get; set; }
		public IPAddress Interface { get; set; }
		public string Host { get; set; }
		public ushort Port { get; set; }
		public ushort SrcPort { get; set; }
		public int Timeout { get; set; }

		public static int MaxSendSize = 65000;

		private string _type = null;
		private Socket _socket = null;
		private MemoryStream _recvBuffer = null;
		private MemoryStream _sendBuffer = null;
		private int _errorsMax = 10;
		private int _errorsOpen = 0;
		private int _errorsSend = 0;
		private int _errorsRecv = 0;

		protected abstract Socket OpenSocket();

		public SocketPublisher(string type, Dictionary<string, Variant> args)
			: base(args)
		{
			_type = type;
		}

		protected override void OnOpen()
		{
			System.Diagnostics.Debug.Assert(_socket == null);

			try
			{
				_socket = OpenSocket();

				if (_recvBuffer == null || _recvBuffer.Capacity < _socket.ReceiveBufferSize)
					_recvBuffer = new MemoryStream(MaxSendSize);

				if (_sendBuffer == null || _sendBuffer.Capacity < _socket.SendBufferSize)
					_sendBuffer = new MemoryStream(MaxSendSize);

				_errorsOpen = 0;
			}
			catch (Exception ex)
			{
				Logger.Error("Unable to open {0} socket to {1}:{2}. {3}.", _type, Host, Port, ex.Message);

				if (++_errorsOpen == _errorsMax)
					throw new PeachException("Failed to open " + _type + " socket after " + _errorsOpen + " attempts.");

				throw new SoftException();
			}
		}

		protected override void OnClose()
		{
			System.Diagnostics.Debug.Assert(_socket != null);
			_socket.Close();
			_socket = null;
		}

		protected override void OnInput()
		{
			System.Diagnostics.Debug.Assert(_socket != null);
			System.Diagnostics.Debug.Assert(_recvBuffer != null);

			_recvBuffer.Seek(0, SeekOrigin.Begin);
			_recvBuffer.SetLength(_recvBuffer.Capacity);

			try
			{
				byte[] buf = _recvBuffer.GetBuffer();
				int offset = (int)_recvBuffer.Position;
				int size = (int)_recvBuffer.Length;

				var ar = _socket.BeginReceive(buf, offset, size, SocketFlags.None, null, null);
				if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(Timeout)))
					throw new TimeoutException();
				var rxLen = _socket.EndReceive(ar);

				_recvBuffer.SetLength(rxLen);
				_errorsRecv = 0;
			}
			catch (Exception ex)
			{
				if (ex is TimeoutException)
				{
					Logger.Debug("{0} packet not received from {1}:{2} in {3}ms, timing out.",
						_type, Host, Port, Timeout);
				}
				else
				{
					Logger.Error("Unable to receive {0} packet from {1}:{2}. {3}",
						_type, Host, Port, ex.Message);
				}

				if (++_errorsRecv == _errorsMax)
					throw new PeachException("Failed to receive " + _type + " packet after " + _errorsRecv + " attempts.");

				throw new SoftException();
			}
		}

		protected override void OnOutput(Stream data)
		{
			System.Diagnostics.Debug.Assert(_socket != null);

			var stream = data as MemoryStream;
			if (stream == null)
			{
				stream = _sendBuffer;
				stream.Seek(0, SeekOrigin.Begin);
				stream.SetLength(0);
				data.CopyTo(stream);
				stream.Seek(0, SeekOrigin.Begin);
			}

			byte[] buf = stream.GetBuffer();
			int offset = (int)stream.Position;
			int size = (int)stream.Length;

			if (size > MaxSendSize)
			{
				if (Iteration == 0)
					throw new PeachException("Data to output is larger than the maximum {0} packet size of {1} bytes.", _type, MaxSendSize);

				// This will be logged below as a truncated send
				size = MaxSendSize;
			}

			try
			{
				var ar = _socket.BeginSend(buf, offset, size, SocketFlags.None, null, null);
				if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(Timeout)))
					throw new TimeoutException();
				var txLen = _socket.EndSend(ar);

				_errorsSend = 0;

				if (data.Length != txLen)
					Logger.Debug("Only sent {0} of {1} byte {2} packet to {3}:{4}.",
						_type, txLen, data.Length, Host, Port);
			}
			catch (Exception ex)
			{
				if (ex is TimeoutException)
				{
					Logger.Debug("{0} packet not sent to {1}:{2} in {3}ms, timing out.",
						_type, Host, Port, Timeout);
				}
				else
				{
					Logger.Error("Unable to send {0} packet to {1}:{2}. {3}",
						_type, Host, Port, ex.Message);
				}

				if (++_errorsSend == _errorsMax)
					throw new PeachException("Failed to send " + _type + " packet after " + _errorsSend + " attempts.");

				throw new SoftException();
			}
		}

		#region Read Stream

		public override bool CanRead
		{
			get { return _recvBuffer.CanRead; }
		}

		public override bool CanSeek
		{
			get { return _recvBuffer.CanSeek; }
		}

		public override long Length
		{
			get { return _recvBuffer.Length; }
		}

		public override long Position
		{
			get { return _recvBuffer.Position; }
			set { _recvBuffer.Position = value; }
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return _recvBuffer.Seek(offset, origin);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return _recvBuffer.Read(buffer, offset, count);
		}

		#endregion
	}
}
