using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Xml;
using System.Windows.Forms;
using Peach.Core.Xml;

namespace PeachXmlGenerator
{
	public partial class FormMain : Form
	{
		public FormMain()
		{
			InitializeComponent();
		}

		private void buttonDtdBrowse_Click(object sender, EventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Title = "Locate DTD File";
			dialog.DefaultExt = ".dtd";

			if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
				return;

			textBoxDtd.Text = dialog.FileName;
		}

		private void buttonSamplesBrowse_Click(object sender, EventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Title = "Locate Sample Files";
			dialog.DefaultExt = ".xml";

			if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
				return;

			textBoxSamples.Text = dialog.FileName.Substring(0, dialog.FileName.LastIndexOf("\\"));
		}

		private void buttonGenerate_Click(object sender, EventArgs e)
		{
			Generate dialog = new Generate();
			dialog.Show();

			XmlDocument doc = null;
			string dtdFile = textBoxDtd.Text;
			string rootElementName = string.IsNullOrEmpty(textBoxRootElement.Text) ? null : textBoxRootElement.Text;
			string samplesFolder = string.IsNullOrEmpty(textBoxSamples.Text) ? null : textBoxSamples.Text;
			int? iterations = int.Parse(textBoxCount.Text);
			string xmlns = string.IsNullOrEmpty(textBoxRootNamespace.Text) ? null : textBoxRootNamespace.Text;

			Console.WriteLine(" * Using DTD '" + dtdFile + "'.");
			Console.WriteLine(" * Root element '" + rootElementName + "'.");

			TextReader reader = new StreamReader(dtdFile);
			Parser parser = new ParserDtd();
			parser.parse(reader);

			if (samplesFolder != null)
			{
				Console.Write(" * Loading Samples from '" + samplesFolder + "'...");
				Defaults defaults = new Defaults(parser.elements, true);
				defaults.ProcessFolder(samplesFolder);
				Console.WriteLine("done.");
			}

			Generator generator = new Generator(parser.elements[rootElementName], parser.elements);

			Console.Write(" * Generating XML files...");

			dialog.progressBarGenerate.Maximum = (int)iterations;
			for (int i = 0; true; i++)
			{
				if (iterations != null && i >= iterations)
					break;
				dialog.progressBarGenerate.Value = i;

				if (i % 100 == 0)
					Console.Write(".");

				doc = generator.GenerateXmlDocument();

				if (xmlns != null)
					doc.DocumentElement.Attributes.Append(CreateXmlAttribute(doc, "xmlns", xmlns));

				if (File.Exists("fuzzed-" + i.ToString() + ".svg"))
					File.Delete("fuzzed-" + i.ToString() + ".svg");

				using (FileStream sout = File.OpenWrite("fuzzed-" + i.ToString() + ".svg"))
				{
					using (StreamWriter tout = new StreamWriter(sout))
					{
						tout.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
						tout.WriteLine(doc.OuterXml);
					}
				}
			}

			Console.WriteLine("done.\n");
			dialog.Hide();

		}
		protected static XmlAttribute CreateXmlAttribute(XmlDocument doc, string name, string value)
		{
			XmlAttribute a = doc.CreateAttribute(name);
			a.InnerText = value;
			return a;
		}

	}
}
