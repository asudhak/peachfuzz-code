using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using NLog;
using NLog.Targets;
using NLog.Config;

using NUnit.Framework;
using NUnit.Framework.Constraints;

using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Analyzers;
using Peach.Core.Cracker;
using Peach.Core.IO;

namespace Peach
{
	public class AssertTestFail : System.Diagnostics.TraceListener
	{
		public override void Write(string message)
		{
			Assert.Fail(message);
		}

		public override void WriteLine(string message)
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine("Assertion " + message);
			sb.AppendLine(new System.Diagnostics.StackTrace(2, true).ToString());

			Assert.Fail(sb.ToString());
		}
	}

	[SetUpFixture]
	public class TestBase
	{
		public static ushort MakePort(ushort min, ushort max)
		{
			int pid = System.Diagnostics.Process.GetCurrentProcess().Id;
			int seed = Environment.TickCount * pid;
			var rng = new Peach.Core.Random((uint)seed);
			var ret = (ushort)rng.Next(min, max);
			return ret;
		}

		[SetUp]
		public void Initialize()
		{
			System.Diagnostics.Debug.Listeners.Insert(0, new AssertTestFail());

			ColoredConsoleTarget consoleTarget = new ColoredConsoleTarget();
			consoleTarget.Layout = "${date:format=HH\\:MM\\:ss} ${logger} ${message}";

			LoggingConfiguration config = new LoggingConfiguration();
			config.AddTarget("console", consoleTarget);

			LoggingRule rule = new LoggingRule("*", LogLevel.Info, consoleTarget);
			config.LoggingRules.Add(rule);

			LogManager.Configuration = config;

			Peach.Core.Platform.LoadAssembly();
		}
	}


	[TestFixture]
	class AssertTest
	{
		[Test]
		public void TestAssert()
		{
#if DEBUG
			Assert.Throws<AssertionException>(delegate() {
				System.Diagnostics.Debug.Assert(false);
			});
#else
			System.Diagnostics.Debug.Assert(false);
#endif

		}
	}
}
