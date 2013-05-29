
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
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Peach.Core;
using Peach.Core.IO;
using Peach.Core.Dom;
using Peach.Core.Cracker;
using Peach.Core.Analyzers;

using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core.Dom.XPath;

namespace Peach.Core.Test
{
	[TestFixture]
	class ReportedTests
	{
		/// <summary>
		/// From Sirus.
		/// </summary>
		/// <remarks>
		/// input data attached.   For some reason stalls in the ObjectCopier clone method 
		/// trying to deserialze a 120 megabyte (!) memory stream of a Dom.Number... Wonder 
		/// what the heck the serializer is doing..
		/// </remarks>
		[Test]
		public void ObjectCopierExplode()
		{
			string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Peach>
<DataModel name=""GeneratedModel"">
            <Block name=""0"">
                <Number name=""1"" signed=""false"" size=""32""/>
                <Block name=""2"">
                    <Number name=""3"" signed=""false"" size=""32""/>
                    <Number name=""4"" maxOccurs=""9999"" minOccurs=""0"" signed=""false"" size=""32"">
                        <Relation type=""count"" of=""5""/>
                    </Number>
                    <Blob name=""5""/>
                </Block>
            </Block>
    </DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			Assert.NotNull(dom);

			var newDom = dom.dataModels[0].Clone();

			Assert.NotNull(newDom);
		}

		/// <summary>
		/// Reported by Sirus
		/// </summary>
		[Test, ExpectedException(typeof(CrackingFailure), ExpectedMessage = "Block 'GeneratedModel.0.2' has length of 5381942480 bits, already read 64 bits, but buffer only has 40 bits left.")]
		public void CrackExplode()
		{
			string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Peach>
<DataModel name=""GeneratedModel"">
            <Block name=""0"">
                <Number name=""1"" size=""32""/>
                <Block name=""2"">
                    <Relation type=""size"" from=""4""/>
                    <Number name=""3"" size=""32""/>
                    <Number name=""4"" size=""32"">
                        <Relation type=""size"" of=""2""/>
                    </Number>
                </Block>
            </Block>
    </DataModel>
</Peach>
";
			byte[] dataBytes = new byte[] { 
						0x5f,						0xfa,
						0x8a,						0x68,
						0x09,						0x00,
						0x00,						0x00,
						0x9a,						0x3d,
						0x19,						0x28,
						0xc7,						0x1c,
						0x03,						0x9a,
						0xa6 };

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(dataBytes);
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);
		}

		/// <summary>
		/// Proper data bytes for model of CrackExplode
		/// </summary>
		[Test]
		public void CrackNoExplode()
		{
			string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Peach>
<DataModel name=""GeneratedModel"">
            <Block name=""0"">
                <Number name=""1"" size=""32""/>
                <Block name=""2"">
                    <Relation type=""size"" from=""4""/>
                    <Number name=""3"" size=""32""/>
                    <Number name=""4"" size=""32"">
                        <Relation type=""size"" of=""2""/>
                    </Number>
                </Block>
            </Block>
    </DataModel>
</Peach>
";
			byte[] dataBytes = new byte[] { 
						0x5f,						0xfa,
						0x8a,						0x68,
						0x9a,						0x3d,
						0x19,						0x28,
						0x09,						0x00,
						0x00,						0x00,
						0xc7,						0x1c,
						0x03,						0x9a,
						0xa6 };

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(dataBytes);
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.NotNull(dom);

			var newDom = dom.dataModels[0].Clone();

			Assert.NotNull(newDom);
		}

		/// <summary>
		/// Reported by Sirus
		/// </summary>
		[Test, ExpectedException(typeof(CrackingFailure), ExpectedMessage = "Block 'GeneratedModel.0.1' has length of 8 bits but already read 64 bits.")]
		public void CrackExplode2()
		{
			string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Peach>
<DataModel name=""GeneratedModel"">
            <Block name=""0"">
                <Block name=""1"">
                    <Relation type=""size"" from=""4""/>
                    <Number name=""3"" size=""32"">
                        <Relation type=""size"" of=""2""/>
                    </Number>
                    <Number name=""4"" size=""32"">
                        <Relation type=""size"" of=""1""/>
                    </Number>
                </Block>
                <Blob name=""2"">
                    <Relation type=""size"" from=""3""/>
                </Blob>
            </Block>
    </DataModel>
</Peach>";
			byte[] dataBytes = new byte[] { 
						0x1b,
						0x53,    0xcb,
						0x22,    0x01,
						0x00,    0x00,
						0x00,    0x05 };

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(dataBytes);
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);
		}


		[Test]
		public void BugUtf16Length()
		{
			string xml = @"<?xml version='1.0' encoding='UTF-8'?>
<Peach>
	<DataModel name='bug_utf16_length'>
		<String name='signature' token='true' value='START_MARKER'/>

		<Number name='FILENAME_LENGTH' endian='little' size='16' signed='false' occurs='1'>
			<Relation of='FILENAME' type='size'/>
		</Number>

		<Number name='OBJECT_LENGTH' endian='little' size='32' signed='false' occurs='1'>
			<Relation of='OBJECT' type='size'/>
		</Number>

		<String name='FILENAME' occurs='1' nullTerminated='false'>
			<Transformer class='encode.Utf16'/>
		</String>

		<Block name='OBJECT' occurs='1'>
			<String value='END_MARKER'/>
		</Block>
	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			byte[] FILENAME = Encoding.Unicode.GetBytes("sample_mpeg4.mp4");
			byte[] OBJECT = Encoding.ASCII.GetBytes("END_MARKER");

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(Encoding.ASCII.GetBytes("START_MARKER"));
			data.WriteInt16((short)FILENAME.Length);
			data.WriteInt32(OBJECT.Length);
			data.WriteBytes(FILENAME);
			data.WriteBytes(OBJECT);
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(5, dom.dataModels[0].Count);
			Assert.AreEqual("START_MARKER", (string)dom.dataModels[0][0].DefaultValue);

			var array = dom.dataModels[0][1] as Dom.Array;
			Assert.NotNull(array);
			Assert.AreEqual(1, array.Count);
			Assert.AreEqual(FILENAME.Length, (int)array[0].DefaultValue);

			array = dom.dataModels[0][2] as Dom.Array;
			Assert.NotNull(array);
			Assert.AreEqual(1, array.Count);
			Assert.AreEqual(OBJECT.Length, (int)array[0].DefaultValue);

			array = dom.dataModels[0][3] as Dom.Array;
			Assert.NotNull(array);
			Assert.AreEqual(1, array.Count);
			Assert.AreEqual("sample_mpeg4.mp4", (string)array[0].DefaultValue);

			array = dom.dataModels[0][4] as Dom.Array;
			Assert.NotNull(array);
			Assert.AreEqual(1, array.Count);
			var block = array[0] as Block;
			Assert.NotNull(block);
			Assert.AreEqual(1, block.Count);
			Assert.AreEqual("END_MARKER", (string)block[0].DefaultValue);

		}

		[Test]
		public void SlurpArray()
		{
			string xml = @"
<Peach>
	<DataModel name='TheDataModel'>
		<String name='str' value='Hello'/>
		<Block name='blk' minOccurs='1'>
			<String name='val'/>
		</Block>
	</DataModel>

	<StateModel name='TheState' initialState='State1'>
		<State name='State1'>
			<Action type='output'>
				<DataModel ref='TheDataModel'/>
			</Action>
		</State>
	</StateModel>

	<Test name='Default'>
		<StateModel ref='TheState'/>
		<Publisher class='Console'/>
	</Test>
</Peach>
";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			Dom.Array array = dom.tests[0].stateModel.states["State1"].actions[0].dataModel[1] as Dom.Array;
			array.ExpandTo(50);

			PeachXPathNavigator navi = new PeachXPathNavigator(dom);
			var iter = navi.Select("//str");
			if (!iter.MoveNext())
				Assert.Fail();

			DataElement valueElement = ((PeachXPathNavigator)iter.Current).currentNode as DataElement;
			if (valueElement == null)
				Assert.Fail();

			if (iter.MoveNext())
				Assert.Fail();

			iter = navi.Select("//val");

			if (!iter.MoveNext())
				Assert.Fail();

			int count = 0;
			do
			{
				var setElement = ((PeachXPathNavigator)iter.Current).currentNode as DataElement;
				if (setElement == null)
					Assert.Fail();

				setElement.DefaultValue = valueElement.DefaultValue;
				++count;
			}
			while (iter.MoveNext());

			// When Array.ExpandTo() is used, it duplicates the element and adds that
			// same element over and over, so there are really only 2 unique elements in the array...
			Assert.AreEqual(2, count);
			Assert.AreEqual(50, array.Count);

			int hash = 0;
			for (int i = 0; i < array.Count; ++i)
			{
				if (i <= 1)
					hash = array[i].GetHashCode();

				Assert.AreEqual(hash, array[i].GetHashCode());
				var b = array[i] as Block;
				Assert.NotNull(b);
				Assert.AreEqual("Hello", (string)b[0].DefaultValue);
			}
		}
	}
}
