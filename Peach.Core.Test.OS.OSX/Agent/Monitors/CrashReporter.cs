using System;
using System.Collections;
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
			// ProcessName argument not provided to the monitor
			// When no crashing program is run, the monitor should not detect a fault

			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			string peach = "";
			string process = null;
			bool shouldFault = false;

			RunProcess(peach, process, shouldFault, args);
		}

		[Test]
		public void NoProcessFault()
		{
			// ProcessName argument not provided to the monitor
			// When crashing program is run, the monitor should detect a fault

			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			string peach = "qwertyuiopasdfghjklzxcvbnm";
			string process = "CrashingProgram";
			bool shouldFault = true;

			Hashtable hash = RunProcess(peach, process, shouldFault, args);

			Assert.AreNotEqual(0, hash.Count);
			Assert.True(hash.ContainsKey("CrashReporter"));
			string data = hash["CrashReporter"] as string;
			Assert.AreNotEqual(null, data);
		}

		[Test]
		public void ProcessFault()
		{
			// Correct ProcessName argument is provided to the monitor
			// When crashing program is run, the monitor should detect a fault

			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["ProcessName"] = new Variant("CrashingProgram");
			string peach = "qwertyuiopasdfghjklzxcvbnm";
			string process = "CrashingProgram";
			bool shouldFault = true;

			Hashtable hash = RunProcess(peach, process, shouldFault, args);

			Assert.AreNotEqual(0, hash.Count);
			Assert.True(hash.ContainsKey("CrashReporter"));
			string data = hash["CrashReporter"] as string;
			Assert.AreNotEqual(null, data);
		}

		[Test]
		public void WrongProcessFault()
		{
			// Incorrect ProcessName argument is provided to the monitor
			// When crashing program is run, the monitor should not detect a fault

			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["ProcessName"] = new Variant("WrongCrashingProgram");
			string peach = "qwertyuiopasdfghjklzxcvbnm";
			string process = "CrashingProgram";
			bool shouldFault = false;

			RunProcess(peach, process, shouldFault, args);
		}

		private static Hashtable RunProcess(string peach, string process, bool shouldFault, Dictionary<string, Variant> args)
		{
			CrashReporter reporter = new CrashReporter("name", args);
			reporter.SessionStarting();
			reporter.IterationStarting(0, false);
			if (process != null)
			{
				System.Diagnostics.Process p = new System.Diagnostics.Process();
				p.StartInfo = new System.Diagnostics.ProcessStartInfo();
				p.StartInfo.EnvironmentVariables["PEACH"] = peach;
				p.StartInfo.UseShellExecute = false;
				p.StartInfo.FileName = process;
				p.Start();
			}
			reporter.IterationFinished();
			Assert.AreEqual(shouldFault, reporter.DetectedFault());
			System.Collections.Hashtable hash = new System.Collections.Hashtable();
			reporter.GetMonitorData(hash);
			reporter.StopMonitor();
			return hash;
		}
	}
}

