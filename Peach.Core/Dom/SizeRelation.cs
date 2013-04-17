
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
	/// Byte size relation.
	/// </summary>
	[Serializable]
	[Relation("size", true)]
	[Description("Byte size relation")]
	[Parameter("of", typeof(string), "Element used to generate relation value", "")]
	[Parameter("from", typeof(string), "Element that receives relation value", "")]
	[Parameter("expressionGet", typeof(string), "Scripting expression that is run when getting the value", "")]
	[Parameter("expressionSet", typeof(string), "Scripting expression that is run when setting the value", "")]
	public class SizeRelation : Relation
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger(); 

		protected bool _isRecursing = false;
		protected bool _isByteRelation = true;

		public override long GetValue()
		{
			if (_isRecursing)
				return 0;

			try
			{
				_isRecursing = true;
				long size = (long)From.DefaultValue;

				if (_expressionGet != null)
				{
					Dictionary<string, object> state = new Dictionary<string, object>();
					state["size"] = size;
					state["value"] = size;
					state["self"] = this._parent;

					object value = Scripting.EvalExpression(_expressionGet, state);
					size = Convert.ToInt64(value);
				}

				if (_isByteRelation)
					size = size * 8;

				return size;
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
				if (Of == null)
				{
					logger.Error("Error, Of returned null");
					return null;
				}

				_isRecursing = true;
				long size = Of.Value.LengthBits;

				if (_isByteRelation)
				{
					if (_expressionSet != null)
					{
						Dictionary<string, object> state = new Dictionary<string, object>();
						state["size"] = size / 8;
						state["value"] = size / 8;
						state["self"] = this._parent;

						object newValue = Scripting.EvalExpression(_expressionSet, state);
						size = Convert.ToInt64(newValue) * 8;
					}

					size = size / 8;
				}
				else
				{
					if (_expressionSet != null)
					{
						Dictionary<string, object> state = new Dictionary<string, object>();
						state["size"] = size;
						state["value"] = size;
						state["self"] = this._parent;

						object newValue = Scripting.EvalExpression(_expressionSet, state);
						size = Convert.ToInt64(newValue);
					}
				}
				
				return new Variant(size);
			}
			finally
			{
				_isRecursing = false;
			}
		}

		public override void SetValue(Variant value)
		{
			int size = (int)value;

			if (_expressionSet != null)
			{
				Dictionary<string, object> state = new Dictionary<string, object>();
				state["size"] = size / 8;
				state["value"] = size / 8;
				state["self"] = this._parent;

				object newValue = Scripting.EvalExpression(_expressionSet, state);
				size = Convert.ToInt32(newValue);
			}

			_from.DefaultValue = new Variant(size);
		}
	}
}

// end
