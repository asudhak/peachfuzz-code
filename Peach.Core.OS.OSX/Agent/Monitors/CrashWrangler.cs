
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
	}

	/// <summary>
	/// Monitor will use OS X's built in CrashReporter (similar to watson)
	/// to detect and report crashes.
	/// </summary>
	[Monitor("CrashWrangler")]
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
	public class CrashWrangler : Monitor
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		protected string _command = null;
		protected string _arguments = null;
		protected string _startOnCall = null;
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
		}

		public override void IterationStarting(uint iterationCount, bool isReproduction)
		{
			_detectedFault = null;

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
			}

			return _detectedFault.Value;
		}

		public override Fault GetMonitorData()
		{
			if (!DetectedFault())
				return null;

			string log = File.ReadAllText(_cwLogFile);
			string summary = GenerateSummary(log);

			Fault fault = new Fault();
			fault.detectionSource = "CrashWrangler";
			fault.folderName = "CrashWrangler";
			fault.type = FaultType.Fault;
			fault.description = summary;
			fault.collectedData["Log"] = File.ReadAllBytes(_cwLogFile);
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
			if (_startOnCall != null)
				_StopProcess();

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

			if (name == "Action.Call.IsRunning" && ((string)data) == _startOnCall)
			{
				if (!_IsProcessRunning())
				{
					return new Variant(0);
				}

				if (!_noCpuKill && _IsIdleCpu())
				{
					_StopProcess();
					return new Variant(0);
				}
				
				return new Variant(1);
			}

			return null;
		}

		private bool _IsIdleCpu()
		{
			System.Diagnostics.Debug.Assert(_procCommand != null);

			var lastTime = _totalProcessorTime;

			_totalProcessorTime = GetTotalCputime(_procCommand);

			return _totalProcessorTime > 0 && lastTime == _totalProcessorTime;
		}

		private ulong GetTotalCputime(Proc p)
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
			return _procCommand != null && !_procCommand.HasExited && !IsZombie(_procCommand);
		}

		private bool IsZombie(Proc p)
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

		private void _StartProcess()
		{
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
				throw new PeachException(string.Format("CrashWrangler: Could not start handler \"{0}\" - {1}", _execHandler, err));
			}

			_totalProcessorTime = 0;

			// Wait for pid file to exist, open it up and read it
			while (!File.Exists(_cwPidFile) && !_procHandler.HasExited)
				Thread.Sleep(100);

			string strPid = File.ReadAllText(_cwPidFile);
			int pid = Convert.ToInt32(strPid);

			try
			{
				_procCommand = Proc.GetProcessById(pid);
			}
			catch (ArgumentException)
			{
				if (!_procHandler.HasExited)
					throw new PeachException("CrashWrangler: Could not open handle to command \"" + _command + "\" with pid \"" + pid + "\"");

				var ret = _procHandler.ExitCode;
				var log = File.Exists(_cwLogFile);

				// If the exit code non-zero and no log means it was unable to run the command
				if (ret != 0 && !log)
					throw new PeachException("CrashWrangler: Handler could not run command \"" + _command + "\"");

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
