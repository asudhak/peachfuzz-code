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
using Peach.Core;
using System.Runtime.InteropServices;

namespace Peach
{
	[SetUpFixture]
	public class TestBase
	{
		SingleInstance si;

		[SetUp]
		public void Initialize()
		{
			ColoredConsoleTarget consoleTarget = new ColoredConsoleTarget();
			consoleTarget.Layout = "${date:format=HH\\:MM\\:ss} ${logger} ${message}";

			LoggingConfiguration config = new LoggingConfiguration();
			config.AddTarget("console", consoleTarget);

			LoggingRule rule = new LoggingRule("*", LogLevel.Debug, consoleTarget);
			config.LoggingRules.Add(rule);

			LogManager.Configuration = config;

			// Ensure only 1 instance of the platform tests runs at a time
			si = SingleInstance.CreateInstance("Peach.Core.Test.OS.OSX.dll");
			si.Lock();
		}

		[TearDown]
		public void TearDown()
		{
			si.Dispose();
			si = null;
		}
	}
}
