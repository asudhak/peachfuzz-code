
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
	public class BlockTests
	{
		[Test]
		public void CrackBlock1()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Block>" +
				"			<Block>" +
				"				<Block>" +
				"				</Block>" +
				"			</Block>" +
				"		</Block>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(new byte[] { 1, 2, 3, 4, 5 });
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(0, data.TellBits());
		}

		[Test]
		public void CrackBlock2()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Block>" +
				"			<Block>" +
				"				<Block>" +
				"					<String name=\"FooString\" length=\"12\" />"+
				"				</Block>" +
				"			</Block>" +
				"		</Block>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World!"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("Hello World!", (string)dom.dataModels[0].find("FooString").DefaultValue);
		}

		[Test]
		public void CrackBlock3()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Block name=\"b1\">" +
				"			<String name=\"str1\" length=\"5\" />" +
				"			<String name=\"str2\" length=\"5\" />" +
				"		</Block>" +
				"		<Block name=\"b2\">" +
				"			<String name=\"str3\" length=\"1\"/>" +
				"		</Block>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("HelloWorld!........................."));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);


			Assert.AreEqual("Hello", (string)dom.dataModels[0].find("str1").DefaultValue);
			Assert.AreEqual("World", (string)dom.dataModels[0].find("str2").DefaultValue);
			Assert.AreEqual("!", (string)dom.dataModels[0].find("str3").DefaultValue);
		}

		[Test]
		public void CrackBlock4()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Block name=\"b1\" occurs=\"2\">" +
				"			<String name=\"str1\" length=\"5\" />" +
				"			<String name=\"str2\" length=\"5\" />" +
				"		</Block>" +
				"		<Block name=\"b2\">" +
				"			<String name=\"str3\" length=\"1\"/>" +
				"		</Block>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("HelloWorldHelloWorld!......................................................"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);


			Assert.AreEqual("Hello", (string)dom.dataModels[0].find("b1.str1").DefaultValue);
			Assert.AreEqual("World", (string)dom.dataModels[0].find("b1.str2").DefaultValue);
			Assert.AreEqual("Hello", (string)dom.dataModels[0].find("b1_1.str1").DefaultValue);
			Assert.AreEqual("World", (string)dom.dataModels[0].find("b1_1.str2").DefaultValue);
			Assert.AreEqual("!", (string)dom.dataModels[0].find("str3").DefaultValue);
		}

		[Test]
		public void CrackBlock5()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number size=\"8\">" +
				"			<Relation type=\"count\" of=\"b1\"/>" +
				"		</Number>" +
				"		<Block name=\"b1\" minOccurs=\"1\">" +
				"			<String name=\"str1\" length=\"5\" />" +
				"			<String name=\"str2\" length=\"5\" />" +
				"		</Block>" +
				"		<Block name=\"b2\">" +
				"			<String name=\"str3\" length=\"1\"/>" +
				"		</Block>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("\x02HelloWorldHelloWorld!......................................................"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);


			Assert.AreEqual("Hello", (string)dom.dataModels[0].find("b1.str1").DefaultValue);
			Assert.AreEqual("World", (string)dom.dataModels[0].find("b1.str2").DefaultValue);
			Assert.AreEqual("Hello", (string)dom.dataModels[0].find("b1_1.str1").DefaultValue);
			Assert.AreEqual("World", (string)dom.dataModels[0].find("b1_1.str2").DefaultValue);
			Assert.AreEqual("!", (string)dom.dataModels[0].find("str3").DefaultValue);
		}

		[Test]
		public void CrackBlock6()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number size=\"8\">" +
				"			<Relation type=\"count\" of=\"b1\"/>" +
				"		</Number>" +
				"		<String name=\"unsized\"/>" +
				"		<Block name=\"b1\" minOccurs=\"1\">" +
				"			<String name=\"str1\" length=\"5\" />" +
				"			<String name=\"str2\" length=\"5\" />" +
				"		</Block>" +
				"		<Block name=\"b2\">" +
				"			<String name=\"str3\" length=\"1\"/>" +
				"		</Block>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("\x02.....HelloWorldHelloWorld!"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);


			Assert.AreEqual(".....", (string)dom.dataModels[0].find("unsized").DefaultValue);
			Assert.AreEqual("Hello", (string)dom.dataModels[0].find("b1.str1").DefaultValue);
			Assert.AreEqual("World", (string)dom.dataModels[0].find("b1.str2").DefaultValue);
			Assert.AreEqual("Hello", (string)dom.dataModels[0].find("b1_1.str1").DefaultValue);
			Assert.AreEqual("World", (string)dom.dataModels[0].find("b1_1.str2").DefaultValue);
			Assert.AreEqual("!", (string)dom.dataModels[0].find("str3").DefaultValue);
		}

		[Test]
		public void CrackBlock7()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number size=\"8\">" +
				"			<Relation type=\"size\" of=\"b1\"/>" +
				"		</Number>" +
				"		<Block name=\"b1\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(new byte[] { 2, 0, 0 });
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			// Ensure we actually advance the BitStream over sized blocks with no children
			Assert.AreEqual(3, data.TellBytes());
			Assert.AreEqual(24, data.TellBits());
		}

		[Test]
		public void CrackBlock8()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
			"			<Number name=\"n1\" size=\"8\">" +
			"				<Relation type=\"size\" of=\"b1\"/>" +
			"			</Number>" +
			"			<Block name=\"b1\">" +
			"				<Number name=\"n2\" size=\"8\">" +
			"					<Relation type=\"size\" of=\"b2\"/>" +
			"				</Number>" +
				"			<Block name=\"b2\">" +
				"				<Number name=\"n3\" size=\"8\"/>" +
				"			</Block>" +
				"		</Block>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(new byte[] { 8, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
			data.SeekBits(0, SeekOrigin.Begin);

			Dictionary<string, string> place = new Dictionary<string, string>();

			DataCracker cracker = new DataCracker();

			cracker.EnterHandleNodeEvent += new EnterHandleNodeEventHandler(delegate(DataElement de, long pos, BitStream bs)
			{
				place.Add(de.fullName, pos.ToString());
			});

			cracker.ExitHandleNodeEvent += new ExitHandleNodeEventHandler(delegate(DataElement de, long pos, BitStream bs)
			{
				place[de.fullName] += "," + pos.ToString();
			});

			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(6, place.Count);
			Assert.AreEqual("0,72", place["TheDataModel"]);
			Assert.AreEqual("0,8", place["TheDataModel.n1"]);
			Assert.AreEqual("8,72", place["TheDataModel.b1"]);
			Assert.AreEqual("8,16", place["TheDataModel.b1.n2"]);
			Assert.AreEqual("16,48", place["TheDataModel.b1.b2"]);
			Assert.AreEqual("16,24", place["TheDataModel.b1.b2.n3"]);

		}
	}
}
