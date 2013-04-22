
//
// Copyright (c) Michael Eddington
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in	
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

// Authors:
//   Michael Eddington (mike@dejavusecurity.com)

// $Id$

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Analyzers;
using Peach.Core.IO;
using Peach.Core.Agent;
using System.Collections;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;

namespace Peach.Core.Test.Monitors
{
	[Monitor("TestMonitor", true)]
	public class TestMonitor : Peach.Core.Agent.Monitor
	{
		private IPAddress _dest;
		private Socket _socket;
		private int port1;
		private int port2;

		public TestMonitor(IAgent agent, string name, Dictionary<string, Variant> args)
			: base(agent, name, args)
		{
			bool ret = IPAddress.TryParse((string)args["Address"], out _dest);
			System.Diagnostics.Debug.Assert(ret);
			port1 = int.Parse((string)args["Port1"]);
			port2 = int.Parse((string)args["Port2"]);
		}

		public override void StopMonitor()
		{
		}

		public override void SessionStarting()
		{
			_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.IP);
		}

		public override void SessionFinished()
		{
			if (_socket != null)
				_socket.Close();
		}

		public override void IterationStarting(uint iterationCount, bool isReproduction)
		{
			_socket.SendTo(Encoding.ASCII.GetBytes("Hello"), new IPEndPoint(_dest, port1));
			_socket.SendTo(Encoding.ASCII.GetBytes("Hello"), new IPEndPoint(_dest, port2));
			_socket.SendTo(Encoding.ASCII.GetBytes("Hello"), new IPEndPoint(_dest, port2));
			System.Threading.Thread.Sleep(1000);
		}

		public override bool IterationFinished()
		{
			return false;
		}

		public override bool DetectedFault()
		{
			return true;
		}

		public override Fault GetMonitorData()
		{
            return null;
		}

		public override bool MustStop()
		{
			return false;
		}

		public override Variant Message(string name, Variant data)
		{
			return null;
		}
	}

	[TestFixture]
	class PcapMonitorTests
	{
		List<string> testResults = new List<string>();

		// Return a tuple of interface name and broadcast IP for the first
		// interface that is up and has a valid IPv4 address.
		private static Tuple<string, IPAddress> GetInterface()
		{
			if (Platform.GetOS() == Platform.OS.Linux)
				return new Tuple<string, IPAddress>("lo", IPAddress.Loopback);

			if (Platform.GetOS() == Platform.OS.OSX)
				return new Tuple<string, IPAddress>("lo0", IPAddress.Loopback);

			NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
			foreach (NetworkInterface adapter in nics)
			{
				if (adapter.OperationalStatus != OperationalStatus.Up)
					continue;

				foreach (var addr in adapter.GetIPProperties().UnicastAddresses)
				{
					if (addr.Address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
						continue;

					byte[] raw = addr.Address.GetAddressBytes();
					byte[] mask = addr.IPv4Mask.GetAddressBytes();

					for (int i = 0; i < 4; ++i)
					{
						raw[i] |= (byte)~mask[i];
					}

					return new Tuple<string, IPAddress>(adapter.Name, new IPAddress(raw));
				}
			}

			throw new Exception("Couldn't find a valid network adapter");
		}

		private static string pre_xml = 
			"<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
			"<Peach>" +
			"	<DataModel name=\"TheDataModel\">" +
			"		<String value=\"Hello World\" />" +
			"	</DataModel>";

		private static string post_xml = 
			"	<StateModel name=\"TheState\" initialState=\"Initial\">" +
			"		<State name=\"Initial\">" +
			"			<Action type=\"output\">" +
			"				<DataModel ref=\"TheDataModel\"/>" +
			"			</Action>" +
			"		</State>" +
			"	</StateModel>" +
			"	" +
			"	<Test name=\"Default\" replayEnabled='false'>" +
			"		<Agent ref=\"LocalAgent\"/>" +
			"		<StateModel ref=\"TheState\"/>" +
			"		<Publisher class=\"Null\" />" +
			"		<Strategy class=\"RandomDeterministic\"/>" +
			"	</Test>" +
			"</Peach>";

		private void RunTest(string xml, uint iterations, Engine.FaultEventHandler OnFault)
		{
			var pid = System.Diagnostics.Process.GetCurrentProcess().Id;
			var rng = new Random((uint)pid);
			var iface = GetInterface();
			var port1 = rng.Next(8000, 10000);
			var port2 = rng.Next(100, 1000) + port1;
			xml = pre_xml + string.Format(xml, iface.Item1, iface.Item2, port1, port2) + post_xml;

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			config.range = true;
			config.rangeStart = 1;
			config.rangeStop = 1 + iterations;

			if (OnFault == null)
			{
				e.startFuzzing(dom, config);
				return;
			}

			e.Fault += OnFault;

			try
			{
				e.startFuzzing(dom, config);
				Assert.Fail("Should throw.");
			}
			catch (PeachException ex)
			{
				Assert.AreEqual("Fault detected on control iteration.", ex.Message);
			}

			Assert.AreEqual(1, testResults.Count);
			testResults.Clear();

			Assert.AreEqual(0, testResults.Count);
		}

		[Test]
		public void BasicTest()
		{
			string agent_xml =
				"	<Agent name=\"LocalAgent\">" +
				"		<Monitor class=\"Pcap\">" +
				"			<Param name=\"Device\" value=\"{0}\"/>" +
				"		</Monitor>" +
				"	</Agent>";

			RunTest(agent_xml, 1, null);
		}

		[Test]
		public void MultipleIterationsTest()
		{
			string agent_xml =
				"	<Agent name=\"LocalAgent\">" +
				"		<Monitor class=\"Pcap\">" +
				"			<Param name=\"Device\" value=\"{0}\"/>" +
				"		</Monitor>" +
				"	</Agent>";

			RunTest(agent_xml, 10, null);
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage="Error, PcapMonitor was unable to locate device 'Some Unknown Device'.")]
		public void BadDeviceTest()
		{
			string agent_xml =
				"	<Agent name=\"LocalAgent\">" +
				"		<Monitor class=\"Pcap\">" +
				"			<Param name=\"Device\" value=\"Some Unknown Device\"/>" +
				"		</Monitor>" +
				"	</Agent>";

			RunTest(agent_xml, 1, null);
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Error, PcapMonitor requires a device name.")]
		public void NoDeviceTest()
		{
			string agent_xml =
				"	<Agent name=\"LocalAgent\">" +
				"		<Monitor class=\"Pcap\">" +
				"		</Monitor>" +
				"	</Agent>";

			RunTest(agent_xml, 1, null);
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Error, PcapMonitor was unable to set the filter 'bad filter string'.")]
		public void BadFilterTest()
		{
			string agent_xml =
				"	<Agent name=\"LocalAgent\">" +
				"		<Monitor class=\"Pcap\">" +
				"			<Param name=\"Device\" value=\"{0}\"/>" +
				"			<Param name=\"Filter\" value=\"bad filter string\"/>" +
				"		</Monitor>" +
				"	</Agent>";

			RunTest(agent_xml, 1, null);
		}

		[Test]
		public void MultipleMonitorsTest()
		{
			string agent_xml =
				"	<Agent name=\"LocalAgent\">" +
				"		<Monitor class=\"Pcap\" name=\"Mon0\">" +
				"			<Param name=\"Device\" value=\"{0}\"/>" +
				"			<Param name=\"Filter\" value=\"ip src 255.255.255.255\"/>" +
				"		</Monitor>" +
				"		<Monitor class=\"Pcap\" name=\"Mon1\">" +
				"			<Param name=\"Device\" value=\"{0}\"/>" +
				"			<Param name=\"Filter\" value=\"udp port {2}\"/>" +
				"		</Monitor>" +
				"		<Monitor class=\"Pcap\" name=\"Mon2\">" +
				"			<Param name=\"Device\" value=\"{0}\"/>" +
				"			<Param name=\"Filter\" value=\"udp port {3}\"/>" +
				"		</Monitor>" +
				"		<Monitor class=\"TestMonitor\">" +
				"			<Param name=\"Address\" value=\"{1}\"/>" +
				"			<Param name=\"Port1\" value=\"{2}\"/>" +
				"			<Param name=\"Port2\" value=\"{3}\"/>" +
				"		</Monitor>" +
				"	</Agent>";

			RunTest(agent_xml, 1, new Engine.FaultEventHandler(_Fault));
		}

		void _Fault(RunContext context, uint currentIteration, Dom.StateModel stateModel, Fault[] faults)
		{
			Assert.AreEqual(3, faults.Length);

			Assert.AreEqual("Collected 0 packets.", faults[0].description);
			Assert.AreEqual(1, faults[0].collectedData.Keys.Count);
			Assert.True(faults[0].collectedData.ContainsKey("Mon0_NetworkCapture.pcap"));
			Assert.Greater(faults[0].collectedData["Mon0_NetworkCapture.pcap"].Length, 0);

			Assert.AreEqual("Collected 1 packets.", faults[1].description);
			Assert.AreEqual(1, faults[1].collectedData.Keys.Count);
			Assert.True(faults[1].collectedData.ContainsKey("Mon1_NetworkCapture.pcap"));
			Assert.Greater(faults[1].collectedData["Mon1_NetworkCapture.pcap"].Length, 0);

			Assert.AreEqual("Collected 2 packets.", faults[2].description);
			Assert.AreEqual(1, faults[2].collectedData.Keys.Count);
			Assert.True(faults[2].collectedData.ContainsKey("Mon2_NetworkCapture.pcap"));
			Assert.Greater(faults[2].collectedData["Mon2_NetworkCapture.pcap"].Length, 0);

			testResults.Add("Success");
		}
	}
}

// end
