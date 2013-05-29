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
		public void TestRefDataModel()
		{
			// standard test

			string xml = @"
<Peach>
	<DataModel name='Base'>
		<Block name='Header' mutable='false'>
			<Number name='id' size='8' signed='false' mutable='false'>
				<Fixup class='SequenceIncrementFixup'/>
			</Number>
		</Block>
	</DataModel>

	<DataModel name='TheDataModel' ref='Base'>
		<Block name='Payload'>
			<String name='value' value='Hello World'/>
		</Block>
	</DataModel>

	<StateModel name='TheState' initialState='Initial'>
		<State name='Initial'>
			<Action type='output'>
				<DataModel ref='TheDataModel'/>
			</Action>
		</State>
	</StateModel>

	<Test name='Default'>
		<StateModel ref='TheState'/>
		<Publisher class='Null'/>
	</Test>
</Peach>";

			PitParser parser = new PitParser();

			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.range = true;
			config.rangeStart = 0;
			config.rangeStop = 10;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			// verify values
			Assert.AreEqual(11, dataModels.Count);

			for (int i = 0; i < dataModels.Count; ++i)
			{
				var bytes = dataModels[i].Value.Value;
				Assert.GreaterOrEqual(bytes.Length, 1);
				Assert.AreEqual(i + 1, bytes[0]);
			}
		}

		[Test]
		public void TestIncrement()
		{
			// Should only increment on output actions

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
			// Should only increment on output actions, count
			// is always increasing even if value is set via slurp
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
				<DataModel name='in' ref='DM'/>
			</Action>

			<Action type='slurp' valueXpath='//in/num' setXpath='//DM/num'/>

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
			Assert.AreEqual(0, (int)this.actions[0].dataModel[0].InternalValue);

			Assert.AreEqual(12, (int)this.actions[1].dataModel[0].DefaultValue);
			Assert.AreEqual(1, (int)this.actions[1].dataModel[0].InternalValue);

			Assert.AreEqual(12, (int)this.actions[2].dataModel[0].DefaultValue);
			Assert.AreEqual(2, (int)this.actions[2].dataModel[0].InternalValue);
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
			Assert.AreEqual(1, (int)this.actions[0].dataModel[0].InternalValue);

			Assert.AreEqual(0, (int)this.actions[1].dataModel[0].DefaultValue);
			Assert.AreEqual(2, (int)this.actions[1].dataModel[0].InternalValue);
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
			Assert.AreEqual("1", (string)this.actions[0].dataModel[0].InternalValue);

			Assert.AreEqual("100", (string)this.actions[1].dataModel[0].DefaultValue);
			Assert.AreEqual("2", (string)this.actions[1].dataModel[0].InternalValue);

			Assert.AreEqual("100", (string)this.actions[2].dataModel[0].DefaultValue);
			Assert.AreEqual("3", (string)this.actions[2].dataModel[0].InternalValue);
		}

		[Test]
		public void FuzzSame()
		{
			string xml = @"
<Peach>
	<DataModel name='DM'>
		<Number name='str' size='16' mutable='false'>
			<Fixup class='SequenceIncrementFixup'>
				<Param name='Offset' value='0'/>
			</Fixup>
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

			for (int i = 0; i < 12; i += 3)
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
		public void FuzzOffsetOne()
		{
			string xml = @"
<Peach>
	<DataModel name='DM'>
		<Number name='str' size='16' mutable='false'>
			<Fixup class='SequenceIncrementFixup'>
				<Param name='Offset' value='1'/>
			</Fixup>
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

			for (int i = 0; i < 12; i += 3)
			{
				// Should be:
				// 1, 2, 3
				// 1, 2, 3
				// 2, 3, 4
				// 3, 4, 5

				int exp = Math.Max(0, i - 3);
				int start = 1 * (exp / 3);

				Assert.AreEqual(0, (int)dataModels[i + 0][0].DefaultValue);
				Assert.AreEqual(start + 1, (int)dataModels[i + 0][0].InternalValue);

				Assert.AreEqual(0, (int)dataModels[i + 1][0].DefaultValue);
				Assert.AreEqual(start + 2, (int)dataModels[i + 1][0].InternalValue);

				Assert.AreEqual(0, (int)dataModels[i + 2][0].DefaultValue);
				Assert.AreEqual(start + 3, (int)dataModels[i + 2][0].InternalValue);
			}
		}

		[Test]
		public void FuzzOffsetFive()
		{
			string xml = @"
<Peach>
	<DataModel name='DM'>
		<Number name='str' size='16' mutable='false'>
			<Fixup class='SequenceIncrementFixup'>
				<Param name='Offset' value='5'/>
			</Fixup>
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

			for (int i = 0; i < 12; i += 3)
			{
				// Should be:
				// 1, 2, 3
				// 1, 2, 3
				// 6, 7, 8
				// 11, 12, 13

				int exp = Math.Max(0, i - 3);
				int start = 5 * (exp / 3);

				Assert.AreEqual(0, (int)dataModels[i + 0][0].DefaultValue);
				Assert.AreEqual(start + 1, (int)dataModels[i + 0][0].InternalValue);

				Assert.AreEqual(0, (int)dataModels[i + 1][0].DefaultValue);
				Assert.AreEqual(start + 2, (int)dataModels[i + 1][0].InternalValue);

				Assert.AreEqual(0, (int)dataModels[i + 2][0].DefaultValue);
				Assert.AreEqual(start + 3, (int)dataModels[i + 2][0].InternalValue);
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

		[Test]
		public void CrossIteration()
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
			config.rangeStart = 1;
			config.rangeStop = 20;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			Assert.AreEqual(21 * 3, this.dataModels.Count);

			Assert.AreEqual(0, (int)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual(0, (int)dom.dataModels[0][0].InternalValue);

			for (int i = 0; i < 63; ++i)
			{
				Assert.AreEqual(0, (int)dataModels[i][0].DefaultValue);
				Assert.AreEqual(i + 1, (int)dataModels[i][0].InternalValue);
			}
		}

		[Test]
		public void CrossIterationOnce()
		{
			string xml = @"
<Peach>
	<DataModel name='DM'>
		<Number name='str' size='16' mutable='false'>
			<Fixup class='SequenceIncrementFixup'>
				<Param name='Once' value='true'/>
			</Fixup>
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
			config.rangeStart = 1;
			config.rangeStop = 20;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			Assert.AreEqual(21 * 3, this.dataModels.Count);

			Assert.AreEqual(0, (int)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual(0, (int)dom.dataModels[0][0].InternalValue);

			for (int i = 0; i < 21; ++i)
			{
				Assert.AreEqual(0, (int)dataModels[(3 * i) + 0][0].DefaultValue);
				Assert.AreEqual(i + 1, (int)dataModels[(3 * i) + 0][0].InternalValue);
				Assert.AreEqual(0, (int)dataModels[(3 * i) + 1][0].DefaultValue);
				Assert.AreEqual(i + 1, (int)dataModels[(3 * i) + 1][0].InternalValue);
				Assert.AreEqual(0, (int)dataModels[(3 * i) + 2][0].DefaultValue);
				Assert.AreEqual(i + 1, (int)dataModels[(3 * i) + 2][0].InternalValue);
			}
		}

		[Test]
		public void ReplayNoOffset()
		{
			// When the Offset parameter is not used, the fixup will continue to
			// increment even when the engine replays iterations
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

	<Agent name='LocalAgent'>
		<Monitor class='FaultingMonitor'>
			<Param name='Iteration' value='2'/>
		</Monitor>
	</Agent>

	<Test name='Default' faultWaitTime='0' replayEnabled='true'>
		<Agent ref='LocalAgent'/>
		<StateModel ref='TheState'/>
		<Publisher class='Null'/>
	</Test>
</Peach>
";
			cloneActions = true;
			iterationHistory = new List<uint>();

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.range = true;
			config.rangeStart = 1;
			config.rangeStop = 3;

			Engine e = new Engine(null);
			e.IterationStarting += IterationStarting;
			e.startFuzzing(dom, config);

			Assert.AreEqual(new uint[] { 1, 1, 2, 2, 3 }, iterationHistory.ToArray());

			Assert.AreEqual(15, actions.Count);

			for (int i = 0; i < 15; ++i)
			{
				Assert.AreEqual(i + 1, (int)actions[i].dataModel[0].InternalValue);
			}
		}

		[Test]
		public void ReplayOffset()
		{
			// When the Offset parameter is used, the fixup will produce the same
			// values when the engine replays iterations
			string xml = @"
<Peach>
	<DataModel name='DM'>
		<Number name='str' size='16' mutable='false'>
			<Fixup class='SequenceIncrementFixup'>
				<Param name='Offset' value='3'/>
			</Fixup>
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

	<Agent name='LocalAgent'>
		<Monitor class='FaultingMonitor'>
			<Param name='Iteration' value='2'/>
		</Monitor>
	</Agent>

	<Test name='Default' faultWaitTime='0' replayEnabled='true'>
		<Agent ref='LocalAgent'/>
		<StateModel ref='TheState'/>
		<Publisher class='Null'/>
	</Test>
</Peach>
";
			cloneActions = true;
			iterationHistory = new List<uint>();

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.range = true;
			config.rangeStart = 1;
			config.rangeStop = 3;

			Engine e = new Engine(null);
			e.IterationStarting += IterationStarting;
			e.startFuzzing(dom, config);

			Assert.AreEqual(new uint[] { 1, 1, 2, 2, 3 }, iterationHistory.ToArray());


			var expected = new int[] { 1, 2, 3, 1, 2, 3, 4, 5, 6, 4, 5, 6, 7, 8, 9 };
			Assert.AreEqual(expected.Length, actions.Count);

			for (int i = 0; i < expected.Length; ++i)
			{
				Assert.AreEqual(expected[i], (int)actions[i].dataModel[0].InternalValue);
			}
		}

		List<uint> iterationHistory = new List<uint>();

		void IterationStarting(RunContext context, uint currentIteration, uint? totalIterations)
		{
			iterationHistory.Add(currentIteration);
		}

		[Test]
		public void SizedParent()
		{
			// Make sure increment loops when it would overflow the size of the parent

			string xml = @"
<Peach>
	<DataModel name='DM'>
		<Number name='str' size='2' mutable='false'>
			<Fixup class='SequenceIncrementFixup'>
				<Param name='Offset' value='3'/>
			</Fixup>
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
			cloneActions = true;

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.range = true;
			config.rangeStart = 0;
			config.rangeStop = 10;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			Assert.AreEqual(33, this.dataModels.Count);

			Assert.AreEqual(0, (int)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual(0, (int)dom.dataModels[0][0].InternalValue);

			for (int i = 0; i < 33; i += 3)
			{
				Assert.AreEqual(0, (int)dataModels[i + 0][0].DefaultValue);
				Assert.AreEqual(1, (int)dataModels[i + 0][0].InternalValue);

				Assert.AreEqual(0, (int)dataModels[i + 1][0].DefaultValue);
				Assert.AreEqual(2, (int)dataModels[i + 1][0].InternalValue);

				Assert.AreEqual(0, (int)dataModels[i + 2][0].DefaultValue);
				Assert.AreEqual(3, (int)dataModels[i + 2][0].InternalValue);
			}

			ResetContainers();
			cloneActions = true;

			parser = new PitParser();
			dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			config = new RunConfiguration();
			config.range = true;
			config.rangeStart = 3;
			config.rangeStop = 12;

			e = new Engine(null);
			e.startFuzzing(dom, config);

			Assert.AreEqual(33, this.dataModels.Count);

			Assert.AreEqual(0, (int)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual(0, (int)dom.dataModels[0][0].InternalValue);

			for (int i = 0; i < 33; i += 3)
			{
				Assert.AreEqual(0, (int)dataModels[i + 0][0].DefaultValue);
				Assert.AreEqual(1, (int)dataModels[i + 0][0].InternalValue);

				Assert.AreEqual(0, (int)dataModels[i + 1][0].DefaultValue);
				Assert.AreEqual(2, (int)dataModels[i + 1][0].InternalValue);

				Assert.AreEqual(0, (int)dataModels[i + 2][0].DefaultValue);
				Assert.AreEqual(3, (int)dataModels[i + 2][0].InternalValue);
			}
		}
	}
}

// end
