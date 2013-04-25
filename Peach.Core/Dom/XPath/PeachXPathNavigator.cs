using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.XPath;

using NLog;

using Peach.Core;
using Peach.Core.IO;
using Peach.Core.Dom;

namespace Peach.Core.Dom.XPath
{
	/// <summary>
	/// Create an XPath Navigator for Peach DOM objects.
	/// </summary>
	/// <remarks>
	/// The XPath query syntax is the purfect way to select nodes
	/// from a Peach DOM.  By implementing an XPathNavigator we 
	/// should beable to use the built in .NET XPath system with
	/// our Peach DOM.
	/// 
	/// This XPath navigator will only search root -> run -> test -> stateModel -> States* -> Actions* -> DataModels*.
	/// </remarks>
	public class PeachXPathNavigator : XPathNavigator
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		protected List<object> moveToHistory = new List<object>();

		/// <summary>
		/// Attributes for each known type
		/// </summary>
		/// <remarks>
		/// List of property names that we will expose as "attributes"
		/// for the xpath expressions.
		/// </remarks>
		protected static Dictionary<Type, string[]> AttributeMatrix = new Dictionary<Type, string[]>();

		/// <summary>
		/// Map between Type and PeachXPathNodeType
		/// </summary>
		protected static Dictionary<Type, PeachXPathNodeType> NodeTypeMap = new Dictionary<Type, PeachXPathNodeType>();

		/// <summary>
		/// The Peach DOM we are navigating.
		/// </summary>
		public Dom dom;

		/// <summary>
		/// The current node/position in the dom.
		/// </summary>
		public object currentNode;

		/// <summary>
		/// Type of current node.
		/// </summary>
		public PeachXPathNodeType currentNodeType;

		/// <summary>
		/// Current attribute index.
		/// </summary>
		protected int attributeIndex = 0;

		/// <summary>
		/// Current test index.
		/// </summary>
		protected int testIndex = 0;

		/// <summary>
		/// Are we iterating attributes?
		/// </summary>
		protected bool iteratingAttributes = false;

		static PeachXPathNavigator()
		{
			AttributeMatrix[typeof(Dom)] = new string[] { "name" };
			AttributeMatrix[typeof(DataElement)] = new string[] { "name", "isMutable", "isToken", "length" };
			AttributeMatrix[typeof(StateModel)] = new string[] { "name" };
			AttributeMatrix[typeof(State)] = new string[] { "name" };
			AttributeMatrix[typeof(Action)] = new string[] { "name", "type", "method", "property" };
			AttributeMatrix[typeof(Test)] = new string[] { "name" };

			NodeTypeMap[typeof(Dom)] = PeachXPathNodeType.Root;
			NodeTypeMap[typeof(DataElement)] = PeachXPathNodeType.DataModel;
			NodeTypeMap[typeof(StateModel)] = PeachXPathNodeType.StateModel;
			NodeTypeMap[typeof(State)] = PeachXPathNodeType.StateModel;
			NodeTypeMap[typeof(Action)] = PeachXPathNodeType.StateModel;
			NodeTypeMap[typeof(Test)] = PeachXPathNodeType.Test;
		}

		protected PeachXPathNodeType MapObjectToNodeType(object obj)
		{
			foreach(Type key in NodeTypeMap.Keys)
			{
				if (key.IsInstanceOfType(obj))
					return NodeTypeMap[key];
			}

			throw new ArgumentException("Object is of unknown type.");
		}

		public PeachXPathNavigator(Dom dom)
		{
			currentNode = dom;
			currentNodeType = PeachXPathNodeType.Root;
		}

		protected PeachXPathNavigator(Dom dom, object currentNode, PeachXPathNodeType currentNodeType, 
			int attributeIndex, bool iteratingAttributes)
		{
			this.dom = dom;
			this.currentNode = currentNode;
			this.currentNodeType = currentNodeType;
			this.attributeIndex = attributeIndex;
			this.iteratingAttributes = iteratingAttributes;
		}

		#region Abstract XPathNavigator

		public override string BaseURI
		{
			get { return string.Empty; }
		}

		public override XPathNavigator Clone()
		{
			return new PeachXPathNavigator(dom, currentNode, currentNodeType, attributeIndex, iteratingAttributes);
		}

		public override bool IsEmptyElement
		{
			get { return false; }
		}

		public override bool IsSamePosition(XPathNavigator other)
		{
			if (!(other is PeachXPathNavigator))
				return false;

			var otherXpath = other as PeachXPathNavigator;
			return (otherXpath.dom == dom && 
				otherXpath.currentNode == currentNode && 
				otherXpath.attributeIndex == attributeIndex);
		}

		public override string LocalName
		{
			get
			{
				if (iteratingAttributes)
					return GetCurrentNodeAttributeMatrix().ElementAt(attributeIndex);

				return ((INamed)currentNode).name.Split(':').Last();
			}
		}

		public override bool MoveTo(XPathNavigator other)
		{
			logger.Trace("MoveTo");

			var otherXpath = other as PeachXPathNavigator;
			if(otherXpath == null)
				return false;

			this.dom = otherXpath.dom;
			this.currentNode = otherXpath.currentNode;
			this.currentNodeType = otherXpath.currentNodeType;
			this.attributeIndex = otherXpath.attributeIndex;

			return true;
		}

		public override bool MoveToFirstAttribute()
		{
			logger.Trace("MoveToFirstAttribute");

			iteratingAttributes = true;
			attributeIndex = 0;
			return true;
		}

		public override bool MoveToFirstChild()
		{
			logger.Trace("MoveToFirstChild(" + ((INamed)currentNode).name + ")");

			if (currentNode is DataElementContainer)
			{
				var container = currentNode as DataElementContainer;
				if (container.Count == 0)
					return false;

				currentNode = container[0];
				return true;
			}
			else if (currentNode is DataElement)
			{
				return false;
			}
			else if (currentNode is Dom)
			{
				var dom = currentNode as Dom;

				if (dom.tests.Count > 0)
				{
					testIndex = 0;
					currentNode = dom.tests[0];
					currentNodeType = PeachXPathNodeType.Test;
					return true;
				}

				return false;
			}
			else if (currentNode is StateModel)
			{
				var stateModel = currentNode as StateModel;

				if (stateModel.states.Count == 0)
					return false;

				currentNode = stateModel.states.Values.First();
				return true;
			}
			else if (currentNode is State)
			{
				var state = currentNode as State;
				if (state.actions.Count == 0)
					return false;

				currentNode = state.actions[0];
				return true;
			}
			else if (currentNode is Action)
			{
				var action = currentNode as Action;
				if (action.dataModel != null)
				{
					currentNode = action.dataModel;
					currentNodeType = PeachXPathNodeType.DataModel;
					return true;
				}

				if (action.parameters.Count == 0)
					return false;

				currentNode = action.parameters[0].dataModel;
				currentNodeType = PeachXPathNodeType.DataModel;
				return true;
			}
			else if (currentNode is Test)
			{
				var test = currentNode as Test;
				if (test.stateModel == null)
					return false;

				currentNode = test.stateModel;
				currentNodeType = PeachXPathNodeType.StateModel;
				return true;
			}

			throw new ArgumentException("Error, unknown type");
		}

		public override bool MoveToFirstNamespace(XPathNamespaceScope namespaceScope)
		{
			logger.Trace("MoveToFirstNamespace");

			return false;
		}

		public override bool MoveToId(string id)
		{
			logger.Trace("MoveToId");

			return false;
		}

		public override bool MoveToNext()
		{
			logger.Trace("MoveToNext(" + ((INamed)currentNode).name + ")");

			if (currentNodeType == PeachXPathNodeType.Root)
				return false;

			dynamic obj = currentNode;
			object parent = obj.parent;

			if (parent == null)
			{
				if (currentNode is DataModel)
				{
					if (obj.dom != null)
						parent = obj.dom;
					else if (obj.action != null)
						parent = obj.action;
				}

				if(parent == null)
					throw new Exception("Error, parent was unexpectedly null for object '" +
						obj.name + "' of type " + currentNode.GetType().ToString() + ".");
			}
			// DataModel drives from Block, so if our parent is a DataElementContainer we are all good
			if (currentNode is DataModel && !(parent is DataElementContainer))
			{
				var action = parent as Action;
				if (action == null)
					throw new Exception("Error, data model has weird parent!");

				if (action.dataModel == currentNode)
				{
					if (action.parameters.Count == 0)
						return false;

					currentNode = action.parameters[0].dataModel;
					currentNodeType = PeachXPathNodeType.DataModel;
					return true;
				}

				int idx = action.parameters.FindIndex(a => a.dataModel == currentNode);
				if (idx == -1)
					throw new Exception("Error, data model missing from action parameters!");

				if (++idx >= action.parameters.Count)
					return false;

				currentNode = action.parameters[idx].dataModel;
				currentNodeType = PeachXPathNodeType.DataModel;
				return true;
			}
			else if (currentNode is DataElement)
			{
				if (parent is DataElementContainer)
				{
					var curr = currentNode as DataElement;
					var block = parent as DataElementContainer;
					int index = block.IndexOf(curr);

					for (int i = index + 1; i < block.Count; ++i)
					{
						var elem = block[i];
						if (elem != curr)
						{
							currentNode = elem;
							return true;
						}
					}

					return false;
				}
				return false;
			}
			else if (currentNode is StateModel)
			{
				return false;
			}
			else if (currentNode is State)
			{
				var stateModel = parent as StateModel;
				int index = 0;
				for (int cnt = 0; cnt < stateModel.states.Values.Count; cnt++)
				{
					if (stateModel.states.Values.ElementAt(cnt) == currentNode)
					{
						index = cnt;
						break;
					}
				}

				if (stateModel.states.Values.Count <= (index + 1))
					return false;

				currentNode = stateModel.states.Values.ElementAt(index + 1);
				return true;
			}
			else if (currentNode is Action)
			{
				var state = parent as State;
				int index = state.actions.IndexOf((Action)currentNode);
				if (state.actions.Count <= (index + 1))
					return false;

				currentNode = state.actions[index + 1];
				return true;
			}
			else if (currentNode is Test)
			{
				var dom = parent as Dom;
				int index = dom.tests.IndexOfKey(((INamed)currentNode).name);
				if (dom.tests.Count <= (index + 1))
					return false;

				currentNode = dom.tests[index + 1];
				testIndex = index + 1;
				return true;
			}

			throw new ArgumentException("Error, unknown type");
		}

		public override bool MoveToNextAttribute()
		{
			logger.Trace("MoveToNextAttribute");

			if (GetCurrentNodeAttributeMatrix().Length <= (attributeIndex + 1))
				return false;

			iteratingAttributes = true;
			attributeIndex++;
			return true;
		}

		protected string[] GetCurrentNodeAttributeMatrix()
		{
			foreach (Type key in AttributeMatrix.Keys)
			{
				if (key.IsInstanceOfType(currentNode))
					return AttributeMatrix[key];
			}

			return null;
		}

		public override bool MoveToNextNamespace(XPathNamespaceScope namespaceScope)
		{
			logger.Trace("MoveToNextNamespace");

			return false;
		}

		public override bool MoveToParent()
		{
			logger.Trace("MoveToParent(" +currentNode.GetType() + ":" + ((INamed)currentNode).name + ")");

			if (iteratingAttributes)
			{
				iteratingAttributes = false;
				return true;
			}

			if (currentNodeType == PeachXPathNodeType.Root)
				return false;

			dynamic obj = currentNode;

			// DataModel drives from Block, so if our parent is a DataElementContainer we are all good
			if (obj is DataModel && !(obj.parent is DataElementContainer))
			{
				if (obj.dom != null)
					currentNode = obj.dom;
				else if (obj.action != null)
					currentNode = obj.action;
				else
					throw new Exception("Error, data model with no dom/action parent!");
			}
			else if (obj is StateModel)
			{
				// state models have a parent of the dom, but we need to walk
				// back up to the test since that is how we descend
				Dom root = obj.parent as Dom;
				currentNode = root.tests[testIndex];
			}
			else
				currentNode = obj.parent;

			currentNodeType = MapObjectToNodeType(currentNode);

			return true;
		}

		public override bool MoveToPrevious()
		{
			logger.Trace("MoveToPrevious");

			throw new NotImplementedException();
		}

		public override string Name
		{
			get { return ((INamed)currentNode).name; }
		}

		public override System.Xml.XmlNameTable NameTable
		{
			get { throw new NotImplementedException(); }
		}

		public override string NamespaceURI
		{
			get
			{
				if (iteratingAttributes)
					return string.Empty;

				var parts = ((INamed)currentNode).name.Split(':');
				return parts.Length > 1 ? parts[0] : string.Empty;
			}
		}

		public override XPathNodeType NodeType
		{
			get
			{
				if (iteratingAttributes)
					return XPathNodeType.Attribute;

				if (currentNodeType == PeachXPathNodeType.Root)
					return XPathNodeType.Root;

				return XPathNodeType.Element;
			}
		}

		public override string Prefix
		{
			get
			{
				return string.Empty;
			}
		}

		public override string Value
		{
			get
			{
				if (!iteratingAttributes)
					return string.Empty;

				string attr = LocalName;

				if (attr == "name")
					return ((INamed)currentNode).name;

				if (currentNode is DataElement)
				{
					switch (attr)
					{
						case "isMutable": return ((DataElement)currentNode).isMutable.ToString();
						case "isToken": return ((DataElement)currentNode).isToken.ToString();
						case "length": return ((DataElement)currentNode).length.ToString();
					}
				}
				else if (currentNode is Action)
				{
					switch (attr)
					{
						case "type": return ((Action)currentNode).type.ToString();
						case "method": return ((Action)currentNode).method;
						case "property": return ((Action)currentNode).property;
					}
				}

				return string.Empty;
			}
		}

		public override string GetAttribute(string localName, string namespaceURI)
		{
			return string.Empty;
		}

		#endregion

		#region XPathItem

		public override object TypedValue
		{
			get
			{
				return base.TypedValue;
			}
		}

		public override object ValueAs(Type returnType)
		{
			return base.ValueAs(returnType);
		}

		public override object ValueAs(Type returnType, System.Xml.IXmlNamespaceResolver nsResolver)
		{
			return base.ValueAs(returnType, nsResolver);
		}

		public override bool ValueAsBoolean
		{
			get
			{
				return base.ValueAsBoolean;
			}
		}

		public override DateTime ValueAsDateTime
		{
			get
			{
				return base.ValueAsDateTime;
			}
		}

		public override double ValueAsDouble
		{
			get
			{
				return base.ValueAsDouble;
			}
		}

		public override int ValueAsInt
		{
			get
			{
				return base.ValueAsInt;
			}
		}

		public override long ValueAsLong
		{
			get
			{
				return base.ValueAsLong;
			}
		}

		public override Type ValueType
		{
			get
			{
				return base.ValueType;
			}
		}

		public override System.Xml.Schema.XmlSchemaType XmlType
		{
			get
			{
				return base.XmlType;
			}
		}

		#endregion
	}
}
