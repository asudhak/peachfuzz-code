using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Analyzers;
using Peach.Core.IO;

namespace Peach.Core.Test.Mutators
{
    [TestFixture]
    class ArrayRandomizeOrderMutatorTests : DataModelCollector
    {
        [Test]
        public void Test1()
        {
            // standard test - will generate random permutations of the array (default 50)
            // 01234 -> ?????

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
            dom.tests[0].includedMutators.Add("ArrayRandomizeOrderMutator");

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
            int numSame = 0;
            byte[] ogArray = { (byte)('0'), (byte)('1'), (byte)('2'), (byte)('3'), (byte)('4') };
            Assert.AreEqual(50, mutations.Count);

            foreach (var item in mutations)
            {
                Assert.AreEqual(Variant.VariantType.BitStream, item.GetVariantType());
                byte[] val = (byte[])item;
                Assert.NotNull(val);
                Assert.AreEqual(ogArray.Length, val.Length);
                if (ogArray.SequenceEqual(val))
                    ++numSame;
            }
            Assert.LessOrEqual(numSame, 2);
        }

        [Test]
        public void Test2()
        {
            // standard test - will generate N random permutations of the array (N = 5)
            // 01234 -> ?????

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <String name=\"a0\" value=\"0\" maxOccurs=\"100\">" +
                "           <Hint name=\"ArrayRandomizeOrderMutator-N\" value=\"5\"/>" +
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
            dom.tests[0].includedMutators.Add("ArrayRandomizeOrderMutator");

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
            int numSame = 0;
            byte[] ogArray = { (byte)('0'), (byte)('1'), (byte)('2'), (byte)('3'), (byte)('4') };
            Assert.AreEqual(5, mutations.Count);

            foreach (var item in mutations)
            {
                Assert.AreEqual(Variant.VariantType.BitStream, item.GetVariantType());
                byte[] val = (byte[])item;
                Assert.NotNull(val);
                Assert.AreEqual(ogArray.Length, val.Length);
                if (ogArray.SequenceEqual(val))
                    ++numSame;
            }
            Assert.LessOrEqual(numSame, 2);
        }

        [Test]
        public void Test3()
        {
            // Using the sequential strategy:
            // Test that mutator produces consistent results for each run
            // but different results across each iteration

            Test1();
            Assert.AreEqual(50, mutations.Count);

            var oldMutations = mutations;

            ResetContainers();
            Assert.AreEqual(0, mutations.Count);

            Test1();
            Assert.AreEqual(50, mutations.Count);

            int numSame = 0;
            for (int i = 0; i < 50; ++i)
            {
                Assert.AreEqual(mutations[i], oldMutations[i]);
                for (int j = (i+1); j < 50; ++j)
                {
                    if (((byte[])mutations[i]).SequenceEqual((byte[])mutations[j]))
                        ++numSame;
                }
            }

            Assert.Less(numSame, 12);
        }
    }
}

// end
