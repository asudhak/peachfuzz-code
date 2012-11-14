
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
				"		<Number size=\"8\" endian=\"big\" signed=\"true\"/>" +
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
			if (signed)
				Assert.AreEqual(value, (long)num.DefaultValue);
			else
				Assert.AreEqual(value, (ulong)num.DefaultValue);
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
			TestString<short>(0xabc, new byte[] { 0xbc, 0xa0 }, 12, false, true);
		}

		[Test]
		public void TestNoValue()
		{
			string xml = "<Peach><DataModel name=\"DM\"><Number size=\"12\" value=\"0x123\"/></DataModel></Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Number num = dom.dataModels[0][0] as Number;

			var defaultValue = num.DefaultValue;
			Assert.NotNull(defaultValue);
			var final = num.Value;
			Assert.NotNull(final);

			BitStream data = new BitStream( new byte[] { 0x01, 0x23 } );
			Peach.Core.Cracker.DataCracker cracker = new Peach.Core.Cracker.DataCracker();
			cracker.CrackData(dom.dataModels[0], data);
			defaultValue = num.DefaultValue;
			Assert.NotNull(defaultValue);
			final = num.Value;
			Assert.NotNull(final);

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

		[Test]
		public void TestHexParseShort()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number name=\"TheNumber\" size=\"32\" value=\"01\"" +
				"		signed=\"true\" endian=\"big\" valueType=\"hex\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Number num = dom.dataModels[0][0] as Number;

			Assert.AreEqual(true, num.Signed);
			Assert.AreEqual(false, num.LittleEndian);
			Assert.AreEqual(1, (int)num.DefaultValue);
			BitStream val = num.Value;
			Assert.AreEqual(32, val.LengthBits);
			Assert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x01 }, val.Value);
		}
	}
}
