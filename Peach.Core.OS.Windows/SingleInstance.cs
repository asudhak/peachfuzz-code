using System;
using System.Threading;

namespace Peach.Core
{
	[PlatformImpl(Platform.OS.Windows)]
	public class SingleInstanceImpl : SingleInstance
	{
		object obj;
		Mutex mutex;
		bool locked;

		public SingleInstanceImpl(string name)
		{
			this.locked = false;
			this.obj = new object();
			this.mutex = new Mutex(false, "Global\\" + name);
		}

		public override void Dispose()
		{
			lock (obj)
			{
				if (mutex != null)
				{
					if (locked)
					{
						mutex.ReleaseMutex();
						locked = false;
					}

					mutex.Dispose();
					mutex = null;
				}
			}
		}

		public override bool TryLock()
		{
			lock (obj)
			{
				if (mutex == null)
					throw new ObjectDisposedException("SingleInstanceImpl");

				if (locked)
					return true;

				try
				{
					locked = mutex.WaitOne(0);
					return locked;
				}
				catch (AbandonedMutexException)
				{
					return TryLock();
				}
			}
		}

		public override void Lock()
		{
			lock (obj)
			{
				if (mutex == null)
					throw new ObjectDisposedException("SingleInstanceImpl");

				if (locked)
					return;

				try
				{
					mutex.WaitOne();
					locked = true;
				}
				catch (AbandonedMutexException)
				{
					Lock();
				}
			}
		}
	}
}
