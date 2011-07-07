using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Peach.Core.Dom;
using Peach.Core.IO;
using PeachBuilder.Controls;

namespace PeachBuilder.Models
{
	public class CrackModel : Model, ITreeModel
	{
		public static CrackModel Root = null;

		public CrackModel(DataElement element, int position, int length)
		{
			DataElement = element;
			Position = position;
			Length = length;
		}

		public string Name
		{
			get { return DataElement.name; }
		}

		public string IconName
		{
			get
			{
				return "/Icons/node-" + DataElement.GetType().Name.ToLower() + ".png";
			}
		}

		public DataElement DataElement
		{
			get;
			set;
		}

		public int Position
		{
			get;
			set;
		}

		public int Length
		{
			get;
			set;
		}

		public string Value
		{
			get
			{
				if (DataElement is DataElementContainer)
					return "";

				try
				{
					string ret = (string)this.DataElement.InternalValue;
					return ret.Length > 20 ? ret.Substring(0, 20) : ret;
				}
				catch
				{
					string ret = ASCIIEncoding.ASCII.GetString((byte[])this.DataElement.InternalValue);
					return ret.Length > 20 ? ret.Substring(0, 20) : ret;
				}
			}
		}

		protected ObservableCollection<CrackModel> children = new ObservableCollection<CrackModel>();
		public ObservableCollection<CrackModel> Children
		{
			get { return children; }
			set { children = value; }
		}

		#region ITreeModel Members

		public System.Collections.IEnumerable GetChildren(object parent)
		{
			if (parent == null)
				return new CrackModel[] { Root };

			return ((CrackModel)parent).Children;
		}

		public bool HasChildren(object parent)
		{
			if (parent == null)
				return true;

			return ((CrackModel)parent).Children.Count > 0;
		}

		#endregion
	}
}
