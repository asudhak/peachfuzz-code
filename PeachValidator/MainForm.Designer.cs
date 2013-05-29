namespace PeachValidator
{
	partial class MainForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
			this.toolStripButtonOpenPit = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonRefreshPit = new System.Windows.Forms.ToolStripButton();
			this.toolStripComboBoxDataModel = new System.Windows.Forms.ToolStripComboBox();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
			this.toolStripButtonOpenSample = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonRefreshSample = new System.Windows.Forms.ToolStripButton();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.hexBox1 = new Be.Windows.Forms.HexBox();
			this.treeViewAdv1 = new Aga.Controls.Tree.TreeViewAdv();
			this.treeColumn1 = new Aga.Controls.Tree.TreeColumn();
			this.treeColumn3 = new Aga.Controls.Tree.TreeColumn();
			this.treeColumn4 = new Aga.Controls.Tree.TreeColumn();
			this.treeColumn5 = new Aga.Controls.Tree.TreeColumn();
			this.nodeIcon1 = new Aga.Controls.Tree.NodeControls.NodeIcon();
			this.nodeTextBoxName = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.nodeTextBoxPosition = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.nodeTextBoxLength = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.nodeTextBoxValue = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.toolStrip1.SuspendLayout();
			this.splitContainer1.BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStrip1
			// 
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel2,
            this.toolStripButtonOpenPit,
            this.toolStripButtonRefreshPit,
            this.toolStripComboBoxDataModel,
            this.toolStripSeparator1,
            this.toolStripLabel1,
            this.toolStripButtonOpenSample,
            this.toolStripButtonRefreshSample});
			this.toolStrip1.Location = new System.Drawing.Point(0, 0);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(651, 25);
			this.toolStrip1.TabIndex = 0;
			this.toolStrip1.Text = "toolStrip1";
			// 
			// toolStripLabel2
			// 
			this.toolStripLabel2.Name = "toolStripLabel2";
			this.toolStripLabel2.Size = new System.Drawing.Size(24, 22);
			this.toolStripLabel2.Text = "Pit:";
			// 
			// toolStripButtonOpenPit
			// 
			this.toolStripButtonOpenPit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonOpenPit.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonOpenPit.Image")));
			this.toolStripButtonOpenPit.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonOpenPit.Name = "toolStripButtonOpenPit";
			this.toolStripButtonOpenPit.Size = new System.Drawing.Size(23, 22);
			this.toolStripButtonOpenPit.Text = "toolStripButton2";
			this.toolStripButtonOpenPit.ToolTipText = "Open Pit File";
			this.toolStripButtonOpenPit.Click += new System.EventHandler(this.toolStripButtonOpenPit_Click);
			// 
			// toolStripButtonRefreshPit
			// 
			this.toolStripButtonRefreshPit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonRefreshPit.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonRefreshPit.Image")));
			this.toolStripButtonRefreshPit.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonRefreshPit.Name = "toolStripButtonRefreshPit";
			this.toolStripButtonRefreshPit.Size = new System.Drawing.Size(23, 22);
			this.toolStripButtonRefreshPit.Text = "toolStripButton1";
			this.toolStripButtonRefreshPit.ToolTipText = "Reload Pit File";
			this.toolStripButtonRefreshPit.Click += new System.EventHandler(this.toolStripButtonRefreshPit_Click);
			// 
			// toolStripComboBoxDataModel
			// 
			this.toolStripComboBoxDataModel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.toolStripComboBoxDataModel.Name = "toolStripComboBoxDataModel";
			this.toolStripComboBoxDataModel.Size = new System.Drawing.Size(121, 25);
			this.toolStripComboBoxDataModel.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBoxDataModel_SelectedIndexChanged);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			// 
			// toolStripLabel1
			// 
			this.toolStripLabel1.Name = "toolStripLabel1";
			this.toolStripLabel1.Size = new System.Drawing.Size(68, 22);
			this.toolStripLabel1.Text = "Sample file:";
			// 
			// toolStripButtonOpenSample
			// 
			this.toolStripButtonOpenSample.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonOpenSample.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonOpenSample.Image")));
			this.toolStripButtonOpenSample.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonOpenSample.Name = "toolStripButtonOpenSample";
			this.toolStripButtonOpenSample.Size = new System.Drawing.Size(23, 22);
			this.toolStripButtonOpenSample.Text = "toolStripButton3";
			this.toolStripButtonOpenSample.ToolTipText = "Open Sample File";
			this.toolStripButtonOpenSample.Click += new System.EventHandler(this.toolStripButtonOpenSample_Click);
			// 
			// toolStripButtonRefreshSample
			// 
			this.toolStripButtonRefreshSample.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonRefreshSample.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonRefreshSample.Image")));
			this.toolStripButtonRefreshSample.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonRefreshSample.Name = "toolStripButtonRefreshSample";
			this.toolStripButtonRefreshSample.Size = new System.Drawing.Size(23, 22);
			this.toolStripButtonRefreshSample.Text = "toolStripButton4";
			this.toolStripButtonRefreshSample.ToolTipText = "Reload Sample File";
			this.toolStripButtonRefreshSample.Click += new System.EventHandler(this.toolStripButtonRefreshSample_Click);
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 25);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.hexBox1);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.treeViewAdv1);
			this.splitContainer1.Size = new System.Drawing.Size(651, 569);
			this.splitContainer1.SplitterDistance = 217;
			this.splitContainer1.TabIndex = 1;
			// 
			// hexBox1
			// 
			this.hexBox1.ColumnInfoVisible = true;
			this.hexBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.hexBox1.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.hexBox1.InfoForeColor = System.Drawing.Color.Empty;
			this.hexBox1.LineInfoVisible = true;
			this.hexBox1.Location = new System.Drawing.Point(0, 0);
			this.hexBox1.Name = "hexBox1";
			this.hexBox1.ReadOnly = true;
			this.hexBox1.ShadowSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(60)))), ((int)(((byte)(188)))), ((int)(((byte)(255)))));
			this.hexBox1.Size = new System.Drawing.Size(651, 217);
			this.hexBox1.StringViewVisible = true;
			this.hexBox1.TabIndex = 0;
			this.hexBox1.VScrollBarVisible = true;
			// 
			// treeViewAdv1
			// 
			this.treeViewAdv1.AutoRowHeight = true;
			this.treeViewAdv1.BackColor = System.Drawing.SystemColors.Window;
			this.treeViewAdv1.Columns.Add(this.treeColumn1);
			this.treeViewAdv1.Columns.Add(this.treeColumn3);
			this.treeViewAdv1.Columns.Add(this.treeColumn4);
			this.treeViewAdv1.Columns.Add(this.treeColumn5);
			this.treeViewAdv1.DefaultToolTipProvider = null;
			this.treeViewAdv1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeViewAdv1.DragDropMarkColor = System.Drawing.Color.Black;
			this.treeViewAdv1.FullRowSelect = true;
			this.treeViewAdv1.Indent = 10;
			this.treeViewAdv1.LineColor = System.Drawing.SystemColors.ControlDark;
			this.treeViewAdv1.Location = new System.Drawing.Point(0, 0);
			this.treeViewAdv1.Model = null;
			this.treeViewAdv1.Name = "treeViewAdv1";
			this.treeViewAdv1.NodeControls.Add(this.nodeIcon1);
			this.treeViewAdv1.NodeControls.Add(this.nodeTextBoxName);
			this.treeViewAdv1.NodeControls.Add(this.nodeTextBoxPosition);
			this.treeViewAdv1.NodeControls.Add(this.nodeTextBoxLength);
			this.treeViewAdv1.NodeControls.Add(this.nodeTextBoxValue);
			this.treeViewAdv1.RowHeight = 32;
			this.treeViewAdv1.SelectedNode = null;
			this.treeViewAdv1.Size = new System.Drawing.Size(651, 348);
			this.treeViewAdv1.TabIndex = 0;
			this.treeViewAdv1.Text = "treeViewAdv1";
			this.treeViewAdv1.UseColumns = true;
			this.treeViewAdv1.SelectionChanged += new System.EventHandler(this.treeViewAdv1_SelectionChanged);
			// 
			// treeColumn1
			// 
			this.treeColumn1.Header = "Name";
			this.treeColumn1.SortOrder = System.Windows.Forms.SortOrder.None;
			this.treeColumn1.TooltipText = null;
			this.treeColumn1.Width = 220;
			// 
			// treeColumn3
			// 
			this.treeColumn3.Header = "Position";
			this.treeColumn3.Sortable = true;
			this.treeColumn3.SortOrder = System.Windows.Forms.SortOrder.None;
			this.treeColumn3.TooltipText = null;
			// 
			// treeColumn4
			// 
			this.treeColumn4.Header = "Length";
			this.treeColumn4.SortOrder = System.Windows.Forms.SortOrder.None;
			this.treeColumn4.TooltipText = null;
			// 
			// treeColumn5
			// 
			this.treeColumn5.Header = "Value";
			this.treeColumn5.SortOrder = System.Windows.Forms.SortOrder.None;
			this.treeColumn5.TooltipText = null;
			this.treeColumn5.Width = 300;
			// 
			// nodeIcon1
			// 
			this.nodeIcon1.DataPropertyName = "Icon";
			this.nodeIcon1.LeftMargin = 1;
			this.nodeIcon1.ParentColumn = this.treeColumn1;
			this.nodeIcon1.ScaleMode = Aga.Controls.Tree.ImageScaleMode.Clip;
			// 
			// nodeTextBoxName
			// 
			this.nodeTextBoxName.DataPropertyName = "Name";
			this.nodeTextBoxName.IncrementalSearchEnabled = true;
			this.nodeTextBoxName.LeftMargin = 3;
			this.nodeTextBoxName.ParentColumn = this.treeColumn1;
			// 
			// nodeTextBoxPosition
			// 
			this.nodeTextBoxPosition.DataPropertyName = "Position";
			this.nodeTextBoxPosition.IncrementalSearchEnabled = true;
			this.nodeTextBoxPosition.LeftMargin = 3;
			this.nodeTextBoxPosition.ParentColumn = this.treeColumn3;
			// 
			// nodeTextBoxLength
			// 
			this.nodeTextBoxLength.DataPropertyName = "Length";
			this.nodeTextBoxLength.IncrementalSearchEnabled = true;
			this.nodeTextBoxLength.LeftMargin = 3;
			this.nodeTextBoxLength.ParentColumn = this.treeColumn4;
			// 
			// nodeTextBoxValue
			// 
			this.nodeTextBoxValue.DataPropertyName = "Value";
			this.nodeTextBoxValue.IncrementalSearchEnabled = true;
			this.nodeTextBoxValue.LeftMargin = 3;
			this.nodeTextBoxValue.ParentColumn = this.treeColumn5;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(651, 594);
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.toolStrip1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "MainForm";
			this.Text = "Peach Validator v3.0";
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.ToolStripLabel toolStripLabel2;
		private System.Windows.Forms.ToolStripButton toolStripButtonOpenPit;
		private System.Windows.Forms.ToolStripButton toolStripButtonRefreshPit;
		private System.Windows.Forms.ToolStripComboBox toolStripComboBoxDataModel;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripLabel toolStripLabel1;
		private System.Windows.Forms.ToolStripButton toolStripButtonOpenSample;
		private System.Windows.Forms.ToolStripButton toolStripButtonRefreshSample;
		private Be.Windows.Forms.HexBox hexBox1;
		private Aga.Controls.Tree.TreeViewAdv treeViewAdv1;
		private Aga.Controls.Tree.TreeColumn treeColumn1;
		private Aga.Controls.Tree.TreeColumn treeColumn3;
		private Aga.Controls.Tree.TreeColumn treeColumn4;
		private Aga.Controls.Tree.TreeColumn treeColumn5;
		private Aga.Controls.Tree.NodeControls.NodeIcon nodeIcon1;
		private Aga.Controls.Tree.NodeControls.NodeTextBox nodeTextBoxName;
		private Aga.Controls.Tree.NodeControls.NodeTextBox nodeTextBoxPosition;
		private Aga.Controls.Tree.NodeControls.NodeTextBox nodeTextBoxLength;
		private Aga.Controls.Tree.NodeControls.NodeTextBox nodeTextBoxValue;
	}
}

