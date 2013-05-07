using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Reflection;

using Peach.Core;
using Peach.Core.Agent;

using NLog;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Collections;
using System.Text.RegularExpressions;

namespace Peach.Core.OS.Linux.Agent.Monitors
{
	[Monitor("LinuxDebugger", true)]
	[Parameter("Executable", typeof(string), "Executable to launch")]
	[Parameter("Arguments", typeof(string), "Optional command line arguments", "")]
	[Parameter("GdbPath", typeof(string), "Path to gdb", "/usr/bin/gdb")]
	[Parameter("RestartOnEachTest", typeof(bool), "Restart process for each interation", "false")]
	[Parameter("FaultOnEarlyExit", typeof(bool), "Trigger fault if process exists", "false")]
	[Parameter("NoCpuKill", typeof(bool), "Disable process killing when CPU usage nears zero", "false")]
	[Parameter("StartOnCall", typeof(string), "Start command on state model call", "")]
	[Parameter("WaitForExitOnCall", typeof(string), "Wait for process to exit on state model call and fault if timeout is reached", "")]
	[Parameter("WaitForExitTimeout", typeof(int), "Wait for exit timeout value in milliseconds (-1 is infinite)", "10000")]
	public class LinuxDebugger : Peach.Core.Agent.Monitor
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		static readonly string template = @"
define log_if_crash
 if ($_thread != 0x00)
  printf ""Crash detected, running exploitable.\n""
  source {3}
  set logging overwrite on
  set logging redirect on
  set logging on {0}
  exploitable -v
  set logging off
 end
end

handle all nostop noprint
handle SIGSEGV EXC_BAD_ACCESS EXC_BAD_INSTRUCTION EXC_ARITHMETIC stop print

file {1}
set args {2}

start
python with open('{4}', 'w') as f: f.write(str(gdb.inferiors()[0].pid))
cont
log_if_crash
quit
";

		Process _procHandler;
		Process _procCommand;
		Fault _fault = null;
		bool _messageExit = false;
		string _exploitable = null;
		string _tmpPath = null;
		string _gdbCmd = null;
		string _gdbPid = null;
		string _gdbLog = null;

		Regex reHash = new Regex(@"^Hash: (\w+)\.(\w+)$", RegexOptions.Multiline);
		Regex reClassification = new Regex(@"^Exploitability Classification: (.*)$", RegexOptions.Multiline);
		Regex reDescription = new Regex(@"^Short description: (.*)$", RegexOptions.Multiline);
		Regex reOther = new Regex(@"^Other tags: (.*)$", RegexOptions.Multiline);

		public string GdbPath { get; private set; }
		public string Executable { get; private set; }
		public string Arguments { get; private set; }
		public bool RestartOnEachTest { get; private set; }
		public bool FaultOnEarlyExit { get; private set; }
		public bool NoCpuKill { get; private set; }
		public string StartOnCall { get; private set; }
		public string WaitForExitOnCall { get; private set; }
		public int WaitForExitTimeout { get; private set; }

		public LinuxDebugger(IAgent agent, string name, Dictionary<string, Variant> args)
			: base(agent, name, args)
		{
			ParameterParser.Parse(this, args);

			_exploitable = FindExploitable();
		}

		string FindExploitable()
		{
			var target = "gdb/exploitable/exploitable.py";

			var dirs = new List<string> {
				Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
				Directory.GetCurrentDirectory(),
			};

			string path = Environment.GetEnvironmentVariable("PATH");
			if (!string.IsNullOrEmpty(path))
				dirs.AddRange(path.Split(Path.PathSeparator));

			foreach (var dir in dirs)
			{
				string full = Path.Combine(dir, target);
				if (File.Exists(full))
					return full;
			};

			throw new PeachException("Error, LinuxDebugger could not find '" + target + "' in search path.");
		}

		void _Start()
		{
			var si = new ProcessStartInfo();
			si.FileName = GdbPath;
			si.Arguments = "-batch -n -x " + _gdbCmd;
			si.UseShellExecute = false;

			_procHandler = new System.Diagnostics.Process();
			_procHandler.StartInfo = si;

			logger.Debug("_Start(): Starting gdb process");

			if (File.Exists(_gdbLog))
				File.Delete(_gdbLog);

			if (File.Exists(_gdbPid))
				File.Delete(_gdbPid);

			try
			{
				_procHandler.Start();
			}
			catch (Exception ex)
			{
				_procHandler = null;
				throw new PeachException("Could not start debugger '" + GdbPath + "'.  " + ex.Message + ".", ex);
			}

			// Wait for pid file to exist, open it up and read it
			while (!File.Exists(_gdbPid) && !_procHandler.HasExited)
				Thread.Sleep(250);

			if (!File.Exists(_gdbPid) && _procHandler.HasExited)
				throw new PeachException("GDB was unable to start '" + Executable + "'.");

			string strPid = File.ReadAllText(_gdbPid);
			int pid = Convert.ToInt32(strPid);

			try
			{
				_procCommand = Process.GetProcessById(pid);
			}
			catch (ArgumentException)
			{
				// Program ran to completion
				_procCommand = null;
			}
		}

		void _Stop()
		{
			if (_procHandler == null)
				return;

			// Stopping procCommand will cause procHandler to exit
			if (_procCommand != null)
			{
				if (!_procCommand.HasExited)
				{
					logger.Debug("_Stop(): Stopping process");
					_procCommand.CloseMainWindow();
					_procCommand.WaitForExit(500);

					if (!_procCommand.HasExited)
					{
						try
						{
							logger.Debug("_Stop(): Killing process");
							_procCommand.Kill();
						}
						catch (InvalidOperationException)
						{
							// Already exited between HasExited and Kill()
						}
						_procCommand.WaitForExit();
					}
				}

				logger.Debug("_Stop(): Closing process");
				_procCommand.Close();
				_procCommand = null;
			}

			if (!_procHandler.HasExited)
			{
				logger.Debug("_Stop(): Waiting for gdb to complete");
				_procHandler.WaitForExit();
			}

			logger.Debug("_Stop(): Closing gdb");
			_procHandler.Close();
			_procHandler = null;
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
						var pi = ProcessInfo.Instance.Snapshot(_procCommand);

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

				if (!_procCommand.WaitForExit(WaitForExitTimeout))
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
			return _procCommand != null && !_procCommand.HasExited;
		}

		Fault MakeFault(string folder, string reason)
		{
			return new Fault()
			{
				type = FaultType.Fault,
				detectionSource = "LinuxDebugger",
				title = reason,
				description = "{0}: {1} {2}".Fmt(reason, Executable, Arguments),
				folderName = folder,
			};
		}

		[DllImport("libc", CharSet = CharSet.Ansi, SetLastError = true)]
		static extern IntPtr mkdtemp(StringBuilder template);

		string MakeTempDir()
		{
			StringBuilder dir = new StringBuilder(Path.Combine(Path.GetTempPath(), "gdb.XXXXXX"));
			IntPtr ptr = mkdtemp(dir);
			if (ptr == IntPtr.Zero)
				throw new Win32Exception(Marshal.GetLastWin32Error());

			return dir.ToString();
		}

		public override void IterationStarting(uint iterationCount, bool isReproduction)
		{
			_fault = null;
			_messageExit = false;

			if (RestartOnEachTest)
				_Stop();

			if (!_IsRunning() && StartOnCall == null)
				_Start();
		}

		public override bool DetectedFault()
		{
			if (!File.Exists(_gdbLog))
				return _fault != null;

			logger.Info("DetectedFault - Caught fault with gdb");

			_Stop();

			byte[] bytes = File.ReadAllBytes(_gdbLog);
			string output = Encoding.UTF8.GetString(bytes);

			_fault = new Fault();
			_fault.type = FaultType.Fault;
			_fault.detectionSource = "LinuxDebugger";

			var hash = reHash.Match(output);
			if (hash.Success)
			{
				_fault.majorHash = hash.Groups[1].Value;
				_fault.minorHash = hash.Groups[2].Value;
			}

			var exp = reClassification.Match(output);
			if (exp.Success)
				_fault.exploitability = exp.Groups[1].Value;

			var desc = reDescription.Match(output);
			if (desc.Success)
				_fault.title = desc.Groups[1].Value;

			var other = reOther.Match(output);
			if (other.Success)
				_fault.title += ", " + other.Groups[1].Value;

			_fault.collectedData["StackTrace.txt"] = bytes;
			_fault.description = output;

			return true;
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
			_tmpPath = MakeTempDir();
			_gdbCmd = Path.Combine(_tmpPath, "gdb.cmd");
			_gdbPid = Path.Combine(_tmpPath, "gdb.pid");
			_gdbLog = Path.Combine(_tmpPath, "gdb.log");

			string cmd = string.Format(template, _gdbLog, Executable, Arguments, _exploitable, _gdbPid);
			File.WriteAllText(_gdbCmd, cmd);

			logger.Debug("Wrote gdb commands to '{0}'", _gdbCmd);

			if (StartOnCall == null && !RestartOnEachTest)
				_Start();
		}

		public override void SessionFinished()
		{
			_Stop();

			Directory.Delete(_tmpPath, true);
		}

		public override bool IterationFinished()
		{
			if (!_messageExit && FaultOnEarlyExit && !_IsRunning())
			{
				_fault = MakeFault("ProcessExitedEarly", "Process exited early");
				_Stop();
			}
			else if (StartOnCall != null)
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
