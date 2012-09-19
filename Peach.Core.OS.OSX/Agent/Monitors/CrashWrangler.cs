
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
using Proc = System.Diagnostics.Process;

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
	[Parameter("Command", typeof(string), "Command to execute", true)]
	[Parameter("Arguments", typeof(string), "Commad line arguments", false)]
	[Parameter("StartOnCall", typeof(string), "Start command on state model call", false)]
	[Parameter("UseDebugMalloc", typeof(bool), "Use OS X Debug Malloc (slower) (defaults to false)", false)]
	[Parameter("ExecHandler", typeof(string), "Crash Wrangler Execution Handler program.", true)]
	[Parameter("ExploitableReads", typeof(bool), "Are read a/v's considered exploitable? (defaults to false)", false)]
	[Parameter("NoCpuKill", typeof(bool), "Disable process killing by CPU usage? (defaults to false)", false)]
	[Parameter("CwLogFile", typeof(string), "CrashWrangler Log file (defaults to cw.log)", false)]
	[Parameter("CwLockFile", typeof(string), "CrashWRangler Lock file (defaults to cw.lock)", false)]
	[Parameter("CwPidFile", typeof(string), "CrashWrangler PID file (defaults to cw.pid)", false)]
	public class CrashWrangler : Monitor
	{
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
		protected TimeSpan _totalProcessorTime = TimeSpan.Zero;

		public CrashWrangler(string name, Dictionary<string, Variant> args)
			: base(name, args)
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
				// Give CrashWrangler a change to write the log
				Thread.Sleep(500);
				_detectedFault = File.Exists(_cwLogFile);
			}

			return _detectedFault.Value;
		}

		public override void GetMonitorData(System.Collections.Hashtable data)
		{
			if (!DetectedFault())
				return;

			data.Add("CrashWrangler", "TODO: Crash goes here!");
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
			// TODO: Need to query _procCommand!
			System.Diagnostics.Debug.Assert(_procHandler != null);

			var lastTime = _totalProcessorTime;
			_totalProcessorTime = _procHandler.TotalProcessorTime;

			return lastTime == _totalProcessorTime;
		}

		private bool _IsProcessRunning()
		{
			return _procHandler != null && !_procHandler.HasExited;
		}

		private void _StartProcess()
		{
			// Delete the lock, pid and log
			// Start exc_handler, it will write the pid of _command to the pid file

			System.Diagnostics.Debug.Assert(_procHandler == null);
			System.Diagnostics.Debug.Assert(_procCommand == null);

			var si = new ProcessStartInfo();
			si.FileName = _execHandler;
			si.Arguments = _command + " " + _arguments;
			si.UseShellExecute = false;
			si.EnvironmentVariables["PATH"] = Environment.CurrentDirectory + Path.PathSeparator + Environment.GetEnvironmentVariable("PATH");
			si.EnvironmentVariables["CW_LOG_PATH"] = _cwLogFile;
			si.EnvironmentVariables["CW_PID_FILE"] = _cwPidFile;
			si.EnvironmentVariables["CW_LOCK_FILE"] = _cwLockFile;
			si.EnvironmentVariables["CW_USE_GMAL"] = _useDebugMalloc ? "1" : "0";
			si.EnvironmentVariables["CW_EXPLOITABLE_READS"] = _exploitableReads ? "1" : "0";

			_procHandler = new Proc();
			_procHandler.StartInfo = si;
			_procHandler.Start();

			_totalProcessorTime = TimeSpan.Zero;

			// Wait for pid file to exist, open it up and read it
		}

		private void _StopProcess()
		{
			// Check if _procCommand is running
			// If so, wait for _cwLockFile to not exist before killing
			// Then stop _procHandler
			if (_procHandler == null)
				return;

			// Try and exit gracefully
			if (!_procHandler.HasExited)
			{
				_procHandler.CloseMainWindow();
				_procHandler.WaitForExit(500);

				// Kill forcefully
				if (!_procHandler.HasExited)
				{
					_procHandler.Kill();
					_procHandler.WaitForExit();
				}
			}

			_procHandler.Close();
			_procHandler = null;
		}
	}
}

// end
