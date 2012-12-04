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
	public class ProcessInfo
	{
		#region Public Properties

		public int Id { get; private set; }
		public string ProcessName { get; private set; }
		public bool Responding { get; private set; }

		public TimeSpan TotalProcessorTime { get; private set; }
		public TimeSpan UserProcessorTime { get; private set; }
		public TimeSpan PrivilegedProcessorTime { get; private set; }

		public long PeakVirtualMemorySize64 { get; private set; }
		public long PeakWorkingSet64 { get; private set; }
		public long PrivateMemorySize64 { get; private set; }
		public long VirtualMemorySize64 { get; private set; }
		public long WorkingSet64 { get; private set; }

		#endregion

		#region Public Methods

		private static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		public static ProcessInfo Snapshot(Process p)
		{
			return impl.Snapshot(p);
		}

		#endregion

		#region Base Impl

		private static Impl impl = LoadImpl();

		private static Impl LoadImpl()
		{
			switch (Platform.GetOS())
			{
				case Platform.OS.Windows:
					return new WindowsImpl();
				case Platform.OS.OSX:
					return new MacImpl();
				case Platform.OS.Linux:
					return new LinuxImpl();
				default:
					throw new NotSupportedException();
			}
		}

		private interface Impl
		{
			ProcessInfo Snapshot(Process p);
		}

		#endregion

		#region Windows Impl

		private class WindowsImpl : Impl
		{
			public ProcessInfo Snapshot(Process p)
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

		#endregion

		#region Linux Impl

		private class LinuxImpl : Impl
		{
			private static string StatPath = "/proc/{0}/stat";

			private enum Fields : int
			{
				State = 0,
				UserTime = 11,
				KernelTime = 12,
				Max = 13,
			}

			private static string[] ReadProc(int pid)
			{
				string stat;

				try
				{
					stat = File.ReadAllText(string.Format(StatPath, pid));
				}
				catch (Exception ex)
				{
					logger.Debug("Failed to query information for PID {0}.  {1}", pid, ex.Message);
					return null;
				}

				int start = stat.IndexOf('(');
				int end = stat.LastIndexOf(')');

				if (stat.Length < 2 || start < 0 || end < start)
				{
					logger.Debug("Failed to query information for PID {0}: unable to parse status \"{1}\".", pid, stat);
					return null;
				}

				string before = stat.Substring(0, start);
				string middle = stat.Substring(start + 1, end - start - 1);
				string after = stat.Substring(end + 1);

				string[] strPid = before.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				if (strPid.Length != 1 || strPid[0] != pid.ToString())
				{
					logger.Debug("Failed to query information for PID {0}: stat returned unexpected PID \"{1}\".", pid, strPid[0]);
					return null;
				}

				string[] parts = after.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length < (int)Fields.Max)
				{
					logger.Debug("Failed to query information for PID {0}: stat returned unexpected status \"{1}\".", pid, after);
					return null;
				}

				return parts;
			}

			public ProcessInfo Snapshot(Process p)
			{
				var parts = ReadProc(p.Id);
				if (parts == null)
					throw new InvalidOperationException();

				ProcessInfo pi = new ProcessInfo();

				pi.Id = p.Id;
				pi.ProcessName = p.ProcessName;
				pi.Responding = parts[(int)Fields.State] != "Z";

				pi.UserProcessorTime = TimeSpan.FromTicks(long.Parse(parts[(int)Fields.UserTime]));
				pi.PrivilegedProcessorTime = TimeSpan.FromTicks(long.Parse(parts[(int)Fields.KernelTime]));
				pi.TotalProcessorTime = pi.UserProcessorTime + pi.PrivilegedProcessorTime;

				pi.PrivateMemorySize64 = p.PrivateMemorySize64;         // /proc/[pid]/status VmData
				pi.VirtualMemorySize64 = p.VirtualMemorySize64;         // /proc/[pid]/status VmSize
				pi.PeakVirtualMemorySize64 = p.PeakVirtualMemorySize64; // /proc/[pid]/status VmPeak
				pi.WorkingSet64 = p.WorkingSet64;                       // /proc/[pid]/status VmRSS
				pi.PeakWorkingSet64 = p.PeakWorkingSet64;               // /proc/[pid]/status VmHWM

				return pi;
			}
		}

		#endregion

		#region Mac Impl

		private class MacImpl : Impl
		{
			#region P/Invoke Stuff

			// <libproc.h>
			[DllImport("libc")]
			private static extern int proc_pidinfo(int pid, int flavor, ulong arg, IntPtr buffer, int buffersize);

			// <sys/proc_info.h>
			[StructLayout(LayoutKind.Sequential)]
			struct proc_taskinfo
			{
				public ulong pti_virtual_size;       /* virtual memory size (bytes) */
				public ulong pti_resident_size;      /* resident memory size (bytes) */
				public ulong pti_total_user;         /* total time */
				public ulong pti_total_system;
				public ulong pti_threads_user;       /* existing threads only */
				public ulong pti_threads_system;
				public int pti_policy;               /* default policy for new threads */
				public int pti_faults;               /* number of page faults */
				public int pti_pageins;              /* number of actual pageins */
				public int pti_cow_faults;           /* number of copy-on-write faults */
				public int pti_messages_sent;        /* number of messages sent */
				public int pti_messages_received;    /* number of messages received */
				public int pti_syscalls_mach;        /* number of mach system calls */
				public int pti_syscalls_unix;        /* number of unix system calls */
				public int pti_csw;                  /* number of context switches */
				public int pti_threadnum;            /* number of threads in the task */
				public int pti_numrunning;           /* number of running threads */
				public int pti_priority;             /* task priority*/
			}

			// <sys/proc_info.h>
			private static int PROC_PIDTASKINFO { get { return 4; } }

			// sizeof(struct kinfo_proc)
			private static int kinfo_proc_size { get { return 648; } }

			// <sys/proc.h>
			// Only contains the interesting parts at the beginning of the struct.
			// However, we allocate kinfo_proc_size when calling the sysctl.
			[StructLayout(LayoutKind.Sequential)]
			struct extern_proc
			{
				public int p_starttime_tv_sec;
				public int p_starttime_tv_usec;
				public IntPtr p_vmspace;
				public IntPtr p_sigacts;
				public int p_flag;
				public byte p_stat;
				public int p_pid;
				public int p_oppid;
				public int p_dupfd;
				public IntPtr user_stack;
				public IntPtr exit_thread;
				public int p_debugger;
				public int sigwait;
				public uint p_estcpu;
				public int p_cpticks;
				public uint p_pctcpu;
				public IntPtr p_wchan;
				public IntPtr p_wmesg;
				public uint p_swtime;
				public uint p_slptime;
				public uint p_realtimer_it_interval_tv_sec;
				public uint p_realtimer_it_interval_tv_usec;
				public uint p_realtimer_it_value_tv_sec;
				public uint p_realtimer_it_value_tv_usec;
				public uint p_rtime_tv_sec;
				public uint p_rtime_tv_usec;
				public ulong p_uticks;
				public ulong p_sticks;
				public ulong p_iticks;
			}

			// <sys/sysctl.h>
			private static int CTL_KERN = 1;
			private static int KERN_PROC = 14;
			private static int KERN_PROC_PID = 1;

			// <sys/proc.h>
			private enum p_stat : byte
			{
				SIDL   = 1, // Process being created by fork.
				SRUN   = 2, // Currently runnable.
				SSLEEP = 3, // Sleeping on an address.
				SSTOP  = 4, // Process debugging or suspension.
				SZOMB  = 5, // Awiting collection by parent.
			}

			[DllImport("libc")]
			private static extern IntPtr strerror(int err);

			[DllImport("libc")]
			private static extern int sysctl([MarshalAs(UnmanagedType.LPArray)] int[] name, uint namelen, IntPtr oldp, ref int oldlenp, IntPtr newp, int newlen);

			#endregion

			private static extern_proc? GetKernProc(int pid)
			{
				int[] mib = new int[] {
					CTL_KERN,
					KERN_PROC,
					KERN_PROC_PID,
					pid
				};

				extern_proc? kp = null;
				int len = kinfo_proc_size;
				IntPtr ptr = Marshal.AllocHGlobal(len);
				int ret = sysctl(mib, (uint)mib.Length, ptr, ref len, IntPtr.Zero, 0);
				if (ret != -1)
					kp = (extern_proc)Marshal.PtrToStructure(ptr, typeof(extern_proc));
				Marshal.FreeHGlobal(ptr);
			
				return kp;
			}

			private static proc_taskinfo? GetTaskInfo(int pid)
			{
				proc_taskinfo? ti = null;
				int len = Marshal.SizeOf(typeof(proc_taskinfo));
				IntPtr ptr = Marshal.AllocHGlobal(len);
				int err = proc_pidinfo(pid, PROC_PIDTASKINFO, 0, ptr, len);
				if (err == len)
					ti = (proc_taskinfo)Marshal.PtrToStructure(ptr, typeof(proc_taskinfo));
				Marshal.FreeHGlobal(ptr);
				
				return ti;
			}

			public ProcessInfo Snapshot(Process p)
			{
				var kp = GetKernProc(p.Id);
				if (!kp.HasValue)
					throw new InvalidOperationException();

				var ti = GetTaskInfo(p.Id);
				if (!ti.HasValue)
					throw new InvalidOperationException();

				ProcessInfo pi = new ProcessInfo();
				pi.Id = p.Id;
				pi.ProcessName = p.ProcessName;

				pi.Id = p.Id;
				pi.ProcessName = p.ProcessName;
				pi.Responding = kp.Value.p_stat != (byte)p_stat.SZOMB;

				pi.UserProcessorTime = TimeSpan.FromTicks((long)ti.Value.pti_total_user);
				pi.PrivilegedProcessorTime = TimeSpan.FromTicks((long)ti.Value.pti_total_system);
				pi.TotalProcessorTime = pi.UserProcessorTime + pi.PrivilegedProcessorTime;

				pi.VirtualMemorySize64 = (long)ti.Value.pti_virtual_size;
				pi.WorkingSet64 = (long)ti.Value.pti_resident_size;
				pi.PrivateMemorySize64 = 0;
				pi.PeakVirtualMemorySize64 = 0;
				pi.PeakWorkingSet64 = 0;

				return pi;
			}
		}

		#endregion
	}

}
