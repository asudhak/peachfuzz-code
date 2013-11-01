
//
// Copyright (c) Michael Eddington
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in	
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

// Authors:
//   Michael Eddington (mike@dejavusecurity.com)

// $Id$

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using NUnit.Framework;

using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Analyzers;

namespace Peach.Core.Test.PitParserTests
{
	[TestFixture]
	class TestTests
	{
		[Test]
		public void Default()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Blob />" +
				"		<Blob name=\"Blob2\" />" +
				"		<Blob />" +
				"	</DataModel>" +
				"	<StateModel name=\"TheStateModel\" initialState=\"TheState\">" +
				"		<State name=\"TheState\">" +
				"			<Action type=\"output\">" +
				"				<DataModel ref=\"TheDataModel\"/>" +
				"			</Action>" +
				"		</State>" +
				"	</StateModel>" +
				"	<Test name=\"Default\">" +
				"		<StateModel ref=\"TheStateModel\" />" +
				"		<Publisher class=\"Null\" />" +
				"		<Exclude/>" +
				"	</Test> " +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var config = new RunConfiguration() { singleIteration = true };
			var engine = new Engine(null);
			engine.startFuzzing(dom, config);

			Assert.AreEqual(true, dom.dataModels[0][0].isMutable);
			Assert.AreEqual(true, dom.dataModels[0][1].isMutable);
			Assert.AreEqual(true, dom.dataModels[0][2].isMutable);
		}

		[Test]
		public void ExcludeAll()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Blob />" +
				"		<Blob name=\"Blob2\" />" +
				"		<Blob />" +
				"	</DataModel>" +
				"	<StateModel name=\"TheStateModel\" initialState=\"TheState\">" +
				"		<State name=\"TheState\">" +
				"			<Action type=\"output\">" +
				"				<DataModel ref=\"TheDataModel\"/>" +
				"			</Action>" +
				"		</State>" +
				"	</StateModel>" +
				"	<Test name=\"Default\">" +
				"		<StateModel ref=\"TheStateModel\" />" +
				"		<Publisher class=\"Null\" />" +
				"		<Exclude/>" +
				"	</Test> " +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var config = new RunConfiguration() { singleIteration = true };
			var engine = new Engine(null);
			engine.startFuzzing(dom, config);

			Assert.AreEqual(false, dom.tests[0].stateModel.states.Values.ElementAt(0).actions[0].dataModel[0].isMutable);
			Assert.AreEqual(false, dom.tests[0].stateModel.states.Values.ElementAt(0).actions[0].dataModel[1].isMutable);
			Assert.AreEqual(false, dom.tests[0].stateModel.states.Values.ElementAt(0).actions[0].dataModel[2].isMutable);
		}

		[Test]
		public void ExcludeThenIncludeAll()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Blob />" +
				"		<Blob name=\"Blob2\" />" +
				"		<Blob />" +
				"	</DataModel>" +
				"	<StateModel name=\"TheStateModel\" initialState=\"TheState\">" +
				"		<State name=\"TheState\">" +
				"			<Action type=\"output\">" +
				"				<DataModel ref=\"TheDataModel\"/>" +
				"			</Action>" +
				"		</State>" +
				"	</StateModel>" +
				"	<Test name=\"Default\">" +
				"		<StateModel ref=\"TheStateModel\" />" +
				"		<Publisher class=\"Null\" />" +
				"		<Exclude/>" +
				"		<Include xpath=\"//Blob2\"/>" +
				"	</Test> " +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var config = new RunConfiguration() { singleIteration = true };
			var engine = new Engine(null);
			engine.startFuzzing(dom, config);

			Assert.AreEqual(false, dom.tests[0].stateModel.states.Values.ElementAt(0).actions[0].dataModel[0].isMutable);
			Assert.AreEqual(true,  dom.tests[0].stateModel.states.Values.ElementAt(0).actions[0].dataModel[1].isMutable);
			Assert.AreEqual(false, dom.tests[0].stateModel.states.Values.ElementAt(0).actions[0].dataModel[2].isMutable);
		}

		[Test]
		public void ExcludeThenIncludeBlock()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Blob />" +
				"		<Block name=\"Block2\">" +
				"			<Block>" +
				"				<Blob/>" +
				"			</Block>" +
				"		</Block>" +
				"		<Blob />" +
				"	</DataModel>" +
				"	<StateModel name=\"TheStateModel\" initialState=\"TheState\">" +
				"		<State name=\"TheState\">" +
				"			<Action type=\"output\">" +
				"				<DataModel ref=\"TheDataModel\"/>" +
				"			</Action>" +
				"		</State>" +
				"	</StateModel>" +
				"	<Test name=\"Default\">" +
				"		<StateModel ref=\"TheStateModel\" />" +
				"		<Publisher class=\"Null\" />" +
				"		<Exclude/>" +
				"		<Include xpath=\"//Block2\"/>" +
				"	</Test> " +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var config = new RunConfiguration() { singleIteration = true };
			var engine = new Engine(null);
			engine.startFuzzing(dom, config);

			Assert.AreEqual(false, dom.tests[0].stateModel.states.Values.ElementAt(0).actions[0].dataModel[0].isMutable);
			Assert.AreEqual(true, dom.tests[0].stateModel.states.Values.ElementAt(0).actions[0].dataModel[1].isMutable);
			Assert.AreEqual(false, dom.tests[0].stateModel.states.Values.ElementAt(0).actions[0].dataModel[2].isMutable);

			var cont = dom.tests[0].stateModel.states.Values.ElementAt(0).actions[0].dataModel[1] as DataElementContainer;
			Assert.NotNull(cont);
			Assert.AreEqual(1, cont.Count);
			cont = cont[0] as DataElementContainer;
			Assert.NotNull(cont);
			Assert.AreEqual(1, cont.Count);
			Assert.AreEqual(true, cont.isMutable);
			Assert.AreEqual(true, cont[0].isMutable);
		}

		[Test]
		public void ExcludeSpecific()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Blob />" +
				"		<Blob name=\"Blob2\" />" +
				"		<Blob />" +
				"	</DataModel>" +
				"	<StateModel name=\"TheStateModel\" initialState=\"TheState\">" +
				"		<State name=\"TheState\">" +
				"			<Action type=\"output\">" +
				"				<DataModel ref=\"TheDataModel\"/>" +
				"			</Action>" +
				"		</State>" +
				"	</StateModel>" +
				"	<Test name=\"Default\">" +
				"		<StateModel ref=\"TheStateModel\" />" +
				"		<Publisher class=\"Null\" />" +
				"		<Exclude xpath=\"//Blob2\"/>" +
				"	</Test> " +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var config = new RunConfiguration() { singleIteration = true };
			var engine = new Engine(null);
			engine.startFuzzing(dom, config);

			Assert.AreEqual(true, dom.tests[0].stateModel.states.Values.ElementAt(0).actions[0].dataModel[0].isMutable);
			Assert.AreEqual(false, dom.tests[0].stateModel.states.Values.ElementAt(0).actions[0].dataModel[1].isMutable);
			Assert.AreEqual(true, dom.tests[0].stateModel.states.Values.ElementAt(0).actions[0].dataModel[2].isMutable);
		}

		[Test]
		public void ExcludeBlock()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Blob />" +
				"		<Block name=\"Block2\">" +
				"			<Block>" +
				"				<Blob/>" +
				"			</Block>" +
				"		</Block>" +
				"		<Blob />" +
				"	</DataModel>" +
				"	<StateModel name=\"TheStateModel\" initialState=\"TheState\">" +
				"		<State name=\"TheState\">" +
				"			<Action type=\"output\">" +
				"				<DataModel ref=\"TheDataModel\"/>" +
				"			</Action>" +
				"		</State>" +
				"	</StateModel>" +
				"	<Test name=\"Default\">" +
				"		<StateModel ref=\"TheStateModel\" />" +
				"		<Publisher class=\"Null\" />" +
				"		<Exclude xpath=\"//Block2\"/>" +
				"	</Test> " +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var config = new RunConfiguration() { singleIteration = true };
			var engine = new Engine(null);
			engine.startFuzzing(dom, config);

			Assert.AreEqual(true, dom.tests[0].stateModel.states.Values.ElementAt(0).actions[0].dataModel[0].isMutable);
			Assert.AreEqual(false, dom.tests[0].stateModel.states.Values.ElementAt(0).actions[0].dataModel[1].isMutable);
			Assert.AreEqual(true, dom.tests[0].stateModel.states.Values.ElementAt(0).actions[0].dataModel[2].isMutable);

			var cont = dom.tests[0].stateModel.states.Values.ElementAt(0).actions[0].dataModel[1] as DataElementContainer;
			Assert.NotNull(cont);
			Assert.AreEqual(1, cont.Count);
			cont = cont[0] as DataElementContainer;
			Assert.NotNull(cont);
			Assert.AreEqual(1, cont.Count);
			Assert.AreEqual(false, cont.isMutable);
			Assert.AreEqual(false, cont[0].isMutable);
		}

		[Test]
		public void IncludeExcludeScope()
		{
			string xml = @"
<Peach>
	<DataModel name='DM'>
		<String name='str'/>
		<Number name='num' size='32'/>
		<Blob name='blob'/>
	</DataModel>

	<StateModel name='StateModel' initialState='initial'>
		<State name='initial'>
			<Action type='output'>
				<DataModel ref='DM'/>
			</Action> 
		</State>
	</StateModel>

	<Test name='Test0'>
		<StateModel ref='StateModel'/>
		<Publisher class='Null'/>
		<Exclude xpath='//str'/>
	</Test>

	<Test name='Test1'>
		<StateModel ref='StateModel'/>
		<Publisher class='Null'/>
		<Exclude xpath='//blob'/>
	</Test>
</Peach>
";

			var parser = new PitParser();
			var dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var config = new RunConfiguration();
			config.singleIteration = true;
			config.runName = "Test1";

			var engine = new Engine(null);
			engine.startFuzzing(dom, config);

			var dm = dom.tests[1].stateModel.states["initial"].actions[0].dataModel;
			Assert.AreEqual(3, dm.Count);
			Assert.True(dm[0].isMutable);
			Assert.True(dm[1].isMutable);
			Assert.False(dm[2].isMutable);
		}

		[Test]
		public void WaitTime()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Blob />" +
				"		<Blob name=\"Blob2\" />" +
				"		<Blob />" +
				"	</DataModel>" +
				"	<StateModel name=\"TheStateModel\" initialState=\"TheState\">" +
				"		<State name=\"TheState\">" +
				"			<Action type=\"output\">" +
				"				<DataModel ref=\"TheDataModel\"/>" +
				"			</Action>" +
				"		</State>" +
				"	</StateModel>" +
				"	<Test name=\"Default\" waitTime=\"10.5\" faultWaitTime=\"99.9\">" +
				"		<StateModel ref=\"TheStateModel\" />" +
				"		<Publisher class=\"File\">" +
				"			<Param name=\"FileName\" value=\"test.fuzzed.txt\" /> " +
				"		</Publisher>" +
				"	</Test> " +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			Assert.AreEqual(10.5, dom.tests[0].waitTime);
			Assert.AreEqual(99.9, dom.tests[0].faultWaitTime);
		}
	}
}
