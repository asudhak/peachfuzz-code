
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
	class StringTests
	{
		static byte[] AppendByte(byte[] buf)
		{
			var list = buf.ToList();
			list.Add(0xff);
			return list.ToArray();
		}

		[Test]
		public void TestEncodings()
		{
			Assert.True(Encoding.ASCII.IsSingleByte);
			Assert.True(Utilities.ExtendedASCII.IsSingleByte);
			Assert.False(Encoding.BigEndianUnicode.IsSingleByte);
			Assert.False(Encoding.Unicode.IsSingleByte);
			Assert.False(Encoding.UTF7.IsSingleByte);
			Assert.False(Encoding.UTF8.IsSingleByte);
			Assert.False(Encoding.UTF32.IsSingleByte);

			// Why???
			if (Platform.GetOS() == Platform.OS.Windows)
			{
				Assert.AreEqual(2, Encoding.ASCII.GetMaxByteCount(1));
				Assert.AreEqual(4, Encoding.BigEndianUnicode.GetMaxByteCount(1));
				Assert.AreEqual(4, Encoding.Unicode.GetMaxByteCount(1));
				Assert.AreEqual(5, Encoding.UTF7.GetMaxByteCount(1));
				Assert.AreEqual(6, Encoding.UTF8.GetMaxByteCount(1));
				Assert.AreEqual(8, Encoding.UTF32.GetMaxByteCount(1));
			}
			else
			{
				Assert.AreEqual(1, Encoding.ASCII.GetMaxByteCount(1));
				Assert.AreEqual(2, Encoding.BigEndianUnicode.GetMaxByteCount(1));
				Assert.AreEqual(2, Encoding.Unicode.GetMaxByteCount(1));
				Assert.AreEqual(5, Encoding.UTF7.GetMaxByteCount(1));
				Assert.AreEqual(4, Encoding.UTF8.GetMaxByteCount(1));
				Assert.AreEqual(4, Encoding.UTF32.GetMaxByteCount(1));
			}

			Assert.Throws<EncoderFallbackException>(delegate()
			{
				Utilities.StringToBytes("\u00abX", Encoding.ASCII);
			});

			Assert.Throws<EncoderFallbackException>(delegate()
			{
				Utilities.StringToBytes("\x80", Encoding.ASCII);
			});

			var bufD = Utilities.StringToBytes("\x80", Utilities.ExtendedASCII);
			Assert.AreEqual(1, bufD.Length);
			Assert.AreEqual(0x80, bufD[0]);

			var buf = Utilities.StringToBytes("Hello", Encoding.ASCII);
			Assert.AreEqual(5, buf.Length);
			var buf16 = Utilities.StringToBytes("\u00abX", Encoding.Unicode);
			Assert.AreEqual(4, buf16.Length);
			var buf16be = Utilities.StringToBytes("\u00abX", Encoding.BigEndianUnicode);
			Assert.AreEqual(4, buf16be.Length);
			var buf32 = Utilities.StringToBytes("\u00abX", Encoding.UTF32);
			Assert.AreEqual(8, buf32.Length);
			var buf8 = Utilities.StringToBytes("\u00abX", Encoding.UTF8);
			Assert.AreEqual(3, buf8.Length);
			var buf7 = Utilities.StringToBytes("\u00abX", Encoding.UTF7);
			Assert.AreEqual(6, buf7.Length);

			string str;

			str = Utilities.BytesToString(buf, Encoding.ASCII);
			Assert.AreEqual("Hello", str);
			str = Utilities.BytesToString(bufD, Utilities.ExtendedASCII);
			Assert.AreEqual("\x80", str);
			str = Utilities.BytesToString(buf16, Encoding.Unicode);
			Assert.AreEqual("\u00abX", str);
			str = Utilities.BytesToString(buf16be, Encoding.BigEndianUnicode);
			Assert.AreEqual("\u00abX", str);
			str = Utilities.BytesToString(buf8, Encoding.UTF8);
			Assert.AreEqual("\u00abX", str);
			str = Utilities.BytesToString(buf7, Encoding.UTF7);
			Assert.AreEqual("\u00abX", str);
			str = Utilities.BytesToString(buf32, Encoding.UTF32);
			Assert.AreEqual("\u00abX", str);

			Assert.Throws<DecoderFallbackException>(delegate()
			{
				Utilities.BytesToString(AppendByte(buf), Encoding.ASCII);
			});

			Assert.Throws<DecoderFallbackException>(delegate()
			{
				Utilities.BytesToString(AppendByte(buf16), Encoding.Unicode);
			});

			Assert.Throws<DecoderFallbackException>(delegate()
			{
				Utilities.BytesToString(AppendByte(buf16be), Encoding.BigEndianUnicode);
			});

			Assert.Throws<DecoderFallbackException>(delegate()
			{
				Utilities.BytesToString(AppendByte(buf32), Encoding.UTF32);
			});

			Assert.Throws<DecoderFallbackException>(delegate()
			{
				Utilities.BytesToString(AppendByte(buf8), Encoding.UTF8);
			});

			Assert.Throws<DecoderFallbackException>(delegate()
			{
				Utilities.BytesToString(AppendByte(buf7), Encoding.UTF7);
			});
		}

		[Test]
		public void CrackString1()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("Hello World", (string)dom.dataModels[0][0].DefaultValue);
		}

		[Test]
		public void CrackString2()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String length=\"5\" />" +
				"		<String />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("12345Hello World"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("12345", (string)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual("Hello World", (string)dom.dataModels[0][1].DefaultValue);
		}

		[Test]
		public void CrackString3()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String />" +
				"		<String length=\"5\" />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World12345"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("Hello World", (string)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual("12345", (string)dom.dataModels[0][1].DefaultValue);
		}

		[Test]
		public void CrackString4()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String nullTerminated=\"true\" />" +
				"		<String />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World"));
			data.WriteByte(0);
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Foo Bar"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("Hello World\0", (string)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual("Foo Bar", (string)dom.dataModels[0][1].DefaultValue);
		}

		[Test]
		public void CrackString5()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String />" +
				"		<Number size=\"16\" />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World"));
			data.WriteInt16(3111);
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("Hello World", (string)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual(3111, (int)dom.dataModels[0][1].DefaultValue);
		}

		[Test]
		public void CrackString6()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String name=\"TheString\" />" +
				"		<Block name=\"TheBlock\">" +
				"			<Number name=\"TheNumber\" size=\"16\" />" +
				"		</Block>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World"));
			data.WriteInt16(3111);
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("Hello World", (string)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual(3111, (int)((DataElementContainer)dom.dataModels[0][1])[0].DefaultValue);
		}

		[Test]
		public void CrackString7()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String />" +
				"		<Block>" +
				"			<Number size=\"16\" />" +
				"		</Block>" +
				"		<Block>" +
				"			<Number size=\"16\" />" +
				"		</Block>" +
				"		<Block>" +
				"			<Number size=\"16\" />" +
				"		</Block>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World"));
			data.WriteInt16(3111);
			data.WriteInt16(3112);
			data.WriteInt16(3113);
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("Hello World", (string)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual(3111, (int)((DataElementContainer)dom.dataModels[0][1])[0].DefaultValue);
			Assert.AreEqual(3112, (int)((DataElementContainer)dom.dataModels[0][2])[0].DefaultValue);
			Assert.AreEqual(3113, (int)((DataElementContainer)dom.dataModels[0][3])[0].DefaultValue);
		}
	}
}

// end
