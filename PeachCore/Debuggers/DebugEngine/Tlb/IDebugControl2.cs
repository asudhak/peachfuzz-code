namespace Peach.Core.Debuggers.DebugEngine.Tlb
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short) 1), Guid("D4366723-44DF-4BED-8C7E-4C05424F4588")]
    public interface IDebugControl2
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
        void Output([In] uint Mask, [In] ref sbyte Format, [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType=VarEnum.VT_VARIANT)] object[] __MIDL__IDebugControl20000);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void OutputVaList([In] uint Mask, [In] ref sbyte Format, [In] ref sbyte Args);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ControlledOutput([In] uint OutputControl, [In] uint Mask, [In] ref sbyte Format, [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType=VarEnum.VT_VARIANT)] object[] __MIDL__IDebugControl20001);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ControlledOutputVaList([In] uint OutputControl, [In] uint Mask, [In] ref sbyte Format, [In] ref sbyte Args);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void OutputPrompt([In] uint OutputControl, [In, Optional] ref sbyte Format, [Optional, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType=VarEnum.VT_VARIANT)] object[] __MIDL__IDebugControl20002);
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
        void Execute([In] uint OutputControl, [In] ref sbyte Command, [In] uint Flags);
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
    }
}

