using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

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

		private Socket _socket;
		private Fault _fault;

		public SocketMonitor(IAgent agent, string name, Dictionary<string, Variant> args)
			: base(agent, name, args)
		{
			ParameterParser.Parse(this, args);
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
				_fault.description = string.Format("Received {0} bytes from '{1}'.", data.Item1, data.Item2);
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
			_socket.Bind(new IPEndPoint(local, Port));
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
				else
				{
					using (var s = new Socket(Host.AddressFamily, SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp))
					{
						s.Connect(Host, 1);
						local = ((IPEndPoint)s.LocalEndPoint).Address;
					}
				}
			}
			else if (Host != null && local.AddressFamily != Host.AddressFamily)
			{
				throw new PeachException("Interface '{0}' is not compatible with the address family for Host '{1}'.", Interface, Host);
			}

			return local;
		}

		#endregion
	}
}
