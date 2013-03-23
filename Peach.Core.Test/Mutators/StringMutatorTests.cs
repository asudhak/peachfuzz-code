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
    class StringMutatorTests : DataModelCollector
    {
        [Test]
        public void Test1()
        {
            // standard test generating odd unicode strings for each <String> element

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
            dom.tests[0].includedMutators.Add("StringMutator");

            RunConfiguration config = new RunConfiguration();

            Engine e = new Engine(null);
            e.startFuzzing(dom, config);

            // verify first two values, last two values, and count (= 2379)
            string val1 = "Peach";
            string val2 = "abcdefghijklmnopqrstuvwxyz";
            string val3 = "18446744073709551664";
            string val4 = "10";

            Assert.AreEqual(2379, mutations.Count);
            Assert.AreEqual(val1, (string)mutations[0]);
            Assert.AreEqual(val2, (string)mutations[1]);
            Assert.AreEqual(val3, (string)mutations[mutations.Count - 2]);
            Assert.AreEqual(val4, (string)mutations[mutations.Count - 1]);
        }
    }
}

// end
