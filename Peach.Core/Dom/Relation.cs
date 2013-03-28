
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
using System.Diagnostics;

using NLog;

namespace Peach.Core.Dom
{
	/// <summary>
	/// Base class for all data element relations
	/// </summary>
	[Serializable]
	[DebuggerDisplay("Of={_ofName} From={_fromName}")]
	public abstract class Relation
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		protected DataElement _parent = null;
		protected DataElement _of = null;
		protected DataElement _from = null;

		protected string _ofName = null;
		protected string _fromName = null;
		protected string _expressionGet = null;
		protected string _expressionSet = null;

		/// <summary>
		/// Expression that is run when getting the value.
		/// </summary>
		/// <remarks>
		/// This expression is only run when the data cracker
		/// has identified a size relation exists and is getting
		/// the value from the "from" side of the relation.
		/// 
		/// The expressionGet will only get executed when direcly calling
		/// the Relation.GetValue() method directly.  It is not called from
		/// DataElement by design.
		/// </remarks>
		public string ExpressionGet
		{
			get { return _expressionGet; }
			set
			{
				if (string.Equals(_expressionGet, value))
					return;

				_expressionGet = value;
				if(From != null)
					From.Invalidate();
			}
		}

		/// <summary>
		/// Expression that is run when setting the value.
		/// </summary>
		/// <remarks>
		/// This expression can be called numerouse times.  It will be
		/// executed any time the attached data element re-generates it's
		/// value (internal or real).
		/// 
		/// The ExpressionSet is executed typically from DataElement.GenerateInteralValue() via
		/// Relation.CalculateFromValue().  As such this expression should limit the amount of
		/// time intensive tasks it performs.
		/// </remarks>
		public string ExpressionSet
		{
			get { return _expressionSet; }
			set
			{
				if (string.Equals(_expressionSet, value))
					return;

				_expressionSet = value;
				if (From != null)
					From.Invalidate();
			}
		}

		/// <summary>
		/// Parent of relation.  This is
		/// typically our From as well.
		/// </summary>
		/// <remarks>
		/// We are now adding the Relation to both our
		/// "from" and "of" side.  The meaning of parent is nolonger
		/// clear and should be removed in the future.
		/// </remarks>
		public DataElement parent
		{
			get { return _parent; }
			set
			{
				if (object.Equals(_parent, value))
					return;

				if (_parent != null)
				{
					_parent.Invalidate();
					_parent = null;
				}

				_parent = value;

				if (_parent != null)
				{
					_parent.Invalidate();
				}
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
				if (string.Equals(_ofName, value))
					return;

				if (_of != null)
					_of.Invalidated -= OfInvalidated;

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
				if (string.Equals(_fromName, value))
					return;

				if (_from != null)
					_from.Invalidate();

				_fromName = value;
				_from = null;
			}
		}

		public void Reset()
		{
			if (_of != null)
				_of.Invalidated -= OfInvalidated;

			_of = null;
			_from = null;
		}

		/// <summary>
		/// DataElement used to generate our value.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public DataElement Of
		{
			get
			{
				// When request we should evaluate

				if (_of == null && parent != null)
				{
					_of = parent.find(_ofName);

					if (_of == null)
					{
						logger.Error("Error, unable to resolve '" + _ofName + "' from relation attached to '" + parent.fullName + "'.");
						return null;
					}

					_of.Invalidated += new InvalidatedEventHandler(OfInvalidated);

					if (_from != null)
					{
						// Verify _of and _from don't share Choice as common parent
						if (FindCommonParent(_from, _of) is Choice)
						{
							logger.Error("Error, a Relation's 'of' and 'from' sides cannot share a common parent that is of type 'Choice'.  Relation: " + _of.fullName);
						}
					}
				}

				return _of;
			}
			set
			{
				if (object.Equals(_of, value))
					return;

				if (_of != null)
				{
					// Remove existing event
					_of.Invalidated -= OfInvalidated;
				}

				if (_from != null)
				{
					// Verify _of and _from don't share Choice as common parent
					if (FindCommonParent(value, _from) is Choice)
						throw new PeachException("Error, a Relation's 'of' and 'from' sides cannot share a common parent that is of type 'Choice'.  Relation: " + value.fullName);
				}

				_of = value;
				_of.Invalidated += new InvalidatedEventHandler(OfInvalidated);

				_ofName = _of.name;

				// We need to invalidate now that we have a new of.
				From.Invalidate();
			}
		}

		/// <summary>
		/// DataElement that receives our value
		/// when generated.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public DataElement From
		{
			get
			{
				if (_from == null)
				{
					if (_fromName != null)
					{
						_from = parent.find(_fromName);
					}
					else if (Of != null && Of != parent)
					{
						_from = parent;
						_fromName = _from.name;
					}

					if (_of != null)
					{
						// Verify _of and _from don't share Choice as common parent
						if (FindCommonParent(_from, _of) is Choice)
						{
							logger.Error("Error, a Relation's 'of' and 'from' sides cannot share a common parent that is of type 'Choice'.  Relation: " + _of.fullName);
						}
					}
				}

				return _from;
			}

			set
			{
				if (object.Equals(_from, value))
					return;

				if (_of != null)
				{
					// Verify _of and _from don't share Choice as common parent
					if (FindCommonParent(value, _of) is Choice)
					{
						logger.Error("Error, a Relation's 'of' and 'from' sides cannot share a common parent that is of type 'Choice'.  Relation: " + _of.fullName);
					}
				}

				_from = value;
				_fromName = _from.name;
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
		/// Calculate the new From value based on Of
		/// </summary>
		/// <remarks>
		/// This method is called every time our attached DataElement re-generates it's
		/// value by calling DataElement.GenerateInteralValue().
		/// </remarks>
		/// <returns></returns>
		public abstract Variant CalculateFromValue();

		/// <summary>
		/// Get value from our "from" side.
		/// </summary>
		/// <remarks>
		/// Gets the value from our "from" side and run it through expressionGet (if set).
		/// This method is only called by the DataCracker and never from DataElement.
		/// </remarks>
		public abstract long GetValue();

		/// <summary>
		/// Set value on from side
		/// </summary>
		/// <remarks>
		/// I'm not sure this method is used anymore.  It's been replaced by CalculateFromValue.
		/// 
		/// TODO - Remove me?
		/// </remarks>
		/// <param name="value"></param>
		public abstract void SetValue(Variant value);

		/// <summary>
		/// Find the first common parent between two DataElements
		/// </summary>
		/// <param name="elem1"></param>
		/// <param name="elem2"></param>
		/// <returns>Common parent of null</returns>
		public DataElement FindCommonParent(DataElement elem1, DataElement elem2)
		{
			List<DataElement> elem1Parents = new List<DataElement>();
			DataElementContainer parent = null;

			parent = elem1.parent;
			while(parent != null)
			{
				elem1Parents.Add(parent);
				parent = parent.parent;
			}
			
			parent = elem2.parent;
			while(parent != null)
			{
				if (elem1Parents.Contains(parent))
					return parent;

				parent = parent.parent;
			}
			
			return null;
		}

		private class Metadata
		{
			public DataElement of = null;
			public DataElement from = null;
			public DataElement parent = null;
			public string ofName = null;
			public string fromName = null;
		}

		[Serializable]
		private class FullNames
		{
			public string of = null;
			public string from = null;
			public string parent = null;
		}

		private FullNames _fullNames = null;

		[OnSerializing]
		private void OnSerializing(StreamingContext context)
		{
			DataElement.CloneContext ctx = context.Context as DataElement.CloneContext;
			if (ctx == null)
				return;

			if (DataElement.DebugClone)
				logger.Debug("Serializing From={0}, Of={1}",
					_of == null ? "(null) " + _ofName : _of.fullName,
					_from == null ? "(null) " + _fromName : _from.fullName);

			System.Diagnostics.Debug.Assert(_fullNames == null);
			System.Diagnostics.Debug.Assert(!ctx.metadata.ContainsKey(this));

			string relName;
			_fullNames = new FullNames();
			Metadata m = new Metadata();

			if (ctx.rename.Contains(_of))
			{
				m.ofName = _ofName;
				_ofName = ctx.newName;
			}
			else if (_of != null && !_of.isChildOf(ctx.root, out relName))
			{
				_fullNames.of = relName;
				ctx.elements[_fullNames.of] = _of;
				m.of = _of;
				_of = null;
			}
			else if (_ofName == ctx.oldName && parent.find(_ofName) == null)
			{
				if (_of == null && ctx.oldName == _ofName)
					_of = ctx.root;

				m.ofName = _ofName;
				_ofName = ctx.newName;
			}

			if (ctx.rename.Contains(_from))
			{
				m.fromName = _fromName;
				_fromName = ctx.newName;
			}
			else if (_from != null && !_from.isChildOf(ctx.root, out relName))
			{
				_fullNames.from = relName;
				ctx.elements[_fullNames.from] = _from;
				m.from = _from;
				_from = null;
			}
			else if (_fromName == ctx.oldName && parent.find(_fromName) == null)
			{
				if (_from == null && ctx.oldName == _fromName)
					_from = ctx.root;

				m.fromName = _fromName;
				_fromName = ctx.newName;
			}

			if (ctx.rename.Contains(_parent))
			{
				if (_of == null && _ofName == ctx.oldName)
				{
					m.ofName = _ofName;
					_ofName = ctx.newName;
				}
				if (_from == null && _fromName == ctx.oldName)
				{
					m.fromName = _fromName;
					_fromName = ctx.newName;
				}
			}
			else if (_parent != null && !_parent.isChildOf(ctx.root, out relName))
			{
				_fullNames.parent = relName;
				ctx.elements[_fullNames.parent] = _parent;
				m.parent = _parent;
				_parent = null;
			}

			if (m.from != null || m.of != null || m.parent != null || m.ofName != null || m.fromName != null)
				ctx.metadata.Add(this, m);
		}

		[OnSerialized]
		private void OnSerialized(StreamingContext context)
		{
			DataElement.CloneContext ctx = context.Context as DataElement.CloneContext;
			if (ctx == null)
				return;

			System.Diagnostics.Debug.Assert(_fullNames != null);
			_fullNames = null;

			object obj;
			if (!ctx.metadata.TryGetValue(this, out obj))
				return;

			Metadata m = obj as Metadata;

			if (m.of != null)
				this._of = m.of;
			if (m.from != null)
				this._from = m.from;
			if (m.parent != null)
				this._parent = m.parent;
			if (m.ofName != null)
				this._ofName = m.ofName;
			if (m.fromName != null)
				this._fromName = m.fromName;
		}

		[OnDeserializing]
		private void OnDeserializing(StreamingContext context)
		{
		}

		[OnDeserialized]
		private void OnDeserialized(StreamingContext context)
		{
			DataElement.CloneContext ctx = context.Context as DataElement.CloneContext;
			if (ctx == null)
				return;

			System.Diagnostics.Debug.Assert(_fullNames != null);

			if (_of == null && !string.IsNullOrEmpty(_fullNames.of))
			{
				System.Diagnostics.Debug.Assert(ctx.elements.ContainsKey(_fullNames.of));
				_of = ctx.elements[_fullNames.of];
				_of.relations.Add(this, false);
			}

			if (_from == null && !string.IsNullOrEmpty(_fullNames.from))
			{
				System.Diagnostics.Debug.Assert(ctx.elements.ContainsKey(_fullNames.from));
				_from = ctx.elements[_fullNames.from];
				_from.relations.Add(this, false);
			}

			if (_parent == null && !string.IsNullOrEmpty(_fullNames.parent))
			{
				System.Diagnostics.Debug.Assert(ctx.elements.ContainsKey(_fullNames.parent));
				_parent = ctx.elements[_fullNames.parent];
			}

			// Must always re-subscribe the invalidated event on deserialize
			if (_of != null)
				_of.Invalidated += new InvalidatedEventHandler(OfInvalidated);

			_fullNames = null;
		}
	}

	/// <summary>
	/// Used to indicate a class is a valid Relation and 
	/// provide it's invoking name used in the Pit XML file.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public class RelationAttribute : PluginAttribute
	{
		public RelationAttribute(string name, bool isDefault = false)
			: base(typeof(Relation), name, isDefault)
		{
		}
	}
}

// end
