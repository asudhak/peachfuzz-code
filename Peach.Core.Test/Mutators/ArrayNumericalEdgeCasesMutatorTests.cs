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
using Peach.Core.Publishers;
using NLog;

namespace Peach.Core.Test.Mutators
{
    [TestFixture]
    class ArrayNumericalEdgeCasesMutatorTests : DataModelCollector
    {
        [Test]
        public void Test1()
        {
            // standard test - will change the length of arrays to lengths of +/- 50 around numerical edge cases
            // -- edge cases are: 50, 127, 255, 32767, 65535

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
            dom.tests[0].includedMutators.Add("ArrayNumericalEdgeCasesMutator");

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
#if MONO
            Assert.AreEqual(303, mutations.Count);
#else
            Assert.AreEqual(505, mutations.Count);
#endif
            foreach (var item in mutations)
            {
                Assert.AreEqual(Variant.VariantType.BitStream, item.GetVariantType());
                Assert.NotNull((byte[])item);
            }
        }

        [Test]
        public void Test2()
        {
            // standard test - will change the length of arrays to lengths of +/- N around numerical edge cases
            // -- N = 5
            // -- edge cases are: 50, 127, 255, 32767, 65535

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <String name=\"a0\" value=\"0\" maxOccurs=\"100\">" +
                "           <Hint name=\"ArrayNumericalEdgeCasesMutator-N\" value=\"5\"/>" +
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
            dom.tests[0].includedMutators.Add("ArrayNumericalEdgeCasesMutator");

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
#if MONO
            Assert.AreEqual(33, mutations.Count);
#else
            Assert.AreEqual(55, mutations.Count);
#endif
            foreach (var item in mutations)
            {
                Assert.AreEqual(Variant.VariantType.BitStream, item.GetVariantType());
                Assert.NotNull((byte[])item);
            }
        }

		class FixedInputPublisher : StreamPublisher
		{
			private static NLog.Logger logger = LogManager.GetCurrentClassLogger();
			protected override NLog.Logger Logger { get { return logger; } }

			byte[] buffer = new byte[] { 0, 1, 2, 3 };

			public FixedInputPublisher()
				: base(new Dictionary<string, Variant>())
			{
				this.stream = new MemoryStream();
			}

			protected override void OnInput()
			{
				stream = new MemoryStream(buffer);
			}

			protected override void OnOutput(byte[] buffer, int offset, int count)
			{
			}
		}

		[Test]
		public void InputTest()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
				"<Peach>" +
				"   <DataModel name=\"TheDataModel\">" +
				"       <Number name=\"num\" size=\"8\" maxOccurs=\"4\"/>" +
				"   </DataModel>" +

				"   <StateModel name=\"TheState\" initialState=\"Initial\">" +
				"       <State name=\"Initial\">" +
				"           <Action type=\"input\">" +
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
			dom.tests[0].includedMutators.Add("ArrayNumericalEdgeCasesMutator");
			dom.tests[0].publishers[0] = new FixedInputPublisher();

			RunConfiguration config = new RunConfiguration();

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

		}

		[Test]
		public void CountOverflow()
		{
			string xml = @"
<Peach>
	<DataModel name=""DM"">
		<Number name=""num"" size=""4""/>
		<Number name=""count"" size=""4"">
			<Relation type=""count"" of=""array""/>
		</Number>
		<String name=""array"" value=""1"" maxOccurs=""100""/>
	</DataModel>

	<StateModel name=""TheState"" initialState=""Initial"">
		<State name=""Initial"">
			<Action type=""output"">
				<DataModel ref=""DM""/>
			</Action>
		</State>
	</StateModel>

	<Test name=""Default"">
		<StateModel ref=""TheState""/>
		<Publisher class=""Null""/>
		<Strategy class=""Sequential""/>
	</Test>
</Peach>";

			PitParser parser = new PitParser();

			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			dom.tests[0].includedMutators = new List<string>();
			dom.tests[0].includedMutators.Add("ArrayNumericalEdgeCasesMutator");
			dom.tests[0].publishers[0] = new FixedInputPublisher();

			RunConfiguration config = new RunConfiguration();

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			Assert.NotNull(mutations);

		}

    }
}

// end
