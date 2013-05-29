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
    class IcmpChecksumFixupTests : DataModelCollector
    {
        [Test]
        public void Test1()
        {
            // standard test (Odd length string)

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Number name=\"ICMPChecksum\" signed=\"false\" endian=\"big\" size=\"16\">" +
                "           <Fixup class=\"IcmpChecksumFixup\">" +
                "               <Param name=\"ref\" value=\"Data\"/>" +
                "           </Fixup>" +
                "       </Number>" +
                "       <Blob name=\"Data\" value=\"Hello\"/>" +
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
            // -- this is the pre-calculated checksum from Peach2.3 on the blob: "Hello"
            byte[] precalcChecksum = new byte[] { 0xDC, 0x2D };
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual(precalcChecksum, values[0].Value);
        }

        [Test]
        public void Test2()
        {
            // standard test (Even length string)

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
				"       <Number name=\"ICMPChecksum\" signed=\"false\" endian=\"big\" size=\"16\">" +
                "           <Fixup class=\"IcmpChecksumFixup\">" +
                "               <Param name=\"ref\" value=\"Data\"/>" +
                "           </Fixup>" +
                "       </Number>" +
                "       <Blob name=\"Data\" value=\"1234\"/>" +
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
            // -- this is the pre-calculated checksum from Peach2.3 on the blob: "Hello"
            byte[] precalcChecksum = new byte[] { 0x9b, 0x99 };
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual(precalcChecksum, values[0].Value);
        }

    }
}

// end
