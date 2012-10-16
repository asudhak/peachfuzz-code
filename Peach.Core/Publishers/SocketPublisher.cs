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
		public string Interface { get; set; }
		public string Host { get; set; }
		public ushort Port { get; set; }
		public ushort SrcPort { get; set; }
		public int Timeout { get; set; }

		private string _type = null;
		private Socket _socket = null;
		private EndPoint _recvEp = null;
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
					_recvBuffer = new MemoryStream(_socket.ReceiveBufferSize);

				if (_sendBuffer == null || _sendBuffer.Capacity < _socket.SendBufferSize)
					_sendBuffer = new MemoryStream(_socket.SendBufferSize);

				_errorsOpen = 0;

				if (_socket.AddressFamily == AddressFamily.InterNetwork)
					_recvEp = new IPEndPoint(IPAddress.Any, 0);
				else
					_recvEp = new IPEndPoint(IPAddress.IPv6Any, 0);
			}
			catch (Exception ex)
			{
				logger.Error("Unable to open {0} socket to {1}:{2}. {3}.", _type, Host, Port, ex.Message);

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
			_recvEp = null;
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

				var ar = _socket.BeginReceiveFrom(buf, offset, size, SocketFlags.None, ref _recvEp, null, null);
				if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(Timeout)))
					throw new TimeoutException();
				var rxLen = _socket.EndReceiveFrom(ar, ref _recvEp);

				_recvBuffer.SetLength(rxLen);
				_errorsRecv = 0;
			}
			catch (Exception ex)
			{
				if (ex is TimeoutException)
				{
					logger.Debug("{0} packet not received from {1}:{2} in {3}ms, timing out.",
						_type, Host, Port, Timeout);
				}
				else
				{
					logger.Error("Unable to receive {0} packet from {1}:{2}. {3}",
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

			try
			{
				EndPoint ep = _socket.RemoteEndPoint;
				byte[] buf = stream.GetBuffer();
				int offset = (int)stream.Position;
				int size = (int)stream.Length;

				var ar = _socket.BeginSendTo(buf, offset, size, SocketFlags.None, ep, null, null);
				if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(Timeout)))
					throw new TimeoutException();
				var txLen = _socket.EndSendTo(ar);

				_errorsSend = 0;

				if (stream.Length != txLen)
					logger.Debug("Only sent {0} of {1} byte {2} packet to {3}:{4}.",
						_type, txLen, stream.Length, Host, Port);
			}
			catch (Exception ex)
			{
				if (ex is TimeoutException)
				{
					logger.Debug("{0} packet not sent to {1}:{2} in {3}ms, timing out.",
						_type, Host, Port, Timeout);
				}
				else
				{
					logger.Error("Unable to send {0} packet to {1}:{2}. {3}",
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
