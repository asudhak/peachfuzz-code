namespace Peach.Core.Debuggers.DebugEngine.Tlb
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short) 1), Guid("7A5E852F-96E9-468F-AC1B-0B3ADDC4A049")]
    public interface IDebugDataSpaces2
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
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void VirtualToPhysical([In] ulong Virtual, out ulong Physical);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetVirtualTranslationPhysicalOffsets([In] ulong Virtual, [Optional] out ulong Offsets, [In, Optional] uint OffsetsSize, [Optional] out uint Levels);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ReadHandleData([In] ulong Handle, [In] uint DataType, [In, Out, Optional] IntPtr Buffer, [In, Optional] uint BufferSize, [Optional] out uint DataSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void FillVirtual([In] ulong Start, [In] uint Size, [In] IntPtr Pattern, [In] uint PatternSize, [Optional] out uint Filled);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void FillPhysical([In] ulong Start, [In] uint Size, [In] IntPtr Pattern, [In] uint PatternSize, [Optional] out uint Filled);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void QueryVirtual([In] ulong Offset, out _MEMORY_BASIC_INFORMATION64 Info);
    }
}

