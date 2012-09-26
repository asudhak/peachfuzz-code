
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

namespace Peach.Core.Test.CrackingTests
{
	[TestFixture]
	public class PlacementTests
	{
		[Test]
		public void BasicAfter()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Block name=\"Block1\">" +
				"			<Blob name=\"Data\">" +
				"				<Placement after=\"Block1\"/>" +
				"			</Blob>" +
				"		</Block>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();

			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("Data", dom.dataModels[0][1].name);
			Assert.AreEqual(ASCIIEncoding.ASCII.GetBytes("Hello World"), (byte[])dom.dataModels[0][1].DefaultValue);
		}

		[Test]
		public void BasicBefore()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Block name=\"Block1\">" +
				"			<Blob name=\"Data\">" +
				"				<Placement before=\"Block1\"/>" +
				"			</Blob>" +
				"		</Block>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();

			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("Data", dom.dataModels[0][0].name);
			Assert.AreEqual(ASCIIEncoding.ASCII.GetBytes("Hello World"), (byte[])dom.dataModels[0][0].DefaultValue);
		}

		[Test]
		public void SameName()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Block name=\"Block1\">" +
				"			<Blob name=\"Data\">" +
				"				<Placement after=\"Block1\"/>" +
				"			</Blob>" +
				"		</Block>" +
				"		<Block name=\"Data\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();

			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("Data_0", dom.dataModels[0][1].name);
			Assert.AreEqual(ASCIIEncoding.ASCII.GetBytes("Hello World"), (byte[])dom.dataModels[0][1].DefaultValue);
		}

		[Test]
		public void RelationTest()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String name=\"TheString\" length=\"2\">" +
				"			<Relation type=\"size\" of=\"Data\"/>" +
				"		</String>" +
				"		<Block name=\"Block1\">" +
				"			<Blob name=\"Data\">" +
				"				<Placement after=\"Block1\"/>" +
				"			</Blob>" +
				"		</Block>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();

			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("11Hello World"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(1, dom.dataModels[0][0].relations.Count);
			Assert.AreEqual("TheDataModel.Data", dom.dataModels[0][0].relations[0].OfName);
			Assert.AreEqual(ASCIIEncoding.ASCII.GetBytes("Hello World"), (byte[])dom.dataModels[0][2].DefaultValue);
		}

		[Test]
		public void FixupTest()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String name=\"TheString\" length=\"11\">" +
				"			<Fixup class=\"CopyValue\">" +
				"				<Param name=\"ref\" value=\"Data\"/>"+
				"			</Fixup>"+
				"		</String>" +
				"		<Block name=\"Block1\">" +
				"			<Blob name=\"Data\">" +
				"				<Placement after=\"Block1\"/>" +
				"			</Blob>" +
				"		</Block>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();

			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("HELLO WORLDHello World"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("TheDataModel.Data", (string)dom.dataModels[0][0].fixup.arguments["ref"]);
			Assert.AreEqual(ASCIIEncoding.ASCII.GetBytes("Hello World"), (byte[])dom.dataModels[0][0].InternalValue);
			Assert.AreEqual(ASCIIEncoding.ASCII.GetBytes("Hello World"), (byte[])dom.dataModels[0][2].DefaultValue);
		}
	}
}

// end
