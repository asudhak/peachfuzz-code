
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
		[Test]
		public void CrackSizedString()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String length=\"5\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("Hello", (string)dom.dataModels[0][0].DefaultValue);
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

			Assert.AreEqual("Hello World", (string)dom.dataModels[0][0].DefaultValue);
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


		[Test]
		public void CrackString8()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String nullTerminated=\"true\" length=\"8\" value=\"Foo\" />" +
				"		<String />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello"));
			data.WriteByte(0);
			data.WriteByte(0);
			data.WriteByte(0);
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Foo Bar"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("Hello\0\0\0", (string)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual("Foo Bar", (string)dom.dataModels[0][1].DefaultValue);
		}

		[Test]
		public void CrackAsciiLengthChars()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String type=\"utf8\" />" +
				"		<String type=\"ascii\" length=\"5\" lengthType=\"chars\" />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.WriteBytes(Encoding.UTF8.GetBytes("Hello World"));
			data.WriteBytes(Encoding.ASCII.GetBytes("12345"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("Hello World", (string)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual("12345", (string)dom.dataModels[0][1].DefaultValue);
		}

		[Test]
		public void CrackAsciiVariableChars()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String type=\"utf8\" />" +
				"		<String type=\"utf32\" length=\"5\" lengthType=\"chars\" />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.WriteBytes(Encoding.UTF8.GetBytes("Hello World"));
			data.WriteBytes(Encoding.ASCII.GetBytes("12345"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			Assert.Throws<CrackingFailure>(delegate() {
				cracker.CrackData(dom.dataModels[0], data);
			});
		}

		[Test]
		public void CrackAsciiInvalidChars()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String type=\"ascii\" length=\"5\" lengthType=\"chars\" />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.WriteBytes(Encoding.ASCII.GetBytes("1234"));
			data.WriteByte(0xff);
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			Assert.Throws<CrackingFailure>(delegate()
			{
				cracker.CrackData(dom.dataModels[0], data);
			});
		}

		[Test]
		public void CrackAsciiLengthCharsBefore()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String type=\"ascii\" length=\"5\" lengthType=\"chars\" />" +
				"		<String type=\"utf8\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.WriteBytes(Encoding.ASCII.GetBytes("12345"));
			data.WriteBytes(Encoding.UTF8.GetBytes("Hello World"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("12345", (string)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual("Hello World", (string)dom.dataModels[0][1].DefaultValue);
		}

		[Test]
		public void CrackUtf32LengthCharsBefore()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String type=\"utf32\" length=\"5\" lengthType=\"chars\" />" +
				"		<String type=\"utf8\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.WriteBytes(Encoding.UTF32.GetBytes("12345"));
			data.WriteBytes(Encoding.UTF8.GetBytes("Hello World"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("12345", (string)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual("Hello World", (string)dom.dataModels[0][1].DefaultValue);
		}

		[Test]
		public void CrackUtf16LengthCharsBefore()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String type=\"utf16\" length=\"5\" lengthType=\"chars\" />" +
				"		<String type=\"utf8\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.WriteBytes(Encoding.Unicode.GetBytes("12345"));
			data.WriteBytes(Encoding.UTF8.GetBytes("Hello World"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("12345", (string)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual("Hello World", (string)dom.dataModels[0][1].DefaultValue);
		}

		[Test]
		public void CrackUtf8LengthCharsBefore()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String type=\"utf8\" length=\"1\" lengthType=\"chars\" />" +
				"		<String type=\"utf8\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.WriteBytes(Encoding.UTF8.GetBytes("\u30ab"));
			data.WriteBytes(Encoding.UTF8.GetBytes("Hello World"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("\u30ab", (string)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual("Hello World", (string)dom.dataModels[0][1].DefaultValue);
		}

		[Test]
		public void CrackNullTermToken()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String value=\"Hello\" token=\"true\" nullTerminated=\"true\" />" +
				"		<String value=\"World\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.WriteBytes(Encoding.UTF8.GetBytes("Hello\0World"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("Hello", (string)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual("World", (string)dom.dataModels[0][1].DefaultValue);
		}

		[Test]
		public void CrackNullTermLengthToken()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String value=\"Hello\" length=\"6\" nullTerminated=\"true\" token=\"true\" />" +
				"		<String value=\"World\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.WriteBytes(Encoding.UTF8.GetBytes("Hello\0World"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("Hello\0", (string)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual("World", (string)dom.dataModels[0][1].DefaultValue);
		}

		[Test]
		public void CrackNullTermString()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String type=\"utf8\" nullTerminated=\"true\" />" +
				"		<String type=\"utf8\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.WriteBytes(Encoding.UTF8.GetBytes("\u00abX\u00abX\0"));
			data.WriteBytes(Encoding.UTF8.GetBytes("Hello World"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("\u00abX\u00abX", (string)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual("Hello World", (string)dom.dataModels[0][1].DefaultValue);
		}

		[Test]
		public void CrackTokenNext()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String type=\"utf8\"/>" +
				"		<String type=\"utf8\" value=\"\u00abX\" token=\"true\" />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(Encoding.UTF8.GetBytes(xml)));

			BitStream data = new BitStream();
			data.WriteBytes(Encoding.UTF8.GetBytes("Hello World"));
			data.WriteBytes(Encoding.UTF8.GetBytes("\u00abX"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("Hello World", (string)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual("\u00abX", (string)dom.dataModels[0][1].DefaultValue);
		}

		[Test]
		public void CrackLastUnsized()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String type=\"utf8\"/>" +
				"		<Number size=\"8\"/>" +
				"		<String type=\"utf8\" value=\"\u00abX\" token=\"true\" />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(Encoding.UTF8.GetBytes(xml)));

			BitStream data = new BitStream();
			data.WriteBytes(Encoding.UTF8.GetBytes("Hello World"));
			data.WriteByte(255);
			data.WriteBytes(Encoding.UTF8.GetBytes("\u00abX"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("Hello World", (string)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual(255, (int)dom.dataModels[0][1].DefaultValue);
			Assert.AreEqual("\u00abX", (string)dom.dataModels[0][2].DefaultValue);
		}

		[Test]
		public void CrackNumericalOn()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(Encoding.UTF8.GetBytes(xml)));

			Assert.False(dom.dataModels[0][0].Hints.ContainsKey("NumericalString"));

			BitStream data = new BitStream();
			data.WriteBytes(Encoding.UTF8.GetBytes("100"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.True(dom.dataModels[0][0].Hints.ContainsKey("NumericalString"));
		}

		[Test]
		public void CrackNumericalOff()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String value=\"100\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(Encoding.UTF8.GetBytes(xml)));

			Assert.True(dom.dataModels[0][0].Hints.ContainsKey("NumericalString"));

			BitStream data = new BitStream();
			data.WriteBytes(Encoding.UTF8.GetBytes("Hello World"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.False(dom.dataModels[0][0].Hints.ContainsKey("NumericalString"));
		}

		[Test]
		public void FieldNumerical()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String name=\"str\"/>" +
				"	</DataModel>" +
				"	<StateModel name=\"TheStateModel\" initialState=\"InitialState\">" +
				"		<State name=\"InitialState\">" +
				"			<Action type=\"output\">" +
				"				<DataModel ref=\"TheDataModel\"/>" +
				"				<Data>" +
				"					<Field name=\"str\" value=\"100\"/>" +
				"				</Data>" +
				"			</Action>" +
				"		</State>" +
				"	</StateModel>" +
				"	<Test name=\"Default\">" +
				"		<StateModel ref=\"TheStateModel\"/>" +
				"		<Publisher class=\"Null\"/>" +
				"		<Strategy class=\"RandomDeterministic\"/>" +
				"	</Test>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(Encoding.UTF8.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			Assert.False(dom.dataModels[0][0].Hints.ContainsKey("NumericalString"));
			Assert.True(dom.tests[0].stateModel.states["InitialState"].actions[0].dataModel[0].Hints.ContainsKey("NumericalString"));
		}

	}
}

// end
