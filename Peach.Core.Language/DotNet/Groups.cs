
/* Copyright (c) 2007-2009 Michael Eddington
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy 
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights 
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
 * copies of the Software, and to permit persons to whom the Software is 
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in	
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * 
 * Authors:
 *   Michael Eddington (mike@phed.org)
 * 
 * $Id$
 */

using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace Peach.Core.Language.DotNet
{
	public class Group
	{
		protected string _name;
		protected List<IGenerator> _generators = new List<IGenerator>();

		public Group()
		{
			_name = "";
		}

		public Group(string name)
		{
			_name = name;
		}

		public void AddGenerator(IGenerator generator)
		{
			_generators.Add(generator);
		}

		public void RemoveGenerator(IGenerator generator)
		{
			_generators.Remove(generator);
		}

		public virtual void Next()
		{
			bool done = true;

			foreach (IGenerator g in _generators)
			{
				try
				{
					g.Next();
					done = false;
				}
				catch (GeneratorCompleted)
				{
				}
			}

			if (done)
				throw new GroupCompleted();
		}

		public virtual void Reset()
		{
			foreach (IGenerator g in _generators)
				g.Reset();
		}
	}

	public class GroupSequence: Group, IList<Group>
	{
		protected List<Group> _groups;
		protected int _count = 1;
		protected int _position = 0;

		public GroupSequence(List<Group> groups)
			: base()
		{
			_groups = groups;
		}
		public GroupSequence(int groupCount)
			: base()
		{
			for(int i = 0; i<groupCount; i++)
				_groups.Add(new Group());
		}
		public GroupSequence(List<Group> groups, string name)
			: base(name)
		{
			_groups = groups;
		}

		public Group AddGruop()
		{
			Group group = new Group();
			_groups.Add(group);
			return group;
		}

		public override void Next()
		{
			if (_position >= _groups.Count)
				throw new GroupCompleted();

			try
			{
				_groups[_position].Next();
				_count += 1;
			}
			catch (GroupCompleted)
			{
				Console.WriteLine(_name + ": GroupSequence.GroupCompleted -- [" + _count + "]");

				_count = 1;
				
				_groups[_position].Reset();
				_position++;

				if (_position >= _groups.Count)
				{
					_position--;
					throw new GroupCompleted();
				}
			}
		}

		public override void Reset()
		{
			_position = 0;

			foreach (Group g in _groups)
				g.Reset();
		}

		public Group this[int index]
		{
			get { return _groups[index]; }
			set { _groups[index] = value; }
		}



		#region IList<Group> Members

		public int IndexOf(Group item)
		{
			return _groups.IndexOf(item);
		}

		public void Insert(int index, Group item)
		{
			_groups.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			_groups.RemoveAt(index);
		}

		#endregion

		#region ICollection<Group> Members

		public void Add(Group item)
		{
			_groups.Add(item);
		}

		public void Clear()
		{
			_groups.Clear();
		}

		public bool Contains(Group item)
		{
			return _groups.Contains(item);
		}

		public void CopyTo(Group[] array, int arrayIndex)
		{
			_groups.CopyTo(array, arrayIndex);
		}

		public int Count
		{
			get { return _groups.Count; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public bool Remove(Group item)
		{
			return _groups.Remove(item);
		}

		#endregion

		#region IEnumerable<Group> Members

		public IEnumerator<Group> GetEnumerator()
		{
			return _groups.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _groups.GetEnumerator();
		}

		#endregion
	}

	public class GroupForeachDo : Group
	{
		protected Group _groupA;
		protected Group _groupB;
		protected int _count = 1;

		public GroupForeachDo(Group groupA, Group groupB)
			: base()
		{
			_groupA = groupA;
			_groupB = groupB;
		}
		public GroupForeachDo(Group groupA, Group groupB, string name)
			: base(name)
		{
			_groupA = groupA;
			_groupB = groupB;
		}

		public override void Next()
		{
			try
			{
				_groupB.Next();
				_count++;
			}
			catch (GroupCompleted)
			{
				Console.WriteLine(_name + ": GroupForeachDo.GroupCompleted -- [" + _count + "]");
				_groupB.Reset();
				_groupA.Next();
				_count++;
			}
		}

		public override void Reset()
		{
			_groupA.Reset();
			_groupB.Reset();
		}
	}
}
