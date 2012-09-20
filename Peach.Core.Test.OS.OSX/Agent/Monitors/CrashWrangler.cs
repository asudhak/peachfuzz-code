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
			
			CrashWrangler w = new CrashWrangler("name", args);
			string expected = "CrashWrangler could not start handler \"foo\" - No such file or directory.";
			Assert.Throws<PeachException>(delegate() { w.SessionStarting(); }, expected);
		}

		// XXX: This test causes the testing framework to immediately die.  Need to investigate why
		//[Test]
		//public void BadCommand()
		//{
		//	Dictionary<string, Variant> args = new Dictionary<string, Variant>();
		//	args["Command"] = new Variant("foo");
		//	
		//	CrashWrangler w = new CrashWrangler("name", args);
		//	string expected = "CrashWrangler handler could not run command \"foo\".";
		//	Assert.Throws<PeachException>(delegate() { w.SessionStarting(); }, expected);
		//}
		
		[Test]
		public void TestNoFault()
		{
			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["Command"] = new Variant("echo");
			args["Arguments"] = new Variant("hello");

			CrashWrangler w = new CrashWrangler("name", args);
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
			
			CrashWrangler w = new CrashWrangler("name", args);
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
			Variant ret;

			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["Command"] = new Variant("nc");
			args["Arguments"] = new Variant("-l 12345");
			args["StartOnCall"] = foo;
			args["NoCpuKill"] = new Variant("true");
			
			CrashWrangler w = new CrashWrangler("name", args);
			ret = w.Message("Action.Call.IsRunning", foo);
			Assert.AreEqual(0, (int)ret);

			w.SessionStarting();
			ret = w.Message("Action.Call.IsRunning", foo);
			Assert.AreEqual(0, (int)ret);

			w.IterationStarting(0, false);
			ret = w.Message("Action.Call.IsRunning", foo);
			Assert.AreEqual(0, (int)ret);

			w.Message("Action.Call", foo);
			Thread.Sleep(1000);

			ret = w.Message("Action.Call.IsRunning", foo);
			Assert.AreEqual(1, (int)ret);

			w.IterationFinished();
			ret = w.Message("Action.Call.IsRunning", foo);
			Assert.AreEqual(0, (int)ret);

			Assert.AreEqual(false, w.DetectedFault());

			w.SessionFinished();
			ret = w.Message("Action.Call.IsRunning", foo);
			Assert.AreEqual(0, (int)ret);

			w.StopMonitor();
			ret = w.Message("Action.Call.IsRunning", foo);
			Assert.AreEqual(0, (int)ret);
		}

		[Test]
		public void TestCpuKill()
		{
			Variant foo = new Variant("foo");
			Variant ret;
			
			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["Command"] = new Variant("nc");
			args["Arguments"] = new Variant("-l 12345");
			args["StartOnCall"] = foo;
			
			CrashWrangler w = new CrashWrangler("name", args);
			w.Message("Action.Call", foo);
			Thread.Sleep(1000);

			ret = w.Message("Action.Call.IsRunning", foo);
			w.StopMonitor();
			Assert.AreEqual(0, (int)ret);
		}

		[Test]
		public void TestGetData()
		{
			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["Command"] = new Variant("CrashingProgram");

			System.Environment.SetEnvironmentVariable("PEACH", "qwertyuiopasdfghjklzxcvbnmqwertyuio");

			CrashWrangler w = new CrashWrangler("name", args);
			w.SessionStarting();
			w.IterationStarting(0, false);
			Thread.Sleep(5000);
			w.IterationFinished();
			Assert.AreEqual(true, w.DetectedFault());
			w.SessionFinished();
			w.StopMonitor();
		}
	}
}
