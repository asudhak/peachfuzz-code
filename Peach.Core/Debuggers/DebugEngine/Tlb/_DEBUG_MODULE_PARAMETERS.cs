namespace Peach.Core.Debuggers.DebugEngine.Tlb
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=8)]
    public struct _DEBUG_MODULE_PARAMETERS
    {
        public ulong Base;
        public uint Size;
        public uint TimeDateStamp;
        public uint CheckSum;
        public uint Flags;
        public uint SymbolType;
        public uint ImageNameSize;
        public uint ModuleNameSize;
        public uint LoadedImageNameSize;
        public uint SymbolFileNameSize;
        public uint MappedImageNameSize;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=2)]
        public ulong[] Reserved;
    }
}

