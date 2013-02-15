
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
	class RefTests
	{
		[Test]
		public void BasicTest()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel1\">" +
				"		<String name=\"Str1\" />" +
				"		<String name=\"Str2\" />" +
				"		<String name=\"Str3\" />" +
				"	</DataModel>" +
				"	<DataModel name=\"TheDataModel2\" ref=\"TheDataModel1\">" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			Assert.AreEqual(3, dom.dataModels[0].Count);
			Assert.AreEqual(3, dom.dataModels[1].Count);
			Assert.AreEqual("Str1", dom.dataModels[1][0].name);
			Assert.AreEqual("Str2", dom.dataModels[1][1].name);
			Assert.AreEqual("Str3", dom.dataModels[1][2].name);
		}

		[Test]
		public void BasicTest2()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel1\">" +
				"		<String name=\"Str1\" />" +
				"		<String name=\"Str2\" />" +
				"		<String name=\"Str3\" />" +
				"	</DataModel>" +
				"	<DataModel name=\"TheDataModel2\" ref=\"TheDataModel1\">" +
				"		<String name=\"Str4\" />" +
				"		<String name=\"Str5\" />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			Assert.AreEqual(3, dom.dataModels[0].Count);
			Assert.AreEqual(5, dom.dataModels[1].Count);
			Assert.AreEqual("Str1", dom.dataModels[1][0].name);
			Assert.AreEqual("Str2", dom.dataModels[1][1].name);
			Assert.AreEqual("Str3", dom.dataModels[1][2].name);
			Assert.AreEqual("Str4", dom.dataModels[1][3].name);
			Assert.AreEqual("Str5", dom.dataModels[1][4].name);
		}

		[Test]
		public void BasicTest3()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel2\">" +
				"		<Block name=\"Block1\">" +
				"			<String name=\"Str1\" />" +
				"			<String name=\"Str2\" />" +
				"			<String name=\"Str3\" />" +
				"		</Block>" +
				"		<Block ref=\"Block1\">" +
				"		</Block>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			Assert.AreEqual(3, ((Block)dom.dataModels[0][0]).Count);
			Assert.AreEqual(3, ((Block)dom.dataModels[0][1]).Count);
			Assert.AreEqual("Str1", ((Block)dom.dataModels[0][1])[0].name);
			Assert.AreEqual("Str2", ((Block)dom.dataModels[0][1])[1].name);
			Assert.AreEqual("Str3", ((Block)dom.dataModels[0][1])[2].name);
		}

		[Test]
		public void BasicTest4()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Block name=\"Block1\">" +
				"			<String name=\"Str1\" />" +
				"			<String name=\"Str2\" />" +
				"			<String name=\"Str3\" />" +
				"		</Block>" +
				"		<Block name=\"Block2\" ref=\"Block1\">" +
				"			<String name=\"Str4\" />" +
				"			<String name=\"Str5\" />" +
				"		</Block>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			Assert.AreEqual(3, ((Block)dom.dataModels[0][0]).Count);
			Assert.AreEqual(5, ((Block)dom.dataModels[0][1]).Count);
			Assert.AreEqual("Str1", ((Block)dom.dataModels[0][1])[0].name);
			Assert.AreEqual("Str2", ((Block)dom.dataModels[0][1])[1].name);
			Assert.AreEqual("Str3", ((Block)dom.dataModels[0][1])[2].name);
			Assert.AreEqual("Str4", ((Block)dom.dataModels[0][1])[3].name);
			Assert.AreEqual("Str5", ((Block)dom.dataModels[0][1])[4].name);
		}

		[Test]
		public void BasicTestReplace1()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Block name=\"Block1\">" +
				"			<String name=\"Str1\" />" +
				"			<String name=\"Str2\" />" +
				"			<String name=\"Str3\" />" +
				"		</Block>" +
				"		<Block name=\"Block2\" ref=\"Block1\">" +
				"			<String name=\"Str1\" />" +
				"			<String name=\"Str2\" />" +
				"			<String name=\"Str3\" />" +
				"		</Block>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			Assert.AreEqual(3, ((Block)dom.dataModels[0][0]).Count);
			Assert.AreEqual(3, ((Block)dom.dataModels[0][1]).Count);
			Assert.AreEqual("Str1", ((Block)dom.dataModels[0][1])[0].name);
			Assert.AreEqual("Str2", ((Block)dom.dataModels[0][1])[1].name);
			Assert.AreEqual("Str3", ((Block)dom.dataModels[0][1])[2].name);
		}

		[Test]
		public void BasicTestReplace2()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
				"<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Block name=\"Block1\">" +
				"			<Block name=\"Block1a\">" +
				"				<String name=\"Str1\" />" +
				"				<String name=\"Str2\" />" +
				"			</Block>" +
				"			<String name=\"Str3\" />" +
				"		</Block>" +
				"		<Block name=\"Block2\" ref=\"Block1\">" +
			"				<String name=\"Block1a.Str1\" />" +
			"				<String name=\"Block1a.Str2\" />" +
				"			<String name=\"Str3\" />" +
				"		</Block>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			Assert.AreEqual(2, ((Block)dom.dataModels[0][0]).Count);
			Assert.AreEqual(2, ((Block)dom.dataModels[0][1]).Count);
			Assert.AreEqual(2, ((Block)((Block)dom.dataModels[0][1])[0]).Count);
			Assert.AreEqual("Str1", ((Block)((Block)dom.dataModels[0][1])[0])[0].name);
			Assert.AreEqual("Str2", ((Block)((Block)dom.dataModels[0][1])[0])[1].name);
			Assert.AreEqual("Str3", ((Block)dom.dataModels[0][1])[1].name);
		}

		[Test]
		public void BlockTest1()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel1\">" +
				"		<String name=\"Str1\" />" +
				"		<String name=\"Str2\" />" +
				"		<String name=\"Str3\" />" +
				"	</DataModel>" +
				"	<DataModel name=\"TheDataModel2\">" +
				"		<Block name=\"TheBlock1\" ref=\"TheDataModel1\" />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			Assert.AreEqual(3, dom.dataModels[0].Count);
			Assert.AreEqual(1, dom.dataModels[1].Count);
			Assert.AreEqual(3, ((Block)dom.dataModels[1][0]).Count);
			Assert.AreEqual("Str1", ((Block)dom.dataModels[1][0])[0].name);
			Assert.AreEqual("Str2", ((Block)dom.dataModels[1][0])[1].name);
			Assert.AreEqual("Str3", ((Block)dom.dataModels[1][0])[2].name);
		}

		[Test]
		public void BlockMinMaxTest1()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel1\">" +
				"		<String name=\"Str1\" />" +
				"		<String name=\"Str2\" />" +
				"		<String name=\"Str3\" />" +
				"	</DataModel>" +
				"	<DataModel name=\"TheDataModel2\">" +
				"		<Block name=\"TheBlock1\" minOccurs=\"0\" maxOccurs=\"1\" ref=\"TheDataModel1\" />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			Assert.AreEqual(3, dom.dataModels[0].Count);
			Assert.AreEqual(1, dom.dataModels[1].Count);
			Peach.Core.Dom.Array BlockArray = dom.dataModels[1][0] as Peach.Core.Dom.Array;
			Assert.NotNull(BlockArray);

			Block ReferencedBlock = ((Block)BlockArray.origionalElement);
			Assert.AreEqual(3, ReferencedBlock.Count);
			Assert.AreEqual("Str1", ReferencedBlock[0].name);
			Assert.AreEqual("Str2", ReferencedBlock[1].name);
			Assert.AreEqual("Str3", ReferencedBlock[2].name);
		}

	}
}

// end
