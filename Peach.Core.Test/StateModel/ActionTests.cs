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
	class ParamPublisher : Publisher
	{
		private static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		protected override NLog.Logger Logger { get { return logger; } }

		public ParamPublisher(Dictionary<string, Variant> args)
			: base(args)
		{
		}

		protected override Variant OnCall(string method, List<ActionParameter> args)
		{
			Assert.AreEqual(args[0].name, "Named Param 1");
			Assert.AreEqual(args[0].type, ActionParameterType.In);
			Assert.AreEqual("Param1", (string)args[0].dataModel[0].InternalValue);

			Assert.AreEqual(args[1].name, "Named Param 2");
			Assert.AreEqual(args[1].type, ActionParameterType.Out);
			Assert.AreEqual("Param2", (string)args[1].dataModel[0].InternalValue);

			Assert.AreEqual(args[2].name, "Named Param 3");
			Assert.AreEqual(args[2].type, ActionParameterType.InOut);
			Assert.AreEqual("Param3", (string)args[2].dataModel[0].InternalValue);

			Assert.True(args[3].name.StartsWith("Unknown Parameter"));
			Assert.AreEqual(args[3].type, ActionParameterType.In);
			Assert.AreEqual("Param4", (string)args[3].dataModel[0].InternalValue);

			return new Variant(Encoding.ASCII.GetBytes("The Result!"));
		}
	}

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
		<Strategy class='RandomDeterministic'/>
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

		[Test]
		public void TestActionParam()
		{
			string xml = @"
<Peach>
	<DataModel name='DM1'>
		<String name='str1' value='Hello' mutable='false'/>
		<String name='str2' value='World'/>
	</DataModel>

	<DataModel name='DM2'>
		<String name='str'/>
	</DataModel>

	<StateModel name='SM' initialState='Initial'>
		<State name='Initial'>
			<Action name='action' type='call'>
				<Param name='Named Param 1' type='in'>
					<DataModel ref='DM1'/>
					<Data>
						<Field name='str1' value='Param1'/>
					</Data>
				</Param>
				<Param name='Named Param 2' type='out'>
					<DataModel ref='DM1'/>
					<Data>
						<Field name='str1' value='Param2'/>
					</Data>
				</Param>
				<Param name='Named Param 3' type='inout'>
					<DataModel ref='DM1'/>
					<Data>
						<Field name='str1' value='Param3'/>
					</Data>
				</Param>
				<Param>
					<DataModel ref='DM1'/>
					<Data>
						<Field name='str1' value='Param4'/>
					</Data>
				</Param>
				<Result name='res'>
					<DataModel ref='DM2'/>
				</Result>
			</Action>
		</State>
	</StateModel>

	<Test name='Default'>
		<StateModel ref='SM'/>
		<Publisher class='Null'/>
		<Mutators mode='include'>
			<Mutator class='StringCaseMutator'/>
		</Mutators>
		<Strategy class='RandomDeterministic'/>
	</Test>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(Encoding.ASCII.GetBytes(xml)));
			dom.tests[0].publishers[0] = new ParamPublisher(new Dictionary<string, Variant>());

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			var act = dom.tests[0].stateModel.states["Initial"].actions[0];

			Assert.NotNull(act.result);
			Assert.AreEqual("res", act.result.name);
			Assert.NotNull(act.result.dataModel);
			string str = (string)act.result.dataModel[0].InternalValue;
			Assert.AreEqual("The Result!", str);

		}
	}
}
