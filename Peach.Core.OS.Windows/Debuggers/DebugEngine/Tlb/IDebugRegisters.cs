namespace Peach.Core.Debuggers.DebugEngine.Tlb
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("CE289126-9E84-45A7-937E-67BB18691493"), InterfaceType((short) 1)]
    public interface IDebugRegisters
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
    }
}

