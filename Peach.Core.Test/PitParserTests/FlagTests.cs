
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
	public class FlagTests
	{
		[Test]
		public void SimpleFlags()
		{
			string xml = @"
<Peach>
	<DataModel name=""DM"">
		<Flags size=""8"">
			<Flag position=""0"" size=""1""/>
			<Flag position=""1"" size=""1""/>
			<Flag position=""2"" size=""1""/>
			<Flag position=""3"" size=""1""/>
			<Flag position=""4"" size=""1""/>
			<Flag position=""5"" size=""1""/>
			<Flag position=""6"" size=""1""/>
			<Flag position=""7"" size=""1""/>
		</Flags>
	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			Assert.AreEqual(1, dom.dataModels.Count);
			Assert.AreEqual(1, dom.dataModels[0].Count);

			Flags flags = dom.dataModels[0][0] as Flags;

			Assert.NotNull(flags);
			Assert.AreEqual(8, flags.Count);
		}

		private void RunOverlap(int pos1, int size1, int pos2, int size2)
		{
			string template = @"
<Peach>
	<DataModel name=""DM"">
		<Flags size=""8"">
			<Flag position=""{0}"" size=""{1}""/>
			<Flag position=""{2}"" size=""{3}""/>
		</Flags>
	</DataModel>
</Peach>";

			string xml = string.Format(template, pos1, size1, pos2, size2);

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			Assert.AreEqual(1, dom.dataModels.Count);
			Assert.AreEqual(1, dom.dataModels[0].Count);

			Flags flags = dom.dataModels[0][0] as Flags;

			Assert.NotNull(flags);
			Assert.AreEqual(2, flags.Count);
		}

		[Test]
		public void TestOverlap()
		{
			Assert.Throws<PeachException>(delegate() { RunOverlap(0, 1, 8, 1); } );
			Assert.Throws<PeachException>(delegate() { RunOverlap(0, 1, 7, 2); });
			Assert.Throws<PeachException>(delegate() { RunOverlap(0, 4, 3, 1); });
			Assert.Throws<PeachException>(delegate() { RunOverlap(0, 4, 3, 2); });
			Assert.Throws<PeachException>(delegate() { RunOverlap(0, 4, 3, 3); });
			Assert.Throws<PeachException>(delegate() { RunOverlap(0, 8, 3, 3); });
			Assert.Throws<PeachException>(delegate() { RunOverlap(5, 2, 3, 3); });

			RunOverlap(0, 6, 7, 1);
			RunOverlap(7, 1, 0, 6);
		}

		private void DoEndian(string endian, byte[] expected)
		{
			string template = @"
<Peach>
	<DataModel name=""DM"">
		<Flags size=""16"" endian=""{0}"">
			<Flag position=""0""  size=""4"" value=""10""/>
			<Flag position=""4""  size=""4"" value=""11""/>
			<Flag position=""8""  size=""4"" value=""12""/>
			<Flag position=""12"" size=""4"" value=""13""/>
		</Flags>
	</DataModel>
</Peach>";

			string xml = string.Format(template, endian);
			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			Assert.AreEqual(1, dom.dataModels.Count);


			Flags flags = dom.dataModels[0][0] as Flags;

			Assert.NotNull(flags);
			Assert.AreEqual(4, flags.Count);

			var value = dom.dataModels[0].Value;

			Assert.NotNull(value);
			Assert.AreEqual(2, value.LengthBytes);

			MemoryStream ms = value.Stream as MemoryStream;
			Assert.NotNull(ms);

			byte[] actual = new byte[expected.Length];
			Buffer.BlockCopy(ms.GetBuffer(), 0, actual, 0, actual.Length);
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void TestEndian()
		{
			DoEndian("little", new byte[] { 0xdc, 0xba });
			DoEndian("big", new byte[] { 0xab, 0xcd });
		}

		[Test]
		public void TestRelation()
		{
			string xml = @"
<Peach>
	<DataModel name=""DM"">
		<Flags size=""16"" endian=""big"">
			<Flag position=""0"" size=""7"" value=""1""/>
			<Flag position=""7"" size=""9"">
				<Relation type=""size"" of=""blob""/>
			</Flag>
		</Flags>
		<Blob name=""blob"" length=""100""/>
	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			Assert.AreEqual(1, dom.dataModels.Count);

			var value = dom.dataModels[0].Value;

			Assert.NotNull(value);
			Assert.AreEqual(102, value.LengthBytes);

			MemoryStream ms = value.Stream as MemoryStream;
			Assert.NotNull(ms);

			Assert.AreEqual(2, ms.GetBuffer()[0]);
			Assert.AreEqual(100, ms.GetBuffer()[1]);
		}
	}
}
