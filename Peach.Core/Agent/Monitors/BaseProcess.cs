
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
	[Parameter("Executable", typeof(string), "Executable to launch", true)]
	[Parameter("Arguments", typeof(string), "Optional command line arguments", false)]
	[Parameter("RestartOnEachTest", typeof(bool), "Restart process for each interation (defaults to false)", false)]
	[Parameter("FaultOnEarlyExit", typeof(bool), "Trigger fault if process exists (defaults to false)", false)]
	[Parameter("CpuKill", typeof(bool), "Terminate process when CPU usage nears zero (defaults to false)", false)]
	[Parameter("StartOnCall", typeof(string), "Start command on state model call", false)]
	[Parameter("WaitForExitOnCall", typeof(string), "Wait for process to exit on state model call", false)]
	public abstract class BaseProcess : Monitor
	{
		protected abstract ulong GetTotalCpuTime(System.Diagnostics.Process process);

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
		ulong _totalProcessorTime = 0;

		public BaseProcess(string name, Dictionary<string, Variant> args)
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

				logger.Debug("_Start(): Starting process");
				_process.Start();

				_totalProcessorTime = ulong.MaxValue;
			}
			else
			{
				logger.Debug("_Start(): Process already running, ignore");
			}
		}

		void _Stop()
		{
			logger.Debug("_Stop()");

			for(int i = 0; i < 100 && (_process != null && !_process.HasExited); i++)
			{
				logger.Debug("_Stop(): Killing process");
				try
				{
					_process.Kill();
					_process.WaitForExit();
					_process.Dispose();
					_process = null;
				}
				catch
				{
				}
			}
			
			if (_process != null)
			{
				_process.Dispose();
				_process = null;
			}
			else
			{
				logger.Debug("_Stop(): _process == null, done!");
			}
		}

		public override void IterationStarting(uint iterationCount, bool isReproduction)
		{
			if (_restartOnEachTest)
				_Stop();

			if (_startOnCall == null)
				_Start();
		}

		public override bool DetectedFault()
		{
			if (_faultOnEarlyExit && (_process == null || _process.HasExited))
			{
				logger.Debug("DetectedFault(): Process exited early, saying true!");
				return true;
			}

			return false;
		}

		public override Fault GetMonitorData()
		{
			if (!DetectedFault())
				return null;

            Fault fault = new Fault();
            fault.type = FaultType.Fault;
            fault.detectionSource = "ProcessMonitor";
            fault.title = "Process exited early";
            fault.description = "Process exited early: " + _executable + " " + _arguments;
            fault.folderName = "ProcessExitedEarly";

            return fault;
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
			logger.Debug("SessionFinished(): Calling stop");
			_Stop();
		}

		public override bool IterationFinished()
		{
			if (_firstIteration)
				_firstIteration = false;

			if (_restartOnEachTest || _startOnCall != null)
			{
				logger.Debug("IterationFinished(): Calling _Stop");
				_Stop();
			}

			return true;
		}

		public override Variant Message(string name, Variant data)
		{
			logger.Debug("Message(" + name + ", " + (string)data + ")");

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

			else if (name == "Action.Call.IsRunning" && ((string)data) == _startOnCall)
			{
				try
				{
					if (_process == null || _process.HasExited)
					{
						logger.Debug("Message(Action.Call.IsRunning): Process has exited!");
						_Stop();
						return new Variant(0);
					}

					if (_cpuKill)
					{
						var lastTime = _totalProcessorTime;
						_totalProcessorTime = GetTotalCpuTime(_process);

						if (lastTime == _totalProcessorTime)
						{
							logger.Debug("Message(Action.Call.IsRunning): Stopping process.");
							_Stop();
							return new Variant(0);
						}
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
