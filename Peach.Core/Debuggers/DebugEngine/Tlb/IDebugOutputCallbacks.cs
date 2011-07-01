namespace Peach.Core.Debuggers.DebugEngine.Tlb
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("4BF58045-D654-4C40-B0AF-683090F356DC"), InterfaceType((short) 1)]
    public interface IDebugOutputCallbacks
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void Output([In] uint Mask, [In] [MarshalAs(UnmanagedType.LPStr)] string Text);
    }
}

