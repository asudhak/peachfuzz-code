
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
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Peach.Core.Dom;
using SharpPcap;
using SharpPcap.LibPcap;
using SharpPcap.WinPcap;
using SharpPcap.AirPcap;

namespace Peach.Core.Agent.Monitors
{
	[Monitor("Pcap", true)]
	[Monitor("network.PcapMonitor")]
	[Parameter("Device", typeof(string), "Device name for capturing on")]
	[Parameter("Filter", typeof(string), "PCAP Style filter", "")]
	public class PcapMonitor : Monitor
	{
		protected string _deviceName;
		protected string _filter = "";
		protected string _tempFileName = Path.GetTempFileName();
		protected object _lock = new object();
		protected int _numPackets = 0;
		protected LibPcapLiveDevice _device = null;
		protected CaptureFileWriterDevice _writer = null;

		public PcapMonitor(IAgent agent, string name, Dictionary<string, Variant> args)
			: base(agent, name, args)
		{
			if (args.ContainsKey("Device"))
				_deviceName = (string)args["Device"];
			if (args.ContainsKey("Filter"))
				_filter = (string)args["Filter"];
		}

		private void _OnPacketArrival(object sender, CaptureEventArgs packet)
		{
			lock (_lock)
			{
				// _writer can be null if a packet arrives before the 1st iteration
				if (_writer != null && _writer.Opened)
				{
					_writer.Write(packet.Packet);
					_numPackets += 1;
				}
			}
		}

		private CaptureDeviceList _GetDeviceList()
		{
			try
			{
				return CaptureDeviceList.New();
			}
			catch (DllNotFoundException ex)
			{
				throw new PeachException("Error, PcapMonitor was unable to get the device list.  Ensure libpcap is installed and try again.", ex);
			}
		}

		public override void StopMonitor()
		{
			if (File.Exists(_tempFileName))
				File.Delete(_tempFileName);
		}

		public override void SessionStarting()
		{
			if (_deviceName == null)
				throw new PeachException("Error, PcapMonitor requires a device name.");

			// Retrieve all capture devices
			// Don't use the singlton interface so we can support multiple
			// captures on the same device with different filters
			var devices = _GetDeviceList();

			if (devices.Count == 0)
				throw new PeachException("No pcap devices found. Ensure appropriate permissions for using libpcap.");

			// differentiate based upon types
			foreach (var item in devices)
			{
				var dev = item as LibPcapLiveDevice;
				System.Diagnostics.Debug.Assert(dev != null);
				if (dev.Interface.FriendlyName == _deviceName)
				{
					_device = dev;
					break;
				}
			}

			if (_device == null)
			{
				Console.WriteLine("Found the following pcap devices: ");
				foreach (var item in devices)
				{
					var dev = item as LibPcapLiveDevice;
					System.Diagnostics.Debug.Assert(dev != null);
					if (dev.Interface.FriendlyName != null && dev.Interface.FriendlyName.Length > 0)
						Console.WriteLine(" " + dev.Interface.FriendlyName);
				}
				throw new PeachException("Error, PcapMonitor was unable to locate device '" + _deviceName + "'.");
			}

			_device.OnPacketArrival += new PacketArrivalEventHandler(_OnPacketArrival);
			_device.Open();

			try
			{
				_device.Filter = _filter;
			}
			catch (PcapException ex)
			{
				throw new PeachException("Error, PcapMonitor was unable to set the filter '" + _filter + "'.", ex);
			}

			_device.StartCapture();
		}

		public override void SessionFinished()
		{
			if (_device != null)
			{
				_device.StopCapture();
				_device.Close();

				IterationFinished();
			}
		}

		public override void IterationStarting(uint iterationCount, bool isReproduction)
		{
			lock (_lock)
			{
				_writer = new CaptureFileWriterDevice(_device, _tempFileName);
				_numPackets = 0;
			}
		}

		public override bool IterationFinished()
		{
			lock (_lock)
			{
				if (_writer != null)
				{
					_writer.Close();
				}
			}
			return false;
		}

		public override bool DetectedFault()
		{
			return false;
		}

		public override Fault GetMonitorData()
		{
			Fault fault = new Fault();

			fault.detectionSource = "PcapMonitor";
			fault.folderName = "PcapMonitor";
			fault.type = FaultType.Data;
			fault.description = "Collected " + _numPackets + " packets.";
			fault.collectedData[this.Name + "_NetworkCapture.pcap"] = File.ReadAllBytes(_writer.Name);

			return fault;
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
}

// end
