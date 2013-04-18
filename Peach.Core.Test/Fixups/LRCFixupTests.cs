using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core.IO;
using Peach.Core.Dom;
using Peach.Core.Analyzers;

namespace Peach.Core.Test.Fixups
{
    [TestFixture]
    class LRCFixupTests : DataModelCollector
    {
        [Test]
        public void Test1()
        {
            // standard test

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Number name=\"CRC\" size=\"32\" signed=\"false\">" +
                "           <Fixup class=\"LRCFixup\">" +
                "               <Param name=\"ref\" value=\"Data\"/>" +
                "           </Fixup>" +
                "       </Number>" +
                "       <Blob name=\"Data\" value=\"12345\"/>" +
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
            // -- this is the pre-calculated result from Peach2.3 on the blob: "12345"
            byte[] precalcResult = new byte[] { 0x01, 0x00, 0x00, 0x00 };
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual(precalcResult, values[0].Value);
        }

		[Test]
		public void TestTypes()
		{
			string xml = @"
<Peach>
	<DataModel name=""TheDataModel"">
		<Blob name=""blob"" valueType=""hex"" value=""00 01 10""/>

		<Blob length=""1"">
			<Fixup class=""LRCFixup"">
				<Param name=""ref"" value=""blob""/>
			</Fixup>
		</Blob>

		<String>
			<Fixup class=""LRCFixup"">
				<Param name=""ref"" value=""blob""/>
			</Fixup>
		</String>

		<Number size=""32"" endian=""little"">
			<Fixup class=""LRCFixup"">
				<Param name=""ref"" value=""blob""/>
			</Fixup>
		</Number>

		<Number size=""32"" endian=""big"">
			<Fixup class=""LRCFixup"">
				<Param name=""ref"" value=""blob""/>
			</Fixup>
		</Number>

		<Flags size=""16"" endian=""big"">
			<Flag position=""4"" size=""8"">
				<Fixup class=""LRCFixup"">
					<Param name=""ref"" value=""blob""/>
				</Fixup>
			</Flag>
		</Flags>
	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var val = dom.dataModels[0].Value;
			Assert.NotNull(val);

			MemoryStream ms = val.Stream as MemoryStream;
			Assert.NotNull(ms);

			byte[] expected = { 0x00, 0x01, 0x10, 0xef, 0x32, 0x33, 0x39, 0xef, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xef, 0x0e, 0xf0 };
			Assert.AreEqual(expected.Length, ms.Length);

			byte[] actual = new byte[ms.Length];
			Buffer.BlockCopy(ms.GetBuffer(), 0, actual, 0, (int)ms.Length);

			Assert.AreEqual(expected, actual);

		}

    }
}

// end
