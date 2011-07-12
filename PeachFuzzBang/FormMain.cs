using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using Peach;
using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Publishers;
using Peach.Core.Agent;
using Peach.Core.MutationStrategies;
using System.Threading;

namespace PeachFuzzBang
{
	public partial class FormMain : Form
	{
		public FormMain()
		{
			InitializeComponent();

			List<MutationStrategy> strategies = new List<MutationStrategy>();

			foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (Type t in a.GetExportedTypes())
				{
					if (!t.IsClass)
						continue;

					foreach (object attrib in t.GetCustomAttributes(true))
					{
						if (attrib is MutationStrategyAttribute)
						{
							//strategies.Add(((MutationStrategyAttribute)attrib).name);
							comboBoxFuzzingStrategy.Items.Add(((MutationStrategyAttribute)attrib).name);
						}
					}
				}
			}
		}

		private void button4_Click(object sender, EventArgs e)
		{
			tabControl1.SelectedTab = tabPageOutput;

			Dom dom = new Dom();

			// DataModel
			DataModel dataModel = new DataModel();
			dataModel.name = "TheDataModel";
			Peach.Core.Dom.String str = new Peach.Core.Dom.String();
			str.DefaultValue = new Variant("Hello World!");
			dataModel.Add(str);

			dom.dataModels.Add(dataModel.name, dataModel);

			// Publisher
			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["FileName"] = new Variant(textBoxFuzzedFile.Text);
			Peach.Core.Publishers.File file = new File(args);

			// StateModel
			StateModel stateModel = new StateModel();
			stateModel.name = "TheStateModel";

			State state = new State();
			state.name = "TheState";

			Peach.Core.Dom.Action actionOutput = new Peach.Core.Dom.Action();
			actionOutput.type = ActionType.Output;
			actionOutput.dataModel = dataModel;

			Peach.Core.Dom.Action actionClose = new Peach.Core.Dom.Action();
			actionClose.type = ActionType.Close;

			Peach.Core.Dom.Action actionCall = new Peach.Core.Dom.Action();
			actionCall.type = ActionType.Call;
			actionCall.publisher = "Peach.Agent";
			actionCall.method = "ScoobySnacks";

			state.actions.Add(actionOutput);
			state.actions.Add(actionClose);
			state.actions.Add(actionCall);

			stateModel.states.Add(state);
			stateModel.initialState = state;

			dom.stateModels.Add(stateModel.name, stateModel);

			// Agent
			Peach.Core.Dom.Agent agent = new Peach.Core.Dom.Agent();
			agent.name = "TheAgent";
			agent.url = "local://";

			Peach.Core.Dom.Monitor monitor = new Peach.Core.Dom.Monitor();
			monitor.cls = "WindowsDebugEngine";
			monitor.parameters["CommandLine"] = new Variant(textBoxExecutable.Text + " " + textBoxCommandLine.Text);
			monitor.parameters["StartOnCall"] = new Variant("ScoobySnacks");

			agent.monitors.Add(monitor);
			dom.agents.Add(agent.name, agent);

			// Mutation Strategy
			Sequencial strat = new Sequencial(new Dictionary<string, string>());

			// Test
			Test test = new Test();
			test.name = "TheTest";
			test.stateModel = stateModel;
			test.agents.Add(agent.name, agent);
			test.publishers.Add("FileWriter", file);
			test.strategy = strat;

			dom.tests.Add(test.name, test);

			// Run
			Run run = new Run();
			run.name = "DefaultRun";
			run.tests.Add(test.name, test);

			dom.runs.Add(run.name, run);

			// START FUZZING!!!!!
			Thread thread = new Thread(new ParameterizedThreadStart(Run));
			thread.Start(dom);
		}

		public void Run(object obj)
		{
			Dom dom = obj as Dom;
			RunConfiguration config = new RunConfiguration();
			Engine e = new Engine(new ConsoleWatcher(this));

			e.startFuzzing(dom, config);
		}
	}
}
