
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
	public abstract class Relation : Binding
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		protected string _expressionGet = null;
		protected string _expressionSet = null;

		public Relation(DataElement parent)
			: base(parent)
		{
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
