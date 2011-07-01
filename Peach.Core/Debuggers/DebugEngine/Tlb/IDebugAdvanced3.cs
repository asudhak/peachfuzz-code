namespace Peach.Core.Debuggers.DebugEngine.Tlb
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("CBA4ABB4-84C4-444D-87CA-A04E13286739"), InterfaceType((short) 1)]
    public interface IDebugAdvanced3
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetThreadContext([Out] IntPtr Context, [In] uint ContextSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetThreadContext([In] IntPtr Context, [In] uint ContextSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void Request([In] uint Request, [In, Optional] IntPtr InBuffer, [In, Optional] uint InBufferSize, [Out, Optional] IntPtr OutBuffer, [In, Optional] uint OutBufferSize, [Optional] out uint OutSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSourceFileInformation([In] uint Which, [In] ref sbyte SourceFile, [In] ulong Arg64, [In] uint Arg32, [In, Out, Optional] IntPtr Buffer, [In, Optional] uint BufferSize, [Optional] out uint InfoSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void FindSourceFileAndToken([In] uint StartElement, [In] ulong ModAddr, [In] ref sbyte File, [In] uint Flags, [In, Optional] IntPtr FileToken, [In, Optional] uint FileTokenSize, [Optional] out uint FoundElement, [Optional] out sbyte Buffer, [In, Optional] uint BufferSize, [Optional] out uint FoundSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSymbolInformation([In] uint Which, [In] ulong Arg64, [In] uint Arg32, [In, Out, Optional] IntPtr Buffer, [In, Optional] uint BufferSize, [Optional] out uint InfoSize, [Optional] out sbyte StringBuffer, [In, Optional] uint StringBufferSize, [Optional] out uint StringSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSystemObjectInformation([In] uint Which, [In] ulong Arg64, [In] uint Arg32, [In, Out, Optional] IntPtr Buffer, [In, Optional] uint BufferSize, [Optional] out uint InfoSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSourceFileInformationWide([In] uint Which, [In] ref ushort SourceFile, [In] ulong Arg64, [In] uint Arg32, [In, Out, Optional] IntPtr Buffer, [In, Optional] uint BufferSize, [Optional] out uint InfoSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void FindSourceFileAndTokenWide([In] uint StartElement, [In] ulong ModAddr, [In] ref ushort File, [In] uint Flags, [In, Optional] IntPtr FileToken, [In, Optional] uint FileTokenSize, [Optional] out uint FoundElement, [Optional] out ushort Buffer, [In, Optional] uint BufferSize, [Optional] out uint FoundSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSymbolInformationWide([In] uint Which, [In] ulong Arg64, [In] uint Arg32, [In, Out, Optional] IntPtr Buffer, [In, Optional] uint BufferSize, [Optional] out uint InfoSize, [Optional] out ushort StringBuffer, [In, Optional] uint StringBufferSize, [Optional] out uint StringSize);
    }
}

