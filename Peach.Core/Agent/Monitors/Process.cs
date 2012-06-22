
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

using NLog;

namespace Peach.Core.Agent.Monitors
{
	/// <summary>
	/// Start a process
	/// </summary>
	[Monitor("Process")]
	[Monitor("process.Process")]
	[Parameter("Executable", typeof(string), "Executable to launch", true)]
	[Parameter("Arguments", typeof(string), "Optional command line arguments", false)]
	[Parameter("RestartOnEachTest", typeof(bool), "Restart process for each interation (defaults to false)", false)]
	[Parameter("FaultOnEarlyExit", typeof(bool), "Trigger fault if process exists (defaults to false)", false)]
	[Parameter("CpuKill", typeof(bool), "Terminate process when CPU usage nears zero (defaults to false)", false)]
	[Parameter("StartOnCall", typeof(string), "Start command on state model call", false)]
	[Parameter("WaitForExitOnCall", typeof(string), "Wait for process to exit on state model call", false)]
	public class Process : Monitor
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		System.Diagnostics.Process _process = null;
		string _executable = null;
		string _arguments = null;
		string _startOnCall = null;
		string _waitForExitOnCall = null;
		bool _restartOnEachTest = false;
		bool _faultOnEarlyExit = false;
		bool _cpuKill = false;
		bool _firstIteration = true;
		DateTime _processStarted = DateTime.MinValue;
		bool _cpuKillProcessStarted = false;

		public Process(string name, Dictionary<string, Variant> args)
			: base(name, args)
		{
			if (args.ContainsKey("Executable"))
				_executable = (string)args["Executable"];
			if (args.ContainsKey("Arguments"))
				_arguments = (string)args["Arguments"];
			if (args.ContainsKey("FaultOnEarlyExit") && ((string)args["FaultOnEarlyExit"]).ToLower() == "true")
				_faultOnEarlyExit = true;
			if (args.ContainsKey("RestartOnEachTest") && ((string)args["RestartOnEachTest"]).ToLower() == "true")
				_restartOnEachTest = true;
			if (args.ContainsKey("CpuKill") && ((string)args["CpuKill"]).ToLower() == "true")
				_cpuKill = true;
			if (args.ContainsKey("StartOnCall"))
				_startOnCall = (string)args["StartOnCall"];
			if (args.ContainsKey("WaitForExitOnCall"))
				_waitForExitOnCall = (string)args["WaitForExitOnCall"];
		}

		void _Start()
		{
			if (_process == null || _process.HasExited)
			{
				if (_process != null)
					_process.Dispose();

				_process = new System.Diagnostics.Process();
				_process.StartInfo.FileName = _executable;
				if (!string.IsNullOrEmpty(_arguments))
					_process.StartInfo.Arguments = _arguments;

				_process.Start();
			}

			_cpuKillProcessStarted = false;
			_processStarted = DateTime.Now;
		}

		void _Stop()
		{
			if (_process != null && !_process.HasExited)
			{
				_process.Kill();
				_process.WaitForExit();
			}
			if (_process != null)
			{
				_process.Dispose();
				_process = null;
			}
		}

		public override void IterationStarting(int iterationCount, bool isReproduction)
		{
			if (_restartOnEachTest)
				_Stop();

			_Start();
		}

		public override bool DetectedFault()
		{
			if (_faultOnEarlyExit && (_process == null || _process.HasExited))
			{
				return true;
			}

			return false;
		}

		public override void GetMonitorData(System.Collections.Hashtable data)
		{
			if (!DetectedFault())
				return;

			data.Add("Process", "Process exited early: " + _executable + " " + _arguments);
		}

		public override bool MustStop()
		{
			return false;
		}

		public override void StopMonitor()
		{
			_Stop();
		}

		public override void SessionStarting()
		{
			_firstIteration = true;

			if (_startOnCall == null && !_restartOnEachTest)
				_Start();
		}

		public override void SessionFinished()
		{
			_Stop();
		}

		public override bool IterationFinished()
		{
			if (_firstIteration)
				_firstIteration = false;

			if (_restartOnEachTest || _startOnCall != null)
				_Stop();

			return true;
		}

		public override Variant Message(string name, Variant data)
		{
			if (name == "Action.Call" && ((string)data) == _startOnCall)
			{
				_Stop();
				_Start();
				return null;
			}
			else if (name == "Action.Call" && ((string)data) == _waitForExitOnCall)
			{
				if (_process != null && !_process.HasExited)
				{
					// WARNING: Infinite wait!
					_process.WaitForExit();
				}

				_Stop();
				return null;
			}

			else if (name == "Action.Call.IsRunning" && ((string)data) == _startOnCall && _cpuKill)
			{
				try
				{
					if (_process == null || _process.HasExited)
						return new Variant(0);

					try
					{
						float cpu = GetProcessCpuUsage(_process);

						logger.Debug("Message: GetProcessCpuUsage: " + cpu);

						if (cpu > 1.0 || (DateTime.Now - _processStarted).Seconds > 3)
							_cpuKillProcessStarted = true;

						if (_cpuKillProcessStarted && cpu < 1.0)
						{
							logger.Debug("Message: Stopping process.");
							_Stop();
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
			else
			{
				logger.Debug("Unknown msg: " + name + " data: " + (string)data);
			}

			return null;
		}

		PerformanceCounter _performanceCounter = null;
		public float GetProcessCpuUsage(System.Diagnostics.Process proc)
		{
			try
			{
				PerformanceCounter tmp = new PerformanceCounter("Process", "% User Time", proc.ProcessName);
				tmp.NextValue();
				System.Threading.Thread.Sleep(100);
				logger.Debug("% User Time: " + tmp.NextValue());

				tmp = new PerformanceCounter("Process", "% Privileged Time", proc.ProcessName);
				tmp.NextValue();
				System.Threading.Thread.Sleep(100);
				logger.Debug("% Privileged Time: " + tmp.NextValue());

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
	}
}

// end
