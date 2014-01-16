using System;
using System.IO;

using Peach.Core.Xsd;
using System.Xml.Serialization;

namespace Peach.Core.Test
{
	[XmlRoot("Foo")]
	public class TestElement
	{
		[PluginElement("class", typeof(Peach.Core.Agent.Monitor))]
		public NamedCollection<Peach.Core.Dom.Monitor> Monitors { get; set; }
	}

	public class TestObject
	{
		[XmlAttribute]
		public string name { get; set; }
	}

	public abstract class TestAbstract
	{
		public TestAbstract()
		{
			Objects = new System.Collections.Generic.List<TestObject>();
		}

		[XmlElement]
		public System.Collections.Generic.List<TestObject> Objects { get; set; }

		[XmlIgnore]
		public abstract bool include { get; }
	}

	public class TestTrue : TestAbstract
	{
		public override bool include { get { return true; } }

		[XmlAttribute]
		public string foo { get; set; }

		[XmlElement]
		public TestObject FooObj { get; set; }
	}

	public class TestFalse : TestAbstract
	{
		public override bool include { get { return false; } }

		[XmlAttribute]
		public string bar { get; set; }

		[XmlElement]
		public TestObject BarObj { get; set; }
	}

	[XmlRoot("Root")]
	public class TestRootElement
	{
		public TestRootElement()
		{
			Filters = new System.Collections.Generic.List<TestAbstract>();
		}

		[XmlElement(typeof(TestTrue))]
		[XmlElement(typeof(TestFalse))]
		public System.Collections.Generic.List<TestAbstract> Filters { get; set; }

		[XmlElement(typeof(TestObject))]
		public TestObject MyObj { get; set; }
	}

	public class SchemaTests
	{
		private void TestType(Type type)
		{
			var stream = new MemoryStream();

			SchemaBuilder.Generate(type, stream);

			var buf = stream.ToArray();
			var xsd = Encoding.UTF8.GetString(buf);

			Console.WriteLine(xsd);
		}

		public void Test1()
		{
			TestType(typeof(Peach.Core.Xsd.Dom));
		}

		public void Test2()
		{
			TestType(typeof(TestElement));
		}

		public void Test3()
		{
			TestType(typeof(TestRootElement));
		}
	}
}
