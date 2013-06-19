
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
	public class PlacementTests
	{
		[Test]
		public void BasicAfter()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Block name=\"Block1\">" +
				"			<Blob name=\"Data\">" +
				"				<Placement after=\"Block1\"/>" +
				"			</Blob>" +
				"		</Block>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var data = Bits.Fmt("{0}", "Hello World");

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("Data", dom.dataModels[0][1].name);
			Assert.AreEqual("Hello World", dom.dataModels[0][1].DefaultValue.BitsToString());
		}

		[Test]
		public void BasicBefore()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Block name=\"Block\">" +
				"			<Blob name=\"Data\">" +
				"				<Placement before=\"Block1\"/>" +
				"			</Blob>" +
				"		</Block>" +
				"		<Block name=\"Block1\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var data = Bits.Fmt("{0}", "Hello World");

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("Data", dom.dataModels[0][1].name);
			Assert.AreEqual("Hello World", dom.dataModels[0][1].DefaultValue.BitsToString());
		}

		[Test]
		public void SameName()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Block name=\"Block1\">" +
				"			<Blob name=\"Data\">" +
				"				<Placement after=\"Block1\"/>" +
				"			</Blob>" +
				"		</Block>" +
				"		<Block name=\"Data\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var data = Bits.Fmt("{0}", "Hello World");

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("Data_0", dom.dataModels[0][1].name);
			Assert.AreEqual("Hello World", dom.dataModels[0][1].DefaultValue.BitsToString());
		}

		[Test]
		public void RelationTest()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String name=\"TheString\" length=\"2\">" +
				"			<Relation type=\"size\" of=\"Data\"/>" +
				"		</String>" +
				"		<Block name=\"Block1\">" +
				"			<Blob name=\"Data\">" +
				"				<Placement after=\"Block1\"/>" +
				"			</Blob>" +
				"		</Block>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var data = Bits.Fmt("{0}", "11Hello World");

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(1, dom.dataModels[0][0].relations.Count);
			Assert.AreEqual("TheDataModel.Data", dom.dataModels[0][0].relations[0].OfName);
			Assert.AreEqual("Hello World", dom.dataModels[0][2].DefaultValue.BitsToString());
		}

		[Test]
		public void FixupTest()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String name=\"TheString\" length=\"11\">" +
				"			<Fixup class=\"CopyValue\">" +
				"				<Param name=\"ref\" value=\"Data\"/>"+
				"			</Fixup>"+
				"		</String>" +
				"		<Block name=\"Block1\">" +
				"			<Blob name=\"Data\">" +
				"				<Placement after=\"Block1\"/>" +
				"			</Blob>" +
				"		</Block>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var data = Bits.Fmt("{0}", "HELLO WORLDHello World");

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(1, dom.dataModels[0][0].fixup.references.Count());
			var item = dom.dataModels[0][0].fixup.references.First();
			Assert.AreEqual("ref", item.Item1);
			Assert.AreEqual("TheDataModel.Data", item.Item2);
			Assert.AreEqual("Hello World", dom.dataModels[0][0].InternalValue.BitsToString());
			Assert.AreEqual("Hello World", dom.dataModels[0][2].DefaultValue.BitsToString());
		}

		[Test]
		public void RelationCloneTest()
		{
			// When the item is placed, it must be copied since an item of the same name will already exist
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Block name=\"Block1\">" +
				"			<String name=\"TheString\" length=\"2\">" +
				"				<Relation type=\"size\" of=\"Data\"/>" +
				"			</String>" +
				"			<Blob name=\"Data\">" +
				"				<Placement after=\"Block1\"/>" +
				"			</Blob>" +
				"		</Block>" +
				"		<Block name=\"Data\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var data = Bits.Fmt("{0}", "11Hello World");

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(3, dom.dataModels[0].Count);
			Assert.AreEqual("Block1", dom.dataModels[0][0].name);
			Assert.AreEqual("Data_0", dom.dataModels[0][1].name);
			Assert.AreEqual("Data", dom.dataModels[0][2].name);


			var Block1 = dom.dataModels[0][0] as DataElementContainer;
			Assert.NotNull(Block1);

			Assert.AreEqual(1, Block1.Count);
			Assert.AreEqual("TheString", Block1[0].name);

			Assert.AreEqual(1, Block1[0].relations.Count);
			Assert.AreEqual("TheDataModel.Data_0", Block1[0].relations[0].OfName);
		}

		[Test]
		public void FixupCloneTest()
		{
			// Verify fixups remain intact when the item is cloned during placement
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Block name=\"Block0\">" +
				"			<Number name=\"TheCRC\" size=\"32\">" +
				"				<Fixup class=\"Crc32DualFixup\">" +
				"					<Param name=\"ref1\" value=\"TheString\"/>" +
				"					<Param name=\"ref2\" value=\"Data\"/>" +
				"				</Fixup>" +
				"			</Number>" +
				"			<String name=\"TheString\" length=\"2\">" +
				"				<Relation type=\"size\" of=\"Data\"/>" +
				"			</String>" +
				"			<String name=\"Data\">" +
				"				<Placement before=\"Placement\"/>" +
				"			</String>" +
				"		</Block>" +
				"		<Block name=\"Block1\">" +
				"			<Number name=\"TheCRC\" size=\"32\">" +
				"				<Fixup class=\"Crc32Fixup\">" +
				"					<Param name=\"ref\" value=\"Data\"/>" +
				"				</Fixup>" +
				"			</Number>" +
				"			<String name=\"TheString\" length=\"2\">" +
				"				<Relation type=\"size\" of=\"Data\"/>" +
				"			</String>" +
				"			<String name=\"Data\">" +
				"				<Placement before=\"Placement\"/>" +
				"			</String>" +
				"		</Block>" +
				"		<Block name=\"Placement\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var data = Bits.Fmt("{0}", "000011000011Hello WorldhELLO wORLD");

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(5, dom.dataModels[0].Count);
			Assert.AreEqual("Block0", dom.dataModels[0][0].name);
			Assert.AreEqual("Block1", dom.dataModels[0][1].name);
			Assert.AreEqual("Data", dom.dataModels[0][2].name);
			Assert.AreEqual("Data_0", dom.dataModels[0][3].name);
			Assert.AreEqual("Placement", dom.dataModels[0][4].name);

			var block0 = dom.dataModels[0][0] as DataElementContainer;
			var block1 = dom.dataModels[0][1] as DataElementContainer;
			Assert.NotNull(block0);
			Assert.NotNull(block1);
			Assert.AreEqual(2, block0.Count);
			Assert.AreEqual(2, block1.Count);
			Assert.AreEqual("TheCRC", block0[0].name);
			Assert.AreEqual("TheString", block0[1].name);
			Assert.AreEqual("TheCRC", block0[0].name);
			Assert.AreEqual("TheString", block0[1].name);

			var fixup0 = block0[0].fixup;
			var fixup1 = block1[0].fixup;
			Assert.NotNull(fixup0);
			Assert.NotNull(fixup1);

			Assert.AreEqual(2, fixup0.references.Count());
			var fixup0_first = fixup0.references.First();
			var fixup0_last = fixup0.references.Last();
			Assert.AreEqual("ref1", fixup0_first.Item1);
			Assert.AreEqual("TheString", fixup0_first.Item2);
			Assert.AreEqual("ref2", fixup0_last.Item1);
			Assert.AreEqual("TheDataModel.Data", fixup0_last.Item2);

			Assert.AreEqual(1, fixup1.references.Count());
			var fixup1_first = fixup1.references.First();
			Assert.AreEqual("ref", fixup1_first.Item1);
			Assert.AreEqual("TheDataModel.Data_0", fixup1_first.Item2);
		}

		[Test]
		public void ArrayAfterPlacement()
		{
			string xml = @"
<Peach>
	<DataModel name='DM'>
		<Number name='NumPackets' size='8' >
			<Relation type='count' of='Packets'/>
		</Number>
		<Block name='Wrapper'>
			<Block name='Packets' maxOccurs='1024'>
				<Number name='PacketLength' size='8'>
					<Relation type='size' of='Packet'/>
				</Number>
				<String name='Packet'>
					<Placement after='Wrapper'/>
				</String>
			</Block>
		</Block>
	</DataModel>
</Peach>
";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			// When using placement with after, the order gets reversed.  This is because
			// each placement puts the element directly after the target.
			var expected = Encoding.ASCII.GetBytes("\x02\x05\x07!fuzzerpeach");

			var data = Bits.Fmt("{0}", expected);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			var final = dom.dataModels[0].Value.ToArray();
			Assert.AreEqual(expected, final);

			Assert.AreEqual(4, dom.dataModels[0].Count);
			Assert.AreEqual("!fuzzer", (string)dom.dataModels[0][2].DefaultValue);
			Assert.AreEqual("peach", (string)dom.dataModels[0][3].DefaultValue);
		}

		[Test]
		public void ArrayBeforePlacement()
		{
			string xml = @"
<Peach>
	<DataModel name='DM'>
		<Number name='NumPackets' size='8' >
			<Relation type='count' of='Packets'/>
		</Number>
		<Block name='Packets' maxOccurs='1024'>
			<Number name='PacketLength' size='8'>
				<Relation type='size' of='Packet'/>
			</Number>
			<String name='Packet'>
				<Placement before='Marker'/>
			</String>
		</Block>
		<Block name='Marker'/>
	</DataModel>
</Peach>
";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			// When using placement with before, the order is maintained.  This is because
			// each placement puts the element directly before the target.
			var expected = Encoding.ASCII.GetBytes("\x02\x05\x07peach!fuzzer");

			var data = Bits.Fmt("{0}", expected);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			var final = dom.dataModels[0].Value.ToArray();
			Assert.AreEqual(expected, final);

			Assert.AreEqual(5, dom.dataModels[0].Count);
			Assert.AreEqual("peach", (string)dom.dataModels[0][2].DefaultValue);
			Assert.AreEqual("!fuzzer", (string)dom.dataModels[0][3].DefaultValue);
		}

		[Test]
		public void SizedArrayPlacement()
		{
			string xml = @"
<Peach>
	<Defaults>
		<Number endian='big' signed='false'/>
	</Defaults>
			
	<DataModel name='DM'>
		<Number name='NumEntries' size='16'>
			<Relation type='count' of='Entries'/>
		</Number>
		
		<Block name='Entries' minOccurs='1'>
			<Number name='Offset' size='16'>
				<Relation type='offset' of='Data'/>
			</Number>
			<Number name='Size' size='16'>
				<Relation type='size' of='Data'/>
			</Number>
			<String name='Data'>
				<Placement before='Marker'/>
			</String>
		</Block>

		<Block name='Marker'/>
	</DataModel>
</Peach>
";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var data = Bits.Fmt("{0:B16}{1:B16}{2:B16}{3:B16}{4:B16}{5}",
				2, 14, 5, 27, 7, "junkpeachmorejunk!fuzzerevenmorejunk");

			var expected = data.ToArray();
			Assert.NotNull(expected);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(5, dom.dataModels[0].Count);
			Assert.AreEqual("peach", (string)dom.dataModels[0][2].DefaultValue);
			Assert.AreEqual("!fuzzer", (string)dom.dataModels[0][3].DefaultValue);
		}


		[Test, Ignore("See Issue #417")]
		public void BeforeAndAfterPlacement()
		{
			string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
				<Peach>
					<DataModel name=""TheDataModel"">
						<Number size=""8"" name=""Offset1"">
							<Relation type=""offset"" of=""Block1"" />
						</Number>

						<Block name=""Block1"">
							<Placement before=""PlaceHolder""/>	
							
							<Number size=""8"" name=""Offset2"">
								<Relation type=""offset"" of=""Block2"" />
							</Number>

							<Block name=""Block2"">
								<Placement after=""PlaceHolder""/>
								<Blob name=""DataPlaced"" length=""1"" />
							</Block>							
						</Block>				
						
						<Blob name=""Data"" />

						<Block name=""PlaceHolder""/>

					</DataModel>
				</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var data = Bits.Fmt("{0}", new byte[] { 0x03, 0x41, 0x41, 0x04, 0x42 });

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("Offset1", dom.dataModels[0][0].name);
			Assert.AreEqual(3, (int)dom.dataModels[0][0].DefaultValue);

			var Blob1 = (Dom.Blob)dom.dataModels[0][1];
			Assert.AreEqual("Data", Blob1.name);
			Assert.AreEqual(new byte[] { 0x41, 0x41 }, Blob1.DefaultValue.BitsToArray());

			// Possible two bugs here:
			// 1) This Block1 should be after the placeholder
			// 2) The Value 0x42 is never set when cracked

			var PlaceHolder = (Dom.Block)dom.dataModels[0][2];
			Assert.AreEqual("PlaceHolder", PlaceHolder.name);

			var DataPlaced = (Dom.Blob)((Dom.Block)dom.dataModels[0][3])[0];
			Assert.AreEqual("DataPlaced", DataPlaced.name);
			Assert.AreEqual(new byte[] { 0x42 }, DataPlaced.DefaultValue.BitsToArray());

		}
	}
}

// end
