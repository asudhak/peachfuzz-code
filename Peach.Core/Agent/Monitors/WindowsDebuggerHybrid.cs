
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
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;

using Peach.Core.Dom;
using Peach.Core.Agent.Monitors.WindowsDebug;

namespace Peach.Core.Agent.Monitors
{
	[Monitor("WindowsDebuggerHybrid")]
	[Parameter("CommandLine", typeof(string), "Command line of program to start.", false)]
	[Parameter("ProcessName", typeof(string), "Name of process to attach too.", false)]
	[Parameter("KernelConnectionString", typeof(string), "Connection string for kernel debugging.", false)]
	[Parameter("Service", typeof(string), "Name of Windows Service to attach to.  Service will be started if stopped or crashes.", false)]
	[Parameter("SymbolsPath", typeof(string), "Optional Symbol path.  Default is Microsoft public symbols server.", false)]
	[Parameter("WinDbgPath", typeof(string), "Path to WinDbg install.  If not provided we will try and locate it.", false)]
	[Parameter("StartOnCall", typeof(string), "Indicate the debugger should wait to start or attach to process until notified by state machine.", false)]
	[Parameter("IgnoreFirstChanceGuardPage", typeof(string), "Ignore first chance guard page faults.  These are sometimes false posistives or anti-debugging faults.", false)]
	[Parameter("IgnoreSecondChanceGuardPage", typeof(string), "Ignore second chance guard page faults.  These are sometimes false posistives or anti-debugging faults.", false)]
	[Parameter("NoCpuKill", typeof(string), "Don't use process CPU usage to terminate early.", false)]
	public class WindowsDebuggerHybrid : Monitor
	{
		string _name = null;
		static bool _firstIteration = true;
		string _commandLine = null;
		string _processName = null;
		string _kernelConnectionString = null;
		string _service = null;

		string _winDbgPath = null;
		string _symbolsPath = "SRV*http://msdl.microsoft.com/download/symbols";
		string _startOnCall = null;

		bool _ignoreFirstChanceGuardPage = false;
		bool _ignoreSecondChanceGuardPage = false;
		bool _noCpuKill = false;

		bool _hybrid = true;
		bool _replay = false;

		DebuggerInstance _debugger = null;
		SystemDebuggerInstance _systemDebugger = null;
		IpcChannel _ipcChannel = null;

		public WindowsDebuggerHybrid(string name, Dictionary<string, Variant> args)
			: base(name, args)
		{
			_name = name;

			if (args.ContainsKey("CommandLine"))
				_commandLine = (string)args["CommandLine"];
			else if (args.ContainsKey("ProcessName"))
				_processName = (string)args["ProcessName"];
			else if (args.ContainsKey("KernelConnectionString"))
			{
				_hybrid = false;
				_kernelConnectionString = (string)args["KernelConnectionString"];
			}
			else if (args.ContainsKey("Service"))
				_service = (string)args["Service"];
			else
				throw new PeachException("Error, WindowsDebugEngine started with out a CommandLine, ProcessName, KernelConnectionString or Service parameter.");

			if (args.ContainsKey("SymbolsPath"))
				_symbolsPath = (string)args["SymbolsPath"];
			if (args.ContainsKey("StartOnCall"))
				_startOnCall = (string)args["StartOnCall"];
			if (args.ContainsKey("WinDbgPath"))
				_winDbgPath = (string)args["WinDbgPath"];
			else
			{
				_winDbgPath = FindWinDbg();
				if (_winDbgPath == null)
					throw new PeachException("Error, unable to locate WinDbg, please specify using 'WinDbgPath' parameter.");
			}

			if (args.ContainsKey("IgnoreFirstChanceGuardPage") && ((string)args["IgnoreFirstChanceGuardPage"]).ToLower() == "true")
				_ignoreFirstChanceGuardPage = true;
			if (args.ContainsKey("IgnoreSecondChanceGuardPage") && ((string)args["IgnoreSecondChanceGuardPage"]).ToLower() == "true")
				_ignoreSecondChanceGuardPage = true;
			if (args.ContainsKey("NoCpuKill") && ((string)args["NoCpuKill"]).ToLower() == "true")
				_noCpuKill = true;

			// Register IPC Channel for connecting to debug process
			_ipcChannel = new IpcChannel("Peach.Core_" + (new Random().Next().ToString()));
			ChannelServices.RegisterChannel(_ipcChannel, false);
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

		PerformanceCounter _performanceCounter = null;
		public float GetProcessCpuUsage(System.Diagnostics.Process proc)
		{
			try
			{
				if (_performanceCounter == null)
				{
					_performanceCounter = new PerformanceCounter("Process", "% Processor Time", proc.ProcessName);
					_performanceCounter.NextValue();
					if (_firstIteration)
					{
						_firstIteration = false;
						System.Threading.Thread.Sleep(1000);
					}
					else
					{
						System.Threading.Thread.Sleep(100);
					}
				}

				return _performanceCounter.NextValue();
			}
			catch
			{
				return 100;
			}
		}

		public override Variant Message(string name, Variant data)
		{
			if (name == "Action.Call" && ((string)data) == _startOnCall)
			{
				_StartDebugger();
				return null;
			}

			if (name == "Action.Call.IsRunning" && ((string)data) == _startOnCall && !_noCpuKill)
			{
				try
				{
					if (!_IsDebuggerRunning())
						return new Variant(0);

					try
					{
						int pid = _debugger != null ? _debugger.ProcessId : _systemDebugger.ProcessId;
						var proc = System.Diagnostics.Process.GetProcessById(pid);
						if (proc.HasExited)
							return new Variant(0);

						float cpu = GetProcessCpuUsage(proc);

						if (cpu < 1.0)
						{
							_StopDebugger();
							return new Variant(0);
						}
					}
					catch
					{
					}

					return new Variant(1);
				}
				catch (ArgumentException)
				{
					// Might get thrown if process has already died.
				}
			}

			return null;
		}

		public override void StopMonitor()
		{
			_StopDebugger();
			_FinishDebugger();

			if(_ipcChannel != null)
				ChannelServices.UnregisterChannel(_ipcChannel);
			
			_ipcChannel = null;
			_debugger = null;
			_systemDebugger = null;
		}

		public override void SessionStarting()
		{
			if (_startOnCall != null)
				return;

			_StartDebugger();
		}

		public override void SessionFinished()
		{
			_StopDebugger();
			_FinishDebugger();
		}

		public override void IterationStarting(int iterationCount, bool isReproduction)
		{
			if (!_IsDebuggerRunning() && _startOnCall == null)
				_StartDebugger();
		}

		public override bool IterationFinished()
		{
			if (_startOnCall != null)
				_StopDebugger();

			return false;
		}

		public override bool DetectedFault()
		{
			bool fault = false;

			if (_systemDebugger != null && _systemDebugger.caughtException)
			{
				_replay = true;

				throw new ReplayTestException();
			}

			if (_debugger != null && _debugger.caughtException)
			{
				// Kill off our debugger process and re-create
				_debuggerProcessUsage = _debuggerProcessUsageMax;
				fault = true;
			}

			return fault;
		}

		public override void GetMonitorData(System.Collections.Hashtable data)
		{
			if (!DetectedFault())
				return;

			data.Add(_name + "WindowsDebuggerHybrid", _debugger.crashInfo);
		}

		public override bool MustStop()
		{
			return false;
		}

		protected bool _IsDebuggerRunning()
		{
			if (_systemDebugger != null && _systemDebugger.IsRunning)
				return true;

			if (_debugger != null && _debugger.IsRunning)
				return true;

			return false;
		}

		System.Diagnostics.Process _debuggerProcess = null;
		int _debuggerProcessUsage = 0;
		int _debuggerProcessUsageMax = 100;
		string _debuggerChannelName = null;

		protected void _StartDebugger()
		{
			if (_hybrid)
				_StartDebuggerHybrid();
			else
				_StartDebuggerNonHybrid();
		}

		/// <summary>
		/// The hybrid mode uses both WinDbg and System Debugger
		/// </summary>
		/// <remarks>
		/// When _hybrid == true && _replay == false we will use the
		/// System Debugger.
		/// 
		/// When we hit a fault in _hybrid mode we will replay with windbg.
		/// 
		/// When _hybrid == false we will just use windbg.
		/// </remarks>
		protected void _StartDebuggerHybrid()
		{
			if (_replay)
			{
				_StartDebuggerHybridReplay();
				return;
			}

			// Start system debugger
			if (_systemDebugger == null || !_systemDebugger.IsRunning)
			{
				if (_systemDebugger == null)
				{
					_systemDebugger = new SystemDebuggerInstance();

					_systemDebugger.commandLine = _commandLine;
					_systemDebugger.processName = _processName;
					_systemDebugger.service = _service;
					_systemDebugger.ignoreFirstChanceGuardPage = _ignoreFirstChanceGuardPage;
					_systemDebugger.ignoreSecondChanceGuardPage = _ignoreSecondChanceGuardPage;
					_systemDebugger.noCpuKill = _noCpuKill;
				}

				_systemDebugger.StartDebugger();
			}
		}

		/// <summary>
		/// Hybrid replay mode uses windbg
		/// </summary>
		protected void _StartDebuggerHybridReplay()
		{
			//if (_debuggerProcess == null || _debuggerProcess.HasExited)
			//{
			//    _debuggerChannelName = "PeachCore_" + (new Random().Next().ToString());

			//    // Launch the server process
			//    _debuggerProcess = new System.Diagnostics.Process();
			//    _debuggerProcess.StartInfo.CreateNoWindow = true;
			//    _debuggerProcess.StartInfo.UseShellExecute = false;
			//    _debuggerProcess.StartInfo.Arguments = _debuggerChannelName;
			//    _debuggerProcess.StartInfo.FileName = Path.Combine(
			//        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
			//        "Peach.Core.WindowsDebugInstance.exe");
			//    _debuggerProcess.Start();
			//}

			// Try and create instance over IPC.  We will continue trying for 1 minute.

			DateTime startTimer = DateTime.Now;
			while (true)
			{
				try
				{
					_debugger = (DebuggerInstance)Activator.GetObject(typeof(DebuggerInstance),
						"ipc://" + _debuggerChannelName + "/DebuggerInstance");
					//_debugger = new DebuggerInstance();

					_debugger.commandLine = _commandLine;
					_debugger.processName = _processName;
					_debugger.kernelConnectionString = _kernelConnectionString;
					_debugger.service = _service;
					_debugger.symbolsPath = _symbolsPath;
					_debugger.startOnCall = _startOnCall;
					_debugger.ignoreFirstChanceGuardPage = _ignoreFirstChanceGuardPage;
					_debugger.ignoreSecondChanceGuardPage = _ignoreSecondChanceGuardPage;
					_debugger.noCpuKill = _noCpuKill;
					_debugger.winDbgPath = _winDbgPath;

					break;
				}
				catch
				{
					if ((DateTime.Now - startTimer).Minutes >= 1)
					{
						_debuggerProcess.Kill();
						_debuggerProcess = null;
						throw;
					}
				}
			}

			_debugger.StartDebugger();
		}

		/// <summary>
		/// Origional non-hybrid windbg only mode
		/// </summary>
		protected void _StartDebuggerNonHybrid()
		{
			if (_debuggerProcessUsage >= _debuggerProcessUsageMax && _debuggerProcess != null)
			{
				_FinishDebugger();

				_debuggerProcessUsage = 0;
			}

			if (_debuggerProcess == null || _debuggerProcess.HasExited)
			{
				_debuggerChannelName = "PeachCore_" + (new Random().Next().ToString());

				// Launch the server process
				_debuggerProcess = new System.Diagnostics.Process();
				_debuggerProcess.StartInfo.CreateNoWindow = true;
				_debuggerProcess.StartInfo.UseShellExecute = false;
				_debuggerProcess.StartInfo.Arguments = _debuggerChannelName;
				_debuggerProcess.StartInfo.FileName = Path.Combine(
					Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
					"Peach.Core.WindowsDebugInstance.exe");
				_debuggerProcess.Start();
			}

			_debuggerProcessUsage++;

			// Try and create instance over IPC.  We will continue trying for 1 minute.

			DateTime startTimer = DateTime.Now;
			while (true)
			{
				try
				{
					_debugger = (DebuggerInstance)Activator.GetObject(typeof(DebuggerInstance),
						"ipc://" + _debuggerChannelName + "/DebuggerInstance");
					//_debugger = new DebuggerInstance();

					_debugger.commandLine = _commandLine;
					_debugger.processName = _processName;
					_debugger.kernelConnectionString = _kernelConnectionString;
					_debugger.service = _service;
					_debugger.symbolsPath = _symbolsPath;
					_debugger.startOnCall = _startOnCall;
					_debugger.ignoreFirstChanceGuardPage = _ignoreFirstChanceGuardPage;
					_debugger.ignoreSecondChanceGuardPage = _ignoreSecondChanceGuardPage;
					_debugger.noCpuKill = _noCpuKill;
					_debugger.winDbgPath = _winDbgPath;

					break;
				}
				catch
				{
					if ((DateTime.Now - startTimer).Minutes >= 1)
					{
						_debuggerProcess.Kill();
						throw;
					}
				}
			}

			_debugger.StartDebugger();
		}

		protected void _FinishDebugger()
		{
			_StopDebugger();

			if (_systemDebugger != null)
				_systemDebugger.FinishDebugging();

			if (_debugger != null)
				_debugger.FinishDebugging();

			_debugger = null;
			_systemDebugger = null;

			if (_performanceCounter != null)
			{
				_performanceCounter.Close();
				_performanceCounter = null;
			}

			if (_debuggerProcess != null)
			{
				try
				{
					_debuggerProcess.Kill();
				}
				catch
				{
				}

				_debuggerProcess = null;
			}
		}

		protected void _StopDebugger()
		{
			_replay = false;

			if (_systemDebugger != null)
			{
				try
				{
					_debugger.StopDebugger();
				}
				catch
				{
				}

				if (_performanceCounter != null)
				{
					_performanceCounter.Close();
					_performanceCounter = null;
				}
			}

			if (_debugger != null)
			{
				try
				{
					_debugger.StopDebugger();
				}
				catch
				{
				}

				if (_performanceCounter != null)
				{
					_performanceCounter.Close();
					_performanceCounter = null;
				}
			}
		}
	}
}

// end
