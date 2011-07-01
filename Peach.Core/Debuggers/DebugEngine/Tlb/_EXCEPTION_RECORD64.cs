namespace Peach.Core.Debuggers.DebugEngine.Tlb
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=8)]
    public struct _EXCEPTION_RECORD64
    {
        public uint ExceptionCode;
        public uint ExceptionFlags;
        public long ExceptionRecord;
        public long ExceptionAddress;
        public uint NumberParameters;
        public uint __unusedAlignment;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=15)]
        public long[] ExceptionInformation;
    }
}

