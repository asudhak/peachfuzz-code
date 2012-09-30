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
	<DataModel name=""Base"">
		<Number name=""Length"" size=""32"">
			<Relation type=""size"" of=""Data"" />
		</Number>
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

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			Assert.AreEqual(4, dom.dataModels.Count);
			var final = dom.dataModels[3];
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

			DataElement foo = fromRel.parent.Clone("foo");

			IFormatter formatter = new BinaryFormatter();
			Stream stream = new MemoryStream();
			formatter.Serialize(stream, fromRel);
			stream.Seek(0, SeekOrigin.Begin);
			long lenBefore = stream.Length;
			Relation fromCopy = formatter.Deserialize(stream) as Relation;

			fromCopy.parent = null;
			fromCopy.Reset();

			stream = new MemoryStream();
			formatter.Serialize(stream, fromCopy);
			stream.Seek(0, SeekOrigin.Begin);
			long lenAfter = stream.Length;

			Assert.LessOrEqual(lenBefore, 26250);
			Assert.LessOrEqual(lenAfter, 750);
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

			Block copy = root.Clone("copy") as Block;
			Assert.NotNull(copy);
		}
	}
}
