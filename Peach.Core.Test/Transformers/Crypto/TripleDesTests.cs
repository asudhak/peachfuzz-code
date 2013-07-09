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
    class TripleDesTests : DataModelCollector
    {
        [Test]
        public void KeySize128Test()
        {
            RunTest("ae1234567890aeaffeda214354647586fefdfaddefeeaf12", "aeaeaeaeaeaeaeae", new byte[] { 0x5d, 0xa5, 0x88, 0x82, 0x44, 0x24, 0x05, 0x67 });
        }

        [Test]
        public void KeySize192Test()
        {
            RunTest("ae1234567890aeaffeda214354647586", "aeaeaeaeaeaeaeae", new byte[] { 0x95, 0x4d, 0x29, 0x9a, 0xbc, 0x9d, 0x07, 0x5e });
        }

        [Test, ExpectedException(typeof(PeachException))]
        public void WrongSizedKeyTest()
        {
            string msg;

            if (Platform.GetOS() == Platform.OS.Windows)
                msg = "Error, unable to create instance of 'Transformer' named 'TripleDes'.\nExtended error: Exception during object creation: Specified key is not a valid size for this algorithm.";
            else
                msg = "Error, unable to create instance of 'Transformer' named 'TripleDes'.\nExtended error: Exception during object creation: Wrong Key Length";

            try
            {
                RunTest("aaaa", "aeaeaeaeaeaeaeae", new byte[]{});
            }
            catch (Exception ex)
            {
                Assert.AreEqual(msg, ex.Message);
                throw;
            }
        }

        [Test, ExpectedException(typeof(PeachException))]
        public void WeakKeyTest()
        {
            string msg;

            if (Platform.GetOS() == Platform.OS.Windows)
                msg = "Error, unable to create instance of 'Transformer' named 'TripleDes'.\nExtended error: Exception during object creation: Specified key is a known weak key for 'TripleDES' and cannot be used.";
            else
                msg = "Error, unable to create instance of 'Transformer' named 'TripleDes'.\nExtended error: Exception during object creation: Weak Key";

            try
            {
                RunTest("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", "aeaeaeaeaeaeaeae", new byte[] { });
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
                msg = "Error, unable to create instance of 'Transformer' named 'TripleDes'.\nExtended error: Exception during object creation: Specified initialization vector (IV) does not match the block size for this algorithm.";
            else
                msg = "Error, unable to create instance of 'Transformer' named 'TripleDes'.\nExtended error: Exception during object creation: IV length is different than block size";

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
                "           <Transformer class=\"TripleDes\">" +
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
