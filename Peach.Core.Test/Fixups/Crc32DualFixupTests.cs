using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Analyzers;

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
            e.config = config;
            e.startFuzzing(dom, config);

            // verify values
            // -- this is the pre-calculated checksum from Peach2.3 on the blobs: { 1, 2, 3, 4, 5 } and { 6, 7, 8, 9 }
            byte[] precalcChecksum = new byte[] { 0x26, 0x39, 0xF4, 0xCB };
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual(precalcChecksum, values[0].Value);
        }
    }
}

// end
