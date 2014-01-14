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
		public void Test1()
		{
			var stream = new MemoryStream();

			SchemaBuilder.Generate(typeof(TestElement), stream);

			var buf = stream.ToArray();
			var xsd = Encoding.UTF8.GetString(buf);

			Console.WriteLine(xsd);
		}
	}
}
