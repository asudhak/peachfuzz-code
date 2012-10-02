
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

namespace Peach.Core.Test.PitParserTests
{
	[TestFixture]
	class ArrayTests
	{
		class Resetter : DataElement
		{
			public static void Reset()
			{
				DataElement._uniqueName = 0;
			}

			public override void Crack(Cracker.DataCracker context, IO.BitStream data)
			{
				throw new NotImplementedException();
			}

			public override object GetParameter(string parameterName)
			{
				throw new NotImplementedException();
			}
		}

		[Test]
		public void ArrayHintsTest()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Blob value=\"Hello World\" minOccurs=\"100\">" +
				"			<Hint name=\"Hello\" value=\"World\"/>"+
				"		</Blob>"+
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Dom.Array array = dom.dataModels[0][0] as Dom.Array;

			Assert.NotNull(array);
			Assert.AreEqual(1, array.Count);

			Assert.NotNull(array.Hints);
			Assert.AreEqual(1, array.Hints.Count);
			Assert.AreEqual("World", array.Hints["Hello"].Value);
		}

		[Test]
		public void ArrayNameTest()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Blob name=\"stuff\" value=\"Hello World\" minOccurs=\"100\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Dom.Array array = dom.dataModels[0][0] as Dom.Array;

			Assert.NotNull(array);
			Assert.AreEqual(1, array.Count);
			Assert.AreEqual("TheDataModel.stuff", array.fullName);
			Assert.AreEqual("TheDataModel.stuff.stuff", array[0].fullName);
			Assert.AreEqual(array, array[0].parent);
		}

		[Test]
		public void ArrayNoNameTest()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Blob value=\"Hello World\" minOccurs=\"100\"/>" +
				"	</DataModel>" +
				"</Peach>";

			Resetter.Reset();

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Dom.Array array = dom.dataModels[0][0] as Dom.Array;

			Assert.NotNull(array);
			Assert.AreEqual(1, array.Count);
			Assert.AreEqual("TheDataModel.DataElement_0.DataElement_0", array[0].fullName);
			Assert.AreEqual("TheDataModel.DataElement_0", array.fullName);
		}

		[Test]
		public void ArrayOfRelationTest()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number name=\"Length\" size=\"32\">" +
				"			<Relation type=\"size\" of=\"Data\" />" +
				"		</Number>" +
				"		<Blob name=\"Data\" value=\"Hello World\" minOccurs=\"100\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Dom.Array array = dom.dataModels[0][1] as Dom.Array;

			Assert.NotNull(array);
			Assert.AreEqual(1, array.Count);
			Assert.AreEqual(1, array.relations.Count);
			Assert.AreEqual(0, array[0].relations.Count);
		}

		[Test]
		public void ArrayFromRelationTest()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Blob name=\"Data\" value=\"Hello World\"/>" +
				"		<Number name=\"Length\" size=\"32\"  minOccurs=\"100\">" +
				"			<Relation type=\"size\" of=\"Data\" />" +
				"		</Number>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Dom.Array array = dom.dataModels[0][1] as Dom.Array;

			Assert.NotNull(array);
			Assert.AreEqual(1, array.Count);
			Assert.AreEqual(0, array.relations.Count);
			Assert.AreEqual(1, array[0].relations.Count);
		}

		[Test]
		public void TestArrayClone()
		{
			// If an array is cloned with a new name, the 1st element in the array needs
			// to have its name updated as well

			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Blob name=\"Data\" value=\"Hello World\" minOccurs=\"100\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Dom.Array array = dom.dataModels[0][0] as Dom.Array;

			Assert.NotNull(array);
			Assert.AreEqual(1, array.Count);
			Assert.AreEqual("Data", array.name);
			Assert.AreEqual("Data", array[0].name);

			var clone = array.Clone("NewData") as Dom.Array;

			Assert.NotNull(clone);
			Assert.AreEqual(1, clone.Count);
			Assert.AreEqual("NewData", clone.name);
			Assert.AreEqual("NewData", clone[0].name);
		}
	}
}
