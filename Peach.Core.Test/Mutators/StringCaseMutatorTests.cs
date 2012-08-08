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
                "       <Strategy class=\"Sequencial\"/>" +
                "   </Test>" +

                "   <Run name=\"DefaultRun\">" +
                "       <Test ref=\"TheTest\"/>" +
                "   </Run>" +
                "</Peach>";

            PitParser parser = new PitParser();

            Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
            dom.tests[0].includedMutators = new List<string>();
            dom.tests[0].includedMutators.Add("StringCaseMutator");

            RunConfiguration config = new RunConfiguration();

            Engine e = new Engine(null);
            e.config = config;
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
    }
}

// end
