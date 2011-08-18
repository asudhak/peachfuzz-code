namespace PeachXmlGenerator
{
	partial class Generate
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
			this.progressBarGenerate = new System.Windows.Forms.ProgressBar();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// progressBarGenerate
			// 
			this.progressBarGenerate.Location = new System.Drawing.Point(12, 12);
			this.progressBarGenerate.Name = "progressBarGenerate";
			this.progressBarGenerate.Size = new System.Drawing.Size(260, 23);
			this.progressBarGenerate.TabIndex = 0;
			// 
			// buttonCancel
			// 
			this.buttonCancel.Location = new System.Drawing.Point(105, 41);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonCancel.TabIndex = 1;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// Generate
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 72);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.progressBarGenerate);
			this.Name = "Generate";
			this.Text = "Generate XML Files";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button buttonCancel;
		public System.Windows.Forms.ProgressBar progressBarGenerate;
	}
}