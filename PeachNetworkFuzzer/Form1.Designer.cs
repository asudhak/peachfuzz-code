namespace PeachNetworkFuzzer
{
	partial class Form1
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
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.groupBox6 = new System.Windows.Forms.GroupBox();
			this.radioButtonUDP = new System.Windows.Forms.RadioButton();
			this.radioButtonTCP = new System.Windows.Forms.RadioButton();
			this.textTargetHost = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.textBoxTemplateFiles = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.groupBox7 = new System.Windows.Forms.GroupBox();
			this.button3 = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.textBoxExecutable = new System.Windows.Forms.TextBox();
			this.textBoxCommandLine = new System.Windows.Forms.TextBox();
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
			this.radioButton1 = new System.Windows.Forms.RadioButton();
			this.tabPage3 = new System.Windows.Forms.TabPage();
			this.tabPage4 = new System.Windows.Forms.TabPage();
			this.textRunTime = new System.Windows.Forms.TextBox();
			this.textPacketCount = new System.Windows.Forms.TextBox();
			this.label10 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.buttonStopFuzzing = new System.Windows.Forms.Button();
			this.buttonSave = new System.Windows.Forms.Button();
			this.buttonStartFuzzing = new System.Windows.Forms.Button();
			this.textTargetPort = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.groupBox6.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.groupBox7.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.tabPage4.SuspendLayout();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Controls.Add(this.tabPage3);
			this.tabControl1.Controls.Add(this.tabPage4);
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.Location = new System.Drawing.Point(0, 0);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(553, 441);
			this.tabControl1.TabIndex = 2;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.textTargetPort);
			this.tabPage1.Controls.Add(this.label7);
			this.tabPage1.Controls.Add(this.groupBox6);
			this.tabPage1.Controls.Add(this.textTargetHost);
			this.tabPage1.Controls.Add(this.label2);
			this.tabPage1.Controls.Add(this.textBoxTemplateFiles);
			this.tabPage1.Controls.Add(this.label1);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(545, 415);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "General";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// groupBox6
			// 
			this.groupBox6.Controls.Add(this.radioButtonUDP);
			this.groupBox6.Controls.Add(this.radioButtonTCP);
			this.groupBox6.Location = new System.Drawing.Point(116, 6);
			this.groupBox6.Name = "groupBox6";
			this.groupBox6.Size = new System.Drawing.Size(293, 52);
			this.groupBox6.TabIndex = 12;
			this.groupBox6.TabStop = false;
			this.groupBox6.Text = "Protocol";
			// 
			// radioButtonUDP
			// 
			this.radioButtonUDP.AutoSize = true;
			this.radioButtonUDP.Location = new System.Drawing.Point(149, 19);
			this.radioButtonUDP.Name = "radioButtonUDP";
			this.radioButtonUDP.Size = new System.Drawing.Size(48, 17);
			this.radioButtonUDP.TabIndex = 1;
			this.radioButtonUDP.TabStop = true;
			this.radioButtonUDP.Text = "UDP";
			this.radioButtonUDP.UseVisualStyleBackColor = true;
			// 
			// radioButtonTCP
			// 
			this.radioButtonTCP.AutoSize = true;
			this.radioButtonTCP.Location = new System.Drawing.Point(47, 19);
			this.radioButtonTCP.Name = "radioButtonTCP";
			this.radioButtonTCP.Size = new System.Drawing.Size(46, 17);
			this.radioButtonTCP.TabIndex = 0;
			this.radioButtonTCP.TabStop = true;
			this.radioButtonTCP.Text = "TCP";
			this.radioButtonTCP.UseVisualStyleBackColor = true;
			// 
			// textTargetHost
			// 
			this.textTargetHost.Location = new System.Drawing.Point(116, 90);
			this.textTargetHost.Name = "textTargetHost";
			this.textTargetHost.Size = new System.Drawing.Size(293, 20);
			this.textTargetHost.TabIndex = 4;
			this.textTargetHost.Text = "127.0.0.1";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(44, 93);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(66, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "Target Host:";
			// 
			// textBoxTemplateFiles
			// 
			this.textBoxTemplateFiles.Location = new System.Drawing.Point(116, 64);
			this.textBoxTemplateFiles.Name = "textBoxTemplateFiles";
			this.textBoxTemplateFiles.Size = new System.Drawing.Size(293, 20);
			this.textBoxTemplateFiles.TabIndex = 1;
			this.textBoxTemplateFiles.Text = "8000";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(35, 67);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(75, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Incoming Port:";
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.groupBox7);
			this.tabPage2.Controls.Add(this.groupBox4);
			this.tabPage2.Controls.Add(this.groupBox3);
			this.tabPage2.Controls.Add(this.groupBox2);
			this.tabPage2.Controls.Add(this.groupBox1);
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage2.Size = new System.Drawing.Size(545, 415);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Debugger";
			this.tabPage2.UseVisualStyleBackColor = true;
			// 
			// groupBox7
			// 
			this.groupBox7.Controls.Add(this.button3);
			this.groupBox7.Controls.Add(this.label3);
			this.groupBox7.Controls.Add(this.label4);
			this.groupBox7.Controls.Add(this.textBoxExecutable);
			this.groupBox7.Controls.Add(this.textBoxCommandLine);
			this.groupBox7.Location = new System.Drawing.Point(8, 82);
			this.groupBox7.Name = "groupBox7";
			this.groupBox7.Size = new System.Drawing.Size(529, 76);
			this.groupBox7.TabIndex = 4;
			this.groupBox7.TabStop = false;
			this.groupBox7.Text = "Start Process";
			// 
			// button3
			// 
			this.button3.Location = new System.Drawing.Point(410, 16);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(75, 23);
			this.button3.TabIndex = 20;
			this.button3.Text = "Browse";
			this.button3.UseVisualStyleBackColor = true;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(25, 47);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(80, 13);
			this.label3.TabIndex = 16;
			this.label3.Text = "Command Line:";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(8, 21);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(97, 13);
			this.label4.TabIndex = 19;
			this.label4.Text = "Target Executable:";
			// 
			// textBoxExecutable
			// 
			this.textBoxExecutable.Location = new System.Drawing.Point(111, 18);
			this.textBoxExecutable.Name = "textBoxExecutable";
			this.textBoxExecutable.Size = new System.Drawing.Size(293, 20);
			this.textBoxExecutable.TabIndex = 17;
			// 
			// textBoxCommandLine
			// 
			this.textBoxCommandLine.Location = new System.Drawing.Point(111, 44);
			this.textBoxCommandLine.Name = "textBoxCommandLine";
			this.textBoxCommandLine.Size = new System.Drawing.Size(293, 20);
			this.textBoxCommandLine.TabIndex = 18;
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.textBox7);
			this.groupBox4.Controls.Add(this.label6);
			this.groupBox4.Location = new System.Drawing.Point(8, 316);
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
			this.groupBox3.Location = new System.Drawing.Point(8, 246);
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
			this.groupBox2.Location = new System.Drawing.Point(8, 164);
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
			this.groupBox1.Controls.Add(this.radioButton1);
			this.groupBox1.Location = new System.Drawing.Point(8, 6);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(529, 70);
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
			this.radioButton4.TabStop = true;
			this.radioButton4.Text = "Attach to Process";
			this.radioButton4.UseVisualStyleBackColor = true;
			// 
			// radioButton3
			// 
			this.radioButton3.AutoSize = true;
			this.radioButton3.Location = new System.Drawing.Point(128, 42);
			this.radioButton3.Name = "radioButton3";
			this.radioButton3.Size = new System.Drawing.Size(105, 17);
			this.radioButton3.TabIndex = 2;
			this.radioButton3.TabStop = true;
			this.radioButton3.Text = "Kernel Debugger";
			this.radioButton3.UseVisualStyleBackColor = true;
			// 
			// radioButton2
			// 
			this.radioButton2.AutoSize = true;
			this.radioButton2.Location = new System.Drawing.Point(6, 42);
			this.radioButton2.Name = "radioButton2";
			this.radioButton2.Size = new System.Drawing.Size(107, 17);
			this.radioButton2.TabIndex = 1;
			this.radioButton2.TabStop = true;
			this.radioButton2.Text = "Attach to Service";
			this.radioButton2.UseVisualStyleBackColor = true;
			// 
			// radioButton1
			// 
			this.radioButton1.AutoSize = true;
			this.radioButton1.Location = new System.Drawing.Point(6, 19);
			this.radioButton1.Name = "radioButton1";
			this.radioButton1.Size = new System.Drawing.Size(88, 17);
			this.radioButton1.TabIndex = 0;
			this.radioButton1.TabStop = true;
			this.radioButton1.Text = "Start Process";
			this.radioButton1.UseVisualStyleBackColor = true;
			// 
			// tabPage3
			// 
			this.tabPage3.Location = new System.Drawing.Point(4, 22);
			this.tabPage3.Name = "tabPage3";
			this.tabPage3.Size = new System.Drawing.Size(545, 415);
			this.tabPage3.TabIndex = 2;
			this.tabPage3.Text = "GUI";
			this.tabPage3.UseVisualStyleBackColor = true;
			// 
			// tabPage4
			// 
			this.tabPage4.Controls.Add(this.textRunTime);
			this.tabPage4.Controls.Add(this.textPacketCount);
			this.tabPage4.Controls.Add(this.label10);
			this.tabPage4.Controls.Add(this.label8);
			this.tabPage4.Location = new System.Drawing.Point(4, 22);
			this.tabPage4.Name = "tabPage4";
			this.tabPage4.Size = new System.Drawing.Size(545, 415);
			this.tabPage4.TabIndex = 3;
			this.tabPage4.Text = "Fuzzing";
			this.tabPage4.UseVisualStyleBackColor = true;
			// 
			// textRunTime
			// 
			this.textRunTime.Location = new System.Drawing.Point(151, 44);
			this.textRunTime.Name = "textRunTime";
			this.textRunTime.Size = new System.Drawing.Size(284, 20);
			this.textRunTime.TabIndex = 6;
			// 
			// textPacketCount
			// 
			this.textPacketCount.Location = new System.Drawing.Point(151, 18);
			this.textPacketCount.Name = "textPacketCount";
			this.textPacketCount.Size = new System.Drawing.Size(284, 20);
			this.textPacketCount.TabIndex = 4;
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(89, 47);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(56, 13);
			this.label10.TabIndex = 2;
			this.label10.Text = "Run Time:";
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(70, 21);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(75, 13);
			this.label8.TabIndex = 0;
			this.label8.Text = "Packet Count:";
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.buttonStopFuzzing);
			this.panel1.Controls.Add(this.buttonSave);
			this.panel1.Controls.Add(this.buttonStartFuzzing);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(0, 441);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(553, 32);
			this.panel1.TabIndex = 3;
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
			// buttonSave
			// 
			this.buttonSave.Location = new System.Drawing.Point(237, 3);
			this.buttonSave.Name = "buttonSave";
			this.buttonSave.Size = new System.Drawing.Size(109, 23);
			this.buttonSave.TabIndex = 1;
			this.buttonSave.Text = "Save Configuration";
			this.buttonSave.UseVisualStyleBackColor = true;
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
			// textTargetPort
			// 
			this.textTargetPort.Location = new System.Drawing.Point(116, 116);
			this.textTargetPort.Name = "textTargetPort";
			this.textTargetPort.Size = new System.Drawing.Size(293, 20);
			this.textTargetPort.TabIndex = 14;
			this.textTargetPort.Text = "8000";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(44, 119);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(66, 13);
			this.label7.TabIndex = 13;
			this.label7.Text = "Target Host:";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(553, 473);
			this.Controls.Add(this.tabControl1);
			this.Controls.Add(this.panel1);
			this.Name = "Form1";
			this.Text = "Peach Dumb Network Fuzzer";
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tabPage1.PerformLayout();
			this.groupBox6.ResumeLayout(false);
			this.groupBox6.PerformLayout();
			this.tabPage2.ResumeLayout(false);
			this.groupBox7.ResumeLayout(false);
			this.groupBox7.PerformLayout();
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
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TextBox textTargetHost;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox textBoxTemplateFiles;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.TextBox textBox7;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.ComboBox comboBox1;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.TextBox textBox6;
		private System.Windows.Forms.TextBox textBox5;
		private System.Windows.Forms.RadioButton radioButton6;
		private System.Windows.Forms.RadioButton radioButton5;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.RadioButton radioButton4;
		private System.Windows.Forms.RadioButton radioButton3;
		private System.Windows.Forms.RadioButton radioButton2;
		private System.Windows.Forms.RadioButton radioButton1;
		private System.Windows.Forms.TabPage tabPage3;
		private System.Windows.Forms.TabPage tabPage4;
		private System.Windows.Forms.TextBox textRunTime;
		private System.Windows.Forms.TextBox textPacketCount;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button buttonStopFuzzing;
		private System.Windows.Forms.Button buttonSave;
		private System.Windows.Forms.Button buttonStartFuzzing;
		private System.Windows.Forms.GroupBox groupBox6;
		private System.Windows.Forms.RadioButton radioButtonUDP;
		private System.Windows.Forms.RadioButton radioButtonTCP;
		private System.Windows.Forms.GroupBox groupBox7;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox textBoxExecutable;
		private System.Windows.Forms.TextBox textBoxCommandLine;
		private System.Windows.Forms.TextBox textTargetPort;
		private System.Windows.Forms.Label label7;
	}
}

