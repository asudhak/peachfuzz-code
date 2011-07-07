using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Be.Windows.Forms;
using PeachBuilder.Models;
using Peach.Core.Dom;
using Peach.Core;
using Peach.Core.Cracker;
using Peach.Core.IO;
using Peach.Core.Analyzers;
using System.Reflection;
using ActiproSoftware.Products.PropertyGrid;
using ActiproSoftware.Windows.Controls.PropertyGrid;

namespace PeachBuilder
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		CrackModel CrackRootModel = null;

		public MainWindow()
		{
			InitializeComponent();

			this.Title = "Peach Build v3 DEV - template.xml";

			byte[] buff;
			using (Stream sin = File.OpenRead(@"c:\4-Key.png"))
			{
				buff = new byte[sin.Length];
				sin.Read(buff, 0, buff.Length);
			}

			DynamicFileByteProvider dynamicFileByteProvider;
			dynamicFileByteProvider = new DynamicFileByteProvider(new MemoryStream(buff));
			TheHexBox.ByteProvider = dynamicFileByteProvider;

			PitParser parser = new PitParser();
			Dom dom = parser.asParser(new Dictionary<string, string>(), File.OpenRead(@"c:\peach3.0\peach\template.xml"));

			xmlEditor.Document.LoadFile(File.OpenRead(@"c:\peach3.0\peach\template.xml"), Encoding.UTF8);

			var models = new List<DesignModel>();
			models.Add(new DesignPeachModel(dom));
			DesignerTreeView.ItemsSource = models;

			DesignHexDataModelsCombo.ItemsSource = dom.dataModels.Values;

			BitStream data = new BitStream(buff);
			DataCracker cracker = new DataCracker();
			cracker.EnterHandleNodeEvent += new EnterHandleNodeEventHandler(cracker_EnterHandleNodeEvent);
			cracker.ExitHandleNodeEvent += new ExitHandleNodeEventHandler(cracker_ExitHandleNodeEvent);
			cracker.CrackData(dom.dataModels[1], data);

			CrackModel.Root = CrackRootModel;
			CrackTree.Model = CrackRootModel;

			DesignHexDataModelsCombo.Text = dom.dataModels[1].name;
		}

		protected Stack<CrackModel> containerStack = new Stack<CrackModel>();
		protected CrackModel currentModel = null;

		void cracker_ExitHandleNodeEvent(DataElement element, BitStream data)
		{
			if (element is DataElementContainer)
				currentModel = containerStack.Pop();

			currentModel.Length = ((BitStream)currentModel.DataElement.Value).LengthBytes;

			if(containerStack.Count > 0)
				containerStack.Peek().Children.Add(currentModel);
		}

		void cracker_EnterHandleNodeEvent(DataElement element, BitStream data)
		{
			currentModel = new CrackModel(element, data.TellBytes(), 0);

			if (element is DataElementContainer)
			{
				if (CrackRootModel == null)
					CrackRootModel = currentModel;

				containerStack.Push(currentModel);
			}
		}

		private void DesignerTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			DesignPropertyGrid.Items.Clear();

			if (!(DesignerTreeView.SelectedItem is DesignDataElementModel))
				return;

			DesignDataElementModel model = (DesignDataElementModel)DesignerTreeView.SelectedItem;
			foreach (Attribute attribute in model.DataElement.GetType().GetCustomAttributes(true))
			{
				if (attribute is ParameterAttribute)
				{
					var item = new PropertyGridPropertyItem();
					item.Name = ((ParameterAttribute)attribute).name;
					item.ValueName = ((ParameterAttribute)attribute).name;
					item.Value = GetValue(model.DataElement, ((ParameterAttribute)attribute).name);
					item.Description = ((ParameterAttribute)attribute).description;
					item.ValueType = ((ParameterAttribute)attribute).type;

					DesignPropertyGrid.Items.Add(item);
				}
			}
		}

		private string GetValue(DataElement elem, string property)
		{
			var pinfo = elem.GetType().GetProperty(property);
			if (pinfo == null)
				return "";

			return pinfo.GetValue(elem, new object[0]).ToString();
		}

		private void CrackTree_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count == 0)
				return;

			CrackModel model = (CrackModel) ((PeachBuilder.Controls.TreeNode)e.AddedItems[0]).Tag;
			TheHexBox.Select(model.Position, model.Length);
		}
	}
}
