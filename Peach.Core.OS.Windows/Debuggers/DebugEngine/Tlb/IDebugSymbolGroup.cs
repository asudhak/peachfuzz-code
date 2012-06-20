namespace Peach.Core.Debuggers.DebugEngine.Tlb
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("F2528316-0F1A-4431-AEED-11D096E1E2AB"), InterfaceType((short) 1)]
    public interface IDebugSymbolGroup
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetNumberSymbols(out uint Number);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void AddSymbol([In] ref sbyte Name, [In, Out] ref uint Index);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void RemoveSymbolByName([In] ref sbyte Name);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void RemoveSymbolByIndex([In] uint Index);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSymbolName([In] uint Index, [Optional] out sbyte Buffer, [In, Optional] uint BufferSize, [Optional] out uint NameSize);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSymbolParameters([In] uint Start, [In] uint Count, out _DEBUG_SYMBOL_PARAMETERS Params);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ExpandSymbol([In] uint Index, [In] int Expand);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void OutputSymbols([In] uint OutputControl, [In] uint Flags, [In] uint Start, [In] uint Count);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void WriteSymbol([In] uint Index, [In] ref sbyte Value);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void OutputAsType([In] uint Index, [In] ref sbyte Type);
    }
}

