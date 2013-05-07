
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
using System.Linq;
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
				fault.description = File.ReadAllText(_cwLogFile);
				fault.collectedData["StackTrace.txt"] = File.ReadAllBytes(_cwLogFile);

				var s = new Summary(fault.description);

				fault.majorHash = s.majorHash;
				fault.minorHash = s.minorHash;
				fault.title = s.title;
				fault.exploitability = s.exploitable;

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

			si.EnvironmentVariables["CW_NO_CRASH_REPORTER"] = "1";
			si.EnvironmentVariables["CW_QUIET"] = "1";
			si.EnvironmentVariables["CW_LOG_PATH"] = _cwLogFile;
			si.EnvironmentVariables["CW_PID_FILE"] = _cwPidFile;
			si.EnvironmentVariables["CW_LOCK_FILE"] = _cwLockFile;

			if (_useDebugMalloc)
				si.EnvironmentVariables["CW_USE_GMAL"] = "1";

			if (_exploitableReads)
				si.EnvironmentVariables["CW_EXPLOITABLE_READS"] = "1";

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

		private static string GetLastError(int err)
		{
			IntPtr ptr = strerror(err);
			string ret = Marshal.PtrToStringAnsi(ptr);
			return ret;
		}

		[DllImport("libc")]
		private static extern IntPtr strerror(int err);

		class Summary
		{
			public string majorHash { get; private set; }
			public string minorHash { get; private set; }
			public string title { get; private set; }
			public string exploitable { get; private set; }

			private static readonly string[] system_modules =
			{
				"libSystem.B.dylib",
				"libsystem_kernel.dylib",
				"libsystem_c.dylib",
				"com.apple.CoreFoundation",
				"libstdc++.6.dylib",
				"libobjc.A.dylib",
				"libgcc_s.1.dylib",
				"libgmalloc.dylib",
				"libc++abi.dylib",
				"modified_gmalloc.dylib", // Apple internal dylib
				"???",                    // For when it doesn't exist in a known module
			};

			private static readonly string[] offset_functions = 
			{
				"__memcpy",
				"__longcopy",
				"__memmove",
				"__bcopy",
				"__memset_pattern",
				"__bzero",
				"memcpy",
				"longcopy",
				"memmove",
				"bcopy",
				"bzero",
				"memset_pattern",
			};

			private const int major_depth = 5;

			public Summary(string log)
			{
				string is_exploitable = null;
				string access_type = null;
				string exception = null;

				exploitable = "UNKNOWN";

				var reProp = new Regex(@"^(((?<key>\w+)=(?<value>[^:]+):)+)$", RegexOptions.Multiline);
				var mProp = reProp.Match(log);
				if (mProp.Success)
				{
					var ti = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo;
					var keys = mProp.Groups["key"].Captures;
					var vals = mProp.Groups["value"].Captures;

					System.Diagnostics.Debug.Assert(keys.Count == vals.Count);

					for (int i = 0; i < keys.Count; ++i)
					{
						var key = keys[i].Value;
						var val = vals[i].Value;

						switch (key)
						{
							case "is_exploitable":
								is_exploitable = val.ToLower();
								break;
							case "exception":
								exception = string.Join("", val.ToLower().Split('_').Where(a => a != "exc").Select(a => ti.ToTitleCase(a)).ToArray());
								break;
							case "access_type":
								access_type = ti.ToTitleCase(val.ToLower());
								break;
						}
					}
				}

				title = string.Format("{0}{1}", access_type, exception);

				if (is_exploitable == null)
					exploitable = "UNKNOWN";
				else if (is_exploitable == "yes")
					exploitable = "EXPLOITABLE";
				else
					exploitable = "NOT_EXPLOITABLE";

				Regex reTid = new Regex(@"^Crashed Thread:\s+(\d+)", RegexOptions.Multiline);
				Match mTid = reTid.Match(log);
				if (!mTid.Success)
					return;

				string tid = mTid.Groups[1].Value;
				string strReAddr = @"^Thread " + tid + @" Crashed:.*\n((\d+\s+(?<file>\S*)\s+(?<addr>0x[0-9,a-f,A-F]+)\s(?<func>.+)\n)+)";
				Regex reAddr = new Regex(strReAddr, RegexOptions.Multiline);
				Match mAddr = reAddr.Match(log);
				if (!mAddr.Success)
					return;

				var files = mAddr.Groups["file"].Captures;
				var addrs = mAddr.Groups["addr"].Captures;
				var names = mAddr.Groups["func"].Captures;

				string maj = "";
				string min = "";
				int cnt = 0;

				for (int i = 0; i < files.Count; ++i)
				{
					var file = files[i].Value;
					var addr = addrs[i].Value;
					var name = names[i].Value;

					// Ignore certian system modules
					if (system_modules.Contains(file))
						continue;

					// When generating a signature, remove offsets for common functions
					string other = offset_functions.Where(a => name.StartsWith(a)).FirstOrDefault();
					if (other != null)
						addr = other;

					string sig = (cnt == 0 ? "" : ",") + addr;
					min += sig;

					if (++cnt <= major_depth)
						maj += sig;
				}

				// If we have no usable backtrace info, hash on the reProp line
				if (cnt == 0)
				{
					maj = mProp.Value;
					min = mProp.Value;
				}

				majorHash = Md5(maj);
				minorHash = Md5(min);
			}

			private static string Md5(string input)
			{
				using (var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider())
				{
					byte[] buf = Encoding.UTF8.GetBytes(input);
					byte[] final = md5.ComputeHash(buf);
					var sb = new StringBuilder();
					foreach (byte b in final)
						sb.Append(b.ToString("X2"));
					return sb.ToString();
				}
			}
		}
	}
}

// end
