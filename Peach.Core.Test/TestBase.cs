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

namespace Peach.Core.Test
{
	class TestBase
	{
		[SetUp]
		public void Initialize()
		{
			// Step 1. Create configuration object 
			LoggingConfiguration config = new LoggingConfiguration();

			// Step 2. Create targets and add them to the configuration 
			ColoredConsoleTarget consoleTarget = new ColoredConsoleTarget();
			config.AddTarget("console", consoleTarget);

			FileTarget fileTarget = new FileTarget();
			config.AddTarget("file", fileTarget);

			// Step 3. Set target properties 
			consoleTarget.Layout = "${date:format=HH\\:MM\\:ss} ${logger} ${message}";
			fileTarget.FileName = "c:\\peach3.txt";
			fileTarget.Layout = "${message}";

			// Step 4. Define rules
			LoggingRule rule1 = new LoggingRule("*", LogLevel.Debug, consoleTarget);
			config.LoggingRules.Add(rule1);

			LoggingRule rule2 = new LoggingRule("*", LogLevel.Debug, fileTarget);
			config.LoggingRules.Add(rule2);

			// Step 5. Activate the configuration
			LogManager.Configuration = config;
		}
	}
}
