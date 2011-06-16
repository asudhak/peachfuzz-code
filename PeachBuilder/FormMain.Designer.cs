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
			System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Peach");
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
			System.Windows.Forms.ListViewItem listViewItem9 = new System.Windows.Forms.ListViewItem("Number");
			System.Windows.Forms.ListViewItem listViewItem10 = new System.Windows.Forms.ListViewItem("String");
			System.Windows.Forms.ListViewItem listViewItem11 = new System.Windows.Forms.ListViewItem("StateModel");
			System.Windows.Forms.ListViewItem listViewItem12 = new System.Windows.Forms.ListViewItem("DataModel");
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
			this.tabPageEditor = new System.Windows.Forms.TabPage();
			this.tabContext = new System.Windows.Forms.TabControl();
			this.tabPageProperties = new System.Windows.Forms.TabPage();
			this.tabPageToolbox = new System.Windows.Forms.TabPage();
			this.propertyGridDesigner = new System.Windows.Forms.PropertyGrid();
			this.listToolbox = new System.Windows.Forms.ListView();
			this.tabPageHex = new System.Windows.Forms.TabPage();
			this.toolStripHex = new System.Windows.Forms.ToolStrip();
			this.hexBox = new Be.Windows.Forms.HexBox();
			this.toolStripButtonHexOpen = new System.Windows.Forms.ToolStripButton();
			this.contextMenuStripHex = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.toolStripMenuItemAsNumber = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemAsString = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemAsBlob = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemAsFlags = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMain.SuspendLayout();
			this.tabControlMain.SuspendLayout();
			this.tabPageDesigner.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.tabContext.SuspendLayout();
			this.tabPageProperties.SuspendLayout();
			this.tabPageToolbox.SuspendLayout();
			this.tabPageHex.SuspendLayout();
			this.toolStripHex.SuspendLayout();
			this.contextMenuStripHex.SuspendLayout();
			this.SuspendLayout();
			// 
			// statusStripMain
			// 
			this.statusStripMain.Location = new System.Drawing.Point(0, 533);
			this.statusStripMain.Name = "statusStripMain";
			this.statusStripMain.Size = new System.Drawing.Size(631, 22);
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
			this.toolStripMain.Size = new System.Drawing.Size(631, 25);
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
			this.tabControlMain.Size = new System.Drawing.Size(631, 508);
			this.tabControlMain.TabIndex = 2;
			// 
			// tabPageDesigner
			// 
			this.tabPageDesigner.Controls.Add(this.splitContainer1);
			this.tabPageDesigner.Location = new System.Drawing.Point(4, 22);
			this.tabPageDesigner.Name = "tabPageDesigner";
			this.tabPageDesigner.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageDesigner.Size = new System.Drawing.Size(623, 482);
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
			this.splitContainer1.Panel2.Controls.Add(this.tabContext);
			this.splitContainer1.Size = new System.Drawing.Size(617, 476);
			this.splitContainer1.SplitterDistance = 320;
			this.splitContainer1.TabIndex = 0;
			// 
			// treeViewPit
			// 
			this.treeViewPit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeViewPit.ImageIndex = 0;
			this.treeViewPit.ImageList = this.imageListMain;
			this.treeViewPit.Location = new System.Drawing.Point(0, 0);
			this.treeViewPit.Name = "treeViewPit";
			treeNode1.Name = "Node0";
			treeNode1.Text = "Peach";
			this.treeViewPit.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1});
			this.treeViewPit.SelectedImageIndex = 0;
			this.treeViewPit.Size = new System.Drawing.Size(320, 476);
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
			// tabPageEditor
			// 
			this.tabPageEditor.Location = new System.Drawing.Point(4, 22);
			this.tabPageEditor.Name = "tabPageEditor";
			this.tabPageEditor.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageEditor.Size = new System.Drawing.Size(623, 482);
			this.tabPageEditor.TabIndex = 1;
			this.tabPageEditor.Text = "Editor";
			this.tabPageEditor.UseVisualStyleBackColor = true;
			// 
			// tabContext
			// 
			this.tabContext.Alignment = System.Windows.Forms.TabAlignment.Bottom;
			this.tabContext.Controls.Add(this.tabPageProperties);
			this.tabContext.Controls.Add(this.tabPageToolbox);
			this.tabContext.Controls.Add(this.tabPageHex);
			this.tabContext.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabContext.Location = new System.Drawing.Point(0, 0);
			this.tabContext.Multiline = true;
			this.tabContext.Name = "tabContext";
			this.tabContext.SelectedIndex = 0;
			this.tabContext.Size = new System.Drawing.Size(293, 476);
			this.tabContext.TabIndex = 0;
			// 
			// tabPageProperties
			// 
			this.tabPageProperties.Controls.Add(this.propertyGridDesigner);
			this.tabPageProperties.Location = new System.Drawing.Point(4, 4);
			this.tabPageProperties.Name = "tabPageProperties";
			this.tabPageProperties.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageProperties.Size = new System.Drawing.Size(285, 450);
			this.tabPageProperties.TabIndex = 0;
			this.tabPageProperties.Text = "Properties";
			this.tabPageProperties.UseVisualStyleBackColor = true;
			// 
			// tabPageToolbox
			// 
			this.tabPageToolbox.Controls.Add(this.listToolbox);
			this.tabPageToolbox.Location = new System.Drawing.Point(4, 4);
			this.tabPageToolbox.Name = "tabPageToolbox";
			this.tabPageToolbox.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageToolbox.Size = new System.Drawing.Size(285, 450);
			this.tabPageToolbox.TabIndex = 1;
			this.tabPageToolbox.Text = "Toolbox";
			this.tabPageToolbox.UseVisualStyleBackColor = true;
			// 
			// propertyGridDesigner
			// 
			this.propertyGridDesigner.Dock = System.Windows.Forms.DockStyle.Fill;
			this.propertyGridDesigner.Location = new System.Drawing.Point(3, 3);
			this.propertyGridDesigner.Name = "propertyGridDesigner";
			this.propertyGridDesigner.Size = new System.Drawing.Size(279, 444);
			this.propertyGridDesigner.TabIndex = 1;
			// 
			// listToolbox
			// 
			this.listToolbox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listToolbox.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem9,
            listViewItem10,
            listViewItem11,
            listViewItem12});
			this.listToolbox.Location = new System.Drawing.Point(3, 3);
			this.listToolbox.MultiSelect = false;
			this.listToolbox.Name = "listToolbox";
			this.listToolbox.Size = new System.Drawing.Size(279, 444);
			this.listToolbox.TabIndex = 0;
			this.listToolbox.UseCompatibleStateImageBehavior = false;
			this.listToolbox.View = System.Windows.Forms.View.List;
			// 
			// tabPageHex
			// 
			this.tabPageHex.Controls.Add(this.hexBox);
			this.tabPageHex.Controls.Add(this.toolStripHex);
			this.tabPageHex.Location = new System.Drawing.Point(4, 4);
			this.tabPageHex.Name = "tabPageHex";
			this.tabPageHex.Size = new System.Drawing.Size(285, 450);
			this.tabPageHex.TabIndex = 2;
			this.tabPageHex.Text = "Hex";
			this.tabPageHex.UseVisualStyleBackColor = true;
			// 
			// toolStripHex
			// 
			this.toolStripHex.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonHexOpen});
			this.toolStripHex.Location = new System.Drawing.Point(0, 0);
			this.toolStripHex.Name = "toolStripHex";
			this.toolStripHex.Size = new System.Drawing.Size(285, 25);
			this.toolStripHex.TabIndex = 0;
			this.toolStripHex.Text = "toolStrip1";
			// 
			// hexBox
			// 
			this.hexBox.ContextMenuStrip = this.contextMenuStripHex;
			this.hexBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.hexBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.hexBox.HexCasing = Be.Windows.Forms.HexCasing.Lower;
			this.hexBox.LineInfoForeColor = System.Drawing.Color.Empty;
			this.hexBox.LineInfoVisible = true;
			this.hexBox.Location = new System.Drawing.Point(0, 25);
			this.hexBox.Name = "hexBox";
			this.hexBox.ReadOnly = true;
			this.hexBox.ShadowSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(60)))), ((int)(((byte)(188)))), ((int)(((byte)(255)))));
			this.hexBox.Size = new System.Drawing.Size(285, 425);
			this.hexBox.StringViewVisible = true;
			this.hexBox.TabIndex = 1;
			this.hexBox.VScrollBarVisible = true;
			// 
			// toolStripButtonHexOpen
			// 
			this.toolStripButtonHexOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonHexOpen.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonHexOpen.Image")));
			this.toolStripButtonHexOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonHexOpen.Name = "toolStripButtonHexOpen";
			this.toolStripButtonHexOpen.Size = new System.Drawing.Size(23, 22);
			this.toolStripButtonHexOpen.Text = "toolStripButton1";
			this.toolStripButtonHexOpen.ToolTipText = "Open File in Hex View";
			this.toolStripButtonHexOpen.Click += new System.EventHandler(this.toolStripButtonHexOpen_Click);
			// 
			// contextMenuStripHex
			// 
			this.contextMenuStripHex.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemAsNumber,
            this.toolStripMenuItemAsString,
            this.toolStripMenuItemAsBlob,
            this.toolStripMenuItemAsFlags});
			this.contextMenuStripHex.Name = "contextMenuStripHex";
			this.contextMenuStripHex.Size = new System.Drawing.Size(135, 92);
			// 
			// toolStripMenuItemAsNumber
			// 
			this.toolStripMenuItemAsNumber.Name = "toolStripMenuItemAsNumber";
			this.toolStripMenuItemAsNumber.Size = new System.Drawing.Size(134, 22);
			this.toolStripMenuItemAsNumber.Text = "As Number";
			// 
			// toolStripMenuItemAsString
			// 
			this.toolStripMenuItemAsString.Name = "toolStripMenuItemAsString";
			this.toolStripMenuItemAsString.Size = new System.Drawing.Size(134, 22);
			this.toolStripMenuItemAsString.Text = "As String";
			// 
			// toolStripMenuItemAsBlob
			// 
			this.toolStripMenuItemAsBlob.Name = "toolStripMenuItemAsBlob";
			this.toolStripMenuItemAsBlob.Size = new System.Drawing.Size(134, 22);
			this.toolStripMenuItemAsBlob.Text = "As Blob";
			// 
			// toolStripMenuItemAsFlags
			// 
			this.toolStripMenuItemAsFlags.Name = "toolStripMenuItemAsFlags";
			this.toolStripMenuItemAsFlags.Size = new System.Drawing.Size(134, 22);
			this.toolStripMenuItemAsFlags.Text = "As Flags";
			// 
			// FormMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(631, 555);
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
			this.tabContext.ResumeLayout(false);
			this.tabPageProperties.ResumeLayout(false);
			this.tabPageToolbox.ResumeLayout(false);
			this.tabPageHex.ResumeLayout(false);
			this.tabPageHex.PerformLayout();
			this.toolStripHex.ResumeLayout(false);
			this.toolStripHex.PerformLayout();
			this.contextMenuStripHex.ResumeLayout(false);
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
		private System.Windows.Forms.ImageList imageListMain;
		private System.Windows.Forms.TabControl tabContext;
		private System.Windows.Forms.TabPage tabPageProperties;
		private System.Windows.Forms.PropertyGrid propertyGridDesigner;
		private System.Windows.Forms.TabPage tabPageToolbox;
		private System.Windows.Forms.ListView listToolbox;
		private System.Windows.Forms.TabPage tabPageHex;
		private Be.Windows.Forms.HexBox hexBox;
		private System.Windows.Forms.ContextMenuStrip contextMenuStripHex;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemAsNumber;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemAsString;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemAsBlob;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemAsFlags;
		private System.Windows.Forms.ToolStrip toolStripHex;
		private System.Windows.Forms.ToolStripButton toolStripButtonHexOpen;
	}
}

