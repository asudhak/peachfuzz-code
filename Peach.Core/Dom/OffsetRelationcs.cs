
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

using Peach.Core;
using Peach.Core.IO;

namespace Peach.Core.Dom
{

	/// <summary>
	/// Byte offset relation
	/// </summary>
	[Serializable]
	public class OffsetRelation : Relation
	{
		public bool isRelativeOffset;
		public string relativeTo = null;

		protected bool _isRecursing = false;

		public override long GetValue()
		{
			if (_isRecursing)
				return 0;

			try
			{
				_isRecursing = true;
				long offset = (long)From.DefaultValue;

				if (_expressionGet != null)
				{
					Dictionary<string, object> state = new Dictionary<string, object>();
					state["offset"] = offset;
					state["value"] = offset;
					state["self"] = this._parent;

					object value = Scripting.EvalExpression(_expressionGet, state);
					offset = Convert.ToInt64(value);
				}

				return offset;
			}
			finally
			{
				_isRecursing = false;
			}
		}

		public override Variant CalculateFromValue()
		{
			if (_isRecursing)
				return new Variant(0);

			try
			{
				_isRecursing = true;
				long offset = calculateOffset(From, Of);

				if (_expressionGet != null)
				{
					Dictionary<string, object> state = new Dictionary<string, object>();
					state["offset"] = offset;
					state["value"] = offset;
					state["self"] = this._parent;

					object value = Scripting.EvalExpression(_expressionSet, state);
					offset = Convert.ToInt32(value);
				}

				return new Variant(offset);
			}
			finally
			{
				_isRecursing = false;
			}
		}

		public override void SetValue(Variant value)
		{
			int offset = (int)value;

			if (_expressionSet != null)
			{
				Dictionary<string, object> state = new Dictionary<string, object>();
				state["offset"] = offset;
				state["value"] = offset;
				state["self"] = this._parent;

				object newValue = Scripting.EvalExpression(_expressionGet, state);
				offset = Convert.ToInt32(newValue);
			}

			_from.DefaultValue = new Variant(offset);
		}

		/// <summary>
		/// Caluclate the offset in bytes between two data elements.
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <returns>Returns the offset in bytes between two elements.  Return can be negative.</returns>
		protected long calculateOffset(DataElement from, DataElement to)
		{
			DataElementContainer commonAncestor = null;

			if (isRelativeOffset)
			{
				if (string.IsNullOrEmpty(relativeTo))
				{
					commonAncestor = findCommonRoot(from, to);
				}
				else
				{
					commonAncestor = findCommonRoot(from.find(relativeTo), to);
				}
			}
			else
			{
				commonAncestor = findCommonRoot(from.getRoot(), to);
			}

			if (commonAncestor == null)
				throw new PeachException("Error, unable to calculate offset between '" + 
					from.fullName + "' and '" + to.fullName + "'.");

			BitStream stream = commonAncestor.Value;
			long fromPosition = stream.DataElementPosition(from);
			long toPosition = stream.DataElementPosition(to);

			return toPosition - fromPosition;
		}

		/// <summary>
		/// Locate the nearest common ancestor conainer.
		/// </summary>
		/// <remarks>
		/// To calculate the offset of elem2 from elem1 we need a the
		/// nearest common ancestor.  From that ancestor we can determine
		/// the offset of the two elements.  If the elements do not share
		/// a common ancestor we cannot calculate the offset.
		/// </remarks>
		/// <param name="elem1"></param>
		/// <param name="elem2"></param>
		/// <returns></returns>
		protected DataElementContainer findCommonRoot(DataElement elem1, DataElement elem2)
		{
			List<DataElementContainer> parentsElem1 = new List<DataElementContainer>();

			DataElementContainer parent = elem1.parent;
			while (parent != null)
			{
				parentsElem1.Add(parent);
				parent = parent.parent;
			}

			parent = elem2.parent;
			while (parent != null)
			{
				if (parentsElem1.Contains(parent))
					return parent;

				parent = parent.parent;
			}

			return null;
		}
	}
}

// end
