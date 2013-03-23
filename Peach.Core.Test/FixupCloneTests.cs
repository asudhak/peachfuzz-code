using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Analyzers;
using Peach.Core.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Peach.Core.Test
{
	[TestFixture]
	class FixupCloneTests
	{
		[Test]
		public void Test1()
		{
			string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Peach>
	<DataModel name=""Common"">
		<Number name=""Length"" endian=""big"" size=""32"">
			<Relation type=""size"" of=""Data"" />
		</Number>
		<Number name=""CRC"" endian=""big"" size=""32"">
			<Fixup class=""Crc32Fixup"">
				<Param name=""ref"" value=""Common""/>
			</Fixup>
		</Number>

	</DataModel>

	<DataModel name=""Base"" ref=""Common"">
		<Block name=""Payload"">
			<Block name=""Data""/>
		</Block>
	</DataModel>

	<DataModel name=""Msg"" ref=""Base"">
		<Block name=""Payload"">
			<Block name=""Data"">
				<String name=""str"" value=""Hello World"" />
			</Block>
		</Block>
	</DataModel>

	<DataModel name=""Final"">
		<Block name=""blk"" ref=""Msg"" />
	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			Assert.AreEqual(4, dom.dataModels.Count);
			var dm = dom.dataModels[3];
			var val = dm.Value.Value;

			byte[] expected = Encoding.ISOLatin1.GetBytes("\x00\x00\x00\x0b\xa1\x43\xe2\x68Hello World");
			Assert.AreEqual(expected, val);
		}
	}
}
