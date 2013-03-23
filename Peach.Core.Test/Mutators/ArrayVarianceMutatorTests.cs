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
    class ArrayVarianceMutatorTests : DataModelCollector
    {
        [Test]
        public void Test1()
        {
            // standard test -- change the length of the array to count - N to count + N (default is 50)
            // 01234 -> [0, 01, 012, 0123, 01234, 012344, 0123444, ... len(55)]

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
            dom.tests[0].includedMutators.Add("ArrayVarianceMutator");

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
            Assert.AreEqual(56, mutations.Count);
        }

        [Test]
        public void Test2()
        {
            // standard test -- change the length of the array to count - N to count + N (N = 5)
            // 01234 -> [0, 01, 012, 0123, 01234, 012344, 0123444, 01234444, 012344444, 0123444444]

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <String name=\"a0\" value=\"0\" maxOccurs=\"100\">" +
                "           <Hint name=\"ArrayVarianceMutator-N\" value=\"5\"/>" +
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
            dom.tests[0].includedMutators.Add("ArrayVarianceMutator");

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
            byte[][] expected = new byte[][] {
                new byte[0],
                Encoding.ASCII.GetBytes("0"),
                Encoding.ASCII.GetBytes("01"),
                Encoding.ASCII.GetBytes("012"),
                Encoding.ASCII.GetBytes("0123"),
                Encoding.ASCII.GetBytes("01234"),
                Encoding.ASCII.GetBytes("012344"),
                Encoding.ASCII.GetBytes("0123444"),
                Encoding.ASCII.GetBytes("01234444"),
                Encoding.ASCII.GetBytes("012344444"),
                Encoding.ASCII.GetBytes("0123444444"),
            };
            Assert.AreEqual(expected.Length, mutations.Count);
            for (int i = 0; i < expected.Length; ++i)
            {
                var item = mutations[i];
                Assert.AreEqual(Variant.VariantType.BitStream, item.GetVariantType());
                Assert.AreEqual(expected[i], (byte[])item);
            }
        }

        [Test]
        public void Test3()
        {
            // Test that random mutations honor the correct +/- N
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <String name=\"a0\" value=\"0\" maxOccurs=\"100\">" +
                "           <Hint name=\"ArrayVarianceMutator-N\" value=\"5\"/>" +
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
                "       <Strategy class=\"RandomStrategy\"/>" +
                "   </Test>" +
                "</Peach>";

            PitParser parser = new PitParser();

            Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
            dom.tests[0].includedMutators = new List<string>();
            dom.tests[0].includedMutators.Add("ArrayVarianceMutator");

            var myArray = (Dom.Array)dom.tests[0].stateModel.initialState.actions[0].dataModel[0];
            myArray.origionalElement = myArray[0];
            myArray.hasExpanded = true;
            myArray.Add(new Dom.String("a1") { DefaultValue = new Variant("1") });
            myArray.Add(new Dom.String("a2") { DefaultValue = new Variant("2") });
            myArray.Add(new Dom.String("a3") { DefaultValue = new Variant("3") });
            myArray.Add(new Dom.String("a4") { DefaultValue = new Variant("4") });

            RunConfiguration config = new RunConfiguration();
            config.range = true;
            config.rangeStart = 0;
            config.rangeStop = 999;

            Engine e = new Engine(null);
            e.startFuzzing(dom, config);

            // verify values
            Assert.AreEqual(999, mutations.Count);
            int min = int.MaxValue;
            int max = int.MinValue;
            for (int i = 0; i < 999; ++i)
            {
                var item = (byte[])mutations[i];
                if (item.Length == 0)
                    continue;

                if (item.Length > max)
                    max = item.Length;
                if (item.Length < min)
                    min = item.Length;
            }
            Assert.AreEqual(1, min);
            Assert.AreEqual(10, max);
        }

		[Test]
		public void TestBlock()
		{
			// standard test -- change the length of the array to count - N to count + N (N = 5)
			// 01234 -> [0, 01, 012, 0123, 01234, 012344, 0123444, 01234444, 012344444, 0123444444]
			// however, the data element in the array is a block

			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
				"<Peach>" +
				"   <DataModel name=\"TheDataModel\">" +
				"       <Block name=\"a0\" maxOccurs=\"100\">" +
				"           <Hint name=\"ArrayVarianceMutator-N\" value=\"5\"/>" +
				"           <String name=\"str\" value=\"0\"/>" +
				"       </Block>" +
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
			dom.tests[0].includedMutators.Add("ArrayVarianceMutator");

			RunConfiguration config = new RunConfiguration();

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			// verify values
			byte[][] expected = new byte[][] {
                new byte[0],
                Encoding.ASCII.GetBytes("0"),
                Encoding.ASCII.GetBytes("00"),
                Encoding.ASCII.GetBytes("000"),
                Encoding.ASCII.GetBytes("0000"),
                Encoding.ASCII.GetBytes("00000"),
                Encoding.ASCII.GetBytes("000000"),
            };
			Assert.AreEqual(expected.Length, mutations.Count);
			for (int i = 0; i < expected.Length; ++i)
			{
				var item = mutations[i];
				Assert.AreEqual(Variant.VariantType.BitStream, item.GetVariantType());
				Assert.AreEqual(expected[i], (byte[])item);
			}
		}

    }
}

// end
