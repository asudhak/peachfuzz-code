namespace Peach.Core.Debuggers.DebugEngine.Tlb
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=8)]
    public struct _DEBUG_SYMBOL_SOURCE_ENTRY
    {
        public ulong ModuleBase;
        public ulong Offset;
        public ulong FileNameId;
        public ulong EngineInternal;
        public uint Size;
        public uint Flags;
        public uint FileNameSize;
        public uint StartLine;
        public uint EndLine;
        public uint StartColumn;
        public uint EndColumn;
        public uint Reserved;
    }
}

