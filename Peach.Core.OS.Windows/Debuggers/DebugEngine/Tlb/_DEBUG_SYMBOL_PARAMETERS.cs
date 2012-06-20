namespace Peach.Core.Debuggers.DebugEngine.Tlb
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=8)]
    public struct _DEBUG_SYMBOL_PARAMETERS
    {
        public ulong Module;
        public uint TypeId;
        public uint ParentSymbol;
        public uint SubElements;
        public uint Flags;
        public ulong Reserved;
    }
}

