
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

using Peach.Core;
using Peach.Core.IO;

using NLog;

namespace Peach.Core.Dom
{

	/// <summary>
	/// Byte offset relation
	/// </summary>
	[Serializable]
	[Relation("offset", true)]
	[Description("Byte offset relation")]
	[Parameter("of", typeof(string), "Element used to generate relation value", "")]
	[Parameter("from", typeof(string), "Element that receives relation value", "")]
	[Parameter("expressionGet", typeof(string), "Scripting expression that is run when getting the value", "")]
	[Parameter("expressionSet", typeof(string), "Scripting expression that is run when setting the value", "")]
	[Parameter("relative", typeof(bool), "Is the offset relative", "false")]
	[Parameter("relativeTo", typeof(string), "Element to compute value relative to", "")]
	public class OffsetRelation : Relation
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();
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
				if (Of == null)
				{
					logger.Error("Error, Of returned null");
					return null;
				}

				_isRecursing = true;

				// calculateOffset can throw PeachException during mutations
				// we will catch and return null;
				long offset = calculateOffset(From, Of) / 8;

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
			catch (PeachException ex)
			{
				logger.Error(ex.Message);
				return null;
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
		/// <returns>Returns the offset in bits between two elements.  Return can be negative.</returns>
		protected long calculateOffset(DataElement from, DataElement to)
		{
			DataElementContainer commonAncestor = null;
			long fromPosition = 0;
			long toPosition = 0;

			if (isRelativeOffset)
			{
				if (!string.IsNullOrEmpty(relativeTo))
				{
					DataElement relative = from.find(relativeTo);
					if (relative == null)
						throw new PeachException(string.Format("Error, offset relation from element '{0}' couldn't locate relative to element '{1}'.", from.fullName, relativeTo));
					from = relative;
				}

				commonAncestor = findCommonRoot(from, to);

				if (commonAncestor == null)
				{
					throw new PeachException("Error, unable to calculate offset between '" +
						from.fullName + "' and '" + to.fullName + "'.");
				}

				BitStream stream = commonAncestor.Value;
				if (from != commonAncestor)
				{
					if (!stream.HasDataElement(from.fullName))
						throw new PeachException("Error, unable to calculate offset between '" +
							from.fullName + "' and '" + to.fullName + "'.");

					fromPosition = stream.DataElementPosition(from);
				}

				if (!stream.HasDataElement(to.fullName))
					throw new PeachException("Error, unable to calculate offset between '" +
						from.fullName + "' and '" + to.fullName + "'.");

				toPosition = stream.DataElementPosition(to);
			}
			else
			{
				commonAncestor = findCommonRoot(from, to);
				if (commonAncestor == null)
					throw new PeachException("Error, unable to calculate offset between '" +
						from.fullName + "' and '" + to.fullName + "'.");

				BitStream stream = commonAncestor.Value;
				fromPosition = 0;

				if (!stream.HasDataElement(to.fullName))
					throw new PeachException("Error, unable to calculate offset between '" +
						from.fullName + "' and '" + to.fullName + "'.");

				toPosition = stream.DataElementPosition(to);
			}

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

			if (elem1 is DataElementContainer)
				parentsElem1.Add((DataElementContainer)elem1);

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

		[NonSerialized]
		private string tempRelativeTo = null;

		[OnSerializing]
		private void OnSerializing(StreamingContext context)
		{
			DataElement.CloneContext ctx = context.Context as DataElement.CloneContext;
			if (ctx == null)
				return;

			if (DataElement.DebugClone)
				logger.Debug("Serializing relativeTo={0}", relativeTo);

			if (string.IsNullOrEmpty(relativeTo))
				return;

			var from = _from;
			if (_from == null)
			{
				if (_fromName != null)
					from = parent.find(_fromName);
				else
					from = parent;
			}

			var elem = from.find(relativeTo);
			if (elem == null && relativeTo == ctx.oldName)
			{
				tempRelativeTo = relativeTo;
				relativeTo = ctx.newName;
			}
		}

		[OnSerialized]
		private void OnSerialized(StreamingContext context)
		{
			DataElement.CloneContext ctx = context.Context as DataElement.CloneContext;
			if (ctx == null)
				return;

			if (tempRelativeTo != null)
				relativeTo = tempRelativeTo;

			tempRelativeTo = null;
		}
	}
}

// end
