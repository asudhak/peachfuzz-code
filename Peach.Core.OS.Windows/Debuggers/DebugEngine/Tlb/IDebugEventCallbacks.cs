namespace Peach.Core.Debuggers.DebugEngine.Tlb
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("337BE28B-5036-4D72-B6BF-C45FBB9F2EAA"), InterfaceType((short) 1)]
    public interface IDebugEventCallbacks
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetInterestMask(out uint Mask);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void Breakpoint([In, MarshalAs(UnmanagedType.Interface)] IDebugBreakpoint Bp);
        
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void Exception([In] ref _EXCEPTION_RECORD64 Exception, [In] uint FirstChance);
        
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void CreateThread([In] ulong Handle, [In] ulong DataOffset, [In] ulong StartOffset);
        
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ExitThread([In] uint ExitCode);
        
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void CreateProcess([In] ulong ImageFileHandle, [In] ulong Handle, [In] ulong BaseOffset, 
			[In] uint ModuleSize,
			[In, Optional] [MarshalAs(UnmanagedType.LPStr)] string ModuleName,
			[In, Optional] [MarshalAs(UnmanagedType.LPStr)] string ImageName, 
			[In, Optional] uint CheckSum, [In, Optional] uint TimeDateStamp, 
			[In, Optional] ulong InitialThreadHandle, [In, Optional] ulong ThreadDataOffset, 
			[In, Optional] ulong StartOffset);
        
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ExitProcess([In] uint ExitCode);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void LoadModule([In] ulong ImageFileHandle, [In] ulong BaseOffset, [In] uint ModuleSize,
			[In, Optional] [MarshalAs(UnmanagedType.LPStr)] string ModuleName,
			[In, Optional] [MarshalAs(UnmanagedType.LPStr)] string ImageName, 
			[In, Optional] uint CheckSum, [In, Optional] uint TimeDateStamp);
        
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void UnloadModule([In, Optional] [MarshalAs(UnmanagedType.LPStr)] string ImageBaseName, [In, Optional] ulong BaseOffset);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SystemError([In] uint Error, [In] uint Level);
        
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SessionStatus([In] uint Status);
        
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ChangeDebuggeeState([In] uint Flags, [In] ulong Argument);
        
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ChangeEngineState([In] uint Flags, [In] ulong Argument);
        
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ChangeSymbolState([In] uint Flags, [In] ulong Argument);
    }
}

