namespace Peach.Core.Debuggers.DebugEngine.Tlb
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short) 1), Guid("E3ACB9D7-7EC2-4F0C-A0DA-E81E0CBBE628")]
    public interface IDebugClient5
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void AttachKernel([In] uint Flags, [In, Optional] ref string ConnectOptions);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetKernelConnectionOptions([Optional] out sbyte Buffer, [In, Optional] uint BufferSize, [Optional] out uint OptionsSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetKernelConnectionOptions([In] ref sbyte Options);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void StartProcessServer([In] uint Flags, [In] ref sbyte Options, [In, Optional] IntPtr Reserved);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ConnectProcessServer([In] ref sbyte RemoteOptions, out ulong Server);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void DisconnectProcessServer([In] ulong Server);
        
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetRunningProcessSystemIds([In] ulong Server, 
			[Optional][MarshalAs(UnmanagedType.LPArray)] uint[] Ids, [In, Optional] uint Count, [Optional] out uint ActualCount);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetRunningProcessSystemIdByExecutableName([In] ulong Server, [In] ref sbyte ExeName, [In] uint Flags, out uint Id);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetRunningProcessDescription([In] ulong Server, [In] uint SystemId, [In] uint Flags, [Optional] out sbyte ExeName, [In, Optional] uint ExeNameSize, [Optional] out uint ActualExeNameSize, [Optional] out sbyte Description, [In, Optional] uint DescriptionSize, [Optional] out uint ActualDescriptionSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void AttachProcess([In] ulong Server, [In] uint ProcessId, [In] uint AttachFlags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void CreateProcess([In] ulong Server, [In] string CommandLine, [In] uint CreateFlags);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void CreateProcessAndAttach([In] [MarshalAs(UnmanagedType.U8)] ulong Server,
			[In, Optional] [MarshalAs(UnmanagedType.LPStr)] string CommandLine,
			[In] [MarshalAs(UnmanagedType.U4)] uint CreateFlags,
			[In] [MarshalAs(UnmanagedType.U4)] uint ProcessId,
			[In] [MarshalAs(UnmanagedType.U4)] uint AttachFlags);
        
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetProcessOptions(out uint Options);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void AddProcessOptions([In] uint Options);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void RemoveProcessOptions([In] uint Options);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetProcessOptions([In] uint Options);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void OpenDumpFile([In] ref sbyte DumpFile);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void WriteDumpFile([In] ref sbyte DumpFile, [In] uint Qualifier);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ConnectSession([In] uint Flags, [In] uint HistoryLimit);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void StartServer([In] ref sbyte Options);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void OutputServers([In] uint OutputControl, [In] ref sbyte Machine, [In] uint Flags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void TerminateProcesses();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void DetachProcesses();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void EndSession([In] uint Flags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetExitCode(out uint Code);
        
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void DispatchCallbacks([In] uint Timeout);
        
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ExitDispatch([In, MarshalAs(UnmanagedType.Interface)] IDebugClient Client);
        
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void CreateClient([MarshalAs(UnmanagedType.Interface)] out IDebugClient Client);
        
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetInputCallbacks([MarshalAs(UnmanagedType.Interface)] out IDebugInputCallbacks Callbacks);
        
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetInputCallbacks([In, Optional, MarshalAs(UnmanagedType.Interface)] IDebugInputCallbacks Callbacks);
        
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetOutputCallbacks([MarshalAs(UnmanagedType.Interface)] out IDebugOutputCallbacks Callbacks);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetOutputCallbacks([In, Optional, MarshalAs(UnmanagedType.Interface)] IDebugOutputCallbacks Callbacks);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetOutputMask(out uint Mask);
        
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetOutputMask([In] uint Mask);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetOtherOutputMask([In, MarshalAs(UnmanagedType.Interface)] IDebugClient Client, out uint Mask);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetOtherOutputMask([In, MarshalAs(UnmanagedType.Interface)] IDebugClient Client, [In] uint Mask);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetOutputWidth(out uint Columns);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetOutputWidth([In] uint Columns);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetOutputLinePrefix([Optional] out sbyte Buffer, [In, Optional] uint BufferSize, [Optional] out uint PrefixSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetOutputLinePrefix([In, Optional] ref sbyte Prefix);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetIdentity([Optional] out sbyte Buffer, [In, Optional] uint BufferSize, [Optional] out uint IdentitySize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void OutputIdentity([In] uint OutputControl, [In] uint Flags, [In] ref sbyte Format);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetEventCallbacks([MarshalAs(UnmanagedType.Interface)] out IDebugEventCallbacks Callbacks);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetEventCallbacks([In, Optional, MarshalAs(UnmanagedType.Interface)] IDebugEventCallbacks Callbacks);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void FlushCallbacks();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void WriteDumpFile2([In] ref sbyte DumpFile, [In] uint Qualifier, [In] uint FormatFlags, [In, Optional] ref sbyte Comment);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void AddDumpInformationFile([In] ref sbyte InfoFile, [In] uint Type);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void EndProcessServer([In] ulong Server);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void WaitForProcessServerEnd([In] uint Timeout);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void IsKernelDebuggerEnabled();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void TerminateCurrentProcess();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void DetachCurrentProcess();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void AbandonCurrentProcess();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetRunningProcessSystemIdByExecutableNameWide([In] ulong Server, [In] ref ushort ExeName, [In] uint Flags, out uint Id);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetRunningProcessDescriptionWide([In] ulong Server, [In] uint SystemId, [In] uint Flags, [Optional] out ushort ExeName, [In, Optional] uint ExeNameSize, [Optional] out uint ActualExeNameSize, [Optional] out ushort Description, [In, Optional] uint DescriptionSize, [Optional] out uint ActualDescriptionSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void CreateProcessWide([In] ulong Server, [In] ref ushort CommandLine, [In] uint CreateFlags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void CreateProcessAndAttachWide([In] ulong Server, [In, Optional] ref ushort CommandLine, [In, Optional] uint CreateFlags, [In, Optional] uint ProcessId, [In, Optional] uint AttachFlags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void OpenDumpFileWide([In, Optional] ref ushort FileName, [In, Optional] ulong FileHandle);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void WriteDumpFileWide([In, Optional] ref ushort FileName, [In, Optional] ulong FileHandle, [In, Optional] uint Qualifier, [In, Optional] uint FormatFlags, [In, Optional] ref ushort Comment);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void AddDumpInformationFileWide([In, Optional] ref ushort FileName, [In, Optional] ulong FileHandle, [In, Optional] uint Type);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetNumberDumpFiles(out uint Number);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetDumpFile([In] uint Index, [Optional] out sbyte Buffer, [In, Optional] uint BufferSize, [Optional] out uint NameSize, [Optional] out ulong Handle, [Optional] out uint Type);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetDumpFileWide([In] uint Index, [Optional] out ushort Buffer, [In, Optional] uint BufferSize, [Optional] out uint NameSize, [Optional] out ulong Handle, [Optional] out uint Type);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void AttachKernelWide([In] uint Flags, [In, Optional] ref ushort ConnectOptions);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetKernelConnectionOptionsWide([Optional] out ushort Buffer, [In, Optional] uint BufferSize, [Optional] out uint OptionsSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetKernelConnectionOptionsWide([In] ref ushort Options);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void StartProcessServerWide([In] uint Flags, [In] ref ushort Options, [In, Optional] IntPtr Reserved);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ConnectProcessServerWide([In] ref ushort RemoteOptions, out ulong Server);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void StartServerWide([In] ref ushort Options);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void OutputServersWide([In] uint OutputControl, [In] ref ushort Machine, [In] uint Flags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetOutputCallbacksWide([MarshalAs(UnmanagedType.Interface)] out IDebugOutputCallbacksWide Callbacks);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetOutputCallbacksWide([In, MarshalAs(UnmanagedType.Interface)] IDebugOutputCallbacksWide Callbacks);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetOutputLinePrefixWide([Optional] out ushort Buffer, [In, Optional] uint BufferSize, [Optional] out uint PrefixSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetOutputLinePrefixWide([In, Optional] ref ushort Prefix);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetIdentityWide([Optional] out ushort Buffer, [In, Optional] uint BufferSize, [Optional] out uint IdentitySize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void OutputIdentityWide([In] uint OutputControl, [In] uint Flags, [In] ref ushort Format);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetEventCallbacksWide([MarshalAs(UnmanagedType.Interface)] out IDebugEventCallbacksWide Callbacks);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetEventCallbacksWide([In, MarshalAs(UnmanagedType.Interface)] IDebugEventCallbacksWide Callbacks);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void CreateProcess2([In] ulong Server, [In] ref sbyte CommandLine, [In] IntPtr OptionsBuffer, [In] uint OptionsBufferSize, [In, Optional] ref sbyte InitialDirectory, [In, Optional] ref sbyte Environment);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void CreateProcess2Wide([In] ulong Server, [In] ref ushort CommandLine, [In] IntPtr OptionsBuffer, [In] uint OptionsBufferSize, [In, Optional] ref ushort InitialDirectory, [In, Optional] ref ushort Environment);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void CreateProcessAndAttach2([In] ulong Server, [In, Optional] ref sbyte CommandLine, [In, Optional] IntPtr OptionsBuffer, [In, Optional] uint OptionsBufferSize, [In, Optional] ref sbyte InitialDirectory, [In, Optional] ref sbyte Environment, [In, Optional] uint ProcessId, [In, Optional] uint AttachFlags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void CreateProcessAndAttach2Wide([In] ulong Server, [In, Optional] ref ushort CommandLine, [In, Optional] IntPtr OptionsBuffer, [In, Optional] uint OptionsBufferSize, [In, Optional] ref ushort InitialDirectory, [In, Optional] ref ushort Environment, [In, Optional] uint ProcessId, [In, Optional] uint AttachFlags);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void PushOutputLinePrefix([In, Optional] ref sbyte NewPrefix, [Optional] out ulong Handle);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void PushOutputLinePrefixWide([In, Optional] ref ushort NewPrefix, [Optional] out ulong Handle);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void PopOutputLinePrefix([In] ulong Handle);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetNumberInputCallbacks(out uint Count);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetNumberOutputCallbacks(out uint Count);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetNumberEventCallbacks([In] uint EventFlags, out uint Count);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetQuitLockString([Optional] out sbyte Buffer, [In, Optional] uint BufferSize, [Optional] out uint StringSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetQuitLockString([In] ref sbyte String);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetQuitLockStringWide([Optional] out ushort Buffer, [In, Optional] uint BufferSize, [Optional] out uint StringSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetQuitLockStringWide([In] ref ushort String);
    }
}

