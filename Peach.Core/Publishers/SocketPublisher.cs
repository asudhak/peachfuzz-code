using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;

namespace Peach.Core.Publishers
{
	public abstract class SocketPublisher : Publisher
	{
		public byte Protocol { get; set; }
		public IPAddress Interface { get; set; }
		public string Host { get; set; }
		public ushort Port { get; set; }
		public ushort SrcPort { get; set; }
		public int Timeout { get; set; }

		public static int MaxSendSize = 65000;

		private bool _multicast = false;
		private EndPoint _localEp = null;
		private EndPoint _remoteEp = null;
		private string _type = null;
		private Socket _socket = null;
		private MemoryStream _recvBuffer = null;
		private MemoryStream _sendBuffer = null;

		protected abstract bool AddressFamilySupported(AddressFamily af);

		protected abstract Socket OpenSocket(EndPoint remote);

		public SocketPublisher(string type, Dictionary<string, Variant> args)
			: base(args)
		{
			_type = type;

			// Ensure Protocol is supported
			if (Platform.GetOS() == Platform.OS.Windows)
				GetProtocolType(Protocol);
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

		private static SocketError WSAGetLastError()
		{
			int err = Marshal.GetLastWin32Error();

			switch (err)
			{
				case 1:  // EPERM -> WSAEACCESS
					return SocketError.AccessDenied;
				case 13: // EACCES -> WSAEACCESS
					return SocketError.AccessDenied;
				case 97: // EAFNOSUPPORT -> WSAEAFNOSUPPORT
					return SocketError.AddressFamilyNotSupported;
				case 22: // EINVAL -> WSAEINVAL
					return SocketError.InvalidArgument;
				case 24: // EMFILE -> WSAEPROCLIM
					return SocketError.ProcessLimit;
				case 23: // ENFILE -> WSAEMFILE
					return SocketError.TooManyOpenSockets;
				case 105: // ENOBUFS -> WSAENOBUFS
					return SocketError.NoBufferSpaceAvailable;
				case 12: // ENOMEM -> WSAENOBUFS
					return SocketError.NoBufferSpaceAvailable;
				case 93: // EPROTONOTSUPPORT -> WSAEPROTONOSUPPORT
					return SocketError.ProtocolNotSupported;
				default:
					return SocketError.SocketError;
			}
		}

		private ProtocolType GetProtocolType(int protocol)
		{
			switch (protocol)
			{
				case 0:   // Dummy Protocol
				case 1:   // Internet Control Message Protocol
				case 2:   // Internet Group Management Protocol
				case 4:   // IPIP tunnels
				case 6:   // Transmission Control Protocol
				case 12:  // PUP protocol
				case 17:  // User Datagram Protocol
				case 22:  // XNS IDP Protocol
				case 41:  // IPv6-in-IPv4 tunneling
				case 43:  // IPv6 routing header
				case 44:  // IPv6 fragmentation header
				case 50:  // Encapsulation Security PAyload protocol
				case 51:  // Authentication Header protocol
				case 58:  // ICMPv6
				case 59:  // IPv6 no next header
				case 60:  // IPv6 destination options
				case 255: // Raw IP packets
					return (ProtocolType)protocol;
				default:
					throw new PeachException("Error, the {0} publisher does not support protocol type 0x{1:X2}.", _type, protocol);
			}
		}

		[DllImport("libc", SetLastError = true)]
		private static extern int socket(int family, int type, int protocol);

		[DllImport("libc", SetLastError=true)]
		private static extern int close(int fd);

		protected Socket OpenRawSocket(AddressFamily af, int protocol)
		{
			if (Platform.GetOS() == Platform.OS.Windows)
			{
				ProtocolType protocolType = GetProtocolType(protocol);
				return new Socket(af, SocketType.Raw, protocolType);
			}

			// This is slightly tricky.  Mono doesn't let us specify socket
			// parameters that are not in the enums. See:
			// https://bugzilla.xamarin.com/show_bug.cgi?id=262
			// http://lists.ximian.com/pipermail/mono-devel-list/2011-July/037847.html
			// Now these links deal with supporting address families other than AF_INET/AF_INET6
			// We just want to support a different protocol.  To do this we need to create a
			// Socket() object, P/Invoke close the handle, and then call libc's socket() and hope
			// We have 10 tries to get the same fd
			for (int i = 0; i < 10; ++i)
			{
				// Generate the internal mono fd tracking state
				Socket temp = new Socket(af, SocketType.Raw, ProtocolType.Unspecified);

				// Cleanup the object w/o releasing internal state
				var info = temp.DuplicateAndClose(0);

				// ProtocolInformation = [(int)family,(int)type,(int)protocol,(long)fd]
				int oldfd = (int)BitConverter.ToInt64(info.ProtocolInformation, 16);

				// Close the file descriptor
				close(oldfd);

				// Open a new file descriptor for the correct protocol
				int fd = socket((int)af, (int)SocketType.Raw, protocol);

				if (fd != oldfd)
				{
					temp = new Socket(info);
					temp.Close();

					if (fd == -1)
						throw new SocketException((int)WSAGetLastError());

					continue;
				}

				// Save off the new protocol
				Buffer.BlockCopy(BitConverter.GetBytes((int)protocol), 0, info.ProtocolInformation, 8, 4);

				return new Socket(info);
			}

			throw new PeachException("{0} publisher could not open raw socket", _type);
		}

		protected virtual void FilterInput(MemoryStream ms)
		{
		}

		protected virtual void FilterOutput(MemoryStream ms)
		{
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

				_multicast = ep.Address.IsMulticast();

				if (_multicast)
				{
					if (Platform.GetOS() == Platform.OS.Windows)
					{
						// Multicast needs to bind to INADDR_ANY on windows
						if (ep.AddressFamily == AddressFamily.InterNetwork)
							_socket.Bind(new IPEndPoint(IPAddress.Any, SrcPort));
						else
							_socket.Bind(new IPEndPoint(IPAddress.IPv6Any, SrcPort));
					}
					else
					{
						// Multicast needs to bind to the group on *nix
						_socket.Bind(new IPEndPoint(ep.Address, SrcPort));
					}

					var level = local.AddressFamily == AddressFamily.InterNetwork ? SocketOptionLevel.IP : SocketOptionLevel.IPv6;
					var opt = new MulticastOption(ep.Address, local);
					_socket.SetSocketOption(level, SocketOptionName.AddMembership, opt);

					if (local != IPAddress.Any && local != IPAddress.IPv6Any)
					{
						Logger.Trace("Setting multicast interface for {0} socket to {1}.", _type, local);
						_socket.SetSocketOption(level, SocketOptionName.MulticastInterface, local.GetAddressBytes());
					}
					else if (Platform.GetOS() == Platform.OS.OSX)
					{
						throw new PeachException("Error, the value for parameter 'Interface' can not be '{0}' when the 'Host' parameter is multicast.", Interface);
					}
				}
				else
				{
					_socket.Bind(new IPEndPoint(local, SrcPort));
				}
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

			Logger.Trace("Opened {0} socket, Local: {1}, Remote: {2}", _type, _localEp, _remoteEp);
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

					if (!_multicast && !IPEndPoint.Equals(ep, _remoteEp))
					{
						Logger.Debug("Ignoring received packet from {0}, want packets from {1}.", ep, _remoteEp);
					}
					else
					{
						FilterInput(_recvBuffer);

						if (Logger.IsDebugEnabled)
							Logger.Debug("\n\n" + Utilities.HexDump(_recvBuffer));

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
				Logger.Debug("\n\n" + Utilities.HexDump(stream));

			FilterOutput(stream);

			try
			{
				var ar = _socket.BeginSendTo(buf, offset, size, SocketFlags.None, _remoteEp, null, null);
				if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(Timeout)))
					throw new TimeoutException();
				var txLen = _socket.EndSendTo(ar);

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
