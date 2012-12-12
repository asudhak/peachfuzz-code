
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
using System.Linq;
using System.Text;
using Peach.Core;

namespace Peach.Core.Dom
{
	/// <summary>
	/// Hints are attached to data elements providing information
	/// for mutators.
	/// </summary>
	[Serializable]
	[DataElement("Hint")]
	[Parameter("Name", typeof(string), "Name of hint")]
	[Parameter("Value", typeof(string), "Value of hint")]
	public class Hint
	{
		public Hint(string name, string value)
		{
			Name = name;
			Value = value;
		}

		public Hint(Dictionary<string, Variant> args)
		{
			Name = (string)args["Name"];
			Value = (string)args["Value"];
		}

		public string Name
		{
			get;
			set;
		}

		public string Value
		{
			get;
			set;
		}
	}

	/// <summary>
	/// Used to indicate a mutator supports a type of Hint
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class HintAttribute : Attribute
	{
		public string name;
		public string description;

		public HintAttribute(string name, string description)
		{
			this.name = name;
			this.description = description;
		}
	}
}
