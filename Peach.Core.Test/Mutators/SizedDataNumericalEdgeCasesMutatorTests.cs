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
    class SizedDataNumericalEdgeCasesMutatorTests
    {
        bool firstPass = true;

        public struct TestResult
        {
            public long size;
            public byte[] value;

            public TestResult(long sz, byte[] vals)
            {
                size = sz;
                value = vals;
            }
        }

        List<TestResult> listResults = new List<TestResult>();

        [Test]
        public void Test1()
        {
            // standard test ... change the length of sizes to +/- 50 around numerical edge cases ( >= 0 )
            // - Initial string: "AAAAA"
            // - NOTE: this mutator will *NOT* update the length of the size relation

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Number name=\"sizeRelation1\" size=\"32\">" +
                "           <Relation type=\"size\" of=\"string1\"/>" +
                "       </Number>" +
                "       <String name=\"string1\" value=\"AAAAA\"/>" +
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
				"		<Strategy class=\"Sequencial\"/>" +
				"   </Test>" +

                "   <Run name=\"DefaultRun\">" +
                "       <Test ref=\"TheTest\"/>" +
                "   </Run>" +
                "</Peach>";

            PitParser parser = new PitParser();

            Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
            dom.tests[0].includedMutators = new List<string>();
            dom.tests[0].includedMutators.Add("SizedDataNumericalEdgeCasesMutator");

            RunConfiguration config = new RunConfiguration();

            Dom.Action.Finished += new ActionFinishedEventHandler(Action_FinishedTest);

            Engine e = new Engine(null);
            e.config = config;
            e.startFuzzing(dom, config);

            // verify values
            for (int i = 10; i < listResults.Count && i < 20; ++i)
            {
                Assert.AreNotEqual(listResults[i].size, listResults[i].value.Length);
            }

            // reset
            firstPass = true;
            listResults.Clear();
			Dom.Action.Finished -= Action_FinishedTest;
        }

        void Action_FinishedTest(Dom.Action action)
        {
            if (firstPass)
            {
                firstPass = false;
            }
            else
            {
                TestResult tr;
                tr.size = new BitStream((byte[])action.dataModel[0].InternalValue).ReadInt32();
                tr.value = action.dataModel[1].Value.Value;
                listResults.Add(tr);
            }
        }
    }
}

// end
