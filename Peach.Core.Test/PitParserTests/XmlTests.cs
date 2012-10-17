
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
using System.Xml;
using System.Xml.XPath;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Dom.XPath;
using Peach.Core.Analyzers;

namespace Peach.Core.Test.PitParserTests
{
	[TestFixture]
	class XmlTests
	{
        //[Test]
        //public void NumberDefaults()
        //{
        //    string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
        //        "	<Defaults>" +
        //        "		<Number size=\"8\" endian=\"big\" signed=\"true\"/>" +
        //        "	</Defaults>" +
        //        "	<DataModel name=\"TheDataModel\">" +
        //        "		<Number name=\"TheNumber\" size=\"8\"/>" +
        //        "	</DataModel>" +
        //        "</Peach>";

        //    PitParser parser = new PitParser();
        //    Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
        //    Number num = dom.dataModels[0][0] as Number;

        //    Assert.IsTrue(num.Signed);
        //    Assert.IsFalse(num.LittleEndian);
        //}

        [Test]
        public void BasicXmlElement()
        {
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
                "	<DataModel name=\"TheDataModel\">" +
                "		<XmlElement elementName=\"Foo\">"+
                "           <XmlAttribute attributeName=\"bar\">"+
                "               <String value=\"attribute value\"/> "+
                "           </XmlAttribute>"+
                "		    <XmlElement elementName=\"ChildElement\">" +
                "               <XmlAttribute attributeName=\"name\">" +
                "                   <String value=\"attribute value\"/> " +
                "               </XmlAttribute>" +
                "           </XmlElement>" +
                "       </XmlElement>" +
                "	</DataModel>" +
                "</Peach>";

            PitParser parser = new PitParser();
            Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
            var elem = dom.dataModels[0][0];

            Assert.NotNull(elem);
            Assert.IsTrue(elem is Dom.XmlElement);
            Assert.AreEqual(2, ((Dom.XmlElement)elem).Count);
        }

        [Test]
		public void SimpleXPath()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
				"<Peach>" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number name=\"TheNumber\" size=\"8\"/>" +
				"	</DataModel>" +
				"</Peach>";
		
			XPathDocument doc = new XPathDocument(new MemoryStream(Encoding.ASCII.GetBytes(xml)));
			XPathNavigator nav = doc.CreateNavigator();
			XPathNodeIterator it = nav.Select("//Number");
			
			List<string> res = new List<string>();
			while (it.MoveNext())
			{
				var val = it.Current.GetAttribute("name", "");
				res.Add(val);
			}
			
			Assert.AreEqual(1, res.Count);
			Assert.AreEqual("TheNumber", res[0]);
		}

		[Test]
		public void PeachXPath()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
				"<Peach>" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number name=\"TheNumber\" size=\"8\"/>" +
				"	</DataModel>" +

				"   <StateModel name=\"TheStateModel\" initialState=\"InitialState\">" +
				"       <State name=\"InitialState\">" +
				"           <Action name=\"Action1\" type=\"output\">" +
				"               <DataModel ref=\"TheDataModel\"/>" +
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
			
			PeachXPathNavigator nav = new PeachXPathNavigator(dom);
			XPathNodeIterator it = nav.Select("//TheNumber");
			
			List<string> res = new List<string>();
			
			while (it.MoveNext())
			{
				var val = it.Current.Name;
				res.Add(val);
			}
			
			Assert.AreEqual(1, res.Count);
			Assert.AreEqual("TheNumber", res[0]);
		}

		[Test]
		public void PeachXPath2()
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
				"   </Test>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			
			PeachXPathNavigator nav = new PeachXPathNavigator(dom);
			XPathNodeIterator it = nav.Select("//String1");
			
			// Should find one element
			bool ret1 = it.MoveNext();
			Assert.True(ret1);
			
			// The result should be a DataElement
			DataElement valueElement = ((PeachXPathNavigator)it.Current).currentNode as DataElement;
			Assert.NotNull(valueElement);
			
			// There sould on;ly be one result
			bool ret2 = it.MoveNext();
			Assert.False(ret2);
		}
		
	}
}
