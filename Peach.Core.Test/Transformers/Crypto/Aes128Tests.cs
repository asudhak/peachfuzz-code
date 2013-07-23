using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Analyzers;

namespace Peach.Core.Test.Transformers.Crypto
{
    [TestFixture]
    class Aes128Tests : DataModelCollector
    {

        [Test]
        public void KeySize128Test()
        {
            // standard test
            RunTest("ae1234567890aeaffeda214354647586", "aeaeaeaeaeaeaeaeaeaeaeaeaeaeaeae", 
                new byte[] { 0x80, 0xc4, 0x9d, 0xb8, 0xd5, 0xc6, 0xdc, 0x9d, 0xbe, 0xf5, 0xd0, 0x75, 0xa8, 0xb3, 0x10, 0x49 });
        }

        [Test]
        public void KeySize256Test()
        {
            // standard test
            RunTest("ae1234567890aeaffeda214354647586ae1234567890aeaffeda214354647586", "aeaeaeaeaeaeaeaeaeaeaeaeaeaeaeae", 
                new byte[] { 0x2f, 0x1b, 0xe1, 0x64, 0xf7, 0x58, 0xe7, 0xe5, 0x0d, 0x73, 0x2e, 0x01, 0x38, 0x39, 0x1c, 0x2d });
        }

        [Test, ExpectedException(typeof(PeachException))]
        public void WrongSizedKeyTest()
        {
            string msg;

            if (Platform.GetOS() == Platform.OS.Windows)
                msg = "Error, unable to create instance of 'Transformer' named 'Aes128'.\nExtended error: Exception during object creation: Specified key is not a valid size for this algorithm.";
            else
                msg = "Error, unable to create instance of 'Transformer' named 'Aes128'.\nExtended error: Exception during object creation: Key size not supported by algorithm";

            try
            {
                RunTest("aaaa", "aeaeaeaeaeaeaeaeaeaeaeaeaeaeaeae", new byte[] { });
            }
            catch (Exception ex)
            {
                Assert.AreEqual(msg, ex.Message);
                throw;
            }
        }

        [Test, ExpectedException(typeof(PeachException))]
        public void WrongSizedIV()
        {
            string msg;

            if (Platform.GetOS() == Platform.OS.Windows)
                msg = "Error, unable to create instance of 'Transformer' named 'Aes128'.\nExtended error: Exception during object creation: Specified initialization vector (IV) does not match the block size for this algorithm.";
            else
                msg = "Error, unable to create instance of 'Transformer' named 'Aes128'.\nExtended error: Exception during object creation: IV length is different than block size";

            try
            {
                RunTest("ae1234567890aeaffeda214354647586", "aaaa", new byte[] { });
            }
            catch (Exception ex)
            {
                Assert.AreEqual(msg, ex.Message);
                throw;
            }
        }

        public void RunTest(string key, string iv, byte[] expected)
        {
            // standard test

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "        <Blob name=\"Data\" value=\"Hello\">" +
                "           <Transformer class=\"Aes128\">" +
                "               <Param name=\"Key\" value=\"{0}\"/>" +
                "               <Param name=\"IV\" value=\"{1}\"/>" +
                "           </Transformer>" +
                "        </Blob>" +
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
            xml = string.Format(xml, key, iv);
            PitParser parser = new PitParser();

            Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

            RunConfiguration config = new RunConfiguration();
            config.singleIteration = true;

            Engine e = new Engine(null);
            e.startFuzzing(dom, config);

            // verify values
            // -- this is the pre-calculated result on the blob: "Hello"
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual(expected, values[0].ToArray());
        }
    }
}
