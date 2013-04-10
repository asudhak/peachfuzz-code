
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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Peach.Core.Dom;
using Proc = System.Diagnostics.Process;
using System.Text.RegularExpressions;
using NLog;

namespace Peach.Core.Agent.Monitors
{
	static class ConfigExtensions
	{
		public static string GetString(this Dictionary<string, Variant> args, string key, string defaultValue)
		{
			Variant value;
			if (args.TryGetValue(key, out value))
				return (string)value;
			return defaultValue;
		}

		public static bool GetBoolean(this Dictionary<string, Variant> args, string key, bool defaultValue)
		{
			string ret = args.GetString(key, defaultValue.ToString());
			return Convert.ToBoolean(ret);
		}

		public static int GetInt(this Dictionary<string, Variant> args, string key, int defaultValue)
		{
			string ret = args.GetString(key, defaultValue.ToString());
			return int.Parse(ret);
		}
	}

	/// <summary>
	/// Monitor will use OS X's built in CrashReporter (similar to watson)
	/// to detect and report crashes.
	/// </summary>
	[Monitor("CrashWrangler", true)]
	[Monitor("osx.CrashWrangler")]
	[Parameter("Command", typeof(string), "Command to execute")]
	[Parameter("Arguments", typeof(string), "Commad line arguments", "")]
	[Parameter("StartOnCall", typeof(string), "Start command on state model call", "")]
	[Parameter("UseDebugMalloc", typeof(bool), "Use OS X Debug Malloc (slower) (defaults to false)", "false")]
	[Parameter("ExecHandler", typeof(string), "Crash Wrangler Execution Handler program.", "exc_handler")]
	[Parameter("ExploitableReads", typeof(bool), "Are read a/v's considered exploitable? (defaults to false)", "false")]
	[Parameter("NoCpuKill", typeof(bool), "Disable process killing by CPU usage? (defaults to false)", "false")]
	[Parameter("CwLogFile", typeof(string), "CrashWrangler Log file (defaults to cw.log)", "cw.log")]
	[Parameter("CwLockFile", typeof(string), "CrashWRangler Lock file (defaults to cw.lock)", "cw.lock")]
	[Parameter("CwPidFile", typeof(string), "CrashWrangler PID file (defaults to cw.pid)", "cw.pid")]
	[Parameter("FaultOnEarlyExit", typeof(bool), "Trigger fault if process exists (defaults to false)", "false")]
	[Parameter("WaitForExitOnCall", typeof(string), "Wait for process to exit on state model call and fault if timeout is reached", "")]
	[Parameter("WaitForExitTimeout", typeof(int), "Wait for exit timeout value in milliseconds (-1 is infinite)", "10000")]
	[Parameter("RestartOnEachTest", typeof(bool), "Restart process for each interation", "false")]
	public class CrashWrangler : Monitor
	{
		protected string _command = null;
		protected string _arguments = null;
		protected string _startOnCall = null;
		protected string _waitForExitOnCall = null;
		protected bool _useDebugMalloc = false;
		protected string _execHandler = null;
		protected bool _exploitableReads = false;
		protected bool _noCpuKill = false;
		protected string _cwLogFile = "cw.log";
		protected string _cwLockFile = "cw.lck";
		protected string _cwPidFile = "cw.pid";
		protected Proc _procHandler = null;
		protected Proc _procCommand = null;
		protected bool? _detectedFault = null;
		protected ulong _totalProcessorTime = 0;
		protected bool _faultOnEarlyExit = false;
		protected bool _faultExitFail = false;
		protected bool _faultExitEarly = false;
		protected bool _messageExit = false;
		protected bool _restartOnEachTest = false;
		protected int _waitForExitTimeout = 10000;

		public CrashWrangler(IAgent agent, string name, Dictionary<string, Variant> args)
			: base(agent, name, args)
		{
			_command = args.GetString("Command", null);
			_arguments = args.GetString("Arguments", "");
			_startOnCall = args.GetString("StartOnCall", null);
			_useDebugMalloc = args.GetBoolean("UseDebugMalloc", false);
			_execHandler = args.GetString("ExecHandler", "exc_handler");
			_exploitableReads = args.GetBoolean("ExploitableReads", false);
			_noCpuKill = args.GetBoolean("NoCpuKill", false);
			_cwLogFile = args.GetString("CwLogFile", "cw.log");
			_cwLockFile = args.GetString("CwLockFile", "cw.lock");
			_cwPidFile = args.GetString("CwPidFile", "cw.pid");
			_faultOnEarlyExit = args.GetBoolean("FaultOnEarlyExit", false);
			_waitForExitOnCall = args.GetString("WaitForExitOnCall", null);
			_waitForExitTimeout = args.GetInt("WaitForExitTimeout", 10000);
			_restartOnEachTest = args.GetBoolean("RestartOnEachTest", false);
		}

		public override void IterationStarting(uint iterationCount, bool isReproduction)
		{
			_detectedFault = null;
			_faultExitFail = false;
			_faultExitEarly = false;
			_messageExit = false;

			if (!_IsProcessRunning() && _startOnCall == null)
				_StartProcess();
		}

		public override bool DetectedFault()
		{
			if (_detectedFault == null)
			{
				// Give CrashWrangler a chance to write the log
				Thread.Sleep(500);
				_detectedFault = File.Exists(_cwLogFile);

				if (!_detectedFault.Value)
				{
					if (_faultOnEarlyExit && _faultExitEarly)
						_detectedFault = true;
					else if (_faultExitFail)
						_detectedFault = true;
				}
			}

			return _detectedFault.Value;
		}

		public override Fault GetMonitorData()
		{
			if (!DetectedFault())
				return null;

			Fault fault = new Fault();
			fault.detectionSource = "CrashWrangler";
			fault.type = FaultType.Fault;

			if (File.Exists(_cwLogFile))
			{
				string log = File.ReadAllText(_cwLogFile);
				fault.description = GenerateSummary(log);
				fault.collectedData["Log"] = File.ReadAllBytes(_cwLogFile);
				fault.folderName = "CrashWrangler";
			}
			else if (!_faultExitFail)
			{
				fault.title = "Process exited early";
				fault.description = "Process exited early: " + _command + " " + _arguments;
				fault.folderName = "ProcessExitedEarly";
			}
			else
			{
				fault.title = "Process did not exit in " + _waitForExitTimeout + "ms";
				fault.description = fault.title + ": " + _command + " " + _arguments;
				fault.folderName = "ProcessFailedToExit";
			}
			return fault;
		}

		public override bool MustStop()
		{
			return false;
		}

		public override void StopMonitor()
		{
			_StopProcess();
		}

		public override void SessionStarting()
		{
			if (_startOnCall == null)
				_StartProcess();
		}

		public override void SessionFinished()
		{
			_StopProcess();
		}

		public override bool IterationFinished()
		{
			if (!_messageExit && _faultOnEarlyExit && !_IsProcessRunning())
			{
				_faultExitEarly = true;
				_StopProcess();
			}
			else if (_startOnCall != null)
			{
				_WaitForExit(true);
				_StopProcess();
			}
			else if (_restartOnEachTest)
			{
				_StopProcess();
			}

			return false;
		}

		public override Variant Message(string name, Variant data)
		{
			if (name == "Action.Call" && ((string)data) == _startOnCall)
			{
				_StopProcess();
				_StartProcess();
				return null;
			}
			else if (name == "Action.Call" && ((string)data) == _waitForExitOnCall)
			{
				_messageExit = true;
				_WaitForExit(false);
				_StopProcess();
			}

			return null;
		}

		private ulong _GetTotalCputime(Proc p)
		{
			try
			{
				return ProcessInfo.Instance.Snapshot(p).TotalProcessorTicks;
			}
			catch
			{
				return 0;
			}
		}

		private bool _IsProcessRunning()
		{
			return _procCommand != null && !_procCommand.HasExited && !_IsZombie(_procCommand);
		}

		private bool _IsZombie(Proc p)
		{
			try
			{
				return !ProcessInfo.Instance.Snapshot(p).Responding;
			}
			catch
			{
				return false;
			}
		}

		private bool _CommandExists()
		{
			using (var p = new Proc())
			{
				p.StartInfo = new ProcessStartInfo("which", "-s \"" + _command + "\"");
				p.Start();
				p.WaitForExit();
				return p.ExitCode == 0;
			}
		}

		private void _StartProcess()
		{
			if (!_CommandExists())
				throw new PeachException("CrashWrangler: Could not find command \"" + _command + "\"");

			if (File.Exists(_cwPidFile))
				File.Delete(_cwPidFile);

			if (File.Exists(_cwLogFile))
				File.Delete(_cwLogFile);

			if (File.Exists(_cwLockFile))
				File.Delete(_cwLockFile);

			var si = new ProcessStartInfo();
			si.FileName = _execHandler;
			si.Arguments = _command + (_arguments.Length == 0 ? "" : " ") + _arguments;
			si.UseShellExecute = false;

			foreach (DictionaryEntry de in Environment.GetEnvironmentVariables())
				si.EnvironmentVariables[de.Key.ToString()] = de.Value.ToString();

			si.EnvironmentVariables["CW_LOG_PATH"] = _cwLogFile;
			si.EnvironmentVariables["CW_PID_FILE"] = _cwPidFile;
			si.EnvironmentVariables["CW_LOCK_FILE"] = _cwLockFile;
			si.EnvironmentVariables["CW_USE_GMAL"] = _useDebugMalloc ? "1" : "0";
			si.EnvironmentVariables["CW_EXPLOITABLE_READS"] = _exploitableReads ? "1" : "0";

			_procHandler = new Proc();
			_procHandler.StartInfo = si;

			try
			{
				_procHandler.Start();
			}
			catch (Win32Exception ex)
			{

				string err = GetLastError(ex.NativeErrorCode);
				throw new PeachException(string.Format("CrashWrangler: Could not start handler \"{0}\" - {1}", _execHandler, err), ex);
			}

			_totalProcessorTime = 0;

			// Wait for pid file to exist, open it up and read it
			while (!File.Exists(_cwPidFile) && !_procHandler.HasExited)
				Thread.Sleep(250);

			string strPid = File.ReadAllText(_cwPidFile);
			int pid = Convert.ToInt32(strPid);

			try
			{
				_procCommand = Proc.GetProcessById(pid);
			}
			catch (ArgumentException ex)
			{
				if (!_procHandler.HasExited)
					throw new PeachException("CrashWrangler: Could not open handle to command \"" + _command + "\" with pid \"" + pid + "\"", ex);

				var ret = _procHandler.ExitCode;
				var log = File.Exists(_cwLogFile);

				// If the exit code non-zero and no log means it was unable to run the command
				if (ret != 0 && !log)
					throw new PeachException("CrashWrangler: Handler could not run command \"" + _command + "\"", ex);

				// If the exit code is 0 or there is a log, the program ran to completion
				if (_procCommand != null)
				{
					_procCommand.Close();
					_procCommand = null;
				}
			}
		}

		private void _StopProcess()
		{
			if (_procHandler == null)
				return;

			// Ensure a crash report is not being generated
			while (File.Exists(_cwLockFile))
				Thread.Sleep(250);

			// Killing _procCommand will cause _procHandler to exit
			// _procCommand might not exist if the program ran to completion
			// prior to opening a handle to the pid
			if (_procCommand != null)
			{
				if (!_procCommand.HasExited)
				{
					_procCommand.CloseMainWindow();
					_procCommand.WaitForExit(500);

					if (!_procCommand.HasExited)
					{
						try
						{
							_procCommand.Kill();
						}
						catch (InvalidOperationException)
						{
							// Already exited between HasEcited and Kill()
						}
						_procCommand.WaitForExit();
					}
				}

				_procCommand.Close();
				_procCommand = null;
			}

			if (!_procHandler.HasExited)
			{
				_procHandler.WaitForExit();
			}

			_procHandler.Close();
			_procHandler = null;
		}

		private void _WaitForExit(bool useCpuKill)
		{
			const int pollInterval = 200;
			int i = 0;

			if (!_IsProcessRunning())
				return;

			if (useCpuKill && !_noCpuKill)
			{
				ulong lastTime = 0;

				for (i = 0; i < _waitForExitTimeout; i += pollInterval)
				{
					var currTime = _GetTotalCputime(_procCommand);

					if (i != 0 && lastTime == currTime)
						break;

					lastTime = currTime;
					Thread.Sleep(pollInterval);
				}

				_StopProcess();
			}
			else
			{
				// For some reason, Process.WaitForExit is causing a SIGTERM
				// to be delivered to the process. So we poll instead.

				if (_waitForExitTimeout >= 0)
				{
					for (i = 0; i < _waitForExitTimeout; i += pollInterval)
					{
						if (!_IsProcessRunning())
							break;

						Thread.Sleep(pollInterval);
					}

					if (i >= _waitForExitTimeout && !useCpuKill)
					{
						_detectedFault = true;
						_faultExitFail = true;
					}
				}
				else
				{
					while (_IsProcessRunning())
						Thread.Sleep(pollInterval);
				}
			}
		}

		private static string GenerateSummary(string file)
		{
			StringBuilder summary = new StringBuilder();

			if (file.Contains(":is_exploitable=no:"))
				summary.Append("NotExploitable");
			else if (file.Contains(":is_exploitable=yes:"))
				summary.Append("Exploitable");
			else
				summary.Append("Unknown");

			if (file.Contains("exception=EXC_BAD_ACCESS:"))
			{
				summary.Append("_BadAccess");

				if (file.Contains(":access_Type=read:"))
					summary.Append("_Read");
				else if (file.Contains(":access_Type=write:"))
					summary.Append("_Write");
				else if (file.Contains(":access_Type=exec:"))
					summary.Append("_Exec");
				else if (file.Contains(":access_Type=recursion:"))
					summary.Append("_Recursion");
				else if (file.Contains(":access_Type=unknown:"))
					summary.Append("_Unknown");
			}
			else if (file.Contains("exception=EXC_BAD_INSTRUCTION:"))
				summary.Append("_BadInstruction");
			else if (file.Contains("exception=EXC_ARITHMETIC:"))
				summary.Append("_Arithmetic");
			else if (file.Contains("exception=EXC_CRASH:"))
				summary.Append("_Crash");

			Regex reTid = new Regex(@"^Crashed Thread:\s+(\d+)", RegexOptions.Multiline);
			Match mTid = reTid.Match(file);
			if (mTid.Success)
			{
				string tid = mTid.Groups[1].Value;

				string strReAddr = @"^Thread " + tid + @" Crashed:.*\n((\d+.*\s(?<addr>0x[0-9,a-f,A-F]+)\s.*\n)+)";
				Regex reAddr = new Regex(strReAddr, RegexOptions.Multiline);
				Match mAddr = reAddr.Match(file);
				if (mAddr.Success)
				{
					var captures = mAddr.Groups["addr"].Captures;
					for (int i = captures.Count - 1; i >= 0; --i)
						summary.Append("_" + captures[i].Value);
				}
			}

			return summary.ToString();
		}

		private static string GetLastError(int err)
		{
			IntPtr ptr = strerror(err);
			string ret = Marshal.PtrToStringAnsi(ptr);
			return ret;
		}

		[DllImport("libc")]
		private static extern IntPtr strerror(int err);
	}
}

// end
