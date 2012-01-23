
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
//   Michael Eddington (mike@phed.org)

// $Id$

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
using Peach.Core.Loggers;
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

			tabControl.TabPages.Remove(tabPageGUI);
			tabControl.TabPages.Remove(tabPageFuzzing);
			//tabControl.TabPages.Remove(tabPageOutput);

			buttonStartFuzzing.Enabled = true;
			buttonSaveConfiguration.Enabled = false;
			buttonStopFuzzing.Enabled = false;

			if (Directory.Exists(@"C:\Program Files (x86)\Debugging Tools for Windows (x86)"))
				textBoxDebuggerPath.Text = @"C:\Program Files (x86)\Debugging Tools for Windows (x86)";
			if (Directory.Exists(@"C:\Program Files\Debugging Tools for Windows (x86)"))
				textBoxDebuggerPath.Text = @"C:\Program Files\Debugging Tools for Windows (x86)";
			if (Directory.Exists(@"C:\Program Files\Debugging Tools for Windows"))
				textBoxDebuggerPath.Text = @"C:\Program Files\Debugging Tools for Windows";
		}

		private void buttonStartFuzzing_Click(object sender, EventArgs e)
		{
			//tabControl.TabPages.Remove(tabPageGeneral);
			//tabControl.TabPages.Remove(tabPageDebugger);
			//tabControl.TabPages.Insert(0, tabPageOutput);
			tabControl.SelectedTab = tabPageOutput;
			buttonStartFuzzing.Enabled = false;
			buttonSaveConfiguration.Enabled = false;
			buttonStopFuzzing.Enabled = true;

			Dom dom = new Dom();

			byte [] buff = File.ReadAllBytes(textBoxTemplateFiles.Text);

			// DataModel
			DataModel dataModel = new DataModel("TheDataModel");
			Peach.Core.Dom.Blob blob = new Peach.Core.Dom.Blob(new Variant(buff));
			dataModel.Add(blob);

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

			Dictionary<string, Variant> loggerArgs = new Dictionary<string, Variant>();
			loggerArgs["Path"] = new Variant(textBoxLogPath.Text);
			run.logger = new Peach.Core.Loggers.FileLogger(loggerArgs);

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

			config.pitFile = "PeachFuzzBang";

			if (!string.IsNullOrEmpty(textBoxIterations.Text))
			{
				try
				{
					int iter = int.Parse(textBoxIterations.Text);
					config.range = true;
					config.rangeStart = 0;
					config.rangeStop = (uint)iter;
				}
				catch
				{
				}
			}

			Engine.RunFinished += new Engine.RunFinishedEventHandler(Engine_RunFinished);
			Engine.RunError += new Engine.RunErrorEventHandler(Engine_RunError);

			e.startFuzzing(dom, config);
		}

		void Engine_RunError(RunContext context, Exception e)
		{
			// TODO 
			//throw new NotImplementedException();
		}

		void Engine_RunFinished(RunContext context)
		{
			// TODO
			//throw new NotImplementedException();
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

		private void buttonStopFuzzing_Click(object sender, EventArgs e)
		{
			buttonStartFuzzing.Enabled = true;
			buttonSaveConfiguration.Enabled = false;
			buttonStopFuzzing.Enabled = false;

			tabControl.SelectedTab = tabPageGeneral;

			thread.Abort();
			thread = null;
		}

		private void buttonBrowseTemplates_Click(object sender, EventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Title = "Select Template";
			if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
				return;

			textBoxTemplateFiles.Text = dialog.FileName;
		}

		private void buttonBrowseFuzzedFile_Click(object sender, EventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Title = "Select Fuzzed File";
			if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
				return;

			textBoxFuzzedFile.Text = dialog.FileName;
		}

		private void buttonDebuggerCommandBrowse_Click(object sender, EventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Title = "Select Executable";
			if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
				return;

			textBoxDebuggerCommandLine.Text = dialog.FileName;
		}

		private void buttonLogPathBrowse_Click(object sender, EventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Title = "Select Logs Path";
			if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
				return;

			textBoxLogPath.Text = dialog.FileName;
		}
	}
}
