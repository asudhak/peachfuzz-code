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
    class SizedNumericalEdgeCasesMutatorTests : DataModelCollector
    {
        [Test]
        public void Test1()
        {
            // standard test ... change the length of sizes to +/- 50 around numerical edge cases ( >= 0 )
            // - Initial string: "AAAAA"
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
            dom.tests[0].includedMutators.Add("SizedNumericalEdgeCasesMutator");

            RunConfiguration config = new RunConfiguration();

            Engine e = new Engine(null);
            e.config = config;
            e.startFuzzing(dom, config);

            // verify values
            Assert.Greater(dataModels.Count, 1);
            foreach (var item in dataModels)
            {
                Assert.AreEqual(Variant.VariantType.Long, item[0].InternalValue.GetVariantType());
                Assert.AreEqual((long)item[0].InternalValue, item[1].Value.Value.Length);
            }
        }
    }
}

// end
