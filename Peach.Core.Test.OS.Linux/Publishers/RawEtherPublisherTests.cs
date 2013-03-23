using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core;
using Peach.Core.Analyzers;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;

namespace Peach.Core.Test.Publishers
{
	[TestFixture]
	class RawEtherPublisherTests
	{

		[Test]
		public void Test()
		{
			string template = @"
<Peach>
	<DataModel name=""TheDataModel"">
		<Blob name=""buf"" valueType=""hex"" value=""{0}""/>
	</DataModel>

	<StateModel name=""TheStateModel"" initialState=""InitialState"">
		<State name=""InitialState"">
			<Action name=""Send"" type=""output"">
				<DataModel ref=""TheDataModel""/>
			</Action>

			<Action name=""Recv"" type=""input"">
				<DataModel ref=""TheDataModel""/>
			</Action>
		</State>
	</StateModel>

	<Test name=""Default"">
		<StateModel ref=""TheStateModel""/>
		<Publisher class=""RawEther"">
			<Param name=""Interface"" value=""eth0""/>
			<Param name=""Protocol"" value=""ETH_P_IP""/>
		</Publisher>
	</Test>

</Peach>
";
			string xml = string.Format(template, new string('f', 2 * 50));

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

		}

		[Test]
		public void TestShort()
		{
			string xml = @"
<Peach>
	<DataModel name=""TheDataModel"">
		<String value=""Test""/>
	</DataModel>

	<StateModel name=""TheStateModel"" initialState=""InitialState"">
		<State name=""InitialState"">
			<Action name=""Send"" type=""output"">
				<DataModel ref=""TheDataModel""/>
			</Action>
		</State>
	</StateModel>

	<Test name=""Default"">
		<StateModel ref=""TheStateModel""/>
		<Publisher class=""RawEther"">
			<Param name=""Interface"" value=""eth0""/>
			<Param name=""Protocol"" value=""ETH_P_IP""/>
		</Publisher>
	</Test>

</Peach>
";
			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

		}
	}
}
