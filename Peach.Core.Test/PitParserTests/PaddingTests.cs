
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

using Peach.Core;
using Peach.Core.IO;
using Peach.Core.Dom;
using Peach.Core.Analyzers;

namespace Peach.Core.Test.PitParserTests
{
	[TestFixture]
	class PaddingTests
	{
		[Test]
		public void PaddingTest1()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Padding alignment=\"8\" />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Padding padding = dom.dataModels[0][0] as Padding;

			Assert.AreEqual(0, ((BitStream)padding.DefaultValue).LengthBits);
		}

		[Test]
		public void PaddingTest2()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number size=\"8\" />"+
				"		<Padding alignment=\"16\" />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Padding padding = dom.dataModels[0][1] as Padding;

			Assert.AreEqual(8, ((BitStream)padding.DefaultValue).LengthBits);
		}

		[Test]
		public void PaddingTest3()
		{
			string xml = @"
<Peach>
	<DataModel name='DM'>
		<Blob name='header' length='10'/>
		<Block name='blk'>
			<Blob name='payload' length='10' valueType='hex' value='11 22 33 44 55 66 77 88 99 aa' />
			<Padding name='padding' alignment='128'>
				<Fixup class='FillValue'>
					<Param name='ref' value='padding'/>
					<Param name='start' value='1'/>
					<Param name='stop' value='255'/>
				</Fixup>
			</Padding>
			<Number name='len' size='8' signed='false'>
				<Relation type='size' of='padding'/>
			</Number>
			<Number name='next' size='8' value='255' signed='false'/>
		</Block>
	</DataModel>
</Peach>
";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var val = dom.dataModels[0].Value;
			Assert.NotNull(val);

			val.Seek(0, SeekOrigin.Begin);
			string str = Utilities.HexDump(val);
			Assert.NotNull(str);

			Assert.AreEqual(80 + 128, val.LengthBits);

			byte[] expected = new byte[] {
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // header
				0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xaa, // payload
				0x01, 0x02, 0x03, 0x04,                                     // padding
				0x04,                                                       // length of padding
				0xff                                                        // next
			};

			Assert.AreEqual(expected, val.ToArray());
		}

		[Test]
		public void PaddingTest4()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number size=\"33\" />" +
				"		<Padding alignment=\"16\" />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Padding padding = dom.dataModels[0][1] as Padding;

			Assert.AreEqual(15, ((BitStream)padding.DefaultValue).LengthBits);
		}

		[Test]
		public void PaddingTest5()
		{
			string xml = @"
<Peach>
	<DataModel name='data'>
		<Block name='block1'>
			<String value='top' />
		</Block>
		<Block name='block2'>
			<Padding alignment='32' alignedTo='block1' />
			<String value='middle' />
		</Block>
		<Block name='block3'>
			<String value='bottom' />
		</Block>
	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Padding padding = ((Block)dom.dataModels[0][1])[0] as Padding;

			Assert.AreEqual(8, ((BitStream)padding.DefaultValue).LengthBits);
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Error, unable to resolve alignedTo 'missing'.")]
		public void PaddingTest6()
		{
			string xml = @"
<Peach>
	<DataModel name='DM'>
		<String/>
		<Padding alignedTo='missing'/>
	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
		}

		[Test]
		public void PaddingTest7()
		{
			string xml = @"
<Peach>
	<DataModel name='TheDataModel'>
		<Blob length='3'/>
		<Padding minSize='32'/>
	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Padding padding = dom.dataModels[0][1] as Padding;

			Assert.AreEqual(8, ((BitStream)padding.DefaultValue).LengthBits);
		}

		[Test]
		public void PaddingTest8()
		{
			string xml = @"
<Peach>
	<DataModel name='TheDataModel'>
		<Blob length='3'/>
		<Padding minSize='32' alignment='48' />
	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Padding padding = dom.dataModels[0][1] as Padding;

			Assert.AreEqual(24, ((BitStream)padding.DefaultValue).LengthBits);
		}

		[Test]
		public void PaddingTest9()
		{
			string xml = @"
<Peach>
	<DataModel name='TheDataModel'>
		<Blob length='3'/>
		<Padding minSize='32' alignment='32' />
	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Padding padding = dom.dataModels[0][1] as Padding;

			Assert.AreEqual(8, ((BitStream)padding.DefaultValue).LengthBits);
		}

		[Test]
		public void PaddingTest10()
		{
			string xml = @"
<Peach>
	<DataModel name='TheDataModel'>
		<Blob length='3'/>
		<Padding minSize='24'/>
	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Padding padding = dom.dataModels[0][1] as Padding;

			Assert.AreEqual(0, ((BitStream)padding.DefaultValue).LengthBits);
		}

		[Test]
		public void PaddingTest11()
		{
			string xml = @"
<Peach>
	<DataModel name='TheDataModel'>
		<Blob length='3'/>
		<Blob length='3'/>
		<Padding minSize='64'/>
	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Padding padding = dom.dataModels[0][2] as Padding;

			Assert.AreEqual(16, ((BitStream)padding.DefaultValue).LengthBits);
		}

		[Test]
		public void PaddingTest12()
		{
			string xml = @"
<Peach>
	<DataModel name='TheDataModel'>
		<Blob length='3'/>
		<Blob name='b' length='3'/>
		<Padding minSize='64' alignedTo='b'/>
	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Padding padding = dom.dataModels[0][2] as Padding;

			Assert.AreEqual(40, ((BitStream)padding.DefaultValue).LengthBits);
		}

		[Test]
		public void PaddingTest13()
		{
			string xml = @"
<Peach>
	<DataModel name='TheDataModel'>
		<Blob length='3'/>
		<Blob name='b' length='3'/>
		<Padding minSize='24' alignedTo='b' alignment='64'/>
	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Padding padding = dom.dataModels[0][2] as Padding;

			Assert.AreEqual(40, ((BitStream)padding.DefaultValue).LengthBits);
		}

	}
}
