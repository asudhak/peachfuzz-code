namespace Be.HexEditor
{
	partial class BitControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BitControl));
            this.lblValue = new System.Windows.Forms.Label();
            this.lblBit = new System.Windows.Forms.Label();
            this.pnBitsEditor = new System.Windows.Forms.Panel();
            this.pnBitsHeader = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // lblValue
            // 
            resources.ApplyResources(this.lblValue, "lblValue");
            this.lblValue.Name = "lblValue";
            // 
            // lblBit
            // 
            resources.ApplyResources(this.lblBit, "lblBit");
            this.lblBit.Name = "lblBit";
            // 
            // pnBitsEditor
            // 
            resources.ApplyResources(this.pnBitsEditor, "pnBitsEditor");
            this.pnBitsEditor.Name = "pnBitsEditor";
            // 
            // pnBitsHeader
            // 
            resources.ApplyResources(this.pnBitsHeader, "pnBitsHeader");
            this.pnBitsHeader.Name = "pnBitsHeader";
            // 
            // BitControl
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnBitsHeader);
            this.Controls.Add(this.pnBitsEditor);
            this.Controls.Add(this.lblValue);
            this.Controls.Add(this.lblBit);
            this.Name = "BitControl";
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label lblValue;
        private System.Windows.Forms.Label lblBit;
        private System.Windows.Forms.Panel pnBitsEditor;
        private System.Windows.Forms.Panel pnBitsHeader;
	}
}
