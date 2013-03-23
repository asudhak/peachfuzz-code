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
    class BlobBitFlipperMutatorTests : DataModelCollector
    {
        static int CountBits(byte val)
        {
            int count = 0;
            for (int i = 0; i < 8; ++i)
            {
                count += val & 1;
                val = (byte)(val >> 1);
            }
            return count;
        }

        [Test]
        public void Test1()
        {
            // standard test of flipping 20% of the bits in a blob
            // : in this case, we'll use 1 byte with a value of 0, so we should get 1 bit flipped.

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Blob name=\"blob1\" length=\"1\" valueType=\"hex\" value=\"00\"/>" +
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
            dom.tests[0].includedMutators.Add("BlobBitFlipperMutator");

            RunConfiguration config = new RunConfiguration();

            Engine e = new Engine(null);
            e.startFuzzing(dom, config);

            // verify values
            Assert.AreEqual(1, mutations.Count);
            Assert.AreEqual(Variant.VariantType.ByteString, mutations[0].GetVariantType());
            byte[] item = (byte[])mutations[0];
            Assert.AreEqual(1, item.Length);
            Assert.AreEqual(1, CountBits(item[0]));
        }

        [Test]
        public void Test2()
        {
            // testing N-hint
            // : N = 50, flipping 4 bits of the byte, will get back 4 non-zero values

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Blob name=\"blob1\" length=\"1\" valueType=\"hex\" value=\"00\">" +
                "           <Hint name=\"BlobBitFlipperMutator-N\" value=\"50\"/>" +
                "       </Blob>" +
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
            dom.tests[0].includedMutators.Add("BlobBitFlipperMutator");

            RunConfiguration config = new RunConfiguration();

            Engine e = new Engine(null);
            e.startFuzzing(dom, config);

            // verify values 
            Assert.AreEqual(4, mutations.Count);
            foreach (var item in mutations)
            {
                Assert.AreEqual(Variant.VariantType.ByteString, item.GetVariantType());
                byte[] val = (byte[])item;
                Assert.AreEqual(1, val.Length);
                Assert.AreEqual(1, CountBits(val[0]));
            }
        }

        List<byte[]> DoMutation()
        {
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Blob name=\"blob1\" length=\"2000\" valueType=\"hex\" value=\"00\"/>" +
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
            dom.tests[0].includedMutators.Add("BlobBitFlipperMutator");

            RunConfiguration config = new RunConfiguration();
            config.range = true;
            config.rangeStart = 0;
            config.rangeStop = 999;
            config.randomSeed = 100;

            Engine e = new Engine(null);
            e.startFuzzing(dom, config);

            // verify values 
            Assert.AreEqual(999, mutations.Count);

            List<byte[]> ret = new List<byte[]>();
            foreach (var item in mutations)
                ret.Add((byte[])item);

            ResetContainers();

            return ret;
        }

        [Test]
        public void Test3()
        {
            // Using the random strategy:
            // Test that mutator produces consistent results for each run
            // but different results across each iteration.
            // TODO: Verify that random mutations honor the correct +/- N

            var run1 = DoMutation();
            var run2 = DoMutation();

            Assert.AreEqual(run1.Count, run2.Count);

            for (int i = 0; i < run1.Count; ++i)
            {
                Assert.True(run1[i].SequenceEqual(run2[i]));
            }

            int numSame = 0;
            for (int i = 0; i < run1.Count; ++i)
            {
                for (int j = (i + 1); j < run2.Count; ++j)
                {
                    if (run1[i].SequenceEqual(run2[j]))
                        ++numSame;
                }
            }

            Assert.Less(numSame, 300);
        }
    }
}

// end
