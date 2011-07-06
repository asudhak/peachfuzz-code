using System;
using System.Collections.Generic;
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
		public MainWindow()
		{
			InitializeComponent();

			this.Title = "Peach Build v3 DEV - template.xml";

			DynamicFileByteProvider dynamicFileByteProvider;
			dynamicFileByteProvider = new DynamicFileByteProvider(@"c:\4-Key.png");
			TheHexBox.ByteProvider = dynamicFileByteProvider;
			TheHexBox.HexCasing = HexCasing.Lower;
			TheHexBox.LineInfoVisible = true;
			TheHexBox.StringViewVisible = true;
			TheHexBox.UseFixedBytesPerLine = true;
			TheHexBox.VScrollBarVisible = true;

			xmlEditor.Document.LoadFile(File.OpenRead(@"c:\peach\template.xml"), Encoding.UTF8);

			DesignDataElementModel model = new DesignDataElementModel();
			model.DataElement = new Peach.Core.Dom.Block("DataModel");
			model.IconName = "/Icons/node-template.png";

			for (int i = 0; i < 5; i++)
			{
				DesignDataElementModel child = new DesignDataElementModel();
				child.DataElement = new Peach.Core.Dom.String("String #"+i.ToString());
				child.IconName = "/Icons/node-string.png";
				model.Children.Add(child);
			}

			var models = new List<DesignDataElementModel>();
			models.Add(model);
			DesignerTreeView.ItemsSource = models;
		}

		private void DesignerTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			DesignPropertyGrid.Items.Clear();
			DesignDataElementModel model = (DesignDataElementModel)DesignerTreeView.SelectedItem;
			foreach (Attribute attribute in model.DataElement.GetType().GetCustomAttributes(true))
			{
				if (attribute is ParameterAttribute)
				{
					var item = new PropertyGridPropertyItem();
					item.Name = ((ParameterAttribute)attribute).name;
					item.ValueName = ((ParameterAttribute)attribute).name;
					item.Description = ((ParameterAttribute)attribute).description;
					item.ValueType = ((ParameterAttribute)attribute).type;

					DesignPropertyGrid.Items.Add(item);
				}
			}
		}
	}
}
