using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.IO;

using Peach.Core.Dom;

using NLog;

namespace Peach.Core.Agent.Monitors
{
	[Monitor("RunCommand", true)]
	[Parameter("Command", typeof(string), "Command line command to run")]
	[Parameter("Arguments", typeof(string), "Optional command line arguments", "")]
	[Parameter("When", typeof(When), "Period _When the command should be ran", "OnCall")]
	[Parameter("StartOnCall", typeof(string), "Run when signaled by the state machine", "")]
	public class RunCommand  : Monitor
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		public string Command { get; private set; }
		public string Arguments { get; private set; }
		public string StartOnCall { get; private set; }
		public When _When { get; private set; }

		public enum When { OnCall, OnStart, OnEnd, OnIterationStart, OnIterationEnd, OnFault };

		public RunCommand(IAgent agent, string name, Dictionary<string, Variant> args)
			: base(agent, name, args)
		{
			ParameterParser.Parse(this, args);
		}

		void _Start()
		{
			logger.Debug("_Start(): Running command " + Command + " with arguments " + Arguments);

			try
			{
				SubProcess.Run(Command, Arguments);
			}
			catch (Exception ex)
			{
				throw new PeachException("Could not run command '" + Command + "'.  " + ex.Message + ".", ex);
			}
		}

		public override void IterationStarting(uint iterationCount, bool isReproduction)
		{
			if (_When == When.OnIterationStart)
				_Start();
		}

		public override bool DetectedFault()
		{
			return false;
		}

		public override Fault GetMonitorData()
		{
			if (_When == When.OnFault)
				_Start();

			return null;
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
			{
				_Start();
			}

			return null;
		}
	}
}
