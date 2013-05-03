
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

namespace Peach.Core.Test.PitParserTests
{
	[TestFixture]
	class ChoiceTests
	{
		[Test]
		public void NumberDefaults()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Choice> "+
				"			<Number name=\"N1\" size=\"8\" endian=\"big\" signed=\"true\"/>" +
				"			<Number name=\"N2\" size=\"8\" endian=\"big\" signed=\"true\"/>" +
				"			<Number name=\"N3\" size=\"8\" endian=\"big\" signed=\"true\"/>" +
				"		</Choice> " +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			Assert.IsTrue(dom.dataModels[0].Count == 1);
			Assert.IsTrue(dom.dataModels[0][0] is Choice);
			Assert.AreEqual(3, ((Choice)dom.dataModels[0][0]).choiceElements.Count);
		}

		[Test]
		public void VerifyParents()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Choice> " +
				"			<Number name=\"N1\" size=\"8\" endian=\"big\" signed=\"true\"/>" +
				"			<Number name=\"N2\" size=\"8\" endian=\"big\" signed=\"true\"/>" +
				"			<Number name=\"N3\" size=\"8\" endian=\"big\" signed=\"true\"/>" +
				"		</Choice> " +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			Assert.IsTrue(dom.dataModels[0].Count == 1);
			var choice = dom.dataModels[0][0] as Choice;
			Assert.NotNull(choice);
			Assert.AreEqual(3, choice.choiceElements.Count);
			Assert.AreEqual(0, choice.Count);
			foreach (var element in choice.choiceElements.Values)
			{
				Assert.NotNull(element.parent);
				Assert.AreEqual(choice, element.parent);
			}
		}

		[Test]
		public void TestOverWrite()
		{
			string xml = @"
<Peach>
	<DataModel name='Base'>
		<Choice name='c'>
			<Block name='b1'>
				<String name='s' value='Hello'/>
			</Block>
			<Block name='b2'>
				<String name='s' value='World'/>
			</Block>
			<Block name='b3'>
				<String name='s' value='!'/>
			</Block>
		</Choice>
	</DataModel>

	<DataModel name='Derived' ref='Base'>
		<String name='c.b1.s' value='World'/>
		<String name='c.b3' value='.'/>
	</DataModel>

</Peach>";

			var parser = new PitParser();
			var dom = parser.asParser(null, new MemoryStream(Encoding.ASCII.GetBytes(xml)));

			Assert.AreEqual(2, dom.dataModels.Count);

			Assert.AreEqual(1, dom.dataModels[0].Count);
			var c1 = dom.dataModels[0][0] as Dom.Choice;
			Assert.NotNull(c1);
			Assert.AreEqual(3, c1.choiceElements.Count);
			var c1_b1 = c1.choiceElements[0] as Dom.Block;
			Assert.NotNull(c1_b1);
			Assert.AreEqual(1, c1_b1.Count);
			Assert.AreEqual("Hello", (string)c1_b1[0].DefaultValue);
			var c1_b2 = c1.choiceElements[1] as Dom.Block;
			Assert.NotNull(c1_b2);
			Assert.AreEqual(1, c1_b2.Count);
			Assert.AreEqual("World", (string)c1_b2[0].DefaultValue);
			var c1_b3 = c1.choiceElements[2] as Dom.Block;
			Assert.NotNull(c1_b3);
			Assert.AreEqual(1, c1_b3.Count);
			Assert.AreEqual("!", (string)c1_b3[0].DefaultValue);

			Assert.AreEqual(1, dom.dataModels[1].Count);
			var c2 = dom.dataModels[1][0] as Dom.Choice;
			Assert.NotNull(c2);
			Assert.AreEqual(3, c2.choiceElements.Count);
			var c2_b1 = c2.choiceElements[0] as Dom.Block;
			Assert.NotNull(c2_b1);
			Assert.AreEqual(1, c2_b1.Count);
			Assert.AreEqual("World", (string)c2_b1[0].DefaultValue);
			var c2_b2 = c2.choiceElements[1] as Dom.Block;
			Assert.NotNull(c2_b2);
			Assert.AreEqual(1, c2_b2.Count);
			Assert.AreEqual("World", (string)c2_b2[0].DefaultValue);
			var c3_b3 = c2.choiceElements[2] as Dom.String;
			Assert.NotNull(c3_b3);
			Assert.AreEqual(".", (string)c3_b3.DefaultValue);

		}

		[Test]
		public void TestAddNewChoice()
		{
			string xml = @"
<Peach>
	<DataModel name='Base'>
		<Choice name='c'>
			<String name='s1' value='Hello'/>
			<String name='s2' value='World'/>
		</Choice>
	</DataModel>

	<DataModel name='Derived' ref='Base'>
		<String name='c.s3' value='Hello'/>
	</DataModel>
</Peach>";

			var parser = new PitParser();
			var dom = parser.asParser(null, new MemoryStream(Encoding.ASCII.GetBytes(xml)));


			Assert.AreEqual(2, dom.dataModels.Count);

			Assert.AreEqual(1, dom.dataModels[0].Count);
			var c1 = dom.dataModels[0][0] as Dom.Choice;
			Assert.AreEqual(2, c1.choiceElements.Count);

			Assert.AreEqual(1, dom.dataModels[1].Count);
			var c2 = dom.dataModels[1][0] as Dom.Choice;
			Assert.AreEqual(3, c2.choiceElements.Count);
		}
	}
}
