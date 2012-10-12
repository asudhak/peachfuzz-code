using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core;

namespace Peach.Core.Test.Publishers
{
	[TestFixture]
	class ParameterTests
	{
		[Publisher("testA")]
		[Parameter("req1", typeof(int), "desc", true)]
		class PubMissingDefaultName : Publisher
		{
			public PubMissingDefaultName(Dictionary<string, Variant> args)
				: base(args)
			{
			}
		}

		[Publisher("testA1")]
		[Publisher("testA1.default", true)]
		[Parameter("req1", typeof(int), "desc", true)]
		class PubDefaultName : Publisher
		{
			public PubDefaultName(Dictionary<string, Variant> args)
				: base(args)
			{
			}
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
		class GoodPub : Publisher
		{
			public GoodPub(Dictionary<string,Variant> args)
				: base(args)
			{
			}

			public string Param_string { get; set; }
		}

		[Test]
		public void TestParse()
		{
			Dictionary<string, Variant> args = new Dictionary<string,Variant>();
			args["Param_string"] = new Variant("the string");

			var p = new GoodPub(args);
			Assert.AreEqual("the string", p.Param_string);
		}
	}
}
