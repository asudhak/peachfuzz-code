namespace Peach.Core.Debuggers.DebugEngine.Tlb
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=4)]
    public struct _DEBUG_PROCESSOR_IDENTIFICATION_AMD64
    {
        public uint Family;
        public uint Model;
        public uint Stepping;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x10)]
        public sbyte[] VendorString;
    }
}

