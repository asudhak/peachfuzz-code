using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Analyzers;
using Peach.Core.Cracker;
using Peach.Core.IO;

namespace Peach.Core.Test.CrackingTests
{
	[TestFixture]
	class FlagsTests
	{
		[Test]
		public void CrackFlag1()
		{
			string xml = @"
<Peach>
	<DataModel name=""TheDataModel"">
		<Flags size=""16"">
			<Flag position=""0"" size=""1""/>
		</Flags>
		<String value=""Hello World""/>
	</DataModel>

	<StateModel name=""TheState"" initialState=""Initial"">
		<State name=""Initial"">
			<Action type=""output"">
				<DataModel ref=""TheDataModel""/>
			</Action>
		</State>
	</StateModel>

	<Test name=""Default"">
		<StateModel ref=""TheState""/>
		<Publisher class=""StdoutHex""/>
		<Strategy class=""Sequential""/>
	</Test>
</Peach>
";
			var pub = new Peach.Core.Test.Publishers.TestPublisher();

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			dom.tests[0].publishers[0] = pub;

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			BitStream val = dom.dataModels[0].Value;
			Assert.NotNull(val);
			Assert.AreEqual(13, val.LengthBytes);
			Assert.AreEqual(13 * 8, val.LengthBits);

			pub.Stream.Seek(0, SeekOrigin.Begin);
			string results = Encoding.ASCII.GetString(pub.Stream.ToArray());
			Assert.NotNull(results);
			string expected = "00000000   00 00 48 65 6C 6C 6F 20  57 6F 72 6C 64            ??Hello World   " + Environment.NewLine;
			Assert.AreEqual(expected, results);
		}

		[Test]
		public void CrackOnlyFlag()
		{
			string xml = @"
<Peach>
	<DataModel name=""TheDataModel"">
		<Flags size=""8"" endian=""big"">
			<Flag position=""0"" size=""3"" token=""true"" value=""7""/>
		</Flags>
	</DataModel>
</Peach>
";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(new byte[] { 0xe6 });
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(1, dom.dataModels[0].Count);
			var flags = dom.dataModels[0][0] as Flags;
			Assert.AreEqual(1, flags.Count);
			var flag = flags[0] as Flag;
			Assert.AreEqual(7, (int)flag.DefaultValue);

			BitStream bad = new BitStream();
			bad.LittleEndian();
			bad.WriteBytes(new byte[] { 0x16 });
			bad.SeekBits(0, SeekOrigin.Begin);

			Assert.Throws<CrackingFailure>(delegate()
			{
				cracker.CrackData(dom.dataModels[0], bad);
			});
		}

		[Test]
		public void CrackFlagsSecond()
		{
			string xml = @"
<Peach>
	<DataModel name=""TheDataModel"">
		<Number size=""8""/>
		<Flags size=""8"" endian=""big"">
			<Flag position=""0"" size=""4""/>
			<Flag position=""4"" size=""4""/>
		</Flags>
	</DataModel>
</Peach>
";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(new byte[] { 0x00, 0xff });
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(2, dom.dataModels[0].Count);
			var flags = dom.dataModels[0][1] as Flags;
			Assert.AreEqual(2, flags.Count);
			var flag1 = flags[0] as Flag;
			Assert.AreEqual(0xf, (int)flag1.DefaultValue);
			var flag2 = flags[1] as Flag;
			Assert.AreEqual(0xf, (int)flag2.DefaultValue);
		}

		[Test]
		public void OutputFlag()
		{
			string xml = @"
<Peach>
	<DataModel name=""Model"">
		<Block name=""block"">
			<Flags name=""tlv"" size=""16"" endian=""big"">
				<Flag name=""type"" size=""7""  position=""0"" value=""1""/>
				<Flag name=""length"" size=""9"" position=""7"" value=""7""/>
			</Flags>
			<Blob name=""value"" valueType=""hex"" value=""ff""/>
		</Block>
	</DataModel>

	<StateModel name=""TheStateModel"" initialState=""initial"">
		<State name=""initial"">
			<Action type=""output"">
				<DataModel ref=""Model"" />
			</Action>
		</State>
	</StateModel>

	<Test name=""Default"">
		<Strategy class=""Sequential""/>
		<StateModel ref=""TheStateModel""/>
		<Publisher class=""Null""/>
	</Test>
</Peach>
";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			BitStream val = dom.dataModels[0].Value;
			Assert.NotNull(val);
			Assert.AreEqual(3, val.LengthBytes);
			Assert.AreEqual(3 * 8, val.LengthBits);

			MemoryStream ms = val.Stream as MemoryStream;
			byte[] buf = ms.GetBuffer();

			Assert.AreEqual(0x02, buf[0]);
			Assert.AreEqual(0x07, buf[1]);
			Assert.AreEqual(0xff, buf[2]);
		}
	}
}
