using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NUnit.Framework;
using NLog;
using Peach.Core.Agent.Monitors;

namespace Peach.Core.Test.OS.Linux
{
	[TestFixture]
	public class PlatformTests
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		[Test]
		public void Test1()
		{
			logger.Debug("Hello World");
			bool value = true;
			Assert.IsTrue(value);
		}

		[Test]
		public void TestCpuUsage()
		{
			Process.ProcessInfo pi = Process.ProcessInfo.Get(1);
			Assert.NotNull(pi);
			Assert.AreEqual(pi.Pid, 1);
			Assert.AreEqual(pi.Name, "init");
			Assert.Greater(pi.KernelTime, 0);
			Assert.Greater(pi.UserTime, 0);

			Assert.Null(Process.ProcessInfo.Get(99999));
		}

	}
}
