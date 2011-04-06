namespace PeachBuilder
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
			this.components = new System.ComponentModel.Container();
			System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Peach");
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
			this.statusStripMain = new System.Windows.Forms.StatusStrip();
			this.toolStripMain = new System.Windows.Forms.ToolStrip();
			this.toolStripButtonMainNew = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonMainOpen = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonMainSave = new System.Windows.Forms.ToolStripButton();
			this.tabControlMain = new System.Windows.Forms.TabControl();
			this.tabPageDesigner = new System.Windows.Forms.TabPage();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.treeViewPit = new System.Windows.Forms.TreeView();
			this.imageListMain = new System.Windows.Forms.ImageList(this.components);
			this.propertyGridDesigner = new System.Windows.Forms.PropertyGrid();
			this.tabPageEditor = new System.Windows.Forms.TabPage();
			this.toolStripMain.SuspendLayout();
			this.tabControlMain.SuspendLayout();
			this.tabPageDesigner.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// statusStripMain
			// 
			this.statusStripMain.Location = new System.Drawing.Point(0, 434);
			this.statusStripMain.Name = "statusStripMain";
			this.statusStripMain.Size = new System.Drawing.Size(530, 22);
			this.statusStripMain.TabIndex = 0;
			this.statusStripMain.Text = "statusStripMain";
			// 
			// toolStripMain
			// 
			this.toolStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonMainNew,
            this.toolStripButtonMainOpen,
            this.toolStripButtonMainSave});
			this.toolStripMain.Location = new System.Drawing.Point(0, 0);
			this.toolStripMain.Name = "toolStripMain";
			this.toolStripMain.Size = new System.Drawing.Size(530, 25);
			this.toolStripMain.TabIndex = 1;
			this.toolStripMain.Text = "toolStripMain";
			// 
			// toolStripButtonMainNew
			// 
			this.toolStripButtonMainNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonMainNew.Image = global::PeachBuilder.Properties.Resources.document_new;
			this.toolStripButtonMainNew.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonMainNew.Name = "toolStripButtonMainNew";
			this.toolStripButtonMainNew.Size = new System.Drawing.Size(23, 22);
			this.toolStripButtonMainNew.Text = "New";
			this.toolStripButtonMainNew.Click += new System.EventHandler(this.toolStripButtonMainNew_Click);
			// 
			// toolStripButtonMainOpen
			// 
			this.toolStripButtonMainOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonMainOpen.Image = global::PeachBuilder.Properties.Resources.document_open;
			this.toolStripButtonMainOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonMainOpen.Name = "toolStripButtonMainOpen";
			this.toolStripButtonMainOpen.Size = new System.Drawing.Size(23, 22);
			this.toolStripButtonMainOpen.Text = "toolStripButton2";
			// 
			// toolStripButtonMainSave
			// 
			this.toolStripButtonMainSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonMainSave.Image = global::PeachBuilder.Properties.Resources.document_save;
			this.toolStripButtonMainSave.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonMainSave.Name = "toolStripButtonMainSave";
			this.toolStripButtonMainSave.Size = new System.Drawing.Size(23, 22);
			this.toolStripButtonMainSave.Text = "toolStripButton3";
			// 
			// tabControlMain
			// 
			this.tabControlMain.Controls.Add(this.tabPageDesigner);
			this.tabControlMain.Controls.Add(this.tabPageEditor);
			this.tabControlMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControlMain.Location = new System.Drawing.Point(0, 25);
			this.tabControlMain.Name = "tabControlMain";
			this.tabControlMain.SelectedIndex = 0;
			this.tabControlMain.Size = new System.Drawing.Size(530, 409);
			this.tabControlMain.TabIndex = 2;
			// 
			// tabPageDesigner
			// 
			this.tabPageDesigner.Controls.Add(this.splitContainer1);
			this.tabPageDesigner.Location = new System.Drawing.Point(4, 22);
			this.tabPageDesigner.Name = "tabPageDesigner";
			this.tabPageDesigner.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageDesigner.Size = new System.Drawing.Size(522, 383);
			this.tabPageDesigner.TabIndex = 0;
			this.tabPageDesigner.Text = "Designer";
			this.tabPageDesigner.UseVisualStyleBackColor = true;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(3, 3);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.treeViewPit);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.propertyGridDesigner);
			this.splitContainer1.Size = new System.Drawing.Size(516, 377);
			this.splitContainer1.SplitterDistance = 172;
			this.splitContainer1.TabIndex = 0;
			// 
			// treeViewPit
			// 
			this.treeViewPit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeViewPit.ImageIndex = 0;
			this.treeViewPit.ImageList = this.imageListMain;
			this.treeViewPit.Location = new System.Drawing.Point(0, 0);
			this.treeViewPit.Name = "treeViewPit";
			treeNode2.Name = "Node0";
			treeNode2.Text = "Peach";
			this.treeViewPit.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode2});
			this.treeViewPit.SelectedImageIndex = 0;
			this.treeViewPit.Size = new System.Drawing.Size(172, 377);
			this.treeViewPit.TabIndex = 0;
			// 
			// imageListMain
			// 
			this.imageListMain.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListMain.ImageStream")));
			this.imageListMain.TransparentColor = System.Drawing.Color.Transparent;
			this.imageListMain.Images.SetKeyName(0, "peach-line.png");
			this.imageListMain.Images.SetKeyName(1, "node-action.png");
			this.imageListMain.Images.SetKeyName(2, "node-agent.png");
			this.imageListMain.Images.SetKeyName(3, "node-blob.png");
			this.imageListMain.Images.SetKeyName(4, "node-block.png");
			this.imageListMain.Images.SetKeyName(5, "node-choice.png");
			this.imageListMain.Images.SetKeyName(6, "node-data.png");
			this.imageListMain.Images.SetKeyName(7, "node-error.png");
			this.imageListMain.Images.SetKeyName(8, "node-flags.gif");
			this.imageListMain.Images.SetKeyName(9, "node-flags.png");
			this.imageListMain.Images.SetKeyName(10, "node-generator.png");
			this.imageListMain.Images.SetKeyName(11, "node-import.png");
			this.imageListMain.Images.SetKeyName(12, "node-logger.png");
			this.imageListMain.Images.SetKeyName(13, "node-monitor.png");
			this.imageListMain.Images.SetKeyName(14, "node-mutator.png");
			this.imageListMain.Images.SetKeyName(15, "node-mutators.png");
			this.imageListMain.Images.SetKeyName(16, "node-namespace.png");
			this.imageListMain.Images.SetKeyName(17, "node-number.png");
			this.imageListMain.Images.SetKeyName(18, "node-peach.png");
			this.imageListMain.Images.SetKeyName(19, "node-publisher.gif");
			this.imageListMain.Images.SetKeyName(20, "node-pythonpath.png");
			this.imageListMain.Images.SetKeyName(21, "node-relation.png");
			this.imageListMain.Images.SetKeyName(22, "node-run.png");
			this.imageListMain.Images.SetKeyName(23, "node-sequence.png");
			this.imageListMain.Images.SetKeyName(24, "node-state.png");
			this.imageListMain.Images.SetKeyName(25, "node-statemachine.png");
			this.imageListMain.Images.SetKeyName(26, "node-string.png");
			this.imageListMain.Images.SetKeyName(27, "node-template.png");
			this.imageListMain.Images.SetKeyName(28, "node-test.png");
			this.imageListMain.Images.SetKeyName(29, "node-transform.png");
			this.imageListMain.Images.SetKeyName(30, "node-unknown.png");
			// 
			// propertyGridDesigner
			// 
			this.propertyGridDesigner.Dock = System.Windows.Forms.DockStyle.Fill;
			this.propertyGridDesigner.Location = new System.Drawing.Point(0, 0);
			this.propertyGridDesigner.Name = "propertyGridDesigner";
			this.propertyGridDesigner.Size = new System.Drawing.Size(340, 377);
			this.propertyGridDesigner.TabIndex = 0;
			// 
			// tabPageEditor
			// 
			this.tabPageEditor.Location = new System.Drawing.Point(4, 22);
			this.tabPageEditor.Name = "tabPageEditor";
			this.tabPageEditor.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageEditor.Size = new System.Drawing.Size(522, 383);
			this.tabPageEditor.TabIndex = 1;
			this.tabPageEditor.Text = "Editor";
			this.tabPageEditor.UseVisualStyleBackColor = true;
			// 
			// FormMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(530, 456);
			this.Controls.Add(this.tabControlMain);
			this.Controls.Add(this.toolStripMain);
			this.Controls.Add(this.statusStripMain);
			this.Name = "FormMain";
			this.Text = "Peach Builder";
			this.toolStripMain.ResumeLayout(false);
			this.toolStripMain.PerformLayout();
			this.tabControlMain.ResumeLayout(false);
			this.tabPageDesigner.ResumeLayout(false);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.StatusStrip statusStripMain;
		private System.Windows.Forms.ToolStrip toolStripMain;
		private System.Windows.Forms.TabControl tabControlMain;
		private System.Windows.Forms.TabPage tabPageDesigner;
		private System.Windows.Forms.TabPage tabPageEditor;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.TreeView treeViewPit;
		private System.Windows.Forms.ToolStripButton toolStripButtonMainNew;
		private System.Windows.Forms.ToolStripButton toolStripButtonMainOpen;
		private System.Windows.Forms.ToolStripButton toolStripButtonMainSave;
		private System.Windows.Forms.PropertyGrid propertyGridDesigner;
		private System.Windows.Forms.ImageList imageListMain;
	}
}

