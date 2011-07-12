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
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.label13 = new System.Windows.Forms.Label();
			this.comboBoxFuzzingStrategy = new System.Windows.Forms.ComboBox();
			this.textBoxIterations = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.button3 = new System.Windows.Forms.Button();
			this.label4 = new System.Windows.Forms.Label();
			this.button2 = new System.Windows.Forms.Button();
			this.button1 = new System.Windows.Forms.Button();
			this.textBoxCommandLine = new System.Windows.Forms.TextBox();
			this.textBoxExecutable = new System.Windows.Forms.TextBox();
			this.textBoxFuzzedFile = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.textBoxTemplateFiles = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.groupBox5 = new System.Windows.Forms.GroupBox();
			this.textBoxDebuggerCommandLine = new System.Windows.Forms.TextBox();
			this.label14 = new System.Windows.Forms.Label();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.textBox7 = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.comboBox1 = new System.Windows.Forms.ComboBox();
			this.label5 = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.textBox6 = new System.Windows.Forms.TextBox();
			this.textBox5 = new System.Windows.Forms.TextBox();
			this.radioButton6 = new System.Windows.Forms.RadioButton();
			this.radioButton5 = new System.Windows.Forms.RadioButton();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.radioButton4 = new System.Windows.Forms.RadioButton();
			this.radioButton3 = new System.Windows.Forms.RadioButton();
			this.radioButton2 = new System.Windows.Forms.RadioButton();
			this.radioButtonDebuggerStartProcess = new System.Windows.Forms.RadioButton();
			this.tabPage3 = new System.Windows.Forms.TabPage();
			this.tabPage4 = new System.Windows.Forms.TabPage();
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
			this.textBoxOutput = new System.Windows.Forms.TextBox();
			this.progressBarOuputFuzzing = new System.Windows.Forms.ProgressBar();
			this.panel1 = new System.Windows.Forms.Panel();
			this.button6 = new System.Windows.Forms.Button();
			this.button5 = new System.Windows.Forms.Button();
			this.button4 = new System.Windows.Forms.Button();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.groupBox5.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.tabPage4.SuspendLayout();
			this.tabPageOutput.SuspendLayout();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Controls.Add(this.tabPage3);
			this.tabControl1.Controls.Add(this.tabPage4);
			this.tabControl1.Controls.Add(this.tabPageOutput);
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.Location = new System.Drawing.Point(0, 0);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(553, 420);
			this.tabControl1.TabIndex = 0;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.label13);
			this.tabPage1.Controls.Add(this.comboBoxFuzzingStrategy);
			this.tabPage1.Controls.Add(this.textBoxIterations);
			this.tabPage1.Controls.Add(this.label7);
			this.tabPage1.Controls.Add(this.button3);
			this.tabPage1.Controls.Add(this.label4);
			this.tabPage1.Controls.Add(this.button2);
			this.tabPage1.Controls.Add(this.button1);
			this.tabPage1.Controls.Add(this.textBoxCommandLine);
			this.tabPage1.Controls.Add(this.textBoxExecutable);
			this.tabPage1.Controls.Add(this.textBoxFuzzedFile);
			this.tabPage1.Controls.Add(this.label3);
			this.tabPage1.Controls.Add(this.label2);
			this.tabPage1.Controls.Add(this.textBoxTemplateFiles);
			this.tabPage1.Controls.Add(this.label1);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(545, 394);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "General";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Location = new System.Drawing.Point(21, 133);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(88, 13);
			this.label13.TabIndex = 15;
			this.label13.Text = "Fuzzing Strategy:";
			// 
			// comboBoxFuzzingStrategy
			// 
			this.comboBoxFuzzingStrategy.FormattingEnabled = true;
			this.comboBoxFuzzingStrategy.Location = new System.Drawing.Point(115, 130);
			this.comboBoxFuzzingStrategy.Name = "comboBoxFuzzingStrategy";
			this.comboBoxFuzzingStrategy.Size = new System.Drawing.Size(293, 21);
			this.comboBoxFuzzingStrategy.TabIndex = 14;
			// 
			// textBoxIterations
			// 
			this.textBoxIterations.Location = new System.Drawing.Point(115, 188);
			this.textBoxIterations.Name = "textBoxIterations";
			this.textBoxIterations.Size = new System.Drawing.Size(293, 20);
			this.textBoxIterations.TabIndex = 13;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(56, 191);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(53, 13);
			this.label7.TabIndex = 12;
			this.label7.Text = "Iterations:";
			// 
			// button3
			// 
			this.button3.Location = new System.Drawing.Point(414, 82);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(75, 23);
			this.button3.TabIndex = 10;
			this.button3.Text = "Browse";
			this.button3.UseVisualStyleBackColor = true;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(12, 81);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(97, 13);
			this.label4.TabIndex = 9;
			this.label4.Text = "Target Executable:";
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(414, 53);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(75, 23);
			this.button2.TabIndex = 8;
			this.button2.Text = "Browse";
			this.button2.UseVisualStyleBackColor = true;
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(414, 24);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 7;
			this.button1.Text = "Browse";
			this.button1.UseVisualStyleBackColor = true;
			// 
			// textBoxCommandLine
			// 
			this.textBoxCommandLine.Location = new System.Drawing.Point(115, 104);
			this.textBoxCommandLine.Name = "textBoxCommandLine";
			this.textBoxCommandLine.Size = new System.Drawing.Size(293, 20);
			this.textBoxCommandLine.TabIndex = 6;
			this.textBoxCommandLine.Text = "fuzzed.txt";
			// 
			// textBoxExecutable
			// 
			this.textBoxExecutable.Location = new System.Drawing.Point(115, 78);
			this.textBoxExecutable.Name = "textBoxExecutable";
			this.textBoxExecutable.Size = new System.Drawing.Size(293, 20);
			this.textBoxExecutable.TabIndex = 5;
			this.textBoxExecutable.Text = "notepad.exe";
			// 
			// textBoxFuzzedFile
			// 
			this.textBoxFuzzedFile.Location = new System.Drawing.Point(115, 52);
			this.textBoxFuzzedFile.Name = "textBoxFuzzedFile";
			this.textBoxFuzzedFile.Size = new System.Drawing.Size(293, 20);
			this.textBoxFuzzedFile.TabIndex = 4;
			this.textBoxFuzzedFile.Text = "fuzzed.txt";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(29, 107);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(80, 13);
			this.label3.TabIndex = 3;
			this.label3.Text = "Command Line:";
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
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.groupBox5);
			this.tabPage2.Controls.Add(this.groupBox4);
			this.tabPage2.Controls.Add(this.groupBox3);
			this.tabPage2.Controls.Add(this.groupBox2);
			this.tabPage2.Controls.Add(this.groupBox1);
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage2.Size = new System.Drawing.Size(545, 394);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Debugger";
			this.tabPage2.UseVisualStyleBackColor = true;
			// 
			// groupBox5
			// 
			this.groupBox5.Controls.Add(this.textBoxDebuggerCommandLine);
			this.groupBox5.Controls.Add(this.label14);
			this.groupBox5.Location = new System.Drawing.Point(8, 61);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.Size = new System.Drawing.Size(529, 58);
			this.groupBox5.TabIndex = 4;
			this.groupBox5.TabStop = false;
			this.groupBox5.Text = "Start Process";
			// 
			// textBoxDebuggerCommandLine
			// 
			this.textBoxDebuggerCommandLine.Location = new System.Drawing.Point(92, 22);
			this.textBoxDebuggerCommandLine.Name = "textBoxDebuggerCommandLine";
			this.textBoxDebuggerCommandLine.Size = new System.Drawing.Size(431, 20);
			this.textBoxDebuggerCommandLine.TabIndex = 1;
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
			this.groupBox4.Controls.Add(this.textBox7);
			this.groupBox4.Controls.Add(this.label6);
			this.groupBox4.Location = new System.Drawing.Point(8, 277);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(531, 68);
			this.groupBox4.TabIndex = 3;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Kernel Debugger";
			// 
			// textBox7
			// 
			this.textBox7.Location = new System.Drawing.Point(141, 28);
			this.textBox7.Name = "textBox7";
			this.textBox7.Size = new System.Drawing.Size(384, 20);
			this.textBox7.TabIndex = 1;
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
			this.groupBox3.Controls.Add(this.comboBox1);
			this.groupBox3.Controls.Add(this.label5);
			this.groupBox3.Location = new System.Drawing.Point(8, 207);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(529, 64);
			this.groupBox3.TabIndex = 2;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Attach To Service";
			// 
			// comboBox1
			// 
			this.comboBox1.FormattingEnabled = true;
			this.comboBox1.Location = new System.Drawing.Point(58, 26);
			this.comboBox1.Name = "comboBox1";
			this.comboBox1.Size = new System.Drawing.Size(187, 21);
			this.comboBox1.TabIndex = 1;
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
			this.groupBox2.Controls.Add(this.textBox6);
			this.groupBox2.Controls.Add(this.textBox5);
			this.groupBox2.Controls.Add(this.radioButton6);
			this.groupBox2.Controls.Add(this.radioButton5);
			this.groupBox2.Location = new System.Drawing.Point(8, 125);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(529, 76);
			this.groupBox2.TabIndex = 1;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Attach to Process";
			// 
			// textBox6
			// 
			this.textBox6.Location = new System.Drawing.Point(110, 44);
			this.textBox6.Name = "textBox6";
			this.textBox6.Size = new System.Drawing.Size(219, 20);
			this.textBox6.TabIndex = 3;
			// 
			// textBox5
			// 
			this.textBox5.Location = new System.Drawing.Point(110, 18);
			this.textBox5.Name = "textBox5";
			this.textBox5.Size = new System.Drawing.Size(219, 20);
			this.textBox5.TabIndex = 2;
			// 
			// radioButton6
			// 
			this.radioButton6.AutoSize = true;
			this.radioButton6.Location = new System.Drawing.Point(9, 45);
			this.radioButton6.Name = "radioButton6";
			this.radioButton6.Size = new System.Drawing.Size(94, 17);
			this.radioButton6.TabIndex = 1;
			this.radioButton6.TabStop = true;
			this.radioButton6.Text = "Process Name";
			this.radioButton6.UseVisualStyleBackColor = true;
			// 
			// radioButton5
			// 
			this.radioButton5.AutoSize = true;
			this.radioButton5.Location = new System.Drawing.Point(9, 19);
			this.radioButton5.Name = "radioButton5";
			this.radioButton5.Size = new System.Drawing.Size(43, 17);
			this.radioButton5.TabIndex = 0;
			this.radioButton5.TabStop = true;
			this.radioButton5.Text = "PID";
			this.radioButton5.UseVisualStyleBackColor = true;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.radioButton4);
			this.groupBox1.Controls.Add(this.radioButton3);
			this.groupBox1.Controls.Add(this.radioButton2);
			this.groupBox1.Controls.Add(this.radioButtonDebuggerStartProcess);
			this.groupBox1.Location = new System.Drawing.Point(8, 6);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(529, 49);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Debugger Type";
			// 
			// radioButton4
			// 
			this.radioButton4.AutoSize = true;
			this.radioButton4.Location = new System.Drawing.Point(128, 19);
			this.radioButton4.Name = "radioButton4";
			this.radioButton4.Size = new System.Drawing.Size(109, 17);
			this.radioButton4.TabIndex = 3;
			this.radioButton4.Text = "Attach to Process";
			this.radioButton4.UseVisualStyleBackColor = true;
			// 
			// radioButton3
			// 
			this.radioButton3.AutoSize = true;
			this.radioButton3.Location = new System.Drawing.Point(418, 19);
			this.radioButton3.Name = "radioButton3";
			this.radioButton3.Size = new System.Drawing.Size(105, 17);
			this.radioButton3.TabIndex = 2;
			this.radioButton3.Text = "Kernel Debugger";
			this.radioButton3.UseVisualStyleBackColor = true;
			// 
			// radioButton2
			// 
			this.radioButton2.AutoSize = true;
			this.radioButton2.Location = new System.Drawing.Point(273, 19);
			this.radioButton2.Name = "radioButton2";
			this.radioButton2.Size = new System.Drawing.Size(107, 17);
			this.radioButton2.TabIndex = 1;
			this.radioButton2.Text = "Attach to Service";
			this.radioButton2.UseVisualStyleBackColor = true;
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
			// tabPage3
			// 
			this.tabPage3.Location = new System.Drawing.Point(4, 22);
			this.tabPage3.Name = "tabPage3";
			this.tabPage3.Size = new System.Drawing.Size(545, 394);
			this.tabPage3.TabIndex = 2;
			this.tabPage3.Text = "GUI";
			this.tabPage3.UseVisualStyleBackColor = true;
			// 
			// tabPage4
			// 
			this.tabPage4.Controls.Add(this.textBox13);
			this.tabPage4.Controls.Add(this.label12);
			this.tabPage4.Controls.Add(this.textBox12);
			this.tabPage4.Controls.Add(this.textBox11);
			this.tabPage4.Controls.Add(this.textBox10);
			this.tabPage4.Controls.Add(this.textBox9);
			this.tabPage4.Controls.Add(this.label11);
			this.tabPage4.Controls.Add(this.label10);
			this.tabPage4.Controls.Add(this.label9);
			this.tabPage4.Controls.Add(this.label8);
			this.tabPage4.Location = new System.Drawing.Point(4, 22);
			this.tabPage4.Name = "tabPage4";
			this.tabPage4.Size = new System.Drawing.Size(545, 394);
			this.tabPage4.TabIndex = 3;
			this.tabPage4.Text = "Fuzzing";
			this.tabPage4.UseVisualStyleBackColor = true;
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
			this.tabPageOutput.Controls.Add(this.textBoxOutput);
			this.tabPageOutput.Controls.Add(this.progressBarOuputFuzzing);
			this.tabPageOutput.Location = new System.Drawing.Point(4, 22);
			this.tabPageOutput.Name = "tabPageOutput";
			this.tabPageOutput.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageOutput.Size = new System.Drawing.Size(545, 394);
			this.tabPageOutput.TabIndex = 4;
			this.tabPageOutput.Text = "Output";
			this.tabPageOutput.UseVisualStyleBackColor = true;
			// 
			// textBoxOutput
			// 
			this.textBoxOutput.Location = new System.Drawing.Point(8, 35);
			this.textBoxOutput.Multiline = true;
			this.textBoxOutput.Name = "textBoxOutput";
			this.textBoxOutput.ReadOnly = true;
			this.textBoxOutput.Size = new System.Drawing.Size(529, 325);
			this.textBoxOutput.TabIndex = 1;
			// 
			// progressBarOuputFuzzing
			// 
			this.progressBarOuputFuzzing.Location = new System.Drawing.Point(8, 6);
			this.progressBarOuputFuzzing.Name = "progressBarOuputFuzzing";
			this.progressBarOuputFuzzing.Size = new System.Drawing.Size(529, 23);
			this.progressBarOuputFuzzing.TabIndex = 0;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.button6);
			this.panel1.Controls.Add(this.button5);
			this.panel1.Controls.Add(this.button4);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(0, 388);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(553, 32);
			this.panel1.TabIndex = 1;
			// 
			// button6
			// 
			this.button6.Location = new System.Drawing.Point(352, 3);
			this.button6.Name = "button6";
			this.button6.Size = new System.Drawing.Size(87, 23);
			this.button6.TabIndex = 2;
			this.button6.Text = "Stop Fuzzing";
			this.button6.UseVisualStyleBackColor = true;
			// 
			// button5
			// 
			this.button5.Location = new System.Drawing.Point(237, 3);
			this.button5.Name = "button5";
			this.button5.Size = new System.Drawing.Size(109, 23);
			this.button5.TabIndex = 1;
			this.button5.Text = "Save Configuration";
			this.button5.UseVisualStyleBackColor = true;
			// 
			// button4
			// 
			this.button4.Location = new System.Drawing.Point(129, 3);
			this.button4.Name = "button4";
			this.button4.Size = new System.Drawing.Size(102, 23);
			this.button4.TabIndex = 0;
			this.button4.Text = "Start Fuzzing";
			this.button4.UseVisualStyleBackColor = true;
			this.button4.Click += new System.EventHandler(this.button4_Click);
			// 
			// FormMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(553, 420);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.tabControl1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "FormMain";
			this.Text = "Peach Dumb File Fuzzer";
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tabPage1.PerformLayout();
			this.tabPage2.ResumeLayout(false);
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
			this.tabPage4.ResumeLayout(false);
			this.tabPage4.PerformLayout();
			this.tabPageOutput.ResumeLayout(false);
			this.tabPageOutput.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.TextBox textBoxCommandLine;
		private System.Windows.Forms.TextBox textBoxExecutable;
		private System.Windows.Forms.TextBox textBoxFuzzedFile;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox textBoxTemplateFiles;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.TabPage tabPage3;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.TextBox textBox6;
		private System.Windows.Forms.TextBox textBox5;
		private System.Windows.Forms.RadioButton radioButton6;
		private System.Windows.Forms.RadioButton radioButton5;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.RadioButton radioButton4;
		private System.Windows.Forms.RadioButton radioButton3;
		private System.Windows.Forms.RadioButton radioButton2;
		private System.Windows.Forms.RadioButton radioButtonDebuggerStartProcess;
		private System.Windows.Forms.TextBox textBox7;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.ComboBox comboBox1;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox textBoxIterations;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.TabPage tabPage4;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button button6;
		private System.Windows.Forms.Button button5;
		private System.Windows.Forms.Button button4;
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
	}
}

