using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Analyzers;
using Peach.Core.Cracker;
using Peach.Core.IO;

namespace Peach.Core.Test.CrackingTests
{
	[TestFixture]
	class FlagsTests
	{
		[Test]
		public void CrackFlag1()
		{
			string xml = @"
<Peach>
	<DataModel name=""TheDataModel"">
		<Flags size=""16"">
			<Flag position=""0"" size=""1""/>
		</Flags>
		<String value=""Hello World""/>
	</DataModel>

	<StateModel name=""TheState"" initialState=""Initial"">
		<State name=""Initial"">
			<Action type=""output"">
				<DataModel ref=""TheDataModel""/>
			</Action>
		</State>
	</StateModel>

	<Test name=""Default"">
		<StateModel ref=""TheState""/>
		<Publisher class=""StdoutHex""/>
		<Strategy class=""Sequential""/>
	</Test>
</Peach>
";
			var pub = new Peach.Core.Test.Publishers.TestPublisher();

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			dom.tests[0].publishers[0] = pub;

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.config = config;
			e.startFuzzing(dom, config);

			BitStream val = dom.dataModels[0].Value;
			Assert.NotNull(val);
			Assert.AreEqual(13, val.LengthBytes);
			Assert.AreEqual(13 * 8, val.LengthBits);

			pub.Stream.Seek(0, SeekOrigin.Begin);
			string results = Encoding.ASCII.GetString(pub.Stream.ToArray());
			Assert.NotNull(results);
			string expected = "00000000   00 00 48 65 6C 6C 6F 20  57 6F 72 6C 64            ??Hello World   " + Environment.NewLine;
			Assert.AreEqual(expected, results);
		}
	}
}
