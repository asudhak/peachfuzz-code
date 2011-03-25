namespace Peach.Core.Debuggers.DebugEngine.Tlb
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=4)]
    public struct _DEBUG_HANDLE_DATA_BASIC
    {
        public uint TypeNameSize;
        public uint ObjectNameSize;
        public uint Attributes;
        public uint GrantedAccess;
        public uint HandleCount;
        public uint PointerCount;
    }
}

