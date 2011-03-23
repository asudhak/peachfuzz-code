
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
using System.Text;
using Peach.Core.Dom;

namespace Peach.Core.Mutators
{
	[Mutator("Perform common string mutations")]
	public class StringMutator : Mutator
	{
		string[] values = new string[] {
			"Peachy",
			"A",
			"AAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
			"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
			"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"
		};

		uint pos = 0;
		uint lastPos = 0;

		public StringMutator()
		{
			pos = 0;
		}

		public new static bool supportedDataElement(DataElement obj)
		{
			if (obj is Dom.String)
				return true;

			return false;
		}

		public override void next()
		{
			pos++;
			if (pos >= values.Length)
			{
				pos = (uint) values.Length - 1;
				throw new MutatorCompleted();
			}
		}

		public override int count
		{
			get { return values.Length; }
		}

		public override void sequencialMutation(Dom.DataElement obj)
		{
			obj.MutatedValue = new Variant(values[pos]);
		}

		public override void randomMutation(Dom.DataElement obj)
		{
			throw new NotImplementedException();
		}
	}
}
