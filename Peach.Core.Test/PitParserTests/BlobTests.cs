
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
//using NUnit.Framework.Constraints;
using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Analyzers;
using Peach.Core.IO;

namespace Peach.Core.Test.PitParserTests
{
	[TestFixture]
	class BlobTests
	{
		[Test]
		public void BlobTest1()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Blob value=\"Hello World\" />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Blob blob = dom.dataModels[0][0] as Blob;

			Assert.AreEqual(Variant.VariantType.ByteString, blob.DefaultValue.GetVariantType());
			Assert.AreEqual(ASCIIEncoding.ASCII.GetBytes("Hello World"), (byte[])blob.DefaultValue);
		}

		[Test]
		public void BlobTest2()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Blob value=\"41 42 43 44\" valueType=\"hex\" />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Blob blob = dom.dataModels[0][0] as Blob;

			Assert.AreEqual(Variant.VariantType.ByteString, blob.DefaultValue.GetVariantType());
			Assert.AreEqual(new byte[] { 0x41, 0x42, 0x43, 0x44 }, (byte[])blob.DefaultValue);
		}

		[Test]
		public void BlobTest3()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Blob value=\"1234\" />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Blob blob = dom.dataModels[0][0] as Blob;

			Assert.AreEqual(Variant.VariantType.ByteString, blob.DefaultValue.GetVariantType());
			Assert.AreEqual(ASCIIEncoding.ASCII.GetBytes("1234"), (byte[])blob.DefaultValue);
		}

		[Test]
		public void BlobTest4()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Blob length=\"20\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var val = dom.dataModels[0].Value;
			Assert.NotNull(val);
			Assert.AreEqual(20, val.LengthBytes);
		}

		private void DoHexPad(bool throws, int length, string value)
		{
			string attr = value == null ? "" : string.Format("value=\"{0}\"", value);

			string template = "<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Blob length=\"{0}\" valueType=\"hex\" {1}/>" +
				"	</DataModel>" +
				"</Peach>";

			string xml = string.Format(template, length, attr);

			PitParser parser = new PitParser();

			if (throws)
			{
				Assert.Throws<PeachException>(delegate() {
					parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
				});
				return;
			}

			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Dom.Blob blob = dom.dataModels[0][0] as Dom.Blob;

			Assert.AreNotEqual(null, blob);
			Assert.NotNull(blob.DefaultValue);
			Assert.AreEqual(0, blob.lengthAsBits % 8);
			Assert.AreEqual(length, blob.lengthAsBits / 8);
			var type = blob.DefaultValue.GetVariantType();
			Assert.True(Variant.VariantType.BitStream == type ||Variant.VariantType.ByteString == type);
			BitStream bs = (BitStream)blob.DefaultValue;
			Assert.AreEqual(length, bs.LengthBytes);
		}

		[Test]
		public void TestHexPad()
		{
			DoHexPad(false, 4, null);
			DoHexPad(false, 4, "01 02 03 04");
			DoHexPad(false, 4, "01 02 03");
			DoHexPad(true, 4, "01 02 03 04 05");
		}
	}
}
