
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
	[Monitor("WindowsDebugEngine")]
	[Monitor("debugger.WindowsDebugEngine")]
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
	public class WindowsDebugEngine : Monitor
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

        DebuggerInstance _debugger = null;

        public WindowsDebugEngine(string name, Dictionary<string, Variant> args) : base(name, args)
        {
			_name = name;

			if (args.ContainsKey("CommandLine"))
				_commandLine = (string)args["CommandLine"];
			else if (args.ContainsKey("ProcessName"))
				_processName = (string)args["ProcessName"];
			else if (args.ContainsKey("KernelConnectionString"))
				_kernelConnectionString = (string)args["KernelConnectionString"];
			else if (args.ContainsKey("Service"))
				_service = (string)args["Service"];
			else
				throw new PeachException("Error, WindowsDebugEngine started with out a CommandLine, ProcessName, KernelConnectionString or Service parameter.");
            
			if(args.ContainsKey("SymbolsPath"))
				_symbolsPath = (string)args["SymbolsPath"];
            if(args.ContainsKey("StartOnCall"))
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
            if(args.ContainsKey("IgnoreSecondChanceGuardPage") && ((string)args["IgnoreSecondChanceGuardPage"]).ToLower() == "true")
                _ignoreSecondChanceGuardPage = true;
            if(args.ContainsKey("NoCpuKill") && ((string)args["NoCpuKill"]).ToLower() == "true")
                _noCpuKill = true;

			_debugger = new DebuggerInstance();
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
		}

		protected string FindWinDbg()
		{
			// Lets try a few common places before failing.
			List<string> pgPaths = new List<string>();
			pgPaths.Add(@"c:\");
			pgPaths.Add(Environment.GetEnvironmentVariable("SystemDrive"));
			pgPaths.Add(Environment.GetEnvironmentVariable("ProgramFiles"));

			if(Environment.GetEnvironmentVariable("ProgramW6432") != null)
				pgPaths.Add(Environment.GetEnvironmentVariable("ProgramW6432"));
			
			if(Environment.GetEnvironmentVariable("ProgramFiles(x86)") != null)
				pgPaths.Add(Environment.GetEnvironmentVariable("ProgramFiles(x86)"));
			
			List<string> dbgPaths = new List<string>();
			dbgPaths.Add("Debuggers");
			dbgPaths.Add("Debugger");
			dbgPaths.Add("Debugging Tools for Windows");
			dbgPaths.Add("Debugging Tools for Windows (x64)");
			dbgPaths.Add("Debugging Tools for Windows (x86)");

			foreach(string path in pgPaths)
			{
				foreach(string dpath in dbgPaths)
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
			if(name == "Action.Call" && ((string)data) == _startOnCall)
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

					int pid = _debugger.ProcessId;
					var proc = System.Diagnostics.Process.GetProcessById(pid);
					if (proc.HasExited)
						return new Variant(0);

					float cpu = GetProcessCpuUsage(proc);
					//Console.WriteLine("cpu: " + cpu);
					if (cpu < 1.0)
					{
						_StopDebugger();
						return new Variant(0);
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
			_debugger = null;
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
			if (_debugger.caughtException)
				return true;

			return false;
        }

        public override void GetMonitorData(System.Collections.Hashtable data)
        {
			if (!DetectedFault())
				return;

			data.Add(_name + "WindowsDebugEngine", _debugger.crashInfo);
        }

        public override bool MustStop()
        {
			return false;
        }

        protected bool _IsDebuggerRunning()
        {
            if (_debugger != null && _debugger.IsRunning)
                return true;

            return false;
        }

        protected void _StartDebugger()
        {
			if (!_debugger.IsRunning)
			{
				if (_performanceCounter != null)
				{
					_performanceCounter.Close();
					_performanceCounter = null;
				}

				_debugger.StartDebugger();
			}
        }

        protected void _StopDebugger()
        {
			if (_debugger.IsRunning)
			{
				_debugger.StopDebugger();

				if (_performanceCounter != null)
				{
					_performanceCounter.Close();
					_performanceCounter = null;
				}
			}
        }
    }

    class DebuggerInstance
    {
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
		public Dictionary<string, Variant> crashInfo = null;

		public DebuggerInstance()
		{
			Instance = this;
		}

		public int ProcessId
		{
			get { return _dbg.processId; }
		}

        public bool IsRunning
        {
            get { return _thread != null && _thread.IsAlive; }
        }

		public void StartDebugger()
		{
			_thread = new Thread(new ThreadStart(Run));
			_thread.Start();

			while (_dbg == null && !dbgExited)
				Thread.Sleep(100);

			if (_dbg != null && !_dbg.loadModules.WaitOne())
				Console.Error.WriteLine("WaitOne == false");
		}

		public void StopDebugger()
		{
			_dbg.exitDebugger.Set();

			for (int cnt = 0; _thread.IsAlive && cnt < 100; cnt++)
				Thread.Sleep(100);

			_thread.Abort();
			_thread.Join();
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
					// TODO
					throw new NotImplementedException();
				}
				else if (kernelConnectionString != null)
				{
					// TODO
					throw new NotImplementedException();
				}
				else if (service != null)
				{
					// TODO
					throw new NotImplementedException();
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
