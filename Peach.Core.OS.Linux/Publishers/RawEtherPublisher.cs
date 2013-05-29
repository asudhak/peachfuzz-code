
#if MONO

using System;
using System.Collections.Generic;
using Peach;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using NLog;
using Mono.Unix;
using Mono.Unix.Native;
using Peach.Core.IO;

namespace Peach.Core.Publishers
{
	[Publisher("RawEther", true)]
	[Publisher("raw.RawEther")]
	[Parameter("Interface", typeof(string), "Name of interface to bind to")]
	[Parameter("Protocol", typeof(EtherProto), "Ethernet protocol to use", "ETH_P_ALL")]
	[Parameter("Timeout", typeof(int), "How many milliseconds to wait for data/connection (default 3000)", "3000")]
	[Parameter("MinMTU", typeof(uint), "Minimum allowable MTU property value", SocketPublisher.DefaultMinMTU)]
	[Parameter("MaxMTU", typeof(uint), "Maximum allowable MTU property value", SocketPublisher.DefaultMaxMTU)]
	public class RawEtherPublisher : Publisher
	{
#region Ethernet Protocols

		public enum EtherProto : ushort
		{
			// These are the defined Ethernet Protocol ID's.
			ETH_P_LOOP       = 0x0060, // Ethernet Loopback packet
			ETH_P_PUP        = 0x0200, // Xerox PUP packet
			ETH_P_PUPAT      = 0x0201, // Xerox PUP Addr Trans packet
			ETH_P_IP         = 0x0800, // Internet Protocol packet
			ETH_P_X25        = 0x0805, // CCITT X.25
			ETH_P_ARP        = 0x0806, // Address Resolution packet
			ETH_P_BPQ        = 0x08FF, // G8BPQ AX.25 Ethernet Packet  [ NOT AN OFFICIALLY REGISTERED ID ]
			ETH_P_IEEEPUP    = 0x0a00, // Xerox IEEE802.3 PUP packet
			ETH_P_IEEEPUPAT  = 0x0a01, // Xerox IEEE802.3 PUP Addr Trans packet
			ETH_P_DEC        = 0x6000, // DEC Assigned proto
			ETH_P_DNA_DL     = 0x6001, // DEC DNA Dump/Load
			ETH_P_DNA_RC     = 0x6002, // DEC DNA Remote Console
			ETH_P_DNA_RT     = 0x6003, // DEC DNA Routing
			ETH_P_LAT        = 0x6004, // DEC LAT
			ETH_P_DIAG       = 0x6005, // DEC Diagnostics
			ETH_P_CUST       = 0x6006, // DEC Customer use
			ETH_P_SCA        = 0x6007, // DEC Systems Comms Arch
			ETH_P_TEB        = 0x6558, // Trans Ether Bridging
			ETH_P_RARP       = 0x8035, // Reverse Addr Res packet
			ETH_P_ATALK      = 0x809B, // Appletalk DDP
			ETH_P_AARP       = 0x80F3, // Appletalk AARP
			ETH_P_8021Q      = 0x8100, // 802.1Q VLAN Extended Header
			ETH_P_IPX        = 0x8137, // IPX over DIX
			ETH_P_IPV6       = 0x86DD, // IPv6 over bluebook
			ETH_P_PAUSE      = 0x8808, // IEEE Pause frames. See 802.3 31B
			ETH_P_SLOW       = 0x8809, // Slow Protocol. See 802.3ad 43B
			ETH_P_WCCP       = 0x883E, // Web-cache coordination protocol defined in draft-wilson-wrec-wccp-v2-00.txt
			ETH_P_PPP_DISC   = 0x8863, // PPPoE discovery messages
			ETH_P_PPP_SES    = 0x8864, // PPPoE session messages
			ETH_P_MPLS_UC    = 0x8847, // MPLS Unicast traffic
			ETH_P_MPLS_MC    = 0x8848, // MPLS Multicast traffic
			ETH_P_ATMMPOA    = 0x884c, // MultiProtocol Over ATM
			ETH_P_LINK_CTL   = 0x886c, // HPNA, wlan link local tunnel
			ETH_P_ATMFATE    = 0x8884, // Frame-based ATM Transport over Ethernet
			ETH_P_PAE        = 0x888E, // Port Access Entity (IEEE 802.1X)
			ETH_P_AOE        = 0x88A2, // ATA over Ethernet
			ETH_P_8021AD     = 0x88A8, // 802.1ad Service VLAN
			ETH_P_TIPC       = 0x88CA, // TIPC
			ETH_P_8021AH     = 0x88E7, // 802.1ah Backbone Service Tag
			ETH_P_1588       = 0x88F7, // IEEE 1588 Timesync
			ETH_P_FCOE       = 0x8906, // Fibre Channel over Ethernet
			ETH_P_TDLS       = 0x890D, // TDLS
			ETH_P_FIP        = 0x8914, // FCoE Initialization Protocol
			ETH_P_QINQ1      = 0x9100, // deprecated QinQ VLAN [ NOT AN OFFICIALLY REGISTERED ID ]
			ETH_P_QINQ2      = 0x9200, // deprecated QinQ VLAN [ NOT AN OFFICIALLY REGISTERED ID ]
			ETH_P_QINQ3      = 0x9300, // deprecated QinQ VLAN [ NOT AN OFFICIALLY REGISTERED ID ]
			ETH_P_EDSA       = 0xDADA, // Ethertype DSA [ NOT AN OFFICIALLY REGISTERED ID ]
			ETH_P_AF_IUCV    = 0xFBFB, // IBM af_iucv [ NOT AN OFFICIALLY REGISTERED ID ]

			// Non DIX types. Won't clash for 1500 types.
			ETH_P_802_3      = 0x0001, // Dummy type for 802.3 frames
			ETH_P_AX25       = 0x0002, // Dummy protocol id for AX.25
			ETH_P_ALL        = 0x0003, // Every packet (be careful!!!)
			ETH_P_802_2      = 0x0004, // 802.2 frames
			ETH_P_SNAP       = 0x0005, // Internal only
			ETH_P_DDCMP      = 0x0006, // DEC DDCMP: Internal only
			ETH_P_WAN_PPP    = 0x0007, // Dummy type for WAN PPP frames
			ETH_P_PPP_MP     = 0x0008, // Dummy type for PPP MP frames
			ETH_P_LOCALTALK  = 0x0009, // Localtalk pseudo type
			ETH_P_CAN        = 0x000C, // Controller Area Network
			ETH_P_PPPTALK    = 0x0010, // Dummy type for Atalk over PPP
			ETH_P_TR_802_2   = 0x0011, // 802.2 frames
			ETH_P_MOBITEX    = 0x0015, // Mobitex (kaz@cafe.net)
			ETH_P_CONTROL    = 0x0016, // Card specific control frames
			ETH_P_IRDA       = 0x0017, // Linux-IrDA
			ETH_P_ECONET     = 0x0018, // Acorn Econet
			ETH_P_HDLC       = 0x0019, // HDLC frames
			ETH_P_ARCNET     = 0x001A, // 1A for ArcNet :-)
			ETH_P_DSA        = 0x001B, // Distributed Switch Arch.
			ETH_P_TRAILER    = 0x001C, // Trailer switch tagging
			ETH_P_PHONET     = 0x00F5, // Nokia Phonet frames
			ETH_P_IEEE802154 = 0x00F6, // IEEE802.15.4 frame
			ETH_P_CAIF       = 0x00F7, // ST-Ericsson CAIF protocol
		}

#endregion

#region P/Invokes

		const int AF_PACKET  = 17;
		const int SOCK_RAW   = 3;
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
			public uint   ifru_mtu;
		}

		[DllImport("libc", SetLastError = true)]
		private static extern int socket(int domain, int type, int protocol);

		[DllImport("libc", SetLastError = true)]
		private static extern int bind(int fd, ref sockaddr_ll addr, int addrlen);

		[DllImport("libc", SetLastError = true)]
		private static extern int if_nametoindex(string ifname);

		[DllImport("libc", SetLastError = true)]
		private static extern int ioctl(int fd, int request, ref ifreq mtu);

#endregion

		public string Interface { get; set; }
		public EtherProto Protocol { get; set; }
		public int Timeout { get; set; }
		public uint MinMTU { get; set; }
		public uint MaxMTU { get; set; }

		private static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		protected override NLog.Logger Logger { get { return logger; } }

		private const uint EthernetHeaderSize = 14;

		// Max IP len is 65535, ensure we can fit that plus ip header plus ethernet header.
		// In order to account for Jumbograms which are > 65535, max MTU is double 65535
		// MinMTU is 128 so that IP info isn't lost if MTU is fuzzed

		private UnixStream _socket = null;
		private MemoryStream _recvBuffer = null;
		private int _bufferSize = 0;
		private uint _mtu = 0;
		private uint orig_mtu = 0;

		public RawEtherPublisher(Dictionary<string, Variant> args)
			: base(args)
		{
		}

		protected override void OnOpen()
		{
			System.Diagnostics.Debug.Assert(_socket == null);

			_socket = OpenSocket(null);

			System.Diagnostics.Debug.Assert(_socket != null);
			System.Diagnostics.Debug.Assert(_bufferSize > 0);

			Logger.Debug("Opened interface \"{0}\" with MTU {1}.", Interface, _bufferSize);
		}

		private UnixStream OpenSocket(uint? mtu)
		{
			sockaddr_ll sa = new sockaddr_ll();
			sa.sll_family = AF_PACKET;
			sa.sll_protocol = (ushort)IPAddress.HostToNetworkOrder((short)Protocol);
			sa.sll_ifindex = if_nametoindex(Interface);

			if (sa.sll_ifindex == 0)
				throw new ArgumentException("The interface \"" + Interface + "\" is not valid.");

			int fd = -1, ret = -1;

			try
			{
				fd = socket(AF_PACKET, SOCK_RAW, sa.sll_protocol);
				UnixMarshal.ThrowExceptionForLastErrorIf(fd);

				ret = bind(fd, ref sa, Marshal.SizeOf(sa));
				UnixMarshal.ThrowExceptionForLastErrorIf(ret);

				ifreq ifr = new ifreq(Interface);

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

				if (ifr.ifru_mtu > (MaxMTU - EthernetHeaderSize))
					_bufferSize = (int)MaxMTU;
				else
					_bufferSize = (int)(ifr.ifru_mtu + EthernetHeaderSize);

				var stream = new UnixStream(fd);
				fd = -1;

				return stream;
			}
			catch (InvalidOperationException ex)
			{
				if (ex.InnerException != null)
				{
					var inner = ex.InnerException as UnixIOException;
					if (inner != null && inner.ErrorCode == Errno.EPERM)
						throw new PeachException("Access denied when opening the raw ethernet publisher.  Ensure the user has the appropriate permissions.", ex);
				}

				throw;
			}
			finally
			{
				if (fd != -1)
					Syscall.close(fd);
			}
		}

		protected override void OnClose()
		{
		        //this never happens....

			System.Diagnostics.Debug.Assert(_socket != null);
			if (orig_mtu != 0)
			  OpenSocket(orig_mtu);
			_socket.Close();
			_socket = null;
		}

		protected override void OnStop()
		{
			if (orig_mtu != 0)
			  OpenSocket(orig_mtu);
		}


		protected override void OnInput()
		{
			System.Diagnostics.Debug.Assert(_socket != null);

			if (_recvBuffer == null || _recvBuffer.Capacity < _bufferSize)
				_recvBuffer = new MemoryStream(_bufferSize);

			_recvBuffer.Seek(0, SeekOrigin.Begin);
			_recvBuffer.SetLength(_recvBuffer.Capacity);

			byte[] buf = _recvBuffer.GetBuffer();
			int offset = (int)_recvBuffer.Position;
			int size = (int)_recvBuffer.Length;

			Pollfd[] fds = new Pollfd[1];
			fds[0].fd = _socket.Handle;
			fds[0].events = PollEvents.POLLIN;

			int expires = Environment.TickCount + Timeout;
			int wait = 0;

			for (;;)
			{
				try
				{
					wait = Math.Max(0, expires - Environment.TickCount);
					fds[0].revents = 0;

					int ret = Syscall.poll(fds, wait);

					if (UnixMarshal.ShouldRetrySyscall(ret))
						continue;

					UnixMarshal.ThrowExceptionForLastErrorIf(ret);

					if (ret == 0)
						throw new TimeoutException();

					if (ret != 1 || (fds[0].revents & PollEvents.POLLIN) == 0)
						continue;

					var rxLen = _socket.Read(buf, offset, size);
					
					_recvBuffer.SetLength(rxLen);

					if (Logger.IsDebugEnabled)
						Logger.Debug("\n\n" + Utilities.HexDump(_recvBuffer));

					// Got a valid packet
					return;
				}
				catch (Exception ex)
				{
					if (ex is TimeoutException)
						Logger.Debug("Ethernet packet not received on {0} in {1}ms, timing out.", Interface, Timeout);
					else
						Logger.Error("Unable to receive ethernet packet on {0}. {1}", Interface, ex.Message);

					throw new SoftException(ex);
				}
			}
		}

		protected override void OnOutput(byte[] buf, int offset, int count)
		{
			int size = count;

			Pollfd[] fds = new Pollfd[1];
			fds[0].fd = _socket.Handle;
			fds[0].events = PollEvents.POLLOUT;

			if (Logger.IsDebugEnabled)
				Logger.Debug("\n\n" + Utilities.HexDump(buf, offset, count));

			int expires = Environment.TickCount + Timeout;
			int wait = 0;

			for (;;)
			{
				try
				{
					wait = Math.Max(0, expires - Environment.TickCount);
					fds[0].revents = 0;

					int ret = Syscall.poll(fds, wait);

					if (UnixMarshal.ShouldRetrySyscall(ret))
						continue;

					UnixMarshal.ThrowExceptionForLastErrorIf(ret);

					if (ret == 0)
						throw new TimeoutException();

					if (ret != 1 || (fds[0].revents & PollEvents.POLLOUT) == 0)
						continue;

					_socket.Write(buf, offset, size);
					
					return;
				}
				catch (Exception ex)
				{
					if (ex is TimeoutException)
						Logger.Debug("Ethernet packet not sent to {0} in {1}ms, timing out.", Interface, Timeout);
					else
						Logger.Error("Unable to send ethernet packet to {0}. {1}", Interface, ex.Message);

					throw new SoftException(ex);
				}
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

		protected override Variant OnGetProperty(string property)
		{
			if (property == "MTU")
			{
				if (_socket != null)
				{
					Logger.Debug("MTU of {0} is {1}.", Interface, _mtu);
					return new Variant(_mtu);
				}

				using (var sock = OpenSocket(null))
				{
					Logger.Debug("MTU of {0} is {1}.", Interface, _mtu);
					return new Variant(_mtu);
				}
			}

			return null;
		}

		protected override void OnSetProperty(string property, Variant value)
		{
			if (property == "MTU")
			{
				uint mtu = 0;

				if (value.GetVariantType() == Variant.VariantType.BitStream)
				{
					var bs = (BitStream)value;
					bs.SeekBits(0, SeekOrigin.Begin);
					int len = (int)Math.Min(bs.LengthBits, 32);
					ulong bits = bs.ReadBits(len);
					mtu = LittleBitWriter.GetUInt32(bits, len);
				}
				else if (value.GetVariantType() == Variant.VariantType.ByteString)
				{
					byte[] buf = (byte[])value;
					int len = Math.Min(buf.Length * 8, 32);
					mtu = LittleBitWriter.GetUInt32(buf, len);
				}
				else
				{
					throw new SoftException("Can't set MTU, 'value' is an unsupported type.");
				}

				if (MaxMTU >= mtu && mtu >= MinMTU)
				{
					try
					{
						using (var sock = OpenSocket(mtu))
						{
							Logger.Debug("Changed MTU of {0} to {1}.", Interface, mtu);
						}
					}
					catch (Exception ex)
					{
						string err = "Failed to change MTU of '{0}' to {1}. {2}".Fmt(Interface, mtu, ex.Message);
						Logger.Error(err);
						var se = new SoftException(err, ex);
						throw new SoftException(se);
					}
				}
			}
		}

#endregion
	}
}

#endif
