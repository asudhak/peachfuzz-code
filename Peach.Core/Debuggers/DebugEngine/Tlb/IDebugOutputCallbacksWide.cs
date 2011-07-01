namespace Peach.Core.Debuggers.DebugEngine.Tlb
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short) 1), Guid("4C7FD663-C394-4E26-8EF1-34AD5ED3764C")]
    public interface IDebugOutputCallbacksWide
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void Output([In] uint Mask, [In] ref ushort Text);
    }
}

