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
	class IncludeTests
	{
		[Test]
		public void Test1()
		{
			string inc1 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Peach>
	<DataModel name=""HelloWorldTemplate"">
		<String name=""str"" value=""Hello World!""/>
		<String>
			<Relation type=""size"" of=""HelloWorldTemplate""/>
		</String>
	</DataModel>
</Peach>
";

			string template = @"
<Peach>
	<Include ns=""example"" src=""{0}"" />

	<StateModel name=""State"" initialState=""Initial"">
		<State name=""Initial"">
			<Action type=""output"">
				<DataModel name=""foo"" ref=""example:HelloWorldTemplate"" />
			</Action>
		</State>
	</StateModel>
	
	<Test name=""Default"">
		<StateModel ref=""State"" />
		<Publisher class=""File"">
			<Param name=""FileName"" value=""{1}""/>
		</Publisher>
	</Test>
	
</Peach>";
			
			string remote = Path.GetTempFileName();
			string output = Path.GetTempFileName();

			string xml = string.Format(template, remote, output);

			using (TextWriter writer = File.CreateText(remote))
			{
				writer.Write(inc1);
			}

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			string result = File.ReadAllText(output);

			Assert.AreEqual("Hello World!13", result);
		}

		[Test]
		public void Test2()
		{
			string inc1 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Peach>

	<DataModel name=""BaseModel"">
		<String name=""str"" value=""Hello World!""/>
	</DataModel>

	<DataModel name=""HelloWorldTemplate"" ref=""BaseModel"">
	</DataModel>
</Peach>
";

			string template = @"
<Peach>
	<Include ns=""example"" src=""file:{0}"" />

	<DataModel name=""DM"">
		<Block ref=""example:HelloWorldTemplate""/>
	</DataModel>

	<StateModel name=""State"" initialState=""Initial"">
		<State name=""Initial"">
			<Action type=""output"">
				<DataModel ref=""DM"" />
			</Action>
		</State>
	</StateModel>
	
	<Test name=""Default"">
		<StateModel ref=""State"" />
		<Publisher class=""File"">
			<Param name=""FileName"" value=""{1}""/>
		</Publisher>
	</Test>
	
</Peach>";

			string remote = Path.GetTempFileName();
			string output = Path.GetTempFileName();

			string xml = string.Format(template, remote, output);

			using (TextWriter writer = File.CreateText(remote))
			{
				writer.Write(inc1);
			}

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			string result = File.ReadAllText(output);

			Assert.AreEqual("Hello World!", result);
		}

		[Test]
		public void Test3()
		{
			string inc1 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Peach>
	<DataModel name=""BaseModel"">
		<String name=""str"" value=""Hello World!""/>
	</DataModel>

	<DataModel name=""DerivedModel"">
		<Block ref=""BaseModel"" />
	</DataModel>
</Peach>
";

			string inc2 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Peach>
	<Include ns=""abc"" src=""file:{0}"" />

	<DataModel name=""BaseModel2"" ref=""abc:DerivedModel""/>

</Peach>
";

			string template = @"
<Peach>
	<Include ns=""example"" src=""file:{0}"" />

	<DataModel name=""DM"">
		<Block ref=""example:BaseModel2""/>
	</DataModel>

	<StateModel name=""State"" initialState=""Initial"">
		<State name=""Initial"">
			<Action type=""output"">
				<DataModel ref=""DM"" />
			</Action>
		</State>
	</StateModel>
	
	<Test name=""Default"">
		<StateModel ref=""State"" />
		<Publisher class=""File"">
			<Param name=""FileName"" value=""{1}""/>
		</Publisher>
	</Test>
	
</Peach>";

			string remote1 = Path.GetTempFileName();
			string remote2 = Path.GetTempFileName();
			string output = Path.GetTempFileName();

			string xml = string.Format(template, remote2, output);

			using (TextWriter writer = File.CreateText(remote1))
			{
				writer.Write(inc1);
			}

			using (TextWriter writer = File.CreateText(remote2))
			{
				writer.Write(string.Format(inc2, remote1));
			}

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			string result = File.ReadAllText(output);

			Assert.AreEqual("Hello World!", result);
		}

		[Test]
		public void Test4()
		{
			string inc1 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Peach>
	<DataModel name=""HelloWorldTemplate"">
		<Number name=""Size"" size=""8"">
			<Relation type=""size"" of=""HelloWorldTemplate""/>
		</Number>
		<String name=""str"" value=""four""/>
	</DataModel>
</Peach>
";

			string template = @"
<Peach>
	<Include ns=""example"" src=""{0}"" />

	<DataModel name='Foo'>
		<String name='slurp' value='slurping' />
	</DataModel>

	<StateModel name=""State"" initialState=""Initial"">
		<State name=""Initial"">
			<Action type=""output"">
				<DataModel ref=""example:HelloWorldTemplate"" />
			</Action>
		</State>
	</StateModel>
	
	<StateModel name=""StateOverride"" initialState=""Initial"">
		<State name=""Initial"">
			<Action type=""output"">
				<DataModel ref=""example:HelloWorldTemplate"" />
				<Data>
					<Field name=""str"" value=""hello""/>
				</Data>
			</Action>
		</State>
	</StateModel>

	<StateModel name=""StateSlurp"" initialState=""Initial"">
		<State name=""Initial"">
			<Action type=""output"" publisher=""pub2"">
				<DataModel ref=""Foo"" />
			</Action>

			<Action type=""slurp"" valueXpath=""//slurp"" setXpath=""//str""/>

			<Action type=""output"">
				<DataModel ref=""example:HelloWorldTemplate"" />
			</Action>
		</State>
	</StateModel>

	<Test name=""Slurp"">
		<StateModel ref=""StateSlurp"" />
		<Publisher class=""File"">
			<Param name=""FileName"" value=""{1}""/>
		</Publisher>
		<Publisher class=""Null"" name=""pub2""/>
	</Test>

	<Test name=""Override"">
		<StateModel ref=""StateOverride"" />
		<Publisher class=""File"">
			<Param name=""FileName"" value=""{1}""/>
		</Publisher>
	</Test>

	<Test name=""Default"">
		<StateModel ref=""State"" />
		<Publisher class=""File"">
			<Param name=""FileName"" value=""{1}""/>
		</Publisher>
	</Test>
	
</Peach>";

			string remote = Path.GetTempFileName();
			string output = Path.GetTempFileName();

			string xml = string.Format(template, remote, output);

			using (TextWriter writer = File.CreateText(remote))
			{
				writer.Write(inc1);
			}

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			config.runName = "Default";
			e.startFuzzing(dom, config);

			byte[] result = File.ReadAllBytes(output);

			Assert.AreEqual(Encoding.ASCII.GetBytes("\x0005four"), result);

			dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			e = new Engine(null);
			config.runName = "Override";
			e.startFuzzing(dom, config);

			result = File.ReadAllBytes(output);

			Assert.AreEqual(Encoding.ASCII.GetBytes("\x0006hello"), result);

			dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			e = new Engine(null);
			config.runName = "Slurp";
			e.startFuzzing(dom, config);

			result = File.ReadAllBytes(output);

			Assert.AreEqual(Encoding.ASCII.GetBytes("\x0009slurping"), result);
		}

		[Test]
		public void Test5()
		{
			string inc1 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Peach>
	<DataModel name=""Model1"">
		<String name=""str1"" value=""foo""/>
	</DataModel>

	<DataModel name=""Model2"">
		<String name=""str2"" value=""bar""/>
	</DataModel>
</Peach>
";

			string template = @"
<Peach>
	<Include ns=""example"" src=""{0}"" />

	<StateModel name=""State"" initialState=""Initial"">
		<State name=""Initial"">
			<Action type=""output"">
				<DataModel ref=""example:Model1"" />
			</Action>

			<Action type=""slurp"" valueXpath=""//example:Model1/str1"" setXpath=""//example:Model2//str2"" />

			<Action type=""output"">
				<DataModel ref=""example:Model2"" />
			</Action>
		</State>
	</StateModel>

	<Test name=""Default"">
		<StateModel ref=""State"" />
		<Publisher class=""Null""/>
	</Test>
</Peach>";

			string remote = Path.GetTempFileName();
			string xml = string.Format(template, remote);
			File.WriteAllText(remote, inc1);

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			var act = dom.tests[0].stateModel.states["Initial"].actions[2];
			Assert.AreEqual("foo", (string)act.dataModel[0].DefaultValue);
		}

		[Test]
		public void MultipleIncludes()
		{
			string inc1 = @"<?xml version='1.0' encoding='utf-8'?>
<Peach>
	<DataModel name='Frame'>
		<Blob name='Type' value='type'/>
		<Blob name='Payload' value='Frame Payload'/>
	</DataModel>

</Peach>
";

			string inc2 = @"<?xml version='1.0' encoding='utf-8'?>
<Peach>
	<DataModel name='Packet'>
		<Blob name='Src' value='src'/>
		<Blob name='Dst' value='dst'/>
		<Blob name='Payload' value='Packet Payload'/>
	</DataModel>

</Peach>
";

			string tmp1 = Path.GetTempFileName();
			string tmp2 = Path.GetTempFileName();
			File.WriteAllText(tmp1, inc1);
			File.WriteAllText(tmp2, inc2);

			string xml = @"
<Peach>
	<Include ns='NS1' src='{0}' />
	<Include ns='NS2' src='{1}' />

	<DataModel name='Packet' ref='NS1:Frame'>
		<Block name='Payload' ref='NS2:Packet'/>
	</DataModel>
</Peach>".Fmt(tmp1, tmp2);

			var parser = new PitParser();
			var dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			Assert.AreEqual(1, dom.dataModels.Count);

			var val = dom.dataModels[0].InternalValue.BitsToString();

			var exp = "typesrcdstPacket Payload";
			Assert.AreEqual(exp, val);
		}
	}
}

