namespace Peach.Core.Debuggers.DebugEngine.Tlb
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("F02FBECC-50AC-4F36-9AD9-C975E8F32FF8"), InterfaceType((short) 1)]
    public interface IDebugSymbols3
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
        void SetSymbolPath([In] [MarshalAs(UnmanagedType.LPStr)] string Path);
        
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
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetModuleVersionInformation([In] uint Index, [In] ulong Base, [In] ref sbyte Item, [In, Out, Optional] IntPtr Buffer, [In, Optional] uint BufferSize, [Optional] out uint VerInfoSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetModuleNameString([In] uint Which, [In] uint Index, [In] ulong Base, [Optional] out sbyte Buffer, [In, Optional] uint BufferSize, [Optional] out uint NameSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetConstantName([In] ulong Module, [In] uint TypeId, [In] ulong Value, [In, Out, Optional] ref sbyte NameBuffer, [In, Optional] uint NameBufferSize, [Optional] out uint NameSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetFieldName([In] ulong Module, [In] uint TypeId, [In] uint FieldIndex, [In, Out, Optional] ref sbyte NameBuffer, [In, Optional] uint NameBufferSize, [Optional] out uint NameSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetTypeOptions(out uint Options);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void AddTypeOptions([In] uint Options);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void RemoveTypeOptions([In] uint Options);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetTypeOptions([In] uint Options);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetNameByOffsetWide([In] ulong Offset, [Optional] out ushort NameBuffer, [In, Optional] uint NameBufferSize, [Optional] out uint NameSize, [In, Out, Optional] ref ulong Displacement);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetOffsetByNameWide([In] ref ushort Symbol, out ulong Offset);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetNearNameByOffsetWide([In] ulong Offset, [In] int Delta, [Optional] out ushort NameBuffer, [In, Optional] uint NameBufferSize, [Optional] out uint NameSize, [In, Out, Optional] ref ulong Displacement);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetLineByOffsetWide([In] ulong Offset, [Optional] out uint Line, [Optional] out ushort FileBuffer, [In, Optional] uint FileBufferSize, [Optional] out uint FileSize, [In, Out, Optional] ref ulong Displacement);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetOffsetByLineWide([In] uint Line, [In] ref ushort File, out ulong Offset);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetModuleByModuleNameWide([In] ref ushort Name, [In] uint StartIndex, [Optional] out uint Index, [Optional] out ulong Base);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSymbolModuleWide([In] ref ushort Symbol, out ulong Base);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetTypeNameWide([In] ulong Module, [In] uint TypeId, [Optional] out ushort NameBuffer, [In, Optional] uint NameBufferSize, [Optional] out uint NameSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetTypeIdWide([In] ulong Module, [In] ref ushort Name, out uint TypeId);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetFieldOffsetWide([In] ulong Module, [In] uint TypeId, [In] ref ushort Field, out uint Offset);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSymbolTypeIdWide([In] ref ushort Symbol, out uint TypeId, [Optional] out ulong Module);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetScopeSymbolGroup2([In] uint Flags, [In, Optional, MarshalAs(UnmanagedType.Interface)] IDebugSymbolGroup2 Update, [Optional, MarshalAs(UnmanagedType.Interface)] out IDebugSymbolGroup2 Symbols);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void CreateSymbolGroup2([MarshalAs(UnmanagedType.Interface)] out IDebugSymbolGroup2 Group);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void StartSymbolMatchWide([In] ref ushort Pattern, out ulong Handle);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetNextSymbolMatchWide([In] ulong Handle, [Optional] out ushort Buffer, [In, Optional] uint BufferSize, [Optional] out uint MatchSize, [Optional] out ulong Offset);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ReloadWide([In] ref ushort Module);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSymbolPathWide([Optional] out ushort Buffer, [In, Optional] uint BufferSize, [Optional] out uint PathSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetSymbolPathWide([In] ref ushort Path);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void AppendSymbolPathWide([In] ref ushort Addition);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetImagePathWide([Optional] out ushort Buffer, [In, Optional] uint BufferSize, [Optional] out uint PathSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetImagePathWide([In] ref ushort Path);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void AppendImagePathWide([In] ref ushort Addition);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSourcePathWide([Optional] out ushort Buffer, [In, Optional] uint BufferSize, [Optional] out uint PathSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSourcePathElementWide([In] uint Index, [Optional] out ushort Buffer, [In, Optional] uint BufferSize, [Optional] out uint ElementSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetSourcePathWide([In] ref ushort Path);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void AppendSourcePathWide([In] ref ushort Addition);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void FindSourceFileWide([In] uint StartElement, [In] ref ushort File, [In] uint Flags, [Optional] out uint FoundElement, [Optional] out ushort Buffer, [In, Optional] uint BufferSize, [Optional] out uint FoundSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSourceFileLineOffsetsWide([In] ref ushort File, [Optional] out ulong Buffer, [In, Optional] uint BufferLines, [Optional] out uint FileLines);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetModuleVersionInformationWide([In] uint Index, [In] ulong Base, [In] ref ushort Item, [In, Out, Optional] IntPtr Buffer, [In, Optional] uint BufferSize, [Optional] out uint VerInfoSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetModuleNameStringWide([In] uint Which, [In] uint Index, [In] ulong Base, [Optional] out ushort Buffer, [In, Optional] uint BufferSize, [Optional] out uint NameSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetConstantNameWide([In] ulong Module, [In] uint TypeId, [In] ulong Value, [Optional] out ushort NameBuffer, [In, Optional] uint NameBufferSize, [Optional] out uint NameSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetFieldNameWide([In] ulong Module, [In] uint TypeId, [In] uint FieldIndex, [Optional] out ushort NameBuffer, [In, Optional] uint NameBufferSize, [Optional] out uint NameSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void IsManagedModule([In] uint Index, [In] ulong Base);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetModuleByModuleName2([In] ref sbyte Name, [In] uint StartIndex, [In] uint Flags, [Optional] out uint Index, [Optional] out ulong Base);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetModuleByModuleName2Wide([In] ref ushort Name, [In] uint StartIndex, [In] uint Flags, [Optional] out uint Index, [Optional] out ulong Base);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetModuleByOffset2([In] ulong Offset, [In] uint StartIndex, [In] uint Flags, [Optional] out uint Index, [Optional] out ulong Base);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void AddSyntheticModule([In] ulong Base, [In] uint Size, [In] ref sbyte ImagePath, [In] ref sbyte ModuleName, [In] uint Flags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void AddSyntheticModuleWide([In] ulong Base, [In] uint Size, [In] ref ushort ImagePath, [In] ref ushort ModuleName, [In] uint Flags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void RemoveSyntheticModule([In] ulong Base);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetCurrentScopeFrameIndex(out uint Index);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetScopeFrameByIndex([In] uint Index);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetScopeFromJitDebugInfo([In] uint OutputControl, [In] ulong InfoOffset);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetScopeFromStoredEvent();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void OutputSymbolByOffset([In] uint OutputControl, [In] uint Flags, [In] ulong Offset);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetFunctionEntryByOffset([In] ulong Offset, [In] uint Flags, [In, Out, Optional] IntPtr Buffer, [In, Optional] uint BufferSize, [Optional] out uint BufferNeeded);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetFieldTypeAndOffset([In] ulong Module, [In] uint ContainerTypeId, [In] ref sbyte Field, [Optional] out uint FieldTypeId, [Optional] out uint Offset);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetFieldTypeAndOffsetWide([In] ulong Module, [In] uint ContainerTypeId, [In] ref ushort Field, [Optional] out uint FieldTypeId, [Optional] out uint Offset);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void AddSyntheticSymbol([In] ulong Offset, [In] uint Size, [In] ref sbyte Name, [In] uint Flags, [Optional] out _DEBUG_MODULE_AND_ID Id);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void AddSyntheticSymbolWide([In] ulong Offset, [In] uint Size, [In] ref ushort Name, [In] uint Flags, [Optional] out _DEBUG_MODULE_AND_ID Id);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void RemoveSyntheticSymbol([In] ref _DEBUG_MODULE_AND_ID Id);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSymbolEntriesByOffset([In] ulong Offset, [In] uint Flags, [Optional] out _DEBUG_MODULE_AND_ID Ids, [In, Out, Optional] ref ulong Displacements, [In, Optional] uint IdsCount, [Optional] out uint Entries);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSymbolEntriesByName([In] ref sbyte Symbol, [In] uint Flags, [Optional] out _DEBUG_MODULE_AND_ID Ids, [In, Optional] uint IdsCount, [Optional] out uint Entries);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSymbolEntriesByNameWide([In] ref ushort Symbol, [In] uint Flags, [Optional] out _DEBUG_MODULE_AND_ID Ids, [In, Optional] uint IdsCount, [Optional] out uint Entries);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSymbolEntryByToken([In] ulong ModuleBase, [In] uint Token, out _DEBUG_MODULE_AND_ID Id);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSymbolEntryInformation([In] ref _DEBUG_MODULE_AND_ID Id, out _DEBUG_SYMBOL_ENTRY Info);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSymbolEntryString([In] ref _DEBUG_MODULE_AND_ID Id, [In] uint Which, [Optional] out sbyte Buffer, [In, Optional] uint BufferSize, [Optional] out uint StringSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSymbolEntryStringWide([In] ref _DEBUG_MODULE_AND_ID Id, [In] uint Which, [Optional] out ushort Buffer, [In, Optional] uint BufferSize, [Optional] out uint StringSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSymbolEntryOffsetRegions([In] ref _DEBUG_MODULE_AND_ID Id, [In] uint Flags, [Optional] out _DEBUG_OFFSET_REGION Regions, [In, Optional] uint RegionsCount, [Optional] out uint RegionsAvail);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSymbolEntryBySymbolEntry([In] ref _DEBUG_MODULE_AND_ID FromId, [In] uint Flags, out _DEBUG_MODULE_AND_ID ToId);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSourceEntriesByOffset([In] ulong Offset, [In] uint Flags, [Optional] out _DEBUG_SYMBOL_SOURCE_ENTRY Entries, [In, Optional] uint EntriesCount, [Optional] out uint EntriesAvail);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSourceEntriesByLine([In] uint Line, [In] ref sbyte File, [In] uint Flags, [Optional] out _DEBUG_SYMBOL_SOURCE_ENTRY Entries, [In, Optional] uint EntriesCount, [Optional] out uint EntriesAvail);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSourceEntriesByLineWide([In] uint Line, [In] ref ushort File, [In] uint Flags, [Optional] out _DEBUG_SYMBOL_SOURCE_ENTRY Entries, [In, Optional] uint EntriesCount, [Optional] out uint EntriesAvail);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSourceEntryString([In] ref _DEBUG_SYMBOL_SOURCE_ENTRY Entry, [In] uint Which, [Optional] out sbyte Buffer, [In, Optional] uint BufferSize, [Optional] out uint StringSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSourceEntryStringWide([In] ref _DEBUG_SYMBOL_SOURCE_ENTRY Entry, [In] uint Which, [Optional] out ushort Buffer, [In, Optional] uint BufferSize, [Optional] out uint StringSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSourceEntryOffsetRegions([In] ref _DEBUG_SYMBOL_SOURCE_ENTRY Entry, [In] uint Flags, [Optional] out _DEBUG_OFFSET_REGION Regions, [In, Optional] uint RegionsCount, [Optional] out uint RegionsAvail);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSourceEntryBySourceEntry([In] ref _DEBUG_SYMBOL_SOURCE_ENTRY FromEntry, [In] uint Flags, out _DEBUG_SYMBOL_SOURCE_ENTRY ToEntry);
    }
}

