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
		[Parameter("req1", typeof(int), "desc")]
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
		[Parameter("req1", typeof(int), "desc")]
		class PubDefaultName : Publisher
		{
			protected override NLog.Logger Logger { get { return logger; } }
			public PubDefaultName(Dictionary<string, Variant> args)
				: base(args)
			{
			}
		}

		[Publisher("enumPub")]
		[Parameter("enum1", typeof(FileMode), "File Mode")]
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

			args["enum2"] = new Variant("DaRkMaGeNtA");
			var p3 = new EnumPub(args);
			Assert.AreEqual(p3.enum1, FileMode.OpenOrCreate);
			Assert.AreEqual(p3.enum2, ConsoleColor.DarkMagenta);
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Publisher 'testA' is missing required parameter 'req1'.")]
		public void TestNameNoDefault()
		{
			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			new PubMissingDefaultName(args);
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Publisher 'testA1.default' is missing required parameter 'req1'.")]
		public void TestNameDefault()
		{
			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			new PubDefaultName(args);
		}

		[Test]
		public void TestBadParameter()
		{
			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["req1"] = new Variant("not a number");

			PeachException pe = null;
			try
			{
				new PubDefaultName(args);
			}
			catch (PeachException ex)
			{
				pe = ex;
			}
			Assert.NotNull(pe);
			Assert.True(pe.Message.StartsWith("Publisher 'testA1.default' could not set parameter 'req1'.  Input string was not in"));
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Publisher 'testA1.default' has no property for parameter 'req1'.")]
		public void TestMissingProperty()
		{
			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["req1"] = new Variant("100");
			new PubDefaultName(args);
		}

		[Publisher("good")]
		[Parameter("Param_string", typeof(string), "desc")]
		[Parameter("Param_ip", typeof(IPAddress), "desc", "")]
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

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Publisher 'good' could not set parameter 'Param_ip'.  An invalid IP address was specified.")]
		public void TestBadIpParameter()
		{
			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["Param_string"] = new Variant("100");
			args["Param_ip"] = new Variant("999.888.777.666");
			new GoodPub(args);
		}

		class CustomType
		{
			public string Message { get; set; }

			public CustomType()
			{
			}
		}

		[Publisher("CustomTypePub")]
		[Parameter("param", typeof(CustomType), "Custom Type")]
		class CustomTypePub : Publisher
		{
			protected override NLog.Logger Logger { get { return logger; } }
			public CustomType param { get; set; }

			public CustomTypePub(Dictionary<string, Variant> args)
				: base(args)
			{
			}

			static void Parse(string str, out IPAddress val)
			{
				val = IPAddress.Parse(str);
			}

			static void Parse(string str, out CustomType val)
			{
				val = new CustomType();
				val.Message = str;
			}
		}

		[Test]
		public void TestCustomConvert()
		{
			// When the bas Publisher can not convert a string parameter
			// into the type defined in the Parameter attribute, it should
			// look for a conversion function on the derived publisher

			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["param"] = new Variant("foo");
			var pub = new CustomTypePub(args);

			Assert.NotNull(pub);
			Assert.AreEqual(pub.param.Message, "foo");
		}

		[Publisher("PrivatePub")]
		[Parameter("param", typeof(string), "param")]
		[Parameter("param1", typeof(string), "param1")]
		class PrivatePub : Publisher
		{
			protected override NLog.Logger Logger { get { return logger; } }

			private string param { get; set; }
			public string param1 { get; private set; }

			public string GetParam { get { return param; } }

			public PrivatePub(Dictionary<string, Variant> args)
				: base(args)
			{
			}
		}

		[Test]
		public void TestPrivate()
		{
			// Ensure the auto setting supports private properties
			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["param"] = new Variant("foo");
			args["param1"] = new Variant("bar");
			var pub = new PrivatePub(args);

			Assert.NotNull(pub);
			Assert.AreEqual(pub.GetParam, "foo");
			Assert.AreEqual(pub.param1, "bar");
		}

		[Publisher("SetPub")]
		[Parameter("param", typeof(string), "param")]
		class SetPub : Publisher
		{
			protected override NLog.Logger Logger { get { return logger; } }

			private string _val;

			private string param { set { _val = value; } }

			public string GetParam { get { return _val; } }

			public SetPub(Dictionary<string, Variant> args)
				: base(args)
			{
			}
		}

		[Test]
		public void TestSetOnly()
		{
			// Ensure the auto setting supports handles only set properties
			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["param"] = new Variant("foo");
			var pub = new SetPub(args);

			Assert.NotNull(pub);
			Assert.AreEqual(pub.GetParam, "foo");
		}

		[Publisher("GetPub")]
		[Parameter("param", typeof(string), "param")]
		class GetPub : Publisher
		{
			protected override NLog.Logger Logger { get { return logger; } }

			public string param { get { return "hello"; } }

			public GetPub(Dictionary<string, Variant> args)
				: base(args)
			{
			}
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Publisher 'GetPub' has no settable property for parameter 'param'.")]
		public void TestGetOnly()
		{
			// Ensure the auto setting supports handles only get properties
			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["param"] = new Variant("foo");
			new GetPub(args);
		}

		[Publisher("NullPlugin", true)]
		[Parameter("custom", typeof(CustomType), "desc", "")]
		[Parameter("str", typeof(string), "desc", "")]
		[Parameter("num", typeof(int), "desc", "")]
		class NullTest
		{
			public NullTest() { }
			public string str { get; set; }
			public int num { get; set; }
			public CustomType custom { set; get; }
		}

		[Test]
		public void TestNullDefault()
		{
			var obj = new NullTest();

			var onlyNum = new Dictionary<string, Variant>();
			onlyNum["num"] = new Variant(10);
			ParameterParser.Parse(obj, onlyNum);

			Assert.Null(obj.str);
			Assert.AreEqual(10, obj.num);
			Assert.Null(obj.custom);

			var onlyStr = new Dictionary<string, Variant>();
			onlyNum["str"] = new Variant("hi");

			Assert.Throws<PeachException>(delegate() { ParameterParser.Parse(obj, onlyStr); });
		}

		[Publisher("NullablePlugin", true)]
		[Parameter("num1", typeof(int?), "desc")]
		[Parameter("num2", typeof(int?), "desc", "")]
		[Parameter("num3", typeof(int?), "desc", "")]
		class NullableTest
		{
			public NullableTest() { }
			public int? num1 { get; set; }
			public int? num2 { get; set; }
			public int? num3 { get; set; }
		}

		[Test]
		public void TestNullable()
		{
			var obj = new NullableTest();

			var onlyNum = new Dictionary<string, Variant>();
			onlyNum["num1"] = new Variant(10);
			onlyNum["num2"] = new Variant(20);

			ParameterParser.Parse(obj, onlyNum);

			Assert.True(obj.num1.HasValue);
			Assert.True(obj.num2.HasValue);
			Assert.False(obj.num3.HasValue);

			Assert.AreEqual(10, obj.num1.Value);
			Assert.AreEqual(20, obj.num2.Value);
		}

		[Publisher("ArrayPlugin", true)]
		[Parameter("num1", typeof(int[]), "desc", "")]
		[Parameter("num2", typeof(int[]), "desc", "")]
		[Parameter("str", typeof(string[]), "desc", "")]
		class ArrayTest
		{
			public ArrayTest() { }
			public int[] num1 { get; set; }
			public int[] num2 { get; set; }
			public string[] str { get; set; }
		}

		[Test]
		public void TestArray()
		{
			// Test that we can parse array parameters, and we strip empty entries
			var obj = new ArrayTest();

			var onlyNum = new Dictionary<string, Variant>();
			onlyNum["num1"] = new Variant("10,11,12");
			onlyNum["str"] = new Variant("string 1,string2,,,,,,,,,,,string three");

			ParameterParser.Parse(obj, onlyNum);

			Assert.NotNull(obj.num1);
			Assert.NotNull(obj.num2);
			Assert.NotNull(obj.str);

			Assert.AreEqual(3, obj.num1.Length);
			Assert.AreEqual(0, obj.num2.Length);
			Assert.AreEqual(3, obj.str.Length);

			Assert.AreEqual(10, obj.num1[0]);
			Assert.AreEqual(11, obj.num1[1]);
			Assert.AreEqual(12, obj.num1[2]);

			Assert.AreEqual("string 1", obj.str[0]);
			Assert.AreEqual("string2", obj.str[1]);
			Assert.AreEqual("string three", obj.str[2]);
		}

		[Publisher("RefPlugin", true)]
		[Parameter("ref", typeof(string), "desc")]
		class RefPlugin
		{
			public RefPlugin() { }
			public string _ref { get; set; }
		}


		[Test]
		public void TestUnderscore()
		{
			// If property 'xxx' doesn't exist, look for property '_xxx'
			var obj = new RefPlugin();
			var args = new Dictionary<string, Variant>();
			args["ref"] = new Variant("foo");

			ParameterParser.Parse(obj, args);

			Assert.AreEqual("foo", obj._ref);
		}

		class MyCustomType
		{
			public MyCustomType(string val)
			{
				this.val = val;
			}

			public string val;
		}

		abstract class MyBaseClass
		{
			static void Parse(string str, out MyCustomType val)
			{
				val = new MyCustomType(str);
			}
		}

		[Publisher("InheritPlugin", true)]
		[Parameter("arg", typeof(MyCustomType), "desc")]
		class InheritPlugin : MyBaseClass
		{
			public MyCustomType arg { get; set; }
		}

		[Test]
		public void TestConvertInherit()
		{
			// Look for base classes for static convert methods for custom types
			var obj = new InheritPlugin();

			var args = new Dictionary<string, Variant>();
			args["arg"] = new Variant("description of my custom type");

			ParameterParser.Parse(obj, args);

			Assert.AreEqual("description of my custom type", obj.arg.val);
		}

		[Publisher("HexString", true)]
		[Parameter("arg", typeof(HexString), "desc")]
		class HexPlugin
		{
			public HexString arg { get; set; }
		}

		[Test]
		public void TestHexStringGood()
		{
			var obj = new HexPlugin();
			var args = new Dictionary<string, Variant>();
			args["arg"] = new Variant("000102030405");

			ParameterParser.Parse(obj, args);

			Assert.NotNull(obj.arg);
			Assert.NotNull(obj.arg.Value);
			Assert.AreEqual(6, obj.arg.Value.Length);

			for (int i = 0; i < obj.arg.Value.Length; ++i)
			{
				Assert.AreEqual(obj.arg.Value[i], i);
			}
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Publisher 'HexString' could not set parameter 'arg'.  An invalid hex string was specified.")]
		public void TestHexStringBad()
		{
			var obj = new HexPlugin();
			var args = new Dictionary<string, Variant>();
			args["arg"] = new Variant("Hello");

			ParameterParser.Parse(obj, args);
		}
	}
}
