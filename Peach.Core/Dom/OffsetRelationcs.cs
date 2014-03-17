
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
	[Parameter("expressionGet", typeof(string), "Scripting expression that is run when getting the value", "")]
	[Parameter("expressionSet", typeof(string), "Scripting expression that is run when setting the value", "")]
	[Parameter("relative", typeof(bool), "Is the offset relative", "false")]
	[Parameter("relativeTo", typeof(string), "Element to compute value relative to", "")]
	public class OffsetRelation : Relation
	{
		private class RelativeBinding : Binding
		{
			OffsetRelation rel;

			public RelativeBinding(OffsetRelation rel, DataElement parent)
				: base(parent)
			{
				this.rel = rel;
			}

			protected override void OnResolve()
			{
				rel.OnRelativeToResolve();
			}
		}

		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		private Binding commonAncestor;
		private RelativeBinding relativeElement;

		private bool _isRecursing;

		public bool isRelativeOffset
		{
			get;
			set;
		}

		public string relativeTo
		{
			get
			{
				return relativeElement.OfName;
			}
			set
			{
				relativeElement.OfName = value;
			}
		}

		public OffsetRelation(DataElement parent)
			: base(parent)
		{
			commonAncestor = new Binding(parent);
			relativeElement = new RelativeBinding(this, parent);

			parent.relations.Add(commonAncestor);
			parent.relations.Add(relativeElement);
		}

		protected override void OnResolve()
		{
			if (!isRelativeOffset)
			{
				// Non-relative offsets are computed from the root
				commonAncestor.Of = From.getRoot();
			}
			else if (string.IsNullOrEmpty(relativeTo))
			{
				// If this is a relative offset but not relativeTo a specific item
				// the offset should be relative to 'From'
				FindCommonParent(From);
			}
		}

		private void FindCommonParent(DataElement from)
		{
			var parent = from.CommonParent(Of);
			if (parent == null)
				throw new PeachException("Error resolving offset relation on {0}, couldn't find common parent between {0} and {1}.".Fmt(From.debugName, from.debugName, Of.debugName));

			commonAncestor.Of = parent;
		}

		private void OnRelativeToResolve()
		{
			FindCommonParent(relativeElement.Of);
		}

		protected override void OnClear()
		{
			commonAncestor.Clear();
			relativeElement.Clear();
		}

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
					state["self"] = From;

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
				long offset = calculateOffset() / 8;

				if (_expressionSet != null)
				{
					Dictionary<string, object> state = new Dictionary<string, object>();
					state["offset"] = offset;
					state["value"] = offset;
					state["self"] = From;

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
				state["self"] = From;

				object newValue = Scripting.EvalExpression(_expressionSet, state);
				offset = Convert.ToInt32(newValue);
			}

			From.DefaultValue = new Variant(offset);
		}

		/// <summary>
		/// Caluclate the offset in bytes between two data elements.
		/// </summary>
		/// <returns>Returns the offset in bits between two elements.  Return can be negative.</returns>
		private long calculateOffset()
		{
			System.Diagnostics.Debug.Assert(_isRecursing);
			var where = commonAncestor.Of;
			if (where == null)
				Error("could not locate common ancestor");

			var stream = where.Value;

			long fromPosition = 0;
			long toPosition = 0;

			if (isRelativeOffset)
			{
				if (relativeElement.OfName == null)
				{
					if (!stream.TryGetPosition(From.fullName, out fromPosition))
						Error("couldn't locate position of {0}".Fmt(From.debugName));
				}
				else if (relativeElement.Of != null)
				{
					if (relativeElement.Of != where && !stream.TryGetPosition(relativeElement.Of.fullName, out fromPosition))
						Error("could't locate position of {0}".Fmt(relativeElement.Of.debugName));
				}
				else
				{
					Error("could't locate element '{0}'".Fmt(relativeElement.OfName));
				}
			}

			if (!stream.TryGetPosition(Of.fullName, out toPosition))
				Error("could't locate position of {0}".Fmt(Of.debugName));

			return toPosition - fromPosition;
		}

		private void Error(string error)
		{
			string msg = string.Format(
				"Error, unable to calculate offset between {0} and {1}, {2}.",
				From.debugName,
				Of.debugName,
				error);

			throw new PeachException(msg);
		}
	}
}

// end
