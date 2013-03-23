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
    class BlobDWORDSliderMutatorTests : DataModelCollector
    {
        [Test]
        public void Test1()
        {
            // standard test of sliding a DWORD through a 8 byte blob
            // { 00 01 02 03 04 05 06 07 } becomes...
            // { FF FF FF FF 04 05 06 07 }, { 00 FF FF FF FF 05 06 07 }, { 00 01 FF FF FF FF 06 07 } ... etc

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Blob name=\"blob1\" length=\"8\" valueType=\"hex\" value=\"00 01 02 03 04 05 06 07\"/>" +
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
            dom.tests[0].includedMutators.Add("BlobDWORDSliderMutator");

            RunConfiguration config = new RunConfiguration();

            Engine e = new Engine(null);
            e.startFuzzing(dom, config);

            // verify values
            byte[][] expected = new byte[][]{
                new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x04, 0x05, 0x06, 0x07 },
                new byte[] { 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x05, 0x06, 0x07 },
                new byte[] { 0x00, 0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0x06, 0x07 },
                new byte[] { 0x00, 0x01, 0x02, 0xFF, 0xFF, 0xFF, 0xFF, 0x07 },
                new byte[] { 0x00, 0x01, 0x02, 0x03, 0xFF, 0xFF, 0xFF, 0xFF },
                new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0xFF, 0xFF, 0xFF },
                new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0xFF, 0xFF },
                new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0xFF }
            };

            Assert.AreEqual(expected.Length, mutations.Count);
            for (int i = 0; i < expected.Length; ++i)
            {
                Assert.AreEqual(Variant.VariantType.BitStream, mutations[i].GetVariantType());
                Assert.AreEqual(expected[i], (byte[])mutations[i]);
            }
        }

        [Test]
        public void Test2()
        {
            // testing "off" hint, which means nothing should happen!

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Blob name=\"blob1\" length=\"8\" valueType=\"hex\" value=\"00 01 02 03 04 05 06 07\">" +
                "           <Hint name=\"BlobDWORDSliderMutator\" value=\"off\"/>" +
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
            dom.tests[0].includedMutators.Add("BlobDWORDSliderMutator");

            RunConfiguration config = new RunConfiguration();

            Engine e = new Engine(null);
            e.startFuzzing(dom, config);

            // verify values
            Assert.IsEmpty(mutations);
        }

        List<byte[]> DoMutation()
        {
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Blob name=\"blob1\" length=\"8\" valueType=\"hex\" value=\"00 01 02 03 04 05 06 07\"/>" +
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
            dom.tests[0].includedMutators.Add("BlobDWORDSliderMutator");

            RunConfiguration config = new RunConfiguration();
            config.range = true;
            config.rangeStart = 0;
            config.rangeStop = 99;
            config.randomSeed = 100;

            Engine e = new Engine(null);
            e.startFuzzing(dom, config);

            // verify values
            Assert.AreEqual(99, mutations.Count);

            List<byte[]> ret = new List<byte[]>();
            foreach (var item in mutations)
                ret.Add((byte[])item);

            ResetContainers();

            return ret;
        }

        [Test]
        public void Test3()
        {
            // Test that random mutations stay within the correct bounds
            // and are reproducable across runs

            var run1 = DoMutation();
            var run2 = DoMutation();

            Assert.AreEqual(run1.Count, run2.Count);

            for (int i = 0; i < run1.Count; ++i)
            {
                bool hasDword = false;
                bool areEqual = true;
                for (int j = 0; j < run1[i].Length; ++j)
                {
                    if (run1[i][j] != run2[i][j])
                        areEqual = false;
                    if (run1[i][j] == 0xff)
                        hasDword = true;
                }
                Assert.True(hasDword);
                Assert.True(areEqual);
            }
        }
    }
}

// end
