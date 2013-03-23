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
    class DataElementDuplicateMutatorTests : DataModelCollector
    {
        [Test]
        public void Test1()
        {
            // standard test of duplicating elements from the data model (1x - 50x)

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Number name=\"num0\" size=\"16\" signed=\"false\"/>" +
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
            dom.tests[0].includedMutators.Add("DataElementDuplicateMutator");

            RunConfiguration config = new RunConfiguration();

            Engine e = new Engine(null);
            e.startFuzzing(dom, config);

            // verify values
            Assert.AreEqual(50, dataModels.Count);
            for (int i = 0; i < 50; ++i)
                Assert.AreEqual(i + 1, dataModels[i].Count);
        }


        [Test]
        public void Test3()
        {
            // Test that random mutations stay within the correct bounds of 1x - 50x

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Number name=\"num0\" size=\"16\" signed=\"false\"/>" +
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
            dom.tests[0].includedMutators.Add("DataElementDuplicateMutator");

            RunConfiguration config = new RunConfiguration();
            config.range = true;
            config.rangeStart = 0;
            config.rangeStop = 1000;
            config.randomSeed = 100;

            Engine e = new Engine(null);
            e.startFuzzing(dom, config);

            // verify values
            // 1000 mutations, switch every 200 iterations
            Assert.AreEqual(1005, dataModels.Count);
            Assert.AreEqual(1000, mutatedDataModels.Count);

            // No mutation on the 1st control iteration
            Assert.AreEqual(1, dataModels[0].Count);

            int min = int.MaxValue;
            int max = int.MinValue;

            for (int i = 0; i < mutatedDataModels.Count; ++i)
            {
                if (dataModels[i].Count > max)
                    max = dataModels[i].Count;
                if (dataModels[i].Count < min)
                    min = dataModels[i].Count;
            }

            // Either duplicates or it doesn't.  This is what Peach 2.3 does, but is it right?
            Assert.AreEqual(1, min);
            Assert.AreEqual(2, max);
        }

		[Test]
		public void TypeTransform()
		{
			string xml = @"
<Peach>
	<DataModel name=""TheModel"">
		<Number name=""num"" size=""16"">
			<Relation type=""size"" of=""str""/>
		</Number>
		<String name=""str"" value=""Hello"" />
	</DataModel>

	<StateModel name=""TheState"" initialState=""Initial"">
		<State name=""Initial"">
			<Action type=""output"">
				<DataModel ref=""TheModel""/>
			</Action>
		</State>
	</StateModel>

	<Test name=""Default"">
		<StateModel ref=""TheState""/>
		<Publisher class=""Null""/>
		<Strategy class=""Random""/>
	</Test>
</Peach>
";
			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			dom.tests[0].includedMutators = new List<string>();
			dom.tests[0].includedMutators.Add("SizedNumericalEdgeCasesMutator");
			dom.tests[0].includedMutators.Add("DataElementDuplicateMutator");

			RunConfiguration config = new RunConfiguration();
			config.range = true;
			config.randomSeed = 1;
			config.rangeStart = 0;
			config.rangeStop = 2;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			Assert.AreEqual(3, iterStrategies.Count);

			// Looking for SizedNumerical on len and DataElementDuplicate on str
			Assert.AreEqual("SizedNumericalEdgeCasesMutator | TheModel.num ; DataElementDuplicateMutator | TheModel.str", iterStrategies[2]);
		}

		[Test]
		public void DuplicateFixup()
		{
			string xml = @"
<Peach>
	<DataModel name=""TheModel"">
		<Number name=""num"" size=""16"">
			<Fixup class=""IcmpChecksumFixup"">
				<Param name=""ref"" value=""TheModel""/>
			</Fixup>
		</Number>
		<String name=""str"" value=""Hello"" />
	</DataModel>

	<StateModel name=""TheState"" initialState=""Initial"">
		<State name=""Initial"">
			<Action type=""output"">
				<DataModel ref=""TheModel""/>
			</Action>
		</State>
	</StateModel>

	<Test name=""Default"">
		<StateModel ref=""TheState""/>
		<Publisher class=""Null""/>
		<Strategy class=""Sequential""/>
	</Test>
</Peach>
";
			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			dom.tests[0].includedMutators = new List<string>();
			dom.tests[0].includedMutators.Add("DataElementDuplicateMutator");

			RunConfiguration config = new RunConfiguration();

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			// 49 mutations of num and 49 mutations of str
			Assert.AreEqual(98, mutations.Count);
		}

		[Test]
		public void NoDuplicateFlag()
		{
			string xml = @"
<Peach>
	<DataModel name=""TheModel"">
		<Flags name=""flags"" size=""16"">
			<Flag name=""flag1"" position=""0"" size=""2""/>
			<Flag name=""flag2"" position=""4"" size=""2""/>
		</Flags>
		<String name=""str"" value=""Hello"" />
	</DataModel>

	<StateModel name=""TheState"" initialState=""Initial"">
		<State name=""Initial"">
			<Action type=""output"">
				<DataModel ref=""TheModel""/>
			</Action>
		</State>
	</StateModel>

	<Test name=""Default"">
		<StateModel ref=""TheState""/>
		<Publisher class=""Null""/>
		<Strategy class=""Sequential""/>
	</Test>
</Peach>
";
			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			dom.tests[0].includedMutators = new List<string>();
			dom.tests[0].includedMutators.Add("DataElementDuplicateMutator");

			RunConfiguration config = new RunConfiguration();

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			Assert.AreEqual(2, strategies.Count);
			Assert.AreEqual("DataElementDuplicateMutator | TheModel.flags", strategies[0]);
			Assert.AreEqual("DataElementDuplicateMutator | TheModel.str", strategies[1]);
		}
    }
}

// end
