
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

namespace Peach.Core.Dom
{
	/// <summary>
	/// Abstract base class for DataElements that contain other
	/// data elements.  Such as Block, Choice, or Flags.
	/// </summary>
	[Serializable]
	public class RelationContainer : IEnumerable<Relation>, IList<Relation>
	{
		protected DataElement parent;
		protected List<Relation> _childrenList = new List<Relation>();

		public RelationContainer(DataElement parent)
		{
			this.parent = parent;
		}

		public DataElement Parent
		{
			get
			{
				return parent;
			}
			set
			{
				parent = value;
			}
		}

		public Relation this[int index]
		{
			get { return _childrenList[index]; }
			set
			{
				if (value == null)
					throw new ApplicationException("Cannot set null value");

				_childrenList[index].parent = null;

				_childrenList.RemoveAt(index);
				_childrenList.Insert(index, value);

				value.parent = parent;
			}
		}

		public IEnumerable<T> Of<T>() where T: class
		{
			foreach (Relation rel in _childrenList)
			{
				T r = rel as T;
				if (r != null && rel.Of == parent)
					yield return r;
			}
		}

		public IEnumerable<T> From<T>() where T : class
		{
			foreach (Relation rel in _childrenList)
			{
				T r = rel as T;
				if (r != null && rel.From == parent)
					yield return r;
			}
		}

		public bool hasOfSizeRelation
		{
			get
			{
				foreach (Relation rel in _childrenList)
					if (rel is SizeRelation && rel.Of == parent)
						return true;

				return false;
			}
		}

		public bool hasOfOffsetRelation
		{
			get
			{
				foreach (Relation rel in _childrenList)
					if (rel is OffsetRelation && rel.Of == parent)
						return true;

				return false;
			}
		}

		public bool hasOfCountRelation
		{
			get
			{
				foreach (Relation rel in _childrenList)
					if (rel is CountRelation && rel.Of == parent)
						return true;

				return false;
			}
		}

        public bool hasFromSizeRelation
        {
            get
            {
                foreach (Relation rel in _childrenList)
                    if (rel is SizeRelation && rel.From == parent)
                        return true;

                return false;
            }
        }

        public bool hasFromOffsetRelation
        {
            get
            {
                foreach (Relation rel in _childrenList)
                    if (rel is OffsetRelation && rel.From == parent)
                        return true;

                return false;
            }
        }

        public bool hasFromCountRelation
        {
            get
            {
                foreach (Relation rel in _childrenList)
                    if (rel is CountRelation && rel.From == parent)
                        return true;

                return false;
            }
        }

		public SizeRelation getOfSizeRelation()
		{
			foreach (Relation rel in _childrenList)
			{
				if (rel is SizeRelation && rel.Of == parent)
					return rel as SizeRelation;
			}

			return null;
		}

		public CountRelation getOfCountRelation()
		{
			foreach (Relation rel in _childrenList)
			{
				if (rel is CountRelation && rel.Of == parent)
					return rel as CountRelation;
			}

			return null;
		}

		public OffsetRelation getOfOffsetRelation()
		{
			foreach (Relation rel in _childrenList)
			{
				if (rel is OffsetRelation && rel.Of == parent)
					return rel as OffsetRelation;
			}

			return null;
		}

        public SizeRelation getFromSizeRelation()
        {
            foreach (Relation rel in _childrenList)
            {
                if (rel is SizeRelation && rel.From == parent)
                    return rel as SizeRelation;
            }

            return null;
        }

        public CountRelation getFromCountRelation()
        {
            foreach (Relation rel in _childrenList)
            {
                if (rel is CountRelation && rel.From == parent)
                    return rel as CountRelation;
            }

            return null;
        }

        public OffsetRelation getFromOffsetRelation()
        {
            foreach (Relation rel in _childrenList)
            {
                if (rel is OffsetRelation && rel.From == parent)
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

		public void Insert(int index, Relation item)
		{
			_childrenList.Insert(index, item);
			item.parent = parent;
		}

		public void RemoveAt(int index)
		{
			_childrenList[index].parent = null;
			_childrenList.RemoveAt(index);
		}

		#endregion

		#region ICollection<Relation> Members

		public void Add(Relation item)
		{
			Add(item, true);
		}

		public void Add(Relation item, bool updateParent)
		{
			_childrenList.Add(item);

			if (updateParent && item.parent != parent)
				item.parent = parent;
		}

		public void Clear()
		{
			foreach (Relation e in _childrenList)
				e.parent = null;

			_childrenList.Clear();
		}

		public bool Contains(Relation item)
		{
			return _childrenList.Contains(item);
		}

		public void CopyTo(Relation[] array, int arrayIndex)
		{
			_childrenList.CopyTo(array, arrayIndex);
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
			bool ret = _childrenList.Remove(item);
			item.parent = null;

			return ret;
		}

		#endregion
	}

}

// end
