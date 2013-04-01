
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
	public class SizeRelationTests
	{
		[Test]
		public void CrackSizeOf1()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number size=\"8\">" +
				"			<Relation type=\"size\" of=\"Data\" />" +
				"		</Number>" +
				"		<Blob name=\"Data\" />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.WriteInt8((sbyte)"Hello World".Length);
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World"));
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("AAAAAAAAAAA"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("Hello World".Length, (int)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual(ASCIIEncoding.ASCII.GetBytes("Hello World"), (byte[])dom.dataModels[0][1].DefaultValue);
		}

		[Test]
		public void CrackSizeOf2()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number size=\"8\">" +
				"			<Relation type=\"size\" of=\"TheDataModel\" />" +
				"		</Number>" +
				"		<Blob name=\"Data\" />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();

			data.WriteInt8((sbyte)("Hello World".Length+1));
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World"));
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("AAAAAAAAAAA"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("Hello World".Length + 1, (int)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual(ASCIIEncoding.ASCII.GetBytes("Hello World"), (byte[])dom.dataModels[0][1].DefaultValue);
		}

		[Test]
		public void CrackSizeOf3()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number size=\"8\">" +
				"			<Relation type=\"size\" of=\"Data\" />" +
				"		</Number>" +
				"		<Block name=\"Data\">" +
				"			<Blob />" +
				"		</Block>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();

			data.WriteInt8((sbyte)"Hello World".Length);
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World"));
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("AAAAAAAAAAA"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("Hello World".Length, (int)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual(ASCIIEncoding.ASCII.GetBytes("Hello World"), (byte[])((DataElementContainer)dom.dataModels[0][1])[0].DefaultValue);
		}

		[Test]
		public void CrackSizeOf4()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number size=\"8\">" +
				"			<Relation type=\"size\" of=\"Data\" expressionGet=\"size/2\" />" +
				"		</Number>" +
				"		<Blob name=\"Data\" />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();

			data.WriteInt8((sbyte) ("Hello World".Length * 2));
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World"));
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("AAAAAAAAAAA"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("Hello World".Length*2, (int)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual(ASCIIEncoding.ASCII.GetBytes("Hello World"), (byte[])dom.dataModels[0][1].DefaultValue);
		}

		[Test]
		public void CrackSizeOf5()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number size=\"8\">" +
				"			<Relation type=\"size\" of=\"Data\" />" +
				"		</Number>" +
				"		<Block name=\"Data\">" +
				"			<Blob name=\"inner\"/>" +
				"		</Block>" +
				"		<Blob name=\"outer\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();

			data.WriteInt8((sbyte)"Hello World".Length);
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World"));
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("ABCDEFG"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("Hello World".Length, (int)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual(ASCIIEncoding.ASCII.GetBytes("Hello World"), (byte[])((DataElementContainer)dom.dataModels[0][1])[0].DefaultValue);
			Assert.AreEqual(ASCIIEncoding.ASCII.GetBytes("ABCDEFG"), (byte[])dom.dataModels[0][2].DefaultValue);
		}

		[Test]
		public void CrackSizeOf6()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number size=\"8\">" +
				"			<Relation type=\"size\" of=\"Second\" />" +
				"		</Number>" +
				"		<String name=\"First\"/>" +
				"		<String name=\"Second\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();

			data.WriteInt8((sbyte)"Hello World".Length);
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("ABCDEFG"));
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("Hello World".Length, (int)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual("ABCDEFG", (string)dom.dataModels[0][1].DefaultValue);
			Assert.AreEqual("Hello World", (string)dom.dataModels[0][2].DefaultValue);
		}

		[Test]
		public void CrackSizeOf7()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number size=\"8\">" +
				"			<Relation type=\"size\" of=\"FirstBlock\" />" +
				"		</Number>" +
				"		<Block name=\"FirstBlock\">" +
				"			<Blob name=\"First\"/>" +
				"		</Block>" +
				"		<String name=\"Second\" value=\"ABCDEFG\" token=\"true\" />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();

			data.WriteInt8((sbyte)"Hello World".Length);
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World"));
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("ABCDEFG"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("Hello World".Length, (int)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual(ASCIIEncoding.ASCII.GetBytes("Hello World"), (byte[])((Block)dom.dataModels[0][1])[0].DefaultValue);
			Assert.AreEqual("ABCDEFG", (string)dom.dataModels[0][2].DefaultValue);
		}

		[Test]
		public void CrackSizeOf8()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number size=\"8\">" +
				"			<Relation type=\"size\" of=\"Second\" />" +
				"		</Number>" +
				"		<String name=\"First\"/>" +
				"		<Block>" +
				"			<String name=\"Second\"/>" +
				"		</Block>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();

			data.WriteInt8((sbyte)"Hello World".Length);
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("ABCDEFG"));
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("Hello World".Length, (int)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual("ABCDEFG", (string)dom.dataModels[0][1].DefaultValue);
			Assert.AreEqual("Hello World", (string)((Dom.Block)dom.dataModels[0][2])[0].DefaultValue);
		}

		[Test]
		public void CrackSizeOfBlockReference()
		{
			string xml = @"
<Peach>
	<DataModel name=""Base"">
		<Number size=""8"" name=""blocksize"">
			<Relation type=""size"" of=""smallData"" />
		</Number>
		<Blob name=""smallData""/>
	</DataModel>

	<DataModel name=""DM"">
		<Blob name=""Header"" length=""1""/>
		<Block name=""Base1"" ref=""Base"" />
		<Blob name=""Footer"" valueType=""hex"" value=""0"" length=""1"" token=""true"" />
	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(new byte[] { 0x01, 0x02, 0x33, 0x44, 0x00 });
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[1], data);

			Assert.AreEqual(3, dom.dataModels[1].Count);
			Assert.AreEqual(new byte[] { 0x01 }, dom.dataModels[1][0].Value.Value);
			Assert.AreEqual(new byte[] { 0x02 }, ((Block)dom.dataModels[1][1])[0].Value.Value);
			Assert.AreEqual(new byte[] { 0x33, 0x44 }, ((Block)dom.dataModels[1][1])[1].Value.Value);
			Assert.AreEqual(new byte[] { 0x00 }, dom.dataModels[1][2].Value.Value);
		}

		[Test]
		public void CrackSizeParent()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
			"	<DataModel name=\"TheDataModel\">" +
			"      <Block name=\"10\">" +
			"         <Number name=\"14\" signed=\"false\" size=\"32\"/>" +
			"         <Number name=\"15\" signed=\"false\" size=\"32\">" +
			"           <Relation type=\"size\" of=\"10\"/>" +
			"         </Number>" +
			"         <Blob name='blob'/>" +
			"      </Block>" +
			"	</DataModel>" +
			"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();

			data.WriteBytes(new byte[] { 0x03, 0xf3, 0x0d, 0x0a, 0x0a, 0x00, 0x00, 0x00, 0xff, 0xff });
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			var elem = dom.dataModels[0].find("TheDataModel.10.blob");
			Assert.NotNull(elem);
			Assert.AreEqual(16, elem.Value.LengthBits);
		}

		[Test, ExpectedException(typeof(CrackingFailure), ExpectedMessage = "Block 'TheDataModel.block' has length of 16 bits but already read 32 bits.")]
		public void CrackBadSizeParent()
		{
			string xml = @"<?xml version='1.0' encoding='utf-8'?>
<Peach>
	<DataModel name='TheDataModel'>
		<Block name='block'>
			<Number name='num' signed='false' endian='big' size='32'>
				<Relation type='size' of='block'/>
			</Number>
		</Block>
	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();

			data.WriteBytes(new byte[] { 0, 0, 0, 2 });
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);
		}

		[Test, ExpectedException(typeof(CrackingFailure), ExpectedMessage = "Block 'TheDataModel.block' has length of 16 bits but already read 32 bits.")]
		public void CrackBadSizeBlockParent()
		{
			string xml = @"<?xml version='1.0' encoding='utf-8'?>
<Peach>
	<DataModel name='TheDataModel'>
		<Block name='block'>
			<Block name='inner'>
				<Number name='num' signed='false' endian='big' size='32'>
					<Relation type='size' of='block'/>
				</Number>
			</Block>
		</Block>
	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();

			data.WriteBytes(new byte[] { 0, 0, 0, 2 });
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);
		}

		[Test]
		public void CrackSizeParentArray()
		{
			string xml = @"<?xml version='1.0' encoding='utf-8'?>
<Peach>
	<DataModel name='TheDataModel'>
		<Block name='block'>
			<Number occurs='1' name='num' signed='false' endian='big' size='32'>
				<Relation type='size' of='block'/>
			</Number>
			<Blob name='blob'/>
		</Block>
	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();

			data.WriteBytes(new byte[] { 0, 0, 0, 6, 0, 0, 1 });
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			var elem = dom.dataModels[0].find("TheDataModel.block.blob");
			Assert.NotNull(elem);
			Assert.AreEqual(16, elem.Value.LengthBits);
		}

		[Test]
		public void RecursiveSizeRelation1()
		{
			string xml = @"
<Peach>
	<DataModel name=""DM"">
		<Block name=""TheBlock"">
			<Number name=""Length"" size=""8"">
				<Relation type=""size"" of=""data""/>
			</Number>
			<Blob name=""data""/>
		</Block>
	</DataModel>

    <DataModel name=""DM2"" ref=""DM"">
		<Block name=""TheBlock.data"">
			<Block name=""R1"" ref=""DM"" />
		</Block>
	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(new byte[] { 0x02, 0x01, 0x60 });
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[1], data);

			Assert.AreEqual(1, dom.dataModels[1].Count);
			Assert.IsTrue(dom.dataModels[1][0] is Dom.Block);

			Dom.Block outerBlock = (Dom.Block)dom.dataModels[1][0];
			Assert.AreEqual(2, outerBlock.Count);
			Assert.IsTrue(outerBlock[0] is Dom.Number);
			Assert.AreEqual(new byte[] { 0x02 }, ((Dom.Number)outerBlock[0]).Value.Value);
			Assert.IsTrue(outerBlock[1] is Dom.Block);

			Dom.Block outerDataBlock = (Dom.Block)outerBlock[1];
			Assert.AreEqual(1, outerDataBlock.Count);
			Assert.IsTrue(outerDataBlock[0] is Dom.Block);
			Assert.AreEqual(1, ((Dom.Block)outerDataBlock[0]).Count);
			Assert.IsTrue(((Dom.Block)outerDataBlock[0])[0] is Dom.Block);

			Dom.Block innerBlock = (Dom.Block)(((Dom.Block)outerDataBlock[0])[0]);
			Assert.AreEqual(2, innerBlock.Count);
			Assert.IsTrue(innerBlock[0] is Dom.Number);
			Assert.AreEqual(new byte[] { 0x01 }, ((Dom.Number)innerBlock[0]).Value.Value);
			Assert.IsTrue(innerBlock[1] is Dom.Blob);
			Assert.AreEqual(new byte[] { 0x60 }, ((Dom.Blob)innerBlock[1]).Value.Value);


		}


		[Test]
		public void RecursiveSizeRelation2()
		{
			string xml = @"
<Peach>
	<DataModel name=""DM"">
		<Block name=""TheBlock"">
			<Number name=""Length"" size=""8"">
				<Relation type=""size"" of=""TheBlock""/>
			</Number>
			<Blob name=""data""/>
		</Block>
	</DataModel>

	<DataModel name=""DM2"" ref=""DM"">
		<Block name=""TheBlock.data"">
			<Block name=""R1"" ref=""DM"" />
		</Block>
	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(new byte[] { 0x03, 0x02, 0x60 });
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[1], data);

			Assert.AreEqual(1, dom.dataModels[1].Count);
			Assert.IsTrue(dom.dataModels[1][0] is Dom.Block);

			Dom.Block outerBlock = (Dom.Block)dom.dataModels[1][0];
			Assert.AreEqual(2, outerBlock.Count);
			Assert.IsTrue(outerBlock[0] is Dom.Number);
			Assert.AreEqual(new byte[] { 0x03 }, ((Dom.Number)outerBlock[0]).Value.Value);
			Assert.IsTrue(outerBlock[1] is Dom.Block);

			Dom.Block outerDataBlock = (Dom.Block)outerBlock[1];
			Assert.AreEqual(1, outerDataBlock.Count);
			Assert.IsTrue(outerDataBlock[0] is Dom.Block);
			Assert.AreEqual(1, ((Dom.Block)outerDataBlock[0]).Count);
			Assert.IsTrue(((Dom.Block)outerDataBlock[0])[0] is Dom.Block);

			Dom.Block innerBlock = (Dom.Block)(((Dom.Block)outerDataBlock[0])[0]);
			Assert.AreEqual(2, innerBlock.Count);
			Assert.IsTrue(innerBlock[0] is Dom.Number);
			Assert.AreEqual(new byte[] { 0x02 }, ((Dom.Number)innerBlock[0]).Value.Value);
			Assert.IsTrue(innerBlock[1] is Dom.Blob);
			Assert.AreEqual(new byte[] { 0x60 }, ((Dom.Blob)innerBlock[1]).Value.Value);


		}

		[Test]
		public void StringSizeRelation()
		{
			string xml = @"
<Peach>
	<DataModel name='StringLengthModel'>
		<String name='ALength'>
			<Relation type='size' of='A' />
		</String>
		<String token='true' value='\r\n' />
		<String name='A' />
	</DataModel>
</Peach>
";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(Encoding.ASCII.GetBytes("3\r\nabc"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("abc", (string)dom.dataModels[0][2].DefaultValue);
		}
	}
}

// end
