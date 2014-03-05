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
			Assert.AreEqual(args[0].type, ActionParameter.Type.In);
			Assert.AreEqual("Param1", (string)args[0].dataModel[0].InternalValue);

			Assert.AreEqual(args[1].name, "Named Param 2");
			Assert.AreEqual(args[1].type, ActionParameter.Type.Out);
			Assert.AreEqual("Hello", (string)args[1].dataModel[0].InternalValue);

			Assert.AreEqual(args[2].name, "Param");
			Assert.AreEqual(args[2].type, ActionParameter.Type.InOut);
			Assert.AreEqual("Param3", (string)args[2].dataModel[0].InternalValue);

			Assert.AreEqual(args[3].name, "Param_1");
			Assert.AreEqual(args[3].type, ActionParameter.Type.In);
			Assert.AreEqual("Param4", (string)args[3].dataModel[0].InternalValue);

			return new Variant(Encoding.ASCII.GetBytes("The Result!"));
		}
	}

	[TestFixture]
	class ActionTests
	{
		bool started;
		bool finished;

		void TestDelegate(string attr)
		{
			// If onStart or onComplete throws, ensure Action.Start/Action.Finished delegates are notified

			string xml = @"
<Peach>
	<DataModel name='DM'>
		<String value='Hello'/>
	</DataModel>

	<StateModel name='SM' initialState='Initial'>
		<State name='Initial'>
			<Action name='action' type='output' {0}='foo' >
				<DataModel ref='DM'/>
			</Action>
		</State>
	</StateModel>

	<Test name='Default'>
		<StateModel ref='SM'/>
		<Publisher class='Null'/>
		<Strategy class='RandomDeterministic'/>
	</Test>
</Peach>".Fmt(attr);

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(Encoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);

			started = false;
			finished = false;

			Dom.Action.Starting +=new ActionStartingEventHandler(Action_Starting);
			Dom.Action.Finished += new ActionFinishedEventHandler(Action_Finished);

			try
			{
				e.startFuzzing(dom, config);
				Assert.Fail("should throw");
			}
			catch (PeachException)
			{
			}
			finally
			{
				Dom.Action.Starting -= new ActionStartingEventHandler(Action_Starting);
				Dom.Action.Finished -= new ActionFinishedEventHandler(Action_Finished);
			}

			Assert.AreEqual(true, started);
			Assert.AreEqual(true, finished);
		}

		void Action_Finished(Dom.Action action)
		{
			Assert.AreEqual(false, finished);
			finished = true;
		}

		void Action_Starting(Dom.Action action)
		{
			Assert.AreEqual(false, started);
			started = true;
		}

		[Test]
		public void TestOnStartThrow()
		{
			TestDelegate("onStart");
		}

		[Test]
		public void TestOnCompleteThrow()
		{
			TestDelegate("onComplete");
		}

		void RunAction(string action, string children, string attr)
		{
			string xml = @"
<Peach>
	<DataModel name='DM'>
		<String value='Hello'/>
	</DataModel>

	<StateModel name='SM' initialState='Initial'>
		<State name='Initial'>
			<Action name='action' type='{0}' {2} >
{1}
			</Action>
		</State>
	</StateModel>

	<Test name='Default'>
		<StateModel ref='SM'/>
		<Publisher class='Null'/>
		<Strategy class='RandomDeterministic'/>
	</Test>
</Peach>".Fmt(action, children, attr);

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
				RunAction(action, "", "");
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Error, action 'SM.Initial.action' is missing required child element <DataModel>.")]
		public void Test2()
		{
			RunAction("input", "", "");
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Error, action 'SM.Initial.action' is missing required child element <DataModel>.")]
		public void Test3()
		{
			RunAction("output", "", "");
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Error, action 'SM.Initial.action' is missing required child element <DataModel>.")]
		public void Test4()
		{
			RunAction("setProperty", "", "property='foo'");
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Error, action 'SM.Initial.action' is missing required child element <DataModel>.")]
		public void Test5()
		{
			RunAction("getProperty", "", "property='foo'");
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Error, <Param> child of action 'SM.Initial.action' is missing required child element <DataModel>.")]
		public void Test6()
		{
			RunAction("call", "<Param/>", "method='foo'");
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Error, action 'SM.Initial.action' has unsupported child element <Data>.")]
		public void Test7()
		{
			// Input should error with <Data>
			RunAction("input", "<DataModel ref='DM'/><Data/>", "");
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Error, action 'SM.Initial.action' has unsupported child element <Data>.")]
		public void Test8()
		{
			// GetProperty should error with <Data>
			RunAction("getProperty", "<DataModel ref='DM'/><Data/>", "property='foo'");
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Error, <Param> child of action 'SM.Initial.action' has unsupported child element <Data>.")]
		public void Test9()
		{
			// Call w/Out param should error with <Data>
			RunAction("call", "<Param type='out'><DataModel ref='DM'/><Data/></Param>", "method='foo'");
		}

		[Test, ExpectedException(typeof(PeachException))]
		public void Test10()
		{
			// Call w/ <Result> should error with <Data>
			RunAction("call", "<Result><DataModel ref='DM'/><Data/></Result>", "method='foo'");
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Error, a <Data> element named 'myData' already exists.")]
		public void Test11()
		{
			string xml = @"
<Peach>
	<Data name='myData'><Field name='foo' value='bar'/></Data>
	<Data name='myData'><Field name='foo' value='bar'/></Data>
</Peach>
";
			new PitParser().asParser(null, new MemoryStream(Encoding.ASCII.GetBytes(xml)));
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Error, a <DataModel> element named 'dm' already exists.")]
		public void Test12()
		{
			string xml = @"
<Peach>
	<DataModel name='dm'/>
	<DataModel name='dm'/>
</Peach>
";
			new PitParser().asParser(null, new MemoryStream(Encoding.ASCII.GetBytes(xml)));
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Error, a <StateModel> element named 'sm' already exists.")]
		public void Test13()
		{
			string xml = @"
<Peach>
	<StateModel name='sm' initialState='Initial'>
		<State name='Initial'/>
	</StateModel>
	<StateModel name='sm' initialState='Initial'>
		<State name='Initial'/>
	</StateModel>
</Peach>
";
			new PitParser().asParser(null, new MemoryStream(Encoding.ASCII.GetBytes(xml)));
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Error, a <Test> element named 'Default' already exists.")]
		public void Test14()
		{
			string xml = @"
<Peach>
	<StateModel name='sm' initialState='Initial'>
		<State name='Initial'/>
	</StateModel>

	<Test name='Default'>
		<StateModel ref='sm'/>
		<Publisher class='Null'/>
	</Test>

	<Test name='Default'>
		<StateModel ref='sm'/>
		<Publisher class='Null'/>
	</Test>
</Peach>
";
			new PitParser().asParser(null, new MemoryStream(Encoding.ASCII.GetBytes(xml)));
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Error, a <State> element named 'Initial' already exists in state model 'sm'.")]
		public void Test15()
		{
			string xml = @"
<Peach>
	<StateModel name='sm' initialState='Initial'>
		<State name='Initial'/>
		<State name='Initial'/>
	</StateModel>
</Peach>
";
			new PitParser().asParser(null, new MemoryStream(Encoding.ASCII.GetBytes(xml)));
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Error, a <Action> element named 'stuff' already exists in state 'sm.initial'.")]
		public void Test16()
		{
			string xml = @"
<Peach>
	<StateModel name='sm' initialState='initial'>
		<State name='initial'>
			<Action name='stuff' type='open'/>
			<Action name='stuff' type='open'/>
		</State>
	</StateModel>
</Peach>
";
			new PitParser().asParser(null, new MemoryStream(Encoding.ASCII.GetBytes(xml)));
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Error, a <Data> element named 'myData' already exists in action 'sm.initial.stuff'.")]
		public void Test17()
		{
			string xml = @"
<Peach>
	<DataModel name='dm'/>

	<StateModel name='sm' initialState='initial'>
		<State name='initial'>
			<Action name='stuff' type='output'>
				<DataModel ref='dm'/>
				<Data name='myData'><Field name='foo' value='bar'/></Data>
				<Data name='myData'><Field name='foo' value='bar'/></Data>
			</Action>
		</State>
	</StateModel>
</Peach>
";
			new PitParser().asParser(null, new MemoryStream(Encoding.ASCII.GetBytes(xml)));
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Error, a <Param> element named 'myParam' already exists in action 'sm.initial.stuff'.")]
		public void Test18()
		{
			string xml = @"
<Peach>
	<DataModel name='dm'/>

	<StateModel name='sm' initialState='initial'>
		<State name='initial'>
			<Action name='stuff' type='call' method='foo'>
				<Param name='myParam'>
					<DataModel ref='dm'/>
				</Param>
				<Param name='myParam'>
					<DataModel ref='dm'/>
				</Param>
			</Action>
		</State>
	</StateModel>
</Peach>
";
			new PitParser().asParser(null, new MemoryStream(Encoding.ASCII.GetBytes(xml)));
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Error, a <Data> element named 'myData' already exists in <Param> child of action 'sm.initial.stuff'.")]
		public void Test19()
		{
			string xml = @"
<Peach>
	<DataModel name='dm'/>

	<StateModel name='sm' initialState='initial'>
		<State name='initial'>
			<Action name='stuff' type='call' method='foo'>
				<Param name='myParam'>
					<DataModel ref='dm'/>
					<Data name='myData'><Field name='foo' value='bar'/></Data>
					<Data name='myData'><Field name='foo' value='bar'/></Data>
				</Param>
			</Action>
		</State>
	</StateModel>
</Peach>
";
			new PitParser().asParser(null, new MemoryStream(Encoding.ASCII.GetBytes(xml)));
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Error, a <Agent> element named 'myAgent' already exists.")]
		public void Test20()
		{
			string xml = @"
<Peach>
	<Agent name='myAgent'/>
	<Agent name='myAgent'/>
</Peach>
";
			new PitParser().asParser(null, new MemoryStream(Encoding.ASCII.GetBytes(xml)));
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Error, a <Monitor> element named 'mon' already exists in agent 'myAgent'.")]
		public void Test21()
		{
			string xml = @"
<Peach>
	<Agent name='myAgent'>
		<Monitor name='mon' class='FaultingMonitor'/>
		<Monitor name='mon' class='FaultingMonitor'/>
	</Agent>
</Peach>
";
			new PitParser().asParser(null, new MemoryStream(Encoding.ASCII.GetBytes(xml)));
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Error, a <Publisher> element named 'myPub' already exists in test 'Default'.")]
		public void Test22()
		{
			string xml = @"
<Peach>
	<StateModel name='sm' initialState='Initial'>
		<State name='Initial'/>
	</StateModel>

	<Test name='Default'>
		<StateModel ref='sm'/>
		<Publisher class='Null'/>
	</Test>

	<Test name='Default'>
		<StateModel ref='sm'/>
		<Publisher name='myPub' class='Null'/>
		<Publisher name='myPub' class='Null'/>
	</Test>
</Peach>
";
			new PitParser().asParser(null, new MemoryStream(Encoding.ASCII.GetBytes(xml)));
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
			<Action name='action' type='call' method='foo'>
				<Param name='Named Param 1' type='in'>
					<DataModel ref='DM1'/>
					<Data>
						<Field name='str1' value='Param1'/>
					</Data>
				</Param>
				<Param name='Named Param 2' type='out'>
					<DataModel ref='DM1'/>
				</Param>
				<Param type='inout'>
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
				<Result>
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

			var act = dom.tests[0].stateModel.states["Initial"].actions[0] as Dom.Actions.Call;

			Assert.NotNull(act.result);
			Assert.NotNull(act.result.dataModel);
			string str = (string)act.result.dataModel[0].InternalValue;
			Assert.AreEqual("The Result!", str);

		}

		[Test]
		public void TestUniqueNames()
		{
			var tmp = Path.GetTempFileName();

			string xml = @"
<Peach>
	<DataModel name='DM'>
		<String name='str' value='Hello'/>
	</DataModel>

	<Agent name='MyAgent'>
		<Monitor class='FaultingMonitor'/>
		<Monitor class='FaultingMonitor'/>
		<Monitor class='FaultingMonitor'/>
	</Agent>

	<Agent name='MyAgent2'>
		<Monitor class='FaultingMonitor'/>
		<Monitor class='FaultingMonitor'/>
		<Monitor class='FaultingMonitor'/>
	</Agent>

	<StateModel name='SM' initialState='Initial'>
		<State name='Initial'>
			<Action type='call' method='foo' >
				<Param>
					<DataModel ref='DM'/>
					<Data fileName='{0}'/>
					<Data fileName='{0}'/>
					<Data fileName='{0}'/>
					<Data><Field name='str' value='other'/></Data>
				</Param>
				<Param>
					<DataModel ref='DM'/>
					<Data fileName='{0}'/>
					<Data fileName='{0}'/>
					<Data fileName='{0}'/>
					<Data><Field name='str' value='other'/></Data>
				</Param>
				<Param>
					<DataModel ref='DM'/>
					<Data fileName='{0}'/>
					<Data fileName='{0}'/>
					<Data fileName='{0}'/>
					<Data><Field name='str' value='other'/></Data>
				</Param>
			</Action>
			<Action type='call' method='foo' >
				<Param>
					<DataModel ref='DM'/>
					<Data fileName='{0}'/>
					<Data fileName='{0}'/>
					<Data fileName='{0}'/>
					<Data><Field name='str' value='other'/></Data>
				</Param>
				<Param>
					<DataModel ref='DM'/>
					<Data fileName='{0}'/>
					<Data fileName='{0}'/>
					<Data fileName='{0}'/>
					<Data><Field name='str' value='other'/></Data>
				</Param>
				<Param>
					<DataModel ref='DM'/>
					<Data fileName='{0}'/>
					<Data fileName='{0}'/>
					<Data fileName='{0}'/>
					<Data><Field name='str' value='other'/></Data>
				</Param>
			</Action>
		</State>
		<State name='Second'>
			<Action type='call' method='foo' >
				<Param>
					<DataModel ref='DM'/>
					<Data fileName='{0}'/>
					<Data fileName='{0}'/>
					<Data fileName='{0}'/>
					<Data><Field name='str' value='other'/></Data>
				</Param>
				<Param>
					<DataModel ref='DM'/>
					<Data fileName='{0}'/>
					<Data fileName='{0}'/>
					<Data fileName='{0}'/>
					<Data><Field name='str' value='other'/></Data>
				</Param>
				<Param>
					<DataModel ref='DM'/>
					<Data fileName='{0}'/>
					<Data fileName='{0}'/>
					<Data fileName='{0}'/>
					<Data><Field name='str' value='other'/></Data>
				</Param>
			</Action>
			<Action type='call' method='foo' >
				<Param>
					<DataModel ref='DM'/>
					<Data fileName='{0}'/>
					<Data fileName='{0}'/>
					<Data fileName='{0}'/>
					<Data><Field name='str' value='other'/></Data>
				</Param>
				<Param>
					<DataModel ref='DM'/>
					<Data fileName='{0}'/>
					<Data fileName='{0}'/>
					<Data fileName='{0}'/>
					<Data><Field name='str' value='other'/></Data>
				</Param>
				<Param>
					<DataModel ref='DM'/>
					<Data fileName='{0}'/>
					<Data fileName='{0}'/>
					<Data fileName='{0}'/>
					<Data><Field name='str' value='other'/></Data>
				</Param>
			</Action>
		</State>
	</StateModel>

	<Test name='Default'>
		<StateModel ref='SM'/>
		<Publisher class='Null'/>
		<Publisher class='Null'/>
		<Publisher class='Null'/>
		<Strategy class='RandomDeterministic'/>
	</Test>
</Peach>".Fmt(tmp);

			var tmpName = Path.GetFileName(tmp);
			Assert.False(tmpName.Contains(Path.DirectorySeparatorChar));

			var parser = new PitParser();
			var dom = parser.asParser(null, new MemoryStream(Encoding.ASCII.GetBytes(xml)));

			Assert.AreEqual(2, dom.agents.Count);
			foreach (var agent in dom.agents)
			{
				Assert.AreEqual(3, agent.monitors.Count);
				Assert.AreEqual("Monitor", agent.monitors[0].name);
				Assert.AreEqual("Monitor_1", agent.monitors[1].name);
				Assert.AreEqual("Monitor_2", agent.monitors[2].name);
			}

			Assert.AreEqual(3, dom.tests[0].publishers.Count);
			Assert.True(dom.tests[0].publishers.ContainsKey("Pub"));
			Assert.True(dom.tests[0].publishers.ContainsKey("Pub_1"));
			Assert.True(dom.tests[0].publishers.ContainsKey("Pub_2"));

			Assert.AreEqual(2, dom.tests[0].stateModel.states.Count);

			foreach (var state in dom.tests[0].stateModel.states)
			{
				Assert.AreEqual(2, state.actions.Count);
				Assert.AreEqual("Action", state.actions[0].name);
				Assert.AreEqual("Action_1", state.actions[1].name);

				foreach (var action in state.actions)
				{
					var actionData = action.allData.ToList();
					Assert.AreEqual(3, actionData.Count);
					Assert.AreEqual("Param", actionData[0].name);
					Assert.AreEqual("Param_1", actionData[1].name);
					Assert.AreEqual("Param_2", actionData[2].name);

					foreach (var item in actionData)
					{
						Assert.AreEqual(4, item.dataSets.Count);
						Assert.AreEqual("Data", item.dataSets[0].name);
						Assert.AreEqual("Data_1", item.dataSets[1].name);
						Assert.AreEqual("Data_2", item.dataSets[2].name);
						Assert.AreEqual("Data_3", item.dataSets[3].name);

						var data = item.allData.ToList();
						Assert.AreEqual(4, item.dataSets.Count);
						Assert.AreEqual("Data/" + tmpName, data[0].name);
						Assert.AreEqual("Data_1/" + tmpName, data[1].name);
						Assert.AreEqual("Data_2/" + tmpName, data[2].name);
						Assert.AreEqual("Data_3", data[3].name);
					}
				}

			}
		}

	}
}
