
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
using System.ServiceProcess;

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
		public int IterationCount = 0;
		public int FaultCount = 0;

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

			comboBoxAttachToServiceServices.Items.Clear();
			foreach (ServiceController srv in ServiceController.GetServices())
			{
				comboBoxAttachToServiceServices.Items.Add(srv.ServiceName);
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

			IterationCount = 0;
			FaultCount = 0;
			textBoxIterationCount.Text = IterationCount.ToString();
			textBoxFaultCount.Text = FaultCount.ToString();
			textBoxOutput.Text = "";

			Dom dom = new Dom();

			// DataModel
			DataModel dataModel = new DataModel("TheDataModel");
			Peach.Core.Dom.Blob blob = null;

			Data fileData = new Data();
			if (Directory.Exists(textBoxTemplateFiles.Text))
			{
				List<string> files = new List<string>();
				foreach (string fileName in Directory.GetFiles(textBoxTemplateFiles.Text))
					files.Add(fileName);

				fileData.DataType = DataType.Files;
				fileData.Files = files;

				blob = new Peach.Core.Dom.Blob(new Variant(File.ReadAllBytes(files[0])));
			}
			else if (File.Exists(textBoxTemplateFiles.Text))
			{
				fileData.DataType = DataType.File;
				fileData.FileName = textBoxTemplateFiles.Text;

				blob = new Peach.Core.Dom.Blob(new Variant(File.ReadAllBytes(fileData.FileName)));
			}
			else
			{
				MessageBox.Show("Error, Unable to locate file/folder called \"" + textBoxTemplateFiles.Text + "\".");
				return;
			}

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
			actionOutput.dataSet = new Peach.Core.Dom.DataSet();
			actionOutput.dataSet.Datas.Add(fileData);

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
			monitor.parameters["StartOnCall"] = new Variant("ScoobySnacks");
			monitor.parameters["WinDbgPath"] = new Variant(textBoxDebuggerPath.Text);

			if(radioButtonDebuggerStartProcess.Checked)
				monitor.parameters["CommandLine"] = new Variant(textBoxDebuggerCommandLine.Text);
			else if (radioButtonDebuggerAttachToProcess.Checked)
			{
				if (radioButtonAttachToProcessPID.Checked)
					monitor.parameters["ProcessName"] = new Variant(textBoxAttachToProcessPID.Text);
				else if (radioButtonAttachToProcessProcessName.Checked)
					monitor.parameters["ProcessName"] = new Variant(textBoxAttachToProcessProcessName.Text);
			}
			else if (radioButtonDebuggerAttachToService.Checked)
				monitor.parameters["Service"] = new Variant(comboBoxAttachToServiceServices.Text);
			else if (radioButtonDebuggerKernelDebugger.Checked)
				monitor.parameters["KernelConnectionString"] = new Variant(textBoxKernelConnectionString.Text);

			agent.monitors.Add(monitor);
			dom.agents.Add(agent.name, agent);

			// Mutation Strategy
			MutationStrategy strat = new RandomStrategy(new Dictionary<string, Variant>());
			if (comboBoxFuzzingStrategy.Text.ToLower().IndexOf("Squencial") > -1)
				strat = new Sequencial(new Dictionary<string, string>());

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

		public void StoppedFuzzing()
		{
			buttonStartFuzzing.Enabled = true;
			buttonSaveConfiguration.Enabled = false;
			buttonStopFuzzing.Enabled = false;

			//tabControl.SelectedTab = tabPageGeneral;
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
