
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
	[Monitor("Process", true)]
	[Monitor("process.Process")]
	[Parameter("Executable", typeof(string), "Executable to launch")]
	[Parameter("Arguments", typeof(string), "Optional command line arguments", "")]
	[Parameter("RestartOnEachTest", typeof(bool), "Restart process for each interation", "false")]
	[Parameter("FaultOnEarlyExit", typeof(bool), "Trigger fault if process exists", "false")]
	[Parameter("NoCpuKill", typeof(bool), "Disable process killing when CPU usage nears zero", "false")]
	[Parameter("StartOnCall", typeof(string), "Start command on state model call", "")]
	[Parameter("WaitForExitOnCall", typeof(string), "Wait for process to exit on state model call and fault if timeout is reached", "")]
	[Parameter("WaitForExitTimeout", typeof(int), "Wait for exit timeout value in milliseconds (-1 is infinite)", "10000")]
	public class Process : Monitor
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		System.Diagnostics.Process _process = null;
		Fault _fault = null;
		bool _messageExit = false;

		public string Executable { get; private set; }
		public string Arguments { get; private set; }
		public bool RestartOnEachTest { get; private set; }
		public bool FaultOnEarlyExit { get; private set; }
		public bool NoCpuKill { get; private set; }
		public string StartOnCall { get; private set; }
		public string WaitForExitOnCall { get; private set; }
		public int WaitForExitTimeout { get; private set; }

		public Process(IAgent agent, string name, Dictionary<string, Variant> args)
			: base(agent, name, args)
		{
			ParameterParser.Parse(this, args);
		}

		void _Start()
		{
			if (_process == null || _process.HasExited)
			{
				if (_process != null)
					_process.Close();

				_process = new System.Diagnostics.Process();
				_process.StartInfo.FileName = Executable;
				_process.StartInfo.UseShellExecute = false;

				if (!string.IsNullOrEmpty(Arguments))
					_process.StartInfo.Arguments = Arguments;

				logger.Debug("_Start(): Starting process");

				try
				{
					_process.Start();
				}
				catch (Exception ex)
				{
					_process = null;
					throw new PeachException("Could not start process '" + Executable + "'.  " + ex.Message + ".", ex);
				}
			}
			else
			{
				logger.Debug("_Start(): Process already running, ignore");
			}
		}

		void _Stop()
		{
			logger.Debug("_Stop()");

			for (int i = 0; i < 100 && _IsRunning(); i++)
			{
				logger.Debug("_Stop(): Killing process");
				try
				{
					_process.Kill();
					_process.WaitForExit();
					_process.Close();
					_process = null;
				}
				catch (Exception ex)
				{
					logger.Error("_Stop(): {0}", ex.Message);
				}
			}

			if (_process != null)
			{
				logger.Debug("_Stop(): Closing process handle");
				_process.Close();
				_process = null;
			}
			else
			{
				logger.Debug("_Stop(): _process == null, done!");
			}
		}

		void _WaitForExit(bool useCpuKill)
		{
			if (!_IsRunning())
				return;

			if (useCpuKill && !NoCpuKill)
			{
				const int pollInterval = 200;
				ulong lastTime = 0;
				int i = 0;

				try
				{
					for (i = 0; i < WaitForExitTimeout; i += pollInterval)
					{
						var pi = ProcessInfo.Instance.Snapshot(_process);

						logger.Trace("CpuKill: OldTicks={0} NewTicks={1}", lastTime, pi.TotalProcessorTicks);

						if (i != 0 && lastTime == pi.TotalProcessorTicks)
						{
							logger.Debug("Cpu is idle, stopping process.");
							break;
						}

						lastTime = pi.TotalProcessorTicks;
						Thread.Sleep(pollInterval);
					}

					if (i >= WaitForExitTimeout)
						logger.Debug("Timed out waiting for cpu idle, stopping process.");
				}
				catch (Exception ex)
				{
					logger.Debug("Error querying cpu time: {0}", ex.Message);
				}

				_Stop();
			}
			else
			{
				logger.Debug("WaitForExit({0})", WaitForExitTimeout == -1 ? "INFINITE" : WaitForExitTimeout.ToString());

				if (!_process.WaitForExit(WaitForExitTimeout))
				{
					if (!useCpuKill)
					{
						logger.Debug("FAULT, WaitForExit ran out of time!");
						_fault = MakeFault("ProcessFailedToExit", "Process did not exit in " + WaitForExitTimeout + "ms");
						this.Agent.QueryMonitors("CanaKitRelay_Reset");
					}
				}
			}
		}

		bool _IsRunning()
		{
			return _process != null && !_process.HasExited;
		}

		Fault MakeFault(string folder, string reason)
		{
			return new Fault()
			{
				type = FaultType.Fault,
				detectionSource = "ProcessMonitor",
				title = reason,
				description = "{0}: {1} {2}".Fmt(reason, Executable, Arguments),
				folderName = folder,
			};
		}

		public override void IterationStarting(uint iterationCount, bool isReproduction)
		{
			_fault = null;
			_messageExit = false;

			if (RestartOnEachTest)
				_Stop();

			if (StartOnCall == null)
				_Start();
		}

		public override bool DetectedFault()
		{
			return _fault != null;
		}

		public override Fault GetMonitorData()
		{
			return _fault;
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
			if (StartOnCall == null && !RestartOnEachTest)
				_Start();
		}

		public override void SessionFinished()
		{
			_Stop();
		}

		public override bool IterationFinished()
		{
			if (!_messageExit && FaultOnEarlyExit && !_IsRunning())
			{
				_fault = MakeFault("ProcessExitedEarly", "Process exited early");
				_Stop();
			}
			else  if (StartOnCall != null)
			{
				_WaitForExit(true);
				_Stop();
			}
			else if (RestartOnEachTest)
			{
				_Stop();
			}

			return true;
		}

		public override Variant Message(string name, Variant data)
		{
			logger.Debug("Message(" + name + ", " + (string)data + ")");

			if (name == "Action.Call" && ((string)data) == StartOnCall)
			{
				_Stop();
				_Start();
			}
			else if (name == "Action.Call" && ((string)data) == WaitForExitOnCall)
			{
				_messageExit = true; 
				_WaitForExit(false);
				_Stop();
			}
			else
			{
				logger.Debug("Unknown msg: " + name + " data: " + (string)data);
			}

			return null;
		}
	}
}

// end
