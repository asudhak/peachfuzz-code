using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace Peach.Core.Agent.Monitors
{
	[Monitor("SocketMonitor", true)]
	[Parameter("Host", typeof(IPAddress), "IP address of remote host", "")]
	[Parameter("Timeout", typeof(int), "How many milliseconds to wait for data/connection (default 3000)", "3000")]
	[Parameter("Interface", typeof(IPAddress), "IP of interface to listen on", "")]
	[Parameter("Protocol", typeof(Proto), "Protocol type to listen for", "tcp")]
	[Parameter("Port", typeof(ushort), "Port to listen on", "8080")]
	[Parameter("Backlog", typeof(int), "Maximum number of pending TCP connections.", "100")]
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
					throw new PeachException("Interface '{0}' is not compatible with the address family for Host '{1}'.", Interface, Host);

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
						throw new PeachException("Error, the value for parameter 'Interface' can not be '{0}' when the 'Host' parameter is multicast.", local);
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
		}

		private void CloseSocket()
		{
			if (_socket != null)
			{
				_socket.Close();
				_socket = null;
			}
		}

		Tuple<IPEndPoint, byte[]> ReadSocket()
		{
			int now = Environment.TickCount;
			int expire = now + Timeout;

			if (_recvBuffer.Capacity < MaxDgramSize)
			{
				_recvBuffer.Seek(MaxDgramSize - 1, SeekOrigin.Begin);
				_recvBuffer.WriteByte(0);
			}

			for (; now <= expire; now = Environment.TickCount)
			{
				var fds = new List<Socket> { _socket };
				Socket.Select(fds, null, null, (expire - now) * 1000);

				if (fds.Count == 0)
					return null;

				_recvBuffer.Seek(0, SeekOrigin.Begin);
				_recvBuffer.SetLength(_recvBuffer.Capacity);

				byte[] buf = _recvBuffer.GetBuffer();
				int offset = (int)_recvBuffer.Position;
				int size = (int)_recvBuffer.Length;

				EndPoint remoteEP = new IPEndPoint(_socket.AddressFamily == AddressFamily.InterNetwork ? IPAddress.Any : IPAddress.IPv6Any, 0);
				int len = _socket.ReceiveFrom(buf, offset, size, SocketFlags.None, ref remoteEP);
				_recvBuffer.SetLength(len);

				if (!_multicast && Host != null && !Host.Equals(((IPEndPoint)remoteEP).Address))
					continue;

				var ret = new Tuple<IPEndPoint, byte[]>((IPEndPoint)remoteEP, _recvBuffer.ToArray());
				return ret;
			}

			return null;
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
