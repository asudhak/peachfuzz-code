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
    class DataElementSwapNearNodesMutatorTests : DataModelCollector
    {
        [Test]
        public void Test1()
        {
            // standard test of swapping the data elements

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Number name=\"num0\" size=\"32\" signed=\"true\" value=\"41\"/>" +
                "       <Number name=\"num1\" size=\"32\" signed=\"true\" value=\"42\"/>" +
                "       <Number name=\"num2\" size=\"32\" signed=\"true\" value=\"43\"/>" +
                "       <Number name=\"num3\" size=\"32\" signed=\"true\" value=\"44\"/>" +
                "       <Number name=\"num4\" size=\"32\" signed=\"true\" value=\"45\"/>" +
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
            dom.tests[0].includedMutators.Add("DataElementSwapNearNodesMutator");

            RunConfiguration config = new RunConfiguration();

            Engine e = new Engine(null);
            e.startFuzzing(dom, config);

            // verify values
            Assert.AreEqual(6, dataModels.Count);

            Assert.AreEqual(5, dataModels[0].Count);
            Assert.AreEqual("num0", dataModels[0][0].name);
            Assert.AreEqual("num1", dataModels[0][1].name);
            Assert.AreEqual("num2", dataModels[0][2].name);
            Assert.AreEqual("num3", dataModels[0][3].name);
            Assert.AreEqual("num4", dataModels[0][4].name);

            Assert.AreEqual(5, dataModels[1].Count);
            Assert.AreEqual("num1", dataModels[1][0].name);
            Assert.AreEqual("num0", dataModels[1][1].name);
            Assert.AreEqual("num2", dataModels[1][2].name);
            Assert.AreEqual("num3", dataModels[1][3].name);
            Assert.AreEqual("num4", dataModels[1][4].name);

            Assert.AreEqual(5, dataModels[2].Count);
            Assert.AreEqual("num0", dataModels[2][0].name);
            Assert.AreEqual("num2", dataModels[2][1].name);
            Assert.AreEqual("num1", dataModels[2][2].name);
            Assert.AreEqual("num3", dataModels[2][3].name);
            Assert.AreEqual("num4", dataModels[2][4].name);

            Assert.AreEqual(5, dataModels[3].Count);
            Assert.AreEqual("num0", dataModels[3][0].name);
            Assert.AreEqual("num1", dataModels[3][1].name);
            Assert.AreEqual("num3", dataModels[3][2].name);
            Assert.AreEqual("num2", dataModels[3][3].name);
            Assert.AreEqual("num4", dataModels[3][4].name);

            Assert.AreEqual(5, dataModels[4].Count);
            Assert.AreEqual("num0", dataModels[4][0].name);
            Assert.AreEqual("num1", dataModels[4][1].name);
            Assert.AreEqual("num2", dataModels[4][2].name);
            Assert.AreEqual("num4", dataModels[4][3].name);
            Assert.AreEqual("num3", dataModels[4][4].name);

            Assert.AreEqual(5, dataModels[5].Count);
            Assert.AreEqual("num0", dataModels[5][0].name);
            Assert.AreEqual("num1", dataModels[5][1].name);
            Assert.AreEqual("num2", dataModels[5][2].name);
            Assert.AreEqual("num3", dataModels[5][3].name);
            Assert.AreEqual("num4", dataModels[5][4].name);
        }

        [Test]
        public void TestBlock()
        {
            // standard test of swapping the data elements

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "     <Block name=\"blk\">" +
                "       <Number name=\"num0\" size=\"32\" signed=\"true\" value=\"41\"/>" +
                "       <Number name=\"num1\" size=\"32\" signed=\"true\" value=\"42\"/>" +
                "       <Number name=\"num2\" size=\"32\" signed=\"true\" value=\"43\"/>" +
                "       <Number name=\"num3\" size=\"32\" signed=\"true\" value=\"44\"/>" +
                "       <Number name=\"num4\" size=\"32\" signed=\"true\" value=\"45\"/>" +
                "     </Block>" +
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
            dom.tests[0].includedMutators.Add("DataElementSwapNearNodesMutator");

            RunConfiguration config = new RunConfiguration();

            Engine e = new Engine(null);
            e.startFuzzing(dom, config);

            DataElementContainer blk = null;

            Assert.AreEqual(7, dataModels.Count);

            Assert.AreEqual(1, dataModels[0].Count);
            blk = dataModels[0][0] as DataElementContainer;
            Assert.NotNull(blk);
            Assert.AreEqual("blk", blk.name);
            Assert.AreEqual(5, blk.Count);
            Assert.AreEqual("num0", blk[0].name);
            Assert.AreEqual("num1", blk[1].name);
            Assert.AreEqual("num2", blk[2].name);
            Assert.AreEqual("num3", blk[3].name);
            Assert.AreEqual("num4", blk[4].name);

            Assert.AreEqual(1, dataModels[1].Count);
            blk = dataModels[1][0] as DataElementContainer;
            Assert.NotNull(blk);
            Assert.AreEqual("blk", blk.name);
            Assert.AreEqual(5, blk.Count);
            Assert.AreEqual("num0", blk[0].name);
            Assert.AreEqual("num1", blk[1].name);
            Assert.AreEqual("num2", blk[2].name);
            Assert.AreEqual("num3", blk[3].name);
            Assert.AreEqual("num4", blk[4].name);

            Assert.AreEqual(1, dataModels[2].Count);
            blk = dataModels[2][0] as DataElementContainer;
            Assert.NotNull(blk);
            Assert.AreEqual("blk", blk.name);
            Assert.AreEqual(5, blk.Count);
            Assert.AreEqual("num1", blk[0].name);
            Assert.AreEqual("num0", blk[1].name);
            Assert.AreEqual("num2", blk[2].name);
            Assert.AreEqual("num3", blk[3].name);
            Assert.AreEqual("num4", blk[4].name);

            Assert.AreEqual(1, dataModels[3].Count);
            blk = dataModels[3][0] as DataElementContainer;
            Assert.NotNull(blk);
            Assert.AreEqual("blk", blk.name);
            Assert.AreEqual(5, blk.Count);
            Assert.AreEqual("num0", blk[0].name);
            Assert.AreEqual("num2", blk[1].name);
            Assert.AreEqual("num1", blk[2].name);
            Assert.AreEqual("num3", blk[3].name);
            Assert.AreEqual("num4", blk[4].name);

            Assert.AreEqual(1, dataModels[4].Count);
            blk = dataModels[4][0] as DataElementContainer;
            Assert.NotNull(blk);
            Assert.AreEqual("blk", blk.name);
            Assert.AreEqual(5, blk.Count);
            Assert.AreEqual("num0", blk[0].name);
            Assert.AreEqual("num1", blk[1].name);
            Assert.AreEqual("num3", blk[2].name);
            Assert.AreEqual("num2", blk[3].name);
            Assert.AreEqual("num4", blk[4].name);

            Assert.AreEqual(1, dataModels[5].Count);
            blk = dataModels[5][0] as DataElementContainer;
            Assert.NotNull(blk);
            Assert.AreEqual("blk", blk.name);
            Assert.AreEqual(5, blk.Count);
            Assert.AreEqual("num0", blk[0].name);
            Assert.AreEqual("num1", blk[1].name);
            Assert.AreEqual("num2", blk[2].name);
            Assert.AreEqual("num4", blk[3].name);
            Assert.AreEqual("num3", blk[4].name);

            Assert.AreEqual(1, dataModels[6].Count);
            blk = dataModels[6][0] as DataElementContainer;
            Assert.NotNull(blk);
            Assert.AreEqual("blk", blk.name);
            Assert.AreEqual(5, blk.Count);
            Assert.AreEqual("num0", blk[0].name);
            Assert.AreEqual("num1", blk[1].name);
            Assert.AreEqual("num2", blk[2].name);
            Assert.AreEqual("num3", blk[3].name);
            Assert.AreEqual("num4", blk[4].name);

        }

		[Test]
		public void NoSwapFlag()
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
			dom.tests[0].includedMutators.Add("DataElementSwapNearNodesMutator");

			RunConfiguration config = new RunConfiguration();

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			Assert.AreEqual(2, strategies.Count);
			Assert.AreEqual("DataElementSwapNearNodesMutator | TheModel.flags", strategies[0]);
			Assert.AreEqual("DataElementSwapNearNodesMutator | TheModel.str", strategies[1]);
		}

    }
}

// end
