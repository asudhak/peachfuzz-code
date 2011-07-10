using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace PeachBuilder.Models
{
	public class DesignModel : Model
	{
		protected ObservableCollection<DesignModel> _children = new ObservableCollection<DesignModel>();

		protected bool _isDragable = false;
		public bool isDragable
		{
			get { return _isDragable; }
			set { _isDragable = value; }
		}

		private bool isExpanded;
		public bool IsExpanded
		{
			get { return isExpanded; }
			set
			{
				isExpanded = value;
				RaisePropertyChanged("IsExpanded");
			}
		}

		private bool isSelected;
		public bool IsSelected
		{
			get { return isSelected; }
			set
			{
				isSelected = value;
				RaisePropertyChanged("IsSelected");
			}
		}

		public virtual bool HasItems
		{
			get { return _children.Count > 0; }
		}

		protected string _image = "/Icons/node-unknown.png";
		public string IconName
		{
			get
			{
				return _image;
			}
			set
			{
				_image = value;
				RaisePropertyChanged("IconName");
			}
		}

		public virtual ObservableCollection<DesignModel> Children
		{
			get { return _children; }
			set
			{
				_children = value;
				RaisePropertyChanged("Children");
			}
		}
	}
}
