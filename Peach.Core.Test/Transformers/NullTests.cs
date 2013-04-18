using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Analyzers;

namespace Peach.Core.Test.Transformers
{
	[TestFixture]
	class NullTests : DataModelCollector
	{
		[Test]
		public void Test1()
		{
			// standard test
			string xml = @"
				<Peach>
					<DataModel name=""TheDataModel"">
						<Block name=""TheBlock"">
							<String value=""Hello""/>
							<Transformer class=""Null""/>
						</Block>
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
						<Publisher class=""Null""/>
					</Test>
				</Peach>";

			PitParser parser = new PitParser();

			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			// verify values
			// -- this is the pre-calculated result on the string: "Hello"
			byte[] precalcResult = new byte[] { 0x48, 0x65, 0x6c, 0x6c, 0x6f  };
			Assert.AreEqual(1, values.Count);
			Assert.AreEqual(precalcResult, values[0].Value);
		}
	}
}
