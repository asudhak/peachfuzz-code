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
	class FilePublisherTests
	{
		[Test]
		public void Test1()
		{
			string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
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

				   <Test name=""Default"">
				       <StateModel ref=""TheStateModel""/>
				       <Publisher class=""File"">
				           <Param name=""FileName"" value=""{0}""/>
				       </Publisher>
				   </Test>

				   <Run name=""DefaultRun"">
				       <Test ref=""TheTest""/>
				   </Run>
				</Peach>";

			string tempFile = Path.GetTempFileName();

			xml = string.Format(xml, tempFile);

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.config = config;
			e.startFuzzing(dom, config);

			string[] output = File.ReadAllLines(tempFile);

			Assert.AreEqual(1, output.Length);
			Assert.AreEqual("Hello", output[0]);
		}
	}
}
