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

		public static CrackModel CreateModelFromPit(DataModel dataModel)
		{
			CrackModel model = new CrackModel();
			model.Root = BuildFromElement(model, dataModel);

			return model;
		}

		public static CrackNode BuildFromElement(CrackModel model, DataElementContainer container)
		{
			CrackNode node = new CrackNode(model, container, 0, 0);

			if (container is Choice)
			{
				foreach (var child in ((Choice)container).choiceElements.Values)
				{
					if (child is DataElementContainer)
					{
						var childNode = BuildFromElement(model, child as DataElementContainer);
						childNode.Parent = node;
						node.Children.Add(childNode);
					}
					else
					{
						var childNode = new CrackNode(model, child, 0, 0);
						childNode.Parent = node;
						node.Children.Add(childNode);
					}
				}
			}

			foreach (var child in container)
			{
				if (child is DataElementContainer)
				{
					var childNode = BuildFromElement(model, child as DataElementContainer);
					childNode.Parent = node;
					node.Children.Add(childNode);
				}
				else
				{
					var childNode = new CrackNode(model, child, 0, 0);
					childNode.Parent = node;
					node.Children.Add(childNode);
				}
			}

			return node;
		}
	}

	public class CrackNode
	{
		public CrackNode(CrackModel model, DataElement element, long startBits, long stopBits)
		{
			Model = model;
			DataElement = element;
			StartBits = startBits;
			StopBits = stopBits;
			Error = false;
		}

		public bool RelativeToParent
		{
			get;
			set;
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
				var asm = System.Reflection.Assembly.GetEntryAssembly();

				var strm = asm.GetManifestResourceStream(IconName);
				if (strm == null)
					strm = asm.GetManifestResourceStream("PeachValidator.icons.node-unknown.png");
				return new Bitmap(strm);

			}
		}

		public CrackNode Parent
		{
			get;
			set;
		}

		public DataElement DataElement
		{
			get;
			set;
		}

		public long StartBits
		{
			get;
			set;
		}

		public long StopBits
		{
			get;
			set;
		}

		public int Position
		{
			get { return (int)(StartBits / 8); }
		}

		public int Length
		{
			get { return StopBits == 0 ? 0 : (int)((StopBits - StartBits + 7) / 8); }
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
				return DataElement.DefaultValue == null ? "" : DataElement.DefaultValue.ToString();
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
