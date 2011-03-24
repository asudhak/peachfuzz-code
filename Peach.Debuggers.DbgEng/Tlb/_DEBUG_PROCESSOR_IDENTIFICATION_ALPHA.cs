namespace Peach.Debuggers.DbgEng.Tlb
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=4)]
    public struct _DEBUG_PROCESSOR_IDENTIFICATION_ALPHA
    {
        public uint Type;
        public uint Revision;
    }
}

