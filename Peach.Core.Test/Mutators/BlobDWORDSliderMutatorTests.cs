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

namespace Peach.Core.Test.Mutators
{
    [TestFixture]
    class BlobDWORDSliderMutatorTests
    {
        bool firstPass = true;
        byte[] result;
        List<byte[]> testResults = new List<byte[]>();

        [Test]
        public void Test1()
        {
            // standard test of sliding a DWORD through a 8 byte blob
            // { 00 01 02 03 04 05 06 07 } becomes...
            // { FF FF FF FF 04 05 06 07 }, { 00 FF FF FF FF 05 06 07 }, { 00 01 FF FF FF FF 06 07 } ... etc

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Blob name=\"blob1\" length=\"8\" valueType=\"hex\" value=\"00 01 02 03 04 05 06 07\"/>" +
                "   </DataModel>" +

                "   <StateModel name=\"TheState\" initialState=\"Initial\">" +
                "       <State name=\"Initial\">" +
                "           <Action type=\"output\">" +
                "               <DataModel ref=\"TheDataModel\"/>" +
                "           </Action>" +
                "       </State>" +
                "   </StateModel>" +

                "   <Test name=\"TheTest\">" +
                "       <StateModel ref=\"TheState\"/>" +
                "       <Publisher class=\"Stdout\"/>" +
				"		<Strategy class=\"Sequencial\"/>" +
				"   </Test>" +

                "   <Run name=\"DefaultRun\">" +
                "       <Test ref=\"TheTest\"/>" +
                "   </Run>" +
                "</Peach>";

            PitParser parser = new PitParser();

            Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
            dom.tests[0].includedMutators = new List<string>();
            dom.tests[0].includedMutators.Add("BlobDWORDSliderMutator");

            RunConfiguration config = new RunConfiguration();

            Dom.Action.Finished += new ActionFinishedEventHandler(Action_FinishedTest);

            Engine e = new Engine(null);
            e.config = config;
            e.startFuzzing(dom, config);

            // verify values
            Assert.IsTrue(testResults.Count == 8);
            Assert.AreEqual(testResults[0], new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x04, 0x05, 0x06, 0x07 });
            Assert.AreEqual(testResults[1], new byte[] { 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x05, 0x06, 0x07 });
            Assert.AreEqual(testResults[2], new byte[] { 0x00, 0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0x06, 0x07 });
            Assert.AreEqual(testResults[3], new byte[] { 0x00, 0x01, 0x02, 0xFF, 0xFF, 0xFF, 0xFF, 0x07 });
            Assert.AreEqual(testResults[4], new byte[] { 0x00, 0x01, 0x02, 0x03, 0xFF, 0xFF, 0xFF, 0xFF });
            Assert.AreEqual(testResults[5], new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0xFF, 0xFF, 0xFF });
            Assert.AreEqual(testResults[6], new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0xFF, 0xFF });
            Assert.AreEqual(testResults[7], new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0xFF });

            // reset
            firstPass = true;
            result = null;
            testResults.Clear();
        }

        [Test]
        public void Test2()
        {
            // testing "off" hint, which means nothing should happen!

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Blob name=\"blob1\" length=\"8\" valueType=\"hex\" value=\"00 01 02 03 04 05 06 07\">" +
                "           <Hint name=\"BlobDWORDSliderMutator\" value=\"off\"/>" +
                "       </Blob>" +
                "   </DataModel>" +

                "   <StateModel name=\"TheState\" initialState=\"Initial\">" +
                "       <State name=\"Initial\">" +
                "           <Action type=\"output\">" +
                "               <DataModel ref=\"TheDataModel\"/>" +
                "           </Action>" +
                "       </State>" +
                "   </StateModel>" +

                "   <Test name=\"TheTest\">" +
                "       <StateModel ref=\"TheState\"/>" +
                "       <Publisher class=\"Stdout\"/>" +
                "   </Test>" +

                "   <Run name=\"DefaultRun\">" +
                "       <Test ref=\"TheTest\"/>" +
                "   </Run>" +
                "</Peach>";

            PitParser parser = new PitParser();

            Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
            dom.tests[0].includedMutators = new List<string>();
            dom.tests[0].includedMutators.Add("BlobDWORDSliderMutator");

            RunConfiguration config = new RunConfiguration();

            Dom.Action.Finished += new ActionFinishedEventHandler(Action_FinishedTest);

            Engine e = new Engine(null);
            e.config = config;
            e.startFuzzing(dom, config);

            // verify values
            // - list should be empty!
            Assert.IsEmpty(testResults);

            // reset
            firstPass = true;
            result = null;
            testResults.Clear();
        }

        void Action_FinishedTest(Dom.Action action)
        {
            if (firstPass)
            {
                firstPass = false;
            }
            else
            {
                result = action.dataModel[0].Value.Value;
                testResults.Add(result);
            }
        }
    }
}

// end
