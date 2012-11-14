
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
	public class StringTests
	{
		// TODO - Unicode and everything 

		[Test]
		public void SimpleStringTest()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String name=\"TheString\" value=\"abc\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Dom.String str = dom.dataModels[0][0] as Dom.String;

			Assert.AreNotEqual(null, str);
			Assert.AreEqual(Dom.StringType.ascii, str.stringType);
			Assert.AreEqual("abc", (string)str.DefaultValue);

			BitStream value = str.Value;
			Assert.AreEqual(3, value.LengthBytes);
			Assert.AreEqual(Encoding.ASCII.GetBytes("abc"), value.Value);
		}

		[Test]
		public void Utf7Test()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String name=\"TheString\" type=\"utf7\" value=\"abc\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Dom.String str = dom.dataModels[0][0] as Dom.String;

			Assert.AreNotEqual(null, str);
			Assert.AreEqual(Dom.StringType.utf7, str.stringType);
			Assert.AreEqual("abc", (string)str.DefaultValue);

			BitStream value = str.Value;
			Assert.AreEqual(Encoding.UTF7.GetBytes("abc"), value.Value);
		}

		[Test]
		public void Utf8Test()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String name=\"TheString\" type=\"utf8\" value=\"abc\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Dom.String str = dom.dataModels[0][0] as Dom.String;

			Assert.AreNotEqual(null, str);
			Assert.AreEqual(Dom.StringType.utf8, str.stringType);
			Assert.AreEqual("abc", (string)str.DefaultValue);

			BitStream value = str.Value;
			Assert.AreEqual(Encoding.UTF8.GetBytes("abc"), value.Value);
		}

		[Test]
		public void Utf16Test()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String name=\"TheString\" type=\"utf16\" value=\"abc\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Dom.String str = dom.dataModels[0][0] as Dom.String;

			Assert.AreNotEqual(null, str);
			Assert.AreEqual(Dom.StringType.utf16, str.stringType);
			Assert.AreEqual("abc", (string)str.DefaultValue);

			BitStream value = str.Value;
			Assert.AreEqual(Encoding.Unicode.GetBytes("abc"), value.Value);
		}

		[Test]
		public void Utf16BeTest()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String name=\"TheString\" type=\"utf16be\" value=\"abc\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Dom.String str = dom.dataModels[0][0] as Dom.String;

			Assert.AreNotEqual(null, str);
			Assert.AreEqual(Dom.StringType.utf16be, str.stringType);
			Assert.AreEqual("abc", (string)str.DefaultValue);

			BitStream value = str.Value;
			Assert.AreEqual(Encoding.BigEndianUnicode.GetBytes("abc"), value.Value);
		}

		[Test]
		public void Utf32Test()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String name=\"TheString\" type=\"utf32\" value=\"abc\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Dom.String str = dom.dataModels[0][0] as Dom.String;

			Assert.AreNotEqual(null, str);
			Assert.AreEqual(Dom.StringType.utf32, str.stringType);
			Assert.AreEqual("abc", (string)str.DefaultValue);

			BitStream value = str.Value;
			Assert.AreEqual(Encoding.UTF32.GetBytes("abc"), value.Value);
		}

		[Test]
		public void HexStringTest()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String name=\"TheString\" valueType=\"hex\" value=\"0x0a\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Dom.String str = dom.dataModels[0][0] as Dom.String;

			Assert.AreNotEqual(null, str);
			Assert.AreEqual(Dom.StringType.ascii, str.stringType);
			Assert.AreEqual(Variant.VariantType.String, str.DefaultValue.GetVariantType());
			Assert.AreEqual("\n", (string)str.DefaultValue);
		}

		[Test]
		public void HexStringTest2()
		{
			// 0xaa 0xbb is not a valid ascii string, should throw
			
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String name=\"TheString\" valueType=\"hex\" value=\"0xaa 0xbb\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();

			Assert.Throws<PeachException>(delegate(){
				parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			});
		}

		[Test]
		public void Utf32StringTest()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String name=\"TheString\" type=\"utf32\" value=\"Hello\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Dom.String str = dom.dataModels[0][0] as Dom.String;

			Assert.AreNotEqual(null, str);
			Assert.AreEqual(Dom.StringType.utf32, str.stringType);
			Assert.AreEqual("Hello", (string)str.DefaultValue);

			BitStream value = str.Value;
			Assert.AreEqual(20, value.LengthBytes);
			Assert.AreEqual(Encoding.UTF32.GetBytes("Hello"), value.Value);
		}

		[Test]
		public void HexStringUtf32Test()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String name=\"TheString\" type=\"utf32\" valueType=\"hex\" value=\"48 00 00 00 65 00 00 00 6c 00 00 00 6c 00 00 00 6f 00 00 00\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Dom.String str = dom.dataModels[0][0] as Dom.String;

			Assert.AreNotEqual(null, str);
			Assert.AreEqual(Dom.StringType.utf32, str.stringType);
			Assert.AreEqual(Variant.VariantType.String, str.DefaultValue.GetVariantType());
			Assert.AreEqual("Hello", (string)str.DefaultValue);
		}
	}
}
