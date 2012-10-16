
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
		static string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
			"	<DataModel name=\"TheDataModel\">" +
			"		<Blob length=\"1\" valueType=\"hex\" value=\"00\" />" +
			"		<Padding aligned=\"true\" alignment=\"16\" /> " +
			"	</DataModel>" +
			"</Peach>";

		[Test]
		public void CrackPadding1()
		{
			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.WriteBytes(new byte[] { 1, 2 });
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(new byte[] { 1 }, (byte[])dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual(8, ((BitStream)dom.dataModels[0][1].DefaultValue).LengthBits);

			var value = dom.dataModels[0].Value;
			Assert.AreEqual(2, value.LengthBytes);
			Assert.AreEqual(1, value.ReadByte());
			Assert.AreEqual(0, value.ReadByte());

		}

		[Test]
		public void GeneratePadding()
		{
			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var data = dom.dataModels[0].Value;
			Assert.AreEqual(16, data.LengthBits);

			var blob = dom.dataModels[0][0];
			Assert.AreEqual(8, blob.Value.LengthBits);

			var padding = dom.dataModels[0][1];
			Assert.AreEqual(8, padding.Value.LengthBits);
		}
	}
}

// end
