
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
	public abstract class Relation : IPitSerializable
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		protected DataElement _parent = null;
		protected string _ofName = null;
		protected string _fromName = null;
		protected DataElement _of = null;
		protected DataElement _from = null;
		protected string _expressionGet = null;
		protected string _expressionSet = null;

		public void Reset()
		{
			_of = null;
			_from = null;
		}

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
				if (_parent != null)
				{
					_parent.Invalidate();
					_parent = null;
				}

				_parent = value;

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
				if (_of != null)
				{
					// Remove existing event
					_of.Invalidated -= new InvalidatedEventHandler(OfInvalidated);
				}

				if (_from != null)
				{
					// Verify _of and _from don't share Choice as common parent
					if (FindCommonParent(value, _from) is Choice)
						throw new PeachException("Error, a Relation's 'of' and 'from' sides cannot share a common parent that is of type 'Choice'.  Relation: " + value.fullName);
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
				if (_of != null)
				{
					// Verify _of and _from don't share Choice as common parent
					if (FindCommonParent(value, _of) is Choice)
					{
						logger.Error("Error, a Relation's 'of' and 'from' sides cannot share a common parent that is of type 'Choice'.  Relation: " + _of.fullName);
					}
				}

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

    public System.Xml.XmlNode pitSerialize(System.Xml.XmlDocument doc, System.Xml.XmlNode parent)
    {
      XmlNode node = doc.CreateNode(XmlNodeType.Element, "Relation", null);
      //type, of, from, when, expressionGet, expressionSet, relative, relativeTo

      node.AppendAttribute("type", this.GetType().ToString());
      node.AppendAttribute("of", this.OfName);
      node.AppendAttribute("from", this.FromName);
      //node.AppendAttribute("when", this.when);
      node.AppendAttribute("expressionGet", this.ExpressionGet);
      node.AppendAttribute("expressionSet", this.ExpressionSet);
      //node.AppendAttribute("relative", this.relative);
      //node.AppendAttribute("relativeTo", this.relativeTo);
      return node;
    }
  }
}

// end
