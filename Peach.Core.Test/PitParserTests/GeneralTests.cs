
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

namespace Peach.Core.Test.PitParserTests
{
	public static class Extensions
	{
		public static bool mutable(this DataModel dm, string element)
		{
			var de = dm.find(element);
			Assert.NotNull(de);
			return de.isMutable;
		}
	}

	[TestFixture]
	class GeneralTests
	{
		//[Test]
		//public void NumberDefaults()
		//{
		//    string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
		//        "	<Defaults>" +
		//        "		<Number size=\"8\" endian=\"big\" signed=\"true\"/>" +
		//        "	</Defaults>" +
		//        "	<DataModel name=\"TheDataModel\">" +
		//        "		<Number name=\"TheNumber\" size=\"8\"/>" +
		//        "	</DataModel>" +
		//        "</Peach>";

		//    PitParser parser = new PitParser();
		//    Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
		//    Number num = dom.dataModels[0][0] as Number;

		//    Assert.IsTrue(num.Signed);
		//    Assert.IsFalse(num.LittleEndian);
		//}

		[Test]
		public void DeepOverride()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel1\">" +
				"       <Block name=\"TheBlock\">" +
				"		      <String name=\"TheString\" value=\"Hello\"/>" +
				"       </Block>" +
				"	</DataModel>" +
				"	<DataModel name=\"TheDataModel\" ref=\"TheDataModel1\">" +
				"      <String name=\"TheBlock.TheString\" value=\"World\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			Assert.AreEqual(1, dom.dataModels["TheDataModel"].Count);
			Assert.AreEqual(1, ((DataElementContainer)dom.dataModels["TheDataModel"][0]).Count);

			Assert.AreEqual("TheString", ((DataElementContainer)dom.dataModels["TheDataModel"][0])[0].name);
			Assert.AreEqual("World", (string)((DataElementContainer)dom.dataModels["TheDataModel"][0])[0].DefaultValue);
		}

		[Test]
		public void PeriodInName()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel1\">" +
				"       <Block name=\"TheBlock\">" +
				"		      <String name=\"The.String\" value=\"Hello\"/>" +
				"       </Block>" +
				"	</DataModel>" +
				"</Peach>";

			Assert.Throws<PeachException>(delegate()
			{
				new Dom.String("Foo.Bar");
			});

			PitParser parser = new PitParser();

			Assert.Throws<PeachException>(delegate()
			{
				parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			});
		}

		[Test]
		public void IncludeMutators()
		{
			string xml =
@"<Peach>
   <DataModel name='TheDataModel'>
       <String name='str' value='Hello World!'/>
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
       <Mutators mode='include'>
           <Mutator class='StringCaseMutator'/>
           <Mutator class='BlobMutator'/>
       </Mutators>
   </Test>
</Peach>";
			PitParser parser = new PitParser();

			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			Assert.AreEqual(0, dom.tests[0].excludedMutators.Count);
			Assert.AreEqual(2, dom.tests[0].includedMutators.Count);
			Assert.AreEqual("StringCaseMutator", dom.tests[0].includedMutators[0]);
			Assert.AreEqual("BlobMutator", dom.tests[0].includedMutators[1]);
		}

		[Test]
		public void ExcludeMutators()
		{
			string xml =
@"<Peach>
   <DataModel name='TheDataModel'>
       <String name='str' value='Hello World!'/>
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
       <Mutators mode='exclude'>
           <Mutator class='StringCaseMutator'/>
           <Mutator class='BlobMutator'/>
       </Mutators>
   </Test>
</Peach>";
			PitParser parser = new PitParser();

			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			Assert.AreEqual(0, dom.tests[0].includedMutators.Count);
			Assert.AreEqual(2, dom.tests[0].excludedMutators.Count);
			Assert.AreEqual("StringCaseMutator", dom.tests[0].excludedMutators[0]);
			Assert.AreEqual("BlobMutator", dom.tests[0].excludedMutators[1]);
		}

		[Test]
		public void IncludeExcludeMutable()
		{
			string xml =
@"<Peach>
	<DataModel name='TheDataModel'>
		<String name='str' value='Hello World!'/>
		<String name='str2' value='Hello World!'/>
		<Block name='block'>
			<Block name='subblock'>
				<Blob name='blob'/>
				<Number name='subnum' size='8'/>
			</Block>
			<Number name='num' size='8'/>
		</Block>
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
		<Exclude ref='str'/>
		<Exclude ref='block'/>
		<Include ref='subblock'/>
	</Test>
</Peach>";
			PitParser parser = new PitParser();

			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			DataElement de;

			// Shouldn't update the top level data model
			de = dom.dataModels[0].find("TheDataModel.str");
			Assert.NotNull(de);
			Assert.True(de.isMutable);

			var dm = dom.tests[0].stateModel.states["Initial"].actions[0].dataModel;
			Assert.NotNull(dm);

			// Should update the action's data model

			Assert.False(dm.mutable("TheDataModel.str"));
			Assert.True( dm.mutable("TheDataModel.str2"));
			Assert.False(dm.mutable("TheDataModel.block"));
			Assert.True( dm.mutable("TheDataModel.block.subblock"));
			Assert.True( dm.mutable("TheDataModel.block.subblock.blob"));
			Assert.True( dm.mutable("TheDataModel.block.subblock.subnum"));
			Assert.False(dm.mutable("TheDataModel.block.num"));
		}

		[Test]
		public void TopDataElement()
		{
			string temp1 = Path.GetTempFileName();
			File.WriteAllBytes(temp1, Encoding.ASCII.GetBytes("Hello World"));

			string xml = 
@"<Peach>
	<Data name='data' fileName='{0}'/>

	<DataModel name='TheDataModel'>
		<String name='str' value='Hello World!'/>
	</DataModel>

	<StateModel name='TheState' initialState='Initial'>
		<State name='Initial'>
			<Action type='output'>
				<DataModel ref='TheDataModel'/>
				<Data ref='data'/>
			</Action>
		</State>
	</StateModel>

	<Test name='Default'>
		<StateModel ref='TheState'/>
		<Publisher class='Null'/>
	</Test>
</Peach>";

			xml = string.Format(xml, temp1);

			PitParser parser = new PitParser();

			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			var ds = dom.stateModels[0].states["Initial"].actions[0].dataSet;
			Assert.NotNull(ds);
			Assert.AreEqual(1, ds.Datas.Count);
			Assert.AreEqual(temp1, ds.Datas[0].FileName);
		}

		[Test]
		public void TopDataElement2()
		{
			string temp1 = Path.GetTempFileName();
			string temp2 = Path.GetTempFileName();
			File.WriteAllBytes(temp1, Encoding.ASCII.GetBytes("Hello World"));
			File.WriteAllBytes(temp2, Encoding.ASCII.GetBytes("Hello World"));

			string xml =
@"<Peach>
	<Data name='data' fileName='{0}'/>

	<DataModel name='TheDataModel'>
		<String name='str' value='Hello World!'/>
	</DataModel>

	<StateModel name='TheState' initialState='Initial'>
		<State name='Initial'>
			<Action type='output'>
				<DataModel ref='TheDataModel'/>
				<Data ref='data' fileName='{1}'/>
			</Action>
		</State>
	</StateModel>

	<Test name='Default'>
		<StateModel ref='TheState'/>
		<Publisher class='Null'/>
	</Test>
</Peach>";

			xml = string.Format(xml, temp1, temp2);

			PitParser parser = new PitParser();

			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			var ds = dom.stateModels[0].states["Initial"].actions[0].dataSet;
			Assert.NotNull(ds);
			Assert.AreEqual(1, ds.Datas.Count);
			Assert.AreEqual(temp2, ds.Datas[0].FileName);
			Assert.AreEqual(temp1, dom.datas[0].FileName);
		}

		[Test]
		public void TopDataElementGlob()
		{
			string tempDir = Path.GetTempFileName() + "_d";

			Directory.CreateDirectory(tempDir);
			File.WriteAllText(Path.Combine(tempDir, "1.txt"), "");
			File.WriteAllText(Path.Combine(tempDir, "2.txt"), "");
			File.WriteAllText(Path.Combine(tempDir, "2a.txt"), "");
			File.WriteAllText(Path.Combine(tempDir, "1.png"), "");
			File.WriteAllText(Path.Combine(tempDir, "2.png"), "");
			File.WriteAllText(Path.Combine(tempDir, "2a.png"), "");


			{
				string xml = string.Format("<Peach><Data name='data' fileName='*'/></Peach>", tempDir);
				PitParser parser = new PitParser();
				Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
				Assert.AreEqual(1, dom.datas.Count);
				Assert.True(dom.datas.ContainsKey("data"));
				var ds = dom.datas["data"];
				Assert.Greater(ds.Files.Count, 0);
			}

			Assert.Throws<PeachException>(delegate()
			{
				string xml = string.Format("<Peach><Data name='data' fileName=''/></Peach>", tempDir);
				PitParser parser = new PitParser();
				parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			});

			Assert.Throws<PeachException>(delegate()
			{
				string xml = string.Format("<Peach><Data name='data' fileName='foo'/></Peach>", tempDir);
				PitParser parser = new PitParser();
				parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			});

			Assert.Throws<PeachException>(delegate()
			{
				string xml = string.Format("<Peach><Data name='data' fileName='*/foo'/></Peach>", tempDir);
				PitParser parser = new PitParser();
				parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			});

			{
				string xml = string.Format("<Peach><Data name='data' fileName='{0}/*'/></Peach>", tempDir);
				PitParser parser = new PitParser();
				Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
				Assert.AreEqual(1, dom.datas.Count);
				Assert.True(dom.datas.ContainsKey("data"));
				var ds = dom.datas["data"];
				Assert.AreEqual(6, ds.Files.Count);
			}

			{
				string xml = string.Format("<Peach><Data name='data' fileName='{0}/*.txt'/></Peach>", tempDir);
				PitParser parser = new PitParser();
				Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
				Assert.AreEqual(1, dom.datas.Count);
				Assert.True(dom.datas.ContainsKey("data"));
				var ds = dom.datas["data"];
				Assert.AreEqual(3, ds.Files.Count);
			}

			{
				string xml = string.Format("<Peach><Data name='data' fileName='{0}/1.*'/></Peach>", tempDir);
				PitParser parser = new PitParser();
				Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
				Assert.AreEqual(1, dom.datas.Count);
				Assert.True(dom.datas.ContainsKey("data"));
				var ds = dom.datas["data"];
				Assert.AreEqual(2, ds.Files.Count);
			}

			{
				string xml = string.Format("<Peach><Data name='data' fileName='{0}/2*.txt'/></Peach>", tempDir);
				PitParser parser = new PitParser();
				Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
				Assert.AreEqual(1, dom.datas.Count);
				Assert.True(dom.datas.ContainsKey("data"));
				var ds = dom.datas["data"];
				Assert.AreEqual(2, ds.Files.Count);
			}

			{
				string xml = string.Format("<Peach><Data name='data' fileName='{0}/*a.*'/></Peach>", tempDir);
				PitParser parser = new PitParser();
				Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
				Assert.AreEqual(1, dom.datas.Count);
				Assert.True(dom.datas.ContainsKey("data"));
				var ds = dom.datas["data"];
				Assert.AreEqual(2, ds.Files.Count);
			}
		}

		[Test]
		public void AgentPlatform()
		{
			string xml =
@"<Peach>
	<DataModel name='TheDataModel'>
		<String name='str' value='Hello World!'/>
	</DataModel>

	<StateModel name='TheState' initialState='Initial'>
		<State name='Initial'>
			<Action type='output'>
				<DataModel ref='TheDataModel'/>
			</Action>
		</State>
	</StateModel>

	<Agent name='TheAgent'>
		<Monitor class='FaultingMonitor'>
			<Param name='Iteration' value='2'/>
		</Monitor>
	</Agent>

	<Test name='Default'>
		<StateModel ref='TheState'/>
		<Publisher class='Null'/>
		<Agent ref='TheAgent' platform='{0}'/>
		<Mutators mode='include'>
			<Mutator class='StringCaseMutator'/>
		</Mutators>
		<Strategy class='RandomDeterministic'/>
	</Test>
</Peach>";
			xml = string.Format(xml, Platform.GetOS() == Platform.OS.Windows ? "linux" : "windows");

			PitParser parser = new PitParser();

			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();

			Engine e = new Engine(null);
			e.Fault += delegate(RunContext context, uint currentIteration, Dom.StateModel stateModel, Fault[] faultData)
			{
				Assert.Fail("Fault should not be detected!");
			};

			e.startFuzzing(dom, config);
		}

		[Test]
		public void RefNamespace()
		{
			string tmp1 = Path.GetTempFileName();
			string tmp2 = Path.GetTempFileName();

			string xml1 = @"
<Peach>
	<DataModel name='TLV'>
		<Number name='Type' size='8' endian='big'/>
		<Number name='Length' size='8'>
			<Relation type='size' of='Value'/>
		</Number>
		<Block name='Value'/>
	</DataModel>

	<Agent name='ThirdAgent'/>

	<DataModel name='Random'>
		<String value='Hello World'/>
	</DataModel>
</Peach>";

			string xml2 = @"
<Peach>
	<Include ns='bar' src='{0}'/>

	<DataModel name='DM'>
		<Block ref='bar:TLV' name='Type1'>
			<Number name='Type' size='8' endian='big' value='201'/>
			<Block name='Value'>
				<Blob length='10' value='0000000000'/>
			</Block>
		</Block>
	</DataModel>

	<Agent name='SomeAgent'/>

	<StateModel name='SM' initialState='InitialState'>
		<State name='InitialState'>
			<Action name='Action1' type='output'>
				<DataModel ref='bar:Random'/>
			</Action>
		</State>
	</StateModel>

</Peach>".Fmt(tmp1);

			string xml3 = @"
<Peach>
	<Include ns='foo' src='{0}'/>

	<DataModel name='DM' ref='foo:DM'>
		<Blob/>
	</DataModel>

	<DataModel name='DM2'>
		<Block ref='foo:bar:Random'/>
	</DataModel>

	<StateModel name='TheStateModel' initialState='InitialState'>
		<State name='InitialState'>
			<Action name='Action1' type='output'>
				<DataModel ref='foo:bar:Random'/>
			</Action>
		</State>
	</StateModel>

	<Test name='Default'>
		<StateModel ref='TheStateModel'/>
		<Publisher class='Null'/>
	</Test>

	<Test name='Other'>
		<StateModel ref='foo:SM'/>
		<Publisher class='Null'/>
	</Test>

	<Test name='Third'>
		<StateModel ref='TheStateModel'/>
		<Agent ref='foo:SomeAgent' />
		<Agent ref='foo:bar:ThirdAgent' />
		<Publisher class='Null'/>
	</Test>

</Peach>".Fmt(tmp2);

			File.WriteAllText(tmp1, xml1);
			File.WriteAllText(tmp2, xml2);

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml3)));

			var final = dom.dataModels[1].Value.Value;
			var expected = Encoding.ASCII.GetBytes("Hello World");

			Assert.AreEqual(expected, final);
		}

	}
}
