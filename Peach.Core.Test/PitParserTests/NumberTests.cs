
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
using Peach.Core.IO;

namespace Peach.Core.Test.PitParserTests
{
	[TestFixture]
	class NumberTests
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
			Assert.AreEqual(0, (int)num.DefaultValue);
		}

		public void TestString<T>(T value, byte[] expected, int size, bool signed, bool isLittleEndian)
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number name=\"TheNumber\" size=\"" + size + "\" value=\"" + value +"\"" +
				"		signed=\"" + (signed ? "true" : "false") + "\"" +
				"		endian=\"" + (isLittleEndian ? "little" : "big") + "\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Number num = dom.dataModels[0][0] as Number;

			Assert.AreEqual(signed, num.Signed);
			Assert.AreEqual(isLittleEndian, num.LittleEndian);
			if (!(value is string))
			{
				if (signed)
					Assert.AreEqual(value, (long)num.DefaultValue);
				else
					Assert.AreEqual(value, (ulong)num.DefaultValue);
			}
			BitStream val = num.Value;
			Assert.AreEqual(size, val.LengthBits);
			Assert.AreEqual(expected, val.Value);
		}

		[Test]
		public void TestStringByteSignedLittle()
		{
			TestString<byte>(16, new byte[] { 16 }, 8, true, true);
		}

		[Test]
		public void TestStringByteSignedBig()
		{
			TestString<byte>(16, new byte[] { 16 }, 8, true, false);
		}

		[Test]
		public void TestStringByteUnsignedLittle()
		{
			TestString<sbyte>(16, new byte[] { 16 }, 8, false, true);
		}

		[Test]
		public void TestStringByteUnsignedBig()
		{
			TestString<sbyte>(16, new byte[] { 16 }, 8, false, false);
		}

		[Test]
		public void TestStringShortSignedLittle()
		{
			TestString<short>(-2, new byte[] { 0xfe, 0xff}, 16, true, true);
		}

		[Test]
		public void TestStringShortSignedBig()
		{
			TestString<short>(-2, new byte[] { 0xff, 0xfe }, 16, true, false);
		}

		[Test]
		public void TestStringUshortUnsignedLittle()
		{
			TestString<ushort>(0x0102, new byte[] { 0x02, 0x01 }, 16, false, true);
		}

		[Test]
		public void TestStringUshortUnsignedBig()
		{
			TestString<ushort>(0x0102, new byte[] { 0x01, 0x02 }, 16, false, false);
		}

		[Test]
		public void TestHexStringUshortUnsignedBig()
		{
			TestString<string>("0x1fb", new byte[] { 0x01, 0xfb }, 16, false, false);
		}

		[Test]
		public void TestStringUintExpandLittle()
		{
			TestString<short>(0x01, new byte[] { 0x01, 0x00, 0x00, 0x00 }, 32, true, true);
		}

		[Test]
		public void TestStringUintExpandBig()
		{
			TestString<short>(0x01, new byte[] { 0x00, 0x00, 0x00, 0x01 }, 32, true, false);
		}

		[Test]
		public void TestBitwise()
		{
			// value, expected, size, signed, little
			TestString<byte>(0xff, new byte[] { 0xff }, 8, false, true);
			TestString<sbyte>(-1, new byte[] { 0xff }, 8, true, true);

			Assert.Throws<PeachException>(delegate() {
				TestString<sbyte>(-1, new byte[] { 0xff }, 8, false, true);
			});

			TestString<sbyte>(7, new byte[] { 0x70 }, 4, true, true);
			TestString<sbyte>(-8, new byte[] { 0x80 }, 4, true, true);

			Assert.Throws<PeachException>(delegate() {
				TestString<byte>(100, new byte[] { 0xf0 }, 4, false, true);
			});

			Assert.Throws<PeachException>(delegate() {
				TestString<sbyte>(-100, new byte[] { 0xf0 }, 4, true, true);
			});

			TestString<ushort>(0xabc, new byte[] { 0xbc, 0xa0 }, 12, false, true);
			TestString<ushort>(0xabc, new byte[] { 0xab, 0xc0 }, 12, false, false);

			TestString<uint>(0xffffffff, new byte[] { 0xff, 0xff, 0xff, 0xff }, 32, false, false);
			TestString<ulong>(0xffffffffffffffff, new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff }, 64, false, false);
		}


		[Test]
		public void TestNoValue()
		{
			string xml = "<Peach><DataModel name=\"DM\"><Number size=\"12\"/></DataModel></Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Number num = dom.dataModels[0][0] as Number;

			var defaultValue = num.DefaultValue;
			Assert.NotNull(defaultValue);
			Assert.AreEqual(0, (int)defaultValue);

			var final = num.Value;
			Assert.NotNull(final);
			Assert.AreEqual(12, final.LengthBits);
			Assert.AreEqual(2, final.LengthBytes);
			MemoryStream ms = final.Stream as MemoryStream;
			Assert.NotNull(ms);
			Assert.AreEqual(0, ms.GetBuffer()[0]);
			Assert.AreEqual(0, ms.GetBuffer()[1]);
		}

		[Test]
		public void TestHexParse()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number name=\"TheNumber\" size=\"32\" value=\"01 02 03 04\"" +
				"		signed=\"true\" endian=\"big\" valueType=\"hex\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Number num = dom.dataModels[0][0] as Number;

			Assert.AreEqual(true, num.Signed);
			Assert.AreEqual(false, num.LittleEndian);
			Assert.AreEqual(0x01020304, (int)num.DefaultValue);
			BitStream val = num.Value;
			Assert.AreEqual(32, val.LengthBits);
			Assert.AreEqual(new byte[]{0x01, 0x02, 0x03, 0x04}, val.Value);
		}

		private void DoHexParse(bool throws, string value, int size)
		{
			string template = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number name=\"TheNumber\" size=\"{0}\" value=\"{1}\"" +
				"		signed=\"true\" endian=\"big\" valueType=\"hex\"/>" +
				"	</DataModel>" +
				"</Peach>";

			string xml = string.Format(template, size, value);
			PitParser parser = new PitParser();

			if (throws)
			{
				Assert.Throws<PeachException>(delegate() {
					parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
				});

				return;
			}

			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Number num = dom.dataModels[0][0] as Number;
			Assert.AreEqual(true, num.Signed);
			Assert.AreEqual(false, num.LittleEndian);
			Assert.AreNotEqual(0, (long)num.DefaultValue);
			BitStream val = num.Value;
			Assert.AreEqual(size, val.LengthBits);
		}

		[Test]
		public void TestHexParseSize()
		{
			DoHexParse(false, "01", 1);
			DoHexParse(false, "01", 8);
			DoHexParse(true, "01", 9);
			DoHexParse(false, "0f", 4);
			DoHexParse(true, "f0", 4);
			DoHexParse(false, "0f 00", 12);
			DoHexParse(true, "f0 00", 12);
			DoHexParse(false, "00 00 00 01", 32);
			DoHexParse(true, "01", 32);
			DoHexParse(true, "00 00 00 00 01", 32);
		}
	}
}
