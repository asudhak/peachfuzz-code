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
	class CopyValueFixupTests : DataModelCollector
	{
		[Test]
		public void TestNumber()
		{
			string xml = @"
<Peach>
	<DataModel name='DM'>
		<Number name='num1' size='32' value='100'>
			<Fixup class='CopyValueFixup'>
				<Param name='ref' value='num2'/>
			</Fixup>
		</Number>
		<Number name='num2' size='32' value='200'/>
	</DataModel>

	<StateModel name='SM' initialState='Initial'>
		<State name='Initial'>
			<Action type='output'>
				<DataModel ref='DM'/>
			</Action>
		</State>
	</StateModel>

	<Test name='Default'>
		<StateModel ref='SM'/>
		<Publisher class='Null'/>
	</Test>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			Assert.AreEqual(1, dataModels.Count);

			var exp = Bits.Fmt("{0:L32}{1:L32}", 200, 200).ToArray();
			Assert.AreEqual(exp, dataModels[0].Value.ToArray());
		}

		[Test]
		public void TestString()
		{
			string xml = @"
<Peach>
	<DataModel name='DM'>
		<String name='str1' value='Hello'>
			<Fixup class='CopyValueFixup'>
				<Param name='ref' value='str2'/>
			</Fixup>
		</String>
		<String name='str2' value='World'/>
	</DataModel>

	<StateModel name='SM' initialState='Initial'>
		<State name='Initial'>
			<Action type='output'>
				<DataModel ref='DM'/>
			</Action>
		</State>
	</StateModel>

	<Test name='Default'>
		<StateModel ref='SM'/>
		<Publisher class='Null'/>
	</Test>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			Assert.AreEqual(1, dataModels.Count);

			var exp = Encoding.ASCII.GetBytes("WorldWorld");
			Assert.AreEqual(exp, dataModels[0].Value.ToArray());
		}

		[Test]
		public void TestBlob()
		{
			string xml = @"
<Peach>
	<DataModel name='DM'>
		<Blob name='blob1' value='Hello'>
			<Fixup class='CopyValueFixup'>
				<Param name='ref' value='blob2'/>
			</Fixup>
		</Blob>
		<Blob name='blob2' value='World'/>
	</DataModel>

	<StateModel name='SM' initialState='Initial'>
		<State name='Initial'>
			<Action type='output'>
				<DataModel ref='DM'/>
			</Action>
		</State>
	</StateModel>

	<Test name='Default'>
		<StateModel ref='SM'/>
		<Publisher class='Null'/>
	</Test>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			Assert.AreEqual(1, dataModels.Count);

			var exp = Encoding.ASCII.GetBytes("WorldWorld");
			Assert.AreEqual(exp, dataModels[0].Value.ToArray());
		}
	}
}

// end
