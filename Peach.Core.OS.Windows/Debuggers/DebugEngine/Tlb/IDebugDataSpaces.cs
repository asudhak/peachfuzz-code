namespace Peach.Core.Debuggers.DebugEngine.Tlb
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short) 1), Guid("88F7DFAB-3EA7-4C3A-AEFB-C4E8106173AA")]
    public interface IDebugDataSpaces
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ReadVirtual([In] ulong Offset, [In, Out] IntPtr Buffer, [In] uint BufferSize, [In, Out, Optional] ref uint BytesRead);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void WriteVirtual([In] ulong Offset, [In] IntPtr Buffer, [In] uint BufferSize, [Optional] out uint BytesWritten);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SearchVirtual([In] ulong Offset, [In] ulong Length, [In] IntPtr Pattern, [In] uint PatternSize, [In] uint PatternGranularity, out ulong MatchOffset);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ReadVirtualUncached([In] ulong Offset, [In, Out] IntPtr Buffer, [In] uint BufferSize, [In, Out, Optional] ref uint BytesRead);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void WriteVirtualUncached([In] ulong Offset, [In] IntPtr Buffer, [In] uint BufferSize, [Optional] out uint BytesWritten);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ReadPointersVirtual([In] uint Count, [In] ulong Offset, out ulong Ptrs);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void WritePointersVirtual([In] uint Count, [In] ulong Offset, [In] ref ulong Ptrs);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ReadPhysical([In] ulong Offset, [In, Out] IntPtr Buffer, [In] uint BufferSize, [In, Out, Optional] ref uint BytesRead);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void WritePhysical([In] ulong Offset, [In] IntPtr Buffer, [In] uint BufferSize, [Optional] out uint BytesWritten);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ReadControl([In] uint Processor, [In] ulong Offset, [In, Out] IntPtr Buffer, [In] uint BufferSize, [In, Out, Optional] ref uint BytesRead);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void WriteControl([In] uint Processor, [In] ulong Offset, [In] IntPtr Buffer, [In] uint BufferSize, [Optional] out uint BytesWritten);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ReadIo([In] uint InterfaceType, [In] uint BusNumber, [In] uint AddressSpace, [In] ulong Offset, [In, Out] IntPtr Buffer, [In] uint BufferSize, [In, Out, Optional] ref uint BytesRead);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void WriteIo([In] uint InterfaceType, [In] uint BusNumber, [In] uint AddressSpace, [In] ulong Offset, [In] IntPtr Buffer, [In] uint BufferSize, [Optional] out uint BytesWritten);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ReadMsr([In] uint Msr, out ulong Value);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void WriteMsr([In] uint Msr, [In] ulong Value);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ReadBusData([In] uint BusDataType, [In] uint BusNumber, [In] uint SlotNumber, [In] uint Offset, [In, Out] IntPtr Buffer, [In] uint BufferSize, [In, Out, Optional] ref uint BytesRead);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void WriteBusData([In] uint BusDataType, [In] uint BusNumber, [In] uint SlotNumber, [In] uint Offset, [In] IntPtr Buffer, [In] uint BufferSize, [Optional] out uint BytesWritten);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void CheckLowMemory();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ReadDebuggerData([In] uint Index, [In, Out] IntPtr Buffer, [In] uint BufferSize, [Optional] out uint DataSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ReadProcessorSystemData([In] uint Processor, [In] uint Index, [In, Out] IntPtr Buffer, [In] uint BufferSize, [Optional] out uint DataSize);
    }
}

