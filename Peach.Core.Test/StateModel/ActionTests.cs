using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using NLog;
using NLog.Targets;
using NLog.Config;

using NUnit.Framework;
using NUnit.Framework.Constraints;

using Peach.Core;
using Peach.Core.IO;
using Peach.Core.Dom;
using Peach.Core.Analyzers;
using Peach.Core.Cracker;
using Peach.Core.Publishers;

namespace Peach.Core.Test.StateModel
{
	[TestFixture]
	class ActionTests
	{
		void RunAction(string action, string children)
		{
			string xml = @"
<Peach>
	<DataModel name='DM'>
		<String value='Hello'/>
	</DataModel>

	<StateModel name='SM' initialState='Initial'>
		<State name='Initial'>
			<Action name='action' type='{0}'>
{1}
			</Action>
		</State>
	</StateModel>

	<Test name='Default'>
		<StateModel ref='SM'/>
		<Publisher class='Null'/>
	</Test>
</Peach>".Fmt(action, children);

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(Encoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);
		}

		[Test]
		public void Test1()
		{
			// These actions do not require a <DataModel> child for the action
			string[] actions = new string[] { "start", "stop", "open", "close" };
			foreach (var action in actions)
				RunAction(action, "");
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Error, action 'action' is missing required child element <DataModel>.")]
		public void Test2()
		{
			RunAction("input", "");
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Error, action 'action' is missing required child element <DataModel>.")]
		public void Test3()
		{
			RunAction("output", "");
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Error, action 'action' is missing required child element <DataModel>.")]
		public void Test4()
		{
			RunAction("setProperty", "");
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Error, action 'action' is missing required child element <DataModel>.")]
		public void Test5()
		{
			RunAction("getProperty", "");
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Error, <Param> child of action 'action' is missing required child element <DataModel>.")]
		public void Test6()
		{
			RunAction("call", "<Param/>");
		}
	}
}
