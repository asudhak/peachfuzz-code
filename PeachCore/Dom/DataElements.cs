
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

namespace Peach.Core.Dom
{
	public enum LengthType
	{
		String,
		Python,
		Ruby,
		Calc
	}

	/// <summary>
	/// Base class for all data element relations
	/// </summary>
	[Serializable]
	public abstract class Relation
	{
		protected DataElement _parent = null;
		protected string _ofName = null;
		protected string _fromName = null;
		protected DataElement _of = null;
		protected DataElement _from = null;
		protected string _expressionGet = null;
		protected string _expressionSet = null;

		/// <summary>
		/// Expression that is run when getting the value.
		/// </summary>
		public string ExpressionGet
		{
			get { return _expressionGet; }
			set
			{
				_expressionGet = value;
				From.Invalidate();
			}
		}

		/// <summary>
		/// Expression that is run when setting the value.
		/// </summary>
		public string ExpressionSet
		{
			get { return _expressionSet; }
			set
			{
				_expressionSet = value;
				From.Invalidate();
			}
		}

		/// <summary>
		/// Parent of relation.  This is
		/// typically our From as well.
		/// </summary>
		public DataElement parent
		{
			get { return _parent; }
			set
			{
				if (_parent != null)
				{
					_parent.Invalidate();
					_parent = null;
				}

				_parent = value;
				_from = _parent;

				if(_parent != null)
					_parent.Invalidate();
			}
		}

		/// <summary>
		/// Name of DataElement used to generate our value.
		/// </summary>
		public string OfName
		{
			get { return _ofName; }
			set
			{
				if(_of != null)
					_of.Invalidated -= new InvalidatedEventHandler(OfInvalidated);

				_ofName = value;
				_of = null;

				if(_from != null)
					_from.Invalidate();
			}
		}

		/// <summary>
		/// Name of DataElement that receives our value
		/// when generated.
		/// </summary>
		public string FromName
		{
			get { return _fromName; }
			set
			{
				if (_from != null)
					_from.Invalidate();

				_fromName = value;
				_from = null;
			}
		}

		/// <summary>
		/// DataElement used to generate our value.
		/// </summary>
		public DataElement Of
		{
			get { return _of; }
			set
			{
				if (_of != null)
				{
					// Remove existing event
					_of.Invalidated -= new InvalidatedEventHandler(OfInvalidated);
				}

				_of = value;
				_of.Invalidated += new InvalidatedEventHandler(OfInvalidated);

				_ofName = _of.fullName;

				// We need to invalidate now that we have a new of.
				From.Invalidate();
			}
		}

		/// <summary>
		/// DataElement that receives our value
		/// when generated.
		/// </summary>
		public DataElement From
		{
			get
			{
				if (_from == null)
				{
					if (_of != null & parent != _of)
						_from = parent;
				}

				return _from;
			}

			set
			{
				_from = value;
				_fromName = _from.fullName;
			}
		}

		/// <summary>
		/// Handle invalidated event from "of" side of
		/// relation.  Need to invalidate "from".
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void OfInvalidated(object sender, EventArgs e)
		{
			// Invalidate 'from' side
			From.Invalidate();
		}

		/// <summary>
		/// Get value from relation as int.
		/// </summary>
		public abstract Variant GetValue();

		/// <summary>
		/// Set value on from side
		/// </summary>
		/// <param name="value"></param>
		public abstract void SetValue(Variant value);
	}

	/// <summary>
	/// Abstract base class for DataElements that contain other
	/// data elements.  Such as Block, Choice, or Flags.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[Serializable]
	public class RelationContainer : IEnumerable<Relation>, IList<Relation>
	{
		protected DataElement parent;
		protected List<Relation> _childrenList = new List<Relation>();
		protected Dictionary<Type, Relation> _childrenDict = new Dictionary<Type, Relation>();

		public RelationContainer(DataElement parent)
		{
			this.parent = parent;
		}

		public Relation this[int index]
		{
			get { return _childrenList[index]; }
			set
			{
				if (value == null)
					throw new ApplicationException("Cannot set null value");

				_childrenDict.Remove(_childrenList[index].GetType());
				_childrenDict.Add(value.GetType(), value);

				_childrenList[index].parent = null;

				_childrenList.RemoveAt(index);
				_childrenList.Insert(index, value);

				value.parent = parent;
			}
		}

		public Relation this[Type key]
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

				value.parent = parent;
			}
		}

		public bool hasWhenRelation
		{
			get
			{
				foreach (Relation rel in _childrenList)
					if (rel is WhenRelation)
						return true;

				return false;
			}
		}

		public bool hasSizeRelation
		{
			get
			{
				foreach (Relation rel in _childrenList)
					if (rel is SizeRelation)
						return true;

				return false;
			}
		}

		public bool hasOffsetRelation
		{
			get
			{
				foreach (Relation rel in _childrenList)
					if (rel is OffsetRelation)
						return true;

				return false;
			}
		}

		public SizeRelation getSizeRelation()
		{
			foreach (Relation rel in _childrenList)
			{
				if (rel is SizeRelation)
					return rel as SizeRelation;
			}

			return null;
		}

		public CountRelation getCountRelation()
		{
			foreach (Relation rel in _childrenList)
			{
				if (rel is CountRelation)
					return rel as CountRelation;
			}

			return null;
		}

		public OffsetRelation getOffsetRelation()
		{
			foreach (Relation rel in _childrenList)
			{
				if (rel is OffsetRelation)
					return rel as OffsetRelation;
			}

			return null;
		}

		public WhenRelation getWhenRelation()
		{
			foreach (Relation rel in _childrenList)
			{
				if (rel is WhenRelation)
					return rel as WhenRelation;
			}

			return null;
		}

		#region IEnumerable<Relation> Members

		public IEnumerator<Relation> GetEnumerator()
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

		#region IList<Relation> Members

		public int IndexOf(Relation item)
		{
			return _childrenList.IndexOf(item);
		}

		protected bool HaveKey(Type key)
		{
			foreach (Type k in _childrenDict.Keys)
				if (k == key)
					return true;

			return false;
		}

		public void Insert(int index, Relation item)
		{
			if (HaveKey(item.GetType()))
				throw new ApplicationException(
					string.Format("Child Relation typed {0} already exists.", item.GetType()));

			_childrenList.Insert(index, item);
			_childrenDict[item.GetType()] = item;

			item.parent = parent;
		}

		public void RemoveAt(int index)
		{
			_childrenDict.Remove(_childrenList[index].GetType());
			_childrenList[index].parent = null;
			_childrenList.RemoveAt(index);
		}

		#endregion

		#region ICollection<Relation> Members

		public void Add(Relation item)
		{
			foreach(Type k in _childrenDict.Keys)
				if(k == item.GetType())
					throw new ApplicationException(
						string.Format("Child Relation typed {0} already exists.", item.GetType()));

			_childrenList.Add(item);
			_childrenDict[item.GetType()] = item;
			item.parent = parent;
		}

		public void Clear()
		{
			foreach (Relation e in _childrenList)
				e.parent = null;

			_childrenList.Clear();
			_childrenDict.Clear();
		}

		public bool Contains(Relation item)
		{
			return _childrenList.Contains(item);
		}

		public void CopyTo(Relation[] array, int arrayIndex)
		{
			_childrenList.CopyTo(array, arrayIndex);
			foreach (Relation e in array)
			{
				_childrenDict[e.GetType()] = e;
				e.parent = parent;
			}
		}

		public int Count
		{
			get { return _childrenList.Count; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public bool Remove(Relation item)
		{
			_childrenDict.Remove(item.GetType());
			bool ret = _childrenList.Remove(item);
			item.parent = null;

			return ret;
		}

		#endregion
	}

	/// <summary>
	/// Byte size relation.
	/// </summary>
	[Serializable]
	public class SizeRelation : Relation
	{
		public override Variant GetValue()
		{
			ulong size = _of.Value.LengthBytes;

			if (_expressionGet != null)
			{
				Dictionary<string, object> state = new Dictionary<string,object>();
				state["size"] = size;
				state["value"] = size;
				state["self"] = this._parent;

				object value = Scripting.EvalExpression(_expressionGet, state);
				size = Convert.ToUInt64(value);
			}

			return new Variant(size);
		}

		public override void SetValue(Variant value)
		{
			ulong size = (ulong)value;

			if (_expressionSet != null)
			{
				Dictionary<string, object> state = new Dictionary<string, object>();
				state["size"] = size;
				state["value"] = size;
				state["self"] = this._parent;

				object newValue = Scripting.EvalExpression(_expressionGet, state);
				size = Convert.ToUInt64(newValue);
			}

			_from.DefaultValue = new Variant(size);
		}
	}

	/// <summary>
	/// Array count relation
	/// </summary>
	public class CountRelation : Relation
	{
		public override Variant GetValue()
		{
			throw new NotImplementedException();
		}

		public override void SetValue(Variant value)
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>
	/// Byte offset relation
	/// </summary>
	[Serializable]
	public class OffsetRelation : Relation
	{
		public bool isRelativeOffset;
		public string relativeTo = null;

		public override Variant GetValue()
		{
			throw new NotImplementedException();
		}

		public override void SetValue(Variant value)
		{
			throw new NotImplementedException();
		}
	}

	[Serializable]
	public class WhenRelation : Relation
	{
		public string WhenExpression = "";

		public override Variant GetValue()
		{
			throw new NotImplementedException();
		}

		public override void SetValue(Variant value)
		{
			throw new NotImplementedException();
		}
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
		/// Mutated vale override's fixup
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
		protected ulong _length = 0;
		protected LengthType _lengthType = LengthType.String;
		protected string _lengthOther = null;

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
			if (nextIndex > _parent.Count)
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

		/// <summary>
		/// Length of element.  In the case that 
		/// LengthType == "Calc" we will evaluate the
		/// expression.
		/// </summary>
		public virtual ulong length
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
						return (ulong)Scripting.EvalExpression(_lengthOther, scope);
					default:
						throw new NotSupportedException("Error calculating length.");
				}
			}
			set
			{
				_lengthType = LengthType.String;
				_length = value;
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
		/// <param name="name">Name to search for</param>
		/// <returns>Returns found data element or null.</returns>
		public DataElement find(string name)
		{
			string [] names = name.Split(new char[] {'.'});

			if (names.Length == 1)
			{
				foreach (DataElement elem in EnumerateElementsByName(names[0]))
				{
					return elem;
				}
			}

			foreach (DataElement elem in EnumerateElementsByName(names[0]))
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

		public virtual IEnumerable<DataElement> EnumerateElementsByName(string name)
		{
			yield return this;
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

		public override IEnumerable<DataElement> EnumerateElementsByName(string name)
		{
			if (name == this.name)
				yield return this;

			foreach (DataElement elem in this)
			{
				if(elem.name == name)
					yield return elem;

				foreach (DataElement d in elem.EnumerateElementsByName(name))
					yield return d;
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

	/// <summary>
	/// Block element
	/// </summary>
	[DataElement("Block")]
	[DataElementChildSupportedAttribute(DataElementTypes.Any)]
	//[ParameterAttribute("length", typeof(uint), "Length of string in characters", false)]
	[Serializable]
	public class Block : DataElementContainer
	{
		public string lengthType;
		public string lengthCalc;
		public Variant length;

		public override Variant GenerateInternalValue()
		{
			Variant value;

			// 1. Default value

			if (_mutatedValue == null)
			{
				BitStream stream = new BitStream();
				foreach (DataElement child in this)
					stream.Write(child.Value, child);

				value = new Variant(stream);
			}
			else
			{
				value = MutatedValue;
			}

			// 2. Relations

			if (_mutatedValue != null && (mutationFlags & MUTATE_OVERRIDE_RELATIONS) != 0)
			{
				_internalValue = _mutatedValue;
				return MutatedValue;
			}

			foreach (Relation r in _relations)
			{
				if (r.Of == this)
				{
					value = r.GetValue();
				}
			}

			// 3. Fixup

			if (_mutatedValue != null && (mutationFlags & MUTATE_OVERRIDE_FIXUP) != 0)
			{
				_internalValue = _mutatedValue;
				return MutatedValue;
			}

			if (_fixup != null)
				value = _fixup.fixup(this);

			_internalValue = value;
			return value;
		}
	}

	/// <summary>
	/// DataModel is just a top level Block.
	/// </summary>
	[Serializable]
	public class DataModel : Block
	{
		//public Dom dom = null;
	}

	/// <summary>
	/// Choice allows the selection of a single
	/// data element based on the current data set.
	/// 
	/// The other options in the choice are available
	/// for mutation by the mutators.
	/// </summary>
	[DataElement("Choice")]
	[DataElementChildSupportedAttribute(DataElementTypes.Any)]
	[Serializable]
	public class Choice : DataElementContainer
	{
		public OrderedDictionary<string, DataElement> choiceElements = new OrderedDictionary<string, DataElement>();
		DataElement _selectedElement = null;

		public DataElement SelectedElement
		{
			get
			{
				if (_selectedElement == null && choiceElements.Count > 0)
				{
					this.Clear();
					this.Add(choiceElements[0]);
					_selectedElement = this[0];
				}

				return _selectedElement;
			}
			set
			{
				if(!choiceElements.Values.Contains(value))
					throw new KeyNotFoundException("value was not found");

				this.Clear();
				this.Add(value);
				_selectedElement = value;
				Invalidate();
			}
		}

		public override Variant GenerateInternalValue()
		{
			Variant value;

			// 1. Default value

			if (_mutatedValue == null)
				value = new Variant(SelectedElement.Value);

			else
				value = MutatedValue;

			// 2. Relations

			if (_mutatedValue != null && (mutationFlags & MUTATE_OVERRIDE_RELATIONS) != 0)
			{
				_internalValue = MutatedValue;
				return MutatedValue;
			}

			foreach (Relation r in _relations)
			{
				if (r.Of == this)
					value = r.GetValue();
			}

			// 3. Fixup

			if (_mutatedValue != null && (mutationFlags & MUTATE_OVERRIDE_FIXUP) != 0)
			{
				_internalValue = MutatedValue;
				return MutatedValue;
			}

			if (_fixup != null)
				value = _fixup.fixup(this);

			_internalValue = value;
			return value;
		}
	}

	/// <summary>
	/// Array of data elements.  Can be
	/// zero or more elements.
	/// </summary>
	[DataElement("Array")]
	[DataElementChildSupported(DataElementTypes.Any)]
	[DataElementRelationSupported(DataElementRelations.Any)]
	[Parameter("minOccurs", typeof(int), "Minimum number of occurances 0-N", false)]
	[Parameter("maxOccurs", typeof(int), "Maximum number of occurances (-1 for unlimited)", false)]
	[Serializable]
	public class Array : Block
	{
		public int minOccurs = 1;
		public int maxOccurs = 1;

		public bool hasExpanded = false;

		public DataElement origionalElement = null;
	}

	/// <summary>
	/// A numerical data element.
	/// </summary>
	[DataElement("Number")]
	[DataElementChildSupportedAttribute(DataElementTypes.NonDataElements)]
	[ParameterAttribute("size", typeof(uint), "Size in bits [8, 16, 24, 32, 64]", true)]
	[ParameterAttribute("signed", typeof(bool), "Is number signed (default false)", false)]
	[ParameterAttribute("endian", typeof(string), "Byte order of number (default 'little')", false)]
	[Serializable]
	public class Number : DataElement
	{
		protected uint _size = 8;
		protected ulong _max = (ulong)sbyte.MaxValue;
		protected long _min = sbyte.MinValue;
		protected bool _signed = true;
		protected bool _isLittleEndian = true;

		public Number()
			: base()
		{
			DefaultValue = new Variant(0);
		}

		public Number(string name)
			: base(name)
		{
			DefaultValue = new Variant(0);
		}

		public Number(string name, long value, uint size)
			:base(name)
		{
			_size = size;
			DefaultValue = new Variant(value);
		}

		public Number(string name, long value, uint size, bool signed, bool isLittleEndian)
			:base(name)
		{
			_size = size;
			_signed = signed;
			_isLittleEndian = isLittleEndian;
			DefaultValue = new Variant(value);
		}

		public override ulong length
		{
			get
			{
				return _size / 8;
			}
			set
			{
				throw new NotSupportedException("A numbers size must be set by Size.");
			}
		}

		public override bool hasLength
		{
			get
			{
				return true;
			}
			set
			{
				throw new NotSupportedException("A number always has a size.");
			}
		}

		public override LengthType lengthType
		{
			get { return LengthType.String; }
			set { throw new NotSupportedException("Cannot set LengthType on a Number."); }
		}

		public override Variant DefaultValue
		{
			get { return base.DefaultValue; }
			set
			{
				if ((long)value >= _min && (ulong)value <= _max)
					base.DefaultValue = value;
				else
					throw new ApplicationException("DefaultValue not with in min/max values.");
			}
		}

		public uint Size
		{
			get { return _size; }
			set
			{
				if (value == 0)
					throw new ApplicationException("Size must be > 0");

				_size = value;

				if (_signed)
				{
					_max = (ulong)Math.Pow(2, _size) / 2;
					_min = 0 - ((long)Math.Pow(2, _size) / 2);
				}
				else
				{
					_max = (ulong)Math.Pow(2, _size) - 1;
					_min = 0;
				}
				
				Invalidate();
			}
		}

		public bool Signed
		{
			get { return _signed; }
			set
			{
				_signed = value;
				Size = Size;

				Invalidate();
			}
		}

		public bool LittleEndian
		{
			get { return _isLittleEndian; }
			set
			{
				_isLittleEndian = value;
				Invalidate();
			}
		}

		public ulong MaxValue
		{
			get { return _max; }
		}

		public long MinValue
		{
			get { return _min; }
		}

		protected override BitStream InternalValueToBitStream(Variant b)
		{
			BitStream bits = new BitStream();

			if (_isLittleEndian)
				bits.LittleEndian();
			else
				bits.BigEndian();

			bits.WriteBits((ulong)InternalValue, Size);

			return bits;
		}
	}

	public enum StringType
	{
		Ascii,
		Utf7,
		Utf8,
		Utf16,
		Utf16be,
		Utf32
	}

	/// <summary>
	/// String data element.  String elements support numerouse encodings
	/// such as straight ASCII through UTF-32.  Both little and big endian
	/// strings are supported.
	/// 
	/// Strings also support standard attributes such as length, null termination,
	/// etc.
	/// </summary>
	[DataElement("String")]
	[DataElementChildSupportedAttribute(DataElementTypes.NonDataElements)]
	[ParameterAttribute("length", typeof(uint), "Length in characters", false)]
	[ParameterAttribute("nullTerminated", typeof(bool), "Is string null terminated?", false)]
	[ParameterAttribute("type", typeof(StringType), "Type of string (encoding)", true)]
	[Serializable]
	public class String : DataElement
	{
		protected StringType _type = StringType.Ascii;
		protected bool _nullTerminated = false;
		protected char _padCharacter = '\0';

		public String() 
			: base()
		{
			DefaultValue = new Variant("Peach");
		}

		public String(string name)
			: base(name)
		{
			DefaultValue = new Variant("Peach");
		}

		public String(string name, string defaultValue)
			: base(name)
		{
			DefaultValue = new Variant(defaultValue);
		}

		public String(string name, Variant defaultValue)
		{
			DefaultValue = defaultValue;
		}

		public String(string name, string defaultValue, StringType type, bool nullTerminated)
			: base(name)
		{
			DefaultValue = new Variant(defaultValue);
			_type = type;
			_nullTerminated = nullTerminated;
		}

		public String(string name, string defaultValue, StringType type, bool nullTerminated, uint length)
			: base(name)
		{
			DefaultValue = new Variant(defaultValue);
			_type = type;
			_nullTerminated = nullTerminated;
			_length = length;
			_lengthType = LengthType.String;
		}

		/// <summary>
		/// String type/encoding to be used.  Default is 
		/// ASCII.
		/// </summary>
		public StringType stringType
		{
			get { return _type; }
			set { _type = value; }
		}

		/// <summary>
		/// Is string null terminated?  For ASCII strings this
		/// is a single NULL characters, for WCHAR's, two NULL 
		/// characters are used.
		/// </summary>
		public bool nullTerminated
		{
			get { return _nullTerminated; }
			set
			{
				_nullTerminated = value;
				Invalidate();
			}
		}

		/// <summary>
		/// Pad character for string.  Defaults to NULL.
		/// </summary>
		public char padCharacter
		{
			get { return _padCharacter; }
			set
			{
				_padCharacter = value;
				Invalidate();
			}
		}

		protected override BitStream InternalValueToBitStream(Variant v)
		{
			byte[] value = null;

			if (_type == StringType.Ascii)
				value = Encoding.ASCII.GetBytes((string)v);

			else if (_type == StringType.Utf7)
				value = Encoding.UTF7.GetBytes((string)v);

			else if (_type == StringType.Utf8)
				value = Encoding.UTF8.GetBytes((string)v);

			else if (_type == StringType.Utf16)
				value = Encoding.Unicode.GetBytes((string)v);

			else if (_type == StringType.Utf16be)
				value = Encoding.BigEndianUnicode.GetBytes((string)v);

			else if (_type == StringType.Utf32)
				value = Encoding.UTF32.GetBytes((string)v);

			else
				throw new ApplicationException("String._type not set properly!");
			
			return new BitStream(value);
		}
	}

	/// <summary>
	/// Binary large object data element
	/// </summary>
	[DataElement("Blob")]
	[DataElementChildSupportedAttribute(DataElementTypes.NonDataElements)]
	[ParameterAttribute("length", typeof(uint), "Length in bytes", false)]
	[Serializable]
	public class Blob : DataElement
	{
		protected uint _length;

	}

	[DataElement("Flags")]
	[DataElementChildSupportedAttribute(DataElementTypes.NonDataElements)]
	[DataElementChildSupportedAttribute("Flag")]
	[ParameterAttribute("size", typeof(uint), "Size in bits.  Typically [8, 16, 24, 32, 64]", true)]
	[ParameterAttribute("endian", typeof(string), "Byte order of number (default 'little')", false)]
	[Serializable]
	public class Flags : DataElementContainer
	{
		protected uint _size = 0;
		protected bool _littleEndian = true;

		public bool LittleEndian
		{
			get { return _littleEndian; }
			set
			{
				_littleEndian = value;
				Invalidate();
			}
		}

		public uint Size
		{
			get { return _size; }
			set
			{
				_size = value;
				Invalidate();
			}
		}

		public override Variant GenerateInternalValue()
		{
			BitStream bits = new BitStream();

			foreach (DataElement child in this)
			{
				if (child is Flag)
				{
					bits.SeekBits(((Flag)child).Position, System.IO.SeekOrigin.Begin);
					bits.Write(child.Value, child);
				}
				else
					throw new ApplicationException("Flag has child thats not a flag!");
			}

			_internalValue = new Variant(bits);
			return _internalValue;
		}

	}

	[DataElement("Flag")]
	[DataElementChildSupportedAttribute(DataElementTypes.NonDataElements)]
	[ParameterAttribute("position", typeof(uint), "Bit position of flag", true)]
	[ParameterAttribute("size", typeof(uint), "Size in bits", true)]
	[Serializable]
	public class Flag : DataElement
	{
		protected uint _size = 0;
		protected uint _position = 0;

		public uint Size
		{
			get { return _size; }
			set
			{
				_size = value;
				Invalidate();
			}
		}

		public uint Position
		{
			get { return _position; }
			set
			{
				_position = value;
				Invalidate();
			}
		}

		protected override BitStream InternalValueToBitStream(Variant v)
		{
			BitStream bits = new BitStream();
			bits.WriteBits((ulong)v, Size);

			return bits;
		}
	}


}

// end
