using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core;
using System.Net;
using System.IO;
using NLog;
namespace Peach.Core.Test.Publishers
{
	[TestFixture]
	class ParameterTests
	{
		private static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		[Publisher("testA")]
		[Parameter("req1", typeof(int), "desc", true)]
		class PubMissingDefaultName : Publisher
		{
			protected override NLog.Logger Logger { get { return logger; } }
			public PubMissingDefaultName(Dictionary<string, Variant> args)
				: base(args)
			{
			}
		}

		[Publisher("testA1")]
		[Publisher("testA1.default", true)]
		[Parameter("req1", typeof(int), "desc", true)]
		[Parameter("ip", typeof(IPAddress), "desc", false)]
		class PubDefaultName : Publisher
		{
			protected override NLog.Logger Logger { get { return logger; } }
			public PubDefaultName(Dictionary<string, Variant> args)
				: base(args)
			{
			}
		}

		[Publisher("enumPub")]
		[Parameter("enum1", typeof(FileMode), "File Mode", true)]
		[Parameter("enum2", typeof(ConsoleColor), "Console Color", "Red")]
		class EnumPub : Publisher
		{
			protected override NLog.Logger Logger { get { return logger; } }
			public FileMode enum1 { get; set; }
			public ConsoleColor enum2 { get; set; }

			public EnumPub(Dictionary<string, Variant> args)
				: base(args)
			{
			}
		}

		[Test]
		public void TestEnums()
		{
			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["enum1"] = new Variant("OpenOrCreate");
			var p1 = new EnumPub(args);
			Assert.AreEqual(p1.enum1, FileMode.OpenOrCreate);
			Assert.AreEqual(p1.enum2, ConsoleColor.Red);

			args["enum2"] = new Variant("DarkCyan");
			var p2 = new EnumPub(args);
			Assert.AreEqual(p2.enum1, FileMode.OpenOrCreate);
			Assert.AreEqual(p2.enum2, ConsoleColor.DarkCyan);
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "testA publisher is missing required parameter 'req1'.")]
		public void TestNameNoDefault()
		{
			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			new PubMissingDefaultName(args);
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "testA1.default publisher is missing required parameter 'req1'.")]
		public void TestNameDefault()
		{
			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			new PubDefaultName(args);
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "testA1.default publisher could not set parameter 'req1'.  Input string was not in a correct format.")]
		public void TestBadParameter()
		{
			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["req1"] = new Variant("not a number");
			new PubDefaultName(args);
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "testA1.default publisher has no public property for parameter 'req1'.")]
		public void TestMissingProperty()
		{
			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["req1"] = new Variant("100");
			new PubDefaultName(args);
		}

		[Publisher("good")]
		[Parameter("Param_string", typeof(string), "desc", true)]
		[Parameter("Param_ip", typeof(IPAddress), "desc", false)]
		class GoodPub : Publisher
		{
			protected override NLog.Logger Logger { get { return logger; } }
			public GoodPub(Dictionary<string, Variant> args)
				: base(args)
			{
			}

			public string Param_string { get; set; }
			public IPAddress Param_ip { get; set; }
		}

		[Test]
		public void TestParse()
		{
			Dictionary<string, Variant> args = new Dictionary<string,Variant>();
			args["Param_string"] = new Variant("the string");
			args["Param_ip"] = new Variant("192.168.1.1");

			var p = new GoodPub(args);
			Assert.AreEqual("the string", p.Param_string);
			Assert.AreEqual(IPAddress.Parse("192.168.1.1"), p.Param_ip);
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "good publisher could not set parameter 'Param_ip'.  An invalid IP address was specified.")]
		public void TestBadIpParameter()
		{
			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["Param_string"] = new Variant("100");
			args["Param_ip"] = new Variant("999.888.777.666");
			new GoodPub(args);
		}
	}
}
