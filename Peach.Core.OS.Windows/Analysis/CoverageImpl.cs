using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using Peach.Core.Debuggers.WindowsSystem;

namespace Peach.Core.Analysis
{
	////public class CoverageImpl : Peach.Core.Analysis.Coverage
	////{
	////    /// <summary>
	////    /// Returns a list of basic blocks for an executable.
	////    /// </summary>
	////    /// <param name="executable"></param>
	////    /// <returns></returns>
	////    public override List<ulong> BasicBlocksForExecutable(string executable)
	////    {
	////        List<ulong> addr = new List<ulong>();

	////        ProcessStartInfo startInfo = new ProcessStartInfo();
	////        startInfo.FileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "basicblocks.exe");
	////        startInfo.Arguments = executable;
	////        startInfo.UseShellExecute = false;
	////        startInfo.CreateNoWindow = true;
	////        startInfo.RedirectStandardOutput = true;

	////        var proc = new System.Diagnostics.Process();
	////        proc.StartInfo = startInfo;
	////        proc.Start();
			
	////        string line = null;
	////        do
	////        {
	////            line = proc.StandardOutput.ReadLine();
	////            if (line != null)
	////                addr.Add(UInt64.Parse(line));
	////        }
	////        while (line != null);

	////        proc.WaitForExit();

	////        return addr;
	////    }

	////    public override List<ulong> CodeCoverageForExecutable(string executable, string arguments, List<ulong> basicBlocks = null)
	////    {
	////        _executable = executable;
	////        _arguments = arguments;
	////        _basicBlocks = basicBlocks;

	////        Start();
	////        return coverage;
	////    }

	////    bool _breakpointsSet = false;
	////    SystemDebugger _dbg;
	////    string _executable;
	////    string _arguments;
	////    List<ulong> _basicBlocks;
	////    public List<ulong> coverage = new List<ulong>();

	////    public CoverageImpl()
	////    {
	////    }

	////    protected void Start()
	////    {
	////        // Do we need to get basic blocks?
	////        if (_basicBlocks.Count == 0)
	////            _basicBlocks = BasicBlocksForExecutable(_executable);

	////        _dbg = SystemDebugger.CreateProcess(_executable + " " + _arguments);
	////        _dbg.HandleBreakPoint = new HandleBreakpoint(HandleBreakPoint);
	////        _dbg.HandleLoadDll = new Debuggers.WindowsSystem.HandleLoadDll(HandleLoadDll);
	////        _dbg.MainLoop();
	////    }

	////    protected void HandleBreakPoint(UnsafeMethods.DEBUG_EVENT e)
	////    {
	////        coverage.Add((ulong)e.u.Exception.ExceptionRecord.ExceptionAddress.ToInt64());
	////    }

	////    protected void HandleLoadDll(UnsafeMethods.DEBUG_EVENT e, string moduleName)
	////    {
	////        // Set breakpoints
	////        if (_breakpointsSet)
	////            return;

	////        IntPtr baseAddr = UnsafeMethods.GetModuleHandle(_executable);

	////        foreach(ulong addr in _basicBlocks)
	////            _dbg.SetBreakpoint(_executable, (ulong)baseAddr.ToInt64() + addr);
	////    }
	////}
}
