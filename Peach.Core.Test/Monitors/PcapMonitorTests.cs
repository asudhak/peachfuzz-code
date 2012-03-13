
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
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Analyzers;
using Peach.Core.IO;
using Peach.Core.Agent;

namespace Peach.Core.Test.Monitors
{
	[TestFixture]
	class PcapMonitorTests
	{
		[Test]
		public void BasicTest()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"+
				"<Peach xmlns=\"http://phed.org/2008/Peach\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\""+
				"	xsi:schemaLocation=\"http://phed.org/2008/Peach /peach3.0/peachcore/peach.xsd\">"+
				"			"+
				"	<Include ns=\"default\" src=\"file:defaults.xml\"/>"+
				"			"+
				"	<DataModel name=\"TheDataModel\">"+
				"		<String value=\"Hello World\" />"+
				"	</DataModel>"+
				"	"+
				"	<Agent name=\"LocalAgent\">"+
				"		<Monitor class=\"PcapMonitor\">"+
				"			<Param name=\"Device\" value=\"rpcap://\\Device\\NPF_{8DB9FD50-6702-4CB7-9ED3-D20E603BF543}\"/>"+
				"			<Param name=\"Filter\" value=\"port 80\"/>"+
				"		</Monitor>"+
				"	</Agent>"+
				"	"+
				"	<StateModel name=\"TheState\" initialState=\"Initial\">"+
				"		<State name=\"Initial\">"+
				"			<Action type=\"output\" publisher=\"Stdout\">"+
				"				<DataModel ref=\"TheDataModel\"/>"+
				"			</Action>"+
				"		</State>"+
				"	</StateModel>"+
				"	"+
				"	<Test name=\"TheTest\">"+
				"		<Agent ref=\"LocalAgent\"/>"+
				"		<StateModel ref=\"TheState\"/>"+
				"		<Publisher class=\"Console\" name=\"Stdout\" />"+
				"	</Test>"+
				"		"+
				"	<Run name=\"DefaultRun\">"+
				"		<Test ref=\"TheTest\"/>"+
				"	</Run>"+
				"		"+
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Number num = dom.dataModels[0][1] as Number;

			Variant val = num.InternalValue;
			Assert.AreEqual(5, (int)val);
		}
	}
}

// end
