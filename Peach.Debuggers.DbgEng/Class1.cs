using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Peach.Debuggers.DbgEng.Tlb;
using System.Runtime.InteropServices;
using System.Runtime;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Diagnostics;

namespace Peach.Debuggers.DbgEng
{
	public class Class1
	{
		[DllImport(@"C:\Program Files (x86)\Debugging Tools for Windows (x86)\dbgeng.dll")]
		static public extern uint DebugCreate(
			ref Guid InterfaceId,
			[MarshalAs(UnmanagedType.IUnknown)] out object Interface);

		public Class1()
		{
			object obj = null;
			Guid clsid = CLSID(typeof(IDebugClient5));
			DebugCreate(ref clsid, out obj);
			IDebugClient5 dbgClient = (IDebugClient5)obj;

			dbgClient.SetOutputCallbacks(new OutputCallbacks());
			dbgClient.CreateProcessAndAttach(0, "notepad.exe");
			dbgClient.WaitForProcessServerEnd(0);

			Debugger.Break();
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
}

	[Guid("F193F926-63C4-4837-8456-40C1CD1720D5")]
	public class OutputCallbacks : IDebugOutputCallbacks
	{
		#region IDebugOutputCallbacks Members

		public void Output(uint Mask, string Text)
		{
			Console.WriteLine(Text);
		}

		#endregion
	}

	[Guid("287D0DC2-2E79-49FB-9113-CA3F34941320")]
	public class EventCallbacks : IDebugEventCallbacks
	{
		#region IDebugEventCallbacks Members

		public void GetInterestMask(out uint Mask)
		{
			Mask = (uint)( Const.DEBUG_EVENT_EXCEPTION | Const.DEBUG_FILTER_INITIAL_BREAKPOINT |
				Const.DEBUG_EVENT_EXIT_PROCESS | Const.DEBUG_EVENT_LOAD_MODULE);
		}

		public void Breakpoint(IDebugBreakpoint Bp)
		{
			throw new NotImplementedException();
		}

		public void Exception(ref _EXCEPTION_RECORD64 Exception, uint FirstChance)
		{
			throw new NotImplementedException();
		}

		public void CreateThread(ulong Handle, ulong DataOffset, ulong StartOffset)
		{
			throw new NotImplementedException();
		}

		public void ExitThread(uint ExitCode)
		{
			throw new NotImplementedException();
		}

		public void CreateProcess(ulong ImageFileHandle, ulong Handle, ulong BaseOffset, uint ModuleSize, string ModuleName = null, string ImageName = null, uint CheckSum = 0, uint TimeDateStamp = 0, ulong InitialThreadHandle = 0, ulong ThreadDataOffset = 0, ulong StartOffset = 0)
		{
			throw new NotImplementedException();
		}

		public void ExitProcess(uint ExitCode)
		{
			throw new NotImplementedException();
		}

		public void LoadModule(ulong ImageFileHandle, ulong BaseOffset, uint ModuleSize, string ModuleName = null, string ImageName = null, uint CheckSum = 0, uint TimeDateStamp = 0)
		{
			throw new NotImplementedException();
		}

		public void UnloadModule(string ImageBaseName = null, ulong BaseOffset = 0)
		{
			throw new NotImplementedException();
		}

		public void SystemError(uint Error, uint Level)
		{
			throw new NotImplementedException();
		}

		public void SessionStatus(uint Status)
		{
			throw new NotImplementedException();
		}

		public void ChangeDebuggeeState(uint Flags, ulong Argument)
		{
			throw new NotImplementedException();
		}

		public void ChangeEngineState(uint Flags, ulong Argument)
		{
			throw new NotImplementedException();
		}

		public void ChangeSymbolState(uint Flags, ulong Argument)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
