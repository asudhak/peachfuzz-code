
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
	public class ArrayTests
	{

		[Test]
		public void CrackUrl()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String />" +
				"		<String value=\"://\" token=\"true\" />" +
				"		<String />" +
				"		<String value=\"/\" token=\"true\" />" +
				"		<String />" +
				"		<String value=\"?\" token=\"true\" />" +

				"		<Block name=\"TheArray\" minOccurs=\"0\" maxOccurs=\"100\">" +
				"		  <String name=\"key1\" />" +
				"		  <String value=\"=\" token=\"true\" />" +
				"		  <String name=\"value1\" />" +
				"			<String value=\"&amp;\" token=\"true\" />" +
				"		</Block>" +

				"		<Block name=\"EndBlock\">" +
				"		  <String name=\"key2\" />" +
				"		  <String value=\"=\" token=\"true\" />" +
				"		  <String name=\"value2\" />" +
				"		</Block>" +
				"	</DataModel>" +
				"</Peach>";

			// Positive test

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("http://www.foo.com/crazy/path.cgi?k1=v1&k2=v2&k3=v3"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(2, ((Dom.Array)dom.dataModels[0]["TheArray"]).Count);
			Assert.AreEqual("k3", (string)((Dom.Block)dom.dataModels[0]["EndBlock"])["key2"].InternalValue);
			Assert.AreEqual("v3", (string)((Dom.Block)dom.dataModels[0]["EndBlock"])["value2"].InternalValue);
		}

		[Test]
		public void CrackUrl2()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String value=\"?\" token=\"true\" />" +

				"		<Block name=\"TheArray\" minOccurs=\"0\" maxOccurs=\"100\">" +
				"		  <String name=\"key1\" />" +
				"		  <String value=\"=\" token=\"true\" />" +
				"		  <String name=\"value1\" />" +
				"			<String value=\"&amp;\" token=\"true\" />" +
				"		</Block>" +
				"		<String name=\"key2\" />" +
				"		<String value=\"=\" token=\"true\" />" +
				"		<String name=\"value2\" />" +
				"	</DataModel>" +
				"</Peach>";

			// Positive test

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("?k1=v1&k2=v2&k3=v3"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(2, ((Dom.Array)dom.dataModels[0]["TheArray"]).Count);
			Assert.AreEqual("k3", (string)dom.dataModels[0]["key2"].InternalValue);
			Assert.AreEqual("v3", (string)dom.dataModels[0]["value2"].InternalValue);
		}

		[Test]
		public void CrackArrayBlob1()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Blob length=\"1\" minOccurs=\"1\" maxOccurs=\"100\" />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(new byte[] { 1, 2, 3, 4, 5, 6 });
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Dom.Array array = (Dom.Array)dom.dataModels[0][0];

			Assert.AreEqual(6, array.Count);
			Assert.AreEqual(new byte[] { 1 }, (byte[])array[0].InternalValue);
			Assert.AreEqual(new byte[] { 6 }, (byte[])array[5].InternalValue);
		}

		/// <summary>
		/// We should stop cracking at maxOccurs.  Question, should we throw an exception or just
		/// stop?
		/// </summary>
		[Test]
		public void CrackArrayStopAtMax()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Blob length=\"1\" minOccurs=\"1\" maxOccurs=\"3\" />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(new byte[] { 1, 2, 3, 4, 5, 6 });
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Dom.Array array = (Dom.Array)dom.dataModels[0][0];

			Assert.AreEqual(3, array.Count);
			Assert.AreEqual(new byte[] { 1 }, (byte[])array[0].InternalValue);
			Assert.AreEqual(new byte[] { 2 }, (byte[])array[1].InternalValue);
			Assert.AreEqual(new byte[] { 3 }, (byte[])array[2].InternalValue);
		}

		[Test]
		public void CrackArrayVerifyMin()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Blob length=\"1\" minOccurs=\"4\" maxOccurs=\"6\" />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(new byte[] { 1, 2, 3 });
			data.SeekBits(0, SeekOrigin.Begin);

			try
			{
				DataCracker cracker = new DataCracker();
				cracker.CrackData(dom.dataModels[0], data);
				Assert.True(false);
			}
			catch (CrackingFailure)
			{
				Assert.True(true);
			}
		}

		[Test]
		public void CrackArrayBlobZeroMore()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Blob length=\"1\" minOccurs=\"0\" maxOccurs=\"100\" />" +
				"		<Blob name=\"Rest\" length=\"6\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(new byte[] { 1, 2, 3, 4, 5, 6 });
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Dom.Array array = (Dom.Array)dom.dataModels[0][0];

			Assert.AreEqual(0, array.Count);

			Blob rest = (Blob)dom.dataModels[0].find("Rest");

			Assert.AreEqual(new byte[] { 1, 2, 3, 4, 5, 6 }, (byte[])rest.InternalValue);
		}

		[Test]
		public void CrackZeroArray()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String value=\"Item\" minOccurs=\"0\" token=\"true\" />" +
				"		<Blob name=\"Rest\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(new byte[] { 1, 2, 3, 4, 5, 6 });
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Dom.Array array = (Dom.Array)dom.dataModels[0][0];

			Assert.AreEqual(0, array.Count);

			Blob rest = (Blob)dom.dataModels[0].find("Rest");

			Assert.AreEqual(new byte[] { 1, 2, 3, 4, 5, 6 }, (byte[])rest.InternalValue);
		}

		[Test]
		public void CrackArrayRelation()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number size=\"8\">" +
				"			<Relation type=\"count\" of=\"TheArray\"/>" +
				"		</Number>" +
				"		<Blob name=\"TheArray\" length=\"1\" minOccurs=\"0\" maxOccurs=\"100\" />" +
				"		<Blob name=\"Rest\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(new byte[] { 3, 1, 2, 3, 4, 5, 6 });
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Dom.Array array = (Dom.Array)dom.dataModels[0][1];
			Blob blob = (Blob)dom.dataModels[0][2];

			Assert.AreEqual(3, array.Count);
			Assert.AreEqual(new byte[] { 1 }, (byte[])array[0].InternalValue);
			Assert.AreEqual(new byte[] { 2 }, (byte[])array[1].InternalValue);
			Assert.AreEqual(new byte[] { 3 }, (byte[])array[2].InternalValue);
			Assert.AreEqual(new byte[] { 4, 5, 6 }, (byte[])blob.InternalValue);
		}

		[Test]
		public void CrackArrayOfOne()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String name=\"str1\" nullTerminated=\"true\" minOccurs=\"1\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(Encoding.ASCII.GetBytes("Hello\x00"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(1, dom.dataModels[0].Count);
			Dom.Array array = (Dom.Array)dom.dataModels[0][0];
			Assert.AreEqual(1, array.Count);
			string str = (string)array[0].InternalValue;

			Assert.AreEqual("Hello", str);
		}

		[Test]
		public void CrackArrayOfZeroOrOne()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String name=\"str1\" nullTerminated=\"true\" minOccurs=\"0\" maxOccurs=\"1\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(Encoding.ASCII.GetBytes("Hello\x00"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(1, dom.dataModels[0].Count);
			Dom.Array array = (Dom.Array)dom.dataModels[0][0];
			Assert.AreEqual(1, array.Count);
			string str = (string)array[0].InternalValue;

			Assert.AreEqual("Hello", str);
		}

		[Test]
		public void CrackArrayWithinArray()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Block occurs=\"3\">" +
				"			<Number name=\"num1\" size=\"8\" minOccurs=\"0\" constraint=\"str(element.DefaultValue) != '0'\" />" +
				"			<Number name=\"num2\" size=\"8\" valueType=\"hex\" value=\"0\" />" +
				"		</Block>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(new byte[] { 0x01, 0x02, 0x03, 0x00, 0x04, 0x00, 0x00});
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(1, dom.dataModels[0].Count);
			Dom.Array blockArray = dom.dataModels[0][0] as Dom.Array;
			Assert.NotNull(blockArray);
			Assert.AreEqual(3, blockArray.Count);
			Block firstBlock = blockArray[0] as Block;
			Assert.NotNull(firstBlock);
			Dom.Array numArray = firstBlock[0] as Dom.Array;
			Assert.NotNull(numArray);
			Assert.AreEqual(3, numArray.Count);
		}

		[Test]
		public void CrackArrayWithTokenSibling()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Block minOccurs=\"0\">" +
				"			<Number name=\"num1\" size=\"8\" constraint=\"str(element.DefaultValue) != '0'\" />" +
				"		</Block>" +
				"		<Number name=\"zero\" size=\"8\" valueType=\"hex\" value=\"0\" token=\"true\" />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(new byte[] { 0x01, 0x02, 0x03, 0x00});
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(2, dom.dataModels[0].Count);
			Dom.Array blockArray = dom.dataModels[0][0] as Dom.Array;
			Assert.NotNull(blockArray);
			Assert.AreEqual(3, blockArray.Count);

		}

		[Test]
		public void CrackArrayParentName()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String name=\"str1\" nullTerminated=\"true\" minOccurs=\"1\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(Encoding.ASCII.GetBytes("Hello\x00World\x00"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(1, dom.dataModels[0].Count);
			Dom.Array array = (Dom.Array)dom.dataModels[0][0];
			Assert.AreEqual(2, array.Count);
			Assert.AreEqual("TheDataModel.str1", array.fullName);
			Assert.AreEqual("TheDataModel.str1.str1", array.origionalElement.fullName);
			Assert.AreEqual("TheDataModel.str1.str1", array[0].fullName);
			Assert.AreEqual("TheDataModel.str1.str1_1", array[1].fullName);
		}

		[Test]
		public void CrackArrayEmptyElement()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Block minOccurs=\"10\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.WriteByte(0);
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(1, dom.dataModels[0].Count);
			Dom.Array array = (Dom.Array)dom.dataModels[0][0];
			Assert.AreEqual(10, array.Count);
		}

		[Test]
		public void CrackArrayEmptyElementMin()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Block minOccurs=\"0\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.WriteByte(0);
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(1, dom.dataModels[0].Count);
			Dom.Array array = (Dom.Array)dom.dataModels[0][0];
			Assert.AreEqual(0, array.Count);
		}

		[Test]
		public void TokenAfterArray()
		{
			string xml = @"
<Peach>
	<DataModel name=""DM"">
		<Block name=""A"">
			<Block name=""block1"" minOccurs=""0"">
				<Number size=""8"" value=""11"" token=""true""/> 
				<Number size=""8"" constraint=""str(element.DefaultValue) != '0'""/> 
			</Block>
			<Block name=""block2"">
				<Number size=""8"" value=""11"" token=""true""/> 
			</Block>
			<Number name=""end"" size=""8"" value=""0""/>
		</Block>
	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(new byte[] { 11, 1, 11, 2, 11, 3, 11, 0 });
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(1, dom.dataModels[0].Count);
			var block = dom.dataModels[0][0] as Dom.Block;
			Assert.NotNull(block);
			Assert.AreEqual(3, block.Count);
			var array = block[0] as Dom.Array;
			Assert.NotNull(array);
			Assert.AreEqual(3, array.Count);
		}

		[Test]
		public void TokenAfterArrayInArray()
		{
			string xml = @"
<Peach>
	<Import import='re'/>

	<DataModel name='TupleLine'>
		<String name='LineHeader' />
		<Block name='ValueTuples' minOccurs='0' maxOccurs='5'>
			<String name='CommaSeparator' value=',' token='true' />
			<String name='TupleValue' constraint='re.search(""^[A-Za-z0-9]+$"", value) != None' />
		</Block>
		<String name='LineTerminator' value='\n' token='true' />
	</DataModel>

	<DataModel name='TheDataModel'>
		<Block name='FirstLine' ref='TupleLine'>
			<String name='LineHeader' value='CIRCLE' token='true' />
		</Block>

		<Block name='SecondLine' ref='TupleLine'>
			<String name='LineHeader' value='RECT' token='true' />
		</Block>
	</DataModel>
</Peach>
";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(Encoding.ASCII.GetBytes("CIRCLE,0,50\nRECT,0\n"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[1], data);

			Assert.AreEqual(2, dom.dataModels[1].Count);

			var b1 = dom.dataModels[1][0] as Block;
			Assert.NotNull(b1);
			Assert.AreEqual(3, b1.Count);
			var b1_array = b1[1] as Dom.Array;
			Assert.AreEqual(2, b1_array.Count);

			var b2 = dom.dataModels[1][1] as Block;
			Assert.NotNull(b2);
			Assert.AreEqual(3, b2.Count);
			var b2_array = b2[1] as Dom.Array;
			Assert.AreEqual(1, b2_array.Count);
		}
	}
}

// end
