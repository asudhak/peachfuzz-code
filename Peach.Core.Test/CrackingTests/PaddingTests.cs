
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
	public class PaddingTests
	{
		static string template = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
			"	<DataModel name=\"TheDataModel\">" +
			"		<Block>" +
			"			<Blob name=\"blb\" length=\"{0}\" valueType=\"hex\" value=\"{1}\" />" +
			"			<Padding alignment=\"16\" /> " +
			"		</Block>" +
			"		<String/>" +
			"	</DataModel>" +
			"</Peach>";

		[Test]
		public void CrackPadding1()
		{
			string xml = template.Fmt(1, "00");

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.WriteBytes(new byte[] { 1, 2, 49, 50, 51});
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(2, dom.dataModels[0].Count);
			var block = dom.dataModels[0][0] as Block;
			Assert.AreEqual(2, block.Count);
			Assert.AreEqual(new byte[] { 1 }, (byte[])block[0].Value.Value);
			Assert.AreEqual(new byte[] { 1 }, (byte[])block[0].DefaultValue);
			Assert.AreEqual(8, ((BitStream)block[1].DefaultValue).LengthBits);
			Assert.AreEqual(8, ((BitStream)block[1].Value).LengthBits);
			Assert.AreEqual("123", (string)dom.dataModels[0][1].DefaultValue);

			var value = dom.dataModels[0].Value;
			value.SeekBytes(0, SeekOrigin.Begin);
			Assert.AreEqual(5, value.LengthBytes);
			Assert.AreEqual(1, value.ReadByte());
			Assert.AreEqual(0, value.ReadByte());
		}

		[Test]
		public void CrackPadding2()
		{
			string xml = template.Fmt(2, "00 00");

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.WriteBytes(new byte[] { 1, 2, 49, 50, 51 });
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(2, dom.dataModels[0].Count);
			var block = dom.dataModels[0][0] as Block;
			Assert.AreEqual(2, block.Count);
			Assert.AreEqual(new byte[] { 1, 2 }, (byte[])block[0].Value.Value);
			Assert.AreEqual(new byte[] { 1, 2 }, (byte[])block[0].DefaultValue);
			Assert.AreEqual(0, ((BitStream)block[1].DefaultValue).LengthBits);
			Assert.AreEqual(0, ((BitStream)block[1].Value).LengthBits);
			Assert.AreEqual("123", (string)dom.dataModels[0][1].DefaultValue);

			var value = dom.dataModels[0].Value;
			value.SeekBytes(0, SeekOrigin.Begin);
			Assert.AreEqual(5, value.LengthBytes);
			Assert.AreEqual(1, value.ReadByte());
			Assert.AreEqual(2, value.ReadByte());
		}

		[Test]
		public void GeneratePadding1()
		{
			string xml = template.Fmt(1, "00");

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var data = dom.dataModels[0].Value;
			Assert.AreEqual(16, data.LengthBits);

			var block = dom.dataModels[0][0] as Block;
			var blob = block[0];
			Assert.AreEqual(8, blob.Value.LengthBits);

			var padding = block[1];
			Assert.AreEqual(8, padding.Value.LengthBits);
		}

		[Test]
		public void GeneratePadding2()
		{
			string xml = template.Fmt(0, "");

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var data = dom.dataModels[0].Value;
			Assert.AreEqual(0, data.LengthBits);

			var block = dom.dataModels[0][0] as Block;
			var blob = block[0];
			Assert.AreEqual(0, blob.Value.LengthBits);

			var padding = block[1];
			Assert.AreEqual(0, padding.Value.LengthBits);
		}

		[Test]
		public void GeneratePadding3()
		{
			string xml = template.Fmt(2, "11 22");

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var data = dom.dataModels[0].Value;
			Assert.AreEqual(16, data.LengthBits);

			var block = dom.dataModels[0][0] as Block;
			var blob = block[0];
			Assert.AreEqual(16, blob.Value.LengthBits);

			var padding = block[1];
			Assert.AreEqual(0, padding.Value.LengthBits);
		}
	}
}

// end
