namespace PeachFuzzBang
{
	partial class FormMain
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
			this.tabControl = new System.Windows.Forms.TabControl();
			this.tabPageGeneral = new System.Windows.Forms.TabPage();
			this.buttonLogPathBrowse = new System.Windows.Forms.Button();
			this.textBoxLogPath = new System.Windows.Forms.TextBox();
			this.label17 = new System.Windows.Forms.Label();
			this.groupBox6 = new System.Windows.Forms.GroupBox();
			this.label16 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.buttonPitFileNameLoad = new System.Windows.Forms.Button();
			this.comboBoxPitDataModel = new System.Windows.Forms.ComboBox();
			this.buttonPitFileNameBrowse = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.textBoxPitFileName = new System.Windows.Forms.TextBox();
			this.label13 = new System.Windows.Forms.Label();
			this.comboBoxFuzzingStrategy = new System.Windows.Forms.ComboBox();
			this.textBoxIterations = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.buttonBrowseFuzzedFile = new System.Windows.Forms.Button();
			this.buttonBrowseTemplates = new System.Windows.Forms.Button();
			this.textBoxFuzzedFile = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.textBoxTemplateFiles = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.tabPageDebugger = new System.Windows.Forms.TabPage();
			this.buttonDebuggerPathBrowse = new System.Windows.Forms.Button();
			this.textBoxDebuggerPath = new System.Windows.Forms.TextBox();
			this.label15 = new System.Windows.Forms.Label();
			this.groupBox5 = new System.Windows.Forms.GroupBox();
			this.buttonDebuggerCommandBrowse = new System.Windows.Forms.Button();
			this.textBoxDebuggerCommandLine = new System.Windows.Forms.TextBox();
			this.label14 = new System.Windows.Forms.Label();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.textBoxKernelConnectionString = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.comboBoxAttachToServiceServices = new System.Windows.Forms.ComboBox();
			this.label5 = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.textBoxAttachToProcessProcessName = new System.Windows.Forms.TextBox();
			this.textBoxAttachToProcessPID = new System.Windows.Forms.TextBox();
			this.radioButtonAttachToProcessProcessName = new System.Windows.Forms.RadioButton();
			this.radioButtonAttachToProcessPID = new System.Windows.Forms.RadioButton();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.radioButtonDebuggerAttachToProcess = new System.Windows.Forms.RadioButton();
			this.radioButtonDebuggerKernelDebugger = new System.Windows.Forms.RadioButton();
			this.radioButtonDebuggerAttachToService = new System.Windows.Forms.RadioButton();
			this.radioButtonDebuggerStartProcess = new System.Windows.Forms.RadioButton();
			this.tabPageGUI = new System.Windows.Forms.TabPage();
			this.tabPageFuzzing = new System.Windows.Forms.TabPage();
			this.textBox13 = new System.Windows.Forms.TextBox();
			this.label12 = new System.Windows.Forms.Label();
			this.textBox12 = new System.Windows.Forms.TextBox();
			this.textBox11 = new System.Windows.Forms.TextBox();
			this.textBox10 = new System.Windows.Forms.TextBox();
			this.textBox9 = new System.Windows.Forms.TextBox();
			this.label11 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.tabPageOutput = new System.Windows.Forms.TabPage();
			this.textBoxIterationCount = new System.Windows.Forms.TextBox();
			this.label19 = new System.Windows.Forms.Label();
			this.textBoxFaultCount = new System.Windows.Forms.TextBox();
			this.label18 = new System.Windows.Forms.Label();
			this.textBoxOutput = new System.Windows.Forms.TextBox();
			this.progressBarOuputFuzzing = new System.Windows.Forms.ProgressBar();
			this.tabPageAbout = new System.Windows.Forms.TabPage();
			this.pictureBox2 = new System.Windows.Forms.PictureBox();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.buttonStopFuzzing = new System.Windows.Forms.Button();
			this.buttonSaveConfiguration = new System.Windows.Forms.Button();
			this.buttonStartFuzzing = new System.Windows.Forms.Button();
			this.tabControl.SuspendLayout();
			this.tabPageGeneral.SuspendLayout();
			this.groupBox6.SuspendLayout();
			this.tabPageDebugger.SuspendLayout();
			this.groupBox5.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.tabPageFuzzing.SuspendLayout();
			this.tabPageOutput.SuspendLayout();
			this.tabPageAbout.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl
			// 
			this.tabControl.Controls.Add(this.tabPageGeneral);
			this.tabControl.Controls.Add(this.tabPageDebugger);
			this.tabControl.Controls.Add(this.tabPageGUI);
			this.tabControl.Controls.Add(this.tabPageFuzzing);
			this.tabControl.Controls.Add(this.tabPageOutput);
			this.tabControl.Controls.Add(this.tabPageAbout);
			this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl.Location = new System.Drawing.Point(0, 0);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = new System.Drawing.Size(553, 433);
			this.tabControl.TabIndex = 0;
			// 
			// tabPageGeneral
			// 
			this.tabPageGeneral.Controls.Add(this.buttonLogPathBrowse);
			this.tabPageGeneral.Controls.Add(this.textBoxLogPath);
			this.tabPageGeneral.Controls.Add(this.label17);
			this.tabPageGeneral.Controls.Add(this.groupBox6);
			this.tabPageGeneral.Controls.Add(this.label13);
			this.tabPageGeneral.Controls.Add(this.comboBoxFuzzingStrategy);
			this.tabPageGeneral.Controls.Add(this.textBoxIterations);
			this.tabPageGeneral.Controls.Add(this.label7);
			this.tabPageGeneral.Controls.Add(this.buttonBrowseFuzzedFile);
			this.tabPageGeneral.Controls.Add(this.buttonBrowseTemplates);
			this.tabPageGeneral.Controls.Add(this.textBoxFuzzedFile);
			this.tabPageGeneral.Controls.Add(this.label2);
			this.tabPageGeneral.Controls.Add(this.textBoxTemplateFiles);
			this.tabPageGeneral.Controls.Add(this.label1);
			this.tabPageGeneral.Location = new System.Drawing.Point(4, 22);
			this.tabPageGeneral.Name = "tabPageGeneral";
			this.tabPageGeneral.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageGeneral.Size = new System.Drawing.Size(545, 407);
			this.tabPageGeneral.TabIndex = 0;
			this.tabPageGeneral.Text = "General";
			this.tabPageGeneral.UseVisualStyleBackColor = true;
			// 
			// buttonLogPathBrowse
			// 
			this.buttonLogPathBrowse.Location = new System.Drawing.Point(414, 106);
			this.buttonLogPathBrowse.Name = "buttonLogPathBrowse";
			this.buttonLogPathBrowse.Size = new System.Drawing.Size(75, 23);
			this.buttonLogPathBrowse.TabIndex = 19;
			this.buttonLogPathBrowse.Text = "Browse";
			this.buttonLogPathBrowse.UseVisualStyleBackColor = true;
			this.buttonLogPathBrowse.Click += new System.EventHandler(this.buttonLogPathBrowse_Click);
			// 
			// textBoxLogPath
			// 
			this.textBoxLogPath.Location = new System.Drawing.Point(115, 105);
			this.textBoxLogPath.Name = "textBoxLogPath";
			this.textBoxLogPath.Size = new System.Drawing.Size(293, 20);
			this.textBoxLogPath.TabIndex = 18;
			this.textBoxLogPath.Text = "Logs";
			// 
			// label17
			// 
			this.label17.AutoSize = true;
			this.label17.Location = new System.Drawing.Point(56, 108);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(53, 13);
			this.label17.TabIndex = 17;
			this.label17.Text = "Log Path:";
			// 
			// groupBox6
			// 
			this.groupBox6.Controls.Add(this.label16);
			this.groupBox6.Controls.Add(this.label4);
			this.groupBox6.Controls.Add(this.buttonPitFileNameLoad);
			this.groupBox6.Controls.Add(this.comboBoxPitDataModel);
			this.groupBox6.Controls.Add(this.buttonPitFileNameBrowse);
			this.groupBox6.Controls.Add(this.label3);
			this.groupBox6.Controls.Add(this.textBoxPitFileName);
			this.groupBox6.Enabled = false;
			this.groupBox6.Location = new System.Drawing.Point(8, 195);
			this.groupBox6.Name = "groupBox6";
			this.groupBox6.Size = new System.Drawing.Size(529, 178);
			this.groupBox6.TabIndex = 16;
			this.groupBox6.TabStop = false;
			this.groupBox6.Text = "Peach Pit (OPTIONAL)";
			// 
			// label16
			// 
			this.label16.AutoSize = true;
			this.label16.Location = new System.Drawing.Point(17, 25);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(270, 52);
			this.label16.TabIndex = 21;
			this.label16.Text = "Optionally an existing Peach PIT may be loaded.  After \r\nloading Peach Pit select" +
    " the Data Model to use.\r\n\r\nThe selected Data Model will be used to fuzz the targ" +
    "et.\r\n";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Enabled = false;
			this.label4.Location = new System.Drawing.Point(36, 137);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(65, 13);
			this.label4.TabIndex = 18;
			this.label4.Text = "Data Model:";
			// 
			// buttonPitFileNameLoad
			// 
			this.buttonPitFileNameLoad.Enabled = false;
			this.buttonPitFileNameLoad.Location = new System.Drawing.Point(433, 104);
			this.buttonPitFileNameLoad.Name = "buttonPitFileNameLoad";
			this.buttonPitFileNameLoad.Size = new System.Drawing.Size(75, 23);
			this.buttonPitFileNameLoad.TabIndex = 20;
			this.buttonPitFileNameLoad.Text = "Load";
			this.buttonPitFileNameLoad.UseVisualStyleBackColor = true;
			this.buttonPitFileNameLoad.Click += new System.EventHandler(this.buttonPitFileNameLoad_Click);
			// 
			// comboBoxPitDataModel
			// 
			this.comboBoxPitDataModel.Enabled = false;
			this.comboBoxPitDataModel.FormattingEnabled = true;
			this.comboBoxPitDataModel.Location = new System.Drawing.Point(107, 134);
			this.comboBoxPitDataModel.Name = "comboBoxPitDataModel";
			this.comboBoxPitDataModel.Size = new System.Drawing.Size(293, 21);
			this.comboBoxPitDataModel.TabIndex = 17;
			// 
			// buttonPitFileNameBrowse
			// 
			this.buttonPitFileNameBrowse.Enabled = false;
			this.buttonPitFileNameBrowse.Location = new System.Drawing.Point(352, 105);
			this.buttonPitFileNameBrowse.Name = "buttonPitFileNameBrowse";
			this.buttonPitFileNameBrowse.Size = new System.Drawing.Size(75, 23);
			this.buttonPitFileNameBrowse.TabIndex = 19;
			this.buttonPitFileNameBrowse.Text = "Browse";
			this.buttonPitFileNameBrowse.UseVisualStyleBackColor = true;
			this.buttonPitFileNameBrowse.Click += new System.EventHandler(this.buttonPitFileNameBrowse_Click);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Enabled = false;
			this.label3.Location = new System.Drawing.Point(60, 110);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(41, 13);
			this.label3.TabIndex = 17;
			this.label3.Text = "Pit File:";
			// 
			// textBoxPitFileName
			// 
			this.textBoxPitFileName.Enabled = false;
			this.textBoxPitFileName.Location = new System.Drawing.Point(107, 107);
			this.textBoxPitFileName.Name = "textBoxPitFileName";
			this.textBoxPitFileName.Size = new System.Drawing.Size(239, 20);
			this.textBoxPitFileName.TabIndex = 18;
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Location = new System.Drawing.Point(21, 81);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(88, 13);
			this.label13.TabIndex = 15;
			this.label13.Text = "Fuzzing Strategy:";
			// 
			// comboBoxFuzzingStrategy
			// 
			this.comboBoxFuzzingStrategy.FormattingEnabled = true;
			this.comboBoxFuzzingStrategy.Items.AddRange(new object[] {
            "Sequencial Strategy",
            "Random Strategy"});
			this.comboBoxFuzzingStrategy.Location = new System.Drawing.Point(115, 78);
			this.comboBoxFuzzingStrategy.Name = "comboBoxFuzzingStrategy";
			this.comboBoxFuzzingStrategy.Size = new System.Drawing.Size(293, 21);
			this.comboBoxFuzzingStrategy.TabIndex = 14;
			this.comboBoxFuzzingStrategy.Text = "Random Strategy";
			// 
			// textBoxIterations
			// 
			this.textBoxIterations.Location = new System.Drawing.Point(115, 131);
			this.textBoxIterations.Name = "textBoxIterations";
			this.textBoxIterations.Size = new System.Drawing.Size(100, 20);
			this.textBoxIterations.TabIndex = 13;
			this.textBoxIterations.Text = "25";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(56, 134);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(53, 13);
			this.label7.TabIndex = 12;
			this.label7.Text = "Iterations:";
			// 
			// buttonBrowseFuzzedFile
			// 
			this.buttonBrowseFuzzedFile.Location = new System.Drawing.Point(414, 53);
			this.buttonBrowseFuzzedFile.Name = "buttonBrowseFuzzedFile";
			this.buttonBrowseFuzzedFile.Size = new System.Drawing.Size(75, 23);
			this.buttonBrowseFuzzedFile.TabIndex = 8;
			this.buttonBrowseFuzzedFile.Text = "Browse";
			this.buttonBrowseFuzzedFile.UseVisualStyleBackColor = true;
			this.buttonBrowseFuzzedFile.Click += new System.EventHandler(this.buttonBrowseFuzzedFile_Click);
			// 
			// buttonBrowseTemplates
			// 
			this.buttonBrowseTemplates.Location = new System.Drawing.Point(414, 24);
			this.buttonBrowseTemplates.Name = "buttonBrowseTemplates";
			this.buttonBrowseTemplates.Size = new System.Drawing.Size(75, 23);
			this.buttonBrowseTemplates.TabIndex = 7;
			this.buttonBrowseTemplates.Text = "Browse";
			this.buttonBrowseTemplates.UseVisualStyleBackColor = true;
			this.buttonBrowseTemplates.Click += new System.EventHandler(this.buttonBrowseTemplates_Click);
			// 
			// textBoxFuzzedFile
			// 
			this.textBoxFuzzedFile.Location = new System.Drawing.Point(115, 52);
			this.textBoxFuzzedFile.Name = "textBoxFuzzedFile";
			this.textBoxFuzzedFile.Size = new System.Drawing.Size(293, 20);
			this.textBoxFuzzedFile.TabIndex = 4;
			this.textBoxFuzzedFile.Text = "fuzzed.png";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(15, 55);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(94, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "Fuzzed File Name:";
			// 
			// textBoxTemplateFiles
			// 
			this.textBoxTemplateFiles.Location = new System.Drawing.Point(115, 26);
			this.textBoxTemplateFiles.Name = "textBoxTemplateFiles";
			this.textBoxTemplateFiles.Size = new System.Drawing.Size(293, 20);
			this.textBoxTemplateFiles.TabIndex = 1;
			this.textBoxTemplateFiles.Text = "samples";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(25, 29);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(84, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Template File(s):";
			// 
			// tabPageDebugger
			// 
			this.tabPageDebugger.Controls.Add(this.buttonDebuggerPathBrowse);
			this.tabPageDebugger.Controls.Add(this.textBoxDebuggerPath);
			this.tabPageDebugger.Controls.Add(this.label15);
			this.tabPageDebugger.Controls.Add(this.groupBox5);
			this.tabPageDebugger.Controls.Add(this.groupBox4);
			this.tabPageDebugger.Controls.Add(this.groupBox3);
			this.tabPageDebugger.Controls.Add(this.groupBox2);
			this.tabPageDebugger.Controls.Add(this.groupBox1);
			this.tabPageDebugger.Location = new System.Drawing.Point(4, 22);
			this.tabPageDebugger.Name = "tabPageDebugger";
			this.tabPageDebugger.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageDebugger.Size = new System.Drawing.Size(545, 407);
			this.tabPageDebugger.TabIndex = 1;
			this.tabPageDebugger.Text = "Debugger";
			this.tabPageDebugger.UseVisualStyleBackColor = true;
			// 
			// buttonDebuggerPathBrowse
			// 
			this.buttonDebuggerPathBrowse.Location = new System.Drawing.Point(462, 58);
			this.buttonDebuggerPathBrowse.Name = "buttonDebuggerPathBrowse";
			this.buttonDebuggerPathBrowse.Size = new System.Drawing.Size(71, 23);
			this.buttonDebuggerPathBrowse.TabIndex = 5;
			this.buttonDebuggerPathBrowse.Text = "Browse";
			this.buttonDebuggerPathBrowse.UseVisualStyleBackColor = true;
			this.buttonDebuggerPathBrowse.Click += new System.EventHandler(this.buttonDebuggerPathBrowse_Click);
			// 
			// textBoxDebuggerPath
			// 
			this.textBoxDebuggerPath.Location = new System.Drawing.Point(97, 61);
			this.textBoxDebuggerPath.Name = "textBoxDebuggerPath";
			this.textBoxDebuggerPath.Size = new System.Drawing.Size(359, 20);
			this.textBoxDebuggerPath.TabIndex = 3;
			this.textBoxDebuggerPath.Text = "C:\\Program Files (x86)\\Debugging Tools for Windows (x86)";
			// 
			// label15
			// 
			this.label15.AutoSize = true;
			this.label15.Location = new System.Drawing.Point(14, 64);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(74, 13);
			this.label15.TabIndex = 2;
			this.label15.Text = "WinDbg Path:";
			// 
			// groupBox5
			// 
			this.groupBox5.Controls.Add(this.buttonDebuggerCommandBrowse);
			this.groupBox5.Controls.Add(this.textBoxDebuggerCommandLine);
			this.groupBox5.Controls.Add(this.label14);
			this.groupBox5.Location = new System.Drawing.Point(8, 87);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.Size = new System.Drawing.Size(529, 58);
			this.groupBox5.TabIndex = 4;
			this.groupBox5.TabStop = false;
			this.groupBox5.Text = "Start Process";
			// 
			// buttonDebuggerCommandBrowse
			// 
			this.buttonDebuggerCommandBrowse.Location = new System.Drawing.Point(454, 19);
			this.buttonDebuggerCommandBrowse.Name = "buttonDebuggerCommandBrowse";
			this.buttonDebuggerCommandBrowse.Size = new System.Drawing.Size(71, 23);
			this.buttonDebuggerCommandBrowse.TabIndex = 6;
			this.buttonDebuggerCommandBrowse.Text = "Browse";
			this.buttonDebuggerCommandBrowse.UseVisualStyleBackColor = true;
			this.buttonDebuggerCommandBrowse.Click += new System.EventHandler(this.buttonDebuggerCommandBrowse_Click);
			// 
			// textBoxDebuggerCommandLine
			// 
			this.textBoxDebuggerCommandLine.Location = new System.Drawing.Point(92, 22);
			this.textBoxDebuggerCommandLine.Name = "textBoxDebuggerCommandLine";
			this.textBoxDebuggerCommandLine.Size = new System.Drawing.Size(356, 20);
			this.textBoxDebuggerCommandLine.TabIndex = 1;
			this.textBoxDebuggerCommandLine.Text = "mspaint.exe fuzzed.png";
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Location = new System.Drawing.Point(6, 25);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(80, 13);
			this.label14.TabIndex = 0;
			this.label14.Text = "Command Line:";
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.textBoxKernelConnectionString);
			this.groupBox4.Controls.Add(this.label6);
			this.groupBox4.Location = new System.Drawing.Point(8, 303);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(531, 68);
			this.groupBox4.TabIndex = 3;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Kernel Debugger";
			// 
			// textBoxKernelConnectionString
			// 
			this.textBoxKernelConnectionString.Enabled = false;
			this.textBoxKernelConnectionString.Location = new System.Drawing.Point(141, 28);
			this.textBoxKernelConnectionString.Name = "textBoxKernelConnectionString";
			this.textBoxKernelConnectionString.Size = new System.Drawing.Size(384, 20);
			this.textBoxKernelConnectionString.TabIndex = 1;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(8, 31);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(127, 13);
			this.label6.TabIndex = 0;
			this.label6.Text = "Kernel Connection String:";
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.comboBoxAttachToServiceServices);
			this.groupBox3.Controls.Add(this.label5);
			this.groupBox3.Location = new System.Drawing.Point(8, 233);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(529, 64);
			this.groupBox3.TabIndex = 2;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Attach To Service";
			// 
			// comboBoxAttachToServiceServices
			// 
			this.comboBoxAttachToServiceServices.Enabled = false;
			this.comboBoxAttachToServiceServices.FormattingEnabled = true;
			this.comboBoxAttachToServiceServices.Location = new System.Drawing.Point(58, 26);
			this.comboBoxAttachToServiceServices.Name = "comboBoxAttachToServiceServices";
			this.comboBoxAttachToServiceServices.Size = new System.Drawing.Size(187, 21);
			this.comboBoxAttachToServiceServices.TabIndex = 1;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(6, 29);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(46, 13);
			this.label5.TabIndex = 0;
			this.label5.Text = "Service:";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.textBoxAttachToProcessProcessName);
			this.groupBox2.Controls.Add(this.textBoxAttachToProcessPID);
			this.groupBox2.Controls.Add(this.radioButtonAttachToProcessProcessName);
			this.groupBox2.Controls.Add(this.radioButtonAttachToProcessPID);
			this.groupBox2.Location = new System.Drawing.Point(8, 151);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(529, 76);
			this.groupBox2.TabIndex = 1;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Attach to Process";
			// 
			// textBoxAttachToProcessProcessName
			// 
			this.textBoxAttachToProcessProcessName.Enabled = false;
			this.textBoxAttachToProcessProcessName.Location = new System.Drawing.Point(110, 44);
			this.textBoxAttachToProcessProcessName.Name = "textBoxAttachToProcessProcessName";
			this.textBoxAttachToProcessProcessName.Size = new System.Drawing.Size(219, 20);
			this.textBoxAttachToProcessProcessName.TabIndex = 3;
			// 
			// textBoxAttachToProcessPID
			// 
			this.textBoxAttachToProcessPID.Enabled = false;
			this.textBoxAttachToProcessPID.Location = new System.Drawing.Point(110, 18);
			this.textBoxAttachToProcessPID.Name = "textBoxAttachToProcessPID";
			this.textBoxAttachToProcessPID.Size = new System.Drawing.Size(219, 20);
			this.textBoxAttachToProcessPID.TabIndex = 2;
			// 
			// radioButtonAttachToProcessProcessName
			// 
			this.radioButtonAttachToProcessProcessName.AutoSize = true;
			this.radioButtonAttachToProcessProcessName.Enabled = false;
			this.radioButtonAttachToProcessProcessName.Location = new System.Drawing.Point(9, 45);
			this.radioButtonAttachToProcessProcessName.Name = "radioButtonAttachToProcessProcessName";
			this.radioButtonAttachToProcessProcessName.Size = new System.Drawing.Size(94, 17);
			this.radioButtonAttachToProcessProcessName.TabIndex = 1;
			this.radioButtonAttachToProcessProcessName.TabStop = true;
			this.radioButtonAttachToProcessProcessName.Text = "Process Name";
			this.radioButtonAttachToProcessProcessName.UseVisualStyleBackColor = true;
			// 
			// radioButtonAttachToProcessPID
			// 
			this.radioButtonAttachToProcessPID.AutoSize = true;
			this.radioButtonAttachToProcessPID.Enabled = false;
			this.radioButtonAttachToProcessPID.Location = new System.Drawing.Point(9, 19);
			this.radioButtonAttachToProcessPID.Name = "radioButtonAttachToProcessPID";
			this.radioButtonAttachToProcessPID.Size = new System.Drawing.Size(43, 17);
			this.radioButtonAttachToProcessPID.TabIndex = 0;
			this.radioButtonAttachToProcessPID.TabStop = true;
			this.radioButtonAttachToProcessPID.Text = "PID";
			this.radioButtonAttachToProcessPID.UseVisualStyleBackColor = true;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.radioButtonDebuggerAttachToProcess);
			this.groupBox1.Controls.Add(this.radioButtonDebuggerKernelDebugger);
			this.groupBox1.Controls.Add(this.radioButtonDebuggerAttachToService);
			this.groupBox1.Controls.Add(this.radioButtonDebuggerStartProcess);
			this.groupBox1.Location = new System.Drawing.Point(8, 6);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(529, 49);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Debugger Type";
			// 
			// radioButtonDebuggerAttachToProcess
			// 
			this.radioButtonDebuggerAttachToProcess.AutoSize = true;
			this.radioButtonDebuggerAttachToProcess.Enabled = false;
			this.radioButtonDebuggerAttachToProcess.Location = new System.Drawing.Point(128, 19);
			this.radioButtonDebuggerAttachToProcess.Name = "radioButtonDebuggerAttachToProcess";
			this.radioButtonDebuggerAttachToProcess.Size = new System.Drawing.Size(109, 17);
			this.radioButtonDebuggerAttachToProcess.TabIndex = 3;
			this.radioButtonDebuggerAttachToProcess.Text = "Attach to Process";
			this.radioButtonDebuggerAttachToProcess.UseVisualStyleBackColor = true;
			// 
			// radioButtonDebuggerKernelDebugger
			// 
			this.radioButtonDebuggerKernelDebugger.AutoSize = true;
			this.radioButtonDebuggerKernelDebugger.Enabled = false;
			this.radioButtonDebuggerKernelDebugger.Location = new System.Drawing.Point(418, 19);
			this.radioButtonDebuggerKernelDebugger.Name = "radioButtonDebuggerKernelDebugger";
			this.radioButtonDebuggerKernelDebugger.Size = new System.Drawing.Size(105, 17);
			this.radioButtonDebuggerKernelDebugger.TabIndex = 2;
			this.radioButtonDebuggerKernelDebugger.Text = "Kernel Debugger";
			this.radioButtonDebuggerKernelDebugger.UseVisualStyleBackColor = true;
			// 
			// radioButtonDebuggerAttachToService
			// 
			this.radioButtonDebuggerAttachToService.AutoSize = true;
			this.radioButtonDebuggerAttachToService.Enabled = false;
			this.radioButtonDebuggerAttachToService.Location = new System.Drawing.Point(273, 19);
			this.radioButtonDebuggerAttachToService.Name = "radioButtonDebuggerAttachToService";
			this.radioButtonDebuggerAttachToService.Size = new System.Drawing.Size(107, 17);
			this.radioButtonDebuggerAttachToService.TabIndex = 1;
			this.radioButtonDebuggerAttachToService.Text = "Attach to Service";
			this.radioButtonDebuggerAttachToService.UseVisualStyleBackColor = true;
			// 
			// radioButtonDebuggerStartProcess
			// 
			this.radioButtonDebuggerStartProcess.AutoSize = true;
			this.radioButtonDebuggerStartProcess.Checked = true;
			this.radioButtonDebuggerStartProcess.Location = new System.Drawing.Point(6, 19);
			this.radioButtonDebuggerStartProcess.Name = "radioButtonDebuggerStartProcess";
			this.radioButtonDebuggerStartProcess.Size = new System.Drawing.Size(88, 17);
			this.radioButtonDebuggerStartProcess.TabIndex = 0;
			this.radioButtonDebuggerStartProcess.TabStop = true;
			this.radioButtonDebuggerStartProcess.Text = "Start Process";
			this.radioButtonDebuggerStartProcess.UseVisualStyleBackColor = true;
			// 
			// tabPageGUI
			// 
			this.tabPageGUI.Location = new System.Drawing.Point(4, 22);
			this.tabPageGUI.Name = "tabPageGUI";
			this.tabPageGUI.Size = new System.Drawing.Size(545, 407);
			this.tabPageGUI.TabIndex = 2;
			this.tabPageGUI.Text = "GUI";
			this.tabPageGUI.UseVisualStyleBackColor = true;
			// 
			// tabPageFuzzing
			// 
			this.tabPageFuzzing.Controls.Add(this.textBox13);
			this.tabPageFuzzing.Controls.Add(this.label12);
			this.tabPageFuzzing.Controls.Add(this.textBox12);
			this.tabPageFuzzing.Controls.Add(this.textBox11);
			this.tabPageFuzzing.Controls.Add(this.textBox10);
			this.tabPageFuzzing.Controls.Add(this.textBox9);
			this.tabPageFuzzing.Controls.Add(this.label11);
			this.tabPageFuzzing.Controls.Add(this.label10);
			this.tabPageFuzzing.Controls.Add(this.label9);
			this.tabPageFuzzing.Controls.Add(this.label8);
			this.tabPageFuzzing.Location = new System.Drawing.Point(4, 22);
			this.tabPageFuzzing.Name = "tabPageFuzzing";
			this.tabPageFuzzing.Size = new System.Drawing.Size(545, 407);
			this.tabPageFuzzing.TabIndex = 3;
			this.tabPageFuzzing.Text = "Fuzzing";
			this.tabPageFuzzing.UseVisualStyleBackColor = true;
			// 
			// textBox13
			// 
			this.textBox13.Location = new System.Drawing.Point(151, 120);
			this.textBox13.Name = "textBox13";
			this.textBox13.Size = new System.Drawing.Size(284, 20);
			this.textBox13.TabIndex = 9;
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Location = new System.Drawing.Point(96, 123);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(49, 13);
			this.label12.TabIndex = 8;
			this.label12.Text = "Strategy:";
			// 
			// textBox12
			// 
			this.textBox12.Location = new System.Drawing.Point(151, 94);
			this.textBox12.Name = "textBox12";
			this.textBox12.Size = new System.Drawing.Size(284, 20);
			this.textBox12.TabIndex = 7;
			// 
			// textBox11
			// 
			this.textBox11.Location = new System.Drawing.Point(151, 68);
			this.textBox11.Name = "textBox11";
			this.textBox11.Size = new System.Drawing.Size(284, 20);
			this.textBox11.TabIndex = 6;
			// 
			// textBox10
			// 
			this.textBox10.Location = new System.Drawing.Point(151, 42);
			this.textBox10.Name = "textBox10";
			this.textBox10.Size = new System.Drawing.Size(284, 20);
			this.textBox10.TabIndex = 5;
			// 
			// textBox9
			// 
			this.textBox9.Location = new System.Drawing.Point(151, 18);
			this.textBox9.Name = "textBox9";
			this.textBox9.Size = new System.Drawing.Size(284, 20);
			this.textBox9.TabIndex = 4;
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(8, 97);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(137, 13);
			this.label11.TabIndex = 3;
			this.label11.Text = "Estimated Completion Time:";
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(89, 71);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(56, 13);
			this.label10.TabIndex = 2;
			this.label10.Text = "Run Time:";
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(65, 45);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(80, 13);
			this.label9.TabIndex = 1;
			this.label9.Text = "Total Iterations:";
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(60, 21);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(85, 13);
			this.label8.TabIndex = 0;
			this.label8.Text = "Current Iteration:";
			// 
			// tabPageOutput
			// 
			this.tabPageOutput.Controls.Add(this.textBoxIterationCount);
			this.tabPageOutput.Controls.Add(this.label19);
			this.tabPageOutput.Controls.Add(this.textBoxFaultCount);
			this.tabPageOutput.Controls.Add(this.label18);
			this.tabPageOutput.Controls.Add(this.textBoxOutput);
			this.tabPageOutput.Controls.Add(this.progressBarOuputFuzzing);
			this.tabPageOutput.Location = new System.Drawing.Point(4, 22);
			this.tabPageOutput.Name = "tabPageOutput";
			this.tabPageOutput.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageOutput.Size = new System.Drawing.Size(545, 407);
			this.tabPageOutput.TabIndex = 4;
			this.tabPageOutput.Text = "Output";
			this.tabPageOutput.UseVisualStyleBackColor = true;
			// 
			// textBoxIterationCount
			// 
			this.textBoxIterationCount.Location = new System.Drawing.Point(92, 356);
			this.textBoxIterationCount.Name = "textBoxIterationCount";
			this.textBoxIterationCount.ReadOnly = true;
			this.textBoxIterationCount.Size = new System.Drawing.Size(100, 20);
			this.textBoxIterationCount.TabIndex = 5;
			this.textBoxIterationCount.Text = "0";
			// 
			// label19
			// 
			this.label19.AutoSize = true;
			this.label19.Location = new System.Drawing.Point(7, 359);
			this.label19.Name = "label19";
			this.label19.Size = new System.Drawing.Size(79, 13);
			this.label19.TabIndex = 4;
			this.label19.Text = "Iteration Count:";
			// 
			// textBoxFaultCount
			// 
			this.textBoxFaultCount.Location = new System.Drawing.Point(281, 356);
			this.textBoxFaultCount.Name = "textBoxFaultCount";
			this.textBoxFaultCount.ReadOnly = true;
			this.textBoxFaultCount.Size = new System.Drawing.Size(100, 20);
			this.textBoxFaultCount.TabIndex = 3;
			this.textBoxFaultCount.Text = "0";
			// 
			// label18
			// 
			this.label18.AutoSize = true;
			this.label18.Location = new System.Drawing.Point(211, 359);
			this.label18.Name = "label18";
			this.label18.Size = new System.Drawing.Size(64, 13);
			this.label18.TabIndex = 2;
			this.label18.Text = "Fault Count:";
			// 
			// textBoxOutput
			// 
			this.textBoxOutput.Location = new System.Drawing.Point(8, 35);
			this.textBoxOutput.Multiline = true;
			this.textBoxOutput.Name = "textBoxOutput";
			this.textBoxOutput.ReadOnly = true;
			this.textBoxOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textBoxOutput.Size = new System.Drawing.Size(529, 315);
			this.textBoxOutput.TabIndex = 1;
			// 
			// progressBarOuputFuzzing
			// 
			this.progressBarOuputFuzzing.Location = new System.Drawing.Point(8, 6);
			this.progressBarOuputFuzzing.Name = "progressBarOuputFuzzing";
			this.progressBarOuputFuzzing.Size = new System.Drawing.Size(529, 23);
			this.progressBarOuputFuzzing.TabIndex = 0;
			// 
			// tabPageAbout
			// 
			this.tabPageAbout.Controls.Add(this.pictureBox2);
			this.tabPageAbout.Controls.Add(this.pictureBox1);
			this.tabPageAbout.Location = new System.Drawing.Point(4, 22);
			this.tabPageAbout.Name = "tabPageAbout";
			this.tabPageAbout.Size = new System.Drawing.Size(545, 407);
			this.tabPageAbout.TabIndex = 5;
			this.tabPageAbout.Text = "About";
			this.tabPageAbout.UseVisualStyleBackColor = true;
			// 
			// pictureBox2
			// 
			this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
			this.pictureBox2.Location = new System.Drawing.Point(9, 144);
			this.pictureBox2.Name = "pictureBox2";
			this.pictureBox2.Size = new System.Drawing.Size(528, 228);
			this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.pictureBox2.TabIndex = 1;
			this.pictureBox2.TabStop = false;
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.InitialImage = ((System.Drawing.Image)(resources.GetObject("pictureBox1.InitialImage")));
			this.pictureBox1.Location = new System.Drawing.Point(8, 3);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(529, 135);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.buttonStopFuzzing);
			this.panel1.Controls.Add(this.buttonSaveConfiguration);
			this.panel1.Controls.Add(this.buttonStartFuzzing);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(0, 401);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(553, 32);
			this.panel1.TabIndex = 1;
			// 
			// buttonStopFuzzing
			// 
			this.buttonStopFuzzing.Location = new System.Drawing.Point(352, 3);
			this.buttonStopFuzzing.Name = "buttonStopFuzzing";
			this.buttonStopFuzzing.Size = new System.Drawing.Size(87, 23);
			this.buttonStopFuzzing.TabIndex = 2;
			this.buttonStopFuzzing.Text = "Stop Fuzzing";
			this.buttonStopFuzzing.UseVisualStyleBackColor = true;
			this.buttonStopFuzzing.Click += new System.EventHandler(this.buttonStopFuzzing_Click);
			// 
			// buttonSaveConfiguration
			// 
			this.buttonSaveConfiguration.Location = new System.Drawing.Point(237, 3);
			this.buttonSaveConfiguration.Name = "buttonSaveConfiguration";
			this.buttonSaveConfiguration.Size = new System.Drawing.Size(109, 23);
			this.buttonSaveConfiguration.TabIndex = 1;
			this.buttonSaveConfiguration.Text = "Save Configuration";
			this.buttonSaveConfiguration.UseVisualStyleBackColor = true;
			// 
			// buttonStartFuzzing
			// 
			this.buttonStartFuzzing.Location = new System.Drawing.Point(129, 3);
			this.buttonStartFuzzing.Name = "buttonStartFuzzing";
			this.buttonStartFuzzing.Size = new System.Drawing.Size(102, 23);
			this.buttonStartFuzzing.TabIndex = 0;
			this.buttonStartFuzzing.Text = "Start Fuzzing";
			this.buttonStartFuzzing.UseVisualStyleBackColor = true;
			this.buttonStartFuzzing.Click += new System.EventHandler(this.buttonStartFuzzing_Click);
			// 
			// FormMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(553, 433);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.tabControl);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "FormMain";
			this.Text = "Peach Fuzz Bang";
			this.tabControl.ResumeLayout(false);
			this.tabPageGeneral.ResumeLayout(false);
			this.tabPageGeneral.PerformLayout();
			this.groupBox6.ResumeLayout(false);
			this.groupBox6.PerformLayout();
			this.tabPageDebugger.ResumeLayout(false);
			this.tabPageDebugger.PerformLayout();
			this.groupBox5.ResumeLayout(false);
			this.groupBox5.PerformLayout();
			this.groupBox4.ResumeLayout(false);
			this.groupBox4.PerformLayout();
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.tabPageFuzzing.ResumeLayout(false);
			this.tabPageFuzzing.PerformLayout();
			this.tabPageOutput.ResumeLayout(false);
			this.tabPageOutput.PerformLayout();
			this.tabPageAbout.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabControl tabControl;
		private System.Windows.Forms.TabPage tabPageGeneral;
		private System.Windows.Forms.Button buttonBrowseFuzzedFile;
		private System.Windows.Forms.Button buttonBrowseTemplates;
		private System.Windows.Forms.TextBox textBoxFuzzedFile;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox textBoxTemplateFiles;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TabPage tabPageDebugger;
		private System.Windows.Forms.TabPage tabPageGUI;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.TextBox textBoxAttachToProcessProcessName;
		private System.Windows.Forms.TextBox textBoxAttachToProcessPID;
		private System.Windows.Forms.RadioButton radioButtonAttachToProcessProcessName;
		private System.Windows.Forms.RadioButton radioButtonAttachToProcessPID;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.RadioButton radioButtonDebuggerAttachToProcess;
		private System.Windows.Forms.RadioButton radioButtonDebuggerKernelDebugger;
		private System.Windows.Forms.RadioButton radioButtonDebuggerAttachToService;
		private System.Windows.Forms.RadioButton radioButtonDebuggerStartProcess;
		private System.Windows.Forms.TextBox textBoxKernelConnectionString;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.ComboBox comboBoxAttachToServiceServices;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox textBoxIterations;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.TabPage tabPageFuzzing;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button buttonStopFuzzing;
		private System.Windows.Forms.Button buttonSaveConfiguration;
		private System.Windows.Forms.Button buttonStartFuzzing;
		private System.Windows.Forms.TextBox textBox13;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.TextBox textBox12;
		private System.Windows.Forms.TextBox textBox11;
		private System.Windows.Forms.TextBox textBox10;
		private System.Windows.Forms.TextBox textBox9;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.ComboBox comboBoxFuzzingStrategy;
		private System.Windows.Forms.GroupBox groupBox5;
		private System.Windows.Forms.TextBox textBoxDebuggerCommandLine;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.TabPage tabPageOutput;
		public System.Windows.Forms.TextBox textBoxOutput;
		public System.Windows.Forms.ProgressBar progressBarOuputFuzzing;
		private System.Windows.Forms.Button buttonDebuggerPathBrowse;
		private System.Windows.Forms.TextBox textBoxDebuggerPath;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.Button buttonDebuggerCommandBrowse;
		private System.Windows.Forms.GroupBox groupBox6;
		private System.Windows.Forms.Button buttonPitFileNameBrowse;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox textBoxPitFileName;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button buttonPitFileNameLoad;
		private System.Windows.Forms.ComboBox comboBoxPitDataModel;
		private System.Windows.Forms.TabPage tabPageAbout;
		private System.Windows.Forms.PictureBox pictureBox2;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Button buttonLogPathBrowse;
		private System.Windows.Forms.TextBox textBoxLogPath;
		private System.Windows.Forms.Label label17;
		private System.Windows.Forms.Label label19;
		private System.Windows.Forms.Label label18;
		public System.Windows.Forms.TextBox textBoxIterationCount;
		public System.Windows.Forms.TextBox textBoxFaultCount;
	}
}

