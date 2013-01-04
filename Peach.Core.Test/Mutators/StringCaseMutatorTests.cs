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
    class StringCaseMutatorTests : DataModelCollector
    {
        [Test]
        public void Test1()
        {
            // standard test changing string case to all lower, all upper, and random case

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <String name=\"str1\" value=\"Hello, World!\"/>" +
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
                "       <Strategy class=\"Sequential\"/>" +
                "   </Test>" +
                "</Peach>";

            PitParser parser = new PitParser();

            Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
            dom.tests[0].includedMutators = new List<string>();
            dom.tests[0].includedMutators.Add("StringCaseMutator");

            RunConfiguration config = new RunConfiguration();

            Engine e = new Engine(null);
            e.startFuzzing(dom, config);

            // verify values
            Assert.AreEqual(3, mutations.Count);

            Assert.AreEqual(Variant.VariantType.String, mutations[0].GetVariantType());
            Assert.AreEqual("hello, world!", (string)mutations[0]);

            Assert.AreEqual(Variant.VariantType.String, mutations[1].GetVariantType());
            Assert.AreEqual("HELLO, WORLD!", (string)mutations[1]);

            Assert.AreEqual(Variant.VariantType.String, mutations[2].GetVariantType());
            Assert.AreNotEqual("Hello, World!", (string)mutations[2]);
            Assert.AreNotEqual("hello, world!", (string)mutations[2]);
            Assert.AreNotEqual("HELLO, WORLD!", (string)mutations[2]);
        }

        private List<string> DoMutation()
        {
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <String name=\"str1\" value=\"Hello World? Hello World!\"/>" +
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
                "       <Strategy class=\"Random\"/>" +
                "   </Test>" +
                "</Peach>";

            PitParser parser = new PitParser();

            Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
            dom.tests[0].includedMutators = new List<string>();
            dom.tests[0].includedMutators.Add("StringCaseMutator");

            RunConfiguration config = new RunConfiguration();
            config.range = true;
            config.rangeStart = 0;
            config.rangeStop = 999;
            config.randomSeed = 100;

            Engine e = new Engine(null);
            e.startFuzzing(dom, config);

            // verify values
            Assert.AreEqual(999, mutations.Count);

            List<string> ret = new List<string>();
            foreach (string item in mutations)
            {
                if (item != "hello world? hello world!" && item != "HELLO WORLD? HELLO WORLD!")
                {
                    ret.Add(item);
                }
            }

            ResetContainers();

            return ret;
        }

        [Test]
        public void Test2()
        {
            // Using the random strategy:
            // Test that random case flip produces consistent results for each run
            // but different results across each iteration

            var run1 = DoMutation();
            var run2 = DoMutation();

            Assert.AreEqual(run1.Count, run2.Count);

            // For 1000 iterations, about 1/3 of the time the random case will be picked
            Assert.Greater(run1.Count, 330);
            Assert.Less(run1.Count, 350);

            int numSame = 0;

            for (int i = 0; i < run1.Count; ++i)
            {
                for (int j = (i + 1); j < run2.Count; ++j)
                {
                    if (run1[i] == run2[j])
                        ++numSame;
                }
            }

            Assert.AreEqual(0, numSame);
        }
    }
}

// end
