namespace Peach.Core.Debuggers.DebugEngine.Tlb
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("67721FE9-56D2-4A44-A325-2B65513CE6EB"), InterfaceType((short) 1)]
    public interface IDebugOutputCallbacks2
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void Output([In] uint Mask, [In] string Text);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetInterestMask(out uint Mask);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void Output2([In] uint Which, [In] uint Flags, [In] ulong Arg, [In, Optional] string Text);
    }
}

