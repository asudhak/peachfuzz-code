using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
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
		Thread thread = null;

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

			MemoryStream sout = new MemoryStream();
			byte [] buff = new byte[1024];
			int cnt;

			using(FileStream sin = System.IO.File.OpenRead(textBoxTemplateFiles.Text))
			{
				cnt = sin.Read(buff, 0, buff.Length);
				sout.Write(buff, 0, cnt);
			}

			// DataModel
			DataModel dataModel = new DataModel("TheDataModel");
			//Peach.Core.Dom.Blob blob = new Peach.Core.Dom.Blob(new Variant(sout.ToArray()));
			//dataModel.Add(blob);
			Peach.Core.Dom.String str = new Peach.Core.Dom.String("Data", new Variant(ASCIIEncoding.ASCII.GetString(sout.ToArray())));
			dataModel.Add(str);

			dom.dataModels.Add(dataModel.name, dataModel);

			// Publisher
			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["FileName"] = new Variant(textBoxFuzzedFile.Text);
			Peach.Core.Publishers.FilePublisher file = new Peach.Core.Publishers.FilePublisher(args);

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

			stateModel.states.Add(state.name, state);
			stateModel.initialState = state;

			dom.stateModels.Add(stateModel.name, stateModel);

			// Agent
			Peach.Core.Dom.Agent agent = new Peach.Core.Dom.Agent();
			agent.name = "TheAgent";
			agent.url = "local://";

			Peach.Core.Dom.Monitor monitor = new Peach.Core.Dom.Monitor();
			monitor.cls = "WindowsDebugEngine";
			monitor.parameters["CommandLine"] = new Variant(textBoxDebuggerCommandLine.Text);
			monitor.parameters["StartOnCall"] = new Variant("ScoobySnacks");
			monitor.parameters["WinDbgPath"] = new Variant(textBoxDebuggerPath.Text);

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
			thread = new Thread(new ParameterizedThreadStart(Run));
			thread.Start(dom);
		}

		public void Run(object obj)
		{
			Dom dom = obj as Dom;
			RunConfiguration config = new RunConfiguration();
			Engine e = new Engine(new ConsoleWatcher(this));

			e.startFuzzing(dom, config);
		}

		private void buttonDebuggerPathBrowse_Click(object sender, EventArgs e)
		{
			OpenFileDialog browse = new OpenFileDialog();
			browse.DefaultExt = ".exe";
			browse.Title = "Browse to WinDbg";
			if (browse.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
				return;

			string fileName = browse.FileName;
			textBoxDebuggerPath.Text = fileName.Substring(0, fileName.LastIndexOf("\\"));
		}

		private void buttonPitFileNameLoad_Click(object sender, EventArgs e)
		{
			if (!System.IO.File.Exists(textBoxPitFileName.Text))
			{
				MessageBox.Show("Error, Pit file does not exist.");
				return;
			}

			label4.Enabled = true;
			comboBoxPitDataModel.Enabled = true;
		}

		private void buttonPitFileNameBrowse_Click(object sender, EventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.DefaultExt = ".xml";

			if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
				return;

			textBoxPitFileName.Text = dialog.FileName;
		}

		private void button6_Click(object sender, EventArgs e)
		{
			thread.Abort();
			thread = null;
		}
	}
}
