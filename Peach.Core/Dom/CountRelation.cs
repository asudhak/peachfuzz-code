
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

using NLog;

namespace Peach.Core.Dom
{
	/// <summary>
	/// Array count relation
	/// </summary>
	[Serializable]
	[Relation("count", true)]
	[Description("Array count relation")]
	[Parameter("of", typeof(string), "Element used to generate relation value", "")]
	[Parameter("from", typeof(string), "Element that receives relation value", "")]
	[Parameter("expressionGet", typeof(string), "Scripting expression that is run when getting the value", "")]
	[Parameter("expressionSet", typeof(string), "Scripting expression that is run when setting the value", "")]
	public class CountRelation : Relation
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger(); 
		protected bool _isRecursing = false;

		public override long GetValue()
		{
			if (_isRecursing)
				return 0;

			try
			{
				_isRecursing = true;

				long count = (long)From.DefaultValue;

				if (_expressionGet != null)
				{
					Dictionary<string, object> state = new Dictionary<string, object>();
					state["count"] = count;
					state["value"] = count;
					state["self"] = this._parent;

					object value = Scripting.EvalExpression(_expressionGet, state);
					count = Convert.ToInt64(value);
				}

				return count;
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

				if (Of == null)
				{
					logger.Error("Error, Of returned null");
					return null;
				}

                Array OfArray = Of as Array;

				if (OfArray == null)
				{
					logger.Error(
						string.Format("Count Relation requires '{0}' to be an array.  Set the minOccurs and maxOccurs properties.",
						OfName));

					return null;
				}

				int count = OfArray.Count;

				// Allow us to override the count of the array
				if (OfArray.overrideCount.HasValue)
					count = (int)OfArray.overrideCount;

				if (_expressionSet != null)
				{
					Dictionary<string, object> state = new Dictionary<string, object>();
					state["count"] = count;
					state["value"] = count;
					state["self"] = this._parent;

					object value = Scripting.EvalExpression(_expressionSet, state);
					count = Convert.ToInt32(value);
				}

				return new Variant(count);
			}
			finally
			{
				_isRecursing = false;
			}
		}

		public override void SetValue(Variant value)
		{
			int count = (int)value;

			if (_expressionSet != null)
			{
				Dictionary<string, object> state = new Dictionary<string, object>();
				state["count"] = count;
				state["value"] = count;
				state["self"] = this._parent;

				object newValue = Scripting.EvalExpression(_expressionSet, state);
				count = Convert.ToInt32(newValue);
			}

			_from.DefaultValue = new Variant(count);
		}
	}
}

// end
