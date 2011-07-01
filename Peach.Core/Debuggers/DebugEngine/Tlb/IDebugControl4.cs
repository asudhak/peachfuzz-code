namespace Peach.Core.Debuggers.DebugEngine.Tlb
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("94E60CE9-9B41-4B19-9FC0-6D9EB35272B3"), InterfaceType((short) 1)]
    public interface IDebugControl4
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetInterrupt();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetInterrupt([In] uint Flags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetInterruptTimeout(out uint Seconds);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetInterruptTimeout([In] uint Seconds);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetLogFile([Optional] out sbyte Buffer, [In, Optional] uint BufferSize, [Optional] out uint FileSize, [Optional] out int Append);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void OpenLogFile([In] ref sbyte File, [In] int Append);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void CloseLogFile();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetLogMask(out uint Mask);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetLogMask([In] uint Mask);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void Input(out sbyte Buffer, [In] uint BufferSize, [Optional] out uint InputSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ReturnInput([In] ref sbyte Buffer);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void Output([In] uint Mask, [In] ref sbyte Format, [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType=VarEnum.VT_VARIANT)] object[] __MIDL__IDebugControl40000);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void OutputVaList([In] uint Mask, [In] ref sbyte Format, [In] ref sbyte Args);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ControlledOutput([In] uint OutputControl, [In] uint Mask, [In] ref sbyte Format, [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType=VarEnum.VT_VARIANT)] object[] __MIDL__IDebugControl40001);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ControlledOutputVaList([In] uint OutputControl, [In] uint Mask, [In] ref sbyte Format, [In] ref sbyte Args);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void OutputPrompt([In] uint OutputControl, [In, Optional] ref sbyte Format, [Optional, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType=VarEnum.VT_VARIANT)] object[] __MIDL__IDebugControl40002);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void OutputPromptVaList([In] uint OutputControl, [In, Optional] ref sbyte Format, [In, Optional] ref sbyte Args);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetPromptText([Optional] out sbyte Buffer, [In, Optional] uint BufferSize, [Optional] out uint TextSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void OutputCurrentState([In] uint OutputControl, [In] uint Flags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void OutputVersionInformation([In] uint OutputControl);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetNotifyEventHandle(out ulong Handle);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetNotifyEventHandle([In] ulong Handle);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void Assemble([In] ulong Offset, [In] ref sbyte Instr, out ulong EndOffset);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void Disassemble([In] ulong Offset, [In] uint Flags, [Optional] out sbyte Buffer, [In, Optional] uint BufferSize, [Optional] out uint DisassemblySize, [Optional] out ulong EndOffset);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetDisassembleEffectiveOffset(out ulong Offset);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void OutputDisassembly([In] uint OutputControl, [In] ulong Offset, [In] uint Flags, out ulong EndOffset);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void OutputDisassemblyLines([In] uint OutputControl, [In] uint PreviousLines, [In] uint TotalLines, [In] ulong Offset, [In] uint Flags, [Optional] out uint OffsetLine, [Optional] out ulong StartOffset, [Optional] out ulong EndOffset, [Optional] out ulong LineOffsets);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetNearInstruction([In] ulong Offset, [In] int Delta, out ulong NearOffset);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetStackTrace([In] ulong FrameOffset, [In] ulong StackOffset, [In] ulong InstructionOffset, [In, Out] ref _DEBUG_STACK_FRAME Frames, [In] uint FramesSize, [Optional] out uint FramesFilled);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetReturnOffset(out ulong Offset);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void OutputStackTrace([In] uint OutputControl, [In, Optional] ref _DEBUG_STACK_FRAME Frames, [In, Optional] uint FramesSize, [In, Optional] uint Flags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetDebuggeeType(out uint Class, out uint Qualifier);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetActualProcessorType(out uint Type);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetExecutingProcessorType(out uint Type);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetNumberPossibleExecutingProcessorTypes(out uint Number);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetPossibleExecutingProcessorTypes([In] uint Start, [In] uint Count, out uint Types);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetNumberProcessors(out uint Number);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSystemVersion(out uint PlatformId, out uint Major, out uint Minor, [Optional] out sbyte ServicePackString, [In, Optional] uint ServicePackStringSize, [Optional] out uint ServicePackStringUsed, [Optional] out uint ServicePackNumber, [Optional] out sbyte BuildString, [In, Optional] uint BuildStringSize, [Optional] out uint BuildStringUsed);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetPageSize(out uint Size);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void IsPointer64Bit();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ReadBugCheckData(out uint Code, out ulong Arg1, out ulong Arg2, out ulong Arg3, out ulong Arg4);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetNumberSupportedProcessorTypes(out uint Number);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSupportedProcessorTypes([In] uint Start, [In] uint Count, out uint Types);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetProcessorTypeNames([In] uint Type, [Optional] out sbyte FullNameBuffer, [In, Optional] uint FullNameBufferSize, [Optional] out uint FullNameSize, [Optional] out sbyte AbbrevNameBuffer, [In, Optional] uint AbbrevNameBufferSize, [Optional] out uint AbbrevNameSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetEffectiveProcessorType(out uint Type);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetEffectiveProcessorType([In] uint Type);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetExecutionStatus(out uint Status);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetExecutionStatus([In] uint Status);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetCodeLevel(out uint Level);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetCodeLevel([In] uint Level);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetEngineOptions(out uint Options);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void AddEngineOptions([In] uint Options);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void RemoveEngineOptions([In] uint Options);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetEngineOptions([In] uint Options);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSystemErrorControl(out uint OutputLevel, out uint BreakLevel);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetSystemErrorControl([In] uint OutputLevel, [In] uint BreakLevel);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetTextMacro([In] uint Slot, [Optional] out sbyte Buffer, [In, Optional] uint BufferSize, [Optional] out uint MacroSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetTextMacro([In] uint Slot, [In] ref sbyte Macro);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetRadix(out uint Radix);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetRadix([In] uint Radix);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void Evaluate([In] ref sbyte Expression, [In] uint DesiredType, out _DEBUG_VALUE Value, [Optional] out uint RemainderIndex);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void CoerceValue([In] ref _DEBUG_VALUE In, [In] uint OutType, out _DEBUG_VALUE Out);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void CoerceValues([In] uint Count, [In] ref _DEBUG_VALUE In, [In] ref uint OutTypes, out _DEBUG_VALUE Out);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void Execute([In] uint OutputControl, [In] [MarshalAs(UnmanagedType.LPStr)] string Command, [In] uint Flags);
        
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ExecuteCommandFile([In] uint OutputControl, [In] ref sbyte CommandFile, [In] uint Flags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetNumberBreakpoints(out uint Number);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetBreakpointByIndex([In] uint Index, [MarshalAs(UnmanagedType.Interface)] out IDebugBreakpoint Bp);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetBreakpointById([In] uint Id, [MarshalAs(UnmanagedType.Interface)] out IDebugBreakpoint Bp);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetBreakpointParameters([In] uint Count, [In, Optional] ref uint Ids, [In, Optional] uint Start, [Optional] out _DEBUG_BREAKPOINT_PARAMETERS Params);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void AddBreakpoint([In] uint Type, [In] uint DesiredId, [MarshalAs(UnmanagedType.Interface)] out IDebugBreakpoint Bp);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void RemoveBreakpoint([In, MarshalAs(UnmanagedType.Interface)] IDebugBreakpoint Bp);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void AddExtension([In] ref sbyte Path, [In] uint Flags, out ulong Handle);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void RemoveExtension([In] ulong Handle);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetExtensionByPath([In] ref sbyte Path, out ulong Handle);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void CallExtension([In] ulong Handle, [In] ref sbyte Function, [In, Optional] ref sbyte Arguments);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetExtensionFunction([In] ulong Handle, [In] ref sbyte FuncName, out IntPtr Function);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetWindbgExtensionApis32([In, Out] ref _WINDBG_EXTENSION_APIS32 Api);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetWindbgExtensionApis64([In, Out] ref _WINDBG_EXTENSION_APIS64 Api);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetNumberEventFilters(out uint SpecificEvents, out uint SpecificExceptions, out uint ArbitraryExceptions);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetEventFilterText([In] uint Index, [Optional] out sbyte Buffer, [In, Optional] uint BufferSize, [Optional] out uint TextSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetEventFilterCommand([In] uint Index, [Optional] out sbyte Buffer, [In, Optional] uint BufferSize, [Optional] out uint CommandSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetEventFilterCommand([In] uint Index, [In] ref sbyte Command);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSpecificFilterParameters([In] uint Start, [In] uint Count, out _DEBUG_SPECIFIC_FILTER_PARAMETERS Params);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetSpecificFilterParameters([In] uint Start, [In] uint Count, [In] ref _DEBUG_SPECIFIC_FILTER_PARAMETERS Params);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSpecificFilterArgument([In] uint Index, [Optional] out sbyte Buffer, [In, Optional] uint BufferSize, [Optional] out uint ArgumentSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetSpecificFilterArgument([In] uint Index, [In] ref sbyte Argument);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetExceptionFilterParameters([In] uint Count, [In, Optional] ref uint Codes, [In, Optional] uint Start, [Optional] out _DEBUG_EXCEPTION_FILTER_PARAMETERS Params);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetExceptionFilterParameters([In] uint Count, [In] ref _DEBUG_EXCEPTION_FILTER_PARAMETERS Params);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetExceptionFilterSecondCommand([In] uint Index, [Optional] out sbyte Buffer, [In, Optional] uint BufferSize, [Optional] out uint CommandSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetExceptionFilterSecondCommand([In] uint Index, [In] ref sbyte Command);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void WaitForEvent([In] uint Flags, [In] uint Timeout);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetLastEventInformation(out uint Type, out uint ProcessId, out uint ThreadId, [Out, Optional] IntPtr ExtraInformation, [In, Optional] uint ExtraInformationSize, [Optional] out uint ExtraInformationUsed, [Optional] out sbyte Description, [In, Optional] uint DescriptionSize, [Optional] out uint DescriptionUsed);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetCurrentTimeDate(out uint TimeDate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetCurrentSystemUpTime(out uint UpTime);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetDumpFormatFlags(out uint FormatFlags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetNumberTextReplacements(out uint NumRepl);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetTextReplacement([In, Optional] ref sbyte SrcText, [In, Optional] uint Index, [Optional] out sbyte SrcBuffer, [In, Optional] uint SrcBufferSize, [Optional] out uint SrcSize, [Optional] out sbyte DstBuffer, [In, Optional] uint DstBufferSize, [Optional] out uint DstSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetTextReplacement([In] ref sbyte SrcText, [In, Optional] ref sbyte DstText);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void RemoveTextReplacements();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void OutputTextReplacements([In] uint OutputControl, [In] uint Flags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetAssemblyOptions(out uint Options);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void AddAssemblyOptions([In] uint Options);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void RemoveAssemblyOptions([In] uint Options);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetAssemblyOptions([In] uint Options);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetExpressionSyntax(out uint Flags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetExpressionSyntax([In] uint Flags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetExpressionSyntaxByName([In] ref sbyte AbbrevName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetNumberExpressionSyntaxes(out uint Number);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetExpressionSyntaxNames([In] uint Index, [Optional] out sbyte FullNameBuffer, [In, Optional] uint FullNameBufferSize, [Optional] out uint FullNameSize, [Optional] out sbyte AbbrevNameBuffer, [In, Optional] uint AbbrevNameBufferSize, [Optional] out uint AbbrevNameSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetNumberEvents(out uint Events);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetEventIndexDescription([In] uint Index, [In] uint Which, [In, Optional] ref sbyte Buffer, [In, Optional] uint BufferSize, [Optional] out uint DescSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetCurrentEventIndex(out uint Index);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetNextEventIndex([In] uint Relation, [In] uint Value, out uint NextIndex);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetLogFileWide([Optional] out ushort Buffer, [In, Optional] uint BufferSize, [Optional] out uint FileSize, [Optional] out int Append);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void OpenLogFileWide([In] ref ushort File, [In] int Append);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void InputWide(out ushort Buffer, [In] uint BufferSize, [Optional] out uint InputSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ReturnInputWide([In] ref ushort Buffer);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void OutputWide([In] uint Mask, [In] ref ushort Format, [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType=VarEnum.VT_VARIANT)] object[] __MIDL__IDebugControl40003);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void OutputVaListWide([In] uint Mask, [In] ref ushort Format, [In] ref sbyte Args);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ControlledOutputWide([In] uint OutputControl, [In] uint Mask, [In] ref ushort Format, [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType=VarEnum.VT_VARIANT)] object[] __MIDL__IDebugControl40004);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ControlledOutputVaListWide([In] uint OutputControl, [In] uint Mask, [In] ref ushort Format, [In] ref sbyte Args);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void OutputPromptWide([In] uint OutputControl, [In, Optional] ref ushort Format, [Optional, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType=VarEnum.VT_VARIANT)] object[] __MIDL__IDebugControl40005);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void OutputPromptVaListWide([In] uint OutputControl, [In, Optional] ref ushort Format, [In, Optional] ref sbyte Args);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetPromptTextWide([Optional] out ushort Buffer, [In, Optional] uint BufferSize, [Optional] out uint TextSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void AssembleWide([In] ulong Offset, [In] ref ushort Instr, out ulong EndOffset);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void DisassembleWide([In] ulong Offset, [In] uint Flags, [Optional] out ushort Buffer, [In, Optional] uint BufferSize, [Optional] out uint DisassemblySize, [Optional] out ulong EndOffset);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetProcessorTypeNamesWide([In] uint Type, [Optional] out ushort FullNameBuffer, [In, Optional] uint FullNameBufferSize, [Optional] out uint FullNameSize, [Optional] out ushort AbbrevNameBuffer, [In, Optional] uint AbbrevNameBufferSize, [Optional] out uint AbbrevNameSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetTextMacroWide([In] uint Slot, [Optional] out ushort Buffer, [In, Optional] uint BufferSize, [Optional] out uint MacroSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetTextMacroWide([In] uint Slot, [In] ref ushort Macro);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void EvaluateWide([In] ref ushort Expression, [In] uint DesiredType, out _DEBUG_VALUE Value, [Optional] out uint RemainderIndex);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ExecuteWide([In] uint OutputControl, [In] ref ushort Command, [In] uint Flags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ExecuteCommandFileWide([In] uint OutputControl, [In] ref ushort CommandFile, [In] uint Flags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetBreakpointByIndex2([In] uint Index, [MarshalAs(UnmanagedType.Interface)] out IDebugBreakpoint2 Bp);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetBreakpointById2([In] uint Id, [MarshalAs(UnmanagedType.Interface)] out IDebugBreakpoint2 Bp);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void AddBreakpoint2([In] uint Type, [In] uint DesiredId, [MarshalAs(UnmanagedType.Interface)] out IDebugBreakpoint2 Bp);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void RemoveBreakpoint2([In, MarshalAs(UnmanagedType.Interface)] IDebugBreakpoint2 Bp);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void AddExtensionWide([In] ref ushort Path, [In] uint Flags, out ulong Handle);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetExtensionByPathWide([In] ref ushort Path, out ulong Handle);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void CallExtensionWide([In] ulong Handle, [In] ref ushort Function, [In, Optional] ref ushort Arguments);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetExtensionFunctionWide([In] ulong Handle, [In] ref ushort FuncName, out IntPtr Function);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetEventFilterTextWide([In] uint Index, [Optional] out ushort Buffer, [In, Optional] uint BufferSize, [Optional] out uint TextSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetEventFilterCommandWide([In] uint Index, [Optional] out ushort Buffer, [In, Optional] uint BufferSize, [Optional] out uint CommandSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetEventFilterCommandWide([In] uint Index, [In] ref ushort Command);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSpecificFilterArgumentWide([In] uint Index, [Optional] out ushort Buffer, [In, Optional] uint BufferSize, [Optional] out uint ArgumentSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetSpecificFilterArgumentWide([In] uint Index, [In] ref ushort Argument);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetExceptionFilterSecondCommandWide([In] uint Index, [Optional] out ushort Buffer, [In, Optional] uint BufferSize, [Optional] out uint CommandSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetExceptionFilterSecondCommandWide([In] uint Index, [In] ref ushort Command);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetLastEventInformationWide(out uint Type, out uint ProcessId, out uint ThreadId, [Out, Optional] IntPtr ExtraInformation, [In, Optional] uint ExtraInformationSize, [Optional] out uint ExtraInformationUsed, [Optional] out ushort Description, [In, Optional] uint DescriptionSize, [Optional] out uint DescriptionUsed);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetTextReplacementWide([In, Optional] ref ushort SrcText, [In, Optional] uint Index, [Optional] out ushort SrcBuffer, [In, Optional] uint SrcBufferSize, [Optional] out uint SrcSize, [Optional] out ushort DstBuffer, [In, Optional] uint DstBufferSize, [Optional] out uint DstSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetTextReplacementWide([In] ref ushort SrcText, [In, Optional] ref ushort DstText);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetExpressionSyntaxByNameWide([In] ref ushort AbbrevName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetExpressionSyntaxNamesWide([In] uint Index, [Optional] out ushort FullNameBuffer, [In, Optional] uint FullNameBufferSize, [Optional] out uint FullNameSize, [Optional] out ushort AbbrevNameBuffer, [In, Optional] uint AbbrevNameBufferSize, [Optional] out uint AbbrevNameSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetEventIndexDescriptionWide([In] uint Index, [In] uint Which, [In, Optional] ref ushort Buffer, [In, Optional] uint BufferSize, [Optional] out uint DescSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetLogFile2([Optional] out sbyte Buffer, [In, Optional] uint BufferSize, [Optional] out uint FileSize, [Optional] out uint Flags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void OpenLogFile2([In] ref sbyte File, [In] uint Flags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetLogFile2Wide([Optional] out ushort Buffer, [In, Optional] uint BufferSize, [Optional] out uint FileSize, [Optional] out uint Flags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void OpenLogFile2Wide([In] ref ushort File, [In] uint Flags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSystemVersionValues(out uint PlatformId, out uint Win32Major, out uint Win32Minor, [Optional] out uint KdMajor, [Optional] out uint KdMinor);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSystemVersionString([In] uint Which, [Optional] out sbyte Buffer, [In, Optional] uint BufferSize, [Optional] out uint StringSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSystemVersionStringWide([In] uint Which, [Optional] out ushort Buffer, [In, Optional] uint BufferSize, [Optional] out uint StringSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetContextStackTrace([In, Optional] IntPtr StartContext, [In, Optional] uint StartContextSize, [In, Out, Optional] ref _DEBUG_STACK_FRAME Frames, [In, Optional] uint FramesSize, [Out, Optional] IntPtr FrameContexts, [In, Optional] uint FrameContextsSize, [In, Optional] uint FrameContextsEntrySize, [Optional] out uint FramesFilled);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void OutputContextStackTrace([In] uint OutputControl, [In] ref _DEBUG_STACK_FRAME Frames, [In] uint FramesSize, [In] IntPtr FrameContexts, [In] uint FrameContextsSize, [In] uint FrameContextsEntrySize, [In] uint Flags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetStoredEventInformation(out uint Type, out uint ProcessId, out uint ThreadId, [Out, Optional] IntPtr Context, [In, Optional] uint ContextSize, [Optional] out uint ContextUsed, [Out, Optional] IntPtr ExtraInformation, [In, Optional] uint ExtraInformationSize, [Optional] out uint ExtraInformationUsed);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetManagedStatus([Optional] out uint Flags, [In, Optional] uint WhichString, [Optional] out sbyte String, [In, Optional] uint StringSize, [Optional] out uint StringNeeded);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetManagedStatusWide([Optional] out uint Flags, [In, Optional] uint WhichString, [Optional] out ushort String, [In, Optional] uint StringSize, [Optional] out uint StringNeeded);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ResetManagedStatus([In] uint Flags);
    }
}

