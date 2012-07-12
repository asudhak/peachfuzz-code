
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
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace Peach.Core.Agent.Monitors
{
	[Monitor("PageHeap")]
	[Parameter("Executable", typeof(string), "Name of executable to enable (NO PATH)", true)]
	[Parameter("WinDbgPath", typeof(string), "Path to WinDbg install.  If not provided we will try and locate it.", false)]
	public class PageHeap : Monitor
	{
		string _executable = null;
		string _winDbgPath = null;
		string _gflags = "gflags.exe";
		string _gflagsArgsEnable = "/p /enable \"{0}\" /full";
		string _gflagsArgsDisable = "/p /disable \"{0}\"";

		public PageHeap(string name, Dictionary<string, Variant> args)
			: base(name, args)
		{
			_executable = (string)args["Executable"];
			
			if(args.ContainsKey("WinDbgPath"))
				_winDbgPath = (string)args["WinDbgPath"];
			else
			{
				_winDbgPath = FindWinDbg();
				if (_winDbgPath == null)
					throw new PeachException("Error, unable to locate WinDbg, please specify using 'WinDbgPath' parameter.");
			}
		}

		protected string FindWinDbg()
		{
			// Lets try a few common places before failing.
			List<string> pgPaths = new List<string>();
			pgPaths.Add(@"c:\");
			pgPaths.Add(Environment.GetEnvironmentVariable("SystemDrive"));
			pgPaths.Add(Environment.GetEnvironmentVariable("ProgramFiles"));

			if (Environment.GetEnvironmentVariable("ProgramW6432") != null)
				pgPaths.Add(Environment.GetEnvironmentVariable("ProgramW6432"));

			if (Environment.GetEnvironmentVariable("ProgramFiles(x86)") != null)
				pgPaths.Add(Environment.GetEnvironmentVariable("ProgramFiles(x86)"));

			List<string> dbgPaths = new List<string>();
			dbgPaths.Add("Debuggers");
			dbgPaths.Add("Debugger");
			dbgPaths.Add("Debugging Tools for Windows");
			dbgPaths.Add("Debugging Tools for Windows (x64)");
			dbgPaths.Add("Debugging Tools for Windows (x86)");

			foreach (string path in pgPaths)
			{
				foreach (string dpath in dbgPaths)
				{
					if (File.Exists(Path.Combine(path, dpath)))
					{
						return Path.Combine(path, dpath);
					}
				}
			}

			return null;
		}

		protected void Enable()
		{
			var startInfo = new ProcessStartInfo();
			startInfo.FileName = Path.Combine(_winDbgPath, _gflags);
			startInfo.Arguments = string.Format(_gflagsArgsEnable, _executable);
			startInfo.CreateNoWindow = true;
			startInfo.UseShellExecute = false;
			System.Diagnostics.Process.Start(startInfo).WaitForExit();
		}

		protected void Disable()
		{
			var startInfo = new ProcessStartInfo();
			startInfo.FileName = Path.Combine(_winDbgPath, _gflags);
			startInfo.Arguments = string.Format(_gflagsArgsDisable, _executable);
			startInfo.CreateNoWindow = true;
			startInfo.UseShellExecute = false;
			System.Diagnostics.Process.Start(startInfo).WaitForExit();
		}

		public override void StopMonitor()
		{
			Disable();
		}

		public override void SessionStarting()
		{
			Enable();
		}

		public override void SessionFinished()
		{
			Disable();
		}

		public override bool DetectedFault()
		{
			return false;
		}


		public override void IterationStarting(int iterationCount, bool isReproduction)
		{
		}

		public override bool IterationFinished()
		{
			return false;
		}

		public override void GetMonitorData(System.Collections.Hashtable data)
		{
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

