using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Peach.Core;
using Peach.Core.Agent.Monitors;
using NUnit.Framework;

namespace Peach.Core.Test.Agent.Monitors
{
	[TestFixture]
	public class CrashWranglerTest
	{
		[Test]
		public void BadHandler()
		{
			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["Command"] = new Variant("foo");
			args["ExecHandler"] = new Variant("foo");
			
			CrashWrangler w = new CrashWrangler(null, "name", args);
			string expected = "CrashWrangler could not start handler \"foo\" - No such file or directory.";
			Assert.Throws<PeachException>(delegate() { w.SessionStarting(); }, expected);
		}

		[Test]
		public void BadCommand()
		{
			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["Command"] = new Variant("foo");

			CrashWrangler w = new CrashWrangler(null, "name", args);
			string expected = "CrashWrangler handler could not find command \"foo\".";
			Assert.Throws<PeachException>(delegate() { w.SessionStarting(); }, expected);
		}
		
		[Test]
		public void TestNoFault()
		{
			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["Command"] = new Variant("echo");
			args["Arguments"] = new Variant("hello");

			CrashWrangler w = new CrashWrangler(null, "name", args);
			w.SessionStarting();
			w.IterationStarting(0, false);
			Thread.Sleep(1000);
			w.IterationFinished();
			Assert.AreEqual(false, w.DetectedFault());
			w.SessionFinished();
			w.StopMonitor();
		}

		[Test]
		public void TestStopping()
		{
			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["Command"] = new Variant("nc");
			args["Arguments"] = new Variant("-l 12345");

			CrashWrangler w = new CrashWrangler(null, "name", args);
			w.SessionStarting();
			w.IterationStarting(0, false);
			Thread.Sleep(1000);
			w.IterationFinished();
			Assert.AreEqual(false, w.DetectedFault());
			w.SessionFinished();
			w.StopMonitor();
		}

		[Test]
		public void TestStartOnCall()
		{
			Variant foo = new Variant("foo");

			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["Command"] = new Variant("nc");
			args["Arguments"] = new Variant("-l 12345");
			args["StartOnCall"] = foo;
			args["WaitForExitTimeout"] = new Variant("2000");
			args["NoCpuKill"] = new Variant("true");

			CrashWrangler w = new CrashWrangler(null, "name", args);

			w.Message("Action.Call", foo);
			Thread.Sleep(1000);

			var before = DateTime.Now;
			w.IterationFinished();
			var after = DateTime.Now;

			var span = (after - before);

			Assert.AreEqual(false, w.DetectedFault());

			w.SessionFinished();
			w.StopMonitor();

			Assert.GreaterOrEqual(span.TotalSeconds, 1.8);
			Assert.LessOrEqual(span.TotalSeconds, 2.2);
		}

		[Test]
		public void TestCpuKill()
		{
			Variant foo = new Variant("foo");
			
			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["Command"] = new Variant("nc");
			args["Arguments"] = new Variant("-l 12345");
			args["StartOnCall"] = foo;

			CrashWrangler w = new CrashWrangler(null, "name", args);

			w.Message("Action.Call", foo);
			Thread.Sleep(1000);

			var before = DateTime.Now;
			w.IterationFinished();
			var after = DateTime.Now;

			var span = (after - before);

			Assert.AreEqual(false, w.DetectedFault());

			w.SessionFinished();
			w.StopMonitor();

			Assert.GreaterOrEqual(span.TotalSeconds, 0.0);
			Assert.LessOrEqual(span.TotalSeconds, 0.5);
		}

		[Test]
		public void TestExitOnCallNoFault()
		{
			Variant foo = new Variant("foo");
			Variant bar = new Variant("bar");

			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["Command"] = new Variant("echo");
			args["Arguments"] = new Variant("hello");
			args["StartOnCall"] = foo;
			args["WaitForExitOnCall"] = bar;
			args["NoCpuKill"] = new Variant("true");

			CrashWrangler w = new CrashWrangler(null, "name", args);

			w.Message("Action.Call", foo);
			w.Message("Action.Call", bar);

			w.IterationFinished();

			Assert.AreEqual(false, w.DetectedFault());

			w.SessionFinished();
			w.StopMonitor();
		}

		[Test]
		public void TestExitOnCallFault()
		{
			Variant foo = new Variant("foo");
			Variant bar = new Variant("bar");

			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["Command"] = new Variant("nc");
			args["Arguments"] = new Variant("-l 12345");
			args["StartOnCall"] = foo;
			args["WaitForExitOnCall"] = bar;
			args["WaitForExitTimeout"] = new Variant("2000");
			args["NoCpuKill"] = new Variant("true");

			CrashWrangler w = new CrashWrangler(null, "name", args);

			w.Message("Action.Call", foo);
			w.Message("Action.Call", bar);

			w.IterationFinished();

			Assert.AreEqual(true, w.DetectedFault());
			Fault f = w.GetMonitorData();
			Assert.NotNull(f);
			Assert.AreEqual("ProcessFailedToExit", f.folderName);

			w.SessionFinished();
			w.StopMonitor();
		}

		[Test]
		public void TestExitTime()
		{
			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["Command"] = new Variant("nc");
			args["Arguments"] = new Variant("-l 12345");
			args["RestartOnEachTest"] = new Variant("true");

			CrashWrangler w = new CrashWrangler(null, "name", args);
			w.SessionStarting();
			w.IterationStarting(1, false);

			var before = DateTime.Now;
			w.IterationFinished();
			var after = DateTime.Now;

			var span = (after - before);

			Assert.AreEqual(false, w.DetectedFault());

			w.SessionFinished();
			w.StopMonitor();

			Assert.GreaterOrEqual(span.TotalSeconds, 0.0);
			Assert.LessOrEqual(span.TotalSeconds, 0.1);
		}

		[Test]
		public void TestExitEarlyFault()
		{
			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["Command"] = new Variant("echo");
			args["Arguments"] = new Variant("hello");
			args["FaultOnEarlyExit"] = new Variant("true");

			CrashWrangler w = new CrashWrangler(null, "name", args);
			w.IterationStarting(1, false);

			System.Threading.Thread.Sleep(1000);

			w.IterationFinished();

			Assert.AreEqual(true, w.DetectedFault());
			Fault f = w.GetMonitorData();
			Assert.NotNull(f);
			Assert.AreEqual("ProcessExitedEarly", f.folderName);

			w.SessionFinished();
			w.StopMonitor();
		}

		[Test]
		public void TestExitEarlyFault1()
		{
			Variant foo = new Variant("foo");
			Variant bar = new Variant("bar");

			// FaultOnEarlyExit doesn't fault when stop message is sent

			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["Command"] = new Variant("echo");
			args["Arguments"] = new Variant("hello");
			args["StartOnCall"] = foo;
			args["WaitForExitOnCall"] = bar;
			args["FaultOnEarlyExit"] = new Variant("true");

			CrashWrangler w = new CrashWrangler(null, "name", args);
			w.SessionStarting();
			w.IterationStarting(1, false);

			w.Message("Action.Call", foo);
			w.Message("Action.Call", bar);

			w.IterationFinished();

			Assert.AreEqual(false, w.DetectedFault());

			w.SessionFinished();
			w.StopMonitor();
		}

		[Test]
		public void TestExitEarlyFault2()
		{
			Variant foo = new Variant("foo");

			// FaultOnEarlyExit faults when StartOnCall is used and stop message is not sent

			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["Command"] = new Variant("echo");
			args["Arguments"] = new Variant("hello");
			args["StartOnCall"] = foo;
			args["FaultOnEarlyExit"] = new Variant("true");

			CrashWrangler w = new CrashWrangler(null, "name", args);
			w.SessionStarting();
			w.IterationStarting(1, false);

			w.Message("Action.Call", foo);

			System.Threading.Thread.Sleep(1000);

			w.IterationFinished();

			Assert.AreEqual(true, w.DetectedFault());
			Fault f = w.GetMonitorData();
			Assert.NotNull(f);
			Assert.AreEqual("ProcessExitedEarly", f.folderName);


			w.SessionFinished();
			w.StopMonitor();
		}

		[Test]
		public void TestExitEarlyFault3()
		{
			Variant foo = new Variant("foo");

			// FaultOnEarlyExit doesn't fault when StartOnCall is used

			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["Command"] = new Variant("nc");
			args["Arguments"] = new Variant("-l 12345");
			args["StartOnCall"] = foo;
			args["FaultOnEarlyExit"] = new Variant("true");

			CrashWrangler w = new CrashWrangler(null, "name", args);
			w.SessionStarting();
			w.IterationStarting(1, false);

			w.Message("Action.Call", foo);

			w.IterationFinished();

			Assert.AreEqual(false, w.DetectedFault());

			w.SessionFinished();
			w.StopMonitor();
		}

		[Test]
		public void TestExitEarlyFault4()
		{
			// FaultOnEarlyExit doesn't fault when restart every iteration is true

			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["Command"] = new Variant("nc");
			args["Arguments"] = new Variant("-l 12345");
			args["RestartOnEachTest"] = new Variant("true");
			args["FaultOnEarlyExit"] = new Variant("true");

			CrashWrangler w = new CrashWrangler(null, "name", args);
			w.SessionStarting();
			w.IterationStarting(1, false);

			w.IterationFinished();

			Assert.AreEqual(false, w.DetectedFault());

			w.SessionFinished();
			w.StopMonitor();
		}

		[Test]
		public void TestGetData()
		{
			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["Command"] = new Variant("CrashingProgram");

			System.Environment.SetEnvironmentVariable("PEACH", "qwertyuiopasdfghjklzxcvbnmqwertyuio");

			CrashWrangler w = new CrashWrangler(null, "name", args);
			w.SessionStarting();
			w.IterationStarting(0, false);
			Thread.Sleep(1000);
			w.IterationFinished();
			Assert.AreEqual(true, w.DetectedFault());
			Fault fault = w.GetMonitorData();
			Assert.NotNull(fault);
			Assert.AreEqual(1, fault.collectedData.Count);
			Assert.True(fault.collectedData.ContainsKey("Log"));
			Assert.Greater(fault.collectedData["Log"].Length, 0);
			Assert.True(fault.description.StartsWith("Exploitable_Crash_0x"));
			w.SessionFinished();
			w.StopMonitor();
		}
	}
}
