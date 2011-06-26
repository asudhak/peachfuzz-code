
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
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Analyzers;
using Peach.Core.IO;

namespace Peach.Core.Test
{
	[TestFixture]
	class RelationSizeTest
	{
		[Test]
		public void BasicTest()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Blob name=\"Data\" value=\"12345\"/>" +
				"		<Number name=\"TheNumber\" size=\"8\">" +
				"			<Relation type=\"size\" of=\"Data\" />" +
				"		</Number>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Number num = dom.dataModels[0][1] as Number;

			Variant val = num.InternalValue;
			Assert.AreEqual(5, (int)val);
		}

		[Test]
		public void BasicTestParent()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Blob name=\"Data\" value=\"12345\"/>" +
				"		<Number name=\"TheNumber\" size=\"8\">" +
				"			<Relation type=\"size\" of=\"TheDataModel\" />" +
				"		</Number>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Number num = dom.dataModels[0][1] as Number;

			Variant val = num.InternalValue;
			Assert.AreEqual(6, (int)val);
		}

		[Test]
		public void RelationInRelation()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Block name=\"TheBlock\">"+
				"			<Blob name=\"Data\" value=\"12345\"/>" +
				"			<Number size=\"8\">" +
				"				<Relation type=\"size\" of=\"Data\" />" +
				"			</Number>" +
				"		</Block>"+
				"		<Number size=\"8\">" +
				"			<Relation type=\"size\" of=\"TheBlock\" />" +
				"		</Number>" +
				"		<Number size=\"8\">" +
				"			<Relation type=\"size\" of=\"TheDataModel\" />" +
				"		</Number>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Number num1 = ((Block)dom.dataModels[0][0])[1] as Number;
			Number num2 = dom.dataModels[0][1] as Number;
			Number num3 = dom.dataModels[0][2] as Number;

			Assert.AreEqual(5, (int)num1.InternalValue);
			Assert.AreEqual(6, (int)num2.InternalValue);
			Assert.AreEqual(8, (int)num3.InternalValue);
		}
	}
}

// end
