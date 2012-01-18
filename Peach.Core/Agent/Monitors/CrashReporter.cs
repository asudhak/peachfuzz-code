
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
//   Michael Eddington (mike@phed.org)

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
	[Monitor("CrashReporter")]
	[Monitor("osx.CrashReporter")]
	[Parameter("ProcessName", typeof(string), "Process name to watch for (defaults to all)", false)]
	[Parameter("LogFolder", typeof(string), "Folder with log files (defaults to current users crash folder)", false)]
	public class CrashReporter : Monitor
	{
		protected string processName = null;
		protected string logFolder = null;
		protected bool crashWrangler = false;

		protected string data = null;
		protected List<string> startingFiles = new List<string>();

		bool alreadyPaused = false;

		public CrashReporter(string name, Dictionary<string, Variant> args)
			: base(name, args)
		{
			if (args.ContainsKey("ProcessName"))
				processName = (string)args["ProcessName"];
			
			if (args.ContainsKey("LogFolder"))
				logFolder = (string)args["LogFolder"];
			else
				logFolder = Path.Combine(System.Environment.GetEnvironmentVariable("HOME"), "Library", "Logs", "CrashReporter");
		}

		public override void IterationStarting(int iterationCount, bool isReproduction)
		{
			alreadyPaused = false;

			foreach (string file in Directory.EnumerateFiles(logFolder))
				startingFiles.Add(file);
		}

		public override bool DetectedFault()
		{
			// Method will get called multiple times
			// we only want to pause the first time.
			if (!alreadyPaused)
			{
				alreadyPaused = true;
				// Wait for CrashReporter to report!
				Thread.Sleep(500);
			}

			foreach (string file in Directory.EnumerateFiles(logFolder))
			{
				if (!startingFiles.Contains(file))
				{
					data = File.ReadAllText(file);
					return true;
				}
			}

			return false;
		}

		public override void GetMonitorData(System.Collections.Hashtable data)
		{
			if (!DetectedFault())
				return;

			data.Add("CrashReporter", data);
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
