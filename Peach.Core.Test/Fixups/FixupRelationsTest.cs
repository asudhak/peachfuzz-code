using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core.IO;
using Peach.Core.Dom;
using Peach.Core.Analyzers;
using Peach.Core.Fixups.Libraries;

namespace Peach.Core.Test.Fixups
{
	[TestFixture]
	class FixupRelationsTest : DataModelCollector
	{
		[Test]
		public void TestFixupAfter()
		{
			// Verify that in a DOM with Fixups after Relations, the fixup runs
			// after the relation has.

			// In this case the data model is:
			// Len, 4 byte number whose value is the size of the data model
			// CRC, 4 byte number whose value is the CRC of the data model
			// Data, 5 byte string.

			// The CRC should include the computed size relation.

			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
				"<Peach>" +
				"   <DataModel name=\"TheDataModel\">" +
				"       <Number name=\"len\" size=\"32\" signed=\"false\">" +
				"           <Relation type=\"size\" of=\"TheDataModel\" />" +
				"       </Number>" +
				"       <Block>" +
				"           <Number name=\"CRC\" size=\"32\" signed=\"false\">" +
				"               <Fixup class=\"Crc32Fixup\">" +
				"                   <Param name=\"ref\" value=\"TheDataModel\"/>" +
				"               </Fixup>" +
				"           </Number>" +
				"           <Blob name=\"Data\" value=\"Hello\">" +
				"               <Hint name=\"BlobMutator-How\" value=\"ExpandAllRandom\"/>" +
				"           </Blob>" +
				"       </Block>" +
				"   </DataModel>" +

				"   <StateModel name=\"TheState\" initialState=\"Initial\">" +
				"       <State name=\"Initial\">" +
				"           <Action type=\"output\">" +
				"               <DataModel ref=\"TheDataModel\"/>" +
				"           </Action>" +
				"       </State>" +
				"   </StateModel>" +

				"   <Test name=\"Default\">" +
				"       <StateModel ref=\"TheState\"/>" +
				"       <Publisher class=\"Null\"/>" +
				"       <Strategy class=\"RandomDeterministic\"/>" +
				"   </Test>" +
				"</Peach>";

			PitParser parser = new PitParser();

			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			dom.tests[0].includedMutators = new List<string>();
			dom.tests[0].includedMutators.Add("BlobMutator");

			RunConfiguration config = new RunConfiguration();

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			Assert.AreEqual(2, dataModels.Count);

			byte[] dm1 = dataModels[0].Value.Value;
			byte[] dm2 = dataModels[1].Value.Value;

			Assert.AreEqual(4 + 4 + 5, dm1.Length);
			Assert.Greater(dm2.Length, dm1.Length);

			BitStream bs = new BitStream();
			bs.WriteUInt32(13);
			bs.WriteUInt32(0);
			bs.WriteBytes(Encoding.ASCII.GetBytes("Hello"));

			var crc = new CRCTool();
			crc.Init(CRCTool.CRCCode.CRC32);
			bs.SeekBytes(4, SeekOrigin.Begin);
			bs.WriteUInt32((uint)crc.crctablefast(bs.Value));

			byte[] final = bs.Value;
			Assert.AreEqual(final, dm1);
		}

		[Test]
		public void TestFixupBefore()
		{
			// Verify that in a DOM with Fixups before Relations, the fixup runs
			// after the relation has.

			// In this case the data model is:
			// CRC, 4 byte number whose value is the CRC of the data model
			// Len, 4 byte number whose value is the size of the data model
			// Data, 5 byte string.

			// The CRC should include the computed size relation.

			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
				"<Peach>" +
				"   <DataModel name=\"TheDataModel\">" +
				"       <Block>" +
				"           <Number name=\"CRC\" size=\"32\" signed=\"false\">" +
				"               <Fixup class=\"Crc32Fixup\">" +
				"                   <Param name=\"ref\" value=\"TheDataModel\"/>" +
				"               </Fixup>" +
				"           </Number>" +
				"       </Block>" +
				"       <Number name=\"len\" size=\"32\" signed=\"false\">" +
				"           <Relation type=\"size\" of=\"TheDataModel\" />" +
				"       </Number>" +
				"       <Blob name=\"Data\" value=\"Hello\">" +
				"           <Hint name=\"BlobMutator-How\" value=\"ExpandAllRandom\"/>" +
				"       </Blob>" +
				"   </DataModel>" +

				"   <StateModel name=\"TheState\" initialState=\"Initial\">" +
				"       <State name=\"Initial\">" +
				"           <Action type=\"output\">" +
				"               <DataModel ref=\"TheDataModel\"/>" +
				"           </Action>" +
				"       </State>" +
				"   </StateModel>" +

				"   <Test name=\"Default\">" +
				"       <StateModel ref=\"TheState\"/>" +
				"       <Publisher class=\"Null\"/>" +
				"       <Strategy class=\"RandomDeterministic\"/>" +
				"   </Test>" +
				"</Peach>";

			PitParser parser = new PitParser();

			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			dom.tests[0].includedMutators = new List<string>();
			dom.tests[0].includedMutators.Add("BlobMutator");

			RunConfiguration config = new RunConfiguration();

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			Assert.AreEqual(2, dataModels.Count);

			byte[] dm1 = dataModels[0].Value.Value;
			byte[] dm2 = dataModels[1].Value.Value;

			Assert.AreEqual(4 + 4 + 5, dm1.Length);
			Assert.Greater(dm2.Length, dm1.Length);

			BitStream bs = new BitStream();
			bs.WriteUInt32(0);
			bs.WriteUInt32(13);
			bs.WriteBytes(Encoding.ASCII.GetBytes("Hello"));

			var crc = new CRCTool();
			crc.Init(CRCTool.CRCCode.CRC32);
			bs.SeekBytes(0, SeekOrigin.Begin);
			bs.WriteUInt32((uint)crc.crctablefast(bs.Value));

			byte[] final = bs.Value;
			Assert.AreEqual(final, dm1);
		}

		[Test]
		public void TestFixupSiblingBefore()
		{
			// Verify that in a DOM with Fixups before Relations, the fixup runs
			// after the relation has.

			// In this case the data model is:
			// Len, 4 byte number whose value is the size of the crc
			// CRC, 4 byte number whose value is the CRC of the length

			// The CRC should include the computed size relation.

			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
				"<Peach>" +
				"   <DataModel name=\"TheDataModel\">" +
				"       <Block>" +
				"           <Number name=\"CRC\" size=\"32\" endian=\"big\" signed=\"false\">" +
				"               <Fixup class=\"Crc32Fixup\">" +
				"                   <Param name=\"ref\" value=\"LEN\"/>" +
				"               </Fixup>" +
				"           </Number>" +
				"       </Block>" +
				"       <Number name=\"LEN\" size=\"32\" endian=\"big\" signed=\"false\">" +
				"           <Relation type=\"size\" of=\"CRC\" />" +
				"       </Number>" +
				"   </DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var val = dom.dataModels[0].Value;
			Assert.NotNull(val);

			MemoryStream ms = val.Stream as MemoryStream;
			Assert.NotNull(ms);

			byte[] actual = new byte[ms.Length];
			Buffer.BlockCopy(ms.GetBuffer(), 0, actual, 0, (int)ms.Length);

			byte[] expected = new byte[] { 38, 41, 27, 5, 0, 0, 0, 4 };
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void TestFixupSiblingAfter()
		{
			// Verify that in a DOM with Fixups before Relations, the fixup runs
			// after the relation has.

			// In this case the data model is:
			// Len, 4 byte number whose value is the size of the crc
			// CRC, 4 byte number whose value is the CRC of the length

			// The CRC should include the computed size relation.

			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
				"<Peach>" +
				"   <DataModel name=\"TheDataModel\">" +
				"       <Number name=\"LEN\" size=\"32\" endian=\"big\" signed=\"false\">" +
				"           <Relation type=\"size\" of=\"CRC\" />" +
				"       </Number>" +
				"       <Block>" +
				"           <Number name=\"CRC\" size=\"32\" endian=\"big\" signed=\"false\">" +
				"               <Fixup class=\"Crc32Fixup\">" +
				"                   <Param name=\"ref\" value=\"LEN\"/>" +
				"               </Fixup>" +
				"           </Number>" +
				"       </Block>" +
				"   </DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var val = dom.dataModels[0].Value;
			Assert.NotNull(val);

			MemoryStream ms = val.Stream as MemoryStream;
			Assert.NotNull(ms);

			byte[] actual = new byte[ms.Length];
			Buffer.BlockCopy(ms.GetBuffer(), 0, actual, 0, (int)ms.Length);

			byte[] expected = new byte[] { 0, 0, 0, 4, 38, 41, 27, 5 };
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void TestFixupChildRelation()
		{
			// Verify that in a DOM with Fixups that are siblings of a Relation,
			// where the fixup ref's the parent of the parent of the relation,
			// the fixup runs after the relation has.

			// In this case the data model is:
			// Len, 4 byte number whose value is the size of the crc
			// CRC, 4 byte number whose value is the CRC of the length

			// The CRC should include the computed size relation.

			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
				"<Peach>" +
				"   <DataModel name=\"TheDataModel\">" +
				"       <Block name=\"Data\">" +
				"           <Number name=\"LEN\" size=\"32\" endian=\"big\" signed=\"false\">" +
				"               <Relation type=\"size\" of=\"Data\" />" +
				"           </Number>" +
				"           <Block>" +
				"               <Number name=\"CRC\" size=\"32\" endian=\"big\" signed=\"false\">" +
				"                   <Fixup class=\"Crc32Fixup\">" +
				"                       <Param name=\"ref\" value=\"TheDataModel\"/>" +
				"                   </Fixup>" +
				"               </Number>" +
				"           </Block>" +
				"       </Block>" +
				"   </DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var val = dom.dataModels[0].Value;
			Assert.NotNull(val);

			MemoryStream ms = val.Stream as MemoryStream;
			Assert.NotNull(ms);

			byte[] actual = new byte[ms.Length];
			Buffer.BlockCopy(ms.GetBuffer(), 0, actual, 0, (int)ms.Length);

			byte[] expected = new byte[] { 0, 0, 0, 8, 85, 82, 148, 168 };
			Assert.AreEqual(expected, actual);
		}
	}
}

// end
