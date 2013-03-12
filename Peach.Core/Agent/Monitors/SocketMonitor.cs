using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace Peach.Core.Agent.Monitors
{
	[Monitor("Socket", true)]
	[Parameter("Host", typeof(IPAddress), "IP address of remote host", "")]
	[Parameter("Timeout", typeof(int), "How many milliseconds to wait for data/connection (default 3000)", "3000")]
	[Parameter("Interface", typeof(IPAddress), "IP of interface to listen on", "")]
	[Parameter("Protocol", typeof(Proto), "Protocol type to listen for", "tcp")]
	[Parameter("Port", typeof(ushort), "Port to listen on", "8080")]
	[Parameter("FaultOnSuccess", typeof(bool), "Fault if no connection is recorded", "false")]
	public class SocketMonitor : Peach.Core.Agent.Monitor
	{
		public enum Proto { Udp = ProtocolType.Udp, Tcp = ProtocolType.Tcp }

		public IPAddress    Host           { get; private set; }
		public int          Timeout        { get; private set; }
		public IPAddress    Interface      { get; private set; }
		public Proto        Protocol       { get; private set; }
		public ushort       Port           { get; private set; }
		public int          Backlog        { get; private set; }
		public bool         FaultOnSuccess { get; private set; }

		private const int TcpBlockSize = 1024;
		private const int MaxDgramSize = 65000;
		private MemoryStream _recvBuffer = new MemoryStream();

		private Socket _socket = null;
		private Fault _fault = null;
		private bool _multicast = false;

		public SocketMonitor(IAgent agent, string name, Dictionary<string, Variant> args)
			: base(agent, name, args)
		{
			ParameterParser.Parse(this, args);

			if (Host != null)
			{
				_multicast = Host.IsMulticast();

				if (Interface != null && Interface.AddressFamily != Host.AddressFamily)
					throw new PeachException("Interface '" + Interface + "' is not compatible with the address family for Host '" + Host + "'.");

				if (_multicast && Protocol != Proto.Udp)
					throw new PeachException("Multicast hosts are not supported with the tcp protocol.");
			}
		}

		#region Monitor Interface

		public override void SessionStarting()
		{
			OpenSocket();
		}

		public override void SessionFinished()
		{
			StopMonitor();
		}

		public override void IterationStarting(uint iterationCount, bool isReproduction)
		{
		}

		public override bool IterationFinished()
		{
			_fault = new Fault();
			_fault.detectionSource = "SocketMonitor";
			_fault.folderName = "SocketMonitor";
			_fault.title = "Monitoring " + _socket.LocalEndPoint.ToString();

			Tuple<IPEndPoint, byte[]> data = ReadSocket();
			if (data != null)
			{
				_fault.description = string.Format("Received {0} bytes from '{1}'.", data.Item2.Length, data.Item1);
				_fault.type = FaultOnSuccess ? FaultType.Data : FaultType.Fault;
				_fault.collectedData.Add("Response", data.Item2);
			}
			else
			{
				_fault.description = "No connections recorded.";
				_fault.type = FaultOnSuccess ? FaultType.Fault : FaultType.Data;
			}
			
			return false;
		}

		public override void StopMonitor()
		{
			CloseSocket();
		}

		public override bool MustStop()
		{
			return false;
		}

		public override Variant Message(string name, Variant data)
		{
			return null;
		}

		public override bool DetectedFault()
		{
			return _fault.type == FaultType.Fault;
		}

		public override Fault GetMonitorData()
		{
			return _fault;
		}

		#endregion

		#region Socket Implementation

		private void OpenSocket()
		{
			System.Diagnostics.Debug.Assert(_socket == null);

			IPAddress local = GetLocalIp();
			_socket = new Socket(local.AddressFamily, Protocol == Proto.Tcp ? SocketType.Stream : SocketType.Dgram, (ProtocolType)Protocol);

			if (_multicast)
			{
				if (Platform.GetOS() == Platform.OS.OSX)
				{
					if (local.Equals(IPAddress.Any) || local.Equals(IPAddress.IPv6Any))
						throw new PeachException("Error, the value for parameter 'Interface' can not be '" + local + "' when the 'Host' parameter is multicast.");
				}

				if (Platform.GetOS() == Platform.OS.Windows)
				{
					// Multicast needs to bind to INADDR_ANY on windows
					if (Host.AddressFamily == AddressFamily.InterNetwork)
						_socket.Bind(new IPEndPoint(IPAddress.Any, Port));
					else
						_socket.Bind(new IPEndPoint(IPAddress.IPv6Any, Port));
				}
				else
				{
					// Multicast needs to bind to the group on *nix
					_socket.Bind(new IPEndPoint(Host, Port));
				}

				var level = local.AddressFamily == AddressFamily.InterNetwork ? SocketOptionLevel.IP : SocketOptionLevel.IPv6;
				var opt = new MulticastOption(Host, local);
				_socket.SetSocketOption(level, SocketOptionName.AddMembership, opt);
			}
			else
			{
				_socket.Bind(new IPEndPoint(local, Port));
			}

			if (Protocol == Proto.Tcp)
				_socket.Listen(Backlog);
		}

		private void CloseSocket()
		{
			if (_socket != null)
			{
				_socket.Close();
				_socket = null;
			}
		}

		private Tuple<IPEndPoint, byte[]> ReadSocket()
		{
			IPEndPoint ep;

			_recvBuffer.Seek(0, SeekOrigin.Begin);
			_recvBuffer.SetLength(0);

			if (Protocol == Proto.Udp)
				ep = WaitForData(_socket, 1, MaxDgramSize, Recv);
			else
				ep = WaitForData(_socket, 1, MaxDgramSize, Accept);

			_recvBuffer.Seek(0, SeekOrigin.Begin);

			return ep == null ? null : new Tuple<IPEndPoint, byte[]>(ep, _recvBuffer.ToArray());
		}

		private delegate IPEndPoint IoFunc(Socket s, int blockSize);

		private IPEndPoint WaitForData(Socket s, int maxReads, int blockSize, IoFunc read)
		{
			IPEndPoint ret = null;
			int now = Environment.TickCount;
			int expire = now + Timeout;
			int cnt = 0;

			while ((maxReads < 0 || cnt < maxReads) && now <= expire)
			{
				int remain = expire - now;

				var fds = new List<Socket> { s };
				Socket.Select(fds, null, null, (remain) * 1000);

				if (fds.Count == 0)
					return null;

				long len = _recvBuffer.Length;

				var ep = read(s, blockSize);

				now = Environment.TickCount;

				if (ep != null)
				{
					ret = ep;
					++cnt;
					expire = now + Timeout;

					// EOF
					if (_recvBuffer.Length == len)
						break;
				}
			}

			return ret;
		}

		private IPEndPoint Accept(Socket s, int blockSize)
		{
			using (Socket client = _socket.Accept())
			{
				IPEndPoint remoteEP = (IPEndPoint)client.RemoteEndPoint;

				if (Host != null && !Host.Equals(remoteEP.Address))
				{
					try
					{
						client.Shutdown(SocketShutdown.Both);
					}
					catch
					{
					}

					return null;
				}

					try
					{
						// Indicate we have nothing to send
						client.Shutdown(SocketShutdown.Send);
					}
					catch
					{
					}

				// Read client data
				WaitForData(client, -1, TcpBlockSize, Recv);

				return remoteEP;
			}
		}

		private IPEndPoint Recv(Socket s, int blockSize)
		{
			_recvBuffer.Seek(blockSize - 1, SeekOrigin.Current);
			_recvBuffer.WriteByte(0);
			_recvBuffer.Seek(-blockSize, SeekOrigin.Current);

			int pos = (int)_recvBuffer.Position;
			byte[] buf = _recvBuffer.GetBuffer();

			EndPoint remoteEP;
			int len;

			if (s.SocketType == SocketType.Stream)
			{
				remoteEP = new IPEndPoint(Host ?? IPAddress.Any, 0);
				len = s.Receive(buf, pos, blockSize, SocketFlags.None);
			}
			else
			{
				remoteEP = new IPEndPoint(_socket.AddressFamily == AddressFamily.InterNetwork ? IPAddress.Any : IPAddress.IPv6Any, 0);
				len = s.ReceiveFrom(buf, pos, blockSize, SocketFlags.None, ref remoteEP);
			}

			if (!_multicast && Host != null && !Host.Equals(((IPEndPoint)remoteEP).Address))
				return null;

			_recvBuffer.SetLength(_recvBuffer.Position + len);
			_recvBuffer.Seek(0, SeekOrigin.End);

			return (IPEndPoint)remoteEP;
		}

		private IPAddress GetLocalIp()
		{
			IPAddress local = Interface;

			if (local == null)
			{
				if (Host == null)
				{
					local = IPAddress.Any;
				}
				else if (Host.IsMulticast())
				{
					// Use INADDR_ANY for the local interface which causes the OS to find
					// the interface with the "best" multicast route
					if (Host.AddressFamily == AddressFamily.InterNetwork)
						return IPAddress.Any;
					else
						return IPAddress.IPv6Any;
				}
				else
				{
					using (var s = new Socket(Host.AddressFamily, SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp))
					{
						s.Connect(Host, 1);
						local = ((IPEndPoint)s.LocalEndPoint).Address;
					}
				}
			}

			return local;
		}

		#endregion
	}
}
