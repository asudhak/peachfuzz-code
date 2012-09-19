
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
using Peach.Core.Agent;

namespace Peach.Core.OS.Linux.Agent.Monitors
{
	[Monitor("LinuxCrashMonitor")]
	[Parameter("Executable", typeof(string), "Target executable used to filter crashes.", false)]
	[Parameter("LogFolder", typeof(string), "Folder with log files. Defaults to /var/peachcrash", false)]
	[Parameter("Mono", typeof(string), "Full path and executable for mono runtime. Defaults to /usr/bin/mono.", false)]
	public class LinuxCrashMonitor : Peach.Core.Agent.Monitor
	{
		protected string corePattern = "|{0} {1} -p=%p -u=%u -g=%g -s=%s -t=%t -h=%h -e=%e";
		protected string monoExecutable = "/usr/bin/mono";
		protected string executable = null;
		protected string logFolder = "/var/peachcrash";
		protected string origionalCorePattern = null;
		protected string linuxCrashHandlerExe = null;

		protected string data = null;
		protected List<string> startingFiles = new List<string>();

		bool alreadyPaused = false;

		public LinuxCrashMonitor(string name, Dictionary<string, Variant> args)
			: base(name, args)
		{
			if (args.ContainsKey("Executable"))
				executable = (string)args["Executable"];
			
			if (args.ContainsKey("LogFolder"))
				logFolder = (string)args["LogFolder"];
		}

		public override void  StopMonitor()
		{
			// Cleanup
			SessionFinished();
		}

		public override void  SessionStarting()
		{
			origionalCorePattern = File.ReadAllText("/proc/sys/kernel/core_pattern", Encoding.ASCII);

			if (origionalCorePattern.IndexOf(linuxCrashHandlerExe) != -1)
			{
				// Register our crash handler via proc file system

				var corePat = string.Format(corePattern,
					monoExecutable,
					linuxCrashHandlerExe);

				File.WriteAllText(
					"/proc/sys/kernel/core_pattern",
					corePat,
					Encoding.ASCII);

				var checkWrite = File.ReadAllText("/proc/sys/kernel/core_pattern", Encoding.ASCII);
				if (checkWrite.IndexOf(linuxCrashHandlerExe) > -1)
					throw new PeachException("Error, LinuxCrashMonitor was unable to update /proc/sys/kernel/core_pattern.");
			}
			else
				origionalCorePattern = null;

			Process p;
			ProcessStartInfo psi;

			if (Directory.Exists(logFolder))
			{
				// Clean up our folder

				psi = new ProcessStartInfo();
				psi.FileName = "/bin/rm";
				psi.Arguments = "-rf " + logFolder + "/*";
				psi.UseShellExecute = true;

				p = new Process();
				p.StartInfo = psi;
				p.Start();
				p.WaitForExit();
			}
			else
			{
				// Create our folder and set permissions

				psi = new ProcessStartInfo();
				psi.FileName = "/bin/mkdir";
				psi.Arguments = "-p " + logFolder;
				psi.UseShellExecute = true;

				p = new Process();
				p.StartInfo = psi;
				p.Start();
				p.WaitForExit();
			}
		}

		public override void  SessionFinished()
		{
			// only replace core_pattern if we updated it.
			if (origionalCorePattern != null)
			{
				File.WriteAllText("/proc/sys/kernel/core_pattern", origionalCorePattern, Encoding.ASCII);
			}

			if (Directory.Exists(logFolder))
			{
				// Remove folder

				var psi = new ProcessStartInfo();
				psi.FileName = "/bin/rm";
				psi.Arguments = "-rf " + logFolder;
				psi.UseShellExecute = true;

				var p = new Process();
				p.StartInfo = psi;
				p.Start();
				p.WaitForExit();
			}
		}

		public override void  IterationStarting(uint iterationCount, bool isReproduction)
		{
		}

		public override bool  IterationFinished()
		{
			return false; // !?
		}

		public override bool  DetectedFault()
		{
			foreach (var file in Directory.EnumerateFiles(logFolder))
			{
				if (executable != null)
				{
					if (file.IndexOf(executable) != -1)
					{
						return true;
					}
				}
				else
					return true;
			}

			return false;
		}

		public override void  GetMonitorData(System.Collections.Hashtable data)
		{
			foreach (var file in Directory.EnumerateFiles(logFolder))
			{
				if (executable != null)
				{
					if (file.IndexOf(executable) != -1)
					{
						data["LinuxCrashMonitor_" + Name] = File.ReadAllBytes(file);
						File.Delete(file);
						return;
					}
				}
				else
				{
					// Support multiple crash files
					data["LinuxCrashMonitor_" + Name + Path.GetFileNameWithoutExtension(file)] = File.ReadAllBytes(file);
					File.Delete(file);
				}
			}
		}

		public override bool  MustStop()
		{
			return false;
		}

		public override Variant Message(string name, Variant data)
		{
			return null;
		}
	}
}
