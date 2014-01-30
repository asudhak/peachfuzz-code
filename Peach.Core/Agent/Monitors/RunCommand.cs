using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;

using Peach.Core.Dom;

using NLog;

namespace Peach.Core.Agent.Monitors
{
	[Monitor("RunCommand", true)]
	[Parameter("Command", typeof(string), "Command line command to run")]
	[Parameter("Arguments", typeof(string), "Optional command line arguments", "")]
	[Parameter("When", typeof(When), "Period _When the command should be ran", "OnCall")]
	[Parameter("StartOnCall", typeof(string), "Run when signaled by the state machine", "")]
	[Parameter("FaultOnNonZeroExit", typeof(bool), "Fault if exit code is non-zero", "false")]
	[Parameter("Timeout", typeof(int), "Fault if process takes more than Timeout seconds where -1 is infinite timeout ", "-1")]
	public class RunCommand  : Monitor
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		public string Command { get; private set; }
		public string Arguments { get; private set; }
		public string StartOnCall { get; private set; }
		public When _When { get; private set; }
		public string CheckValue { get; protected set; }
		public int Timeout { get; private set; }
		public bool FaultOnNonZeroExit { get; protected set; }

		private Fault _fault = null;
		private bool _lastWasFault = false;

		public enum When { OnCall, OnStart, OnEnd, OnIterationStart, OnIterationEnd, OnFault, OnIterationStartAfterFault };

		public RunCommand(IAgent agent, string name, Dictionary<string, Variant> args)
			: base(agent, name, args)
		{
			ParameterParser.Parse(this, args);
		}

		void _Start()
		{
			_fault = null;

			var startInfo = new ProcessStartInfo();
			startInfo.FileName = Command;
			startInfo.UseShellExecute = false;
			startInfo.Arguments = Arguments;
			startInfo.RedirectStandardOutput = true;
			startInfo.RedirectStandardError = true;

			logger.Debug("_Start(): Running command " + Command + " with arguments " + Arguments);

			try
			{
				var p = SubProcess.Run(Command, Arguments, Timeout);

				var stdout = p.StdOut.ToString();
				var stderr = p.StdErr.ToString();

				_fault = new Fault();
				_fault.detectionSource = "RunCommand";
				_fault.folderName = "RunCommand";
				_fault.collectedData.Add(new Fault.Data("stdout", Encoding.ASCII.GetBytes(stdout)));
				_fault.collectedData.Add(new Fault.Data("stderr", Encoding.ASCII.GetBytes(stderr)));

				if (p.Timeout)
				{
					_fault.description = "Process failed to exit in alotted time.";
					_fault.type = FaultType.Fault;
				}
				else if (FaultOnNonZeroExit && p.ExitCode != 0)
				{
					_fault.description = "Process exited with code {0}.".Fmt(p.ExitCode);
					_fault.type = FaultType.Fault;
				}
				else
				{
					_fault.description = stdout;
					_fault.type = FaultType.Data;
				}
			}
			catch (Exception ex)
			{
				throw new PeachException("Could not run command '" + Command + "'.  " + ex.Message + ".", ex);
			}
		}


		public override void IterationStarting(uint iterationCount, bool isReproduction)
		{
			// Sync _lastWasFault incase _Start() throws
			bool lastWasFault = _lastWasFault;
			_lastWasFault = false;

			if (_When == When.OnIterationStart || (lastWasFault && _When == When.OnIterationStartAfterFault))
				_Start();
		}

		public override bool DetectedFault()
		{
			return _fault != null && _fault.type == FaultType.Fault;
		}

		public override Fault GetMonitorData()
		{
			// Some monitor triggered a fault
			_lastWasFault = true;

			if (_When == When.OnFault)
				_Start();

			return _fault;
		}

		public override bool MustStop()
		{
			return false;
		}

		public override void StopMonitor()
		{
			return;
		}

		public override void SessionStarting()
		{
			if (_When == When.OnStart)
				_Start();
		}

		public override void SessionFinished()
		{
			if (_When == When.OnEnd)
				_Start();
		}

		public override bool IterationFinished()
		{
			if (_When == When.OnIterationEnd)
				_Start();

			return true;
		}

		public override Variant Message(string name, Variant data)
		{
			if (name == "Action.Call" && ((string)data) == StartOnCall && _When == When.OnCall)
				_Start();

			return null;
		}
	}
}
