using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NLog;

namespace Peach.Core.Test.OS.Linux
{
	[TestFixture]
	public class PlatformTests
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		[Test]
		public void Test1()
		{
			logger.Debug("Hello World");
			bool value = true;
			Assert.IsTrue(value);
		}
	}
}
