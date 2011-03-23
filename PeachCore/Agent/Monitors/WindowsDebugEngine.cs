
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
using MS.Debuggers.DbgEng;

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
        }

        public override void StopMonitor()
        {
            throw new NotImplementedException();
        }

        public override void SessionStarting()
        {
            throw new NotImplementedException();
        }

        public override void SessionFinished()
        {
            throw new NotImplementedException();
        }

        public override void IterationStarting(int iterationCount, bool isReproduction)
        {
            throw new NotImplementedException();
        }

        public override bool IterationFinished()
        {
            throw new NotImplementedException();
        }

        public override bool DetectedFault()
        {
            throw new NotImplementedException();
        }

        public override System.Collections.Hashtable GetMonitorData()
        {
            throw new NotImplementedException();
        }

        public override bool MustStop()
        {
            throw new NotImplementedException();
        }

        protected bool _IsDebuggerRunning()
        {
            if (_debugger != null && _debugger.IsRunning)
                return true;

            return false;
        }

        protected void _StartDebugger()
        {
        }

        protected void _StopDebugger()
        {
        }
    }

    class DebuggerInstance
    {
        Thread _thread = null;
        Debuggee _dbg = null;

        string _commandLine = null;
        string _processName = null;
        string _kernelConnectionString = null;
        string _service = null;

        string _symbolsPath = "SRV*http://msdl.microsoft.com/download/symbols";
        string _startOnCall = null;

        bool _ignoreFirstChanceGuardPage = false;
        bool _ignoreSecondChanceGuardPage = false;
        bool _noCpuKill = false;

        public static EventWaitHandle Ready = new AutoResetEvent(false);

        public bool IsRunning
        {
            get { return _thread != null && _thread.IsAlive; }
        }

        public void Run()
        {
            _dbg = new Debuggee();
            _dbg.SymbolPath = _symbolsPath;
            _dbg.DebugOutput += new EventHandler<DebugOutputEventArgs>(dbg_DebugOutput);

            if (_commandLine != null)
            {
                _dbg.CreateAndAttachProcess(_commandLine, null);
                _dbg.WaitForEvent(0);
            }
            else if (_processName != null)
            {
                throw new NotImplementedException();
            }
            else if (_kernelConnectionString != null)
            {
                throw new NotImplementedException();
            }
            else if (_service != null)
            {
                throw new NotImplementedException();
            }

            Ready.Set();

        }

        static void dbg_DebugOutput(object sender, DebugOutputEventArgs e)
        {
            Console.WriteLine(e.Output);
        }
    }
}

// end
