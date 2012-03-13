
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
	[Monitor("PcapMonitor")]
	[Monitor("network.PcapMonitor")]
	[Parameter("Device", typeof(string), "Device name for capturing on", true)]
	[Parameter("Filter", typeof(string), "PCAP Style filter", true)]
	public class PcapMonitor : Monitor
    {
		protected string _deviceName = null;
		protected string _filter = null;
		protected string _tmpFilename = null;
		protected ICaptureDevice _device = null;

		protected EventWaitHandle eventStartCapture = new EventWaitHandle(false, EventResetMode.ManualReset);
		protected EventWaitHandle eventStopCapture = new EventWaitHandle(false, EventResetMode.ManualReset);
		protected EventWaitHandle eventResetCapture = new EventWaitHandle(false, EventResetMode.ManualReset);

		public PcapMonitor(string name, Dictionary<string, Variant> args)
			: base(name, args)
		{
			if (args.ContainsKey("Device"))
				_deviceName = (string)args["Device"];
			if (args.ContainsKey("Filter"))
				_filter = (string)args["Filter"];

			_device.OnPacketArrival += new PacketArrivalEventHandler(device_OnPacketArrival);

			_tmpFilename = Path.GetTempFileName();
		}

		private static void device_OnPacketArrival(object sender, CaptureEventArgs packet)
		{
			ICaptureDevice device = packet.Device;

			// if device has a dump file opened
			if (device.DumpOpened)
			{
				// dump the packet to the file
				device.Dump(packet.Packet);
			}
		}

		public override void StopMonitor()
		{
			_device.StopCapture();
			_device.DumpClose();
			_device.Close();
		}

		public override void SessionStarting()
		{
			// Retrieve all capture devices
			var devices = CaptureDeviceList.Instance;

			// differentiate based upon types
			foreach(ICaptureDevice dev in devices)
			{
				if (dev.Name == _deviceName)
				{
					_device = dev;
					break;
				}
			}

			if (_device == null)
			{
				Console.WriteLine("Found the following pcap devices: ");

				foreach (ICaptureDevice dev in devices)
				{
					Console.WriteLine(dev.Name);
				}

				throw new PeachException("Error, PcapMonitor was unable to locate device '" + _deviceName + "'.");
			}

			_device.Open();
			_device.Filter = _filter;
			_device.StartCapture();
		}

		public override void SessionFinished()
		{
			_device.StopCapture();
			_device.Close();
		}

		public override void IterationStarting(int iterationCount, bool isReproduction)
		{
			// Clear old log
			if (_device.DumpOpened)
				_device.DumpClose();

			_device.DumpOpen(_tmpFilename);
		}

		public override bool IterationFinished()
		{
			// Save log
			_device.DumpFlush();
			_device.DumpClose();

			return false;
		}

		public override bool DetectedFault()
		{
			return false;
		}

		public override void GetMonitorData(Hashtable data)
		{
			// Return log
			byte[] buff;
			using (Stream sin = File.OpenRead(_tmpFilename))
			{
				buff = new byte[sin.Length];
				sin.Read(buff, 0, buff.Length);
			}

			var ret = new Hashtable();
			ret["NetworkCapture.pcap"] = buff;

			data[Name + "_NetworkCapture"] = ret;
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
