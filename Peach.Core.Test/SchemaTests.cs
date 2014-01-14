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
	}
}
