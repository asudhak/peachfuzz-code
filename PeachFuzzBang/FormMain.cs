
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
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using System.ServiceProcess;
using System.Linq;

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
		public Int32 IterationCount = 0;
		public Int32 FaultCount = 0;

		Thread thread = null;

		Peach.Core.Dom.Dom userSelectedDom = null;
		DataModel userSelectedDataModel = null;
		bool hasPlatformAsm = false;

		private void LoadPlatformAssembly()
		{
			try
			{
				Platform.LoadAssembly();
				hasPlatformAsm = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Console.WriteLine(ex.Message);
			}
		}

		public FormMain()
		{
			InitializeComponent();

			foreach (var strategy in ClassLoader.GetAllByAttribute<MutationStrategyAttribute>(null))
			{
				comboBoxFuzzingStrategy.Items.Add(strategy.Key.Name);
			}

			//tabControl.TabPages.Remove(tabPageGUI);
			tabControl.TabPages.Remove(tabPageFuzzing);
			//tabControl.TabPages.Remove(tabPageOutput);

			LoadPlatformAssembly();

			// Check OS and load side assembly
			Platform.OS os = Platform.GetOS();

			switch (os)
			{
				case Platform.OS.OSX:
					tabControl.TabPages.Remove(tabPageDebuggerLinux);
					tabControl.TabPages.Remove(tabPageDebuggerWin);
					tabControl.TabPages.Remove(tabPageGUI);
					richTextBoxOSX.LoadFile(Assembly.GetExecutingAssembly().GetManifestResourceStream("PeachFuzzBang.OSXDebugging.rtf"), RichTextBoxStreamType.RichText);
					break;
				case Platform.OS.Linux:
					tabControl.TabPages.Remove(tabPageDebuggerOSX);
					tabControl.TabPages.Remove(tabPageDebuggerWin);
					tabControl.TabPages.Remove(tabPageGUI);
					richTextBoxLinux.LoadFile(Assembly.GetExecutingAssembly().GetManifestResourceStream("PeachFuzzBang.LinuxDebugging.rtf"), RichTextBoxStreamType.RichText);
					break;
				case Platform.OS.Windows:
					{
						comboBoxAttachToServiceServices.Items.Clear();
						foreach (ServiceController srv in ServiceController.GetServices())
						{
							comboBoxAttachToServiceServices.Items.Add(srv.ServiceName);
						}

						textBoxAttachToProcessProcessName.Items.Clear();
						foreach (Process proc in Process.GetProcesses())
						{
							textBoxAttachToProcessProcessName.Items.Add(proc.ProcessName);
							proc.Close();
						}

						tabControl.TabPages.Remove(tabPageDebuggerOSX);
						tabControl.TabPages.Remove(tabPageDebuggerLinux);

						if (!Environment.Is64BitProcess && Environment.Is64BitOperatingSystem)
							MessageBox.Show("Warning: The 64bit version of Peach 3 must be used on 64 bit Operating Systems.", "Warning");

						string windbg = null;
						Type t = ClassLoader.FindTypeByAttribute<MonitorAttribute>((x, y) => y.Name == "WindowsDebugger");
						if (t != null)
							windbg = t.InvokeMember("FindWinDbg", BindingFlags.InvokeMethod, null, null, null) as string;

						if (windbg != null)
							textBoxDebuggerPath.Text = windbg;
						else
							textBoxDebuggerPath.Text = "Error, could not locate windbg!";
					}
					break;
			}

			if (os != Platform.OS.Windows)
			{
				// Update default settings to include full path to PeachFuzzBang
				// When double clicking the app to run it, the current working
				// directory is $HOME
				string cwd = Environment.CurrentDirectory + Path.DirectorySeparatorChar;
				string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar;

				if (path.StartsWith(cwd))
				{
					path = path.Substring(cwd.Length);
				}

				if (!string.IsNullOrEmpty(path))
				{
					textBoxFuzzedFile.Text = Path.Combine(path, textBoxFuzzedFile.Text);
					textBoxTemplateFiles.Text = Path.Combine(path, textBoxTemplateFiles.Text);
					textBoxLinuxArguments.Text = Path.Combine(path, textBoxLinuxArguments.Text);
					textBoxOSXArguments.Text = Path.Combine(path, textBoxOSXArguments.Text);
				}
			}

			buttonStartFuzzing.Enabled = true;
			buttonSaveConfiguration.Enabled = false;
			buttonStopFuzzing.Enabled = false;

			comboBoxPitDataModel.SelectedIndexChanged += new EventHandler(comboBoxPitDataModel_SelectedIndexChanged);

			richTextBoxIntroduction.LoadFile(Assembly.GetExecutingAssembly().GetManifestResourceStream("PeachFuzzBang.Introduction.rtf"), RichTextBoxStreamType.RichText);
		}

		void comboBoxPitDataModel_SelectedIndexChanged(object sender, EventArgs e)
		{
			userSelectedDataModel = userSelectedDom.dataModels[comboBoxPitDataModel.Text];
		}

		private void buttonStartFuzzing_Click(object sender, EventArgs e)
		{
			try
			{
				tabControl.SelectedTab = tabPageOutput;
				buttonStartFuzzing.Enabled = false;
				buttonSaveConfiguration.Enabled = false;
				buttonStopFuzzing.Enabled = true;

				IterationCount = 1;
				FaultCount = 0;
				textBoxIterationCount.Text = IterationCount.ToString();
				textBoxFaultCount.Text = FaultCount.ToString();
				textBoxOutput.Text = "";

				Dom dom = new Dom();
				DataModel dataModel = null;

				// Data Set
				Data fileData = new Data();
				if (Directory.Exists(textBoxTemplateFiles.Text))
				{
					List<string> files = new List<string>();
					foreach (string fileName in Directory.GetFiles(textBoxTemplateFiles.Text))
						files.Add(fileName);

					fileData.DataType = DataType.Files;
					fileData.Files = files;
				}
				else if (File.Exists(textBoxTemplateFiles.Text))
				{
					fileData.DataType = DataType.File;
					fileData.FileName = textBoxTemplateFiles.Text;
				}
				else
				{
					MessageBox.Show("Error, Unable to locate file/folder called \"" + textBoxTemplateFiles.Text + "\".");
					return;
				}

				// DataModel
				if (userSelectedDataModel != null)
				{
					dataModel = userSelectedDataModel.Clone("TheDataModel") as DataModel;
					dataModel.dom = dom;

					dom.dataModels.Add(dataModel.name, dataModel);
				}
				else
				{
					dataModel = new DataModel("TheDataModel");
					dataModel.Add(new Blob());
					dom.dataModels.Add(dataModel.name, dataModel);
				}

				// Publisher
				Dictionary<string, Variant> args = new Dictionary<string, Variant>();
				args["FileName"] = new Variant(textBoxFuzzedFile.Text);
				Peach.Core.Publishers.FilePublisher file = new Peach.Core.Publishers.FilePublisher(args);

				// StateModel
				StateModel stateModel = new StateModel();
				stateModel.name = "TheStateModel";

				State state = new State();
				state.name = "TheState";
				state.parent = stateModel;

				Peach.Core.Dom.Action actionOutput = new Peach.Core.Dom.Action();
				actionOutput.type = ActionType.Output;
				actionOutput.dataModel = dataModel;
				actionOutput.dataSet = new Peach.Core.Dom.DataSet();
				actionOutput.dataSet.Datas.Add(fileData);
				actionOutput.parent = state;

				Peach.Core.Dom.Action actionClose = new Peach.Core.Dom.Action();
				actionClose.type = ActionType.Close;
				actionClose.parent = state;

				Peach.Core.Dom.Action actionCall = new Peach.Core.Dom.Action();
				actionCall.type = ActionType.Call;
				actionCall.publisher = "Peach.Agent";
				actionCall.method = "ScoobySnacks";
				actionCall.parent = state;

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

				switch (Platform.GetOS())
				{
					case Platform.OS.OSX:
						if (radioButtonOSXCrashReporter.Checked)
						{
							monitor.cls = "CrashReporter";
							agent.monitors.Add(monitor);

							monitor = new Peach.Core.Dom.Monitor();
							monitor.cls = "Process";
							monitor.parameters["StartOnCall"] = new Variant("ScoobySnacks");

							if (this.checkBoxOSXCpuKill.Checked)
								monitor.parameters["NoCpuKill"] = new Variant("false");
							else
								monitor.parameters["NoCpuKill"] = new Variant("true");

							monitor.parameters["Executable"] = new Variant(this.textBoxOSXExecutable.Text);
							monitor.parameters["Arguments"] = new Variant(this.textBoxOSXArguments.Text);
						}
						else // Crash Wrangler
						{
							monitor.cls = "CrashWrangler";
							monitor.parameters["StartOnCall"] = new Variant("ScoobySnacks");

							if (this.checkBoxOSXCpuKill.Checked)
								monitor.parameters["NoCpuKill"] = new Variant("false");
							else
								monitor.parameters["NoCpuKill"] = new Variant("true");

							monitor.parameters["Command"] = new Variant(this.textBoxOSXExecutable.Text);
							monitor.parameters["Arguments"] = new Variant(this.textBoxOSXArguments.Text);
							monitor.parameters["CrashWrangler"] = new Variant(this.textBoxOSXCrashWrangler.Text);
						}
						break;

					case Platform.OS.Linux:	// Linux
						monitor.cls = "Process";
						monitor.parameters["StartOnCall"] = new Variant("ScoobySnacks");
						monitor.parameters["Executable"] = new Variant(textBoxLinuxExecutable.Text);
						monitor.parameters["Arguments"] = new Variant(textBoxLinuxArguments.Text);
						monitor.parameters["NoCpuKill"] = new Variant("false");
						break;

					case Platform.OS.Windows:
						monitor.cls = "WindowsDebugger";
						monitor.parameters["StartOnCall"] = new Variant("ScoobySnacks");
						monitor.parameters["WinDbgPath"] = new Variant(textBoxDebuggerPath.Text);

						if (!checkBoxCpuKill.Checked)
							monitor.parameters["NoCpuKill"] = new Variant("true");

						if (radioButtonDebuggerStartProcess.Checked)
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
						break;
				}

				agent.monitors.Add(monitor);
				dom.agents.Add(agent.name, agent);

				// Send WM_CLOSE messages?
				if (checkBoxEnableWmClose.Checked)
				{
					string windowNames = "";
					if (!string.IsNullOrWhiteSpace(textBoxWindowTitle1.Text))
					{
						if (windowNames.Length > 0)
							windowNames += ";";
						windowNames += textBoxWindowTitle1.Text;
					}
					if (!string.IsNullOrWhiteSpace(textBoxWindowTitle2.Text))
					{
						if (windowNames.Length > 0)
							windowNames += ";";
						windowNames += textBoxWindowTitle2.Text;
					}
					if (!string.IsNullOrWhiteSpace(textBoxWindowTitle3.Text))
					{
						if (windowNames.Length > 0)
							windowNames += ";";
						windowNames += textBoxWindowTitle3.Text;
					}
					if (!string.IsNullOrWhiteSpace(textBoxWindowTitle4.Text))
					{
						if (windowNames.Length > 0)
							windowNames += ";";
						windowNames += textBoxWindowTitle4.Text;
					}

					monitor = new Peach.Core.Dom.Monitor();
					monitor.cls = "PopupWatcher";
					monitor.parameters["WindowNames"] = new Variant(windowNames);

					agent.monitors.Add(monitor);
				}

				// Mutation Strategy
				MutationStrategy strat = new RandomStrategy(new Dictionary<string, Variant>());
				if (comboBoxFuzzingStrategy.Text.ToLower().IndexOf("Squencial") > -1)
					strat = new Sequential(new Dictionary<string, Variant>());

				// Test
				Test test = new Test();
				test.name = "Default";
				test.stateModel = stateModel;
				test.agents.Add(agent.name, agent);
				test.publishers.Add("FileWriter", file);
				test.strategy = strat;
				stateModel.parent = test;

				dom.tests.Add(test.name, test);

				if (logger == null)
				{
					Dictionary<string, Variant> loggerArgs = new Dictionary<string, Variant>();
					loggerArgs["Path"] = new Variant(textBoxLogPath.Text);
					logger = new Peach.Core.Loggers.FileLogger(loggerArgs);
				}

				test.loggers.Add(logger);

				// START FUZZING!!!!!
				thread = new Thread(new ParameterizedThreadStart(Run));
				thread.Start(dom);
			}
			catch(Exception ex)
			{
				MessageBox.Show(ex.ToString());
				throw;
			}
		}

		Logger logger = null;
		ConsoleWatcher consoleWatcher = null;

		public void Run(object obj)
		{
			try
			{
				if (consoleWatcher == null)
					consoleWatcher = new ConsoleWatcher(this);

				Dom dom = obj as Dom;
				RunConfiguration config = new RunConfiguration();
				Engine e = new Engine(consoleWatcher);

				config.pitFile = "PeachFuzzBang";

				if (!string.IsNullOrEmpty(textBoxIterations.Text))
				{
					try
					{
						int iter = int.Parse(textBoxIterations.Text);
						if (iter > 0)
						{
							config.range = true;
							config.rangeStart = 0;
							config.rangeStop = (uint)iter;
						}
					}
					catch
					{
					}
				}

				e.TestFinished += new Engine.TestFinishedEventHandler(Engine_RunFinished);
				e.TestError += new Engine.TestErrorEventHandler(Engine_RunError);

				e.startFuzzing(dom, config);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
				throw;
			}
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

			buttonPitFileNameLoad_Click(null, null);
		}

		private void buttonPitFileNameLoad_Click(object sender, EventArgs e)
		{
			if (!System.IO.File.Exists(textBoxPitFileName.Text))
			{
				MessageBox.Show("Error, Pit file does not exist.");
				return;
			}

			var currentCursor = this.Cursor;
			this.Cursor = Cursors.WaitCursor;

			try
			{

				var pitParser = new Peach.Core.Analyzers.PitParser();
				userSelectedDom = pitParser.asParser(null, textBoxPitFileName.Text);

				comboBoxPitDataModel.Items.Clear();
				foreach (var model in userSelectedDom.dataModels.Keys)
				{
					comboBoxPitDataModel.Items.Add(model);
				}

				if (userSelectedDom.dataModels.Count > 0)
					comboBoxPitDataModel.SelectedIndex = 0;

				label4.Enabled = true;
				comboBoxPitDataModel.Enabled = true;
			}
			catch (PeachException ex)
			{
				MessageBox.Show(ex.Message);
			}
			finally
			{
				this.Cursor = currentCursor;
			}
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

		private void FormMain_Load(object sender, EventArgs e)
		{
			if (!hasPlatformAsm)
				Close();
		}
	}
}
