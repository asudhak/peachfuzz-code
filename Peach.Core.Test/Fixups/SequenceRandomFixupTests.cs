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
    class SequenceRandomFixupTests : DataModelCollector
    {
        [Test]
        public void Test1()
        {
            // standard test

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Number name=\"num1\" size=\"32\" signed=\"false\">" +
                "           <Fixup class=\"SequenceRandomFixup\"/>" +
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
            uint val = BitConverter.ToUInt32(values[0].Value, 0);
            Assert.GreaterOrEqual(val, UInt32.MinValue);
            Assert.LessOrEqual(val, UInt32.MaxValue);
        }

		static string pit = @"
<Peach>
	<DataModel name='DM'>
		<Number name='str' size='16' mutable='false'>
			<Fixup class='SequenceRandomFixup'/>
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
		<Strategy class='Random'/>
	</Test>
</Peach>
";

		[Test]
		public void TestFuzz()
		{
			// Random numbers are produced for every action
			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(pit)));

			RunConfiguration config = new RunConfiguration();
			config.range = true;
			config.rangeStart = 0;
			config.rangeStop = 3;
			config.randomSeed = 1;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			Assert.AreEqual((1 + 3) * 3, this.dataModels.Count);

			Assert.AreEqual(0, (int)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual(0, (int)dom.dataModels[0][0].InternalValue);

			HashSet<int> numbers = new HashSet<int>();
			Assert.True(numbers.Add(0));

			Assert.AreEqual(0, (int)dataModels[0][0].DefaultValue);
			Assert.True(numbers.Add((int)dataModels[0][0].InternalValue));

			Assert.AreEqual(0, (int)dataModels[1][0].DefaultValue);
			Assert.True(numbers.Add((int)dataModels[1][0].InternalValue));

			Assert.AreEqual(0, (int)dataModels[2][0].DefaultValue);
			Assert.True(numbers.Add((int)dataModels[2][0].InternalValue));

			// Fuzz run 1 should be the same as the control iteration
			Assert.AreEqual(0, (int)dataModels[3][0].DefaultValue);
			Assert.AreEqual((int)dataModels[0][0].InternalValue, (int)dataModels[3][0].InternalValue);

			Assert.AreEqual(0, (int)dataModels[4][0].DefaultValue);
			Assert.AreEqual((int)dataModels[1][0].InternalValue, (int)dataModels[4][0].InternalValue);

			Assert.AreEqual(0, (int)dataModels[5][0].DefaultValue);
			Assert.AreEqual((int)dataModels[2][0].InternalValue, (int)dataModels[5][0].InternalValue);

			// Other fuzz runs should not produce the same numbers
			for (int i = 6; i < 12; i++)
			{
				Assert.AreEqual(0, (int)dataModels[i][0].DefaultValue);
				Assert.True(numbers.Add((int)dataModels[i][0].InternalValue));
			}
		}

		[Test]
		public void TestSeed()
		{
			// Ensure numbers change based on seed
			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(pit)));

			RunConfiguration config = new RunConfiguration();
			config.randomSeed = 1;
			config.range = true;
			config.rangeStart = 0;
			config.rangeStop = 10;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			Assert.AreEqual((1 + 10) * 3, this.dataModels.Count);

			var origModels = this.dataModels;

			ResetContainers();

			dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(pit)));

			config = new RunConfiguration();
			config.randomSeed = 2;
			config.range = true;
			config.rangeStart = 0;
			config.rangeStop = 10;

			e = new Engine(null);
			e.startFuzzing(dom, config);

			Assert.AreEqual(origModels.Count, dataModels.Count);

			for (int i = 0; i < dataModels.Count; ++i)
			{
				int oldVal = (int)origModels[i][0].InternalValue;
				int newVal = (int)dataModels[i][0].InternalValue;
				Assert.AreNotEqual(oldVal, newVal);
			}
		}

		[Test]
		public void TestReRun()
		{
			// Ensure rerun produces same value
			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(pit)));

			RunConfiguration config = new RunConfiguration();
			config.randomSeed = 1;
			config.range = true;
			config.rangeStart = 0;
			config.rangeStop = 20;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			Assert.AreEqual((1 + 20) * 3, this.dataModels.Count);

			var origModels = this.dataModels;

			ResetContainers();

			dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(pit)));

			config = new RunConfiguration();
			config.randomSeed = 1;
			config.range = true;
			config.rangeStart = 10;
			config.rangeStop = 20;

			e = new Engine(null);
			e.startFuzzing(dom, config);

			Assert.AreEqual((1 + 11) * 3, this.dataModels.Count);

			int offset = origModels.Count - dataModels.Count;

			for (int i = 3; i < dataModels.Count; ++i)
			{
				int oldVal = (int)origModels[offset + i][0].InternalValue;
				int newVal = (int)dataModels[i][0].InternalValue;
				Assert.AreEqual(oldVal, newVal);
			}
		}

		[Test]
		public void NumericString()
		{
			// Test string parent
			string xml = @"
<Peach>
	<DataModel name='DM'>
		<String name='str' value='100' mutable='false'>
			<Fixup class='SequenceRandomFixup'/>
		</String>
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
		<Strategy class='RandomDeterministic'/>
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

			Assert.AreEqual("100", (string)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual("100", (string)dom.dataModels[0][0].InternalValue);

			HashSet<string> numbers = new HashSet<string>();
			Assert.True(numbers.Add("100"));

			Assert.AreEqual("100", (string)dataModels[0][0].DefaultValue);
			Assert.True(numbers.Add((string)dataModels[0][0].InternalValue));

			Assert.AreEqual("100", (string)dataModels[1][0].DefaultValue);
			Assert.True(numbers.Add((string)dataModels[1][0].InternalValue));

			Assert.AreEqual("100", (string)dataModels[2][0].DefaultValue);
			Assert.True(numbers.Add((string)dataModels[2][0].InternalValue));

			// Fuzz run 1 should be the same as the control iteration
			Assert.AreEqual("100", (string)dataModels[3][0].DefaultValue);
			Assert.AreEqual((string)dataModels[0][0].InternalValue, (string)dataModels[3][0].InternalValue);

			Assert.AreEqual("100", (string)dataModels[4][0].DefaultValue);
			Assert.AreEqual((string)dataModels[1][0].InternalValue, (string)dataModels[4][0].InternalValue);

			Assert.AreEqual("100", (string)dataModels[5][0].DefaultValue);
			Assert.AreEqual((string)dataModels[2][0].InternalValue, (string)dataModels[5][0].InternalValue);

			// Other fuzz runs should not produce the same numbers
			for (int i = 6; i < 12; i++)
			{
				Assert.AreEqual("100", (string)dataModels[i][0].DefaultValue);
				Assert.True(numbers.Add((string)dataModels[i][0].InternalValue));
			}
		}

		[Test]
		public void BadStringParent()
		{
			// Test non-numeric string
			string xml = @"
<Peach>
	<DataModel name='DM'>
		<String name='str' value='Hello World'>
			<Fixup class='SequenceRandomFixup'/>
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
				Assert.AreEqual("SequenceRandomFixup has non numeric parent 'DM.str'.", ex.Message);
			}
		}

		[Test]
		public void BadParent()
		{
			// Test bad parent
			string xml = @"
<Peach>
	<DataModel name='DM'>
		<Blob name='blob'>
			<Fixup class='SequenceRandomFixup'/>
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
				Assert.AreEqual("SequenceRandomFixup has non numeric parent 'DM.blob'.", ex.Message);
			}
		}
    }
}

// end
