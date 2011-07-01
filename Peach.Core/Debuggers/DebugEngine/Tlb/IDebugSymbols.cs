namespace Peach.Core.Debuggers.DebugEngine.Tlb
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short) 1), Guid("8C31E98C-983A-48A5-9016-6FE5D667A950")]
    public interface IDebugSymbols
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSymbolOptions(out uint Options);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void AddSymbolOptions([In] uint Options);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void RemoveSymbolOptions([In] uint Options);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetSymbolOptions([In] uint Options);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetNameByOffset([In] ulong Offset, [In, Out, Optional] ref sbyte NameBuffer, [In, Optional] uint NameBufferSize, [Optional] out uint NameSize, [In, Out, Optional] ref ulong Displacement);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetOffsetByName([In] ref sbyte Symbol, out ulong Offset);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetNearNameByOffset([In] ulong Offset, [In] int Delta, [In, Out, Optional] ref sbyte NameBuffer, [In, Optional] uint NameBufferSize, [Optional] out uint NameSize, [In, Out, Optional] ref ulong Displacement);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetLineByOffset([In] ulong Offset, [Optional] out uint Line, [Optional] out sbyte FileBuffer, [In, Optional] uint FileBufferSize, [Optional] out uint FileSize, [In, Out, Optional] ref ulong Displacement);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetOffsetByLine([In] uint Line, [In] ref sbyte File, out ulong Offset);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetNumberModules(out uint Loaded, out uint Unloaded);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetModuleByIndex([In] uint Index, out ulong Base);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetModuleByModuleName([In] ref sbyte Name, [In] uint StartIndex, [Optional] out uint Index, [Optional] out ulong Base);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetModuleByOffset([In] ulong Offset, [In] uint StartIndex, [Optional] out uint Index, [Optional] out ulong Base);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetModuleNames([In] uint Index, [In] ulong Base, [Optional] out sbyte ImageNameBuffer, [In, Optional] uint ImageNameBufferSize, [Optional] out uint ImageNameSize, [Optional] out sbyte ModuleNameBuffer, [In, Optional] uint ModuleNameBufferSize, [Optional] out uint ModuleNameSize, [Optional] out sbyte LoadedImageNameBuffer, [In, Optional] uint LoadedImageNameBufferSize, [Optional] out uint LoadedImageNameSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetModuleParameters([In] uint Count, [In, Optional] ref ulong Bases, [In, Optional] uint Start, [Optional] out _DEBUG_MODULE_PARAMETERS Params);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSymbolModule([In] ref sbyte Symbol, out ulong Base);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetTypeName([In] ulong Module, [In] uint TypeId, [In, Out, Optional] ref sbyte NameBuffer, [In, Optional] uint NameBufferSize, [Optional] out uint NameSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetTypeId([In] ulong Module, [In] ref sbyte Name, out uint TypeId);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetTypeSize([In] ulong Module, [In] uint TypeId, out uint Size);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetFieldOffset([In] ulong Module, [In] uint TypeId, [In] ref sbyte Field, out uint Offset);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSymbolTypeId([In] ref sbyte Symbol, out uint TypeId, [Optional] out ulong Module);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetOffsetTypeId([In] ulong Offset, out uint TypeId, [Optional] out ulong Module);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ReadTypedDataVirtual([In] ulong Offset, [In] ulong Module, [In] uint TypeId, [In, Out] IntPtr Buffer, [In] uint BufferSize, [In, Out, Optional] ref uint BytesRead);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void WriteTypedDataVirtual([In] ulong Offset, [In] ulong Module, [In] uint TypeId, [In] IntPtr Buffer, [In] uint BufferSize, [Optional] out uint BytesWritten);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void OutputTypedDataVirtual([In] uint OutputControl, [In] ulong Offset, [In] ulong Module, [In] uint TypeId, [In] uint Flags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ReadTypedDataPhysical([In] ulong Offset, [In] ulong Module, [In] uint TypeId, [In, Out] IntPtr Buffer, [In] uint BufferSize, [In, Out, Optional] ref uint BytesRead);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void WriteTypedDataPhysical([In] ulong Offset, [In] ulong Module, [In] uint TypeId, [In] IntPtr Buffer, [In] uint BufferSize, [Optional] out uint BytesWritten);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void OutputTypedDataPhysical([In] uint OutputControl, [In] ulong Offset, [In] ulong Module, [In] uint TypeId, [In] uint Flags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetScope([Optional] out ulong InstructionOffset, [Optional] out _DEBUG_STACK_FRAME ScopeFrame, [Out, Optional] IntPtr ScopeContext, [In, Optional] uint ScopeContextSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetScope([In] ulong InstructionOffset, [In, Optional] ref _DEBUG_STACK_FRAME ScopeFrame, [In, Optional] IntPtr ScopeContext, [In, Optional] uint ScopeContextSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ResetScope();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetScopeSymbolGroup([In] uint Flags, [In, Optional, MarshalAs(UnmanagedType.Interface)] IDebugSymbolGroup Update, [Optional, MarshalAs(UnmanagedType.Interface)] out IDebugSymbolGroup Symbols);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void CreateSymbolGroup([MarshalAs(UnmanagedType.Interface)] out IDebugSymbolGroup Group);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void StartSymbolMatch([In] ref sbyte Pattern, out ulong Handle);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetNextSymbolMatch([In] ulong Handle, [Optional] out sbyte Buffer, [In, Optional] uint BufferSize, [Optional] out uint MatchSize, [Optional] out ulong Offset);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void EndSymbolMatch([In] ulong Handle);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void Reload([In] ref sbyte Module);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSymbolPath([Optional] out sbyte Buffer, [In, Optional] uint BufferSize, [Optional] out uint PathSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetSymbolPath([In] ref sbyte Path);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void AppendSymbolPath([In] ref sbyte Addition);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetImagePath([Optional] out sbyte Buffer, [In, Optional] uint BufferSize, [Optional] out uint PathSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetImagePath([In] ref sbyte Path);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void AppendImagePath([In] ref sbyte Addition);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSourcePath([Optional] out sbyte Buffer, [In, Optional] uint BufferSize, [Optional] out uint PathSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSourcePathElement([In] uint Index, [Optional] out sbyte Buffer, [In, Optional] uint BufferSize, [Optional] out uint ElementSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetSourcePath([In] ref sbyte Path);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void AppendSourcePath([In] ref sbyte Addition);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void FindSourceFile([In] uint StartElement, [In] ref sbyte File, [In] uint Flags, [Optional] out uint FoundElement, [Optional] out sbyte Buffer, [In, Optional] uint BufferSize, [Optional] out uint FoundSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSourceFileLineOffsets([In] ref sbyte File, [Optional] out ulong Buffer, [In, Optional] uint BufferLines, [Optional] out uint FileLines);
    }
}

