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
using Peach.Core.Dom;
using Peach.Core;
using Peach.Core.Cracker;
using Peach.Core.IO;
using Peach.Core.Analyzers;
using System.Reflection;

namespace PeachValidator
{
	public partial class MainForm : Form
	{
		string windowTitle = "Peach Validator v3.0 - {0}";
		string sampleFileName = null;
		string pitFileName = null;
		string dataModel = null;
		CrackModel crackModel = new CrackModel();
		Dictionary<DataElement, CrackNode> crackMap = new Dictionary<DataElement, CrackNode>();

		public MainForm()
		{
			InitializeComponent();
		}

		private void toolStripButtonOpenSample_Click(object sender, EventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			if (ofd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
				return;

			sampleFileName = ofd.FileName;

			DynamicFileByteProvider dynamicFileByteProvider;
			dynamicFileByteProvider = new DynamicFileByteProvider(File.OpenRead(sampleFileName));
			hexBox1.ByteProvider = dynamicFileByteProvider;
		}

		private void toolStripButtonRefreshSample_Click(object sender, EventArgs e)
		{
			try
			{
				if (string.IsNullOrEmpty(dataModel) || string.IsNullOrEmpty(sampleFileName) || string.IsNullOrEmpty(pitFileName))
					return;

				byte[] buff;
				using (Stream sin = File.OpenRead(sampleFileName))
				{
					buff = new byte[sin.Length];
					sin.Read(buff, 0, buff.Length);
				}
				
				PitParser parser;
				Dom dom;

				parser = new PitParser();
				dom = parser.asParser(new Dictionary<string, object>(), pitFileName);

				treeViewAdv1.BeginUpdate();
				treeViewAdv1.Model = null;
				crackModel = new CrackModel();
				
				try
				{
					BitStream data = new BitStream(buff);
					DataCracker cracker = new DataCracker();
					cracker.EnterHandleNodeEvent += new EnterHandleNodeEventHandler(cracker_EnterHandleNodeEvent);
					cracker.ExitHandleNodeEvent += new ExitHandleNodeEventHandler(cracker_ExitHandleNodeEvent);
					cracker.CrackData(dom.dataModels[dataModel], data);
				}
				catch
				{
					crackMap[dom.dataModels[dataModel]].Error = true;
				}

				foreach (var node in crackMap.Values)
				{
					if(node.DataElement.parent != null)
						node.Parent = crackMap[node.DataElement.parent];
				}

				crackModel.Root = crackMap.Values.First().Root;
				treeViewAdv1.Model = crackModel;
				treeViewAdv1.EndUpdate();
				//treeViewAdv1.Refresh();

				// No longer needed
				crackMap.Clear();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error cracking file: " + ex.ToString());
			}
		}

		void cracker_ExitHandleNodeEvent(DataElement element, BitStream data)
		{
			var currentModel = crackMap[element];
			currentModel.Length = (int)((BitStream)currentModel.DataElement.Value).LengthBytes;

			if (element.parent != null && crackMap.ContainsKey(element.parent))
				crackMap[element.parent].Children.Add(currentModel);
			else
			{
				// TODO -- Need to handle this case!
			}
		}

		void cracker_EnterHandleNodeEvent(DataElement element, BitStream data)
		{
			crackMap[element] = new CrackNode(crackModel, element, (int)data.TellBytes(), 0);
		}

		private void toolStripButtonOpenPit_Click(object sender, EventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			if (ofd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
				return;

			pitFileName = ofd.FileName;

			toolStripButtonRefreshPit_Click(null, null);
		}

		private void toolStripButtonRefreshPit_Click(object sender, EventArgs e)
		{
			try
			{
				PitParser parser = new PitParser();
				Dom dom;

				dom = parser.asParser(new Dictionary<string, object>(), pitFileName);

				toolStripComboBoxDataModel.Items.Clear();
				foreach (var model in dom.dataModels.Keys)
					toolStripComboBoxDataModel.Items.Add(model);

				if(toolStripComboBoxDataModel.Items.Count > 0)
					toolStripComboBoxDataModel.SelectedIndex = 0;

				Text = string.Format(windowTitle, pitFileName);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error loading file: " + ex.ToString());
			}
		}

		private void toolStripComboBoxDataModel_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				dataModel = toolStripComboBoxDataModel.SelectedItem as string;
			}
			catch
			{
			}
		}

		private void treeViewAdv1_SelectionChanged(object sender, EventArgs e)
		{
			if (treeViewAdv1.SelectedNode == null)
				return;

			var node = (CrackNode)treeViewAdv1.SelectedNode.Tag;
			hexBox1.Select(node.Position, node.Length);
		}
	}
}
