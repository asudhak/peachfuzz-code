
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
//   Michael Eddington (mike@phed.org)

// $Id$

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Dom.XPath;
using Peach.Core.Analyzers;
using Peach.Core.IO;

namespace Peach.Core.Test
{
	[TestFixture]
	class PeachXPathTests : TestBase
	{
		[Test]
		public void BasicTest()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number name=\"TheNumber\" size=\"8\">" +
				"			<Relation type=\"count\" of=\"Array\" />" +
				"		</Number>" +
				"		<String name=\"Array\" value=\"1\" maxOccurs=\"100\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			PeachXPathNavigator navi = new PeachXPathNavigator(dom);
			XPathNodeIterator iter = navi.Select("//TheNumber");

			Assert.IsTrue(iter.MoveNext());
			Assert.AreEqual(dom.dataModels[0][0], ((PeachXPathNavigator)iter.Current).currentNode);
			Assert.IsFalse(iter.MoveNext());
		}

		[Test]
		public void BasicTest2()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number name=\"TheNumber\" size=\"8\">" +
				"			<Relation type=\"count\" of=\"Array\" />" +
				"		</Number>" +
				"		<Block>" +
				"			<Block>" +
				"				<String name=\"FindMe\"/>" +
				"			</Block>" +
				"		</Block>" +
				"		<String name=\"Array\" value=\"1\" maxOccurs=\"100\"/>" +
				"		<Block>" +
				"			<String name=\"FindMe\"/>" +
				"		</Block>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			DataElement findMe1 = ((DataElementContainer)((DataElementContainer)dom.dataModels[0][1])[0])[0];
			DataElement findMe2 = ((DataElementContainer)dom.dataModels[0][3])[0];

			PeachXPathNavigator navi = new PeachXPathNavigator(dom);
			XPathNodeIterator iter = navi.Select("//FindMe");

			Assert.IsTrue(iter.MoveNext());
			Assert.AreEqual(findMe1, ((PeachXPathNavigator)iter.Current).currentNode);
			Assert.IsTrue(iter.MoveNext());
			Assert.AreEqual(findMe2, ((PeachXPathNavigator)iter.Current).currentNode);
			Assert.IsFalse(iter.MoveNext());
		}

		[Test]
		public void BasicTest3()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number name=\"TheNumber\" size=\"8\">" +
				"			<Relation type=\"count\" of=\"Array\" />" +
				"		</Number>" +
				"		<Block name=\"Block1\">" +
				"			<Block name=\"Block1.1\">" +
				"				<String name=\"FindMe\"/>" +
				"			</Block>" +
				"		</Block>" +
				"		<String name=\"Array\" value=\"1\" maxOccurs=\"100\"/>" +
				"		<Block>" +
				"			<String name=\"FindMe\"/>" +
				"		</Block>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			DataElement findMe1 = ((DataElementContainer)((DataElementContainer)dom.dataModels[0][1])[0])[0];
			DataElement findMe2 = ((DataElementContainer)dom.dataModels[0][3])[0];

			PeachXPathNavigator navi = new PeachXPathNavigator(dom);
			XPathNodeIterator iter = navi.Select("//Block1//FindMe");

			Assert.IsTrue(iter.MoveNext());
			Assert.AreEqual(findMe1, ((PeachXPathNavigator)iter.Current).currentNode);
			Assert.IsFalse(iter.MoveNext());
		}

		[Test]
		public void BasicTest4()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number name=\"TheNumber\" size=\"8\">" +
				"			<Relation type=\"count\" of=\"Array\" />" +
				"		</Number>" +
				"		<Block name=\"Block1\">" +
				"			<Block name=\"Block1.1\">" +
				"				<String name=\"FindMe\"/>" +
				"			</Block>" +
				"		</Block>" +
				"		<String name=\"Array\" value=\"1\" maxOccurs=\"100\"/>" +
				"		<Block>" +
				"			<String name=\"FindMe\"/>" +
				"		</Block>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			DataElement findMe1 = ((DataElementContainer)((DataElementContainer)dom.dataModels[0][1])[0])[0];
			DataElement findMe2 = ((DataElementContainer)dom.dataModels[0][3])[0];

			PeachXPathNavigator navi = new PeachXPathNavigator(dom);
			XPathNodeIterator iter = navi.Select("/TheDataModel/Block1/Block1.1/FindMe");

			Assert.IsTrue(iter.MoveNext());
			Assert.AreEqual(findMe1, ((PeachXPathNavigator)iter.Current).currentNode);
			Assert.IsFalse(iter.MoveNext());
		}

		[Test]
		public void BasicAttributeTest()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number name=\"TheNumber\" size=\"8\">" +
				"			<Relation type=\"count\" of=\"Array\" />" +
				"		</Number>" +
				"		<Block name=\"Block1\">" +
				"			<Block name=\"Block1.1\">" +
				"				<String name=\"FindMe\"/>" +
				"			</Block>" +
				"		</Block>" +
				"		<String name=\"Array\" value=\"1\" maxOccurs=\"100\"/>" +
				"		<Block>" +
				"			<String name=\"FindMe\" token=\"true\"/>" +
				"		</Block>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			DataElement findMe1 = ((DataElementContainer)((DataElementContainer)dom.dataModels[0][1])[0])[0];
			DataElement findMe2 = ((DataElementContainer)dom.dataModels[0][3])[0];

			PeachXPathNavigator navi = new PeachXPathNavigator(dom);
			XPathNodeIterator iter = navi.Select("//FindMe[@isToken=true]");

			Assert.IsTrue(iter.MoveNext());
			Assert.AreEqual(findMe2, ((PeachXPathNavigator)iter.Current).currentNode);
			Assert.IsFalse(iter.MoveNext());
		}

		[Test]
		public void StateModelTest()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number name=\"TheNumber\" size=\"8\">" +
				"			<Relation type=\"count\" of=\"Array\" />" +
				"		</Number>" +
				"		<Block name=\"Block1\">" +
				"			<Block name=\"Block1.1\">" +
				"				<String name=\"FindMe\"/>" +
				"			</Block>" +
				"		</Block>" +
				"		<String name=\"Array\" value=\"1\" maxOccurs=\"100\"/>" +
				"		<Block>" +
				"			<String name=\"FindMe\" token=\"true\"/>" +
				"		</Block>" +
				"	</DataModel>" +
				"	<StateModel name=\"FOo\" initialState=\"State1\">"+
				"		<State name=\"State1\">"+
				"			<Action name=\"Action1\">"+
				"				<DataModel name=\"Action1DataModel\" ref=\"TheDataModel\"/>"+
				"			</Action>"+
				"		</State>"+
				"	</StateModel>"+
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			DataElement findMe1 = ((DataElementContainer)((DataElementContainer)dom.dataModels[0][1])[0])[0];
			DataElement findMe2 = ((DataElementContainer)dom.dataModels[0][3])[0];
			DataElement findMe3 = ((DataElementContainer)((DataElementContainer)dom.stateModels[0].states["State1"].actions[0].dataModel[1])[0])[0];
			DataElement findMe4 = ((DataElementContainer)dom.stateModels[0].states["State1"].actions[0].dataModel[3])[0];

			PeachXPathNavigator navi = new PeachXPathNavigator(dom);
			XPathNodeIterator iter = navi.Select("//FindMe");

			Assert.IsTrue(iter.MoveNext());
			Assert.AreEqual(findMe1, ((PeachXPathNavigator)iter.Current).currentNode);
			Assert.IsTrue(iter.MoveNext());
			Assert.AreEqual(findMe2, ((PeachXPathNavigator)iter.Current).currentNode);
			Assert.IsTrue(iter.MoveNext());
			Assert.AreEqual(findMe3, ((PeachXPathNavigator)iter.Current).currentNode);
			Assert.IsTrue(iter.MoveNext());
			Assert.AreEqual(findMe4, ((PeachXPathNavigator)iter.Current).currentNode);
			Assert.IsFalse(iter.MoveNext());
		}
	}
}

// end
