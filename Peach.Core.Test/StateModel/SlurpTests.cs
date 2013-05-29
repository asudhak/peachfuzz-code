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
using Peach.Core.Dom;
using Peach.Core.Analyzers;
using Peach.Core.Cracker;
using Peach.Core.IO;

namespace Peach.Core.Test.StateModel
{
	[TestFixture]
	class SlurpTests
	{
		[Test]
		public void Test1()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
				"<Peach>" +
				"   <DataModel name=\"TheDataModel1\">" +
				"       <String name=\"String1\" value=\"1234567890\"/>" +
				"   </DataModel>" +
				"   <DataModel name=\"TheDataModel2\">" +
				"       <String name=\"String2\" value=\"Hello World!\"/>" +
				"   </DataModel>" +

				"   <StateModel name=\"TheStateModel\" initialState=\"InitialState\">" +
				"       <State name=\"InitialState\">" +
				"           <Action name=\"Action1\" type=\"output\">" +
				"               <DataModel ref=\"TheDataModel1\"/>" +
				"           </Action>" +
				
				"           <Action name=\"Action2\" type=\"slurp\" valueXpath=\"//String1\" setXpath=\"//String2\" />"+

				"           <Action name=\"Action3\" type=\"output\">" +
				"               <DataModel ref=\"TheDataModel2\"/>" +
				"           </Action>" +

				"           <Action name=\"Action4\" type=\"output\">" +
				"               <DataModel ref=\"TheDataModel2\"/>" +
				"           </Action>" +
				"       </State>" +
				"   </StateModel>" +

				"   <Test name=\"Default\">" +
				"       <StateModel ref=\"TheStateModel\"/>" +
				"       <Publisher class=\"Null\"/>" +
				"       <Strategy class=\"RandomDeterministic\"/>" +
				"   </Test>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			var stateModel = dom.tests[0].stateModel;
			var state = stateModel.initialState;

			Assert.AreEqual(state.actions[0].dataModel.Value.Value, state.actions[2].dataModel.Value.Value);
			Assert.AreEqual(state.actions[0].dataModel.Value.Value, state.actions[3].dataModel.Value.Value);
		}

		[Test]
		public void Test2()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
				"<Peach>" +
				"   <DataModel name=\"TheDataModel1\">" +
				"       <String name=\"String1\" value=\"1234567890\"/>" +
				"   </DataModel>" +
				"   <DataModel name=\"TheDataModel2\">" +
				"       <String name=\"String2\" value=\"Hello World!\"/>" +
				"   </DataModel>" +

				"   <StateModel name=\"TheStateModel\" initialState=\"InitialState\">" +
				"       <State name=\"InitialState\">" +
				"           <Action name=\"Action1\" type=\"output\">" +
				"               <DataModel ref=\"TheDataModel1\"/>" +
				"           </Action>" +

				"           <Action name=\"Action2\" type=\"slurp\" valueXpath=\"//Action1/TheDataModel1/String1\" setXpath=\"//String2\" />" +

				"           <Action name=\"Action3\" type=\"output\">" +
				"               <DataModel ref=\"TheDataModel1\"/>" +
				"           </Action>" +

				"           <Action name=\"Action4\" type=\"output\">" +
				"               <DataModel ref=\"TheDataModel2\"/>" +
				"           </Action>" +
				"       </State>" +
				"   </StateModel>" +

				"   <Test name=\"Default\">" +
				"       <StateModel ref=\"TheStateModel\"/>" +
				"       <Publisher class=\"Null\"/>" +
				"       <Strategy class=\"RandomDeterministic\"/>" +
				"   </Test>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			var stateModel = dom.tests[0].stateModel;
			var state = stateModel.initialState;

			Assert.AreEqual(state.actions[0].dataModel.Value.Value, state.actions[2].dataModel.Value.Value);
			Assert.AreEqual(state.actions[0].dataModel.Value.Value, state.actions[3].dataModel.Value.Value);
		}
	}
}
