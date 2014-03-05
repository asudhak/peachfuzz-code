using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using NLog;
using NLog.Targets;
using NLog.Config;
using NUnit.Framework;

namespace Peach
{
	[SetUpFixture]
	public class TestBase
	{
		[SetUp]
		public void Initialize()
		{
			// NUnit [Platform] attribute doesn't differentiate MacOSX/Linux
			if (Peach.Core.Platform.GetOS() != Peach.Core.Platform.OS.Windows)
				Assert.Ignore("Only supported on Windows");

			ColoredConsoleTarget consoleTarget = new ColoredConsoleTarget();
			consoleTarget.Layout = "${date:format=HH\\:MM\\:ss} ${logger} ${message}";

			LoggingConfiguration config = new LoggingConfiguration();
			config.AddTarget("console", consoleTarget);

			LoggingRule rule = new LoggingRule("*", LogLevel.Trace, consoleTarget);
			config.LoggingRules.Add(rule);

			LogManager.Configuration = config;
		}
	}
}
