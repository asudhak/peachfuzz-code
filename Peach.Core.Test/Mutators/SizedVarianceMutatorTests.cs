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
    class SizedVarianceMutatorTests
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
            // standard test ... change the length of sizes to count +/- N (default is 50)
            // - Initial string: "AAAAA"
            // - will produce 1 A through 55 A's (the initial value just wraps when expanding and we negate <= 0 sized results)
            // - NOTE: this mutator will update the length of the size relation

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <String name=\"sizeRelation1\">" +
                "           <Relation type=\"size\" of=\"string1\"/>" +
                "       </String>" +
                "       <String name=\"string1\" value=\"AAAAA\"/>" +
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
            dom.tests[0].includedMutators.Add("SizedVaranceMutator");

            RunConfiguration config = new RunConfiguration();

            Dom.Action.Finished += new ActionFinishedEventHandler(Action_FinishedTest);

            Engine e = new Engine(null);
            e.config = config;
            e.startFuzzing(dom, config);

            // verify values
            Assert.IsTrue(listResults.Count == 55);
            for (int i = 0; i < 55; ++i)
            {
                Assert.AreEqual(i + 1, listResults[i].size);
                Assert.AreEqual(listResults[i].size, listResults[i].value.Length);
            }

            // reset
            firstPass = true;
            listResults.Clear();
        }

        [Test]
        public void Test2()
        {
            // standard test ... change the length of sizes to count +/- N (N = 5)
            // - Initial string: "AAAAA"
            // - will produce 1 A through 10 A's (the initial value just wraps when expanding and we negate <= 0 sized results)
            // - NOTE: this mutator will update the length of the size relation

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <String name=\"sizeRelation1\">" +
                "           <Relation type=\"size\" of=\"string1\"/>" +
                "           <Hint name=\"SizedVaranceMutator-N\" value=\"5\"/>" +
                "       </String>" +
                "       <String name=\"string1\" value=\"AAAAA\"/>" +
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
            dom.tests[0].includedMutators.Add("SizedVaranceMutator");

            RunConfiguration config = new RunConfiguration();

            Dom.Action.Finished += new ActionFinishedEventHandler(Action_FinishedTest);

            Engine e = new Engine(null);
            e.config = config;
            e.startFuzzing(dom, config);

            // verify values
            Assert.IsTrue(listResults.Count == 10);
            for (int i = 0; i < 10; ++i)
            {
                Assert.AreEqual(i + 1, listResults[i].size);
                Assert.AreEqual(listResults[i].size, listResults[i].value.Length);
            }

            // reset
            firstPass = true;
            listResults.Clear();
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
                tr.size = (long)action.dataModel[0].InternalValue;
                tr.value = action.dataModel[1].Value.Value;
                listResults.Add(tr);
            }
        }
    }
}

// end
