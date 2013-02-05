using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core;
using Peach.Core.Analyzers;
using System.IO;

namespace Peach.Core.Test.Publishers
{
	[TestFixture]
	class RemotePublisherTests
	{
		string template = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Peach>
	<DataModel name=""TheDataModel"">
		<String value=""Hello""/>
	</DataModel>

	<StateModel name=""TheStateModel"" initialState=""InitialState"">
		<State name=""InitialState"">
			<Action name=""Action1"" type=""output"">
				<DataModel ref=""TheDataModel""/>
			</Action>
		</State>
	</StateModel>

	<Agent name=""LocalAgent"">
	</Agent>

	<Test name=""Default"">
		<Agent ref=""LocalAgent""/>
		<StateModel ref=""TheStateModel""/>
		<Publisher class=""Remote"">
			<Param name=""Agent"" value=""LocalAgent""/>
			<Param name=""Class"" value=""File""/>
			<Param name=""FileName"" value=""{0}""/>
		</Publisher>
	</Test>
</Peach>";

		[Test]
		public void TestCreate()
		{
			string xml = string.Format(template, "tile.txt");

			PitParser parser = new PitParser();
			parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
		}

		[Test]
		public void TestRun()
		{
			string tempFile = Path.GetTempFileName();

			string xml = string.Format(template, tempFile);

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			string[] output = File.ReadAllLines(tempFile);

			Assert.AreEqual(1, output.Length);
			Assert.AreEqual("Hello", output[0]);
		}
	}
}
