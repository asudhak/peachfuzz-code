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
    class XmlW3CMutatorTests : DataModelCollector
    {
        [Test]
        public void Test1()
        {
            // standard test performing the W3C parser tests for each <String> element, only works with hints: "XMLHint" / "xml"

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <String name=\"str1\" value=\"Hello, World!\">" +
                "           <Hint name=\"XMLhint\" value=\"xml\"/>" +
                "       </String>" +
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
            dom.tests[0].includedMutators.Add("XmlW3CParserTestsMutator");

            RunConfiguration config = new RunConfiguration();

            Engine e = new Engine(null);
            e.startFuzzing(dom, config);

            // verify count (= 1510)           
            Assert.AreEqual(1510, mutations.Count);
            foreach (var item in mutations)
            {
                Assert.AreEqual(Variant.VariantType.ByteString, item.GetVariantType());
                Assert.NotNull((byte[])item);
            }
        }
    }
}

// end
