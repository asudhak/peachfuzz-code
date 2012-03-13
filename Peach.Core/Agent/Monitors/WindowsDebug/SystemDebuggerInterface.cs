
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
using PeachCoreDebuggersWindows;

namespace Peach.Core.Agent.Monitors.WindowsDebug
{
	public class SystemDebuggerInstance
	{
		public static bool ExitInstance = false;
		Thread _thread = null;
		SystemDebugger _dbg = null;

		public string commandLine = null;
		public string processName = null;
		public string service = null;

		public string startOnCall = null;

		public bool ignoreFirstChanceGuardPage = false;
		public bool ignoreSecondChanceGuardPage = false;
		public bool noCpuKill = false;

		public bool dbgExited = false;
		public bool caughtException = false;
		public Dictionary<string, Variant> crashInfo = null;

		int processId = 0;

		public SystemDebuggerInstance()
		{
		}

		public int ProcessId
		{
			get { return (int)_dbg.dwProcessId(); }
		}

		public bool IsRunning
		{
			get { return true; }
		}

		public void StartDebugger()
		{
			if (_dbg != null)
				FinishDebugging();

			//_thread = new Thread(new ThreadStart(Run));
			//_thread.Start();

			//while (_dbg == null && !dbgExited)
			//    Thread.Sleep(100);
			Run();
		}

		public void StopDebugger()
		{
			if (_dbg == null)
				return;

			//_dbg.processExit = true;
			_dbg.StopDebugger();

			//for (int cnt = 0; _thread.IsAlive && cnt < 100; cnt++)
			//    Thread.Sleep(100);

			//_thread.Abort();
			//_thread.Join();

			//uint threadId = UnsafeMethods.GetCurrentThread();
			//UnsafeMethods.TerminateProcess(processId, 0);
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

		//void handleAccessViolation(UnsafeMethods.DEBUG_EVENT debugEvent)
		//{
		//    try
		//    {
		//        string message = "";
		//        var Exception = debugEvent.u.Exception;

		//        if (Exception.dwFirstChance == 1)
		//        {
		//            bool handle = false;

		//            if (ignoreFirstChanceGuardPage && Exception.ExceptionRecord.ExceptionCode == 0x80000001)
		//                return;

		//            // Guard page or illegal op
		//            if (Exception.ExceptionRecord.ExceptionCode == 0x80000001 || Exception.ExceptionRecord.ExceptionCode == 0xC000001D)
		//                handle = true;

		//            if (Exception.ExceptionRecord.ExceptionCode == 0xC0000005)
		//            {
		//                // A/V on EIP || DEP
		//                if (Exception.ExceptionRecord.ExceptionInformation[0] == 0)
		//                    handle = true;

		//                // write a/v not near null
		//                else if (Exception.ExceptionRecord.ExceptionInformation[0] == 1 &&
		//                    Exception.ExceptionRecord.ExceptionInformation[1] != 0)
		//                    handle = true;
		//            }

		//            // Skip uninteresting first chance
		//            if (!handle)
		//                return;
		//        }

		//        if (ignoreSecondChanceGuardPage && Exception.dwFirstChance == 0 &&
		//            Exception.ExceptionRecord.ExceptionCode == 0x80000001)
		//        {
		//            return;
		//        }

		//        // Guard page or illegal op
		//        if (Exception.ExceptionRecord.ExceptionCode == 0x80000001 || Exception.ExceptionRecord.ExceptionCode == 0xC000001D)
		//            message = "Guard page or illegal operation";

		//        if (Exception.ExceptionRecord.ExceptionCode == 0xC0000005)
		//        {
		//            // A/V on EIP || DEP
		//            if (Exception.ExceptionRecord.ExceptionInformation[0] == 0)
		//                message = "A/V on EIP or DEP";

		//            // write a/v not near null
		//            else if (Exception.ExceptionRecord.ExceptionInformation[0] == 1 &&
		//                Exception.ExceptionRecord.ExceptionInformation[1] != 0)
		//                message = "Write A/V not at null";
		//        }

		//        message = "Unknown Access Violation!";

		//        caughtException = true;
		//        crashInfo = new Dictionary<string, Variant>();
		//        crashInfo["SystemDebugger_Infoz.txt"] = new Variant(message);
		//    }
		//    catch
		//    {
		//        string a = "a";
		//    }
		//}

		public void Run()
		{
			_dbg = new SystemDebugger();

			if (commandLine != null)
			{
				_dbg.CreateProcessW(commandLine);
			}
			//else if (processName != null)
			//{
			//    int pid = 0;
			//    System.Diagnostics.Process proc = null;
			//    var procs = System.Diagnostics.Process.GetProcessesByName(processName);
			//    if (procs != null && procs.Length > 0)
			//        proc = procs[0];

			//    if (proc == null && int.TryParse(processName, out pid))
			//        proc = System.Diagnostics.Process.GetProcessById(int.Parse(processName));

			//    if (proc == null)
			//        throw new Exception("Unable to locate process by \"" + processName + "\".");

			//    pid = proc.Id;

			//    proc.Dispose();

			//    _dbg = SystemDebugger.AttachToProcess(pid);
			//}
			//else if (service != null)
			//{
			//    processId = 0;

			//    using (ServiceController srv = new ServiceController(service))
			//    {
			//        if (srv.Status == ServiceControllerStatus.Stopped)
			//            srv.Start();

			//        using (ManagementObject manageService = new ManagementObject(@"Win32_service.Name='" + srv.ServiceName + "'"))
			//        {
			//            object o = manageService.GetPropertyValue("ProcessId");
			//            processId = (int)((UInt32)o);
			//        }
			//    }

			//    _dbg = SystemDebugger.AttachToProcess(processId);
			//}

			//processId = _dbg.dwProcessId;
			//_dbg.ContinueDebugging = new ContinueDebugging(continueDebugging);
			//_dbg.HandleAccessViolation = new HandleAccessViolation(handleAccessViolation);
			//_dbg.MainLoop();

			//UnsafeMethods.TerminateProcess(processId, 0);

			//dbgExited = true;
			//_dbg = null;
		}
	}
}

// end
