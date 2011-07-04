
//
// Copyright (c) Michael Eddington
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in	
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

// Authors:
//   Michael Eddington (mike@phed.org)

// $Id$

using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime;
using System.Reflection;
using System.Runtime.Serialization;
using Peach.Core.IO;

namespace Peach.Core.Dom
{
	public enum LengthType
	{
		String,
		Python,
		Ruby,
		Calc
	}

	public delegate void InvalidatedEventHandler(object sender, EventArgs e);
	public delegate void DefaultValueChangedEventHandler(object sender, EventArgs e);
	public delegate void MutatedValueChangedEventHandler(object sender, EventArgs e);

	/// <summary>
	/// Base class for all data elements.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[Serializable]
	public abstract class DataElement
	{
		/// <summary>
		/// Mutated vale override's fixupImpl
		///
		///  - Default Value
		///  - Relation
		///  - Fixup
		///  - Type contraints
		///  - Transformer
		/// </summary>
		public const uint MUTATE_OVERRIDE_FIXUP = 0x1;
		/// <summary>
		/// Mutated value overrides transformers
		/// </summary>
		public const uint MUTATE_OVERRIDE_TRANSFORMER = 0x2;
		/// <summary>
		/// Mutated value overrides type constraints (e.g. string length,
		/// null terminated, etc.)
		/// </summary>
		public const uint MUTATE_OVERRIDE_TYPE_CONSTRAINTS = 0x4;
		/// <summary>
		/// Mutated value overrides relations.
		/// </summary>
		public const uint MUTATE_OVERRIDE_RELATIONS = 0x8;
		/// <summary>
		/// Default mutate value
		/// </summary>
		public const uint MUTATE_DEFAULT = MUTATE_OVERRIDE_FIXUP;

		public string name;
		public bool isMutable = true;
		public uint mutationFlags = MUTATE_DEFAULT;
		public bool isToken = false;

		protected Dictionary<string, Hint> hints = new Dictionary<string, Hint>();

		protected bool _isReference = false;

		protected Variant _defaultValue;
		protected Variant _mutatedValue;

		protected RelationContainer _relations = null;
		protected Fixup _fixup = null;
		protected Transformer _transformer = null;

		protected DataElementContainer _parent;

		protected Variant _internalValue;
		protected BitStream _value;

		protected bool _invalidated = false;

		protected bool _hasLength = false;
		protected int _length = 0;
		protected LengthType _lengthType = LengthType.String;
		protected string _lengthOther = null;

		protected string _constraint = null;

		#region Events

		public event InvalidatedEventHandler Invalidated;
		public event DefaultValueChangedEventHandler DefaultValueChanged;
		public event MutatedValueChangedEventHandler MutatedValueChanged;

		protected virtual void OnInvalidated(EventArgs e)
		{
			// Cause values to be regenerated next time they are
			// requested.  We don't want todo this now as there could
			// be a series of invalidations that occur.
			_internalValue = null;
			_value = null;

            // Bubble this up the chain
            if(_parent != null)
                _parent.Invalidate();

			if (Invalidated != null)
				Invalidated(this, e);
		}

		protected virtual void OnDefaultValueChanged(EventArgs e)
		{
			if (DefaultValueChanged != null)
				DefaultValueChanged(this, e);
		}

		protected virtual void OnMutatedValueChanged(EventArgs e)
		{
			OnInvalidated(null);

			if (MutatedValueChanged != null)
				MutatedValueChanged(this, e);
		}

		#endregion

		public static OrderedDictionary<string, Type> dataElements = new OrderedDictionary<string, Type>();
		public static void loadDataElements(Assembly assembly)
		{
			foreach (Type type in assembly.GetTypes())
			{
				if (type.IsClass && !type.IsAbstract)
				{
					object [] attr = type.GetCustomAttributes(typeof(DataElementAttribute), false);
					DataElementAttribute dea = attr[0] as DataElementAttribute;
					if (!dataElements.ContainsKey(dea.elementName))
					{
						dataElements.Add(dea.elementName, type);
					}
				}
			}
		}

		static DataElement()
		{
		}

		protected static uint _uniqueName = 0;
		public DataElement()
		{
			name = "DataElement_" + _uniqueName;
			_uniqueName++;
			_relations = new RelationContainer(this);
		}

		public DataElement(string name)
		{
			this.name = name;
			_relations = new RelationContainer(this);
		}

		/// <summary>
		/// Full qualified name of DataElement to
		/// root DataElement.
		/// </summary>
		public string fullName
		{
			// TODO: Cache fullName if possible

			get
			{
				string fullname = name;
				DataElement obj = _parent;
				while (obj != null)
				{
					fullname = obj.name + "." + fullname;
					obj = obj.parent;
				}

				return fullname;
			}
		}

		public Dictionary<string, Hint> Hints
		{
			get { return hints; }
			set { hints = value; }
		}

		/// <summary>
		/// Constraint on value of data element.
		/// </summery>
		/// <remarks>
		/// This
		/// constraint is only enforced when loading data into
		/// the object.  It will not affect values that are
		/// produced during fuzzing.
		/// </remarks>
		public string constraint
		{
			get { return _constraint; }
			set { _constraint = value; }
		}

		/// <summary>
		/// Is this DataElement created by a 
		/// reference to another DataElement?
		/// </summary>
		public bool isReference
		{
			get { return _isReference; }
			set { _isReference = value; }
		}

		public DataElementContainer parent
		{
			get
			{
				return _parent;
			}
			set
			{
				_parent = value;
			}
		}

		public DataElement getRoot()
		{
			DataElement obj = this;

			while (obj != null && obj._parent != null)
				obj = obj.parent;

			return obj;
		}

		/// <summary>
		/// Find our next sibling.
		/// </summary>
		/// <returns>Returns sibling or null.</returns>
		public DataElement nextSibling()
		{
			if (_parent == null)
				return null;

			int nextIndex = _parent.IndexOf(this) + 1;
			if (nextIndex >= _parent.Count)
				return null;

			return _parent[nextIndex];
		}

		/// <summary>
		/// Find our previous sibling.
		/// </summary>
		/// <returns>Returns sibling or null.</returns>
		public DataElement previousSibling()
		{
			if (_parent == null)
				return null;

			int priorIndex = _parent.IndexOf(this) - 1;
			if (priorIndex < 0)
				return null;

			return _parent[priorIndex];
		}

		/// <summary>
		/// Call to invalidate current element and cause rebuilding
		/// of data elements dependent on this element.
		/// </summary>
		public void Invalidate()
		{
			_invalidated = true;

			OnInvalidated(null);

			if(parent != null)
				parent.Invalidate();
		}

		/// <summary>
		/// Is this a leaf of the DataModel tree?
		/// 
		/// True if DataElement has no children.
		/// </summary>
		public virtual bool isLeafNode
		{
			get { return true; }
		}

		/// <summary>
		/// Does element have a length?  This is
		/// separate from Relations.
		/// </summary>
		public virtual bool hasLength
		{
			get { return _hasLength; }
			set { _hasLength = value; }
		}

		public SizeRelation GetSizeRelation()
		{
			// TODO - Make this not suck

			foreach (DataElement elem in this.EnumerateElementsUpTree())
			{
				if (elem.relations != null)
				{
					foreach (Relation relation in elem.relations)
					{
						if (relation is SizeRelation)
							return relation as SizeRelation;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Length of element.  In the case that 
		/// LengthType == "Calc" we will evaluate the
		/// expression.
		/// </summary>
		public virtual int length
		{
			get
			{
				switch (_lengthType)
				{
					case LengthType.String:
						return _length;
					case LengthType.Calc:
						Dictionary<string, object> scope = new Dictionary<string,object>();
						scope["self"] = this;
						return (int)Scripting.EvalExpression(_lengthOther, scope);
					default:
						throw new NotSupportedException("Error calculating length.");
				}
			}
			set
			{
				_lengthType = LengthType.String;
				_length = value;
				_hasLength = true;
			}
		}

		/// <summary>
		/// Length expression.  This expression is used
		/// to calculate the length of this element.
		/// </summary>
		public virtual string lengthOther
		{
			get { return _lengthOther; }
			set { _lengthOther = value; }
		}

		public virtual LengthType lengthType
		{
			get { return _lengthType; }
			set { _lengthType = value; }
		}

		/// <summary>
		/// Default value for this data element.
		/// 
		/// Changing the default value will invalidate
		/// the model.
		/// </summary>
		public virtual Variant DefaultValue
		{
			get { return _defaultValue; }
			set
			{
				_defaultValue = value;
				OnDefaultValueChanged(null);
				Invalidate();
			}
		}

		/// <summary>
		/// Current mutated value (if any) for this data element.
		/// 
		/// Changing the MutatedValue will invalidate the model.
		/// </summary>
		public virtual Variant MutatedValue
		{
			get { return _mutatedValue; }
			set
			{
				_mutatedValue = value;
				OnMutatedValueChanged(null);
				Invalidate();
			}
		}

        /// <summary>
        /// Get the Internal Value of this data element
        /// </summary>
		public virtual Variant InternalValue
		{
			get
			{
				if (_internalValue == null || _invalidated)
					GenerateInternalValue();

				return _internalValue;
			}
		}

        /// <summary>
        /// Get the final Value of this data element
        /// </summary>
		public virtual BitStream Value
		{
			get
			{
				if (_value == null || _invalidated)
				{
					GenerateValue();
					_invalidated = false;
				}

				return _value;
			}
		}

		/// <summary>
		/// Generate the internal value of this data element
		/// </summary>
		/// <returns>Internal value in .NET form</returns>
		public virtual Variant GenerateInternalValue()
		{
			Variant value;

			// 1. Default value

			value = DefaultValue;

			// 2. Relations

			if (MutatedValue != null && (mutationFlags & MUTATE_OVERRIDE_RELATIONS) != 0)
			{
				_internalValue = MutatedValue;
				return MutatedValue;
			}

			foreach(Relation r in _relations)
			{
				if (r.Of != this)
					value = r.GetValue();
			}

			// 3. Fixup

			if (MutatedValue != null && (mutationFlags & MUTATE_OVERRIDE_FIXUP) != 0)
			{
				_internalValue = MutatedValue;
				return MutatedValue;
			}

			if (_fixup != null)
				value = _fixup.fixup(this);

			// 4. Set _internalValue

			_internalValue = value;
			return value;
		}

		protected virtual BitStream InternalValueToBitStream(Variant b)
		{
			if (b == null)
				return new BitStream();
			return (BitStream)b;
		}

		/// <summary>
		/// Generate the final value of this data element
		/// </summary>
		/// <returns></returns>
		public BitStream GenerateValue()
		{
			if (_mutatedValue != null && (mutationFlags & MUTATE_OVERRIDE_TRANSFORMER) != 0)
				return new BitStream((byte[]) MutatedValue);

			BitStream value = InternalValueToBitStream(InternalValue);

			if(_transformer != null)
				value = _transformer.encode(value);

			_value = value;
			return value;
		}

		/// <summary>
		/// Enumerates all DataElements starting from 'start.'
		/// 
		/// This method will first return children, then siblings, then children
		/// of siblings as it walks up the parent chain.  It will not return
		/// any duplicate elements.
		/// 
		/// Note: This is not the fastest way to enumerate all elements in the
		/// tree, it's specifically intended for findings Elements in a search
		/// pattern that matches a persons assumptions about name resolution.
		/// </summary>
		/// <param name="start">Starting DataElement</param>
		/// <returns>All DataElements in model.</returns>
		public static IEnumerable EnumerateAllElementsFromHere(DataElement start)
		{
			foreach(DataElement elem in EnumerateAllElementsFromHere(start, new List<DataElement>()))
				yield return elem;
		}

		/// <summary>
		/// Enumerates all DataElements starting from 'start.'
		/// 
		/// This method will first return children, then siblings, then children
		/// of siblings as it walks up the parent chain.  It will not return
		/// any duplicate elements.
		/// 
		/// Note: This is not the fastest way to enumerate all elements in the
		/// tree, it's specifically intended for findings Elements in a search
		/// pattern that matches a persons assumptions about name resolution.
		/// </summary>
		/// <param name="start">Starting DataElement</param>
		/// <param name="cache">Cache of DataElements already returned</param>
		/// <returns>All DataElements in model.</returns>
		public static IEnumerable EnumerateAllElementsFromHere(DataElement start, 
			List<DataElement> cache)
		{
			// Add ourselvs to the cache is not already done
			if (!cache.Contains(start))
				cache.Add(start);

			// 1. Enumerate all siblings

			if (start.parent != null)
			{
				foreach (DataElement elem in start.parent)
					if (!cache.Contains(elem))
						yield return elem;
			}

			// 2. Children

			foreach (DataElement elem in EnumerateChildrenElements(start, cache))
				yield return elem;

			// 3. Children of siblings

			if (start.parent != null)
			{
				foreach (DataElement elem in start.parent)
				{
					if (!cache.Contains(elem))
					{
						cache.Add(elem);
						foreach(DataElement ret in EnumerateChildrenElements(elem, cache))
							yield return ret;
					}
				}
			}

			// 4. Parent, walk up tree

			if (start.parent != null)
				foreach (DataElement elem in EnumerateAllElementsFromHere(start.parent))
					yield return elem;
		}

		/// <summary>
		/// Enumerates all children starting from, but not including
		/// 'start.'  Will also enumerate the children of children until
		/// leaf nodes are hit.
		/// </summary>
		/// <param name="start">Starting DataElement</param>
		/// <param name="cache">Cache of already seen elements</param>
		/// <returns>Returns DataElement children of start.</returns>
		protected static IEnumerable EnumerateChildrenElements(DataElement start, List<DataElement> cache)
		{
			if (!(start is DataElementContainer))
				yield break;

			foreach (DataElement elem in start as DataElementContainer)
				if (!cache.Contains(elem))
					yield return elem;

			foreach (DataElement elem in start as DataElementContainer)
			{
				if (!cache.Contains(elem))
				{
					cache.Add(elem);
					foreach (DataElement ret in EnumerateAllElementsFromHere(elem, cache))
						yield return ret;
				}
			}
		}

		/// <summary>
		/// Find data element with specific name.
		/// </summary>
		/// <remarks>
		/// We will search starting at our level in the tree, then moving
		/// to children from our level, then walk up node by node to the
		/// root of the tree.
		/// </remarks>
		/// <param name="name">Name to search for</param>
		/// <returns>Returns found data element or null.</returns>
		public DataElement find(string name)
		{
			string [] names = name.Split(new char[] {'.'});

			if (names.Length == 1)
			{
				// Make sure it's not us :)
				if (this.name == names[0])
					return this;

				// Check our children
				foreach (DataElement elem in EnumerateElementsUpTree())
				{
					if(elem.name == names[0])
						return elem;
				}

				// Can't locate!
				return null;
			}

			foreach (DataElement elem in EnumerateElementsUpTree())
			{
				if (!(elem is DataElementContainer))
					continue;

				DataElement ret = ((DataElementContainer)elem).QuickNameMatch(names);
				if (ret != null)
					return ret;
			}

			DataElement root = getRoot();
			if (root == this)
				return null;

			return root.find(name);
		}

		/// <summary>
		/// Enumerate all items in tree starting with our current position
		/// then moving up towards the root.
		/// </summary>
		/// <remarks>
		/// This method uses yields to allow for efficient use even if the
		/// quired node is found quickely.
		/// 
		/// The method in which we return elements should match a human
		/// search pattern of a tree.  We start with our current position and
		/// return all children then start walking up the tree towards the root.
		/// At each parent node we return all children (excluding already returned
		/// nodes).
		/// 
		/// This method is ideal for locating objects in the tree in a way indented
		/// a human user.
		/// </remarks>
		/// <returns></returns>
		public IEnumerable<DataElement> EnumerateElementsUpTree()
		{
			foreach (DataElement e in EnumerateElementsUpTree(new List<DataElement>()))
				yield return e;
		}

		/// <summary>
		/// Enumerate all items in tree starting with our current position
		/// then moving up towards the root.
		/// </summary>
		/// <remarks>
		/// This method uses yields to allow for efficient use even if the
		/// quired node is found quickely.
		/// 
		/// The method in which we return elements should match a human
		/// search pattern of a tree.  We start with our current position and
		/// return all children then start walking up the tree towards the root.
		/// At each parent node we return all children (excluding already returned
		/// nodes).
		/// 
		/// This method is ideal for locating objects in the tree in a way indented
		/// a human user.
		/// </remarks>
		/// <param name="knownParents">List of known parents to stop duplicates</param>
		/// <returns></returns>
		public IEnumerable<DataElement> EnumerateElementsUpTree(List<DataElement> knownParents)
		{
			List<DataElement> toRoot = new List<DataElement>();
			DataElement cur = this;
			while (cur != null)
			{
				toRoot.Add(cur);
				cur = cur.parent;
			}

			foreach (DataElement item in toRoot)
			{
				if (!knownParents.Contains(item))
				{
					foreach (DataElement e in item.EnumerateAllElements())
						yield return e;

					knownParents.Add(item);
				}
			}

			// Root will not be returned otherwise
			yield return getRoot();
		}

		/// <summary>
		/// Enumerate all child elements recursevely.
		/// </summary>
		/// <remarks>
		/// This method will return this objects direct children
		/// and finally recursevely return children's children.
		/// </remarks>
		/// <returns></returns>
		public IEnumerable<DataElement> EnumerateAllElements()
		{
			foreach (DataElement e in EnumerateAllElements(new List<DataElement>()))
				yield return e;
		}

		/// <summary>
		/// Enumerate all child elements recursevely.
		/// </summary>
		/// <remarks>
		/// This method will return this objects direct children
		/// and finally recursevely return children's children.
		/// </remarks>
		/// <param name="knownParents">List of known parents to skip</param>
		/// <returns></returns>
		public virtual IEnumerable<DataElement> EnumerateAllElements(List<DataElement> knownParents)
		{
			yield break;
		}

		/// <summary>
		/// Fixup for this data element.  Can be null.
		/// </summary>
		public Fixup fixup
		{
			get { return _fixup; }
			set { _fixup = value; }
		}

		/// <summary>
		/// Transformer for this data element.  Can be null.
		/// </summary>
		public Transformer transformer
		{
			get { return _transformer; }
			set { _transformer = value; }
		}

		/// <summary>
		/// Relations for this data element.
		/// </summary>
		public RelationContainer relations
		{
			get { return _relations; }
		}
	}

	/// <summary>
	/// Abstract base class for DataElements that contain other
	/// data elements.  Such as Block, Choice, or Flags.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[Serializable]
	public abstract class DataElementContainer : DataElement, IEnumerable<DataElement>, IList<DataElement>
	{
		protected List<DataElement> _childrenList = new List<DataElement>();
		protected Dictionary<string, DataElement> _childrenDict = new Dictionary<string,DataElement>();

		public override bool isLeafNode
		{
			get
			{
				return _childrenList.Count == 0;
			}
		}

		public DataElement QuickNameMatch(string[] names)
		{
			try
			{
				if (this.name != names[0])
					return null;

				DataElement ret = this;
				for (int cnt = 1; cnt < names.Length; cnt++)
				{
					ret = ((DataElementContainer)ret)[names[cnt]];
				}

				return ret;
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		/// Enumerate all child elements recursevely.
		/// </summary>
		/// <remarks>
		/// This method will return this objects direct children
		/// and finally recursevely return children's children.
		/// </remarks>
		/// <param name="knownParents">List of known parents to skip</param>
		/// <returns></returns>
		public override IEnumerable<DataElement> EnumerateAllElements(List<DataElement> knownParents)
		{
			// First our children
			foreach (DataElement child in this)
				yield return child;

			// Next our children's children
			foreach (DataElement child in this)
			{
				if (!knownParents.Contains(child))
				{
					foreach (DataElement subChild in child.EnumerateAllElements(knownParents))
						yield return subChild;
				}
			}
		}

		/// <summary>
		/// Check if we are a parent of an element.  This is
		/// true even if we are not the direct parent, but several
		/// layers up.
		/// </summary>
		/// <param name="element">Element to check</param>
		/// <returns>Returns true if we are a parent of element.</returns>
		public bool isParentOf(DataElement element)
		{
			while (element.parent != null && element.parent is DataElement)
			{
				element = element.parent;
				if (element == this)
					return true;
			}

			return false;
		}

		public DataElement this[int index]
		{
			get { return _childrenList[index]; }
			set
			{
				if (value == null)
					throw new ApplicationException("Cannot set null value");

				_childrenDict.Remove(_childrenList[index].name);
				_childrenDict.Add(value.name, value);

				_childrenList[index].parent = null;

				_childrenList.RemoveAt(index);
				_childrenList.Insert(index, value);

				value.parent = this;

				Invalidate();
			}
		}

		public DataElement this[string key]
		{
			get { return _childrenDict[key]; }
			set
			{
				if (value == null)
					throw new ApplicationException("Cannot set null value");

				int index = _childrenList.IndexOf(_childrenDict[key]);
				_childrenList.RemoveAt(index);
				_childrenDict[key].parent = null;
				_childrenDict[key] = value;
				_childrenList.Insert(index, value);

				value.parent = this;

				Invalidate();
			}
		}

		#region IEnumerable<Element> Members

		public IEnumerator<DataElement> GetEnumerator()
		{
			return _childrenList.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _childrenList.GetEnumerator();
		}

		#endregion

		#region IList<DataElement> Members

		public int IndexOf(DataElement item)
		{
			return _childrenList.IndexOf(item);
		}

		public void Insert(int index, DataElement item)
		{
			foreach(string k in _childrenDict.Keys)
				if(k == item.name)
					throw new ApplicationException(
						string.Format("Child DataElement named {0} already exists.", item.name));

			_childrenList.Insert(index, item);
			_childrenDict[item.name] = item;

			item.parent = this;

			Invalidate();
		}

		public void RemoveAt(int index)
		{
			_childrenDict.Remove(_childrenList[index].name);
			_childrenList[index].parent = null;
			_childrenList.RemoveAt(index);

			Invalidate();
		}

		#endregion

		#region ICollection<DataElement> Members

		public void Add(DataElement item)
		{
			foreach(string k in _childrenDict.Keys)
				if(k == item.name)
					throw new ApplicationException(
						string.Format("Child DataElement named {0} already exists.", item.name));

			_childrenList.Add(item);
			_childrenDict[item.name] = item;
			item.parent = this;

			Invalidate();
		}

		public void Clear()
		{
			foreach (DataElement e in _childrenList)
				e.parent = null;

			_childrenList.Clear();
			_childrenDict.Clear();

			Invalidate();
		}

		public bool Contains(DataElement item)
		{
			return _childrenList.Contains(item);
		}

		public void CopyTo(DataElement[] array, int arrayIndex)
		{
			_childrenList.CopyTo(array, arrayIndex);
			foreach (DataElement e in array)
			{
				_childrenDict[e.name] = e;
				e.parent = this;
			}

			Invalidate();
		}

		public int Count
		{
			get { return _childrenList.Count; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public bool Remove(DataElement item)
		{
			_childrenDict.Remove(item.name);
			bool ret = _childrenList.Remove(item);
			item.parent = null;

			Invalidate();

			return ret;
		}

		#endregion
	}

}

// end
