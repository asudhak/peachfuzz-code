
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
	public class ChoiceTests
	{
		[Test]
		public void CrackChoice1()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Choice>"  +
				"			<Blob name=\"Blob10\" length=\"10\" />" +
				"			<Blob name=\"Blob5\" length=\"5\" />"   +
				"		</Choice>" +
				"	</DataModel>"  +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(new byte[] { 1, 2, 3, 4, 5 });
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.IsTrue(dom.dataModels[0][0] is Choice);
			Assert.AreEqual("Blob5", ((Choice)dom.dataModels[0][0])[0].name);
			Assert.AreEqual(new byte[] { 1, 2, 3, 4, 5 }, (byte[])((DataElementContainer)dom.dataModels[0][0])[0].DefaultValue);
		}

		[Test]
		public void MinOccurs0()
		{
			string xml = @"
<Peach>
	<DataModel name=""DM_choice1"">
		<Blob name=""smallData"" length=""2""/>
		<Number size=""32"" token=""true"" value=""1""/>
	</DataModel>

	<DataModel name=""DM_choice2"">
		<Blob name=""BigData"" length=""10""/>
		<Number size=""32"" token=""true"" value=""2""/>
	</DataModel>

	<DataModel name=""DM"">
		<Blob name=""Header"" length=""5""/>
		<Choice name=""options"" minOccurs=""0"">
			<Block ref=""DM_choice1""/>
			<Block ref=""DM_choice2""/>
		</Choice>
	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(new byte[] { 11, 22, 33, 44, 55 });
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[2], data);

			Assert.AreEqual(2, dom.dataModels[2].Count);
			Assert.IsTrue(dom.dataModels[2][0] is Blob);
			Assert.IsTrue(dom.dataModels[2][1] is Dom.Array);
			Assert.AreEqual(0, ((Dom.Array)dom.dataModels[2][1]).Count);
		}

		[Test]
		public void ArrayOfChoice()
		{
			string xml = @"
<Peach>
	<DataModel name=""DM_choice1"">
		<Block>
		<Number size=""8"" token=""true"" value=""1""/>
		<Blob name=""smallData"" length=""2""/>
		</Block>
	</DataModel>

	<DataModel name=""DM_choice2"">
		<Block>
		<Number size=""8"" token=""true"" value=""2""/>
		<Blob name=""BigData"" length=""4""/>
		</Block>
	</DataModel>

	<DataModel name=""DM"">
		<Blob name=""Header"" length=""5""/>
		<Choice name=""options"" minOccurs=""0"">
			<Block ref=""DM_choice1""/>
			<Block ref=""DM_choice2""/>
		</Choice>
		<Blob name=""extra"" length=""1""/>
	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(new byte[] { 11, 22, 33, 44, 55, 1, 9, 9, 2, 8, 8, 8, 8, 1, 7, 7, 0 });
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[2], data);

			Assert.AreEqual(3, dom.dataModels[2].Count);
			Assert.IsTrue(dom.dataModels[2][0] is Blob);
			Assert.IsTrue(dom.dataModels[2][1] is Dom.Array);
			var array = dom.dataModels[2][1] as Dom.Array;
			Assert.NotNull(array);
			Assert.AreEqual(3, array.Count);
		}

		[Test]
		public void PickChoice()
		{
			string temp = Path.GetTempFileName();

			string xml = @"
<Peach>
	<DataModel name='DM1'>
		<String name='str' value='token1' token='true'/>
	</DataModel>

	<DataModel name='DM2'>
		<String name='str' value='token2' token='true'/>
	</DataModel>

	<DataModel name='DM'>
		<Choice name='choice'>
			<Block name='token1' ref='DM1'/>
			<Block name='token2' ref='DM2'/>
		</Choice>
	</DataModel>

	<StateModel name='SM_In' initialState='Initial'>
		<State name='Initial'>
			<Action type='input'>
				<DataModel ref='DM' />
				<Data DataModel='DM'>
					<Field name='choice.token2' value='' />
				</Data>
			</Action>
		</State>
	</StateModel>

	<StateModel name='SM_Out' initialState='Initial'>
		<State name='Initial'>
			<Action type='output'>
				<DataModel ref='DM' />
				<Data DataModel='DM'>
					<Field name='choice.token2' value='' />
				</Data>
			</Action>
		</State>
	</StateModel>

	<Test name='Input'>
		<StateModel ref='SM_In' />
		<Publisher class='File'>
			<Param name='FileName' value='{0}'/>
			<Param name='Overwrite' value='false'/>
		</Publisher>
	</Test>

	<Test name='Output'>
		<StateModel ref='SM_Out' />
		<Publisher class='File'>
			<Param name='FileName' value='{0}'/>
			<Param name='Overwrite' value='true'/>
		</Publisher>
	</Test>

</Peach>".Fmt(temp);

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;
			config.runName = "Output";

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			var file = File.ReadAllText(temp);
			Assert.AreEqual("token2", file);

			config.runName = "Input";

			dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			e = new Engine(null);
			e.startFuzzing(dom, config);
		}


		[Test]
		public void ChoiceSizeRelations()
		{
			string xml = @"
<Peach>
	<DataModel name=""DM"">
		<Choice>
			<Block name=""C1"">
				<Number name=""version"" size=""8"" value=""1"" token=""true""/>
				<Number name=""LengthBig"" size=""16"">
					<Relation type=""size"" of=""data""/>
				</Number>
			</Block>
			<Block name=""C2"">
				<Number name=""version"" size=""8"" value=""2"" token=""true""/>
				<Number name=""LengthSmall"" size=""8"">
					<Relation type=""size"" of=""data""/>
				</Number>
			</Block>
		</Choice>
		<Blob name=""data""/>
		<Blob/>
	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(new byte[] { 0x02, 0x03, 0x33, 0x44, 0x55 });
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(3, dom.dataModels[0].Count);
			Assert.IsTrue(dom.dataModels[0][0] is Dom.Choice);
			Assert.IsTrue(dom.dataModels[0][1] is Blob);
			Assert.IsTrue(dom.dataModels[0][2] is Blob);
			Assert.AreEqual(3, dom.dataModels[0][1].Value.LengthBytes);
			Assert.AreEqual(0, dom.dataModels[0][2].Value.LengthBytes);

		}

		[Test]
		public void ChoiceSizeRelationsParent()
		{
			string xml = @"
<Peach>
	<DataModel name=""DM"">
		<Block name=""blk"">
			<Choice>
				<Block name=""C1"">
					<Number name=""version"" size=""8"" value=""1"" token=""true""/>
					<Number name=""LengthBig"" size=""16"">
						<Relation type=""size"" of=""blk""/>
					</Number>
				</Block>
				<Block name=""C2"">
					<Number name=""version"" size=""8"" value=""2"" token=""true""/>
					<Number name=""LengthSmall"" size=""8"">
						<Relation type=""size"" of=""blk""/>
					</Number>
				</Block>
			</Choice>
			<Blob name=""blb""/>
		</Block>
		<Blob/>
	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(new byte[] { 0x01, 0x06, 0x00, 0x33, 0x44, 0x55 });
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(2, dom.dataModels[0].Count);
			Assert.IsTrue(dom.dataModels[0][0] is Dom.Block);
			Assert.IsTrue(dom.dataModels[0][1] is Blob);
			Assert.AreEqual(3, dom.dataModels[0].find("blk.blb").Value.LengthBytes);
			Assert.AreEqual(0, dom.dataModels[0][1].Value.LengthBytes);
		}

		[Test]
		public void ChoiceSizeRelationsParentTwice()
		{
			string xml = @"
<Peach>
	<DataModel name=""DM"">
		<Block name=""blk"">
			<Choice>
				<Block name=""C1"">
					<Number name=""LengthBig"" size=""16"">
						<Relation type=""size"" of=""blk"" expressionGet=""size + 3"" expressionSet=""size - 3""/>
					</Number>
					<Number name=""version"" size=""8"" value=""0"" token=""true""/>
				</Block>
				<Block name=""C2"">
					<Number name=""LengthSmall"" size=""8"">
						<Relation type=""size"" of=""blk"" expressionGet=""size + 2"" expressionSet=""size - 2""/>
					</Number>
					<Number name=""version"" size=""8"" value=""0"" token=""true""/>
				</Block>
			</Choice>
			<Blob name=""blb""/>
		</Block>
		<Blob/>
	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(new byte[] { 0x03, 0x00, 0x33, 0x44, 0x55 });
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(2, dom.dataModels[0].Count);
			Assert.IsTrue(dom.dataModels[0][0] is Dom.Block);
			Assert.IsTrue(dom.dataModels[0][1] is Blob);
			Assert.AreEqual(3, dom.dataModels[0].find("blk.blb").Value.LengthBytes);
			Assert.AreEqual(0, dom.dataModels[0][1].Value.LengthBytes);
		}

		[Test]
		public void ChoiceCountRelations()
		{
			string xml = @"
<Peach>
	<DataModel name=""DM"">
		<Choice>
			<Block name=""C1"">
				<Number name=""version"" size=""8"" value=""1"" token=""true""/>
				<Number name=""LengthBig"" size=""16"">
					<Relation type=""count"" of=""data""/>
				</Number>
			</Block>
			<Block name=""C2"">
				<Number name=""version"" size=""8"" value=""2"" token=""true""/>
				<Number name=""LengthSmall"" size=""8"">
					<Relation type=""count"" of=""data""/>
				</Number>
			</Block>
		</Choice>
		<Number size=""8"" minOccurs=""0"" name=""data""/>
		<Blob />
	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(new byte[] { 0x02, 0x03, 0x33, 0x44, 0x55 });
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(3, dom.dataModels[0].Count);
			Assert.IsTrue(dom.dataModels[0][0] is Dom.Choice);
			Assert.IsTrue(dom.dataModels[0][1] is Dom.Array);
			Assert.IsTrue(dom.dataModels[0][2] is Dom.Blob);

			Dom.Array array = dom.dataModels[0][1] as Dom.Array;
			Assert.AreEqual(3, array.Count);

			Assert.AreEqual(0, dom.dataModels[0][2].Value.LengthBytes);
		}

		[Test]
		public void ChoiceOffsetRelations()
		{
			string xml = @"
<Peach>
	<DataModel name=""DM"">
		<Choice>
			<Block name=""C1"">
				<Number name=""version"" size=""8"" value=""1"" token=""true""/>
				<Number name=""LengthBig"" size=""16"">
					<Relation type=""offset"" of=""data""/>
				</Number>
			</Block>
			<Block name=""C2"">
				<Number name=""version"" size=""8"" value=""2"" token=""true""/>
				<Number name=""LengthSmall"" size=""8"">
					<Relation type=""offset"" of=""data""/>
				</Number>
			</Block>
		</Choice>
		<Blob name=""data""/>
	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(new byte[] { 0x02, 0x03, 0x33, 0x44, 0x55 });
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(2, dom.dataModels[0].Count);
			Assert.IsTrue(dom.dataModels[0][0] is Dom.Choice);
			Assert.IsTrue(dom.dataModels[0][1] is Dom.Blob);

			var expected = new byte[] { 0x44, 0x55 };
			var actual = dom.dataModels[0][1].Value.Value;

			Assert.AreEqual(expected, actual);
		}


		[Test]
		public void ChoiceSizeRelations2()
		{
			string xml = @"
<Peach>
	<DataModel name=""DM"">
		<Block name=""TheBlock"">
			<Choice>
				<Block name=""C1"">
					<Number name=""version"" size=""8"" value=""1"" token=""true""/>
					<Number name=""LengthBig"" size=""16"">
						<Relation type=""size"" of=""TheBlock""/>
					</Number>
				</Block>
				<Block name=""C2"">
					<Number name=""version"" size=""8"" value=""2"" token=""true""/>
					<Number name=""LengthSmall"" size=""8"">
						<Relation type=""size"" of=""TheBlock""/>
					</Number>
				</Block>
			</Choice>
			<Blob name=""data""/>
		</Block>
		<Blob/>
	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(new byte[] { 0x02, 0x05, 0x33, 0x44, 0x55 });
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);


			Dom.Block TheBlock = (Dom.Block)dom.dataModels[0][0];
			Assert.AreEqual(2, TheBlock.Count);
			Assert.IsTrue(TheBlock[0] is Dom.Choice);
			Assert.IsTrue(TheBlock[1] is Blob);
			Assert.IsTrue(dom.dataModels[0][1] is Blob);
			Assert.AreEqual(3, TheBlock[1].Value.LengthBytes);
			Assert.AreEqual(0, ((Dom.Blob)dom.dataModels[0][1]).Value.LengthBytes);

		}

		[Test]
		public void ChoiceArrayField()
		{
			string xml = @"
<Peach>
	<DataModel name='DM'>
		<Block name='Root' minOccurs='0'>
			<Choice name='Choice'>
				<Block name='C1'>
					<String name='str1' value='Choice 1='/>
					<String name='str2'/>
				</Block>
				<Block name='C2'>
					<String name='str1' value='Choice 2='/>
					<String name='str2'/>
				</Block>
			</Choice>
			<Blob name='data'/>
		</Block>
		<Blob/>
	</DataModel>

	<StateModel name='SM' initialState='Initial'>
		<State name='Initial'>
			<Action type='output'>
				<DataModel ref='DM' />
				<Data DataModel='DM'>
					<Field name='Root[0].Choice.C1.str2' value='foo,' />
					<Field name='Root[1].Choice.C2.str2' value='bar' />
				</Data>
			</Action>
		</State>
	</StateModel>

	<Test name='Default'>
		<StateModel ref='SM' />
		<Publisher class='Null'/>
	</Test>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			var dm = dom.tests[0].stateModel.states["Initial"].actions[0].dataModel;

			var bytes = dm.Value.Value;
			string str = Encoding.ASCII.GetString(bytes);
			Assert.AreEqual("Choice 1=foo,Choice 2=bar", str);
		}

		[Test]
		public void ChoiceUnsizedLookahead()
		{
			string xml = @"
<Peach>
	<DataModel name=""DM"">
		<Choice name=""c"">
			<Block name=""C1"">
				<Number size=""8"" value=""0xff"" token=""true""/>
				<Blob/>
			</Block>
			<Block name=""C2"">
				<Blob length=""1""/>
				<Blob/>
			</Block>
		</Choice>
	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			// Generate Value prior to cracking, to simulate state model running then an input action
			var model = dom.dataModels[0].Value;
			Assert.NotNull(model);

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(new byte[] { 0x02, 0x05, 0x33, 0x44, 0x55 });
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);


			Dom.Choice c = (Dom.Choice)dom.dataModels[0][0];
			var selected = c.SelectedElement as Dom.Block;
			Assert.AreEqual("C2", selected.name);
			Assert.AreEqual(1, ((byte[])selected[0].DefaultValue).Length);
			Assert.AreEqual(4, ((byte[])selected[1].DefaultValue).Length);
		}

		[Test]
		public void ChoiceUnsizedLookahead2()
		{
			string xml = @"
<Peach>
	<DataModel name=""DM"">
		<Choice name=""c"">
			<Block name=""C1"">
				<Blob name=""blb"" length=""1"" valueType=""hex"" value=""0x08"" token=""true""/>
				<Block name=""array"" minOccurs=""0"">
					<Blob name=""inner"" length=""1"" valueType=""hex"" value=""0x05""/>
					<Blob name=""unsized""/>
				</Block>
			</Block>
		</Choice>
	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			// Generate Value prior to cracking, to simulate state model running then an input action
			var model = dom.dataModels[0].Value;
			Assert.NotNull(model);

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(new byte[] { 0x08, 0x05, 0x33, 0x44, 0x55 });
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);


			Dom.Choice c = dom.dataModels[0][0] as Dom.Choice;
			Assert.NotNull(c);
			var selected = c.SelectedElement as Dom.Block;
			Assert.NotNull(selected);
			Assert.AreEqual("C1", selected.name);
			Assert.AreEqual(2, selected.Count);
			var array = selected[1] as Dom.Array;
			Assert.NotNull(array);
			Assert.AreEqual(1, array.Count);
			var innerBlock = array[0] as Dom.Block;
			Assert.NotNull(innerBlock);
			Assert.AreEqual(2, innerBlock.Count);
			Assert.AreEqual(1, ((byte[])selected[0].DefaultValue).Length);
			Assert.AreEqual(1, ((byte[])innerBlock[0].DefaultValue).Length);
			Assert.AreEqual(3, ((byte[])innerBlock[1].DefaultValue).Length);
		}
	}
}

// end
