using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using Peach.Core;
using NUnit.Framework;
using NLog;

namespace Peach.Core.Test.OS.OSX
{
	[TestFixture]
	public class PlatformTests
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		[Test]
		public void Test1()
		{
			logger.Debug("Hello World");
			logger.Debug(new FileNotFoundException().Message);
			bool value = true;
			Assert.IsTrue(value);
		}

		[Test]
		public void TestCpuUsage()
		{
			using (Process p = Process.GetProcessById(1))
			{
				var pi = ProcessInfo.Instance.Snapshot(p);
				Assert.NotNull(pi);
				Assert.AreEqual(1, pi.Id);
				Assert.AreEqual("launchd", pi.ProcessName);
				Assert.Greater(pi.PrivilegedProcessorTicks, 0);
				Assert.Greater(pi.UserProcessorTicks, 0);
			}

			using (Process p = new Process())
			{
				var si = new ProcessStartInfo();
				si.FileName = "/bin/ls";
				p.StartInfo = si;
				p.Start();
				p.WaitForExit();
				Assert.True(p.HasExited);
				p.Close();

				Assert.Throws<ArgumentException>(delegate() { ProcessInfo.Instance.Snapshot(p); });
			}
		}

	}
}
