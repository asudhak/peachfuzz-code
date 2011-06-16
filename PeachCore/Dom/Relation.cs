
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

				if (_parent != null)
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
				if (_of != null)
					_of.Invalidated -= new InvalidatedEventHandler(OfInvalidated);

				_ofName = value;
				_of = null;

				if (_from != null)
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
			get
			{
				// When request we should evaluate

				if (_of == null)
				{
					_of = From.find(_ofName);

					// TODO - What if null?
					if (_of == null)
						System.Diagnostics.Debugger.Break();
				}

				return _of;
			}
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
			foreach (Type k in _childrenDict.Keys)
				if (k == item.GetType())
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

}

// end
