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
		/// <summary>
		/// Returns a populated ProcessInfo instance.
		/// throws ArgumentException if the Process is not valid.
		/// </summary>
		/// <param name="p">Process to obtain info about.</param>
		/// <returns>Information about the process.</returns>
		ProcessInfo Snapshot(Process p);
	}

	public class ProcessInfo : StaticPlatformFactory<IProcessInfo>
	{
		public int Id;
		public string ProcessName;
		public bool Responding;

		public ulong TotalProcessorTicks;
		public ulong UserProcessorTicks;
		public ulong PrivilegedProcessorTicks;

		public long PeakVirtualMemorySize64;
		public long PeakWorkingSet64;
		public long PrivateMemorySize64;
		public long VirtualMemorySize64;
		public long WorkingSet64;
	}
}
