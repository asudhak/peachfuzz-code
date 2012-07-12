
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
using System.ServiceProcess;
using System.Management;

//using Peach.Core.Debuggers.WindowsSystem;
using Peach.Core.Debuggers.Windows;

namespace Peach.Core.Agent.Monitors.WindowsDebug
{
	public class SystemDebuggerInstance
	{
		public static bool ExitInstance = false;
		SystemDebugger _dbg = null;

		public string commandLine = null;
		public string processName = null;
		public string service = null;

		public string startOnCall = null;

		public bool ignoreFirstChanceGuardPage = false;
		public bool ignoreSecondChanceGuardPage = false;
		public bool noCpuKill = false;

		public bool dbgExited = false;
		public bool _caughtException = false;
		public Dictionary<string, Variant> crashInfo = null;

		public SystemDebuggerInstance()
		{
		}

		public int ProcessId
		{
			get { return (int)_dbg.dwProcessId(); }
		}

		public bool caughtException
		{
			get
			{
				if (_caughtException)
					return true;

				if (_dbg != null && _dbg.HasAccessViolation())
				{
					_caughtException = true;
					crashInfo = new Dictionary<string, Variant>();
					crashInfo["SystemDebugger_Infoz.txt"] = new Variant("Unknown Access Violation!");

					return true;
				}

				return false;
			}
		}

		public bool IsRunning
		{
			get
			{
				if (_dbg == null)
					return false;

				foreach(System.Diagnostics.Process process in System.Diagnostics.Process.GetProcesses())
				{
					if (process.Id == (int)_dbg.dwProcessId())
						return true;
				}

				if (_dbg.HasAccessViolation())
				{
					_caughtException = true;
					crashInfo = new Dictionary<string, Variant>();
					crashInfo["SystemDebugger_Infoz.txt"] = new Variant("Unknown Access Violation!");
				}

				dbgExited = true;
				StopDebugger();

				return false;
			}
		}

		public void StartDebugger()
		{
			if (_dbg != null)
				FinishDebugging();

			Run();
		}

		public void StopDebugger()
		{
			if (_dbg == null)
				return;

			_dbg.StopDebugger();

			// remember if we caught an exception
			var b = this.caughtException;

			dbgExited = true;
			_dbg = null;
		}

		public void FinishDebugging()
		{
			//if (_thread.IsAlive)
			StopDebugger();
			_dbg = null;

			//ExitInstance = true;
		}

		bool continueDebugging()
		{
			return !caughtException;
		}

		public void Run()
		{
			_dbg = new SystemDebugger();

			if (commandLine != null)
			{
				_dbg.CreateProcessW(commandLine);
			}
			else if (processName != null)
			{
				int pid = 0;
				System.Diagnostics.Process proc = null;
				var procs = System.Diagnostics.Process.GetProcessesByName(processName);
				if (procs != null && procs.Length > 0)
					proc = procs[0];

				if (proc == null && int.TryParse(processName, out pid))
					proc = System.Diagnostics.Process.GetProcessById(int.Parse(processName));

				if (proc == null)
					throw new Exception("Unable to locate process by \"" + processName + "\".");

				pid = proc.Id;

				proc.Dispose();

				_dbg.AttachToProcess(pid);
			}
			else if (service != null)
			{
				int processId = 0;

				using (ServiceController srv = new ServiceController(service))
				{
					if (srv.Status == ServiceControllerStatus.Stopped)
						srv.Start();

					using (ManagementObject manageService = new ManagementObject(@"Win32_service.Name='" + srv.ServiceName + "'"))
					{
						object o = manageService.GetPropertyValue("ProcessId");
						processId = (int)((UInt32)o);
					}
				}

				_dbg.AttachToProcess(processId);
			}
		}
	}
}

// end
