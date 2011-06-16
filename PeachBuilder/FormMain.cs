using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Be.Windows.Forms;

namespace PeachBuilder
{
	public partial class FormMain : Form
	{
		public FormMain()
		{
			InitializeComponent();


		}

		private void toolStripButtonMainNew_Click(object sender, EventArgs e)
		{

		}

		private void toolStripButtonHexOpen_Click(object sender, EventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
				return;

			DynamicFileByteProvider provider = new DynamicFileByteProvider(dialog.OpenFile());
			hexBox.ByteProvider = provider;
		}
	}
}
