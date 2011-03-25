
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
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Peach.Core.Dom;

namespace Peach.Core.Agent.Monitors
{
	[Monitor("WindowsDebugEngine")]
	[Monitor("debugger.WindowsDebugEngine")]
	[Parameter("CommandLine", typeof(string), "TODO", false)]
	[Parameter("ProcessName", typeof(string), "TODO", false)]
	[Parameter("KernelConnectionString", typeof(string), "TODO", false)]
	[Parameter("Service", typeof(string), "TODO", false)]
	[Parameter("SymbolsPath", typeof(string), "TODO", false)]
	[Parameter("StartOnCall", typeof(string), "TODO", false)]
	[Parameter("IgnoreFirstChanceGuardPage", typeof(string), "TODO", false)]
	[Parameter("IgnoreSecondChanceGuardPage", typeof(string), "TODO", false)]
	[Parameter("NoCpuKill", typeof(string), "TODO", false)]
	public class WindowsDebugEngine : Monitor
    {
        string _commandLine = null;
        string _processName = null;
        string _kernelConnectionString = null;
        string _service = null;
        
        string _symbolsPath = "SRV*http://msdl.microsoft.com/download/symbols";
        string _startOnCall = null;
        
        bool _ignoreFirstChanceGuardPage = false;
        bool _ignoreSecondChanceGuardPage = false;
        bool _noCpuKill = false;

        DebuggerInstance _debugger = null;

        public WindowsDebugEngine(string name, Dictionary<string, Variant> args) : base(name, args)
        {
            if (args.ContainsKey("CommandLine"))
                _commandLine = (string)args["CommandLine"];
            if(args.ContainsKey("ProcessName"))
				_processName = (string)args["ProcessName"];
            if(args.ContainsKey("KernelConnectionString"))
				_kernelConnectionString = (string)args["KernelConnectionString"];
            if(args.ContainsKey("Service"))
				_service = (string)args["Service"];
            if(args.ContainsKey("SymbolsPath"))
				_symbolsPath = (string)args["SymbolsPath"];
            if(args.ContainsKey("StartOnCall"))
				_startOnCall = (string)args["StartOnCall"];

            if(args.ContainsKey("IgnoreFirstChanceGuardPage") && ((string)args["IgnoreFirstChanceGuardPage"]).ToLower() == "true")
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
        }

		public override Variant Message(string name, Variant data)
		{
			if(name == "Action.Call" && ((string)data) == _startOnCall)
				_StartDebugger();

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
			if (!_IsDebuggerRunning())
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
			return false;
        }

        public override System.Collections.Hashtable GetMonitorData()
        {
            throw new NotImplementedException();
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
			if(!_debugger.IsRunning)
				_debugger.StartDebugger();
        }

        protected void _StopDebugger()
        {
			if (_debugger.IsRunning)
				_debugger.StopDebugger();
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

		public bool ignoreFirstChanceGuardPage = false;
		public bool ignoreSecondChanceGuardPage = false;
		public bool noCpuKill = false;

		public DebuggerInstance()
		{
			Instance = this;
		}

        public bool IsRunning
        {
            get { return _thread != null && _thread.IsAlive; }
        }

		public void StartDebugger()
		{
			_thread = new Thread(new ThreadStart(Run));
			_thread.Start();

			while (_dbg == null)
				Thread.Sleep(100);

			if (!_dbg.loadModules.WaitOne())
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
			using (_dbg = new Debuggers.DebugEngine.WindowsDebugEngine())
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
					throw new NotImplementedException();
				}
				else if (kernelConnectionString != null)
				{
					throw new NotImplementedException();
				}
				else if (service != null)
				{
					throw new NotImplementedException();
				}
			}

			_dbg = null;
        }
    }
}

// end
