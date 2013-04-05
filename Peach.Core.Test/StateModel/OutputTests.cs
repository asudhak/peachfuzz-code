using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using NLog;
using NLog.Targets;
using NLog.Config;

using NUnit.Framework;
using NUnit.Framework.Constraints;

using Peach.Core;
using Peach.Core.IO;
using Peach.Core.Dom;
using Peach.Core.Analyzers;
using Peach.Core.Cracker;
using Peach.Core.Publishers;

namespace Peach.Core.Test.StateModel
{
	class MemoryStreamPublisher : StreamPublisher
	{
		private static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		protected override NLog.Logger Logger { get { return logger; } }

		public MemoryStreamPublisher(MemoryStream stream)
			: base(new Dictionary<string, Variant>())
		{
			this.stream = stream;
		}
	}

	[TestFixture]
	class OutputTests : DataModelCollector
	{
		[Test]
		public void Test1()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
				"<Peach>" +
				"   <DataModel name=\"TheDataModel1\">" +
				"       <String value=\"Hello World!\"/>" +
				"   </DataModel>" +

				"   <StateModel name=\"TheStateModel\" initialState=\"InitialState\">" +
				"       <State name=\"InitialState\">" +
				"           <Action name=\"Action1\" type=\"output\">" +
				"               <DataModel ref=\"TheDataModel1\"/>" +
				"           </Action>" +
				"       </State>" +
				"   </StateModel>" +

				"   <Test name=\"Default\">" +
				"       <StateModel ref=\"TheStateModel\"/>" +
				"       <Publisher class=\"Null\"/>" +
				"   </Test>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			MemoryStream stream = new MemoryStream();
			dom.tests[0].publishers[0] = new MemoryStreamPublisher(stream);

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			byte [] buff = new byte[stream.Length];

			stream.Position = 0;
			stream.Read(buff, 0, buff.Length);

			Assert.AreEqual(ASCIIEncoding.ASCII.GetBytes("Hello World!"), buff);
		}

		[Test]
		public void RecursiveStates()
		{
			string xml = @"
<Peach>
	<DataModel name='Foo'>
		<String name='str1' value='Foo Data Model'/>
	</DataModel>

	<DataModel name='DM'>
		<Number name='num' size='8' mutable='false'>
			<Fixup class='SequenceIncrementFixup'/>
		</Number>
		<String name='str1' value='Hello'/>
		<String name='str2' value='World'/>
	</DataModel>

	<StateModel name='SM' initialState='Initial'>
		<State name='Initial'>
			<Action type='slurp' valueXpath='//Foo/str1' setXpath='//DM/str2'>
				<DataModel ref='Foo'/>
			</Action>
			<Action type='changeState' ref='Send'/>
		</State>

		<State name='Send'>
			<Action type='output'>
				<DataModel ref='DM'/>
			</Action>
			<Action type='changeState' ref='Send' when='int(state.actions[0].dataModel[&quot;num&quot;].InternalValue) &lt; 5'/>
		</State>

	</StateModel>

	<Test name='Default'>
		<StateModel ref='SM'/>
		<Publisher class='Null'/>
		<Strategy class='Random'>
			<Param name='MaxFieldsToMutate' value='1'/>
		</Strategy>
	</Test>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			dom.tests[0].includedMutators = new List<string>();
			dom.tests[0].includedMutators.Add("StringMutator");

			RunConfiguration config = new RunConfiguration();
			config.range = true;
			config.rangeStart = 1;
			config.rangeStop = 10;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);


			Assert.AreEqual(66, dataModels.Count);

			for (int i = 1; i < 6; ++i)
			{
				Assert.AreEqual(i, (int)dataModels[i][0].InternalValue);
				Assert.AreEqual("Hello", (string)dataModels[i][1].InternalValue);
				if (i == 1)
					Assert.AreEqual("Foo Data Model", (string)dataModels[i][2].InternalValue);
				else
					Assert.AreEqual("World", (string)dataModels[i][2].InternalValue);
			}

			for (int i = 6; i < dataModels.Count; i += 6)
			{
				for (int j = 1; j < 6; ++j)
				{
					Assert.AreEqual(j, (int)dataModels[i + j][0].InternalValue);

					string str1 = (string)dataModels[i + j][1].InternalValue;
					string str2 = (string)dataModels[i + j][2].InternalValue;

					string exp = j == 1 ? "Foo Data Model" : "World";

					if (str1 == "Hello")
						Assert.AreNotEqual(exp, str2);
					else
						Assert.AreEqual(exp, str2);
				}
			}

		}
	}
}
