
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
	public class ArrayTests
	{

		[Test]
		public void CrackUrl()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String />" +
				"		<String value=\"://\" token=\"true\" />" +
				"		<String />" +
				"		<String value=\"/\" token=\"true\" />" +
				"		<String />" +
				"		<String value=\"?\" token=\"true\" />" +

				"		<Block name=\"TheArray\" minOccurs=\"0\" maxOccurs=\"100\">" +
				"		  <String name=\"key1\" />" +
				"		  <String value=\"=\" token=\"true\" />" +
				"		  <String name=\"value1\" />" +
				"			<String value=\"&amp;\" token=\"true\" />" +
				"		</Block>" +

				"		<Block name=\"EndBlock\">" +
				"		  <String name=\"key2\" />" +
				"		  <String value=\"=\" token=\"true\" />" +
				"		  <String name=\"value2\" />" +
				"		</Block>" +
				"	</DataModel>" +
				"</Peach>";

			// Positive test

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("http://www.foo.com/crazy/path.cgi?k1=v1&k2=v2&k3=v3"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(2, ((Dom.Array)dom.dataModels[0]["TheArray"]).Count);
			Assert.AreEqual("k3", (string)((Dom.Block)dom.dataModels[0]["EndBlock"])["key2"].InternalValue);
			Assert.AreEqual("v3", (string)((Dom.Block)dom.dataModels[0]["EndBlock"])["value2"].InternalValue);
		}

		[Test]
		public void CrackUrl2()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String value=\"?\" token=\"true\" />" +

				"		<Block name=\"TheArray\" minOccurs=\"0\" maxOccurs=\"100\">" +
				"		  <String name=\"key1\" />" +
				"		  <String value=\"=\" token=\"true\" />" +
				"		  <String name=\"value1\" />" +
				"			<String value=\"&amp;\" token=\"true\" />" +
				"		</Block>" +
				"		<String name=\"key2\" />" +
				"		<String value=\"=\" token=\"true\" />" +
				"		<String name=\"value2\" />" +
				"	</DataModel>" +
				"</Peach>";

			// Positive test

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("?k1=v1&k2=v2&k3=v3"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(2, ((Dom.Array)dom.dataModels[0]["TheArray"]).Count);
			Assert.AreEqual("k3", (string)dom.dataModels[0]["key2"].InternalValue);
			Assert.AreEqual("v3", (string)dom.dataModels[0]["value2"].InternalValue);
		}

		[Test]
		public void CrackArrayBlob1()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Blob length=\"1\" minOccurs=\"1\" maxOccurs=\"100\" />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(new byte[] { 1, 2, 3, 4, 5, 6 });
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Dom.Array array = (Dom.Array)dom.dataModels[0][0];

			Assert.AreEqual(6, array.Count);
			Assert.AreEqual(new byte[] { 1 }, (byte[])array[0].InternalValue);
			Assert.AreEqual(new byte[] { 6 }, (byte[])array[5].InternalValue);
		}

		/// <summary>
		/// We should stop cracking at maxOccurs.  Question, should we throw an exception or just
		/// stop?
		/// </summary>
		[Test]
		public void CrackArrayStopAtMax()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Blob length=\"1\" minOccurs=\"1\" maxOccurs=\"3\" />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(new byte[] { 1, 2, 3, 4, 5, 6 });
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Dom.Array array = (Dom.Array)dom.dataModels[0][0];

			Assert.AreEqual(3, array.Count);
			Assert.AreEqual(new byte[] { 1 }, (byte[])array[0].InternalValue);
			Assert.AreEqual(new byte[] { 2 }, (byte[])array[1].InternalValue);
			Assert.AreEqual(new byte[] { 3 }, (byte[])array[2].InternalValue);
		}

		[Test]
		public void CrackArrayVerifyMin()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Blob length=\"1\" minOccurs=\"4\" maxOccurs=\"6\" />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(new byte[] { 1, 2, 3 });
			data.SeekBits(0, SeekOrigin.Begin);

			try
			{
				DataCracker cracker = new DataCracker();
				cracker.CrackData(dom.dataModels[0], data);
				Assert.True(false);
			}
			catch (CrackingFailure)
			{
				Assert.True(true);
			}
		}

		[Test]
		public void CrackArrayBlobZeroMore()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Blob length=\"1\" minOccurs=\"0\" maxOccurs=\"100\" />" +
				"		<Blob name=\"Rest\" length=\"6\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(new byte[] { 1, 2, 3, 4, 5, 6 });
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Dom.Array array = (Dom.Array)dom.dataModels[0][0];

			Assert.AreEqual(0, array.Count);

			Blob rest = (Blob)dom.dataModels[0].find("Rest");

			Assert.AreEqual(new byte[] { 1, 2, 3, 4, 5, 6 }, (byte[])rest.InternalValue);
		}

		[Test]
		public void CrackArrayRelation()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number size=\"8\">" +
				"			<Relation type=\"count\" of=\"TheArray\"/>" +
				"		</Number>" +
				"		<Blob name=\"TheArray\" length=\"1\" minOccurs=\"0\" maxOccurs=\"100\" />" +
				"		<Blob name=\"Rest\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(new byte[] { 3, 1, 2, 3, 4, 5, 6 });
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Dom.Array array = (Dom.Array)dom.dataModels[0][1];
			Blob blob = (Blob)dom.dataModels[0][2];

			Assert.AreEqual(3, array.Count);
			Assert.AreEqual(new byte[] { 1 }, (byte[])array[0].InternalValue);
			Assert.AreEqual(new byte[] { 2 }, (byte[])array[1].InternalValue);
			Assert.AreEqual(new byte[] { 3 }, (byte[])array[2].InternalValue);
			Assert.AreEqual(new byte[] { 4, 5, 6 }, (byte[])blob.InternalValue);
		}

		[Test]
		public void CrackArrayOfOne()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String name=\"str1\" nullTerminated=\"true\" minOccurs=\"1\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(Encoding.ASCII.GetBytes("Hello\x00"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(1, dom.dataModels[0].Count);
			Dom.Array array = (Dom.Array)dom.dataModels[0][0];
			Assert.AreEqual(1, array.Count);
			string str = (string)array[0].InternalValue;

			Assert.AreEqual("Hello", str);
		}
	}
}

// end
