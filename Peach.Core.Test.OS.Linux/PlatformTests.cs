using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NUnit.Framework;
using NLog;

namespace Peach.Core.Test.OS.Linux
{
	[TestFixture]
	public class PlatformTests
	{
		class ProcessInfo
		{
			public int Pid { get; private set; }
			public string Name { get; private set; }
			public uint UserTime { get; private set; }
			public uint KernelTime { get; private set; }

			private static string StatPath = "/proc/{0}/stat";

			private ProcessInfo()
			{
			}

			public static ProcessInfo Get(int pid)
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

				string[] strPid = before.Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
				if (strPid.Length != 1 || strPid[0] != pid.ToString())
				{
					logger.Debug("Failed to query information for PID {0}: stat returned unexpected PID \"{1}\".", pid, strPid[0]);
					return null;
				}

				string[] parts = after.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length < 13)
				{
					logger.Debug("Failed to query information for PID {0}: stat returned unexpected status \"{1}\".", pid, after);
					return null;
				}

				uint userTime;
				if (!uint.TryParse(parts[11], out userTime))
				{
					logger.Debug("Failed to query information for PID {0}: bad user time \"{1}\".", pid, parts[11]);
					return null;
				}

				uint kernelTime;
				if (!uint.TryParse(parts[12], out kernelTime))
				{
					logger.Debug("Failed to query information for PID {0}: bad kernel time \"{1}\".", pid, parts[12]);
				}

				ProcessInfo pi = new ProcessInfo();
				pi.Pid = pid;
				pi.Name = middle;
				pi.UserTime = userTime;
				pi.KernelTime = kernelTime;
				return pi;
			}
		}

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
			ProcessInfo pi = ProcessInfo.Get(1);
			Assert.NotNull(pi);
			Assert.AreEqual(pi.Pid, 1);
			Assert.AreEqual(pi.Name, "init");
			Assert.Greater(pi.KernelTime, 0);
			Assert.Greater(pi.UserTime, 0);

			Assert.Null(ProcessInfo.Get(99999));
		}

	}
}
