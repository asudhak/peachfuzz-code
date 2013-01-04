﻿
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
	class RelationSizeTest : DataModelCollector
	{
		[Test]
		public void BasicTest()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Blob name=\"Data\" value=\"12345\"/>" +
				"		<Number name=\"TheNumber\" size=\"8\">" +
				"			<Relation type=\"size\" of=\"Data\" />" +
				"		</Number>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Number num = dom.dataModels[0][1] as Number;

			Variant val = num.InternalValue;
			Assert.AreEqual(5, (int)val);
		}

		[Test]
		public void BasicTestParent()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Blob name=\"Data\" value=\"12345\"/>" +
				"		<Number name=\"TheNumber\" size=\"8\">" +
				"			<Relation type=\"size\" of=\"TheDataModel\" />" +
				"		</Number>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Number num = dom.dataModels[0][1] as Number;

			Variant val = num.InternalValue;
			Assert.AreEqual(6, (int)val);
		}

		[Test]
		public void ExpressionGet()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number size=\"8\">" +
				"			<Relation type=\"size\" of=\"Data\" expressionGet=\"size + 5\" />" +
				"		</Number>" +
				"		<Blob name=\"Data\" />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.WriteInt8((sbyte)("Hello World".Length - 5));
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World"));
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("AAAAAAAAAAA"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("Hello World".Length, (int)dom.dataModels[0][0].InternalValue);
			Assert.AreEqual(ASCIIEncoding.ASCII.GetBytes("Hello World"), (byte[])dom.dataModels[0][1].InternalValue);
		}

		[Test]
		public void ExpressionSet()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Blob name=\"Data\" value=\"12345\"/>" +
				"		<Number name=\"TheNumber\" size=\"8\">" +
				"			<Relation type=\"size\" of=\"TheDataModel\" expressionSet=\"size + 10\" />" +
				"		</Number>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Number num = dom.dataModels[0][1] as Number;

			Variant val = num.InternalValue;
			Assert.AreEqual(6 + 10, (int)val);
		}

		[Test]
		public void RelationInRelation()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Block name=\"TheBlock\">"+
				"			<Blob name=\"Data\" value=\"12345\"/>" +
				"			<Number size=\"8\">" +
				"				<Relation type=\"size\" of=\"Data\" />" +
				"			</Number>" +
				"		</Block>"+
				"		<Number size=\"8\">" +
				"			<Relation type=\"size\" of=\"TheBlock\" />" +
				"		</Number>" +
				"		<Number size=\"8\">" +
				"			<Relation type=\"size\" of=\"TheDataModel\" />" +
				"		</Number>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Number num1 = ((Block)dom.dataModels[0][0])[1] as Number;
			Number num2 = dom.dataModels[0][1] as Number;
			Number num3 = dom.dataModels[0][2] as Number;

			Assert.AreEqual(5, (int)num1.InternalValue);
			Assert.AreEqual(6, (int)num2.InternalValue);
			Assert.AreEqual(8, (int)num3.InternalValue);
		}

		[Test]
		public void MultipleFromRelations()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number name=\"num\" size=\"8\">" +
				"			<Relation type=\"size\" of=\"Data1\" />" +
				"			<Relation type=\"size\" of=\"Data2\" />" +
				"		</Number>" +
				"		<Blob name=\"Data1\" />" +
				"		<Blob name=\"Data2\" />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.WriteInt8(5);
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("HelloWorldMore"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(3, dom.dataModels[0].Count);

			Number num = dom.dataModels[0][0] as Number;
			Variant val = num.InternalValue;
			Assert.AreEqual(5, (int)val);

			Blob blob1 = dom.dataModels[0][1] as Blob;
			Variant val2 = blob1.InternalValue;
			Assert.AreEqual(Encoding.ASCII.GetBytes("Hello"), (byte[])val2);

			Blob blob2 = dom.dataModels[0][2] as Blob;
			Variant val3 = blob2.InternalValue;
			Assert.AreEqual(Encoding.ASCII.GetBytes("World"), (byte[])val3);
		}

		[Test]
		public void MultipleOfRelations()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number name=\"num1\" size=\"16\">" +
				"			<Relation type=\"size\" of=\"Data\" />" +
				"		</Number>" +
				"		<Blob name=\"Data\" value=\"00 01 02 03 04 05 06 07\" valueType=\"hex\" >" +
				"			<Hint name=\"BlobMutator-How\" value=\"ExpandAllRandom\" />" + 
				"		</Blob>" +
				"		<Number name=\"num2\" size=\"16\">" +
				"			<Relation type=\"size\" of=\"Data\" />" +
				"		</Number>" +
				"	</DataModel>" +

				"	<StateModel name=\"TheState\" initialState=\"Initial\">" +
				"		<State name=\"Initial\">" +
				"			<Action type=\"output\">" +
				"				<DataModel ref=\"TheDataModel\"/>" +
				"			</Action>" +
				"		</State>" +
				"	</StateModel>" +

				"	<Test name=\"Default\">" +
				"		<StateModel ref=\"TheState\"/>" +
				"			<Publisher class=\"Null\"/>" +
				"			<Strategy class=\"Random\"/>" +
				"	</Test>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			dom.tests[0].includedMutators = new List<string>();
			dom.tests[0].includedMutators.Add("BlobMutator");

			RunConfiguration config = new RunConfiguration();
			config.range = true;
			config.rangeStart = 0;
			config.rangeStop = 9;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			Assert.AreEqual(10, dataModels.Count);

			for (int i = 0; i < 10; ++i)
			{
				var model = dataModels[i];

				Assert.AreEqual(3, model.Count);

				Number num1 = model[0] as Number;
				Variant val1 = num1.InternalValue;
				Assert.GreaterOrEqual((long)val1, 8);

				Blob blob = model[1] as Blob;
				Variant val2 = blob.InternalValue;
				int len = ((byte[])val2).Length;
				Assert.GreaterOrEqual(len, 8);

				Number num2 = model[2] as Number;
				Variant val3 = num2.InternalValue;
				Assert.GreaterOrEqual((long)val3, 8);

				Assert.AreEqual(len, (long)val1);
				Assert.AreEqual(len, (long)val3);
			}
		}

		[Test]
		public void RelationOfDataModel()
		{
			string xml = @"
<Peach>
	<DataModel name=""TheModel"">
		<Number name=""marker"" value=""1"" size=""8"" token=""true""/>
		<Number name=""id"" value=""1"" size=""8""/> 
		<Number name=""length"" size=""16"">
			<Relation type=""size"" of=""TheModel""/>
		</Number>
		<String name=""value"" value=""Hello World!"" />
	</DataModel>

	<StateModel name=""TheState"" initialState=""Initial"">
		<State name=""Initial"">
			<Action type=""output"">
				<DataModel name=""foo"" ref=""TheModel""/>
			</Action>
		</State>
	</StateModel>

	<Test name=""Default"">
		<StateModel ref=""TheState""/>
		<Publisher class=""Null""/>
		<Strategy class=""Sequential""/>
	</Test>
</Peach>
";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Assert.IsTrue(dom.dataModels[0].Count == 4);

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			Assert.AreEqual(1, dataModels.Count);
			BitStream val = dom.dataModels[0].Value;
			Assert.NotNull(val);

			byte[] expected = Encoding.ASCII.GetBytes("\x01\x01\x10\x00Hello World!");
			Assert.AreEqual(expected, val.Value);
		}

		Dictionary<string, object> SpeedTest(uint repeat)
		{
			// When the data element duplicator clones blockData multiple times,
			// it becomes painfully slow to evaulate the final value of DM3

			string outter_xml = @"
<Peach>

<DataModel name=""DM3"">
	<Number name=""tag"" size=""32"" signed=""false"" endian=""big"">
		<Fixup class=""Crc32Fixup"">
			<Param name=""ref"" value=""blockData""/>
		</Fixup>
	</Number>
{0}
</DataModel>

</Peach>
";

			string inner_xml = @"
	<Block name=""blockData{0}"">
		<Number name=""CommandSize"" size=""32"" signed=""false"" endian=""big"">
			<Relation type=""size"" of=""DM3"" />
		</Number>
		<Number name=""CommandCode"" size=""32"" signed=""false"" endian=""big"">
			<Transformer class=""Md5""/>
		</Number>
	</Block>
";

			StringBuilder sb = new StringBuilder();
			sb.Append(string.Format(inner_xml, ""));
			for (int i = 0; i < repeat; ++i)
			{
				sb.Append(string.Format(inner_xml, "_" + i));
			}

			string xml = string.Format(outter_xml, sb.ToString());

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Assert.AreEqual(1, dom.dataModels.Count);

			int start = Environment.TickCount;

			var value = dom.dataModels[0].Value;

			int end = Environment.TickCount;

			Assert.NotNull(value);

			TimeSpan delta = TimeSpan.FromMilliseconds(end - start);

			Dictionary<string, object> ret = new Dictionary<string, object>();
			ret["delta"] = delta;
			ret["count"] = dom.dataModels[0].GenerateCount;
			return ret;
		}

		[Test]
		public void MeasureSpeed()
		{
			for (uint i = 0; i <= 50; i += 10)
			{
				var ret = SpeedTest(i);
				TimeSpan delta = (TimeSpan)ret["delta"];
				uint count = (uint)ret["count"];
				Assert.LessOrEqual(delta, TimeSpan.FromSeconds(30));
				Assert.AreEqual(count, i + 2);
			}
		}

		[Test]
		public void RelationInArray()
		{
			string xml = @"
<Peach>
	<DataModel name=""ElemModel"">
		<Number name=""length"" size=""16"" endian=""big"">
			<Relation type=""size"" of=""data""/>
		</Number>
		<Blob name=""data""/>
	</DataModel>

	<DataModel name=""DM"">
		<Number name=""tag"" size=""16"" endian=""big"" />
		<Number name=""length"" size=""16"" endian=""big"">
			<Relation type=""size"" of=""Elements""/>
		</Number>
		<Block name=""Elements"">
			<Block name=""Elem"" minOccurs=""0"" maxOccurs=""999"">
				<Block name=""Elem0"" ref=""ElemModel""/>
			</Block>
		</Block>
	</DataModel>
</Peach>
";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Assert.AreEqual(2, dom.dataModels.Count);

			BitStream data = new BitStream();
			data.WriteBytes(new byte[] { 0x00, 0x10, 0x00, 0x06, 0x00, 0x04, 0xde, 0xad, 0xbe, 0xef });
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[1], data);

			Assert.AreEqual(3, dom.dataModels[1].Count);

			var tag = dom.dataModels[1][0] as Number;
			var len = dom.dataModels[1][1] as Number;
			var blk = dom.dataModels[1][2] as Block;

			Assert.NotNull(tag);
			Assert.NotNull(len);
			Assert.NotNull(blk);
			Assert.AreEqual(1, blk.Count);

			var arr = blk[0] as Dom.Array;
			Assert.NotNull(arr);
			Assert.AreEqual(1, arr.Count);

			var elm = arr[0] as Block;
			Assert.NotNull(elm);
			Assert.AreEqual(1, elm.Count);

			var el0 = elm[0] as DataModel;
			Assert.NotNull(el0);
			Assert.AreEqual(2, el0.Count);

			var length = el0[0] as Number;
			var blob = el0[1] as Blob;

			Assert.NotNull(length);
			Assert.NotNull(blob);

			Assert.AreEqual(16, (int)tag.DefaultValue);
			Assert.AreEqual(6, (int)len.DefaultValue);
			Assert.AreEqual(4, (int)length.DefaultValue);

			var bs = (BitStream)blob.DefaultValue;
			Assert.NotNull(bs);

			MemoryStream ms = bs.Stream as MemoryStream;
			Assert.NotNull(ms);

			Assert.AreEqual(4, ms.Length);

			var buf = ms.GetBuffer();
			Assert.AreEqual(0xde, buf[0]);
			Assert.AreEqual(0xad, buf[1]);
			Assert.AreEqual(0xbe, buf[2]);
			Assert.AreEqual(0xef, buf[3]);
		}
	}
}

// end
