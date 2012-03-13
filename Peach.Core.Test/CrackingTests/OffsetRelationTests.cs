
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
	public class OffsetRelationTests
	{
		[Test]
		public void BasicOffset()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number size=\"8\">" +
				"			<Relation type=\"offset\" of=\"Data\" />" +
				"		</Number>" +
				"		<Blob name=\"Data\" />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			byte[] offsetdata = ASCIIEncoding.ASCII.GetBytes("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

			BitStream data = new BitStream();
			data.WriteInt8((sbyte)(offsetdata.Length + 1));
			data.WriteBytes(offsetdata);
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(offsetdata.Length + 1, (int)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual(ASCIIEncoding.ASCII.GetBytes("Hello World"), (byte[])dom.dataModels[0][1].DefaultValue);
		}

		[Test]
		public void Basic2Offset()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Blob length=\"5\"/>" +
				"		<Number size=\"8\">" +
				"			<Relation type=\"offset\" of=\"Data\" />" +
				"		</Number>" +
				"		<Blob name=\"Data\" />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			byte[] otherdata = ASCIIEncoding.ASCII.GetBytes("12345");
			byte[] offsetdata = ASCIIEncoding.ASCII.GetBytes("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

			BitStream data = new BitStream();
			data.WriteBytes(otherdata);
			data.WriteInt8((sbyte)(offsetdata.Length + 1 + otherdata.Length));
			data.WriteBytes(offsetdata);
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(offsetdata.Length + 1 + otherdata.Length, (int)dom.dataModels[0][1].DefaultValue);
			Assert.AreEqual(ASCIIEncoding.ASCII.GetBytes("Hello World"), (byte[])dom.dataModels[0][2].DefaultValue);
		}

		[Test]
		public void RelativeOffset()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Blob length=\"5\"/>"+
				"		<Number size=\"8\">" +
				"			<Relation type=\"offset\" of=\"Data\" relative=\"true\" />" +
				"		</Number>" +
				"		<Blob name=\"Data\" />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			byte[] otherdata = ASCIIEncoding.ASCII.GetBytes("12345");
			byte[] offsetdata = ASCIIEncoding.ASCII.GetBytes("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

			BitStream data = new BitStream();
			data.WriteBytes(otherdata);
			data.WriteInt8((sbyte)offsetdata.Length);
			data.WriteBytes(offsetdata);
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(offsetdata.Length, (int)dom.dataModels[0][1].DefaultValue);
			Assert.AreEqual(ASCIIEncoding.ASCII.GetBytes("Hello World"), (byte[])dom.dataModels[0][2].DefaultValue);
		}

		[Test]
		public void RelativeToOffset()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Blob length=\"5\"/>" +
				"		<Number name=\"Size\" size=\"8\">" +
				"			<Relation type=\"offset\" of=\"Data\" relative=\"true\" relativeTo=\"RelData\" />" +
				"		</Number>" +
				"		<Blob length=\"5\"/>" +
				"		<Blob name=\"RelData\" length=\"5\"/>" +
				"		<Blob name=\"Data\" />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			byte[] otherdata = ASCIIEncoding.ASCII.GetBytes("12345");
			byte[] offsetdata = ASCIIEncoding.ASCII.GetBytes("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

			BitStream data = new BitStream();
			data.WriteBytes(otherdata);
			data.WriteInt8((sbyte)(offsetdata.Length + otherdata.Length));
			data.WriteBytes(otherdata);
			data.WriteBytes(otherdata); // RelData
			data.WriteBytes(offsetdata);
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(offsetdata.Length + otherdata.Length, (int)dom.dataModels[0]["Size"].DefaultValue);
			Assert.AreEqual(ASCIIEncoding.ASCII.GetBytes("Hello World"), (byte[])dom.dataModels[0]["Data"].DefaultValue);
		}
	}
}

// end
