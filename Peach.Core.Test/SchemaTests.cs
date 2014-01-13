using System;
using System.IO;

using Peach.Core.Xsd;

namespace Peach.Core.Test
{
	public class SchemaTests
	{
		public void Test1()
		{
			var stream = new MemoryStream();

			SchemaBuilder.Generate(typeof(Peach.Core.Xsd.Dom), stream);

			var buf = stream.ToArray();
			var xsd = Encoding.UTF8.GetString(buf);

			Console.WriteLine(xsd);
		}
	}
}
