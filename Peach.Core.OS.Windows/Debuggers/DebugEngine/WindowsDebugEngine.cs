
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Peach.Core.Debuggers.DebugEngine.Tlb;
using System.Runtime.InteropServices;
using System.Runtime;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Diagnostics;
using System.Threading;
using Peach.Core.Dom;

namespace Peach.Core.Debuggers.DebugEngine
{
	public class WindowsDebugEngine : IDisposable
	{
		private bool _disposed = false;

		public string winDbgPath = null;

		public IDebugClient5 dbgClient = null;
		public IDebugControl4 dbgControl = null;
		public IDebugSymbols3 dbgSymbols = null;
		public IDebugSystemObjects dbgSystemObjects = null;

		public bool skipFirstChanceGuardPageException = false;
		public bool skipSecondChangeGuardPageException = false;

		public int processId = -1;

		// IPC For EventCallbacks
		public EventWaitHandle loadModules = new EventWaitHandle(false, EventResetMode.ManualReset);
		public EventWaitHandle exitProcess = new EventWaitHandle(false, EventResetMode.ManualReset);
		public EventWaitHandle handlingException = new EventWaitHandle(false, EventResetMode.ManualReset);
		public EventWaitHandle handledException = new EventWaitHandle(false, EventResetMode.ManualReset);
		public EventWaitHandle exitDebugger = new EventWaitHandle(false, EventResetMode.ManualReset);

		public StringBuilder output = new StringBuilder();

		public Fault crashInfo = null;

		public delegate uint DebugCreate(
			ref Guid InterfaceId,
			[MarshalAs(UnmanagedType.IUnknown)] out object Interface);

		protected IntPtr hDll = IntPtr.Zero;
		protected IntPtr hProc = IntPtr.Zero;

		#region Kernel32 Imports

		/// <summary>
		/// To load the dll - dllFilePath dosen't have to be const - so I can read path from registry
		/// </summary>
		/// <param name="dllFilePath">file path with file name</param>
		/// <param name="hFile">use IntPtr.Zero</param>
		/// <param name="dwFlags">What will happend during loading dll
		/// <para>LOAD_LIBRARY_AS_DATAFILE</para>
		/// <para>DONT_RESOLVE_DLL_REFERENCES</para>
		/// <para>LOAD_WITH_ALTERED_SEARCH_PATH</para>
		/// <para>LOAD_IGNORE_CODE_AUTHZ_LEVEL</para>
		/// </param>
		/// <returns>Pointer to loaded Dll</returns>
		[

		DllImport("kernel32.dll")]
		private static extern IntPtr LoadLibraryEx(string dllFilePath, IntPtr hFile, uint dwFlags);

		/// <summary>
		/// To unload library 
		/// </summary>
		/// <param name="dllPointer">Pointer to Dll witch was returned from LoadLibraryEx</param>
		/// <returns>If unloaded library was correct then true, else false</returns>
		[DllImport("kernel32.dll")]
		public extern static bool FreeLibrary(IntPtr dllPointer);
 
		/// <summary>
		/// To get function pointer from loaded dll 
		/// </summary>
		/// <param name="dllPointer">Pointer to Dll witch was returned from LoadLibraryEx</param>
		/// <param name="functionName">Function name with you want to call</param>
		/// <returns>Pointer to function</returns>



		[DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
		public extern static IntPtr GetProcAddress(IntPtr dllPointer, string functionName);

		public static uint LOAD_WITH_ALTERED_SEARCH_PATH = 0x00000008;

		/// <summary>
		/// This will to load concret dll file
		/// </summary>
		/// <param name="dllFilePath">Dll file path</param>
		/// <returns>Pointer to loaded dll</returns>
		/// <exception cref="ApplicationException">
		/// when loading dll will failure
		/// </exception>
		public static IntPtr LoadWin32Library(string dllFilePath)
		{
			if (!Path.IsPathRooted(dllFilePath))
				throw new ArgumentException("Must be an absolute path.", "dllFilePath");

			System.IntPtr moduleHandle = LoadLibraryEx(dllFilePath, IntPtr.Zero, LOAD_WITH_ALTERED_SEARCH_PATH);
			if (moduleHandle == IntPtr.Zero)
			{
				// I'm gettin last dll error
				int errorCode = Marshal.GetLastWin32Error();
				throw new ApplicationException(
					string.Format("There was an error during dll loading : {0}, error - {1}", dllFilePath, errorCode)
					);
			}
			return moduleHandle;
		}

		#endregion

		public WindowsDebugEngine(string winDbgPath)
		{
			object obj = null;
			Guid clsid = CLSID(typeof(IDebugClient5));

			this.winDbgPath = winDbgPath;

			hDll = LoadWin32Library(Path.Combine(winDbgPath,"dbgeng.dll"));
			hProc = GetProcAddress(hDll, "DebugCreate");
			DebugCreate debugCreate = (DebugCreate) Marshal.GetDelegateForFunctionPointer(hProc, typeof(DebugCreate));

			if (debugCreate(ref clsid, out obj) != 0)
				Debugger.Break();
			
			dbgClient = (IDebugClient5)obj;
			dbgControl = (IDebugControl4)obj;
			dbgSymbols = (IDebugSymbols3)obj;
			dbgSystemObjects = (IDebugSystemObjects)obj;

			// Reset events
			loadModules.Reset();
			exitProcess.Reset();
			handlingException.Reset();
			handledException.Reset();
			exitDebugger.Reset();

			// Reset output
			output = new StringBuilder();

			dbgSymbols.SetSymbolPath(@"SRV*http://msdl.microsoft.com/download/symbols");

			dbgClient.SetOutputCallbacks(new OutputCallbacks(this));
			dbgClient.SetEventCallbacks(new EventCallbacks(this));
		}

		static int count = 0;

		public void CreateProcessAndAttach(string CommandLine)
		{
			if (_disposed)
				throw new ApplicationException("Object already disposed");

			count++;

			dbgClient.CreateProcessAndAttach(0,
				CommandLine, 1, 0, 0);

			try
			{
				while (!exitDebugger.WaitOne(0, false) && !handledException.WaitOne(0, false))
					dbgControl.WaitForEvent(0, 100);
			}
			catch
			{
				//Debugger.Break();
			}

			dbgClient.EndSession((uint)Const.DEBUG_END_ACTIVE_TERMINATE);
		}

		public void AttachProcess(int pid)
		{
			if (_disposed)
				throw new ApplicationException("Object already disposed");

			count++;

			dbgClient.AttachProcess(0, (uint)pid, 0);

			try
			{
				while (!exitDebugger.WaitOne(0, false) && !handledException.WaitOne(0, false))
					dbgControl.WaitForEvent(0, 100);
			}
			catch
			{
				//Debugger.Break();
			}

			dbgClient.EndSession((uint)Const.DEBUG_END_ACTIVE_TERMINATE);
		}

		public void AttachKernel(string connectionString)
		{
			if (_disposed)
				throw new ApplicationException("Object already disposed");

			count++;

			dbgClient.AttachKernel((uint)Const.DEBUG_ATTACH_KERNEL_CONNECTION, ref connectionString);

			try
			{
				while (!exitDebugger.WaitOne(0, false) && !handledException.WaitOne(0, false))
					dbgControl.WaitForEvent(0, 100);
			}
			catch
			{
				//Debugger.Break();
			}

			dbgClient.EndSession((uint)Const.DEBUG_END_ACTIVE_TERMINATE);
		}

		protected Guid CLSID(object o)
		{
			return CLSID(typeof(object));
		}
		protected Guid CLSID(Type t)
		{
			foreach (Attribute a in t.GetCustomAttributes(true))
			{
				if (a is GuidAttribute)
				{
					return new Guid(((GuidAttribute)a).Value);
				}
			}

			return Guid.Empty;
		}

		#region IDisposable Members

		public void Dispose()
		{
			_disposed = true;

			Marshal.FinalReleaseComObject(dbgClient);
			FreeLibrary(hDll);
		}

		#endregion
	}

	[Guid("F193F926-63C4-4837-8456-40C1CD1720D5")]
	public class OutputCallbacks : IDebugOutputCallbacks
	{
		WindowsDebugEngine _engine;
		public OutputCallbacks(WindowsDebugEngine engine)
		{
			_engine = engine;
		}

		#region IDebugOutputCallbacks Members

		public void Output(uint Mask, string Text)
		{
			_engine.output.Append(Text);
		}

		#endregion
	}

	[Guid("287D0DC2-2E79-49FB-9113-CA3F34941320")]
	public class EventCallbacks : IDebugEventCallbacks
	{
		Regex reMajorHash = new Regex(@"^MAJOR_HASH:(0x.*)\r$", RegexOptions.Multiline);
		Regex reMinorHash = new Regex(@"^MINOR_HASH:(0x.*)\r$", RegexOptions.Multiline);
		Regex reClassification = new Regex(@"^CLASSIFICATION:(.*)\r$", RegexOptions.Multiline);
		Regex reShortDescription = new Regex(@"^SHORT_DESCRIPTION:(.*)\r$", RegexOptions.Multiline);

		WindowsDebugEngine _engine;
		public EventCallbacks(WindowsDebugEngine engine)
		{
			_engine = engine;
		}

		#region IDebugEventCallbacks Members

		public void GetInterestMask(out uint Mask)
		{
			Mask = (uint)( Const.DEBUG_EVENT_EXCEPTION |
				Const.DEBUG_EVENT_EXIT_PROCESS | Const.DEBUG_EVENT_LOAD_MODULE | Const.DEBUG_EVENT_CREATE_PROCESS);
		}

		public void Breakpoint(IDebugBreakpoint Bp)
		{
		}

		public void Exception(ref _EXCEPTION_RECORD64 Exception, uint FirstChance)
		{
			bool handle = false;
			if(FirstChance == 1)
			{
				if(_engine.skipFirstChanceGuardPageException && Exception.ExceptionCode == 0x80000001)
					return;

				// Guard page or illegal op
				if(Exception.ExceptionCode == 0x80000001 || Exception.ExceptionCode == 0xC000001D)
					handle = true;

				if(Exception.ExceptionCode == 0xC0000005)
				{
					// A/V on EIP
					if(Exception.ExceptionInformation[0] == 0 && Exception.ExceptionInformation[1] == Exception.ExceptionAddress)
						handle = true;

					// write a/v
					else if (Exception.ExceptionInformation[0] == 1 && Exception.ExceptionInformation[1] != 0)
						handle = true;

					// DEP
					else if (Exception.ExceptionInformation[0] == 0)
						handle = true;
				}

				// Skip uninteresting first chance
				if(!handle)
					return;
			}

			if(_engine.skipSecondChangeGuardPageException && FirstChance == 0 && Exception.ExceptionCode == 0x80000001)
				return;

			// Don't recurse
			if (_engine.handlingException.WaitOne(0, false) || _engine.handledException.WaitOne(0, false))
				return;

			// ////////////////////////////////////////////////////////////////////////////////////////////////////////

			try
			{
				_engine.handlingException.Set();

				// 1. Output registers

				_engine.dbgControl.Execute((uint)Const.DEBUG_OUTCTL_THIS_CLIENT, "r", (uint)Const.DEBUG_EXECUTE_ECHO);
				_engine.dbgControl.Execute((uint)Const.DEBUG_OUTCTL_THIS_CLIENT, "rF", (uint)Const.DEBUG_EXECUTE_ECHO);
				_engine.dbgControl.Execute((uint)Const.DEBUG_OUTCTL_THIS_CLIENT, "rX", (uint)Const.DEBUG_EXECUTE_ECHO);
				_engine.output.Append("\n\n");

				// 2. Output stacktrace

				// Note: There is a known bug in dbgeng that can cause stack traces to take days due to issues in 
				// resolving symbols.  There is no known work arround.  We need the ability to skip a stacktrace
				// when this occurs.

				_engine.dbgControl.Execute((uint)Const.DEBUG_OUTCTL_THIS_CLIENT, "kb", (uint)Const.DEBUG_EXECUTE_ECHO);
				_engine.output.Append("\n\n");

				// 3. Dump File

				// Note: This can cause hangs on a bad day.  Don't think it's all that important, so skipping.

				// 4. !exploitable

				// TODO - Load correct version of !exploitable
				if (IntPtr.Size == 4)
				{
					// 32bit
					string path = Assembly.GetExecutingAssembly().Location;
					path = Path.GetDirectoryName(path);
					path = Path.Combine(path, "Debuggers\\DebugEngine\\msec86.dll");
					_engine.dbgControl.Execute((uint)Const.DEBUG_OUTCTL_THIS_CLIENT, ".load "+path, (uint)Const.DEBUG_EXECUTE_ECHO);
				}
				else
				{
					// 64bit
					string path = Assembly.GetExecutingAssembly().Location;
					path = Path.GetDirectoryName(path);
					path = Path.Combine(path, "Debuggers\\DebugEngine\\msec64.dll");
					_engine.dbgControl.Execute((uint)Const.DEBUG_OUTCTL_THIS_CLIENT, ".load "+path, (uint)Const.DEBUG_EXECUTE_ECHO);
				}

				_engine.dbgControl.Execute((uint)Const.DEBUG_OUTCTL_THIS_CLIENT, "!exploitable -m", (uint)Const.DEBUG_EXECUTE_ECHO);

				Dictionary<string, Variant> crashInfo = new Dictionary<string, Variant>();
				_engine.output.Replace("\x0a", "\r\n");

				string output = _engine.output.ToString();

                Fault fault = new Fault();
                fault.type = FaultType.Fault;
				fault.detectionSource = "WindowsDebugEngine";
				fault.majorHash = reMajorHash.Match(output).Groups[1].Value;
				fault.minorHash = reMinorHash.Match(output).Groups[1].Value;
				fault.exploitability = reClassification.Match(output).Groups[1].Value;
				fault.title = reShortDescription.Match(output).Groups[1].Value;

                fault.collectedData["StackTrace.txt"] = UTF8Encoding.UTF8.GetBytes(output);
                fault.description = output;

				_engine.crashInfo = fault;
			}
			finally
			{
				//_engine.dbgClient.TerminateProcesses();
				//_engine.dbgClient.EndSession((uint)Const.DEBUG_END_PASSIVE);
				//_engine.dbgClient.EndSession((uint)Const.DEBUG_END_ACTIVE_TERMINATE);
				_engine.handledException.Set();
			}
		}

		public void CreateThread(ulong Handle, ulong DataOffset, ulong StartOffset)
		{
		}

		public void ExitThread(uint ExitCode)
		{
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern int GetProcessId(uint hWnd);

		public void CreateProcess(ulong ImageFileHandle, ulong Handle, ulong BaseOffset, 
			uint ModuleSize, string ModuleName, string ImageName, 
			uint CheckSum, uint TimeDateStamp, ulong InitialThreadHandle, 
			ulong ThreadDataOffset, ulong StartOffset)
		{
			_engine.processId = GetProcessId((uint)Handle);
		}

		public void ExitProcess(uint ExitCode)
		{
			_engine.exitProcess.Set();
		}

		public void LoadModule(ulong ImageFileHandle, ulong BaseOffset, uint ModuleSize, 
			string ModuleName, string ImageName, uint CheckSum, uint TimeDateStamp)
		{
			_engine.loadModules.Set();
		}

		public void UnloadModule(string ImageBaseName, ulong BaseOffset)
		{
		}

		public void SystemError(uint Error, uint Level)
		{
		}

		public void SessionStatus(uint Status)
		{
		}

		public void ChangeDebuggeeState(uint Flags, ulong Argument)
		{
		}

		public void ChangeEngineState(uint Flags, ulong Argument)
		{
		}

		public void ChangeSymbolState(uint Flags, ulong Argument)
		{
		}

		#endregion
	}
}
