namespace Peach.Core.Debuggers.DebugEngine.Tlb
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=4)]
    public struct _DEBUG_CREATE_PROCESS_OPTIONS
    {
        public uint CreateFlags;
        public uint EngCreateFlags;
        public uint VerifierFlags;
        public uint Reserved;
    }
}

