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

		private EndPoint _localEp = null;
		private EndPoint _remoteEp = null;
		private string _type = null;
		private Socket _socket = null;
		private MemoryStream _recvBuffer = null;
		private MemoryStream _sendBuffer = null;
		private int _errorsMax = 10;
		private int _errorsOpen = 0;
		private int _errorsSend = 0;
		private int _errorsRecv = 0;

		protected abstract bool AddressFamilySupported(AddressFamily af);

		protected abstract Socket OpenSocket(EndPoint remote);

		public SocketPublisher(string type, Dictionary<string, Variant> args)
			: base(args)
		{
			_type = type;
		}

		private IPEndPoint ResolveHost()
		{
			IPAddress[] entries = Dns.GetHostAddresses(Host);
			foreach (var ip in entries)
			{
				if (ip.ToString() != Host)
					Logger.Debug("Resolved host \"{0}\" to \"{1}\".", Host, ip);

				return new IPEndPoint(ip, Port);
			}

			throw new PeachException("Could not resolve the IP address of host \"{0}\".", Host);
		}

		/// <summary>
		/// Returns the local ip that should be used to talk to 'remote'
		/// </summary>
		/// <param name="remote"></param>
		/// <returns></returns>
		protected static IPAddress GetLocalIp(IPEndPoint remote)
		{
			using (Socket s = new Socket(remote.AddressFamily, SocketType.Dgram, ProtocolType.Udp))
			{
				s.Connect(remote.Address, 22);
				IPEndPoint local = s.LocalEndPoint as IPEndPoint;
				return local.Address;
			}
		}

		protected override void OnOpen()
		{
			System.Diagnostics.Debug.Assert(_socket == null);

			IPEndPoint ep = null;

			try
			{
				ep = ResolveHost();

				if (!AddressFamilySupported(ep.AddressFamily))
					throw new PeachException("The resolved IP '{0}' for host '{1}' is not compatible with the {2} publisher.", ep, Host, _type);

				_socket = OpenSocket(ep);

				IPAddress local = Interface;
				if (Interface == null)
					local = GetLocalIp(ep);

				_socket.Bind(new IPEndPoint(local, SrcPort));
			}
			catch (Exception ex)
			{
				if (_socket != null)
				{
					_socket.Close();
					_socket = null;
				}

				SocketException se = ex as SocketException;
				if (se != null && se.SocketErrorCode == SocketError.AccessDenied)
					throw new PeachException("Access denied when trying open a {0} socket.  Ensure the user has the appropriate permissions.", _type);

				Logger.Error("Unable to open {0} socket to {1}:{2}. {3}.", _type, Host, Port, ex.Message);

				throw new SoftException(ex);
			}

			_socket.ReceiveBufferSize = MaxSendSize;
			_socket.SendBufferSize = MaxSendSize;

			if (_recvBuffer == null || _recvBuffer.Capacity < _socket.ReceiveBufferSize)
				_recvBuffer = new MemoryStream(MaxSendSize);

			if (_sendBuffer == null || _sendBuffer.Capacity < _socket.SendBufferSize)
				_sendBuffer = new MemoryStream(MaxSendSize);

			_localEp = _socket.LocalEndPoint;
			_remoteEp = ep;

			_errorsOpen = 0;
		}

		protected override void OnClose()
		{
			System.Diagnostics.Debug.Assert(_socket != null);
			_socket.Close();
			_localEp = null;
			_remoteEp = null;
			_socket = null;
		}

		protected override void OnInput()
		{
			System.Diagnostics.Debug.Assert(_socket != null);
			System.Diagnostics.Debug.Assert(_recvBuffer != null);

			EndPoint ep = new IPEndPoint(_socket.AddressFamily == AddressFamily.InterNetwork ? IPAddress.Any : IPAddress.IPv6Any, 0);

			int expires = Environment.TickCount + Timeout;
			int wait = 0;

			for (;;)
			{
				wait = Math.Max(0, expires - Environment.TickCount);

				try
				{
					_recvBuffer.Seek(0, SeekOrigin.Begin);
					_recvBuffer.SetLength(_recvBuffer.Capacity);

					byte[] buf = _recvBuffer.GetBuffer();
					int offset = (int)_recvBuffer.Position;
					int size = (int)_recvBuffer.Length;

					var ar = _socket.BeginReceiveFrom(buf, offset, size, SocketFlags.None, ref ep, null, null);
					if (!ar.AsyncWaitHandle.WaitOne(wait))
						throw new TimeoutException();
					var rxLen = _socket.EndReceiveFrom(ar, ref ep);

					_recvBuffer.SetLength(rxLen);
					_errorsRecv = 0;

					if (!IPEndPoint.Equals(ep, _remoteEp))
					{
						Logger.Debug("Ignoring received packet from {0}, want packets from {1}.", ep, _remoteEp);
					}
					else
					{
						if (Logger.IsDebugEnabled)
							Logger.Debug("\n" + Utilities.FormatAsPrettyHex(buf, offset, size));

						// Got a valid packet
						return;
					}
				}
				catch (Exception ex)
				{
					if (ex is SocketException && ((SocketException)ex).SocketErrorCode == SocketError.ConnectionRefused)
					{
						// Eat Connection reset by peer errors
						Logger.Debug("Connection reset by peer.  Ignoring...");
						continue;
					}
					else if (ex is TimeoutException)
					{
						Logger.Debug("{0} packet not received from {1}:{2} in {3}ms, timing out.",
							_type, Host, Port, Timeout);
					}
					else
					{
						Logger.Error("Unable to receive {0} packet from {1}:{2}. {3}",
							_type, Host, Port, ex.Message);
					}

					throw new SoftException(ex);
				}
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
				// This will be logged below as a truncated send
				size = MaxSendSize;
			}

			if (Logger.IsDebugEnabled)
				Logger.Debug("\n" + Utilities.FormatAsPrettyHex(buf, offset));

			try
			{
				var ar = _socket.BeginSendTo(buf, offset, size, SocketFlags.None, _remoteEp, null, null);
				if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(Timeout)))
					throw new TimeoutException();
				var txLen = _socket.EndSendTo(ar);

				_errorsSend = 0;

				if (data.Length != txLen)
					throw new Exception(string.Format("Only sent {0} of {1} byte {2} packet.", _type, txLen, data.Length));
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

				throw new SoftException(ex);
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
