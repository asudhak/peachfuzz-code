using System;
using System.Net;
using System.IO;
using System.IO.Ports;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace Peach.Core.Agent.Monitors
{
	/// <summary>
	/// Cana Kit 4-port usb relay board.  This is a low cost board with
	/// 4 relays that can be controlled over USB. By default this monitor will turn off then on
	/// a port of your choice when a fault is detected.  Optionally you can have
	/// the on/off occur before every iteration.
	/// </summary>
	/// <remarks>
	/// This monitor is used for embedded device fuzzing when you want to 
	/// turn power or signal on/off while fuzzing.
	/// 
	/// http://www.canakit.com/4-port-usb-relay-controller.html
	/// </remarks>
	[Monitor("CanaKitRelay", true)]
	[Parameter("SerialPort", typeof(string), "Serial port for board (e.g. COM2)")]
	[Parameter("RelayNumber", typeof(int), "Which realy to trigger (1..4)")]
	[Parameter("ResetEveryIteration", typeof(bool), "Reset power on every iteration (default is false)", "false")]
	[Parameter("OnOffPause", typeof(int), "Pause in milliseconds between off/on (default is 1/2 second)", "500")]
	[Parameter("ResetOnStart", typeof(bool), "Reset device on start? (defaults to false)", "false")]
	[Parameter("ReverseSwitch", typeof(bool), "Switches the order of the on off commands for when the NC port is used for power", "false")]
	public class CanaKitRelayMonitor : Monitor
	{
		public string SerialPort { get; private set; }
		public int RelayNumber { get; protected set; }
		public int OnOffPause { get; private set; }
		public bool ResetEveryIteration { get; private set; }
		public bool ResetOnStart { get; private set; }
		public bool ReverseSwitch { get; private set; }

		public CanaKitRelayMonitor(IAgent agent, string name, Dictionary<string, Variant> args)
			: base(agent, name, args)
		{
			ParameterParser.Parse(this, args);
		}

		void resetPower(bool turnOff = true)
		{
			using (var serial = new SerialPort(SerialPort, 115200, Parity.None, 8, StopBits.One))
			{
				serial.Open();
				if (ReverseSwitch)
				{
					if (turnOff)
					{
						serial.Write("REL" + RelayNumber + ".ON\r\n");
						System.Threading.Thread.Sleep(OnOffPause);
					}

					serial.Write("\r\nREL" + RelayNumber + ".OFF\r\n");
				}
				else
				{
					if (turnOff)
					{
						serial.Write("\r\nREL" + RelayNumber + ".OFF\r\n");
						System.Threading.Thread.Sleep(OnOffPause);
					}

					serial.Write("REL" + RelayNumber + ".ON\r\n");
				}
			}
		}

		public override void StopMonitor()
		{
		}

		public override void SessionStarting()
		{
			if (!ResetOnStart)
			{
				resetPower(false);
				System.Threading.Thread.Sleep(250);
				resetPower(false);
			}
			else
			{
				resetPower(false);
				System.Threading.Thread.Sleep(250);
				resetPower();
			}
		}

		public override void SessionFinished()
		{
		}

		public override void IterationStarting(uint iterationCount, bool isReproduction)
		{
			if (ResetEveryIteration)
			{
				resetPower();
			}
		}

		public override bool IterationFinished()
		{
			return false;
		}

		public override bool DetectedFault()
		{
			return false;
		}

		public override Fault GetMonitorData()
		{
			// This indicates a fault was detected and we
			// should reset a port.

			if (!ResetEveryIteration)
				resetPower();

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

		public override object ProcessQueryMonitors(string query)
		{
			if (query == "CanaKitRelay_Reset")
				resetPower();

			return true;
		}
	}
}
