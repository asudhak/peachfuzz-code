
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

namespace Peach.Core.Test
{
	[TestFixture]
	class RelationOffsetTest : DataModelCollector
	{
		[Test]
		public void BasicTest()
		{
			string xml = @"
<Peach>
	<DataModel name='Block'>
		<Number size='32' endian='big'>
			<Relation type='offset' of='StringData' relative='true' relativeTo='TheDataModel'/>
		</Number>
		<Number size='32' endian='big'>
			<Relation type='size' of='StringData'/>
		</Number>
		<String name='StringData' value='test'/>
	</DataModel>

	<DataModel name='TheDataModel'>
		<String value='1234'/>
		<Block ref='Block'/>
	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			Assert.AreEqual(2, dom.dataModels.Count);

			var dm = dom.dataModels[1];
			Assert.AreEqual("TheDataModel", dm.name);

			// "1234   12    4    test"
			byte[] expected = new byte[] { 49, 50, 51, 52, 0, 0, 0, 12, 0, 0, 0, 4, 116, 101, 115, 116 };
			byte[] actual = dm.Value.ToArray();
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void FlagsTest()
		{
			string xml = @"
<Peach>
	<DataModel name='Block'>
		<Number size='32' endian='big'>
			<Relation type='offset' of='StringData' relative='true' relativeTo='TheDataModel'/>
		</Number>
		<Flags size='16' endian='big'>
		</Flags>
		<Number size='16' endian='big'>
			<Relation type='size' of='StringData'/>
		</Number>
		<String name='StringData' value='test'/>
	</DataModel>

	<DataModel name='TheDataModel'>
		<String value='1234'/>
		<Block ref='Block'/>
	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			Assert.AreEqual(2, dom.dataModels.Count);

			var dm = dom.dataModels[1];
			Assert.AreEqual("TheDataModel", dm.name);

			// "1234   12    4    test"
			byte[] expected = new byte[] { 49, 50, 51, 52, 0, 0, 0, 12, 0, 0, 0, 4, 116, 101, 115, 116 };
			byte[] actual = dm.Value.ToArray();
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void RefTest()
		{
			string xml = @"
<Peach>
	<DataModel name='Block'>
		<Number size='32' endian='big'>
			<Relation type='offset' of='StringData' relative='true' relativeTo='Block'/>
		</Number>

		<Number size='32' endian='big'>
			<Relation type='size' of='StringData'/>
		</Number>

		<String name='StringData' value='test'/>
	</DataModel>

	<DataModel name='TheDataModel'>
		<String value='1234'/>
		<Block ref='Block'/>
	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			Assert.AreEqual(2, dom.dataModels.Count);

			var dm = dom.dataModels[1];
			Assert.AreEqual("TheDataModel", dm.name);

			// "1234   12    4    test"
			byte[] expected = new byte[] { 49, 50, 51, 52, 0, 0, 0, 8, 0, 0, 0, 4, 116, 101, 115, 116 };
			byte[] actual = dm.Value.ToArray();
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void RefTest2()
		{
			string xml = @"
<Peach>
	<DataModel name='Block'>
		<Number size='32' endian='big'>
			<Relation type='offset' of='StringData' relative='true' relativeTo='Proxy'/>
		</Number>

		<Number size='32' endian='big'>
			<Relation type='size' of='StringData'/>
		</Number>

		<String name='StringData' value='test'/>
	</DataModel>

	<DataModel name='Proxy'>
		<Block ref='Block'/>
	</DataModel>

	<DataModel name='TheDataModel'>
		<String value='1234'/>
		<Block ref='Proxy'/>
	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			Assert.AreEqual(3, dom.dataModels.Count);

			var dm = dom.dataModels[2];
			Assert.AreEqual("TheDataModel", dm.name);

			// "1234   12    4    test"
			byte[] expected = new byte[] { 49, 50, 51, 52, 0, 0, 0, 8, 0, 0, 0, 4, 116, 101, 115, 116 };
			byte[] actual = dm.Value.ToArray();
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void RefTest3()
		{
			string xml = @"
<Peach>
	<DataModel name='Block'>
		<Number name='BlockSize' size='32' signed='false' endian='big'>
			<Relation type='size' of='Block0' expressionGet='size' expressionSet='size+4'/>
		</Number>

		<Block name='Block0'>
			<Block name='TagTable'>
				<Number name='TagCount' size='32' signed='false' endian='big'>
					<Relation type='size' of='Tags' expressionGet='size' expressionSet='size/12'/>
				</Number>
				<Block name='Tags'>
					<Block name='Tag0'>
						<String value='Tag0'/>
						<Number size='32' signed='false' endian='big'>
							<Relation type='offset' of='Data' relative='true' relativeTo='BlockSize'/>
						</Number>
						<Number size='32' signed='false' endian='big'>
							<Relation type='size' of='Data'/>
						</Number>
					</Block>
					<Block name='Tag1'>
						<String value='Tag1'/>
						<Number size='32' signed='false' endian='big'>
							<Relation type='offset' of='Data' relative='true' relativeTo='BlockSize'/>
						</Number>
						<Number size='32' signed='false' endian='big'>
							<Relation type='size' of='Data'/>
						</Number>
					</Block>
				</Block>
			</Block>

			<Block name='TagData'>
				<Block name='Data'>
					<String value='test'/>
				</Block>
			</Block>
		</Block>
	</DataModel>

	<DataModel name='TheDataModel'>
		<Block ref='Block'/>
	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			Assert.AreEqual(2, dom.dataModels.Count);

			var dm = dom.dataModels[1];
			Assert.AreEqual("TheDataModel", dm.name);

			// "1234   12    4    test"
			byte[] expected = new byte[] {
				0,  0,  0, 36,    0,  0,  0,  2,   84, 97,103, 48,
				0,  0,  0, 32,    0,  0,  0,  4,   84, 97,103, 49,
				0,  0,  0, 32,    0,  0,  0,  4,  116,101,115,116,
			};
			byte[] actual = dm.Value.ToArray();
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void TestFuzz()
		{
			string xml = @"
<Peach>
	<DataModel name='TheDataModel'>
		<Number name='num' size='32' signed='false' endian='big'>
			<Relation type='offset' of='blob'/>
		</Number>
		<String name='str'/>
		<Blob name='blob'/>
	</DataModel>

	<StateModel name='TheState' initialState='Initial'>
		<State name='Initial'>
			<Action type='output'>
				<DataModel ref='TheDataModel'/>
			</Action>
		</State>
	</StateModel>

	<Test name='Default'>
		<StateModel ref='TheState'/>
		<Publisher class='Null'/>
		<Strategy class='Sequential'/>
	</Test>
</Peach>";

			PitParser parser = new PitParser();

			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			dom.tests[0].includedMutators = new List<string>();
			dom.tests[0].includedMutators.Add("StringMutator");

			RunConfiguration config = new RunConfiguration();

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			Assert.AreEqual(2380, dataModels.Count);

			foreach (var dm in dataModels)
			{
				var val = dm.Value;
				var len = val.LengthBits;
				Assert.GreaterOrEqual(len, 32);

				val.Seek(0, SeekOrigin.Begin);
				var rdr = new BitReader(val);
				rdr.BigEndian();
				uint offset = rdr.ReadUInt32();

				Assert.AreEqual(len, offset * 8);
			}
		}

		[Test]
		public void TestRelativeTo()
		{
			string xml = @"
<Peach>
	<DataModel name='TheDataModel'>
		<Number name='len' size='32' signed='false' endian='big'>
			<Relation type='size' of='begin'/>
		</Number>
		<String name='begin'/>
		<String name='eol' mutable='false' value='\r\n'/>
		<Number name='num' size='32' signed='false' endian='big'>
			<Relation type='offset' of='blob' relativeTo='eol'/>
		</Number>
		<String name='str'/>
		<Blob name='blob'/>
	</DataModel>

	<StateModel name='TheState' initialState='Initial'>
		<State name='Initial'>
			<Action type='output'>
				<DataModel ref='TheDataModel'/>
			</Action>
		</State>
	</StateModel>

	<Test name='Default'>
		<StateModel ref='TheState'/>
		<Publisher class='Null'/>
		<Strategy class='Sequential'/>
	</Test>
</Peach>";

			PitParser parser = new PitParser();

			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			dom.tests[0].includedMutators = new List<string>();
			dom.tests[0].includedMutators.Add("StringMutator");

			RunConfiguration config = new RunConfiguration();

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			Assert.AreEqual(4759, dataModels.Count);

			foreach (var dm in dataModels)
			{
				var val = dm.Value;
				var len = val.LengthBits;
				Assert.GreaterOrEqual(len, 32);

				val.Seek(0, SeekOrigin.Begin);
				var rdr = new BitReader(val);
				rdr.BigEndian();
				uint beginLen = rdr.ReadUInt32();

				val.Seek(beginLen + 2, SeekOrigin.Current);
				uint offset = rdr.ReadUInt32();

				Assert.AreEqual((4 + beginLen + offset) * 8, len);
			}
		}

		[Test]
		public void TestRelative()
		{
			string xml = @"
<Peach>
	<DataModel name='TheDataModel'>
		<Number name='len' size='32' signed='false' endian='big'>
			<Relation type='size' of='begin'/>
		</Number>
		<String name='begin'/>
		<String name='eol' mutable='false' value='\r\n'/>
		<Number name='num' size='32' signed='false' endian='big'>
			<Relation type='offset' of='blob' relative='true'/>
		</Number>
		<String name='str'/>
		<Blob name='blob'/>
	</DataModel>

	<StateModel name='TheState' initialState='Initial'>
		<State name='Initial'>
			<Action type='output'>
				<DataModel ref='TheDataModel'/>
			</Action>
		</State>
	</StateModel>

	<Test name='Default'>
		<StateModel ref='TheState'/>
		<Publisher class='Null'/>
		<Strategy class='Sequential'/>
	</Test>
</Peach>";

			PitParser parser = new PitParser();

			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			dom.tests[0].includedMutators = new List<string>();
			dom.tests[0].includedMutators.Add("StringMutator");

			RunConfiguration config = new RunConfiguration();

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			Assert.AreEqual(4759, dataModels.Count);

			foreach (var dm in dataModels)
			{
				var val = dm.Value;
				var len = val.LengthBits;
				Assert.GreaterOrEqual(len, 32);

				val.Seek(0, SeekOrigin.Begin);
				var rdr = new BitReader(val);
				rdr.BigEndian();
				uint beginLen = rdr.ReadUInt32();

				val.Seek(beginLen + 2, SeekOrigin.Current);
				uint offset = rdr.ReadUInt32();

				Assert.AreEqual((4 + beginLen + 2 + offset) * 8, len);
			}
		}
	}
}
// end
