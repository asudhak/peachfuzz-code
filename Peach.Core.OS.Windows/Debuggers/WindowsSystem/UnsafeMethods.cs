
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

		[DllImport("kernel32.dll", SetLastError = true)]
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

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] buffer, UInt32 size, out uint lpNumberOfBytesRead);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out uint lpNumberOfBytesWritten);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool FlushInstructionCache(IntPtr hProcess, IntPtr lpBaseAddress, uint dwSize);

		[DllImport("kernel32.dll", SetLastError = true)]
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

		[DllImport("kernel32.dll")]
		public static extern uint GetFileSize(IntPtr hFile, ref uint lpFileSizeHigh);
		
		[DllImport("kernel32.dll")]
		public static extern uint GetMappedFileName(IntPtr hProcess, IntPtr lpv, ref StringBuilder lpFilename, uint nSize);

		[DllImport("kernel32.dll")]
		public static extern IntPtr GetCurrentProcess();

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);

		[DllImport("kernel32.dll")]
		public static extern IntPtr GetModuleHandle(string moduleName);

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern IntPtr CreateFileMapping(
			IntPtr hFile,
			IntPtr lpFileMappingAttributes,
			FileMapProtection flProtect,
			uint dwMaximumSizeHigh,
			uint dwMaximumSizeLow,
			[MarshalAs(UnmanagedType.LPTStr)] string lpName);

		[DllImport("kernel32.dll")]
		public static extern bool GetThreadContext(IntPtr hThread, ref CONTEXT lpContext);

		[DllImport("kernel32.dll")]
		public static extern bool SetThreadContext(IntPtr hThread, [In] ref CONTEXT lpContext);

		public enum CONTEXT_FLAGS : uint
		{
			CONTEXT_i386 = 0x10000,
			CONTEXT_i486 = 0x10000,   //  same as i386
			CONTEXT_CONTROL = CONTEXT_i386 | 0x01, // SS:SP, CS:IP, FLAGS, BP
			CONTEXT_INTEGER = CONTEXT_i386 | 0x02, // AX, BX, CX, DX, SI, DI
			CONTEXT_SEGMENTS = CONTEXT_i386 | 0x04, // DS, ES, FS, GS
			CONTEXT_FLOATING_POINT = CONTEXT_i386 | 0x08, // 387 state
			CONTEXT_DEBUG_REGISTERS = CONTEXT_i386 | 0x10, // DB 0-3,6,7
			CONTEXT_EXTENDED_REGISTERS = CONTEXT_i386 | 0x20, // cpu specific extensions
			CONTEXT_FULL = CONTEXT_CONTROL | CONTEXT_INTEGER | CONTEXT_SEGMENTS,
			CONTEXT_ALL = CONTEXT_CONTROL | CONTEXT_INTEGER | CONTEXT_SEGMENTS | CONTEXT_FLOATING_POINT | CONTEXT_DEBUG_REGISTERS | CONTEXT_EXTENDED_REGISTERS
		}

		public struct FLOATING_SAVE_AREA
		{
			public uint ControlWord;
			public uint StatusWord;
			public uint TagWord;
			public uint ErrorOffset;
			public uint ErrorSelector;
			public uint DataOffset;
			public uint DataSelector;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 80)]
			public byte[] RegisterArea;
			public uint Cr0NpxState;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct CONTEXT
		{

			public uint ContextFlags; //set this to an appropriate value
			// Retrieved by CONTEXT_DEBUG_REGISTERS
			public uint Dr0;
			public uint Dr1;
			public uint Dr2;
			public uint Dr3;
			public uint Dr6;
			public uint Dr7;
			// Retrieved by CONTEXT_FLOATING_POINT
			public FLOATING_SAVE_AREA FloatSave;
			// Retrieved by CONTEXT_SEGMENTS
			public uint SegGs;
			public uint SegFs;
			public uint SegEs;
			public uint SegDs;
			// Retrieved by CONTEXT_INTEGER
			public uint Edi;
			public uint Esi;
			public uint Ebx;
			public uint Edx;
			public uint Ecx;
			public uint Eax;
			// Retrieved by CONTEXT_CONTROL
			public uint Ebp;
			public uint Eip;
			public uint SegCs;
			public uint EFlags;
			public uint Esp;
			public uint SegSs;
			// Retrieved by CONTEXT_EXTENDED_REGISTERS
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
			public byte[] ExtendedRegisters;

		} 

		[Flags]
		public enum FileMapProtection : uint
		{
			PageReadonly = 0x02,
			PageReadWrite = 0x04,
			PageWriteCopy = 0x08,
			PageExecuteRead = 0x20,
			PageExecuteReadWrite = 0x40,
			SectionCommit = 0x8000000,
			SectionImage = 0x1000000,
			SectionNoCache = 0x10000000,
			SectionReserve = 0x4000000,
		}
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr MapViewOfFile(
			IntPtr hFileMappingObject,
			FileMapAccess dwDesiredAccess,
			uint dwFileOffsetHigh,
			uint dwFileOffsetLow,
			uint dwNumberOfBytesToMap);

		[Flags]
		public enum FileMapAccess : uint
		{
			FileMapCopy = 0x0001,
			FileMapWrite = 0x0002,
			FileMapRead = 0x0004,
			FileMapAllAccess = 0x001f,
			FileMapExecute = 0x0020,
		}

		public enum DebugEventType : uint
		{
			EXCEPTION_DEBUG_EVENT      = 1,
			CREATE_THREAD_DEBUG_EVENT  = 2,
			CREATE_PROCESS_DEBUG_EVENT = 3,
			EXIT_THREAD_DEBUG_EVENT    = 4,
			EXIT_PROCESS_DEBUG_EVENT   = 5,
			LOAD_DLL_DEBUG_EVENT       = 6,
			UNLOAD_DLL_DEBUG_EVENT     = 7,
			OUTPUT_DEBUG_STRING_EVENT  = 8,
			RIP_EVENT                  = 9,
		};

		public struct DEBUG_EVENT
		{
			public DebugEventType dwDebugEventCode;
			public uint dwProcessId;
			public uint dwThreadId;

			public Union u;
		}

		public struct Union
		{
			public EXCEPTION_DEBUG_INFO Exception;
			public CREATE_THREAD_DEBUG_INFO CreateThread;
			public CREATE_PROCESS_DEBUG_INFO CreateProcessInfo;
			public EXIT_THREAD_DEBUG_INFO ExitThread;
			public EXIT_PROCESS_DEBUG_INFO ExitProcess;
			public LOAD_DLL_DEBUG_INFO LoadDll;
			public UNLOAD_DLL_DEBUG_INFO UnloadDll;
			public OUTPUT_DEBUG_STRING_INFO DebugString;
			public RIP_INFO RipInfo;
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
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
			public IntPtr[] ExceptionInformation;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct CREATE_THREAD_DEBUG_INFO
		{
			public IntPtr hThread;
			public IntPtr lpThreadLocalBase;
			public IntPtr lpStartAddress;
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
			public IntPtr lpStartAddress;
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
			public IntPtr lpDebugStringData;
			public ushort fUnicode;
			public ushort nDebugStringLength;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct RIP_INFO
		{
			public uint dwError;
			public uint dwType;
		}

		// Inner union of structs must me aligned on IntPtr boundary
		private static int DEBUG_EVENT_OFFSET = 12 + (12 % IntPtr.Size);

		private static int DEBUG_EVENT_SIZE = Marshal.SizeOf(typeof(EXCEPTION_DEBUG_INFO)) + DEBUG_EVENT_OFFSET;

		public static bool WaitForDebugEvent(out DEBUG_EVENT debug_event, uint dwMilliseconds)
		{
			debug_event = new DEBUG_EVENT();
			int len = DEBUG_EVENT_SIZE;
			IntPtr buf = Marshal.AllocHGlobal(len);
			ZeroMemory(buf, IntPtr.Zero + len);
			bool ret = WaitForDebugEvent(buf, dwMilliseconds);

			if (ret)
			{
				debug_event.dwDebugEventCode = (DebugEventType)Marshal.ReadInt32(buf, 0);
				debug_event.dwProcessId = (uint)Marshal.ReadInt32(buf, 4);
				debug_event.dwThreadId = (uint)Marshal.ReadInt32(buf, 8);

				IntPtr offset = buf + DEBUG_EVENT_OFFSET;

				switch (debug_event.dwDebugEventCode)
				{
					case DebugEventType.EXCEPTION_DEBUG_EVENT:
						debug_event.u.Exception = (EXCEPTION_DEBUG_INFO)Marshal.PtrToStructure(offset, typeof(EXCEPTION_DEBUG_INFO));
						break;
					case DebugEventType.CREATE_THREAD_DEBUG_EVENT:
						debug_event.u.CreateThread = (CREATE_THREAD_DEBUG_INFO)Marshal.PtrToStructure(offset, typeof(CREATE_THREAD_DEBUG_INFO));
						break;
					case DebugEventType.CREATE_PROCESS_DEBUG_EVENT:
						debug_event.u.CreateProcessInfo = (CREATE_PROCESS_DEBUG_INFO)Marshal.PtrToStructure(offset, typeof(CREATE_PROCESS_DEBUG_INFO));
						break;
					case DebugEventType.EXIT_THREAD_DEBUG_EVENT:
						debug_event.u.ExitThread = (EXIT_THREAD_DEBUG_INFO)Marshal.PtrToStructure(offset, typeof(EXIT_THREAD_DEBUG_INFO));
						break;
					case DebugEventType.EXIT_PROCESS_DEBUG_EVENT:
						debug_event.u.ExitProcess = (EXIT_PROCESS_DEBUG_INFO)Marshal.PtrToStructure(offset, typeof(EXIT_PROCESS_DEBUG_INFO));
						break;
					case DebugEventType.LOAD_DLL_DEBUG_EVENT:
						debug_event.u.LoadDll = (LOAD_DLL_DEBUG_INFO)Marshal.PtrToStructure(offset, typeof(LOAD_DLL_DEBUG_INFO));
						break;
					case DebugEventType.UNLOAD_DLL_DEBUG_EVENT:
						debug_event.u.UnloadDll = (UNLOAD_DLL_DEBUG_INFO)Marshal.PtrToStructure(offset, typeof(UNLOAD_DLL_DEBUG_INFO));
						break;
					case DebugEventType.OUTPUT_DEBUG_STRING_EVENT:
						debug_event.u.DebugString = (OUTPUT_DEBUG_STRING_INFO)Marshal.PtrToStructure(offset, typeof(OUTPUT_DEBUG_STRING_INFO));
						break;
					case DebugEventType.RIP_EVENT:
						debug_event.u.RipInfo = (RIP_INFO)Marshal.PtrToStructure(offset, typeof(RIP_INFO));
						break;
					default:
						break;
				}
			}

			Marshal.FreeHGlobal(buf);
			return ret;
		}

		[DllImport("kernel32.dll", EntryPoint = "WaitForDebugEvent")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool WaitForDebugEvent(IntPtr lpDebugEvent, uint dwMilliseconds);

		[DllImport("kernel32.dll")]
		public static extern bool DebugActiveProcess(uint dwProcessId);

		[DllImport("kernel32.dll")]
		public static extern bool DebugActiveProcessStop(uint dwProcessId);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool CloseHandle(IntPtr hObject);

		[DllImport("Kernel32.dll", EntryPoint = "RtlZeroMemory", SetLastError = false)]
		static extern void ZeroMemory(IntPtr dest, IntPtr size);
	}
}
