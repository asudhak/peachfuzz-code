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
	[Parameter("When", typeof(When), "Period _When the command should be ran")]
	[Parameter("UseShellExecute", typeof(bool), "Use the operating system shell to run the command", "true")]
	[Parameter("CheckValue", typeof(string), "Regex to match on response", "")]
	[Parameter("FaultOnMatch", typeof(bool), "Fault if regex matches", "true")]
	public class RunCommand  : Monitor
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		public string Command { get; private set; }
		public string Arguments { get; private set; }
		public When _When { get; private set; }
		public bool UseShellExecute { get; private set; }
		public string CheckValue { get; protected set; }
		public bool FaultOnMatch { get; protected set; }


		private string _output = "";
		private Fault _fault = null;
		private Regex _regex = null;


		public enum When {OnStart, OnEnd, OnIterationStart, OnIterationEnd, OnFault};

		public RunCommand(IAgent agent, string name, Dictionary<string, Variant> args)
			: base(agent, name, args)
		{
			ParameterParser.Parse(this, args);
			try
			{
				_regex = new Regex(CheckValue ?? "", RegexOptions.Multiline);
			}
			catch (ArgumentException ex)
			{
				throw new PeachException("'CheckValue' is not a valid regular expression.  " + ex.Message, ex);
			}

		}

		void _Start()
		{
			var startInfo = new ProcessStartInfo();
			startInfo.FileName = Command;
			startInfo.RedirectStandardOutput = true;
			startInfo.UseShellExecute = UseShellExecute;
			startInfo.Arguments = Arguments;

			logger.Debug("_Start(): Running command " + Command + " with arguments " + Arguments);

			try
			{
				using (var p = new System.Diagnostics.Process())
				{
					_output = "";
					p.StartInfo = startInfo;
					p.Start();
					_output = p.StandardOutput.ReadToEnd();
					p.WaitForExit();

				}
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
			_fault = new Fault();
			_fault.title = this.name + "-Response";
			_fault.description = _output;
			_fault.type = FaultType.Data;
			bool match = _regex.IsMatch(_fault.description);
			if (match)
				_fault.type = FaultOnMatch ? FaultType.Fault : FaultType.Data;
			else
				_fault.type = FaultOnMatch ? FaultType.Data : FaultType.Fault;
			return _fault.type == FaultType.Fault;
		}

		public override Fault GetMonitorData()
		{
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
			return null;
		}
	}
}
