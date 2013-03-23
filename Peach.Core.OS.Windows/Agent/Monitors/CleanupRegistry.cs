
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceProcess;
using Microsoft.Win32;
using Peach.Core.Agent;
using NLog;

namespace Peach.Core.OS.Windows.Agent.Monitors
{
	[Monitor("CleanupRegistry", true)]
	[Parameter("Key", typeof(string), "Registry key to remove.")]
	[Parameter("ChildrenOnly", typeof(bool), "Only cleanup sub-keys. (defaults to false)", "false")]
	public class CleanupRegistry : Monitor
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		string _key = null;
		bool _childrenOnly = false;
		RegistryKey _root = null;

		public CleanupRegistry(IAgent agent, string name, Dictionary<string, Variant> args)
			: base(agent, name, args)
		{
			_key = (string)args["Key"];

			if (args.ContainsKey("ChildrenOnly") && ((string)args["ChildrenOnly"]).ToLower() == "true")
				_childrenOnly = true;

			if (_key.StartsWith("HKCU\\"))
				_root = Registry.CurrentUser;
			else if (_key.StartsWith("HKCC\\"))
				_root = Registry.CurrentConfig;
			else if (_key.StartsWith("HKLM\\"))
				_root = Registry.LocalMachine;
			else if (_key.StartsWith("HKPD\\"))
				_root = Registry.PerformanceData;
			else if (_key.StartsWith("HKU\\"))
				_root = Registry.Users;
			else
				throw new PeachException("Error, CleanupRegistry monitor Key parameter must be prefixed with HKCU, HKCC, HKLM, HKPD, or HKU.");

			_key = _key.Substring(_key.IndexOf("\\") + 1);
		}


		public override void StopMonitor()
		{
		}

		public override void SessionStarting()
		{
		}

		public override void SessionFinished()
		{
		}

		public override void IterationStarting(uint iterationCount, bool isReproduction)
		{
			if (!_childrenOnly)
			{
				logger.Debug("Removing key: " + _key);
				_root.DeleteSubKeyTree(_key, false);
				return;
			}

			var key = _root.OpenSubKey(_key, true);
			if (key == null)
				return;

			foreach (var subkey in key.GetSubKeyNames())
			{
				logger.Debug("Removing subkey: " + subkey);
				key.DeleteSubKeyTree(subkey, false);
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
}
