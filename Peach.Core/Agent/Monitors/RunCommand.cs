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
	[Parameter("CheckValue", typeof(string), "Regex to match on response", "")]
	[Parameter("FaultOnMatch", typeof(bool), "Fault if regex matches", "true")]
	[Parameter("Timeout", typeof(int), "Fail if process takes more than Timeout seconds where zero is no timeout ", "0")]
	[Parameter("UseShellExecute", typeof(bool), "Use the operating system shell to run the command (conflicts with CheckValue)", "true")]
	public class RunCommand  : Monitor
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		public string Command { get; private set; }
		public string Arguments { get; private set; }
		public string StartOnCall { get; private set; }
		public When _When { get; private set; }
		public bool UseShellExecute { get; private set; }
		public string CheckValue { get; protected set; }
		public int Timeout { get; private set; }
		public bool FaultOnMatch { get; protected set; }

		private Fault _fault = null;
		private bool _last_was_fault = false;
		private Regex _regex = null;
		private string _cmd_output = null;
		bool _run_attempted = false;
		bool _run_success = false; 

		public enum When {OnCall, OnStart, OnEnd, OnIterationStart, OnIterationEnd, OnFault, OnIterationStartAfterFault};

		public RunCommand(IAgent agent, string name, Dictionary<string, Variant> args)
			: base(agent, name, args)
		{
			ParameterParser.Parse(this, args);
			try
			{
				_regex = new Regex(CheckValue ?? "", RegexOptions.Multiline);
				if(UseShellExecute & !System.String.IsNullOrEmpty(CheckValue))
					throw new PeachException("'CheckValue' conflicts with 'UseShellExecute'");
			}
			catch (ArgumentException ex)
			{
				throw new PeachException("'CheckValue' is not a valid regular expression.  " + ex.Message, ex);
			}
		}

		void _Start()
		{
			_run_attempted = true;
			var startInfo = new ProcessStartInfo();
			startInfo.FileName = Command;
			startInfo.UseShellExecute = UseShellExecute;
			startInfo.UseShellExecute = false;
			startInfo.Arguments = Arguments;
			startInfo.RedirectStandardOutput = !UseShellExecute;

			logger.Debug("_Start(): Running command " + Command + " with arguments " + Arguments);

			try
			{
				using (var p = new System.Diagnostics.Process())
				{
					p.StartInfo = startInfo;
					p.Start();
					if (Timeout != 0)
					{
						logger.Debug("_Start(): Waiting for " + Timeout +  " seconds for command to exit");
						_run_success = p.WaitForExit(Timeout * 1000); //input in milliseconds, Timeout is in seconds
						if (_run_success)
						{
							logger.Debug("_Start(): command exited cleanly");
						}
						else
						{
							logger.Debug("_Start(): timeout, killing");	
							p.Kill();
						}
					}
					else
					{
						p.WaitForExit();
						_run_success = true;
					}
					
					if(System.String.IsNullOrEmpty(CheckValue))
						_cmd_output = "";
					else
						_cmd_output = p.StandardOutput.ReadToEnd();
				}
			}
			catch (Exception ex)
			{
				throw new PeachException("Could not run command '" + Command + "'.  " + ex.Message + ".", ex);
			}
		}


		public override void IterationStarting(uint iterationCount, bool isReproduction)
		{
			_run_attempted = false; //reset for next run
			if (_When == When.OnIterationStart || ( _last_was_fault && _When == When.OnIterationStartAfterFault))
				_Start();
			_last_was_fault = false;
			_fault = null;
			_cmd_output = "";
		}

		public override bool DetectedFault()
		{
			_fault = new Fault();
			_fault.type = FaultType.Fault;
			_fault.detectionSource = "RunCommand";
			_fault.folderName = "RunCommand";

			try
			{
				_fault.title = "Response";
				_fault.description = _cmd_output;

				bool match = _regex.IsMatch(_fault.description);
				
				logger.Debug("DetectedFault(): Checking for faults during run of command " + Command);
				if (!_run_attempted)
					_fault.type = FaultType.Data;
				else if (!_run_success)
				{
					logger.Debug("DetectedFault(): Execution of command" + Command + " failed");
					_fault.type = FaultType.Fault;
					_fault.description = "Run timed out:" + _cmd_output;
				}
				else if (match && !System.String.IsNullOrEmpty(CheckValue))
				{
					logger.Debug("DetectedFault(): match found");
					_fault.type = FaultOnMatch ? FaultType.Fault : FaultType.Data;
					
				}
				else
				{
					logger.Debug("DetectedFault(): match not found");
					_fault.type = FaultOnMatch ? FaultType.Data : FaultType.Fault;					
				}
			}
			catch (Exception ex)
			{
				_fault.title = "Exception";
				_fault.description = ex.Message;
			}
			
			return _fault.type == FaultType.Fault;

		}

		public override Fault GetMonitorData()
		{
			if (_When == When.OnFault)
				_Start();
			_last_was_fault = true;
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
			{
				_Start();
			}

			return null;
		}
	}
}
