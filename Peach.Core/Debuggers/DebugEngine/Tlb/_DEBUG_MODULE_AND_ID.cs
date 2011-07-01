namespace Peach.Core.Debuggers.DebugEngine.Tlb
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=8)]
    public struct _DEBUG_MODULE_AND_ID
    {
        public ulong ModuleBase;
        public ulong Id;
    }
}

