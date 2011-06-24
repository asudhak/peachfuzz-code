
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
//   Michael Eddington (mike@phed.org)

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
namespace Peach.Core.Test.CrackingTests
{
	[TestFixture]
	public class TokenTests
	{
		[Test]
		public void CrackTokenNumber()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number size=\"16\" value=\"300\" token=\"true\" />" +
				"		<String value=\"Foo Bar\" />" +
				"	</DataModel>" +
				"</Peach>";
			{
				// Positive test

				PitParser parser = new PitParser();
				Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

				BitStream data = new BitStream();
				data.LittleEndian();
				data.WriteInt16(300);
				data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World"));
				data.SeekBits(0, SeekOrigin.Begin);

				DataCracker cracker = new DataCracker();
				cracker.CrackData(dom.dataModels[0], data);

				Assert.AreEqual(300, (int)dom.dataModels[0][0].DefaultValue);
				Assert.AreEqual("Hello World", (string)dom.dataModels[0][1].DefaultValue);
			}
			{
				// Negative test

				PitParser parser = new PitParser();
				Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

				BitStream data = new BitStream();
				data.LittleEndian();
				data.WriteInt16(200);
				data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World"));
				data.SeekBits(0, SeekOrigin.Begin);

				DataCracker cracker = new DataCracker();
				TestDelegate myTestDelegate = () => cracker.CrackData(dom.dataModels[0], data);
				Assert.Throws<CrackingFailure>(myTestDelegate);

				Assert.AreEqual(300, (int)dom.dataModels[0][0].DefaultValue);
				Assert.AreEqual("Foo Bar", (string)dom.dataModels[0][1].DefaultValue);
			}
		}

		// TODO - Create unicode token string tests!!

		[Test]
		public void CrackTokenString()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String name=\"String1\" value=\"300\" token=\"true\" />" +
				"		<String name=\"String2\" value=\"Foo Bar\" />" +
				"	</DataModel>" +
				"</Peach>";
			{
				// Positive test

				PitParser parser = new PitParser();
				Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

				BitStream data = new BitStream();
				data.LittleEndian();
				data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("300"));
				data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World"));
				data.SeekBits(0, SeekOrigin.Begin);

				DataCracker cracker = new DataCracker();
				cracker.CrackData(dom.dataModels[0], data);

				Assert.AreEqual("300", (string)dom.dataModels[0][0].DefaultValue);
				Assert.AreEqual("Hello World", (string)dom.dataModels[0][1].DefaultValue);
			}
			{
				// Negative test

				PitParser parser = new PitParser();
				Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

				BitStream data = new BitStream();
				data.LittleEndian();
				data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("200"));
				data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World"));
				data.SeekBits(0, SeekOrigin.Begin);

				DataCracker cracker = new DataCracker();
				TestDelegate myTestDelegate = () => cracker.CrackData(dom.dataModels[0], data);
				Assert.Throws<CrackingFailure>(myTestDelegate);

				Assert.AreEqual("300", (string)dom.dataModels[0][0].DefaultValue);
				Assert.AreEqual("Foo Bar", (string)dom.dataModels[0][1].DefaultValue);
			}
		}

		[Test]
		public void CrackTokenBlob()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Blob name=\"Blob1\" value=\"300\" token=\"true\" />" +
				"		<String name=\"String1\" value=\"Foo Bar\" />" +
				"	</DataModel>" +
				"</Peach>";
			{
				// Positive test

				PitParser parser = new PitParser();
				Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

				BitStream data = new BitStream();
				//data.LittleEndian();
				data.BigEndian();
				data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("300"));
				data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World"));
				data.SeekBits(0, SeekOrigin.Begin);

				DataCracker cracker = new DataCracker();
				cracker.CrackData(dom.dataModels[0], data);

				Assert.AreEqual(ASCIIEncoding.ASCII.GetBytes("300"), (byte[])dom.dataModels[0][0].DefaultValue);
				Assert.AreEqual("Hello World", (string)dom.dataModels[0][1].DefaultValue);
			}
			{
				// Negative test

				PitParser parser = new PitParser();
				Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

				BitStream data = new BitStream();
				data.LittleEndian();
				data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("200"));
				data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World"));
				data.SeekBits(0, SeekOrigin.Begin);

				DataCracker cracker = new DataCracker();
				TestDelegate myTestDelegate = () => cracker.CrackData(dom.dataModels[0], data);
				Assert.Throws<CrackingFailure>(myTestDelegate);

				Assert.AreEqual(ASCIIEncoding.ASCII.GetBytes("300"), (byte[])dom.dataModels[0][0].DefaultValue);
				Assert.AreEqual("Foo Bar", (string)dom.dataModels[0][1].DefaultValue);
			}
		}

		[Test]
		public void CrackTokenFlag()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Flags name=\"Flags1\" size=\"8\">"+
				"			<Flag name=\"Flag1\" size=\"1\" position=\"0\" value=\"1\" token=\"true\"/> "+
				"		</Flags>"+
				"		<String value=\"Foo Bar\" />" +
				"	</DataModel>" +
				"</Peach>";
			{
				// Positive test

				PitParser parser = new PitParser();
				Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

				BitStream data = new BitStream();
				data.WriteUInt8(255);
				data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World"));
				data.SeekBits(0, SeekOrigin.Begin);

				DataCracker cracker = new DataCracker();
				cracker.CrackData(dom.dataModels[0], data);

				Assert.AreEqual(1, (int)((Flags)dom.dataModels[0][0])[0].DefaultValue);
				Assert.AreEqual("Hello World", (string)dom.dataModels[0][1].DefaultValue);
			}
			{
				// Negative test

				PitParser parser = new PitParser();
				Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

				BitStream data = new BitStream();
				data.LittleEndian();
				data.WriteInt16(0);
				data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World"));
				data.SeekBits(0, SeekOrigin.Begin);

				DataCracker cracker = new DataCracker();
				TestDelegate myTestDelegate = () => cracker.CrackData(dom.dataModels[0], data);
				Assert.Throws<CrackingFailure>(myTestDelegate);

				Assert.AreEqual(1, (int)((Flags)dom.dataModels[0][0])[0].DefaultValue);
				Assert.AreEqual("Foo Bar", (string)dom.dataModels[0][1].DefaultValue);
			}
		}
	}
}
