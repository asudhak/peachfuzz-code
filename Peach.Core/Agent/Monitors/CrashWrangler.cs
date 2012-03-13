
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
using System.Threading;
using Peach.Core.Dom;

namespace Peach.Core.Agent.Monitors
{
	/// <summary>
	/// Monitor will use OS X's built in CrashReporter (similar to watson)
	/// to detect and report crashes.
	/// </summary>
	[Monitor("CrashWrangler")]
	[Monitor("osx.CrashWrangler")]
	[Parameter("Command", typeof(string), "Command to execute", true)]
	[Parameter("Arguments", typeof(string), "Commad line arguments", false)]
	[Parameter("StartOnCall", typeof(string), "Start command on state model call", false)]
	[Parameter("UseDebugMalloc", typeof(bool), "Use OS X Debug Malloc (slower) (defaults to false)", false)]
	[Parameter("ExecHandler", typeof(string), "Crash Wrangler Execution Handler program.", true)]
	[Parameter("ExploitableReads", typeof(bool), "Are read a/v's considered exploitable? (defaults to false)", false)]
	[Parameter("NoCpuKill", typeof(bool), "Disable process killing by CPU usage? (defaults to false)", false)]
	[Parameter("CwLogFile", typeof(string), "CrashWrangler Log file (defaults to cw.log)", false)]
	[Parameter("CwLockFile", typeof(string), "CrashWRangler Lock file (defaults to cw.lck)", false)]
	[Parameter("CwPidFile", typeof(string), "CrashWrangler PID file (defaults to cw.pid)", false)]
	public class CrashWrangler : Monitor
	{
		protected string command = null;
		protected string arguments = null;
		protected string startOnCall = null;
		protected bool useDebugMalloc = false;
		protected string execHandler = null;
		protected bool exploitableReads = false;
		protected bool noCpuKill = false;
		protected string cwLogFile = "cw.log";
		protected string cwLockFile = "cw.lck";
		protected string cwPidFile = "cw.pid";

		public CrashWrangler(string name, Dictionary<string, Variant> args)
			: base(name, args)
		{

		}

		public override void IterationStarting(int iterationCount, bool isReproduction)
		{
		}

		public override bool DetectedFault()
		{
			return false;
		}

		public override void GetMonitorData(System.Collections.Hashtable data)
		{
			//if (!DetectedFault())
			//    return;

			//data.Add("CrashReporter", data);
		}

		public override bool MustStop()
		{
			return false;
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

		public override bool IterationFinished()
		{
			return true;
		}

		public override Variant Message(string name, Variant data)
		{
			return null;
		}
	}
}

// end
