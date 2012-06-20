namespace Peach.Core.Debuggers.DebugEngine.Tlb
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=8)]
    public struct _DEBUG_VALUE
    {
        public __MIDL___MIDL_itf_DbgEng_0001_0073_0127 u;
        public uint TailOfRawBytes;
        public uint Type;
    }
}

