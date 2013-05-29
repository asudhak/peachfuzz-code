using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

using Peach.Core.OS.Linux.Agent.Monitors;
using System.Threading;

namespace Peach.Core.Test.OS.Linux.Agent.Monitors
{
	[TestFixture]
	public class LinuxDebuggerTests
	{
		[Test]
		public void TestFault()
		{
			var args = new Dictionary<string, Variant>();
			args["Executable"] = new Variant("CrashingFileConsumer");
			args["Arguments"] = new Variant("Peach.Core.Test.OS.Linux.dll");
			args["RestartOnEachTest"] = new Variant("true");

			var m = new LinuxDebugger(null, null, args);
			m.SessionStarting();
			m.IterationStarting(1, false);
			Thread.Sleep(1000);
			m.IterationFinished();
			Assert.AreEqual(true, m.DetectedFault());
			Fault fault = m.GetMonitorData();
			Assert.NotNull(fault);
			Assert.AreEqual(1, fault.collectedData.Count);
			Assert.True(fault.collectedData.ContainsKey("StackTrace.txt"));
			Assert.Greater(fault.collectedData["StackTrace.txt"].Length, 0);
			Assert.True(fault.description.Contains("PossibleStackCorruption"));
			m.SessionFinished();
			m.StopMonitor();
		}

		[Test]
		public void TestNoFault()
		{
			var args = new Dictionary<string, Variant>();
			args["Executable"] = new Variant("CrashingFileConsumer");

			var m = new LinuxDebugger(null, null, args);
			m.SessionStarting();
			m.IterationStarting(1, false);
			Thread.Sleep(1000);
			m.IterationFinished();
			Assert.AreEqual(false, m.DetectedFault());
			m.SessionFinished();
			m.StopMonitor();
		}

		[Test]
		public void TestMissingProgram()
		{
			var args = new Dictionary<string, Variant>();
			args["Executable"] = new Variant("MissingProgram");

			var m = new LinuxDebugger(null, null, args);
			try
			{
				m.SessionStarting();
				Assert.Fail("should throw");
			}
			catch (PeachException ex)
			{
				Assert.AreEqual("GDB was unable to start 'MissingProgram'.", ex.Message);
			}
		}

		[Test]
		public void TestMissingGdb()
		{
			var args = new Dictionary<string, Variant>();
			args["Executable"] = new Variant("MissingProgram");
			args["GdbPath"] = new Variant("MissingGdb");

			var m = new LinuxDebugger(null, null, args);

			try
			{
				m.SessionStarting();
				Assert.Fail("should throw");
			}
			catch (PeachException ex)
			{
				var exp = "Could not start debugger 'MissingGdb'.";
				var act = ex.Message.Substring(0, exp.Length);
				Assert.AreEqual(exp, act);
			}
		}

		[Test]
		public void TestCpuKill()
		{
			var args = new Dictionary<string, Variant>();
			args["Executable"] = new Variant("CrashableServer");
			args["Arguments"] = new Variant("127.0.0.1 12346");
			args["StartOnCall"] = new Variant("Foo");

			var m = new LinuxDebugger(null, null, args);
			m.SessionStarting();
			m.IterationStarting(1, false);

			m.Message("Action.Call", new Variant("Foo"));
			Thread.Sleep(1000);

			var before = DateTime.Now;
			m.IterationFinished();
			var after = DateTime.Now;

			var span = (after - before);

			Thread.Sleep(1000);
			Assert.AreEqual(false, m.DetectedFault());
			m.SessionFinished();
			m.StopMonitor();

			Assert.GreaterOrEqual(span.TotalSeconds, 0.0);
			Assert.LessOrEqual(span.TotalSeconds, 0.5);
		}
	}
}
