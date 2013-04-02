using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Analyzers;

namespace Peach.Core.Test.Fixups
{
    [TestFixture]
    class SequenceIncrementFixupTests : DataModelCollector
    {
        [Test]
        public void Test1()
        {
            // standard test

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Number name=\"num1\" size=\"32\" signed=\"false\">" +
                "           <Fixup class=\"SequenceIncrementFixup\"/>" +
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
                "   </Test>" +
                "</Peach>";

            PitParser parser = new PitParser();

            Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

            RunConfiguration config = new RunConfiguration();
            config.singleIteration = true;

            Engine e = new Engine(null);
            e.startFuzzing(dom, config);

            // verify values
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual(1, BitConverter.ToUInt32(values[0].Value, 0));
        }

		[Test]
		public void TestIncrement()
		{
			string tempFile = Path.GetTempFileName();

			string xml = @"
<Peach>
	<DataModel name='DM'>
		<Number name='num' size='16'>
			<Fixup class='SequenceIncrementFixup'/>
		</Number>
	</DataModel>

	<StateModel name='TheState' initialState='Initial'>
		<State name='Initial'>
			<Action type='output' publisher='null'>
				<DataModel ref='DM'/>
			</Action>
			<Action type='input' publisher='file'>
				<DataModel ref='DM'/>
			</Action>
			<Action type='output' publisher='null'>
				<DataModel ref='DM'/>
			</Action>
			<Action type='changeState' ref='Second'/>
		</State>

		<State name='Second'>
			<Action name='src' type='input' publisher='file'>
				<DataModel ref='DM'/>
			</Action>
			<Action type='changeState' ref='Third'/>
		</State>

		<State name='Third'>
			<Action type='output' publisher='null'>
				<DataModel ref='DM'/>
			</Action>

			<Action type='output' publisher='null'>
				<DataModel ref='DM'/>
				<Data DataModel='DM'>
					<Field name='num' value='100' />
				</Data>
			</Action>

			<Action type='output' publisher='null'>
				<DataModel ref='DM'/>
			</Action>

			<Action type='slurp' valueXpath='//src/DM/num' setXpath='//dst/DM/num'/>

			<Action name='dst' type='output' publisher='null'>
				<DataModel ref='DM'/>
			</Action>
		</State>
	</StateModel>

	<Test name='Default'>
		<StateModel ref='TheState'/>
		<Publisher name='null' class='Null'/>
		<Publisher name='file' class='File'>
			<Param name='Overwrite' value='false'/>
			<Param name='FileName' value='{0}'/>
		</Publisher>
	</Test>
</Peach>
".Fmt(tempFile);

			File.WriteAllBytes(tempFile, new byte[] { 12, 0, 13, 0 });

			PitParser parser = new PitParser();

			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			Assert.AreEqual(8, this.actions.Count);

			Assert.AreEqual(0, (int)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual(0, (int)dom.dataModels[0][0].InternalValue);

			Assert.AreEqual(0, (int)this.actions[0].dataModel[0].DefaultValue);
			Assert.AreEqual(1, (int)this.actions[0].dataModel[0].InternalValue);

			Assert.AreEqual(12, (int)this.actions[1].dataModel[0].DefaultValue);
			Assert.AreEqual(1, (int)this.actions[1].dataModel[0].InternalValue);

			Assert.AreEqual(0, (int)this.actions[2].dataModel[0].DefaultValue);
			Assert.AreEqual(2, (int)this.actions[2].dataModel[0].InternalValue);

			Assert.AreEqual(13, (int)this.actions[3].dataModel[0].DefaultValue);
			Assert.AreEqual(2, (int)this.actions[3].dataModel[0].InternalValue);

			Assert.AreEqual(0, (int)this.actions[4].dataModel[0].DefaultValue);
			Assert.AreEqual(3, (int)this.actions[4].dataModel[0].InternalValue);

			Assert.AreEqual(100, (int)this.actions[5].dataModel[0].DefaultValue);
			Assert.AreEqual(4, (int)this.actions[5].dataModel[0].InternalValue);

			Assert.AreEqual(0, (int)this.actions[6].dataModel[0].DefaultValue);
			Assert.AreEqual(5, (int)this.actions[6].dataModel[0].InternalValue);

			Assert.AreEqual(13, (int)this.actions[7].dataModel[0].DefaultValue);
			Assert.AreEqual(6, (int)this.actions[7].dataModel[0].InternalValue);
		}

		[Test]
		public void TestIncrementInitialSlurp()
		{
			string tempFile = Path.GetTempFileName();

			string xml = @"
<Peach>
	<DataModel name='DM'>
		<Number name='num' size='16'>
			<Fixup class='SequenceIncrementFixup'/>
		</Number>
	</DataModel>

	<StateModel name='TheState' initialState='Initial'>
		<State name='Initial'>
			<Action name='src' type='input' publisher='file'>
				<DataModel ref='DM'/>
			</Action>

			<Action type='slurp' valueXpath='//src/DM/num' setXpath='//num'/>

			<Action type='output' publisher='null'>
				<DataModel ref='DM'/>
			</Action>
			<Action name='change' type='changeState' ref='Second'/>
		</State>

		<State name='Second'>
			<Action type='changeState' ref='Third'/>
		</State>

		<State name='Third'>
			<Action type='output' publisher='null'>
				<DataModel ref='DM'/>
			</Action>
		</State>
	</StateModel>

	<Test name='Default'>
		<StateModel ref='TheState'/>
		<Publisher name='null' class='Null'/>
		<Publisher name='file' class='File'>
			<Param name='Overwrite' value='false'/>
			<Param name='FileName' value='{0}'/>
		</Publisher>
	</Test>
</Peach>
".Fmt(tempFile);

			File.WriteAllBytes(tempFile, new byte[] { 12, 0 });

			PitParser parser = new PitParser();

			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			Assert.AreEqual(3, this.actions.Count);

			Assert.AreEqual(0, (int)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual(0, (int)dom.dataModels[0][0].InternalValue);

			Assert.AreEqual(12, (int)this.actions[0].dataModel[0].DefaultValue);
			Assert.AreEqual(12, (int)this.actions[0].dataModel[0].InternalValue);

			Assert.AreEqual(12, (int)this.actions[1].dataModel[0].DefaultValue);
			Assert.AreEqual(13, (int)this.actions[1].dataModel[0].InternalValue);

			Assert.AreEqual(12, (int)this.actions[2].dataModel[0].DefaultValue);
			Assert.AreEqual(14, (int)this.actions[2].dataModel[0].InternalValue);
		}

		[Test]
		public void TestIncrementInitialField()
		{
			string tempFile = Path.GetTempFileName();

			string xml = @"
<Peach>
	<DataModel name='DM'>
		<Number name='num' size='16'>
			<Fixup class='SequenceIncrementFixup'/>
		</Number>
	</DataModel>

	<StateModel name='TheState' initialState='Initial'>
		<State name='Initial'>
			<Action type='output' publisher='null'>
				<DataModel ref='DM'/>
				<Data DataModel='DM'>
					<Field name='num' value='100' />
				</Data>
			</Action>
			<Action type='changeState' ref='Second'/>
		</State>

		<State name='Second'>
			<Action type='changeState' ref='Third'/>
		</State>

		<State name='Third'>
			<Action type='output' publisher='null'>
				<DataModel ref='DM'/>
			</Action>
		</State>
	</StateModel>

	<Test name='Default'>
		<StateModel ref='TheState'/>
		<Publisher name='null' class='Null'/>
		<Publisher name='file' class='File'>
			<Param name='Overwrite' value='false'/>
			<Param name='FileName' value='{0}'/>
		</Publisher>
	</Test>
</Peach>
".Fmt(tempFile);

			File.WriteAllBytes(tempFile, new byte[] { 12, 0 });

			PitParser parser = new PitParser();

			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			Assert.AreEqual(2, this.actions.Count);

			Assert.AreEqual(0, (int)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual(0, (int)dom.dataModels[0][0].InternalValue);

			Assert.AreEqual(100, (int)this.actions[0].dataModel[0].DefaultValue);
			Assert.AreEqual(101, (int)this.actions[0].dataModel[0].InternalValue);

			Assert.AreEqual(0, (int)this.actions[1].dataModel[0].DefaultValue);
			Assert.AreEqual(102, (int)this.actions[1].dataModel[0].InternalValue);
		}

		[Test]
		public void BadParent()
		{
			string xml = @"
<Peach>
	<DataModel name='DM'>
		<Blob name='blob'>
			<Fixup class='SequenceIncrementFixup'/>
		</Blob>
	</DataModel>

	<StateModel name='TheState' initialState='Initial'>
		<State name='Initial'>
			<Action type='output'>
				<DataModel ref='DM'/>
			</Action>
		</State>
	</StateModel>

	<Test name='Default'>
		<StateModel ref='TheState'/>
		<Publisher class='Null'/>
	</Test>
</Peach>
";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			Assert.AreEqual(new byte[0], (byte[])dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual(new byte[0], (byte[])dom.dataModels[0][0].InternalValue);

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);

			try
			{
				e.startFuzzing(dom, config);
				Assert.Fail("Should throw");
			}
			catch (PeachException ex)
			{
				Assert.AreEqual("SequenceIncrementFixup has non numeric parent 'DM.blob'.", ex.Message);
			}
		}

		[Test]
		public void BadStringParent()
		{
			string xml = @"
<Peach>
	<DataModel name='DM'>
		<String name='str' value='Hello World'>
			<Fixup class='SequenceIncrementFixup'/>
		</String>
	</DataModel>

	<StateModel name='TheState' initialState='Initial'>
		<State name='Initial'>
			<Action type='output'>
				<DataModel ref='DM'/>
			</Action>
		</State>
	</StateModel>

	<Test name='Default'>
		<StateModel ref='TheState'/>
		<Publisher class='Null'/>
	</Test>
</Peach>
";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			Assert.AreEqual("Hello World", (string)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual("Hello World", (string)dom.dataModels[0][0].InternalValue);

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);

			try
			{
				e.startFuzzing(dom, config);
				Assert.Fail("Should throw");
			}
			catch (PeachException ex)
			{
				Assert.AreEqual("SequenceIncrementFixup has non numeric parent 'DM.str'.", ex.Message);
			}
		}

		[Test]
		public void NumericString()
		{
			string xml = @"
<Peach>
	<DataModel name='DM'>
		<String name='str' value='100'>
			<Fixup class='SequenceIncrementFixup'/>
		</String>
	</DataModel>

	<StateModel name='TheState' initialState='Initial'>
		<State name='Initial'>
			<Action type='output'>
				<DataModel ref='DM'/>
			</Action>
			<Action type='output'>
				<DataModel ref='DM'/>
			</Action>
			<Action type='output'>
				<DataModel ref='DM'/>
			</Action>
		</State>
	</StateModel>

	<Test name='Default'>
		<StateModel ref='TheState'/>
		<Publisher class='Null'/>
	</Test>
</Peach>
";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			Assert.AreEqual(3, this.actions.Count);

			Assert.AreEqual("100", (string)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual("100", (string)dom.dataModels[0][0].InternalValue);

			Assert.AreEqual("100", (string)this.actions[0].dataModel[0].DefaultValue);
			Assert.AreEqual("101", (string)this.actions[0].dataModel[0].InternalValue);

			Assert.AreEqual("100", (string)this.actions[1].dataModel[0].DefaultValue);
			Assert.AreEqual("102", (string)this.actions[1].dataModel[0].InternalValue);

			Assert.AreEqual("100", (string)this.actions[2].dataModel[0].DefaultValue);
			Assert.AreEqual("103", (string)this.actions[2].dataModel[0].InternalValue);
		}

		[Test]
		public void FuzzSame()
		{
			string xml = @"
<Peach>
	<DataModel name='DM'>
		<Number name='str' size='16' mutable='false'>
			<Fixup class='SequenceIncrementFixup'/>
		</Number>
		<String/>
	</DataModel>

	<StateModel name='TheState' initialState='Initial'>
		<State name='Initial'>
			<Action type='output'>
				<DataModel ref='DM'/>
			</Action>
			<Action type='output'>
				<DataModel ref='DM'/>
			</Action>
			<Action type='output'>
				<DataModel ref='DM'/>
			</Action>
		</State>
	</StateModel>

	<Test name='Default'>
		<StateModel ref='TheState'/>
		<Publisher class='Null'/>
	</Test>
</Peach>
";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.range = true;
			config.rangeStart = 0;
			config.rangeStop = 3;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			Assert.AreEqual((1 + 3) * 3, this.dataModels.Count);

			Assert.AreEqual(0, (int)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual(0, (int)dom.dataModels[0][0].InternalValue);

			for (int i = 0; i < 12; i+=3)
			{
				Assert.AreEqual(0, (int)dataModels[i + 0][0].DefaultValue);
				Assert.AreEqual(1, (int)dataModels[i + 0][0].InternalValue);

				Assert.AreEqual(0, (int)dataModels[i + 1][0].DefaultValue);
				Assert.AreEqual(2, (int)dataModels[i + 1][0].InternalValue);

				Assert.AreEqual(0, (int)dataModels[i + 2][0].DefaultValue);
				Assert.AreEqual(3, (int)dataModels[i + 2][0].InternalValue);
			}
		}

		[Test]
		public void TwoDataModels()
		{
			string xml = @"
<Peach>
	<DataModel name='DM'>
		<Number name='str' size='16' mutable='false'>
			<Fixup class='SequenceIncrementFixup'/>
		</Number>
	</DataModel>

	<DataModel name='DM2'>
		<Number name='str' size='32' mutable='false'>
			<Fixup class='SequenceIncrementFixup'/>
		</Number>
	</DataModel>

	<StateModel name='TheState' initialState='Initial'>
		<State name='Initial'>
			<Action type='output'>
				<DataModel ref='DM'/>
			</Action>
			<Action type='output'>
				<DataModel ref='DM'/>
			</Action>
			<Action type='output'>
				<DataModel ref='DM'/>
			</Action>
		</State>
	</StateModel>

	<Test name='Default'>
		<StateModel ref='TheState'/>
		<Publisher class='Null'/>
	</Test>
</Peach>
";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			Assert.AreEqual(3, this.dataModels.Count);

			Assert.AreEqual(0, (int)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual(0, (int)dom.dataModels[0][0].InternalValue);

			Assert.AreEqual(0, (int)dataModels[0][0].DefaultValue);
			Assert.AreEqual(1, (int)dataModels[0][0].InternalValue);

			Assert.AreEqual(0, (int)dataModels[1][0].DefaultValue);
			Assert.AreEqual(2, (int)dataModels[1][0].InternalValue);

			Assert.AreEqual(0, (int)dataModels[2][0].DefaultValue);
			Assert.AreEqual(3, (int)dataModels[2][0].InternalValue);
		}
    }
}

// end
