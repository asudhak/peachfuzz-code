
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
				"   <Agent name=\"AgentWindows\"> " +
				"		<Monitor class=\"WindowsDebugEngine\"> " +
				"			<Param name=\"CommandLine\" value=\"C:\\Peach3\\Release\\CrashableServer.exe 127.0.0.1 4244\" /> " +
				"			<Param name=\"WinDbgPath\" value=\"C:\\Program Files (x86)\\Debugging Tools for Windows (x86)\" /> " +
				"		</Monitor>" +
				"	</Agent>" +
				"	<Test name=\"Default\">" +
				"		<Agent ref=\"AgentWindows\" platform=\"windows\"/>" +
				"		<StateModel ref=\"TheStateModel\" />" +
				"		<Publisher class=\"File\">" +
				"			<Param name=\"FileName\" value=\"test.fuzzed.txt\" /> " +
				"		</Publisher>" +
				"		<Exclude/>" +
				"	</Test> " +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

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
				"   <Agent name=\"AgentWindows\"> " +
				"		<Monitor class=\"WindowsDebugEngine\"> " +
				"			<Param name=\"CommandLine\" value=\"C:\\Peach3\\Release\\CrashableServer.exe 127.0.0.1 4244\" /> " +
				"			<Param name=\"WinDbgPath\" value=\"C:\\Program Files (x86)\\Debugging Tools for Windows (x86)\" /> " +
				"		</Monitor>" +
				"	</Agent>" +
				"	<Test name=\"Default\">" +
				"		<Agent ref=\"AgentWindows\" platform=\"windows\"/>" +
				"		<StateModel ref=\"TheStateModel\" />" +
				"		<Publisher class=\"File\">" +
				"			<Param name=\"FileName\" value=\"test.fuzzed.txt\" /> " +
				"		</Publisher>" +
				"		<Exclude/>" +
				"	</Test> " +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

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
				"   <Agent name=\"AgentWindows\"> " +
				"		<Monitor class=\"WindowsDebugEngine\"> " +
				"			<Param name=\"CommandLine\" value=\"C:\\Peach3\\Release\\CrashableServer.exe 127.0.0.1 4244\" /> " +
				"			<Param name=\"WinDbgPath\" value=\"C:\\Program Files (x86)\\Debugging Tools for Windows (x86)\" /> " +
				"		</Monitor>" +
				"	</Agent>" +
				"	<Test name=\"Default\">" +
				"		<Agent ref=\"AgentWindows\" platform=\"windows\"/>" +
				"		<StateModel ref=\"TheStateModel\" />" +
				"		<Publisher class=\"File\">" +
				"			<Param name=\"FileName\" value=\"test.fuzzed.txt\" /> " +
				"		</Publisher>" +
				"		<Exclude/>" +
				"		<Include xpath=\"//Blob2\"/>" +
				"	</Test> " +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

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
				"   <Agent name=\"AgentWindows\"> " +
				"		<Monitor class=\"WindowsDebugEngine\"> " +
				"			<Param name=\"CommandLine\" value=\"C:\\Peach3\\Release\\CrashableServer.exe 127.0.0.1 4244\" /> " +
				"			<Param name=\"WinDbgPath\" value=\"C:\\Program Files (x86)\\Debugging Tools for Windows (x86)\" /> " +
				"		</Monitor>" +
				"	</Agent>" +
				"	<Test name=\"Default\">" +
				"		<Agent ref=\"AgentWindows\" platform=\"windows\"/>" +
				"		<StateModel ref=\"TheStateModel\" />" +
				"		<Publisher class=\"File\">" +
				"			<Param name=\"FileName\" value=\"test.fuzzed.txt\" /> " +
				"		</Publisher>" +
				"		<Exclude/>" +
				"		<Include xpath=\"//Block2\"/>" +
				"	</Test> " +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

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
				"   <Agent name=\"AgentWindows\"> " +
				"		<Monitor class=\"WindowsDebugEngine\"> " +
				"			<Param name=\"CommandLine\" value=\"C:\\Peach3\\Release\\CrashableServer.exe 127.0.0.1 4244\" /> " +
				"			<Param name=\"WinDbgPath\" value=\"C:\\Program Files (x86)\\Debugging Tools for Windows (x86)\" /> " +
				"		</Monitor>" +
				"	</Agent>" +
				"	<Test name=\"Default\">" +
				"		<Agent ref=\"AgentWindows\" platform=\"windows\"/>" +
				"		<StateModel ref=\"TheStateModel\" />" +
				"		<Publisher class=\"File\">" +
				"			<Param name=\"FileName\" value=\"test.fuzzed.txt\" /> " +
				"		</Publisher>" +
				"		<Exclude xpath=\"//Blob2\"/>" +
				"	</Test> " +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

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
				"   <Agent name=\"AgentWindows\"> " +
				"		<Monitor class=\"WindowsDebugEngine\"> " +
				"			<Param name=\"CommandLine\" value=\"C:\\Peach3\\Release\\CrashableServer.exe 127.0.0.1 4244\" /> " +
				"			<Param name=\"WinDbgPath\" value=\"C:\\Program Files (x86)\\Debugging Tools for Windows (x86)\" /> " +
				"		</Monitor>" +
				"	</Agent>" +
				"	<Test name=\"Default\">" +
				"		<Agent ref=\"AgentWindows\" platform=\"windows\"/>" +
				"		<StateModel ref=\"TheStateModel\" />" +
				"		<Publisher class=\"File\">" +
				"			<Param name=\"FileName\" value=\"test.fuzzed.txt\" /> " +
				"		</Publisher>" +
				"		<Exclude xpath=\"//Block2\"/>" +
				"	</Test> " +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

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
