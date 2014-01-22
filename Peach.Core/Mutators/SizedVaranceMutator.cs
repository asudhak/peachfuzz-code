
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
using System.Text;
using Peach.Core.IO;
using Peach.Core.Dom;
using NLog;

namespace Peach.Core.Mutators
{
	[Mutator("SizedVaranceMutator")]
	[Description("Change the length of sizes to count - N to count + N")]
	[Hint("SizedVaranceMutator-N", "Gets N by checking node for hint, or returns default (50).")]
	public class SizedVaranceMutator : SizedMutator
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		public SizedVaranceMutator(DataElement obj)
			: base("SizedVaranceMutator", obj)
		{
		}

		protected override NLog.Logger Logger
		{
			get { return logger; }
		}

		protected override bool OverrideRelation
		{
			get { return false; }
		}

		protected override List<long> GenerateValues(DataElement obj, int n)
		{
			// Find all numbers from [-n, n] where (originalDataLength + n) > 0
			// TODO: See if we want to exclude mutations where our size will be 0
			long min = (int)Math.Max(-(long)obj.InternalValue + 1, -n);

			var ret = new List<long>();
			while (min <= n)
				ret.Add(min++);

			return ret;
		}
	}
}
