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
    class NumericalVarianceMutatorTests : DataModelCollector
    {
        [Test]
        public void Test1()
        {
            // standard test of generating values +/- 50 around default value (100)
            // - should produce values 50 - 150

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Number name=\"num1\" size=\"32\" value=\"100\" signed=\"false\"/>" +
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
            dom.tests[0].includedMutators.Add("NumericalVarianceMutator");

            RunConfiguration config = new RunConfiguration();

            Engine e = new Engine(null);
            e.startFuzzing(dom, config);

            // verify values
            Assert.AreEqual(101, mutations.Count);
            for (int i = 0; i <= 100; ++i)
            {
                Assert.AreEqual(Variant.VariantType.ULong, mutations[i].GetVariantType());
                Assert.AreEqual(50 + i, (ulong)mutations[i]);
            }
        }

        [Test]
        public void Test2()
        {
            // testing N-hint
            // : N = 5, generating values 95 - 105

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Number name=\"num1\" size=\"32\" value=\"100\" signed=\"false\">" +
                "           <Hint name=\"NumericalVarianceMutator-N\" value=\"5\"/>" +
                "       </Number>" +
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
            dom.tests[0].includedMutators.Add("NumericalVarianceMutator");

            RunConfiguration config = new RunConfiguration();

            Engine e = new Engine(null);
            e.startFuzzing(dom, config);

            // verify values
            Assert.AreEqual(11, mutations.Count);
            for (int i = 0; i <= 10; ++i)
            {
                Assert.AreEqual(Variant.VariantType.ULong, mutations[i].GetVariantType());
                Assert.AreEqual(95 + i, (ulong)mutations[i]);
            }
        }

        [Test]
        public void Test3()
        {
            // testing numerical string

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <String name=\"numStr1\" value=\"100\">" +
                "           <Hint name=\"NumericalString\" value=\"true\"/>" +
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
            dom.tests[0].includedMutators.Add("NumericalVarianceMutator");

            RunConfiguration config = new RunConfiguration();

            Engine e = new Engine(null);
            e.startFuzzing(dom, config);

            // verify values
            Assert.AreEqual(101, mutations.Count);
            for (int i = 0; i <= 100; ++i)
            {
                Assert.AreEqual(Variant.VariantType.String, mutations[i].GetVariantType());
                Assert.AreEqual((50 + i).ToString(), (string)mutations[i]);
            }
        }

        [Test]
        public void Test4()
        {
            // testing INVALID use of numerical string, this should produce 0 results

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <String name=\"numStr1\" value=\"abc\">" +
                "           <Hint name=\"NumericalString\" value=\"true\"/>" +
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
            dom.tests[0].includedMutators.Add("NumericalVarianceMutator");

            RunConfiguration config = new RunConfiguration();

            Engine e = new Engine(null);
            e.startFuzzing(dom, config);

            // mutations should be empty!!
            Assert.IsEmpty(mutations);
        }

        [Test]
        public void Test5()
        {
            // standard test of generating values +/- 50 around default value (-1)
            // - should produce values -5 - 49

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Number name=\"num1\" size=\"32\" value=\"-1\" signed=\"true\"/>" +
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
            dom.tests[0].includedMutators.Add("NumericalVarianceMutator");

            RunConfiguration config = new RunConfiguration();

            Engine e = new Engine(null);
            e.startFuzzing(dom, config);

            // verify values
            Assert.AreEqual(101, mutations.Count);
            for (int i = 0; i <= 100; ++i)
            {
                Assert.AreEqual(Variant.VariantType.Long, mutations[i].GetVariantType());
                Assert.AreEqual(i - 51, (long)mutations[i]);
            }
        }

    }
}

// end
