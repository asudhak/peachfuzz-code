
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

		object mutex = new object();

		SystemDebugger _dbg = null;
		Thread _dbgThread = null;
		ManualResetEvent _dbgCreated;
		Exception _exception = null;

		public string commandLine = null;
		public string processName = null;
		public string service = null;
		public string startOnCall = null;

		public bool ignoreFirstChanceGuardPage = false;
		public bool ignoreSecondChanceGuardPage = false;
		public bool noCpuKill = false;

		public bool caughtException { get { return crashInfo != null; } }
		public Fault crashInfo = null;
		
		public SystemDebuggerInstance()
		{
		}

		public int ProcessId
		{
			get; private set;
		}

		public bool IsRunning
		{
			get
			{
				lock (mutex)
				{
					return _dbg != null;
				}
			}
		}

		public void StartDebugger()
		{
			FinishDebugging();

			System.Diagnostics.Debug.Assert(_dbg == null);
			System.Diagnostics.Debug.Assert(_dbgCreated == null);
			System.Diagnostics.Debug.Assert(_dbgThread == null);
			System.Diagnostics.Debug.Assert(_exception == null);
			System.Diagnostics.Debug.Assert(crashInfo == null);

			_dbgCreated = new ManualResetEvent(false);
			_dbgThread = new Thread(new ThreadStart(Run));
			_dbgThread.Start();

			// Wait for worker thread to start the process to up.
			_dbgCreated.WaitOne();

			if(_dbg == null)
			{
				_dbgThread.Join();
				_dbgThread = null;
				_dbgCreated.Close();
				_dbgThread = null;

				System.Diagnostics.Debug.Assert(_exception != null);
				var ex = _exception;

				StopDebugger();

				throw new PeachException(ex.Message, ex);
			}

			ProcessId = _dbg.ProcessId;
		}

		public void StopDebugger()
		{
			lock (mutex)
			{
				if (_dbg != null)
				{
					_dbg.TerminateProcess();
					_dbg = null;
				}
			}

			if (_dbgThread != null)
			{
				_dbgThread.Join();
				_dbgThread = null;
			}

			if (_dbgCreated != null)
			{
				_dbgCreated.Close();
				_dbgCreated = null;
			}

			_exception = null;
		}

		public void FinishDebugging()
		{
			StopDebugger();

			crashInfo = null;
		}

		void Run()
		{
			try
			{
				System.Diagnostics.Debug.Assert(_dbg == null);

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
						{
							srv.Start();

							try
							{
								srv.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
							}
							catch (Exception ex)
							{
								throw new PeachException("Timed out waiting for service '" + service + "' to start.", ex);
							}
						}

						using (ManagementObject manageService = new ManagementObject(@"Win32_service.Name='" + srv.ServiceName + "'"))
						{
							object o = manageService.GetPropertyValue("ProcessId");
							processId = (int)((UInt32)o);
						}
					}

					_dbg = SystemDebugger.AttachToProcess(processId);
				}

				_dbg.HandleAccessViolation = HandleAccessViolation;
				_dbg.ProcessCreated = ProcessCreated;
				_dbg.MainLoop();
			}
			catch (Exception ex)
			{
				logger.Error("Run(): Caught exception starting debugger: " + ex.ToString());
				_exception = ex;
			}
			finally
			{
				lock (mutex)
				{
					_dbg = null;
				}

				try
				{
					_dbgCreated.Set();
				}
				catch
				{
				}
			}
		}

		void ProcessCreated()
		{
			_dbgCreated.Set();
		}

		bool HandleAccessViolation(UnsafeMethods.DEBUG_EVENT DebugEv)
		{
			if (DebugEv.u.Exception.dwFirstChance == 1)
			{
				// Only some first chance exceptions are interesting
				bool handled = false;

				if (DebugEv.u.Exception.ExceptionRecord.ExceptionCode == 0x80000001 ||
					DebugEv.u.Exception.ExceptionRecord.ExceptionCode == 0xC000001D)
				{
					handled = true;
				}

				// http://msdn.microsoft.com/en-us/library/windows/desktop/aa363082(v=vs.85).aspx

				// Access violation
				if (DebugEv.u.Exception.ExceptionRecord.ExceptionCode == 0xC0000005)
				{
					// A/V on EIP
					if (DebugEv.u.Exception.ExceptionRecord.ExceptionInformation[0].ToInt64() == 0)
						handled = true;

					// write a/v not near null
					else if (DebugEv.u.Exception.ExceptionRecord.ExceptionInformation[0].ToInt64() == 1 &&
						DebugEv.u.Exception.ExceptionRecord.ExceptionInformation[1].ToInt64() != 0)
						handled = true;

					// DEP
					else if (DebugEv.u.Exception.ExceptionRecord.ExceptionInformation[0].ToInt64() == 8)
						handled = true;
				}

				// Skip uninteresting first chance
				if (!handled)
					return true;
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

			return false;
		}
	}
}

// end
