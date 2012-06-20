namespace Peach.Core.Debuggers.DebugEngine.Tlb
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short) 1), Guid("1656AFA9-19C6-4E3A-97E7-5DC9160CF9C4")]
    public interface IDebugRegisters2
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetNumberRegisters(out uint Number);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetDescription([In] uint Register, [In, Out, Optional] ref sbyte NameBuffer, [In, Optional] uint NameBufferSize, [Optional] out uint NameSize, [Optional] out _DEBUG_REGISTER_DESCRIPTION Desc);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetIndexByName([In] ref sbyte Name, out uint Index);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetValue([In] uint Register, out _DEBUG_VALUE Value);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetValue([In] uint Register, [In] ref _DEBUG_VALUE Value);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetValues([In] uint Count, [In, Optional] ref uint Indices, [In, Optional] uint Start, [Optional] out _DEBUG_VALUE Values);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetValues([In] uint Count, [In, Optional] ref uint Indices, [In, Optional] uint Start, [In, Optional] ref _DEBUG_VALUE Values);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void OutputRegisters([In] uint OutputControl, [In] uint Flags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetInstructionOffset(out ulong Offset);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetStackOffset(out ulong Offset);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetFrameOffset(out ulong Offset);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetDescriptionWide([In] uint Register, [Optional] out ushort NameBuffer, [In, Optional] uint NameBufferSize, [Optional] out uint NameSize, [Optional] out _DEBUG_REGISTER_DESCRIPTION Desc);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetIndexByNameWide([In] ref ushort Name, out uint Index);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetNumberPseudoRegisters(out uint Number);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetPseudoDescription([In] uint Register, [In, Out, Optional] ref sbyte NameBuffer, [In, Optional] uint NameBufferSize, [Optional] out uint NameSize, [Optional] out ulong TypeModule, [Optional] out uint TypeId);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetPseudoDescriptionWide([In] uint Register, [Optional] out ushort NameBuffer, [In, Optional] uint NameBufferSize, [Optional] out uint NameSize, [Optional] out ulong TypeModule, [Optional] out uint TypeId);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetPseudoIndexByName([In] ref sbyte Name, out uint Index);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetPseudoIndexByNameWide([In] ref ushort Name, out uint Index);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetPseudoValues([In] uint Source, [In] uint Count, [In, Optional] ref uint Indices, [In, Optional] uint Start, [Optional] out _DEBUG_VALUE Values);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetPseudoValues([In] uint Source, [In] uint Count, [In, Optional] ref uint Indices, [In, Optional] uint Start, [In, Optional] ref _DEBUG_VALUE Values);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetValues2([In] uint Source, [In] uint Count, [In, Optional] ref uint Indices, [In, Optional] uint Start, [Optional] out _DEBUG_VALUE Values);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetValues2([In] uint Source, [In] uint Count, [In, Optional] ref uint Indices, [In, Optional] uint Start, [In, Optional] ref _DEBUG_VALUE Values);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void OutputRegisters2([In] uint OutputControl, [In] uint Source, [In] uint Flags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetInstructionOffset2([In] uint Source, out ulong Offset);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetStackOffset2([In] uint Source, out ulong Offset);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetFrameOffset2([In] uint Source, out ulong Offset);
    }
}

