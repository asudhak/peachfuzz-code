
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

using Peach.Core.Dom;
using Peach.Core.Agent.Monitors;

namespace Peach.Core.Agent.Monitors.WindowsDebug
{
	/// <summary>
	/// DebuggerInstance will start up a debugger engine
	/// in a seprate thread and interface to it.
	/// </summary>
	/// <remarks>
	/// This instance interface can also be remoted. If
	/// needed.
	/// </remarks>
	public class DebuggerInstance : MarshalByRefObject
	{
		public static bool ExitInstance = false;
		public static DateTime LastHeartBeat = DateTime.MaxValue;
		public static DebuggerInstance Instance = null;
		Thread _thread = null;
		Debuggers.DebugEngine.WindowsDebugEngine _dbg = null;

		public string commandLine = null;
		public string processName = null;
		public string kernelConnectionString = null;
		public string service = null;

		public string symbolsPath = "SRV*http://msdl.microsoft.com/download/symbols";
		public string startOnCall = null;
		public string winDbgPath = null;

		public bool ignoreFirstChanceGuardPage = false;
		public bool ignoreSecondChanceGuardPage = false;
		public bool noCpuKill = false;

		public bool dbgExited = false;
		public bool caughtException = false;
		public Fault crashInfo = null;

		public DebuggerInstance()
		{
			Instance = this;
			LastHeartBeat = DateTime.Now;
		}

		public int ProcessId
		{
			get { return _dbg.processId; }
		}

		public bool IsRunning
		{
			get
			{
				LastHeartBeat = DateTime.Now;
				return _thread != null && _thread.IsAlive;
			}
		}

		public void StartDebugger()
		{
			LastHeartBeat = DateTime.Now;
			_thread = new Thread(new ThreadStart(Run));
			_thread.Start();

			while (_dbg == null && !dbgExited)
				Thread.Sleep(100);

			if (_dbg != null && !_dbg.loadModules.WaitOne())
				Console.Error.WriteLine("WaitOne == false");
		}

		public void StopDebugger()
		{
			LastHeartBeat = DateTime.Now;
			_dbg.exitDebugger.Set();

			for (int cnt = 0; _thread.IsAlive && cnt < 100; cnt++)
				Thread.Sleep(100);

			_thread.Abort();
			_thread.Join();

		}

		public void FinishDebugging()
		{
			LastHeartBeat = DateTime.Now;
			if (_thread.IsAlive)
				StopDebugger();

			ExitInstance = true;
		}

		public void Run()
		{
			using (_dbg = new Debuggers.DebugEngine.WindowsDebugEngine(winDbgPath))
			{
				_dbg.dbgSymbols.SetSymbolPath(symbolsPath);
				_dbg.skipFirstChanceGuardPageException = ignoreFirstChanceGuardPage;
				_dbg.skipSecondChangeGuardPageException = ignoreSecondChanceGuardPage;

				if (commandLine != null)
				{
					_dbg.CreateProcessAndAttach(commandLine);
				}
				else if (processName != null)
				{
					int pid = 0;
					System.Diagnostics.Process proc = null;
					var procs = System.Diagnostics.Process.GetProcessesByName(processName);
					if (procs != null && procs.Length > 0)
					{
						proc = procs[0];
						for (int i = 1; i < procs.Length; ++i)
							procs[i].Close();
					}

					if (proc == null && int.TryParse(processName, out pid))
						proc = System.Diagnostics.Process.GetProcessById(int.Parse(processName));

					if(proc == null)
						throw new Exception("Unable to locate process by \""+processName+"\".");

					pid = proc.Id;

					proc.Dispose();

					_dbg.AttachProcess(pid);
				}
				else if (kernelConnectionString != null)
				{
					_dbg.AttachKernel(kernelConnectionString);
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

					_dbg.AttachProcess(processId);
				}

				if (_dbg.handledException.WaitOne(0, false))
				{
					// Caught exception!
					caughtException = true;
					crashInfo = _dbg.crashInfo;
				}
			}

			dbgExited = true;
			_dbg = null;
		}
	}
}

// end
