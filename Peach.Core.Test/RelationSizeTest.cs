
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
				"		<Number name=\"num1\" size=\"8\">" +
				"			<Relation type=\"size\" of=\"Data\" />" +
				"		</Number>" +
				"		<Blob name=\"Data\" value=\"00 01 02 03 04 05 06 07\" valueType=\"hex\" >" +
				"			<Hint name=\"BlobMutator-How\" value=\"ExpandAllRandom\" />" + 
				"		</Blob>" +
				"		<Number name=\"num2\" size=\"8\">" +
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
			config.rangeStop = 10;

			Engine e = new Engine(null);
			e.config = config;
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
			e.config = config;
			e.startFuzzing(dom, config);

			Assert.AreEqual(1, dataModels.Count);
			BitStream val = dom.dataModels[0].Value;
			Assert.NotNull(val);

			byte[] expected = Encoding.ASCII.GetBytes("\x01\x01\x10\x00Hello World!");
			Assert.AreEqual(expected, val.Value);
		}
	}
}

// end
