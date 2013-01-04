using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Analyzers;
using Peach.Core.IO;
using Peach.Core.Cracker;

namespace Peach.Core.Test.Fixups
{
    [TestFixture]
    class Crc32DualFixupTests : DataModelCollector
    {
        [Test]
        public void Test1()
        {
            // standard test

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Number name=\"CRC\" size=\"32\" signed=\"false\" endian=\"little\">" +
                "           <Fixup class=\"Crc32DualFixup\">" +
                "               <Param name=\"ref1\" value=\"Data1\"/>" +
                "               <Param name=\"ref2\" value=\"Data2\"/>" +
                "           </Fixup>" +
                "       </Number>" +
                "       <Blob name=\"Data1\" value=\"12345\"/>" +
                "       <Blob name=\"Data2\" value=\"6789\"/>" +
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
                "   </Test>" +
                "</Peach>";

            PitParser parser = new PitParser();

            Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

            RunConfiguration config = new RunConfiguration();
            config.singleIteration = true;

            Engine e = new Engine(null);
            e.startFuzzing(dom, config);

            // verify values
            // -- this is the pre-calculated checksum from Peach2.3 on the blobs: { 1, 2, 3, 4, 5 } and { 6, 7, 8, 9 }
            byte[] precalcChecksum = new byte[] { 0x26, 0x39, 0xF4, 0xCB };
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual(precalcChecksum, values[0].Value);
        }

		[Test]
		public void TestCrack()
		{
			string xml = @"
<Peach>
	<DataModel name='DM'>
		<Number size='8' signed='false' name='Length'>
			<Relation type='size' of='DataBlock'/>
		</Number>
		<Block name='DataBlock'>
			<Blob name='Data'/>
			<Number size='32' name='CRC' endian='big' signed='false'>
				<Fixup class='Crc32DualFixup'>
					<Param name='ref1' value='Length'/>
					<Param name='ref2' value='Data'/>
				</Fixup>
			</Number >
		</Block>
	</DataModel>
</Peach>
";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(new byte[] { 0x05, 0x11, 0x22, 0x33, 0x44, 0x55 });
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			var val = dom.dataModels[0].Value;
			Assert.NotNull(val);

			MemoryStream ms = val.Stream as MemoryStream;
			Assert.NotNull(ms);

			byte[] actual = new byte[ms.Length];
			Buffer.BlockCopy(ms.GetBuffer(), 0, actual, 0, (int)ms.Length);

			byte[] expected = new byte[] { 0x05, 0x11, 0x56, 0x1e, 0xc6, 0x48 };
			Assert.AreEqual(expected, actual);
		}
    }
}

// end
