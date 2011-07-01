namespace Peach.Core.Debuggers.DebugEngine.Tlb
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=4)]
    public struct _DEBUG_LAST_EVENT_INFO_EXIT_PROCESS
    {
        public uint ExitCode;
    }
}

