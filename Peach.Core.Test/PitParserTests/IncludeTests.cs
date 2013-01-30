using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NUnit.Framework;
//using NUnit.Framework.Constraints;
using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Analyzers;

namespace Peach.Core.Test.PitParserTests
{
	[TestFixture]
	class IncludeTests
	{
		[Test]
		public void Test1()
		{
			string inc1 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Peach>
	<DataModel name=""HelloWorldTemplate"">
		<String name=""str"" value=""Hello World!""/>
	</DataModel>
</Peach>
";

			string template = @"
<Peach>
	<Include ns=""example"" src=""{0}"" />

	<StateModel name=""State"" initialState=""Initial"">
		<State name=""Initial"">
			<Action type=""output"">
				<DataModel ref=""example:HelloWorldTemplate"" />
			</Action>
		</State>
	</StateModel>
	
	<Test name=""Default"">
		<StateModel ref=""State"" />
		<Publisher class=""File"">
			<Param name=""FileName"" value=""{1}""/>
		</Publisher>
	</Test>
	
</Peach>";
			
			string remote = Path.GetTempFileName();
			string output = Path.GetTempFileName();

			string xml = string.Format(template, remote, output);

			using (TextWriter writer = File.CreateText(remote))
			{
				writer.Write(inc1);
			}

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			string result = File.ReadAllText(output);

			Assert.AreEqual("Hello World!", result);
		}

	}
}

