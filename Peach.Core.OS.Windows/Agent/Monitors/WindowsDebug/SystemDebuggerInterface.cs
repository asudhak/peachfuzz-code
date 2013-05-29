
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
//using Peach.Core.Debuggers.Windows;
using Peach.Core.Debuggers.WindowsSystem;
using NLog;
using System.Runtime.InteropServices;

namespace Peach.Core.Agent.Monitors.WindowsDebug
{
	public class SystemDebuggerInstance
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		public static bool ExitInstance = false;
		SystemDebugger _dbg = null;
		Thread _dbgThread = null;

		public string commandLine = null;
		public string processName = null;
		public string service = null;

		public string startOnCall = null;

		public bool ignoreFirstChanceGuardPage = false;
		public bool ignoreSecondChanceGuardPage = false;
		public bool noCpuKill = false;

		public bool dbgExited = false;
		public Fault crashInfo = null;

		ManualResetEvent _dbgCreated;
		Exception runException = null;
		
		public SystemDebuggerInstance()
		{
		}

		public int ProcessId
		{
			get { return (int)_dbg.dwProcessId; }
		}

		public bool caughtException
		{
			get
			{
				return crashInfo != null;
			}
		}

		public bool IsRunning
		{
			get
			{
				if (_dbg == null)
					return false;

				try
				{
					using (var p = System.Diagnostics.Process.GetProcessById(_dbg.dwProcessId))
					{
						if (p != null && !p.HasExited)
							return true;
					}
				}
				catch (ArgumentException)
				{
					return false;
				}
				catch (System.Runtime.InteropServices.COMException)
				{
					// Handle closed out from underneeth?
					return true;
				}

				dbgExited = true;
				StopDebugger();

				return false;
			}
		}

		public void StartDebugger()
		{
			if (_dbg != null || _dbgThread != null)
				FinishDebugging();

			_dbgCreated = new ManualResetEvent(false);
			runException = null;

			_dbgThread = new Thread(new ThreadStart(Run));
			_dbgThread.Start();

			// Wait for process to start up.
			_dbgCreated.WaitOne();

			if(_dbg == null)
			{
				System.Diagnostics.Debug.Assert(runException != null);
				var ex = runException;
				runException = null;
				throw new PeachException(ex.Message, ex);
			}

			_dbg.processStarted.WaitOne();
		}

		public void StopDebugger()
		{
			if (_dbg == null)
				return;

			_dbg.processExit = true;

			if(_dbgThread.IsAlive)
				_dbgThread.Join();

			// remember if we caught an exception
			var b = this.caughtException;

			dbgExited = true;
			_dbg.Close();
			_dbg = null;
			_dbgThread = null;
			_dbgCreated.Close();
			_dbgCreated = null;
		}

		public void FinishDebugging()
		{
			//if (_thread.IsAlive)
			StopDebugger();
			_dbg = null;
			crashInfo = null;

			//ExitInstance = true;
		}

		bool continueDebugging()
		{
			return !caughtException;
		}

		public void Run()
		{
			try
			{
				_dbg = null;

				if (commandLine != null)
				{
					_dbg = SystemDebugger.CreateProcess(commandLine);
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
						proc = System.Diagnostics.Process.GetProcessById(pid);

					if (proc == null)
						throw new Exception("Unable to locate process id from name \"" + processName + "\".");

					pid = proc.Id;

					proc.Dispose();

					_dbg = SystemDebugger.AttachToProcess(pid);
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

					_dbg = SystemDebugger.AttachToProcess(processId);
				}

				_dbgCreated.Set();
				_dbg.HandleAccessViolation = new HandleAccessViolation(HandleAccessViolation);
				_dbg.MainLoop();
			}
			catch (Exception ex)
			{
				logger.Error("Run(): Caught exception starting debugger: " + ex.ToString());
				runException = ex;
			}
			finally
			{
				try
				{
					_dbgCreated.Set();
				}
				catch
				{
				}
			}
		}

		public void HandleAccessViolation(UnsafeMethods.DEBUG_EVENT DebugEv)
		{
			bool handle = false;

			if (DebugEv.u.Exception.dwFirstChance == 1)
			{
				// Only some first chance exceptions are interesting

				if (DebugEv.u.Exception.ExceptionRecord.ExceptionCode == 0x80000001 ||
					DebugEv.u.Exception.ExceptionRecord.ExceptionCode == 0xC000001D)
				{
					handle = true;
				}

				if (DebugEv.u.Exception.ExceptionRecord.ExceptionCode == 0xC0000005)
				{
					// A/V on EIP || DEP
					if (DebugEv.u.Exception.ExceptionRecord.ExceptionInformation[0].ToInt64() == 0)
						handle = true;

					// write a/v not near null
					else if (DebugEv.u.Exception.ExceptionRecord.ExceptionInformation[0].ToInt64() == 1 &&
						DebugEv.u.Exception.ExceptionRecord.ExceptionInformation[1].ToInt64() != 0)
						handle = true;
				}

				// Skip uninteresting first chance
				if (handle == false)
					return;
			}

			Fault fault = new Fault();
			fault.type = FaultType.Fault;
			fault.detectionSource = "SystemDebugger";
			fault.title = "Exception: 0x" + DebugEv.u.Exception.ExceptionRecord.ExceptionCode.ToString("x8");

			StringBuilder output = new StringBuilder();

			if (DebugEv.u.Exception.dwFirstChance == 1)
				output.Append("First Chance ");

			output.AppendLine(fault.title);

			if (DebugEv.u.Exception.ExceptionRecord.ExceptionCode == 0xC0000005)
			{
				output.Append("Access Violation ");
				if (DebugEv.u.Exception.ExceptionRecord.ExceptionInformation[0].ToInt64() == 0)
					output.Append(" Reading From 0x");
				else
					output.Append(" Writing To 0x");
				output.Append(DebugEv.u.Exception.ExceptionRecord.ExceptionInformation[1].ToInt64().ToString("x16"));
			}

			fault.description = output.ToString();

			crashInfo = fault;
			_dbg.processExit = true;
		}
	}
}

// end
