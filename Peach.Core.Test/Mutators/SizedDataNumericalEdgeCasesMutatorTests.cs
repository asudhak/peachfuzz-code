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
    class SizedDataNumericalEdgeCasesMutatorTests : DataModelCollector
    {
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
                "       <Strategy class=\"Sequential\"/>" +
                "   </Test>" +
                "</Peach>";

            PitParser parser = new PitParser();

            Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
            dom.tests[0].includedMutators = new List<string>();
            dom.tests[0].includedMutators.Add("SizedDataNumericalEdgeCasesMutator");

            RunConfiguration config = new RunConfiguration();

            Engine e = new Engine(null);
            e.startFuzzing(dom, config);

            // verify values
            Assert.Greater(dataModels.Count, 1);
            Assert.AreEqual(Variant.VariantType.Long, dataModels[0][0].InternalValue.GetVariantType());
            Assert.AreEqual(5, (long)dataModels[0][0].InternalValue);
            Assert.AreEqual(Encoding.ASCII.GetBytes("AAAAA"), dataModels[0][1].Value.Value);

            for (int i = 1; i < dataModels.Count; ++i)
            {
                var num = dataModels[i][0].InternalValue;
                var str = dataModels[i][1].Value.Value;
                Assert.AreEqual(Variant.VariantType.BitStream, num.GetVariantType());
                Assert.AreEqual(5, new BitStream((byte[])num).ReadInt32());
                if ((i - 1) <= 50)
                    Assert.AreEqual(i - 1, str.Length);
                else
                    Assert.Greater(str.Length, 50);
            }
        }
    }
}

// end
