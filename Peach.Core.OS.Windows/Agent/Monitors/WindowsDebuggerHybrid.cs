
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
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;

using Peach.Core.Dom;
using Peach.Core.Agent.Monitors.WindowsDebug;

using NLog;

/* Code to determine if an exe is 32/64bit.  Can be used to locate correct windbg.
 * 
 * //I added FileAccess.Read to your FileStream instantiation - otherwise it blows us when trying to determine bitness of DLLs in either C:\Windows or C:\Program Files – AngryHacker Aug 26 '11 at 17:45

 *
 */

namespace Peach.Core.Agent.Monitors
{
	[Monitor("WindowsDebugger", true)]
	[Monitor("WindowsDebuggerHybrid")]
	[Monitor("WindowsDebugEngine")]
	[Monitor("debugger.WindowsDebugEngine")]
	[Parameter("CommandLine", typeof(string), "Command line of program to start.", "")]
	[Parameter("ProcessName", typeof(string), "Name of process to attach too.", "")]
	[Parameter("KernelConnectionString", typeof(string), "Connection string for kernel debugging.", "")]
	[Parameter("Service", typeof(string), "Name of Windows Service to attach to.  Service will be started if stopped or crashes.", "")]
	[Parameter("SymbolsPath", typeof(string), "Optional Symbol path.  Default is Microsoft public symbols server.", "SRV*http://msdl.microsoft.com/download/symbols")]
	[Parameter("WinDbgPath", typeof(string), "Path to WinDbg install.  If not provided we will try and locate it.", "")]
	[Parameter("StartOnCall", typeof(string), "Indicate the debugger should wait to start or attach to process until notified by state machine.", "")]
	[Parameter("IgnoreFirstChanceGuardPage", typeof(string), "Ignore first chance guard page faults.  These are sometimes false posistives or anti-debugging faults.", "false")]
	[Parameter("IgnoreSecondChanceGuardPage", typeof(string), "Ignore second chance guard page faults.  These are sometimes false posistives or anti-debugging faults.", "false")]
	[Parameter("NoCpuKill", typeof(string), "Don't use process CPU usage to terminate early.", "false")]
	[Parameter("FaultOnEarlyExit", typeof(bool), "Trigger fault if process exists (defaults to false)", "false")]
	[Parameter("WaitForExitOnCall", typeof(string), "Wait for process to exit on state model call and fault if timeout is reached", "")]
	[Parameter("WaitForExitTimeout", typeof(int), "Wait for exit timeout value in milliseconds (-1 is infinite)", "10000")]
	[Parameter("RestartOnEachTest", typeof(bool), "Restart process for each interation", "false")]
	public class WindowsDebuggerHybrid : Monitor
	{
		protected static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		string _name = null;
		string _commandLine = null;
		string _processName = null;
		string _kernelConnectionString = null;
		string _service = null;

		string _winDbgPath = null;
		string _symbolsPath = "SRV*http://msdl.microsoft.com/download/symbols";
		string _waitForExitOnCall = null;
		string _startOnCall = null;

		int _waitForExitTimeout = 10000;

		bool _ignoreFirstChanceGuardPage = false;
		bool _ignoreSecondChanceGuardPage = false;
		bool _noCpuKill = false;
		bool _faultOnEarlyExit = false;
		bool _restartOnEachTest = false;

		bool _waitForExitFailed = false;
		bool _earlyExitFault = false;
		bool _stopMessage = false;
		bool _hybrid = true;
		bool _replay = false;
		Fault _fault = null;

		DebuggerInstance _debugger = null;
		SystemDebuggerInstance _systemDebugger = null;
		IpcChannel _ipcChannel = null;

		public WindowsDebuggerHybrid(IAgent agent, string name, Dictionary<string, Variant> args)
			: base(agent, name, args)
		{
			_name = name;

			//var color = Console.ForegroundColor;
			if (!Environment.Is64BitProcess && Environment.Is64BitOperatingSystem)
			{
				//Console.ForegroundColor = ConsoleColor.Yellow;
				//Console.WriteLine("\nError: Cannot use the 32bit version of Peach 3 on a 64bit operating system.");
				//Console.ForegroundColor = color;
				//return;
				throw new PeachException("Error: Cannot use the 32bit version of Peach 3 on a 64bit operating system.");
			}
			else if (Environment.Is64BitProcess && !Environment.Is64BitOperatingSystem)
			{
				//Console.ForegroundColor = ConsoleColor.Yellow;
				//Console.WriteLine("\nError: Cannot use the 64bit version of Peach 3 on a 32bit operating system.");
				//Console.ForegroundColor = color;

				throw new PeachException("Error: Cannot use the 64bit version of Peach 3 on a 32bit operating system.");
			}

			if (args.ContainsKey("CommandLine"))
				_commandLine = (string)args["CommandLine"];
			else if (args.ContainsKey("ProcessName"))
				_processName = (string)args["ProcessName"];
			else if (args.ContainsKey("KernelConnectionString"))
			{
				_hybrid = false;
				_kernelConnectionString = (string)args["KernelConnectionString"];
			}
			else if (args.ContainsKey("Service"))
				_service = (string)args["Service"];
			else
				throw new PeachException("Error, WindowsDebugEngine started with out a CommandLine, ProcessName, KernelConnectionString or Service parameter.");

			if (args.ContainsKey("SymbolsPath"))
				_symbolsPath = (string)args["SymbolsPath"];
			if (args.ContainsKey("StartOnCall"))
				_startOnCall = (string)args["StartOnCall"];
			if (args.ContainsKey("WaitForExitOnCall"))
				_waitForExitOnCall = (string)args["WaitForExitOnCall"];
			if (args.ContainsKey("WaitForExitTimeout") && !int.TryParse((string)args["WaitForExitTimeout"], out _waitForExitTimeout))
				throw new PeachException("Error, 'WaitForExitTimeout' is not a valid number.");

			if (args.ContainsKey("WinDbgPath"))
			{
				_winDbgPath = (string)args["WinDbgPath"];

				var type = GetDllMachineType(Path.Combine(_winDbgPath, "dbgeng.dll"));
				if (Environment.Is64BitProcess && type != MachineType.IMAGE_FILE_MACHINE_AMD64)
					throw new PeachException("Error, provided WinDbgPath is not x64.");
				else if (!Environment.Is64BitProcess && type != MachineType.IMAGE_FILE_MACHINE_I386)
					throw new PeachException("Error, provided WinDbgPath is not x86.");
			}
			else
			{
				_winDbgPath = FindWinDbg();
				if (_winDbgPath == null)
					throw new PeachException("Error, unable to locate WinDbg, please specify using 'WinDbgPath' parameter.");
			}

			if (args.ContainsKey("RestartOnEachTest") && ((string)args["RestartOnEachTest"]).ToLower() == "true")
				_restartOnEachTest = true;
			if (args.ContainsKey("IgnoreFirstChanceGuardPage") && ((string)args["IgnoreFirstChanceGuardPage"]).ToLower() == "true")
				_ignoreFirstChanceGuardPage = true;
			if (args.ContainsKey("IgnoreSecondChanceGuardPage") && ((string)args["IgnoreSecondChanceGuardPage"]).ToLower() == "true")
				_ignoreSecondChanceGuardPage = true;
			if (args.ContainsKey("NoCpuKill") && ((string)args["NoCpuKill"]).ToLower() == "true")
				_noCpuKill = true;
			if (args.ContainsKey("FaultOnEarlyExit") && ((string)args["FaultOnEarlyExit"]).ToLower() == "true")
				_faultOnEarlyExit = true;

			// Register IPC Channel for connecting to debug process
			//_ipcChannel = new IpcChannel("Peach.Core_" + (new Random().Next().ToString()));
			//ChannelServices.RegisterChannel(_ipcChannel, false);
		}

		/// <summary>
		/// Make sure we clean up debuggers
		/// </summary>
		~WindowsDebuggerHybrid()
		{
			_FinishDebugger();
		}

		public override object ProcessQueryMonitors(string query)
		{
			switch (query)
			{
				case "QueryPid":
					if (_kernelConnectionString != null)
						return null;

					if (_debugger != null)
						return _debugger.ProcessId;
					else if (_systemDebugger != null)
						return _systemDebugger.ProcessId;
					else
						return null;
			}

			return null;
		}

		public static string FindWinDbg()
		{
			// Lets try a few common places before failing.
			List<string> pgPaths = new List<string>();
			pgPaths.Add(@"c:\");
			pgPaths.Add(Environment.GetEnvironmentVariable("SystemDrive"));
			pgPaths.Add(Environment.GetEnvironmentVariable("ProgramFiles"));

			if (Environment.GetEnvironmentVariable("ProgramW6432") != null)
				pgPaths.Add(Environment.GetEnvironmentVariable("ProgramW6432"));
			if (Environment.GetEnvironmentVariable("ProgramFiles") != null)
				pgPaths.Add(Environment.GetEnvironmentVariable("ProgramFiles"));
			if (Environment.GetEnvironmentVariable("ProgramFiles(x86)") != null)
				pgPaths.Add(Environment.GetEnvironmentVariable("ProgramFiles(x86)"));

			List<string> dbgPaths = new List<string>();
			dbgPaths.Add("Debuggers");
			dbgPaths.Add("Debugger");
			dbgPaths.Add("Debugging Tools for Windows");
			dbgPaths.Add("Debugging Tools for Windows (x64)");
			dbgPaths.Add("Debugging Tools for Windows (x86)");
			dbgPaths.Add("Windows Kits\\8.0\\Debuggers\\x64");
			dbgPaths.Add("Windows Kits\\8.0\\Debuggers\\x86");

			foreach (string path in pgPaths)
			{
				foreach (string dpath in dbgPaths)
				{
					string pathCheck = Path.Combine(path, dpath);
					if (Directory.Exists(pathCheck) && File.Exists(Path.Combine(pathCheck, "dbgeng.dll")))
					{
						//verify x64 vs x86

						var type = GetDllMachineType(Path.Combine(pathCheck, "dbgeng.dll"));
						if (Environment.Is64BitProcess && type != MachineType.IMAGE_FILE_MACHINE_AMD64)
							continue;
						else if (!Environment.Is64BitProcess && type != MachineType.IMAGE_FILE_MACHINE_I386)
							continue;

						return pathCheck;
					}
				}
			}

			return null;
		}

		public override Variant Message(string name, Variant data)
		{
			if (name == "Action.Call" && ((string)data) == _startOnCall)
			{
				_StopDebugger();
				_StartDebugger();
			}
			else if (name == "Action.Call" && ((string)data) == _waitForExitOnCall)
			{
				_stopMessage = true;
				_WaitForExit(false);
				_StopDebugger();
			}
			else
			{
				logger.Debug("Unknown msg: " + name + " data: " + (string)data);
			}

			return null;
		}

		public override void StopMonitor()
		{
			_StopDebugger();
			_FinishDebugger();

			if(_ipcChannel != null)
				ChannelServices.UnregisterChannel(_ipcChannel);
			
			_ipcChannel = null;
			_debugger = null;
			_systemDebugger = null;
		}

		public override void SessionStarting()
		{
			if (_startOnCall != null)
				return;

			_StartDebugger();
		}

		public override void SessionFinished()
		{
			_StopDebugger();
			_FinishDebugger();
		}

		public override void IterationStarting(uint iterationCount, bool isReproduction)
		{
			_replay = isReproduction;
			_waitForExitFailed = false;
			_earlyExitFault = false;
			_stopMessage = false;

			if (!_IsDebuggerRunning() && _startOnCall == null)
				_StartDebugger();
		}

		public override bool IterationFinished()
		{
			if (!_stopMessage && _faultOnEarlyExit && !_IsDebuggerRunning())
			{
				_earlyExitFault = true;
				_StopDebugger();
			}
			else if (_startOnCall != null)
			{
				_WaitForExit(true);
				_StopDebugger();
			}
			else if (_restartOnEachTest)
			{
				_StopDebugger();
			}

			return false;
		}

		public override bool DetectedFault()
		{
			logger.Info("DetectedFault()");

			_fault = null;

			if (_systemDebugger != null && _systemDebugger.caughtException)
			{
				logger.Info("DetectedFault - Using system debugger, caught exception");
				_fault = _systemDebugger.crashInfo;

				_systemDebugger.StopDebugger();
				_systemDebugger = null;
			}
			else if (_debugger != null && _hybrid)
			{
				logger.Info("DetectedFault - Using WinDbg, checking for fault");

				// Lets give windbg a chance to detect the crash.
				// 10 seconds should be enough.
				for (int i = 0; i < 10; i++)
				{
					if (_debugger != null && _debugger.caughtException)
					{
						// Kill off our debugger process and re-create
						_debuggerProcessUsage = _debuggerProcessUsageMax;
						_fault = _debugger.crashInfo;
						break;
					}

					Thread.Sleep(1000);
				}

				if (_fault == null && _earlyExitFault)
					_fault = GetEarlyExitFault();

				if(_fault != null)
					logger.Info("DetectedFault - Caught fault with windbg");

				if (_debugger != null && _hybrid && _fault == null)
				{
					_StopDebugger();
					_FinishDebugger();
				}
			}
			else if (_debugger != null && _debugger.caughtException)
			{
				// Kill off our debugger process and re-create
				_debuggerProcessUsage = _debuggerProcessUsageMax;
				_fault = _debugger.crashInfo;
			}
			else if (_earlyExitFault)
			{
				logger.Info("DetectedFault() - Fault detected, process exited early");
				_fault = GetEarlyExitFault();
			}
			else if (_waitForExitFailed)
			{
				logger.Info("DetectedFault() - Fault detected, WaitForExitOnCall failed");
				_fault = GetGeneralFault("ProcessFailedToExit", "Process did not exit in " + _waitForExitTimeout + "ms");
			}

			if(_fault == null)
				logger.Info("DetectedFault() - No fault detected");

			return _fault != null;
		}

		public override Fault GetMonitorData()
		{
			if (_fault != null && _hybrid)
			{
				_StopDebugger();
				_FinishDebugger();
			}

			return _fault;
		}

		public override bool MustStop()
		{
			return false;
		}

		protected Fault GetEarlyExitFault()
		{
			return GetGeneralFault("ProcessExitedEarly", "Process exited early");
		}

		protected Fault GetGeneralFault(string folder, string reason)
		{
			var fault = new Fault();
			fault.type = FaultType.Fault;
			fault.detectionSource = _systemDebugger != null ? "SystemDebugger" : "WindowsDebugEngine";
			fault.title = reason;
			fault.description = reason + ": ";

			if (_processName != null)
				fault.description += _processName;
			else if (_commandLine != null)
				fault.description += _commandLine;
			else if (_kernelConnectionString != null)
				fault.description += _kernelConnectionString;
			else if (_service != null)
				fault.description += _service;

			fault.folderName = folder;

			return fault;
		}

		protected bool _IsDebuggerRunning()
		{
			if (_systemDebugger != null && _systemDebugger.IsRunning)
				return true;

			if (_debugger != null && _debugger.IsRunning)
				return true;

			return false;
		}

		System.Diagnostics.Process _debuggerProcess = null;
		int _debuggerProcessUsage = 0;
		int _debuggerProcessUsageMax = 100;
		string _debuggerChannelName = null;

		protected void _StartDebugger()
		{
			if (_hybrid)
				_StartDebuggerHybrid();
			else
				_StartDebuggerNonHybrid();
		}

		/// <summary>
		/// The hybrid mode uses both WinDbg and System Debugger
		/// </summary>
		/// <remarks>
		/// When _hybrid == true &amp;&amp; _replay == false we will use the
		/// System Debugger.
		/// 
		/// When we hit a fault in _hybrid mode we will replay with windbg.
		/// 
		/// When _hybrid == false we will just use windbg.
		/// </remarks>
		protected void _StartDebuggerHybrid()
		{
			if (_replay)
			{
				_StartDebuggerHybridReplay();
				return;
			}

			// Start system debugger
			if (_systemDebugger == null || !_systemDebugger.IsRunning)
			{
				if (_systemDebugger == null)
				{
					_systemDebugger = new SystemDebuggerInstance();

					_systemDebugger.commandLine = _commandLine;
					_systemDebugger.processName = _processName;
					_systemDebugger.service = _service;
					_systemDebugger.ignoreFirstChanceGuardPage = _ignoreFirstChanceGuardPage;
					_systemDebugger.ignoreSecondChanceGuardPage = _ignoreSecondChanceGuardPage;
					_systemDebugger.noCpuKill = _noCpuKill;
				}

				_systemDebugger.StopDebugger();
				_systemDebugger.StartDebugger();
			}
		}

		static int ipcChannelCount = 0;

		/// <summary>
		/// Hybrid replay mode uses windbg
		/// </summary>
		protected void _StartDebuggerHybridReplay()
		{
			if (_debuggerProcess == null || _debuggerProcess.HasExited)
			{
				using (var p = System.Diagnostics.Process.GetCurrentProcess())
				{
					_debuggerChannelName = "PeachCore_" + p.Id + "_" + (ipcChannelCount++);
				}

				// Launch the server process
				_debuggerProcess = new System.Diagnostics.Process();
				_debuggerProcess.StartInfo.CreateNoWindow = true;
				_debuggerProcess.StartInfo.UseShellExecute = false;
				_debuggerProcess.StartInfo.Arguments = _debuggerChannelName;
				_debuggerProcess.StartInfo.FileName = Path.Combine(
					Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
					"Peach.Core.WindowsDebugInstance.exe");
				_debuggerProcess.Start();

				// Let the process get started.
				Thread.Sleep(2000);
			}

			// Try and create instance over IPC.  We will continue trying for 1 minute.

			DateTime startTimer = DateTime.Now;
			while (true)
			{
				try
				{
					_debugger = (DebuggerInstance)Activator.GetObject(typeof(DebuggerInstance),
						"ipc://" + _debuggerChannelName + "/DebuggerInstance");

					_debugger.commandLine = _commandLine;
					_debugger.processName = _processName;
					_debugger.kernelConnectionString = _kernelConnectionString;
					_debugger.service = _service;
					_debugger.symbolsPath = _symbolsPath;
					_debugger.startOnCall = _startOnCall;
					_debugger.ignoreFirstChanceGuardPage = _ignoreFirstChanceGuardPage;
					_debugger.ignoreSecondChanceGuardPage = _ignoreSecondChanceGuardPage;
					_debugger.noCpuKill = _noCpuKill;
					_debugger.winDbgPath = _winDbgPath;

					break;
				}
				catch
				{
					if ((DateTime.Now - startTimer).Minutes >= 1)
					{
						_debuggerProcess.Kill();
						_debuggerProcess = null;
						throw;
					}
				}
			}

			_debugger.StartDebugger();
		}

		/// <summary>
		/// Origional non-hybrid windbg only mode
		/// </summary>
		protected void _StartDebuggerNonHybrid()
		{
			if (_debuggerProcessUsage >= _debuggerProcessUsageMax && _debuggerProcess != null)
			{
				_FinishDebugger();

				_debuggerProcessUsage = 0;
			}

			if (_debuggerProcess == null || _debuggerProcess.HasExited)
			{
				_debuggerChannelName = "PeachCore_" + (new Random((uint)Environment.TickCount).NextUInt32().ToString());

				// Launch the server process
				_debuggerProcess = new System.Diagnostics.Process();
				_debuggerProcess.StartInfo.CreateNoWindow = true;
				_debuggerProcess.StartInfo.UseShellExecute = false;
				_debuggerProcess.StartInfo.Arguments = _debuggerChannelName;
				_debuggerProcess.StartInfo.FileName = Path.Combine(
					Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
					"Peach.Core.WindowsDebugInstance.exe");
				_debuggerProcess.Start();
			}

			_debuggerProcessUsage++;

			// Try and create instance over IPC.  We will continue trying for 1 minute.

			DateTime startTimer = DateTime.Now;
			while (true)
			{
				try
				{
					_debugger = (DebuggerInstance)Activator.GetObject(typeof(DebuggerInstance),
						"ipc://" + _debuggerChannelName + "/DebuggerInstance");
					//_debugger = new DebuggerInstance();

					_debugger.commandLine = _commandLine;
					_debugger.processName = _processName;
					_debugger.kernelConnectionString = _kernelConnectionString;
					_debugger.service = _service;
					_debugger.symbolsPath = _symbolsPath;
					_debugger.startOnCall = _startOnCall;
					_debugger.ignoreFirstChanceGuardPage = _ignoreFirstChanceGuardPage;
					_debugger.ignoreSecondChanceGuardPage = _ignoreSecondChanceGuardPage;
					_debugger.noCpuKill = _noCpuKill;
					_debugger.winDbgPath = _winDbgPath;

					break;
				}
				catch
				{
					if ((DateTime.Now - startTimer).Minutes >= 1)
					{
						_debuggerProcess.Kill();
						_debuggerProcess.Close();
						throw;
					}
				}
			}

			_debugger.StartDebugger();
		}

		protected void _FinishDebugger()
		{
			_StopDebugger();

			if (_systemDebugger != null)
			{
				try
				{
					_systemDebugger.FinishDebugging();
				}
				catch
				{
				}
			}

			if (_debugger != null)
			{
				try
				{
					_debugger.FinishDebugging();
				}
				catch
				{
				}
			}

			_debugger = null;
			_systemDebugger = null;

			if (_debuggerProcess != null)
			{
				try
				{
					if(!_debuggerProcess.WaitForExit(2000))
						_debuggerProcess.Kill();
				}
				catch
				{
				}

				_debuggerProcess.Close();
				_debuggerProcess = null;
			}
		}

		protected void _StopDebugger()
		{
			if (_systemDebugger != null)
			{
				try
				{
					_systemDebugger.StopDebugger();
				}
				catch
				{
				}
			}

			if (_debugger != null)
			{
				try
				{
					_debugger.StopDebugger();
				}
				catch
				{
				}
			}
		}

		protected void _WaitForExit(bool useCpuKill)
		{
			if (!_IsDebuggerRunning())
				return;

			try
			{
				int pid = _debugger != null ? _debugger.ProcessId : _systemDebugger.ProcessId;
				using (var proc = System.Diagnostics.Process.GetProcessById(pid))
				{
					if (proc == null || proc.HasExited)
						return;

					if (useCpuKill && !_noCpuKill)
					{
						const int pollInterval = 200;
						ulong lastTime = 0;
						int i = 0;

						for (i = 0; i < _waitForExitTimeout; i += pollInterval)
						{
							// Note: Performance counters were used and removed due to speed issues.
							//       monitoring the tick count is more reliable and less likely to cause
							//       fuzzing slow-downs.
							var pi = ProcessInfo.Instance.Snapshot(proc);

							logger.Trace("CpuKill: OldTicks={0} NewTicks={1}", lastTime, pi.TotalProcessorTicks);

							if (i != 0 && lastTime == pi.TotalProcessorTicks)
							{
								logger.Debug("Cpu is idle, stopping process.");
								break;
							}

							lastTime = pi.TotalProcessorTicks;
							Thread.Sleep(pollInterval);
						}

						if (i >= _waitForExitTimeout)
							logger.Debug("Timed out waiting for cpu idle, stopping process.");
					}
					else
					{
						logger.Debug("WaitForExit({0})", _waitForExitTimeout == -1 ? "INFINITE" : _waitForExitTimeout.ToString());
						if (!proc.WaitForExit(_waitForExitTimeout))
						{
							if (!useCpuKill)
							{
								logger.Debug("FAULT, WaitForExit ran out of time!");
								_waitForExitFailed = true;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				logger.Debug("_WaitForExit() failed: {0}", ex.Message);
			}
		}

		public static MachineType GetDllMachineType(string dllPath)
		{
			//see http://www.microsoft.com/whdc/system/platform/firmware/PECOFF.mspx
			//offset to PE header is always at 0x3C
			//PE header starts with "PE\0\0" =  0x50 0x45 0x00 0x00
			//followed by 2-byte machine type field (see document above for enum)
			FileStream fs = new FileStream(dllPath, FileMode.Open, FileAccess.Read);
			BinaryReader br = new BinaryReader(fs);
			fs.Seek(0x3c, SeekOrigin.Begin);
			Int32 peOffset = br.ReadInt32();
			fs.Seek(peOffset, SeekOrigin.Begin);
			UInt32 peHead = br.ReadUInt32();
			if (peHead != 0x00004550) // "PE\0\0", little-endian
				throw new Exception("Can't find PE header");
			MachineType machineType = (MachineType)br.ReadUInt16();
			br.Close();
			fs.Close();
			return machineType;
		}

		public enum MachineType : ushort
		{
			IMAGE_FILE_MACHINE_UNKNOWN = 0x0,
			IMAGE_FILE_MACHINE_AM33 = 0x1d3,
			IMAGE_FILE_MACHINE_AMD64 = 0x8664,
			IMAGE_FILE_MACHINE_ARM = 0x1c0,
			IMAGE_FILE_MACHINE_EBC = 0xebc,
			IMAGE_FILE_MACHINE_I386 = 0x14c,
			IMAGE_FILE_MACHINE_IA64 = 0x200,
			IMAGE_FILE_MACHINE_M32R = 0x9041,
			IMAGE_FILE_MACHINE_MIPS16 = 0x266,
			IMAGE_FILE_MACHINE_MIPSFPU = 0x366,
			IMAGE_FILE_MACHINE_MIPSFPU16 = 0x466,
			IMAGE_FILE_MACHINE_POWERPC = 0x1f0,
			IMAGE_FILE_MACHINE_POWERPCFP = 0x1f1,
			IMAGE_FILE_MACHINE_R4000 = 0x166,
			IMAGE_FILE_MACHINE_SH3 = 0x1a2,
			IMAGE_FILE_MACHINE_SH3DSP = 0x1a3,
			IMAGE_FILE_MACHINE_SH4 = 0x1a6,
			IMAGE_FILE_MACHINE_SH5 = 0x1a8,
			IMAGE_FILE_MACHINE_THUMB = 0x1c2,
			IMAGE_FILE_MACHINE_WCEMIPSV2 = 0x169,
		}
		// returns true if the dll is 64-bit, false if 32-bit, and null if unknown
		public static bool? UnmanagedDllIs64Bit(string dllPath)
		{
			switch (GetDllMachineType(dllPath))
			{
				case MachineType.IMAGE_FILE_MACHINE_AMD64:
				case MachineType.IMAGE_FILE_MACHINE_IA64:
					return true;
				case MachineType.IMAGE_FILE_MACHINE_I386:
					return false;
				default:
					return null;
			}
		}

	}
}

// end
