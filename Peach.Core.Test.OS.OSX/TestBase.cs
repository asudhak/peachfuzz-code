using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using NLog;
using NLog.Targets;
using NLog.Config;
using NUnit.Framework;
using System.Runtime.InteropServices;

namespace Peach
{
	[SetUpFixture]
	public class TestBase
	{
		[DllImport("libc", SetLastError = true)]
		static extern int open(string path, int flag, int mode);

		[DllImport("libc", SetLastError = true)]
		static extern int flock(int fd, int operation);

		[DllImport("libc", SetLastError = true)]
		static extern int close(int fd);

		const int O_RDWR = 0x0002;
		const int O_CREAT = 0x0200;
		const int LOCK_EX = 0x0002;
		const int LOCK_UN = 0x0008;

		int fd = -1;
		string lockfile = Path.Combine(Path.GetTempPath(), "Peach.Core.Test.OS.OSX.lock");

		void RaiseError(string op)
		{
			int err = Marshal.GetLastWin32Error();
			string msg = string.Format("{0} lockfile '{1}' failed, error {2}", op, lockfile, err);
			throw new Exception(msg);
		}

		[SetUp]
		public void Initialize()
		{
			ColoredConsoleTarget consoleTarget = new ColoredConsoleTarget();
			consoleTarget.Layout = "${date:format=HH\\:MM\\:ss} ${logger} ${message}";

			LoggingConfiguration config = new LoggingConfiguration();
			config.AddTarget("console", consoleTarget);

			LoggingRule rule = new LoggingRule("*", LogLevel.Debug, consoleTarget);
			config.LoggingRules.Add(rule);

			LogManager.Configuration = config;

			// Ensure only 1 instance of the platform tests runs at a time
			fd = open(lockfile, O_RDWR | O_CREAT, Convert.ToInt32("600", 8));
			if (fd == -1)
				RaiseError("Opening");

			if (flock(fd, LOCK_EX) == -1)
				RaiseError("Locking");
		}

		[TearDown]
		public void TearDown()
		{
			if (fd != -1)
			{
				flock(fd, LOCK_UN);
				close(fd);
				fd = -1;
			}
		}
	}
}
