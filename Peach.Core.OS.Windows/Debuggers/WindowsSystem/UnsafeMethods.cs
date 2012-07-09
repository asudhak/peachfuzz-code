
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Peach.Core.Debuggers.WindowsSystem
{
	/// <summary>
	/// Contains definitions for marshaled method calls and related
	/// types.
	/// </summary>
	public class UnsafeMethods
	{
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct STARTUPINFO
		{
			public Int32 cb;
			public string lpReserved;
			public string lpDesktop;
			public string lpTitle;
			public Int32 dwX;
			public Int32 dwY;
			public Int32 dwXSize;
			public Int32 dwYSize;
			public Int32 dwXCountChars;
			public Int32 dwYCountChars;
			public Int32 dwFillAttribute;
			public Int32 dwFlags;
			public Int16 wShowWindow;
			public Int16 cbReserved2;
			public IntPtr lpReserved2;
			public IntPtr hStdInput;
			public IntPtr hStdOutput;
			public IntPtr hStdError;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct PROCESS_INFORMATION
		{
			public IntPtr hProcess;
			public IntPtr hThread;
			public int dwProcessId;
			public int dwThreadId;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SECURITY_ATTRIBUTES
		{
			public int nLength;
			public IntPtr lpSecurityDescriptor;
			public int bInheritHandle;
		}

		[Flags()]
		public enum ProcessAccess : int
		{
			/// <summary>Specifies all possible access flags for the process object.</summary>
			AllAccess = CreateThread | DuplicateHandle | QueryInformation | SetInformation | Terminate | VMOperation | VMRead | VMWrite | Synchronize,
			/// <summary>Enables usage of the process handle in the CreateRemoteThread function to create a thread in the process.</summary>
			CreateThread = 0x2,
			/// <summary>Enables usage of the process handle as either the source or target process in the DuplicateHandle function to duplicate a handle.</summary>
			DuplicateHandle = 0x40,
			/// <summary>Enables usage of the process handle in the GetExitCodeProcess and GetPriorityClass functions to read information from the process object.</summary>
			QueryInformation = 0x400,
			/// <summary>Enables usage of the process handle in the SetPriorityClass function to set the priority class of the process.</summary>
			SetInformation = 0x200,
			/// <summary>Enables usage of the process handle in the TerminateProcess function to terminate the process.</summary>
			Terminate = 0x1,
			/// <summary>Enables usage of the process handle in the VirtualProtectEx and WriteProcessMemory functions to modify the virtual memory of the process.</summary>
			VMOperation = 0x8,
			/// <summary>Enables usage of the process handle in the ReadProcessMemory function to' read from the virtual memory of the process.</summary>
			VMRead = 0x10,
			/// <summary>Enables usage of the process handle in the WriteProcessMemory function to write to the virtual memory of the process.</summary>
			VMWrite = 0x20,
			/// <summary>Enables usage of the process handle in any of the wait functions to wait for the process to terminate.</summary>
			Synchronize = 0x100000
		}

		[DllImport("kernel32.dll")]
		static extern IntPtr OpenProcess(ProcessAccess dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);


		//[DllImport("kernel32.dll")]
		//public static extern bool CreateProcess(string lpApplicationName,
		//   string lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes,
		//   ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles,
		//   uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory,
		//   [In] ref STARTUPINFO lpStartupInfo,
		//   out PROCESS_INFORMATION lpProcessInformation);

		[DllImport("kernel32.dll")]
		public static extern bool CreateProcess(
			string lpApplicationName,
			string lpCommandLine, 
			int lpProcessAttributes,
			int lpThreadAttributes, 
			bool bInheritHandles,
			uint dwCreationFlags, 
			IntPtr lpEnvironment, 
			string lpCurrentDirectory,
			[In] ref STARTUPINFO lpStartupInfo,
			out PROCESS_INFORMATION lpProcessInformation);

		[DllImport("kernel32.dll")]
		public static extern bool ContinueDebugEvent(uint dwProcessId, uint dwThreadId,
		   uint dwContinueStatus);

		[DllImport("kernel32.dll")]
		public static extern uint GetCurrentThread();

		[DllImport("kernel32.dll")]
		public static extern bool TerminateProcess(int hProcess, uint uExitCode);

		[DllImport("kernel32.dll")]
		public static extern int GetProcessId(int hProcess);

		[DllImport("kernel32.dll")]
		public static extern bool DebugSetProcessKillOnExit(bool KillOnExit);

		[StructLayout(LayoutKind.Explicit, Size=84)]
		public struct Union
		{
			[FieldOffset(0)]
			public EXCEPTION_DEBUG_INFO Exception;
			//public CREATE_THREAD_DEBUG_INFO CreateThread;
			//[FieldOffset(0)]
			//public CREATE_PROCESS_DEBUG_INFO CreateProcessInfo;
			//[FieldOffset(0)]
			//public EXIT_THREAD_DEBUG_INFO ExitThread;
			//[FieldOffset(0)]
			//public EXIT_PROCESS_DEBUG_INFO ExitProcess;
			//[FieldOffset(0)]
			//public LOAD_DLL_DEBUG_INFO LoadDll;
			//[FieldOffset(0)]
			//public UNLOAD_DLL_DEBUG_INFO UnloadDll;
			//[FieldOffset(0)]
			//public OUTPUT_DEBUG_STRING_INFO DebugString;
			//[FieldOffset(0)]
			//public RIP_INFO RipInfo;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct DEBUG_EVENT
		{
			public uint dwDebugEventCode;
			public uint dwProcessId;
			public uint dwThreadId;
			public Union u;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct EXCEPTION_DEBUG_INFO
		{
			public EXCEPTION_RECORD ExceptionRecord;
			public uint dwFirstChance;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct EXCEPTION_RECORD
		{
			public uint ExceptionCode;
			public uint ExceptionFlags;
			public IntPtr ExceptionRecord;
			public IntPtr ExceptionAddress;
			public uint NumberParameters;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 15, ArraySubType = UnmanagedType.U4)]
			public uint[] ExceptionInformation;
		}

		public delegate uint PTHREAD_START_ROUTINE(IntPtr lpThreadParameter);

		[StructLayout(LayoutKind.Sequential)]
		public struct CREATE_THREAD_DEBUG_INFO
		{
			public IntPtr hThread;
			public IntPtr lpThreadLocalBase;
			public PTHREAD_START_ROUTINE lpStartAddress;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct CREATE_PROCESS_DEBUG_INFO
		{
			public IntPtr hFile;
			public IntPtr hProcess;
			public IntPtr hThread;
			public IntPtr lpBaseOfImage;
			public uint dwDebugInfoFileOffset;
			public uint nDebugInfoSize;
			public IntPtr lpThreadLocalBase;
			public PTHREAD_START_ROUTINE lpStartAddress;
			public IntPtr lpImageName;
			public ushort fUnicode;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct EXIT_THREAD_DEBUG_INFO
		{
			public uint dwExitCode;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct EXIT_PROCESS_DEBUG_INFO
		{
			public uint dwExitCode;
		}
		[StructLayout(LayoutKind.Sequential)]
		public struct LOAD_DLL_DEBUG_INFO
		{
			public IntPtr hFile;
			public IntPtr lpBaseOfDll;
			public uint dwDebugInfoFileOffset;
			public uint nDebugInfoSize;
			public IntPtr lpImageName;
			public ushort fUnicode;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct UNLOAD_DLL_DEBUG_INFO
		{
			public IntPtr lpBaseOfDll;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct OUTPUT_DEBUG_STRING_INFO
		{
			[MarshalAs(UnmanagedType.LPStr)]
			public string lpDebugStringData;
			public ushort fUnicode;
			public ushort nDebugStringLength;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct RIP_INFO
		{
			public uint dwError;
			public uint dwType;
		}

		[DllImport("kernel32.dll", EntryPoint = "WaitForDebugEvent")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool WaitForDebugEvent(ref DEBUG_EVENT lpDebugEvent, uint dwMilliseconds);

		[DllImport("kernel32.dll")]
		public static extern bool DebugActiveProcess(uint dwProcessId);

		[DllImport("kernel32.dll")]
		public static extern bool DebugActiveProcessStop(uint dwProcessId);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool CloseHandle(IntPtr hObject);
	}
}
