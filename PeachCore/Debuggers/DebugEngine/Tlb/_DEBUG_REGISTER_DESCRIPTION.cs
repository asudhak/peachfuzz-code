namespace Peach.Core.Debuggers.DebugEngine.Tlb
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=8)]
    public struct _DEBUG_REGISTER_DESCRIPTION
    {
        public uint Type;
        public uint Flags;
        public uint SubregMaster;
        public uint SubregLength;
        public ulong SubregMask;
        public uint SubregShift;
        public uint Reserved0;
    }
}

