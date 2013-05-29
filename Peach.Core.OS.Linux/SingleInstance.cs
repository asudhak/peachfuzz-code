using System;
using System.Runtime.InteropServices;
using System.IO;

namespace Peach.Core
{
	[PlatformImpl(Platform.OS.Linux)]
	public class SingleInstanceImpl : SingleInstance
	{
		[DllImport("libc", SetLastError = true)]
		static extern int open(string path, int flag, int mode);

		[DllImport("libc", SetLastError = true)]
		static extern int flock(int fd, int operation);

		[DllImport("libc", SetLastError = true)]
		static extern int close(int fd);

		const int O_RDWR = 0x0002;
		const int O_CREAT = 0x0040;
		const int LOCK_EX = 0x0002;
		const int LOCK_NB = 0x0004;
		const int LOCK_UN = 0x0008;
		const int EWOULDBLOCK = 11;

		object obj;
		int fd;
		string lockfile;
		bool locked;

		void RaiseError(string op)
		{
			int err = Marshal.GetLastWin32Error();
			string msg = string.Format("{0} lockfile '{1}' failed, error {2}", op, lockfile, err);
			throw new Exception(msg);
		}

		public SingleInstanceImpl(string name)
		{
			obj = new object();
			locked = false;
			lockfile = Path.Combine(Path.GetTempPath(), name);
			fd = open(lockfile, O_RDWR | O_CREAT, Convert.ToInt32("600", 8));
			if (fd == -1)
				RaiseError("Opening");
		}

		public override bool TryLock()
		{
			lock (obj)
			{
				if (fd == -1)
					throw new ObjectDisposedException("SingleInstanceImpl");

				if (locked)
					return true;

				if (flock(fd, LOCK_EX) == -1)
				{
					int err = Marshal.GetLastWin32Error();
					if (err != EWOULDBLOCK)
						RaiseError("Locking");
					return false;
				}

				locked = true;
				return true;
			}
		}

		public override void Lock()
		{
			lock (obj)
			{
				if (fd == -1)
					throw new ObjectDisposedException("SingleInstanceImpl");

				if (locked)
					return;

				if (flock(fd, LOCK_EX) == -1)
					RaiseError("Locking");

				locked = true;
			}
		}

		public override void Dispose()
		{
			lock (obj)
			{
				if (fd != -1)
				{
					if (locked)
					{
						flock(fd, LOCK_UN);
						locked = false;
					}
					close(fd);
					fd = -1;
				}
			}
		}
	}
}
