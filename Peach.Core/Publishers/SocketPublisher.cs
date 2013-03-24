using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;

using NLog;

#if MONO
using Mono.Unix;
using Mono.Unix.Native;
#endif

using Peach;
using Peach.Core.IO;

namespace Peach.Core.Publishers
{
	public abstract class SocketPublisher : Publisher
	{
#if MONO
		#region P/Invokes for MTU

		const int AF_PACKET = 17;
		const int SOCK_RAW = 3;
		const int SIOCGIFMTU = 0x8921;
		const int SIOCSIFMTU = 0x8922;

		[StructLayout(LayoutKind.Sequential)]
		struct sockaddr_ll
		{
			public ushort sll_family;
			public ushort sll_protocol;
			public int sll_ifindex;
			public ushort sll_hatype;
			public byte sll_pkttype;
			public byte sll_halen;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
			public byte[] sll_addr;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		struct ifreq
		{
			public ifreq(string ifr_name)
			{
				this.ifr_name = ifr_name;
				this.ifru_mtu = 0;
			}

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
			public string ifr_name;
			public uint ifru_mtu;
		}

		[DllImport("libc", SetLastError = true)]
		private static extern int if_nametoindex(string ifname);

		[DllImport("libc", SetLastError = true)]
		private static extern int ioctl(int fd, int request, ref ifreq mtu);

		#endregion

		#region MTU Related Declarations

		// Max IP len is 65535, ensure we can fit that plus ip header plus ethernet header.
		// In order to account for Jumbograms which are > 65535, max MTU is double 65535
		// MinMTU is 128 so that IP info isn't lost if MTU is fuzzed
		//
		// These values should be made configurable at some point.
		private const uint MaxMtu = 65535 * 2;
		private const uint MinMtu = 128;

		private uint _mtu = 0;
		private uint orig_mtu = 0;

		#endregion
#endif

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

		protected abstract Socket OpenSocket(EndPoint remote, uint? mtu = null);

		public SocketPublisher(string type, Dictionary<string, Variant> args)
			: base(args)
		{
			_type = type;

			// Ensure Protocol is supported
			if (Platform.GetOS() == Platform.OS.Windows)
				GetProtocolType(Protocol);
		}

		public static bool IsRunningOnMono()
		{
			return Type.GetType("Mono.Runtime") != null;
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

			throw new PeachException("Could not resolve the IP address of host \"" + Host + "\".");
		}

		/// <summary>
		/// Resolves the ScopeId for a Link-Local IPv6 address
		/// </summary>
		/// <param name="ip"></param>
		/// <returns></returns>
		private static IPAddress GetScopeId(IPAddress ip)
		{
			if (!ip.IsIPv6LinkLocal || ip.ScopeId != 0)
				throw new ArgumentException("ip");

			var results = new List<Tuple<string, IPAddress>>();
			NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
			foreach (NetworkInterface adapter in nics)
			{
				foreach (var addr in adapter.GetIPProperties().UnicastAddresses)
				{
					if (!addr.Address.IsIPv6LinkLocal)
						continue;

					IPAddress candidate = new IPAddress(addr.Address.GetAddressBytes(), 0);
					if (IPAddress.Equals(candidate, ip))
					{
						results.Add(new Tuple<string,IPAddress>(adapter.Name, addr.Address));
					}
				}
			}

			if (results.Count == 0)
				throw new PeachException("Could not resolve scope id for interface with address '" + ip + "'.");

			if (results.Count != 1)
				throw new PeachException(string.Format("Found multiple interfaces with address '{0}'.{1}\t{2}",
					ip, Environment.NewLine,
					string.Join(Environment.NewLine + "\t", results.Select( a => a.Item1.ToString() + " -> " + a.Item2.ToString()))));

			return results[0].Item2;
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
					throw new PeachException(string.Format("Error, the {0} publisher does not support protocol type 0x{1:X2}.", _type, protocol));
			}
		}

		[DllImport("libc", SetLastError = true)]
		private static extern int socket(int family, int type, int protocol);

		[DllImport("libc", SetLastError=true)]
		private static extern int close(int fd);

		protected static int AF_INET = 2;
		protected static int AF_INET6 = GetInet6();

		static int GetInet6()
		{
			switch (Platform.GetOS())
			{
				case Platform.OS.Windows:
					return (int)AddressFamily.InterNetworkV6;
				case Platform.OS.Linux:
					return 10;
				case Platform.OS.OSX:
					return 30;
				default:
					throw new NotSupportedException();
			}
		}

		protected Socket OpenRawSocket(AddressFamily af, int protocol)
		{
			return OpenRawSocket(af, protocol, null);
		}

		protected string GetInterfaceName(IPAddress Ip)
		{
			foreach(var iface in NetworkInterface.GetAllNetworkInterfaces())
			{
				foreach (var ifaceIp in iface.GetIPProperties().UnicastAddresses)
				{
					if (ifaceIp.Address == Ip)
						return iface.Name;
				}
			}

			throw new Exception("Unable to locate interface for IP: " + Ip.ToString());
		}

		protected Socket OpenRawSocket(AddressFamily af, int protocol, uint? mtu)
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
				Socket temp = new Socket(af, SocketType.Raw, ProtocolType.Udp);

				// Cleanup the object w/o releasing internal state
				var info = temp.DuplicateAndClose(0);

				// ProtocolInformation = [(int)family,(int)type,(int)protocol,(long)fd]
				int oldfd = (int)BitConverter.ToInt64(info.ProtocolInformation, 16);

				// Close the file descriptor
				close(oldfd);

				// Open a new file descriptor for the correct protocol
				int family = af == AddressFamily.InterNetwork ? AF_INET : AF_INET6;
				int fd = socket(family, (int)SocketType.Raw, protocol);

				if (fd != oldfd)
				{
					temp = new Socket(info);
					temp.Close();

					if (fd == -1)
						throw new SocketException((int)WSAGetLastError());

					continue;
				}

				// Start MTU Related Code -- Duplicated in RawEtherPublisher

#if MONO

				int ret = -1;
				ifreq ifr = new ifreq(GetInterfaceName(Interface));

				if (orig_mtu == 0)
				{
					ret = ioctl(fd, SIOCGIFMTU, ref ifr);
					UnixMarshal.ThrowExceptionForLastErrorIf(ret);
					orig_mtu = ifr.ifru_mtu;
				}

				if (mtu != null)
				{
					ifr.ifru_mtu = mtu.Value;
					ret = ioctl(fd, SIOCSIFMTU, ref ifr);
					UnixMarshal.ThrowExceptionForLastErrorIf(ret);
				}

				ret = ioctl(fd, SIOCGIFMTU, ref ifr);
				UnixMarshal.ThrowExceptionForLastErrorIf(ret);

				if (mtu != null && ifr.ifru_mtu != mtu.Value)
					throw new PeachException("MTU change did not take effect.");

				_mtu = ifr.ifru_mtu;

#endif

				// End MTU Related Code

				// Save off the new protocol
				Buffer.BlockCopy(BitConverter.GetBytes((int)protocol), 0, info.ProtocolInformation, 8, 4);

				return new Socket(info);
			}

			throw new PeachException(_type + " publisher could not open raw socket.");
		}

		protected virtual void FilterInput(byte[] buffer, int offset, int count)
		{
		}

		protected virtual void FilterOutput(byte[] buffer, int offset, int count)
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
					throw new PeachException(string.Format("The resolved IP '{0}' for host '{1}' is not compatible with the {2} publisher.", ep, Host, _type));

				_socket = OpenSocket(ep);

				IPAddress local = Interface;
				if (Interface == null)
					local = GetLocalIp(ep);

				if (local.IsIPv6LinkLocal && local.ScopeId == 0)
				{
					local = GetScopeId(local);
					Logger.Trace("Resolved link-local interface IP for {0} socket to {1}.", _type, local);
				}

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
						throw new PeachException(string.Format("Error, the value for parameter 'Interface' can not be '{0}' when the 'Host' parameter is multicast.", Interface));
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
					throw new PeachException(string.Format("Access denied when trying open a {0} socket.  Ensure the user has the appropriate permissions.", _type), ex);

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
						FilterInput(buf, offset, rxLen);

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

		protected override void OnOutput(byte[] buf, int offset, int count)
		{
			System.Diagnostics.Debug.Assert(_socket != null);

			int size = count;

			if (size > MaxSendSize)
			{
				// This will be logged below as a truncated send
				size = MaxSendSize;
			}

			if (Logger.IsDebugEnabled)
				Logger.Debug("\n\n" + Utilities.HexDump(buf, offset, count));

			FilterOutput(buf, offset, count);

			try
			{
				var ar = _socket.BeginSendTo(buf, offset, size, SocketFlags.None, _remoteEp, null, null);
				if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(Timeout)))
					throw new TimeoutException();
				var txLen = _socket.EndSendTo(ar);

				if (count != txLen)
					throw new Exception(string.Format("Only sent {0} of {1} byte {2} packet.", _type, txLen, count));
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

		protected override Variant OnGetProperty(string property)
		{
#if MONO
			if (property == "MTU")
			{
				if (_socket != null)
				{
					Logger.Debug("MTU of {0} is {1}.", Interface, _mtu);
					return new Variant(_mtu);
				}

				var ep = ResolveHost();

				if (!AddressFamilySupported(ep.AddressFamily))
					throw new PeachException(string.Format("The resolved IP '{0}' for host '{1}' is not compatible with the {2} publisher.", ep, Host, _type));

				using (var sock = OpenSocket(ep, null))
				{
					Logger.Debug("MTU of {0} is {1}.", Interface, _mtu);
					return new Variant(_mtu);
				}
			}
#endif

			return null;
		}

		protected override void OnSetProperty(string property, Variant value)
		{
#if MONO
			if (property == "MTU")
			{
				System.Diagnostics.Debug.Assert(value.GetVariantType() == Variant.VariantType.BitStream);
				var bs = (BitStream)value;
				bs.SeekBits(0, SeekOrigin.Begin);
				int len = (int)Math.Min(bs.LengthBits, 32);
				ulong bits = bs.ReadBits(len);
				uint mtu = LittleBitWriter.GetUInt32(bits, len);
				if (MaxMtu >= mtu && mtu >= MinMtu)
				{
					var ep = ResolveHost();

					if (!AddressFamilySupported(ep.AddressFamily))
						throw new PeachException(string.Format("The resolved IP '{0}' for host '{1}' is not compatible with the {2} publisher.", ep, Host, _type));

					using (var sock = OpenSocket(ep, mtu))
					{
						Logger.Debug("Changed MTU of {0} to {1}.", Interface, mtu);
					}
				}
			}
#endif
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
