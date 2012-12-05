using System;

namespace Peach.Core
{
	[PlatformImpl(Platform.OS.Windows)]
	public class ProcessInfoImpl : IProcessInfo
	{
		public ProcessInfo Snapshot(System.Diagnostics.Process p)
		{
			var pi = new ProcessInfo();

			pi.Id = p.Id;
			pi.ProcessName = p.ProcessName;
			pi.Responding = p.Responding;

			pi.TotalProcessorTime = p.TotalProcessorTime;
			pi.UserProcessorTime = p.UserProcessorTime;
			pi.PrivilegedProcessorTime = p.PrivilegedProcessorTime;

			pi.PeakVirtualMemorySize64 = p.PeakVirtualMemorySize64;
			pi.PeakWorkingSet64 = p.PeakWorkingSet64;
			pi.PrivateMemorySize64 = p.PrivateMemorySize64;
			pi.VirtualMemorySize64 = p.VirtualMemorySize64;
			pi.WorkingSet64 = p.WorkingSet64;

			return pi;
		}
	}
}
