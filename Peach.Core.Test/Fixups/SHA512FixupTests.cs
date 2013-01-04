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
    class SHA512FixupTests : DataModelCollector
    {
        [Test]
        public void Test1()
        {
            // standard test

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Blob name=\"Checksum\">" +
                "           <Fixup class=\"SHA512Fixup\">" +
                "               <Param name=\"ref\" value=\"Data\"/>" +
                "           </Fixup>" +
                "       </Blob>" +
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
            // -- this is the pre-calculated checksum from Peach2.3 on the blob: { 1, 2, 3, 4, 5 }
            byte[] precalcChecksum = new byte[] 
            { 
                0x36, 0x27, 0x90, 0x9A, 0x29, 0xC3, 0x13, 0x81, 0xA0, 0x71, 0xEC, 0x27, 0xF7, 0xC9, 0xCA, 0x97,
                0x72, 0x61, 0x82, 0xAE, 0xD2, 0x9A, 0x7D, 0xDD, 0x2E, 0x54, 0x35, 0x33, 0x22, 0xCF, 0xB3, 0x0A,
                0xBB, 0x9E, 0x3A, 0x6D, 0xF2, 0xAC, 0x2C, 0x20, 0xFE, 0x23, 0x43, 0x63, 0x11, 0xD6, 0x78, 0x56,
                0x4D, 0x0C, 0x8D, 0x30, 0x59, 0x30, 0x57, 0x5F, 0x60, 0xE2, 0xD3, 0xD0, 0x48, 0x18, 0x4D, 0x79
            };
            
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual(precalcChecksum, values[0].Value);
        }
    }
}

// end
