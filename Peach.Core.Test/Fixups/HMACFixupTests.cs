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
    class HMACFixupTests : DataModelCollector
    {
        [Test]
        public void Test1()
        {
            // standard test

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Blob name=\"Checksum\">" +
                "           <Fixup class=\"HMAC\">" +
                "               <Param name=\"ref\" value=\"Data\"/>" +
                "               <Param name=\"Key\" value=\"ae1234567890aeaffeda214354647586\"/>" +
                "               <Param name=\"Hash\" value=\"HMACSHA1\"/>" +
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
            byte[] precalcChecksum = new byte[] { 0xD2, 0xF9, 0x7D, 0xE0, 0x65, 0x0F, 0xE1, 0x24, 0xB7, 0xCF, 0xA5, 0x7F, 0x70, 0xA6, 0xBD, 0x68, 0xC7, 0x14, 0xCC, 0x41};
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual(precalcChecksum, values[0].ToArray());
        }

        [Test]
        public void TruncateTest()
        {
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Blob name=\"Checksum\">" +
                "           <Fixup class=\"HMAC\">" +
                "               <Param name=\"ref\" value=\"Data\"/>" +
                "               <Param name=\"Key\" value=\"4141414141414141414141414141414141414141\"/>" +
                "               <Param name=\"Hash\" value=\"HMACSHA1\"/>" +
                "               <Param name=\"Length\" value=\"12\"/>" +
                "           </Fixup>" +
                "       </Blob>" +
                "       <Blob name=\"Data\" valueType=\"hex\" value=\"0000020100000001baae9ef59ff1ee56211769bd91da50ed22e9ef5f17345fc170ff0b2e713c00e3811c991c817b6484c788762f6c22d0682b96addf984d28a11f156b950b6d12613201816bbd269d5c\"/>" +
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
            byte[] precalcChecksum = new byte[] { 0x37, 0xf4, 0xbd, 0x2e, 0x44, 0x93, 0x01, 0x3c, 0x5d, 0x82, 0x99, 0xf2 };
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual(precalcChecksum, values[0].ToArray());
        }

        [Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Error, unable to create instance of 'Fixup' named 'HMAC'.\nExtended error: Exception during object creation: The truncate length is greater than the hash size for the specified algorithm.")]
        public void LengthGreaterThanHashSizeTest()
        {
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Blob name=\"Checksum\">" +
                "           <Fixup class=\"HMAC\">" +
                "               <Param name=\"ref\" value=\"Data\"/>" +
                "               <Param name=\"Key\" value=\"4141414141414141414141414141414141414141\"/>" +
                "               <Param name=\"Hash\" value=\"HMACSHA1\"/>" +
                "               <Param name=\"Length\" value=\"21\"/>" +
                "           </Fixup>" +
                "       </Blob>" +
                "       <Blob name=\"Data\" valueType=\"hex\" value=\"0000020100000001baae9ef59ff1ee56211769bd91da50ed22e9ef5f17345fc170ff0b2e713c00e3811c991c817b6484c788762f6c22d0682b96addf984d28a11f156b950b6d12613201816bbd269d5c\"/>" +
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
        }

        [Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Error, unable to create instance of 'Fixup' named 'HMAC'.\nExtended error: Exception during object creation: The truncate length must be greater than or equal to 0.")]
        public void LengthLessThanZeroTest()
        {
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Blob name=\"Checksum\">" +
                "           <Fixup class=\"HMAC\">" +
                "               <Param name=\"ref\" value=\"Data\"/>" +
                "               <Param name=\"Key\" value=\"4141414141414141414141414141414141414141\"/>" +
                "               <Param name=\"Hash\" value=\"HMACSHA1\"/>" +
                "               <Param name=\"Length\" value=\"-1\"/>" +
                "           </Fixup>" +
                "       </Blob>" +
                "       <Blob name=\"Data\" valueType=\"hex\" value=\"0000020100000001baae9ef59ff1ee56211769bd91da50ed22e9ef5f17345fc170ff0b2e713c00e3811c991c817b6484c788762f6c22d0682b96addf984d28a11f156b950b6d12613201816bbd269d5c\"/>" +
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
        }
    }
}

// end
