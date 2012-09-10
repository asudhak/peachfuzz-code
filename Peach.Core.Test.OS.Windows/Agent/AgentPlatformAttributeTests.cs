
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
using NUnit.Framework.Constraints;
using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Analyzers;
using Peach.Core.Cracker;
using Peach.Core.IO;

namespace Peach.Core.Test.Agent
{
	[TestFixture]
	public class AgentPlatformAttributeTests
	{
		[Test]
		public void ParsingTest()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Blob />" +
				"	</DataModel>" +
				"	<StateModel name=\"TheStateModel\" initialState=\"TheState\">"+
				"		<State name=\"TheState\">"+
				"			<Action type=\"output\">"+
				"				<DataModel ref=\"TheDataModel\"/>"+
				"			</Action>"+
				"		</State>"+
				"	</StateModel>"+
				"   <Agent name=\"AgentWindows\"> "+
				"		<Monitor class=\"WindowsDebugEngine\"> "+
				"			<Param name=\"CommandLine\" value=\"C:\\Peach3\\Release\\CrashableServer.exe 127.0.0.1 4244\" /> "+
				"			<Param name=\"WinDbgPath\" value=\"C:\\Program Files (x86)\\Debugging Tools for Windows (x86)\" /> "+
				"		</Monitor>"+
				"	</Agent>"+
				"	<Test name=\"TheTest\">"+
				"		<Agent ref=\"AgentWindows\" platform=\"windows\"/>"+
				"		<StateModel ref=\"TheStateModel\" />"+
				"		<Publisher class=\"File\">"+
				"			<Param name=\"FileName\" value=\"test.fuzzed.txt\" /> "+
				"		</Publisher>"+
				"	</Test> "+
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = null;

			try
			{
				dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			}
			catch
			{
				Assert.True(false);
			}

			Assert.NotNull(dom);
			Assert.AreEqual(Platform.OS.Windows, dom.tests[0].agents[0].platform);
		}
	}
}

// end
