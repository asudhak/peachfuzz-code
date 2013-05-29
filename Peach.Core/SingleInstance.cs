using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using NLog;
using Peach.Core;
using System.Runtime.CompilerServices;

namespace Peach.Core
{
	public abstract class SingleInstance : PlatformFactory<SingleInstance>, IDisposable
	{
		public abstract void Dispose();
		public abstract bool TryLock();
		public abstract void Lock();
	}
}
