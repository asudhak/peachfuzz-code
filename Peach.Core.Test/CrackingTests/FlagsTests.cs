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

			BitwiseStream val = dom.dataModels[0].Value;
			Assert.NotNull(val);
			Assert.AreEqual(13, val.Length);
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

			var data = Bits.Fmt("{0}", new byte[] { 0xe6 });

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(1, dom.dataModels[0].Count);
			var flags = dom.dataModels[0][0] as Flags;
			Assert.AreEqual(1, flags.Count);
			var flag = flags[0] as Flag;
			Assert.AreEqual(7, (int)flag.DefaultValue);

			var bad = Bits.Fmt("{0}", new byte[] { 0x16 });

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

			var data = Bits.Fmt("{0}", new byte[] { 0x00, 0xff });

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

			BitwiseStream val = dom.dataModels[0].Value;
			Assert.NotNull(val);
			Assert.AreEqual(3, val.Length);
			Assert.AreEqual(3 * 8, val.LengthBits);

			byte[] buf = val.ToArray();

			Assert.AreEqual(3, buf.Length);
			Assert.AreEqual(0x02, buf[0]);
			Assert.AreEqual(0x07, buf[1]);
			Assert.AreEqual(0xff, buf[2]);
		}

		public void CrackLittleFlag(int size, BitStream data)
		{
			string xml = @"
<Peach>
	<DataModel name=""Model"">
		<Flags name=""fields"" size=""{0}"" endian=""little"">
			<Flag name=""a"" size=""1""  position=""0"" value=""1""/>
			<Flag name=""b"" size=""2""  position=""1"" value=""2""/>
			<Flag name=""c"" size=""3""  position=""3"" value=""3""/>
			<Flag name=""d"" size=""4""  position=""6"" value=""4""/>
			<Flag name=""e"" size=""5""  position=""10"" value=""5""/>
		</Flags>
	</DataModel>
</Peach>
".Fmt(size);
			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			var flags = dom.dataModels[0][0] as Flags;
			Assert.NotNull(flags);

			Assert.AreEqual(1, (int)flags["a"].DefaultValue);
			Assert.AreEqual(2, (int)flags["b"].DefaultValue);
			Assert.AreEqual(3, (int)flags["c"].DefaultValue);
			Assert.AreEqual(4, (int)flags["d"].DefaultValue);
			Assert.AreEqual(5, (int)flags["e"].DefaultValue);

			var final = dom.dataModels[0].Value;

			Assert.AreEqual(data.ToArray(), final.ToArray());

		}

		[Test]
		public void CrackLittleFlags16()
		{
			// Little Endian 16-bit:
			// struct { a:1,b:2,c:3,d:4,e:5 } fields;
			// Buffer: [0x1D 0x15]
			// Memory: __ E5 E4 E3 E2 E1 D4 D3 D2 D1 C3 C2 C1 B2 B1 A1
			//         |--  1  --| |--  5  --| |--  1  --| |--  D  --|
			// Bits:    0  0  0  1  0  1  0  1  0  0  0  1  1  1  0  1
			// Final:  A=1, B=2, C=3, D=4, E=5

			var data = Bits.Fmt("{0}", new byte[] { 0x1D, 0x15 });

			CrackLittleFlag(16, data);
		}

		[Test]
		public void CrackLittleFlags15()
		{
			// Little Endian 15-bit:
			// struct { a:1,b:2,c:3,d:4,e:5 } fields;
			// Buffer: [0x1D 0x2A]
			// Memory: E5 E4 E3 E2 E1 D4 D3    D2 D1 C3 C2 C1 B2 B1 A1
			//         |--  2  --| |--  5  --| |--  1  --| |--  D  --|
			// Bits:    0  0  1  0  1  0  1  0  0  0  1  1  1  0  1
			// Final:  A=1, B=2, C=3, D=4, E=5

			var data = Bits.Fmt("{0}", new byte[] { 0x1D, 0x2A });
			data.SetLengthBits(15);

			CrackLittleFlag(15, data);
		}

	}
}
