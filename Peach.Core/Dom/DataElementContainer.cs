
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
//   Michael Eddington (mike@dejavusecurity.com)

// $Id$

using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;

using Peach.Core.IO;
using Peach.Core.Cracker;

using NLog;

namespace Peach.Core.Dom
{
	/// <summary>
	/// Abstract base class for DataElements that contain other
	/// data elements.  Such as Block, Choice, or Flags.
	/// </summary>
	[Serializable]
	public abstract class DataElementContainer : DataElement, IEnumerable<DataElement>, IList<DataElement>
	{
		protected List<DataElement> _childrenList = new List<DataElement>();
		protected Dictionary<string, DataElement> _childrenDict = new Dictionary<string, DataElement>();

		public DataElementContainer()
		{
		}

		public DataElementContainer(string name)
			: base(name)
		{
		}

		public override void Crack(DataCracker context, BitStream data, long? size)
		{
			BitStream sizedData = ReadSizedData(data, size);
			long startPosition = data.TellBits();

			// Handle children, iterate over a copy since cracking can modify the list
			for (int i = 0; i < this.Count; )
			{
				var child = this[i];
				context.CrackData(child, sizedData);

				// If we are unsized, cracking a child can cause our size
				// to be available.  If so, update and keep going.
				if (!size.HasValue)
				{
					size = context.GetElementSize(this);

					if (size.HasValue)
					{
						long read = data.TellBits() - startPosition;
						sizedData = ReadSizedData(data, size, read);
					}
				}

				int idx = IndexOf(child);
				if (idx != -1)
					i = idx + 1;
			}

			if (size.HasValue && sizedData == data)
				data.SeekBits(startPosition + size.Value, System.IO.SeekOrigin.Begin);
		}

		public override bool isLeafNode
		{
			get
			{
				return _childrenList.Count == 0;
			}
		}

		public DataElement QuickNameMatch(string[] names)
		{
			if (names.Length == 0)
				throw new ArgumentException("Array must contain at least one entry.", "names");

			if (this.name != names[0])
				return null;

			DataElement ret = this;
			for (int cnt = 1; cnt < names.Length; cnt++)
			{
				var cont = ret as DataElementContainer;
				if (cont == null)
					return null;

				var choice = cont as Choice;
				if (choice != null)
				{
					if (!choice.choiceElements.TryGetValue(names[cnt], out ret))
						return null;
				}
				else
				{
					if (!cont._childrenDict.TryGetValue(names[cnt], out ret))
						return null;
				}
			}

			return ret;
		}

		public override BitStream  ReadSizedData(BitStream data, long? size, long read = 0)
		{
			if (!size.HasValue)
				return data;

			long needed = size.Value - read;
			data.WantBytes((needed + 7) / 8);
			long remain = data.LengthBits - data.TellBits();

			if (needed == remain)
				return data;

			 return base.ReadSizedData(data, size, read);
		}

		public override bool CacheValue
		{
			get
			{
				if (!base.CacheValue)
					return false;

				foreach (var elem in this)
				{
					if (!elem.CacheValue)
						return false;
				}

				return true;
			}
		}

		/// <summary>
		/// Recursively execute analyzers
		/// </summary>
		public override void evaulateAnalyzers()
		{
			foreach (DataElement child in this._childrenList.ToArray())
				child.evaulateAnalyzers();

			if (analyzer == null)
				return;

			analyzer.asDataElement(this, null);
		}

		/// <summary>
		/// Does container contain child element with name key?
		/// </summary>
		/// <param name="key">Name of child element to check</param>
		/// <returns>Returns true if child exits</returns>
		public bool ContainsKey(string key)
		{
			return _childrenDict.ContainsKey(key);
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

		/// <summary>
		/// Create a pretty string representation of model from here.
		/// </summary>
		/// <returns></returns>
		public string prettyPrint(StringBuilder sb = null, int indent = 0)
		{
			if(sb == null)
				sb = new StringBuilder();

			stringPrintLineWithIndent(sb, name + ": " + GetType().Name, indent);

			foreach (DataElement child in this)
			{
				if (child is DataElementContainer)
					((DataElementContainer)child).prettyPrint(sb, indent + 1);
				else
					stringPrintLineWithIndent(sb, child.name + ": " + child.GetType().Name, indent);
			}

			return sb.ToString();
		}

		void stringPrintLineWithIndent(StringBuilder sb, string line, int indent)
		{
			for (int i = 0; i < indent; i++)
				sb.Append(' ');

			sb.Append(line);
			sb.Append("\n");
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
			foreach (string k in _childrenDict.Keys)
				if (k == item.name)
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
			foreach (string k in _childrenDict.Keys)
				if (k == item.name)
					throw new ApplicationException(
						string.Format("Child DataElement named {0} already exists.", item.name));

			_childrenList.Add(item);
			_childrenDict[item.name] = item;
			item.parent = this;

			Invalidate();
		}

		public void Clear()
		{
			Clear(true);
		}

		protected void Clear(bool resetParent)
		{
			if (resetParent)
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

		public void SwapElements(int first, int second)
		{
			if (first >= _childrenList.Count || second >= _childrenList.Count)
				throw new ArgumentException();

			var tmp = _childrenList[first];
			_childrenList[first] = _childrenList[second];
			_childrenList[second] = tmp;
		}

		public bool Remove(DataElement item)
		{
			System.Diagnostics.Debug.Assert(item.parent == this);

			if (item.parent is Choice)
				return parent.Remove(item.parent);

			if (item.parent is Array && item.parent.Count == 1)
				return parent.Remove(this);

			item.ClearRelations();

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
