using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using Peach.Core;
using NUnit.Framework;
using NLog;

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
			using (Process p = Process.GetProcessById(1))
			{
				var pi = ProcessInfo.Instance.Snapshot(p);
				Assert.NotNull(pi);
				Assert.AreEqual(1, pi.Id);
				Assert.AreEqual("init", pi.ProcessName);
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

		[Test]
		public void TestProcess()
		{
			MakeProcesses(1000);
		}

		public void MakeProcesses(int max)
		{
			int i = 0;
			try
			{
				for (; i < max; ++i)
				{
					if ((i % 500) == 0)
						logger.Debug("Starting Process #{0}", i + 1);
					using (Process p = new Process())
					{
						p.StartInfo.FileName = "/bin/echo";
						p.StartInfo.Arguments = "-n \"\"";
						p.StartInfo.UseShellExecute = false;
						p.StartInfo.CreateNoWindow = true;
						p.StartInfo.WorkingDirectory = "/";
						p.Start();
					}
				}
			}
			finally
			{
				Assert.AreEqual(max, i);
			}
		}

		[Test,Ignore]
		public void TestOutOfMemory()
		{
			// MONO_GC_PARAMS=max-heap-size=1g

			for (int i = 0; i < 10; ++i)
			{
				logger.Debug("-------------------> Pass #{0}", i);

				TriggerOutOfMemory();

				MakeProcesses(5000);
			}
		}

		private static void TriggerOutOfMemory()
		{
			long len = 0;

			MemoryStream src = new MemoryStream();
			MemoryStream dst = new MemoryStream();

			byte[] b = new byte[1024 * 1024];
			src.Write(b, 0, b.Length);

			try
			{
				while (true)
				{
					src.Seek(0, SeekOrigin.Begin);
					src.CopyTo(dst, (int)src.Length);

					len += src.Length;
				}
			}
			catch (OutOfMemoryException ex)
			{
				logger.Info("-------------------> Out Of Memory: Wrote {1}, {0}", ex, Utilities.PrettyBytes(len));
			}
		}
	}
}
