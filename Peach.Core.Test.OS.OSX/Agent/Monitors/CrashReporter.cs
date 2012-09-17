using System;
using System.Collections.Generic;
using Peach.Core;
using Peach.Core.Agent.Monitors;
using NUnit.Framework;

namespace Peach.Core.Test.Agent.Monitors
{
	[TestFixture]
	public class CrashReporterTest
	{
		[Test]
		public void NoProcessNoFault()
		{
			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			CrashReporter reporter = new CrashReporter("name", args);
			reporter.SessionStarting();
			reporter.IterationStarting(0, false);
			reporter.IterationFinished();
			Assert.False(reporter.DetectedFault());
			reporter.StopMonitor();
		}

		[Test]
		public void NoProcessFault()
		{
			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			CrashReporter reporter = new CrashReporter("name", args);
			reporter.SessionStarting();
			reporter.IterationStarting(0, false);
			System.Diagnostics.Process p = new System.Diagnostics.Process();
			p.StartInfo = new System.Diagnostics.ProcessStartInfo();
			p.StartInfo.EnvironmentVariables["PEACH"] = "qwertyuiopasdfghjklzxcvbnm";
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.FileName = "/Users/seth/src/git/peach/output/osx_debug/bin/CrashingProgram";
			p.Start();
			reporter.IterationFinished();
			Assert.True(reporter.DetectedFault());
			System.Collections.Hashtable hash = new System.Collections.Hashtable();
			reporter.GetMonitorData(hash);
			Assert.AreNotEqual(0, hash.Count);
			Assert.True(hash.ContainsKey("CrashReporter"));
			string data = hash["CrashReporter"] as string;
			Assert.AreNotEqual(null, data);
			reporter.StopMonitor();
		}

		[Test]
		public void ProcessFault()
		{
			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["ProcessName"] = new Variant("CrashingProgram");
			CrashReporter reporter = new CrashReporter("name", args);
			reporter.SessionStarting();
			reporter.IterationStarting(0, false);
			System.Diagnostics.Process p = new System.Diagnostics.Process();
			p.StartInfo = new System.Diagnostics.ProcessStartInfo();
			p.StartInfo.EnvironmentVariables["PEACH"] = "qwertyuiopasdfghjklzxcvbnm";
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.FileName = "/Users/seth/src/git/peach/output/osx_debug/bin/CrashingProgram";
			p.Start();
			reporter.IterationFinished();
			Assert.True(reporter.DetectedFault());
			System.Collections.Hashtable hash = new System.Collections.Hashtable();
			reporter.GetMonitorData(hash);
			Assert.AreNotEqual(0, hash.Count);
			Assert.True(hash.ContainsKey("CrashReporter"));
			string data = hash["CrashReporter"] as string;
			Assert.AreNotEqual(null, data);
			reporter.StopMonitor();
		}

		[Test]
		public void WrongProcessFault()
		{
			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["ProcessName"] = new Variant("WrongCrashingProgram");
			CrashReporter reporter = new CrashReporter("name", args);
			reporter.SessionStarting();
			reporter.IterationStarting(0, false);
			System.Diagnostics.Process p = new System.Diagnostics.Process();
			p.StartInfo = new System.Diagnostics.ProcessStartInfo();
			p.StartInfo.EnvironmentVariables["PEACH"] = "qwertyuiopasdfghjklzxcvbnm";
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.FileName = "/Users/seth/src/git/peach/output/osx_debug/bin/CrashingProgram";
			p.Start();
			reporter.IterationFinished();
			Assert.False(reporter.DetectedFault());
			reporter.StopMonitor();
		}
	}
}

