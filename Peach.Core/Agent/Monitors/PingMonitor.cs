using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Net.Sockets;

namespace Peach.Core.Agent.Monitors
{
	[Monitor("Ping", true)]
	[Parameter("Host", typeof(string), "Host to ping")]
	[Parameter("Timeout", typeof(int), "Ping timeout in milliseconds", "1000")]
	[Parameter("Data", typeof(string), "Data to send", "")]
	[Parameter("FaultOnSuccess", typeof(bool), "Fault if ping is successful", "false")]
	public class PingMonitor : Peach.Core.Agent.Monitor
	{
		public string Host    { get; private set; }
		public int Timeout { get; private set; }
		public string Data { get; private set; }
		public bool FaultOnSuccess { get; private set; }

		private Fault _fault = null;
		private static bool hasPermissions = CheckPermissions();

		private static bool CheckPermissions()
		{
			if (Platform.GetOS() == Platform.OS.Windows)
				return true;

			// Mono has two modes of operation for the Ping object, privileged and unprivileged.
			// In privileged mode, mono uses a raw icmp socket and things work well.
			// In unprivileged mode, mono tries to capture stdout from /bin/ping and things don't work well.
			// Therefore, ensure only privileged mode is used.

			try
			{
				using (Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp))
				{
					return true;
				}
			}
			catch
			{
				return false;
			}
		}

		public PingMonitor(IAgent agent, string name, Dictionary<string, Variant> args)
			: base(agent, name, args)
		{
			ParameterParser.Parse(this, args);

			if (!hasPermissions)
				throw new PeachException("Unable to open ICMP socket.  Ensure user has appropriate permissions.");

			if (Platform.GetOS() != Platform.OS.Windows)
			{
				// Mono only receives 100 bytes in its response processing.
				// This means the only payload we can expect to receive is 72 bytes
				// 100 bytes total - 20 byte IP - 8 byte ICMP
				const int maxLen = 100 - 20 - 8;
				int len = Encoding.ASCII.GetByteCount(Data ?? "");
				if (len > maxLen)
					throw new PeachException("Error, the value of parameter 'Data' is longer than the maximum length of " + maxLen + ".");
			}
		}

		public override void StopMonitor() { }
		public override void SessionStarting() { }
		public override void SessionFinished() { }
		public override void IterationStarting(uint iterationCount, bool isReproduction) { }
		public override bool IterationFinished() { return false; }
		public override bool MustStop() { return false; }
		public override Variant Message(string name, Variant data) { return null; }

		public override bool DetectedFault()
		{
			_fault = new Fault();
			_fault.type = FaultType.Fault;
			_fault.detectionSource = "PingMonitor";

			try
			{
				using (var ping = new Ping())
				{
					PingReply reply = null;

					if (string.IsNullOrEmpty(Data))
						reply = ping.Send(Host, Timeout);
					else
						reply = ping.Send(Host, Timeout, ASCIIEncoding.ASCII.GetBytes(Data));

					if (reply.Status == IPStatus.Success)
						_fault.type = FaultOnSuccess ? FaultType.Fault : FaultType.Data;
					else
						_fault.type = FaultOnSuccess ? FaultType.Data : FaultType.Fault;

					_fault.title = "Ping Reply";
					_fault.description = MakeDescription(reply);
				}
			}
			catch (Exception ex)
			{
				_fault.title = "Exception";

				if (ex is PingException)
					_fault.description = ex.InnerException.Message;
				else
					_fault.description = ex.Message;
			}

			return _fault.type == FaultType.Fault;
		}

		public override Fault GetMonitorData()
		{
			return _fault;
		}

		static string MakeDescription(PingReply reply)
		{
			switch (reply.Status)
			{
				case IPStatus.Success:
					StringBuilder sb = new StringBuilder();
					sb.AppendFormat("Address: {0}", reply.Address.ToString ());
					sb.AppendLine();
					sb.AppendFormat("RoundTrip time: {0}", reply.RoundtripTime);
					sb.AppendLine();
					sb.AppendFormat("Time to live: {0}", reply.Options.Ttl);
					sb.AppendLine();
					sb.AppendFormat("Don't fragment: {0}", reply.Options.DontFragment);
					sb.AppendLine();
					sb.AppendFormat("Buffer size: {0}", reply.Buffer.Length);
					sb.AppendLine();
					return sb.ToString();
				case IPStatus.Unknown:
					return "The ICMP echo request failed for an unknown reason.";
				case IPStatus.DestinationNetworkUnreachable:
					return "The ICMP echo request failed because the network that contains the destination computer is not reachable.";
				case IPStatus.DestinationHostUnreachable:
					return "The ICMP echo request failed because the destination computer is not reachable.";
				case IPStatus.DestinationProhibited:
					return "The ICMP echo request failed because contact with the destination computer is administratively prohibited.";
				//case IPStatus.DestinationProtocolUnreachable:
				//	return "The ICMP echo request failed because the destination computer that is specified in an ICMP echo message is not reachable, because it does not support the packet's protocol.";
				case IPStatus.DestinationPortUnreachable:
					return "The ICMP echo request failed because the port on the destination computer is not available.";
				case IPStatus.NoResources:
					return "The ICMP echo request failed because of insufficient network resources.";
				case IPStatus.BadOption:
					return "The ICMP echo request failed because it contains an invalid option.";
				case IPStatus.HardwareError:
					return "The ICMP echo request failed because of a hardware error.";
				case IPStatus.PacketTooBig:
					return "The ICMP echo request failed because the packet containing the request is larger than the maximum transmission unit (MTU) of a node (router or gateway) located between the source and destination. The MTU defines the maximum size of a transmittable packet.";
				case IPStatus.TimedOut:
					return "The ICMP echo Reply was not received within the allotted time.";
				case IPStatus.BadRoute:
					return "The ICMP echo request failed because there is no valid route between the source and destination computers.";
				case IPStatus.TtlExpired:
					return "The ICMP echo request failed because its Time to Live (TTL) value reached zero, causing the forwarding node (router or gateway) to discard the packet.";
				case IPStatus.TtlReassemblyTimeExceeded:
					return "The ICMP echo request failed because the packet was divided into fragments for transmission and all of the fragments were not received within the time allotted for reassembly.";
				case IPStatus.ParameterProblem:
					return "The ICMP echo request failed because a node (router or gateway) encountered problems while processing the packet header.";
				case IPStatus.SourceQuench:
					return "The ICMP echo request failed because the packet was discarded. This occurs when the source computer's output queue has insufficient storage space, or when packets arrive at the destination too quickly to be processed.";
				case IPStatus.BadDestination:
					return "The ICMP echo request failed because the destination IP address cannot receive ICMP echo requests or should never appear in the destination address field of any IP datagram.";
				case IPStatus.DestinationUnreachable:
					return "The ICMP echo request failed because the destination computer that is specified in an ICMP echo message is not reachable; the exact cause of problem is unknown.";
				case IPStatus.TimeExceeded:
					return "The ICMP echo request failed because its Time to Live (TTL) value reached zero, causing the forwarding node (router or gateway) to discard the packet.";
				case IPStatus.BadHeader:
					return "The ICMP echo request failed because the header is invalid.";
				case IPStatus.UnrecognizedNextHeader:
					return "The ICMP echo request failed because the Next Header field does not contain a recognized value. The Next Header field indicates the extension header type (if present) or the protocol above the IP layer, for example, TCP or UDP.";
				case IPStatus.IcmpError:
					return "The ICMP echo request failed because of an ICMP protocol error.";
				case IPStatus.DestinationScopeMismatch:
					return "The ICMP echo request failed because the source address and destination address that are specified in an ICMP echo message are not in the same scope. This is typically caused by a router forwarding a packet using an interface that is outside the scope of the source address. Address scopes (link-local, site-local, and global scope) determine where on the network an address is valid.";
				default:
					throw new ArgumentException();
			};
		}
	}
}
