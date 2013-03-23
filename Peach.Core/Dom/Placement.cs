
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
	[DataElement("Placement")]
	[Parameter("after", typeof(string), "Place after this element", "")]
	[Parameter("before", typeof(string), "Place before this element", "")]
	public class Placement
	{
		public Placement(Dictionary<string, Variant> args)
		{
			if(args.ContainsKey("after"))
				after = (string)args["after"];
			if (args.ContainsKey("before"))
				before = (string)args["before"];

			if (string.IsNullOrEmpty(after) && string.IsNullOrEmpty(before))
				throw new PeachException("Error, Placement must have one of 'after' or 'before' defined.");
		}

		public string after
		{
			get;
			set;
		}

		public string before
		{
			get;
			set;
		}
	}
}
