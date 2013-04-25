
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
	public class OffsetRelationTests
	{
		[Test, ExpectedException(typeof(CrackingFailure), ExpectedMessage = "String 'TheDataModel.Data' has offset of 160 bits but buffer only has 96 bits.")]
		public void TooBigOffset()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number size=\"8\">" +
				"			<Relation type=\"offset\" of=\"Data\" />" +
				"		</Number>" +
				"		<String name=\"Data\" />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.WriteInt8((sbyte)20);
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);
		}

		[Test, ExpectedException(typeof(CrackingFailure), ExpectedMessage = "String 'TheDataModel.Data' has offset of 0 bits but already read 8 bits.")]
		public void TooSmallOffset()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number size=\"8\">" +
				"			<Relation type=\"offset\" of=\"Data\" />" +
				"		</Number>" +
				"		<String name=\"Data\" />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.WriteInt8((sbyte)0);
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);
		}

		[Test]
		public void BasicOffset()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number size=\"8\">" +
				"			<Relation type=\"offset\" of=\"Data\" />" +
				"		</Number>" +
				"		<Blob name=\"Data\" />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			byte[] offsetdata = ASCIIEncoding.ASCII.GetBytes("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

			BitStream data = new BitStream();
			data.WriteInt8((sbyte)(offsetdata.Length + 1));
			data.WriteBytes(offsetdata);
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(offsetdata.Length + 1, (int)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual(ASCIIEncoding.ASCII.GetBytes("Hello World"), (byte[])dom.dataModels[0][1].DefaultValue);
		}

		[Test]
		public void Basic2Offset()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Blob length=\"5\"/>" +
				"		<Number size=\"8\">" +
				"			<Relation type=\"offset\" of=\"Data\" />" +
				"		</Number>" +
				"		<Blob name=\"Data\" />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			byte[] otherdata = ASCIIEncoding.ASCII.GetBytes("12345");
			byte[] offsetdata = ASCIIEncoding.ASCII.GetBytes("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

			BitStream data = new BitStream();
			data.WriteBytes(otherdata);
			data.WriteInt8((sbyte)(offsetdata.Length + 1 + otherdata.Length));
			data.WriteBytes(offsetdata);
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(offsetdata.Length + 1 + otherdata.Length, (int)dom.dataModels[0][1].DefaultValue);
			Assert.AreEqual(ASCIIEncoding.ASCII.GetBytes("Hello World"), (byte[])dom.dataModels[0][2].DefaultValue);
		}

		[Test]
		public void Basic3Offset()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number size=\"8\">" +
				"			<Relation type=\"offset\" of=\"Data\" />" +
				"		</Number>" +
				"		<String name=\"Middle\"/>" +
				"		<String name=\"Data\" />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			string offsetdata = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
			string target = "Hello World";

			BitStream data = new BitStream();
			data.WriteInt8((sbyte)(offsetdata.Length + 1));
			data.WriteBytes(Encoding.ASCII.GetBytes(offsetdata));
			data.WriteBytes(Encoding.ASCII.GetBytes(target));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(offsetdata.Length + 1, (int)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual(offsetdata, (string)dom.dataModels[0][1].DefaultValue);
			Assert.AreEqual(target, (string)dom.dataModels[0][2].DefaultValue);
		}

		[Test]
		public void Basic4Offset()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number size=\"8\">" +
				"			<Relation type=\"offset\" of=\"Data\" />" +
				"		</Number>" +
				"		<String name=\"Middle\"/>" +
				"		<String name=\"Sized\" length=\"5\"/>" +
				"		<String name=\"Data\" />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			string offsetdata = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
			string sizeddata = "12345";
			string target = "Hello World";

			BitStream data = new BitStream();
			data.WriteInt8((sbyte)(sizeddata.Length + offsetdata.Length + 1));
			data.WriteBytes(Encoding.ASCII.GetBytes(offsetdata));
			data.WriteBytes(Encoding.ASCII.GetBytes(sizeddata));
			data.WriteBytes(Encoding.ASCII.GetBytes(target));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(sizeddata.Length + offsetdata.Length + 1, (int)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual(offsetdata, (string)dom.dataModels[0][1].DefaultValue);
			Assert.AreEqual(sizeddata, (string)dom.dataModels[0][2].DefaultValue);
			Assert.AreEqual(target, (string)dom.dataModels[0][3].DefaultValue);
		}

		[Test, ExpectedException(typeof(CrackingFailure), ExpectedMessage = "String 'TheDataModel.Data' has offset of 16 bits but must be at least 40 bits.")]
		public void BadOffset()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number size=\"8\">" +
				"			<Relation type=\"offset\" of=\"Data\" />" +
				"		</Number>" +
				"		<String name=\"Middle\"/>" +
				"		<String name=\"Sized\" length=\"5\"/>" +
				"		<String name=\"Data\" />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			string offsetdata = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
			string sizeddata = "12345";
			string target = "Hello World";

			BitStream data = new BitStream();
			data.WriteInt8((sbyte)3);
			data.WriteBytes(Encoding.ASCII.GetBytes(offsetdata));
			data.WriteBytes(Encoding.ASCII.GetBytes(sizeddata));
			data.WriteBytes(Encoding.ASCII.GetBytes(target));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);
		}


		[Test]
		public void OffsetInSizedBlock()
		{
			string xml = @"<?xml version='1.0' encoding='utf-8'?>
<Peach>
	<DataModel name='TheDataModel'>

		<Number name='offset' size='8'>
			<Relation type='offset' of='Data'/>
		</Number>

		<Block name='block'>
			<Number name='size' size='8'>
				<Relation type='size' of='block'/>
			</Number>

			<String name='Data'/>
		</Block>

	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			byte[] offsetdata = ASCIIEncoding.ASCII.GetBytes("AAAAAAAAAA");
			byte[] payload = ASCIIEncoding.ASCII.GetBytes("Hello World");

			BitStream data = new BitStream();
			data.WriteByte((byte)(1 + 1 + offsetdata.Length));
			data.WriteByte((byte)(1 + offsetdata.Length + payload.Length));
			data.WriteBytes(offsetdata);
			data.WriteBytes(payload);
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("Hello World", (string)dom.dataModels[0].find("block.Data").DefaultValue);
		}

		[Test, ExpectedException(typeof(CrackingFailure), ExpectedMessage = "String 'TheDataModel.block.Data' has offset of 240 bits but buffer only has 184 bits.")]
		public void BadOffsetInSizedBlock()
		{
			string xml = @"<?xml version='1.0' encoding='utf-8'?>
<Peach>
	<DataModel name='TheDataModel'>

		<Number name='offset' size='8'>
			<Relation type='offset' of='Data'/>
		</Number>

		<Block name='block'>
			<Number name='size' size='8'>
				<Relation type='size' of='block'/>
			</Number>

			<String name='Data'/>
		</Block>
	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			byte[] offsetdata = ASCIIEncoding.ASCII.GetBytes("AAAAAAAAAA");
			byte[] payload = ASCIIEncoding.ASCII.GetBytes("Hello World");

			BitStream data = new BitStream();
			data.WriteByte((byte)30);
			data.WriteByte((byte)(1 + offsetdata.Length + payload.Length));
			data.WriteBytes(offsetdata);
			data.WriteBytes(payload);
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("Hello World", (string)dom.dataModels[0].find("block.Data").DefaultValue);
		}

		[Test]
		public void RelativeOffset()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Blob length=\"5\"/>"+
				"		<Number size=\"8\">" +
				"			<Relation type=\"offset\" of=\"Data\" relative=\"true\" />" +
				"		</Number>" +
				"		<Blob name=\"Data\" />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			byte[] otherdata = ASCIIEncoding.ASCII.GetBytes("12345");
			byte[] offsetdata = ASCIIEncoding.ASCII.GetBytes("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

			BitStream data = new BitStream();
			data.WriteBytes(otherdata);
			data.WriteInt8((sbyte)offsetdata.Length);
			data.WriteBytes(offsetdata);
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(offsetdata.Length, (int)dom.dataModels[0][1].DefaultValue);
			Assert.AreEqual(ASCIIEncoding.ASCII.GetBytes("Hello World"), (byte[])dom.dataModels[0][2].DefaultValue);
		}

		[Test]
		public void RelativeToOffset()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String length=\"5\"/>" +
				"		<Number name=\"Size\" size=\"8\">" +
				"			<Relation type=\"offset\" of=\"Data\" relative=\"true\" relativeTo=\"RelData\" />" +
				"		</Number>" +
				"		<String length=\"5\"/>" +
				"		<String name=\"RelData\" length=\"5\"/>" +
				"		<String name=\"Data\" />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			byte[] otherdata = ASCIIEncoding.ASCII.GetBytes("12345");
			byte[] offsetdata = ASCIIEncoding.ASCII.GetBytes("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

			BitStream data = new BitStream();
			data.WriteBytes(otherdata);
			data.WriteInt8((sbyte)(offsetdata.Length + otherdata.Length));
			data.WriteBytes(otherdata);
			data.WriteBytes(otherdata); // RelData
			data.WriteBytes(offsetdata);
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(offsetdata.Length + otherdata.Length, (int)dom.dataModels[0]["Size"].DefaultValue);
			Assert.AreEqual("Hello World", (string)dom.dataModels[0]["Data"].DefaultValue);
		}

		[Test]
		public void RelativeOffsetBlock()
		{
			string xml = @"<?xml version='1.0' encoding='utf-8'?>
<Peach>
	<DataModel name='TheDataModel'>
		<Block length='5'>
			<Number size='8'>
				<Relation type='offset' of='Data' relative='true' />
			</Number>
		</Block>
		<String name='Data'/>
	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			byte[] offsetdata = ASCIIEncoding.ASCII.GetBytes("AAAAAAAAAA");

			BitStream data = new BitStream();
			data.WriteByte((byte)(offsetdata.Length));
			data.WriteBytes(offsetdata);
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("Hello World", (string)dom.dataModels[0]["Data"].DefaultValue);
		}

		[Test]
		public void RelativeOffsetSpace()
		{
			string xml = @"<?xml version='1.0' encoding='utf-8'?>
<Peach>
	<DataModel name='TheDataModel'>
		<Number size='8'>
			<Relation type='offset' of='Data' relative='true' />
		</Number>
		<String length='5'/>
		<String name='Data'/>
	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			byte[] offsetdata = ASCIIEncoding.ASCII.GetBytes("AAAAAAAAAA");

			BitStream data = new BitStream();
			data.WriteByte((byte)(offsetdata.Length));
			data.WriteBytes(offsetdata);
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("Hello World", (string)dom.dataModels[0]["Data"].DefaultValue);
		}

		[Test]
		public void NonChildOffset()
		{
			string xml = @"<?xml version='1.0' encoding='utf-8'?>
<Peach>
	<DataModel name='TheDataModel'>
		<Block name='blk'>
			<Number size='8'>
				<Relation type='size' of='blk' />
			</Number>
			<Block>
				<Blob/>
				<Number size='8'>
					<Relation type='offset' of='Data'/>
				</Number>
			</Block>
		</Block>
		<Blob name='Data'/>
	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			byte[] otherdata = ASCIIEncoding.ASCII.GetBytes("abcde");
			byte[] offsetdata = ASCIIEncoding.ASCII.GetBytes("AAAAAAAAA");

			BitStream data = new BitStream();
			data.WriteByte(7);
			data.WriteBytes(otherdata);
			data.WriteByte(16);
			data.WriteBytes(offsetdata);
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(ASCIIEncoding.ASCII.GetBytes("Hello World"), (byte[])dom.dataModels[0]["Data"].DefaultValue);
		}

		[Test]
		public void NonChildRelativeOffset()
		{
			string xml = @"<?xml version='1.0' encoding='utf-8'?>
<Peach>
	<DataModel name='TheDataModel'>
		<Block name='blk'>
			<Number size='8'>
				<Relation type='size' of='blk' />
			</Number>
			<Block>
				<Blob/>
				<Number size='8'>
					<Relation type='offset' relative='true' of='Data'/>
				</Number>
			</Block>
		</Block>
		<String name='Data'/>
	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			byte[] otherdata = ASCIIEncoding.ASCII.GetBytes("abcde");
			byte[] offsetdata = ASCIIEncoding.ASCII.GetBytes("AAAAAAAAA");

			BitStream data = new BitStream();
			data.WriteByte((byte)(1 + otherdata.Length + 1));
			data.WriteBytes(otherdata);
			data.WriteByte((byte)offsetdata.Length);
			data.WriteBytes(offsetdata);
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("Hello World", (string)dom.dataModels[0]["Data"].DefaultValue);
		}

		[Test]
		public void NonChildRelativeToOffset()
		{
			string xml = @"<?xml version='1.0' encoding='utf-8'?>
<Peach>
	<DataModel name='TheDataModel'>
		<Block name='blk'>
			<Number size='8'>
				<Relation type='size' of='blk' />
			</Number>
			<Block name='inner'>
				<String/>
				<String name='off' length='1'/>
				<Number size='8'>
					<Relation type='offset' of='Data' relativeTo='off'/>
				</Number>
			</Block>
		</Block>
		<String name='Data'/>
	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			byte[] otherdata = ASCIIEncoding.ASCII.GetBytes("abcde");
			byte[] offsetdata = ASCIIEncoding.ASCII.GetBytes("AAAAAAAAA");

			BitStream data = new BitStream();
			data.WriteByte((byte)(1 + otherdata.Length + 1));
			data.WriteBytes(otherdata);
			data.WriteByte((byte)(1 + 1 + offsetdata.Length));
			data.WriteBytes(offsetdata);
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("Hello World", (string)dom.dataModels[0]["Data"].DefaultValue);
		}

		[Test]
		public void Sample()
		{
			string xml = @"
<Peach>
    <DataModel name='TheDataModel'>
        <String length='4' padCharacter=' '>
                <Relation type='offset' of='Offset0' />
        </String>
        <String length='4' padCharacter=' '>
                <Relation type='offset' of='Offset1' />
        </String>
        <String length='4' padCharacter=' '>
                <Relation type='offset' of='Offset2' />
        </String>
        <String length='4' padCharacter=' '>
                <Relation type='offset' of='Offset3' />
        </String>
        <String length='4' padCharacter=' '>
                <Relation type='offset' of='Offset4' />
        </String>

        <String length='4' padCharacter=' '>
                <Relation type='offset' of='Offset5' />
        </String>

        <String length='4' padCharacter=' '>
                <Relation type='offset' of='Offset6' />
        </String>

        <Block>
                <Block name='Offset0'>
                        <Block>
                                <String name='Offset1' value='CRAZY STRING!' />
                                <String value='aslkjalskdjas' />
                                <String value='aslkdjalskdjasdkjasdlkjasd' />
                        </Block>
                        <String name='Offset2' value='ALSKJDALKSJD' />
                        <Block>
                                <String name='Offset3' value='1' />
                                <String name='Offset4' value='' />
                                <String name='Offset5' value='1293812093' />
                        </Block>
                </Block>
        </Block>

        <String name='Offset6' value='aslkdjalskdjas' />

    </DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var final = dom.dataModels[0].Value;
			string str = Encoding.ASCII.GetString(final.Value);
			Assert.NotNull(str);
		}

	}
}

// end
