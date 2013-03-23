
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
	class DefaultTests
	{
		[Test]
		public void NumberDefaults()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<Defaults>" +
				"		<Number endian=\"big\" signed=\"true\"/>" +
				"	</Defaults>" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number name=\"TheNumber\" size=\"8\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Number num = dom.dataModels[0][0] as Number;

			Assert.IsTrue(num.Signed);
			Assert.IsFalse(num.LittleEndian);
		}

		[Test]
		public void ValueTypeHex()
		{
			string xml = @"
<Peach>
	<Defaults>
		<Number valueType=""hex"" endian=""big"" signed=""false""/>
		<String valueType=""hex""/>
		<Blob   valueType=""hex""/>
	</Defaults>

	<DataModel name=""TheDataModel"">
		<Number name=""num"" value=""00 AA"" size=""16""/>
		<String valueType=""hex"" name=""str"" value=""41 42 43 44 45 46""/>
		<Blob   valueType=""hex"" name=""blb"" value=""61 62 63 64 65 66""/>
	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var val = dom.dataModels[0].Value;

			Assert.NotNull(val);
			MemoryStream ms = val.Stream as MemoryStream;
			Assert.NotNull(ms);

			byte[] expected = new byte[] { 0x00, 0xAA, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66 };
			Assert.AreEqual(expected.Length, ms.Length);

			byte[] actual = new byte[ms.Length];
			Buffer.BlockCopy(ms.GetBuffer(), 0, actual, 0, (int)ms.Length);

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void StringDefaults()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<Defaults>" +
				"		<String lengthType=\"chars\" padCharacter=\"z\" nullTerminated=\"true\" type=\"utf8\"/>" +
				"	</Defaults>" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String name=\"TheNumber\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Dom.String str = dom.dataModels[0][0] as Dom.String;

			Assert.IsTrue(str.nullTerminated);
			Assert.IsTrue(str.stringType == StringType.utf8);
			Assert.IsTrue(str.lengthType == LengthType.Chars);
			Assert.IsTrue(str.padCharacter == 'z');
		}

		[Test]
		public void FlagsDefaults()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<Defaults>" +
				"		<Flags endian=\"big\" size=\"32\"/>" +
				"	</Defaults>" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Flags size=\"32\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Flags flags = dom.dataModels[0][0] as Flags;

			Assert.IsFalse(flags.LittleEndian);
		}

		[Test]
		public void BlobDefaults()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<Defaults>" +
				"		<Blob lengthType=\"bits\"/>" +
				"	</Defaults>" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Blob lengthType=\"bits\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Blob blob = dom.dataModels[0][0] as Blob;

			Assert.IsTrue(blob.lengthType == LengthType.Bits);
		}
	}
}
