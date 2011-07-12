using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Peach.Core.Dom;
using Peach.Core.IO;
using PeachFuzzFactory.Controls;

namespace PeachFuzzFactory.Models
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
				if(Error)
					return "/Icons/node-error.png";

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

		public bool Error
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

				string ret;

				try
				{
					ret = (string)this.DataElement.InternalValue;
				}
				catch
				{
					ret = ASCIIEncoding.ASCII.GetString((byte[])this.DataElement.InternalValue);
				}

				for (int i = 0; i < ret.Length; i++)
				{
					if (ret[i].CompareTo(' ') < 0)
						ret = ret.Replace(ret[i], '.');

					if (ret[i].CompareTo('~') > 0)
						ret = ret.Replace(ret[i], '.');
				}

				return ret.Length > 20 ? ret.Substring(0, 20) : ret;
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
