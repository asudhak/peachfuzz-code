
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
			Assert.IsTrue(str.stringType == StringType.Utf8);
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
