using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using NLog;
using Peach.Core;
using System.Runtime.CompilerServices;

namespace Peach.Core
{
	/// <summary>
	/// Helper class to get information on a process.  The built in 
	/// methods on mono aren't 100% implemented.
	/// </summary>
	
	public interface IProcessInfo
	{
		ProcessInfo Snapshot(Process p);
	}

	public class ProcessInfo : PlatformFactory<IProcessInfo>
	{
		public int Id;
		public string ProcessName;
		public bool Responding;

		public TimeSpan TotalProcessorTime;
		public TimeSpan UserProcessorTime;
		public TimeSpan PrivilegedProcessorTime;

		public long PeakVirtualMemorySize64;
		public long PeakWorkingSet64;
		public long PrivateMemorySize64;
		public long VirtualMemorySize64;
		public long WorkingSet64;
	}
}
