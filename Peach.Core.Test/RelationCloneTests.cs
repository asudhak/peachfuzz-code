using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Analyzers;
using Peach.Core.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Peach.Core.Test
{
	[TestFixture]
	class RelationCloneTests
	{
		[Test]
		public void TestSerializeSize()
		{
			string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Peach>
	<DataModel name=""Common"">
		<Number name=""Length"" size=""32"">
			<Relation type=""size"" of=""Data"" />
		</Number>
	</DataModel>

	<DataModel name=""Base"" ref=""Common"">
		<Block name=""Payload"">
			<Block name=""Data""/>
		</Block>
	</DataModel>

	<DataModel name=""Msg_A"" ref=""Base"">
		<Block name=""Payload"">
			<Block name=""Data"">
				<Relation type=""size"" from=""Length"" />
				<Number name=""num"" size=""8"" />
			</Block>
		</Block>
	</DataModel>

	<DataModel name=""Msg_B"" ref=""Base"">
		<Block name=""Payload"">
			<Block name=""Data"">
				<Relation type=""size"" from=""Length"" />
				<String name=""str"" value=""Hello World"" />
			</Block>
		</Block>
	</DataModel>

	<DataModel name=""Final"">
		<Block name=""blk1"" ref=""Msg_A"" />
		<Block name=""blk2"" ref=""Msg_B"" />
	</DataModel>
</Peach>";

			// Final.blk1.Length           - 1 Relation Of="Final.blk1.Payload.Data"
			// Final.blk1.Payload.Data     - 1 Relation From="Final.blk1.Length"
			// Final.blk1.Payload.Data.num
			// Final.blk2.Length           - 1 Relation Of="Final.blk2.Payload.Data"
			// Final.blk2.Payload.Data     - 1 Relation From="Final.blk2.Length"
			// Final.blk2.Payload.Data.str

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			Assert.AreEqual(5, dom.dataModels.Count);
			var final = dom.dataModels[4];

			Assert.AreEqual("Final", final.name);

			var fromElem = final.find("Final.blk1.Length");
			var ofElem = final.find("Final.blk1.Payload.Data");

			Assert.NotNull(fromElem);
			Assert.AreEqual(1, fromElem.relations.Count);
			Assert.NotNull(ofElem);
			Assert.AreEqual(1, ofElem.relations.Count);

			var fromRel = fromElem.relations[0];
			var ofRel = ofElem.relations[0];

			Assert.NotNull(fromRel.parent);
			Assert.NotNull(fromRel.From);
			Assert.NotNull(fromRel.Of);

			Assert.NotNull(ofRel.parent);
			Assert.NotNull(ofRel.From);
			Assert.NotNull(ofRel.Of);

			Assert.AreEqual(fromRel, ofRel);
			Assert.AreEqual(fromRel.parent, fromElem);
			Assert.AreEqual(fromRel.From, fromElem);
			Assert.AreEqual(fromRel.Of, ofElem);

			long size = 0;
			Assert.AreEqual("Final.blk1.Length", fromRel.parent.fullName);
			DataElement foo = fromRel.parent.Clone("Length_1", ref size);
			fromRel.parent.parent.Insert(fromRel.parent.parent.IndexOf(fromRel.parent), foo);

			// Cloning Final.blk1.Length into Length_1 should yeild:
			// Final.blk1.Length           - 1 Relation Of="Final.blk1.Payload.Data"
			// Final.blk1.Length_1         - 1 Relation Of="Final.blk1.Payload.Data"
			// Final.blk1.Payload.Data     - 2 Relation From="Final.blk1.Length" From="Final.blk1.Length_1"
			// Final.blk1.Payload.Data.num
			// Final.blk2.Length           - 1 Relation Of="Final.blk2.Payload.Data"
			// Final.blk2.Payload.Data     - 1 Relation From="Final.blk2.Length"
			// Final.blk2.Payload.Data.str

			Assert.AreEqual(1, foo.relations.Count);
			Assert.AreEqual("Length_1", foo.relations[0].FromName);
			Assert.AreEqual(foo, foo.relations[0].From);
			Assert.AreEqual(foo, foo.relations[0].parent);
			Assert.AreEqual("Data", foo.relations[0].OfName);
			Assert.AreEqual(fromRel.Of, foo.relations[0].Of);
			Assert.AreEqual(2, fromRel.Of.relations.Count);
			Assert.AreEqual(foo.relations[0], fromRel.Of.relations[1]);

			// Cloning Final.blk1.Payload.Data into Data_1 should yeild:
			// Final.blk1.Length             - 2 Relation Of="Final.blk1.Payload.Data" Of="Final.blk1.Payload.Data_1"
			// Final.blk1.Length_1           - 2 Relation Of="Final.blk1.Payload.Data" Of="Final.blk1.Payload.Data_1"
			// Final.blk1.Payload.Data       - 2 Relation From="Final.blk1.Length" From="Final.blk1.Length_1"
			// Final.blk1.Payload.Data.num
			// Final.blk1.Payload.Data_1     - 2 Relation From="Final.blk1.Length" From="Final.blk1.Length_1"
			// Final.blk1.Payload.Data_1.num
			// Final.blk2.Length             - 1 Relation Of="Final.blk2.Payload.Data"
			// Final.blk2.Payload.Data       - 1 Relation From="Final.blk2.Length"
			// Final.blk2.Payload.Data.str

			DataElement bar = fromRel.Of.Clone("Data_1");
			fromRel.Of.parent.Insert(fromRel.Of.parent.IndexOf(fromRel.Of), bar);

			Assert.AreEqual(2, bar.relations.Count);
			Assert.AreEqual(2, foo.relations.Count);
			Assert.AreEqual(2, fromRel.parent.relations.Count);

			Assert.AreEqual("Length", bar.relations[0].FromName);
			Assert.AreEqual("Length_1", bar.relations[1].FromName);
			Assert.AreEqual(bar, fromRel.parent.relations[1].Of);
			Assert.AreEqual(bar, foo.relations[1].Of);

			// Test size against regular binary serializer
			IFormatter formatter = new BinaryFormatter();
			Stream stream = new MemoryStream();
			formatter.Serialize(stream, fromRel.parent);
			long lenSimple = stream.Length;

			Assert.GreaterOrEqual(lenSimple, size);
			Assert.LessOrEqual(size, 5200);
		}

		[Test]
		public void TestSerialize()
		{
			Block root = new Block("root");
			Number fromElem = new Number("from");
			Blob ofElem = new Blob("of");
			Relation r = new SizeRelation();
			root.Add(fromElem);
			root.Add(ofElem);
			fromElem.relations.Add(r);
			ofElem.relations.Add(r);
			r.parent = fromElem;
			r.Of = ofElem;
			r.From = fromElem;

			Block copy = root.Clone() as Block;
			Assert.NotNull(copy);
		}

		[Test]
		public void TestArrayRelationClone()
		{
			// If an array is cloned wih a new name, and the array element contains a relation, 
			// the relation's Of or From names need to be updated

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
			Assert.AreEqual("Length", array.name);
			Assert.AreEqual(100, array.Count);
			Assert.AreEqual("Length", array[0].name);
			Assert.AreEqual(0, array.relations.Count);
			Assert.AreEqual(1, array[0].relations.Count);
			Assert.AreEqual(100, array[0].relations[0].Of.relations.Count);

			Dom.Array clone = array.Clone("NewLength") as Dom.Array;
			Assert.NotNull(clone);
			Assert.AreEqual("NewLength", clone.name);
			Assert.AreEqual(100, clone.Count);
			Assert.AreEqual("NewLength", clone[0].name);
			Assert.AreEqual(0, clone.relations.Count);
			Assert.AreEqual(1, clone[0].relations.Count);
			Assert.AreEqual("Data", clone[0].relations[0].OfName);
			Assert.AreEqual("NewLength", clone[0].relations[0].FromName);
			Assert.AreEqual(200, clone[0].relations[0].Of.relations.Count);
			Assert.True(clone[0].relations[0].Of.relations.Contains(clone[0].relations[0]));
		}

		[Test]
		public void TestArrayRef()
		{
			string xml = @"
<Peach>
	<DataModel name=""A"">
		<Block name=""B"">
			<Number name=""Length"" size=""32"" endian=""little"">
				<Relation type=""size"" of=""A""/>
			</Number>
		</Block>
	</DataModel>

	<DataModel name=""C"">
		<Block name=""D"">
			<Block occurs=""1"" name=""E"" ref=""A""/>
		</Block>
	</DataModel>
</Peach>";


			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var final = dom.dataModels[1].Value;

			byte[] expected = new byte[] { 4, 0, 0, 0 };

			Assert.AreEqual(expected, final.Value);
		}
	}
}
