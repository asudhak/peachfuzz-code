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
    class BlobBitFlipperMutatorTests
    {
        bool firstPass = true;
        byte[] result;
        List<byte[]> testResults = new List<byte[]>();

        [Test]
        public void Test1()
        {
            // standard test of flipping 20% of the bits in a blob
            // : in this case, we'll use 1 byte with a value of 0, so we should get 1 bit flipped.

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Blob name=\"blob1\" length=\"1\" valueType=\"hex\" value=\"00\"/>" +
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
            dom.tests[0].includedMutators.Add("BlobBitFlipperMutator");

            RunConfiguration config = new RunConfiguration();

            Dom.Action.Finished += new ActionFinishedEventHandler(Action_FinishedTest);

            Engine e = new Engine(null);
            e.config = config;
            e.startFuzzing(dom, config);

            // verify values
            Assert.IsTrue((testResults[0][0] == 1) | (testResults[0][0] == 2) | (testResults[0][0] == 4) | (testResults[0][0] == 8) | (testResults[0][0] == 16) | (testResults[0][0] == 32) | (testResults[0][0] == 64) | (testResults[0][0] == 128));

            // reset
            firstPass = true;
            result = null;
            testResults.Clear();
        }

        [Test]
        public void Test2()
        {
            // testing N-hint
            // : N = 50, flipping 4 bits of the byte, will get back 4 non-zero values

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Blob name=\"blob1\" length=\"1\" valueType=\"hex\" value=\"00\">" +
                "           <Hint name=\"BlobBitFlipperMutator-N\" value=\"50\"/>" +
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
            dom.tests[0].includedMutators.Add("BlobBitFlipperMutator");

            RunConfiguration config = new RunConfiguration();

            Dom.Action.Finished += new ActionFinishedEventHandler(Action_FinishedTest);

            Engine e = new Engine(null);
            e.config = config;
            e.startFuzzing(dom, config);

            // verify values 
            Assert.IsTrue(testResults.Count == 4);
            Assert.IsTrue((testResults[0][0] == 1) | (testResults[0][0] == 2) | (testResults[0][0] == 4) | (testResults[0][0] == 8) | (testResults[0][0] == 16) | (testResults[0][0] == 32) | (testResults[0][0] == 64) | (testResults[0][0] == 128));
            Assert.IsTrue((testResults[1][0] == 1) | (testResults[1][0] == 2) | (testResults[1][0] == 4) | (testResults[1][0] == 8) | (testResults[1][0] == 16) | (testResults[1][0] == 32) | (testResults[1][0] == 64) | (testResults[1][0] == 128));
            Assert.IsTrue((testResults[2][0] == 1) | (testResults[2][0] == 2) | (testResults[2][0] == 4) | (testResults[2][0] == 8) | (testResults[2][0] == 16) | (testResults[2][0] == 32) | (testResults[2][0] == 64) | (testResults[2][0] == 128));
            Assert.IsTrue((testResults[3][0] == 1) | (testResults[3][0] == 2) | (testResults[3][0] == 4) | (testResults[3][0] == 8) | (testResults[3][0] == 16) | (testResults[3][0] == 32) | (testResults[3][0] == 64) | (testResults[3][0] == 128));

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
