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
    class ArrayReverseOrderMutatorTests : DataModelCollector
    {
        [Test]
        public void Test1()
        {
            // standard test - will reverse the order of the array
            // 01234 -> 43210

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <String name=\"a0\" value=\"0\" maxOccurs=\"100\"/>" +
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
            dom.tests[0].includedMutators.Add("ArrayReverseOrderMutator");

            var myArray = (Dom.Array)dom.tests[0].stateModel.initialState.actions[0].dataModel[0];
            myArray.origionalElement = myArray[0];
            myArray.hasExpanded = true;
            myArray.Add(new Dom.String("a1") { DefaultValue = new Variant("1") });
            myArray.Add(new Dom.String("a2") { DefaultValue = new Variant("2") });
            myArray.Add(new Dom.String("a3") { DefaultValue = new Variant("3") });
            myArray.Add(new Dom.String("a4") { DefaultValue = new Variant("4") });

            RunConfiguration config = new RunConfiguration();

            Engine e = new Engine(null);
            e.startFuzzing(dom, config);

            // verify values
            Assert.AreEqual(1, mutations.Count);
            Assert.AreEqual(Variant.VariantType.BitStream, mutations[0].GetVariantType());
            byte[] item = (byte[])mutations[0];
            Assert.NotNull(item);
            Assert.AreEqual(5, item.Length);
            Assert.AreEqual((byte)('4'), item[0]);
            Assert.AreEqual((byte)('3'), item[1]);
            Assert.AreEqual((byte)('2'), item[2]);
            Assert.AreEqual((byte)('1'), item[3]);
            Assert.AreEqual((byte)('0'), item[4]);
        }
    }
}

// end
