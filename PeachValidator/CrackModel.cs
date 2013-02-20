using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aga.Controls.Tree;
using Peach.Core.Dom;

namespace PeachValidator
{
	public class CrackModel : ITreeModel
	{
		public CrackNode Root = null;

		public System.Collections.IEnumerable  GetChildren(TreePath treePath)
		{
			if (treePath.FullPath.Count() == 0)
				return new object[] { Root };

			return ((CrackNode)treePath.LastNode).Children;
		}

		public bool IsLeaf(TreePath treePath)
		{
			return ((CrackNode)treePath.LastNode).Children.Count == 0;
		}

		public void OnNodesChanged(CrackNode sender)
		{
			if (NodesChanged == null)
				return;

			NodesChanged(sender, new TreeModelEventArgs(sender.GetTreePath(), (object[])sender.Children.ToArray()));
		}

		public void OnNodesInserted(CrackNode sender)
		{
			if (NodesInserted == null)
				return;

			NodesInserted(sender, new TreeModelEventArgs(sender.GetTreePath(), (object[])sender.Children.ToArray()));
		}

		public void OnNodesRemoved(CrackNode sender)
		{
			if (NodesRemoved == null)
				return;

			NodesRemoved(sender, new TreeModelEventArgs(sender.GetTreePath(), (object[])sender.Children.ToArray()));
		}

		public void OnStructureChanged(CrackNode sender)
		{
			if (StructureChanged == null)
				return;

			StructureChanged(sender, new TreePathEventArgs(sender.GetTreePath()));
		}

		public event EventHandler<TreeModelEventArgs> NodesChanged;
		public event EventHandler<TreeModelEventArgs> NodesInserted;
		public event EventHandler<TreeModelEventArgs> NodesRemoved;
		public event EventHandler<TreePathEventArgs> StructureChanged;
	}

	public class CrackNode
	{
		public CrackNode(CrackModel model, DataElement element, int position, int length)
		{
			Model = model;
			DataElement = element;
			Position = position;
			Length = length;
			Error = false;
		}

		public bool RelativeToParent
		{
			get;
			set;
		}

		public void MarkChildrenRelativeToParent()
		{
			foreach (var child in Children)
			{
				child.RelativeToParent = true;
			}
		}

		public CrackNode Root
		{
			get
			{
				CrackNode root = this;
				while (root.Parent != null)
					root = root.Parent;

				return root;
			}
		}

		public CrackModel Model { get; set; }

		public string Name
		{
			get { return DataElement.name; }
		}

		public string IconName
		{
			get
			{
				if (Error)
					return "PeachValidator.icons.node-error.png";

				return "PeachValidator.icons.node-" + DataElement.GetType().Name.ToLower() + ".png";
			}
		}

		public Image Icon
		{
			get
			{
				return new Bitmap(
				  System.Reflection.Assembly.GetEntryAssembly().
					GetManifestResourceStream(IconName));

			}
		}

		protected CrackNode parent = null;
		public CrackNode Parent
		{
			get { return parent; }
			set
			{
				parent = value;

				if (Position == 0 && Length > 0 && Parent != null && Parent.Position > 0)
				{
					Parent.MarkChildrenRelativeToParent();
				}
			}
		}

		public DataElement DataElement
		{
			get;
			set;
		}

		protected int position;
		public int Position
		{
			get
			{
				if (RelativeToParent)
					return position + Parent.Position;

				return position;
			}

			set
			{
				position = value;

				if (Position == 0 && Parent != null && Parent.Position > 0)
				{
					Parent.MarkChildrenRelativeToParent();
				}
			}
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

		public List<CrackNode> Children = new List<CrackNode>();

		public TreePath GetTreePath()
		{
			List<object> parents = new List<object>();
			parents.Add(this);
			CrackNode p = Parent;

			while (p != null)
			{
				parents.Add(p);
				p = p.Parent;
			}

			parents.Reverse();

			return new TreePath((object[])parents.ToArray());
		}

		public void AddChild(CrackNode child)
		{
			child.Parent = this;
			Children.Add(child);

			Model.OnNodesChanged(this);
		}

		public void RemoveChild(CrackNode child)
		{
			Children.Remove(child);
			Model.OnNodesRemoved(this);
		}

		public System.Collections.IEnumerable GetChildren(TreePath treePath)
		{
			return Children;
		}

	}
}
