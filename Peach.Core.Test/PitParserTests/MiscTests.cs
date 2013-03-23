using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Analyzers;

namespace Peach.Core.Test.PitParserTests
{
	[TestFixture]
	class MiscTests
	{
		// Pit caused a System.StackOverflowException, see #89
		[Test]
		public void Test1()
		{
			string xml =
@"<Peach>
	<DataModel name=""DM3"">
		<Number name=""tag"" size=""32"" signed=""false"" endian=""big"">
			<Fixup class=""Crc32Fixup"">
				<Param name=""ref"" value=""blockData""/>
			</Fixup>
		</Number>
		<Block name=""blockData"">
			<Number name=""CommandSize"" size=""32"" signed=""false"" endian=""big"">
				<Relation type=""size"" of=""DM3"" />
			</Number>
			<Number name=""CommandCode"" size=""32"" signed=""false"" endian=""big"">
				<Transformer class=""Md5""/>
			</Number>
		</Block>
	</DataModel>

	<StateModel name=""TheState"" initialState=""Initial"">
		<State name=""Initial"">
			<Action type=""output"">
				<DataModel ref=""DM3""/>
			</Action>
		</State>
	</StateModel>

	<Test name=""TheTest"">
		<StateModel ref=""TheState""/>
		<Publisher class=""Null""/>
	</Test>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.rangeStart = 0;
			config.rangeStop = 10;
			config.range = true;
			config.runName = "TheTest";

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Error, the value of element 'blob' is not a valid hex string.")]
		public void TestBadHex()
		{
			// Verify good error message when parsing bad hex value
			string xml =
@"<Peach>
	<DataModel name=""DM"">
		<Blob name=""blob"" valueType=""hex"" value=""hello world""/>
	</DataModel>
</Peach>";
			PitParser parser = new PitParser();
			parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
		}

	}
}
