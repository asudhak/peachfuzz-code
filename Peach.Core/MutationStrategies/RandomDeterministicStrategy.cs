
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
using Peach.Core.Dom;
using System.Reflection;

namespace Peach.Core.MutationStrategies
{
	[DefaultMutationStrategy]
	[MutationStrategy("RandomDeterministic")]
	public class RandomDeterministicStrategy : Sequencial
	{
		public RandomDeterministicStrategy(Dictionary<string, Variant> args)
			: base(args)
		{
		}

		public override void Initialize(RunContext context, Engine engine)
		{
			base.Initialize(context, engine);
		}

		protected override void OnDataModelRecorded()
		{
			// This strategy should randomize the order of mutators
			// that would be performed by the sequencial mutation strategy.
			// The data model record pass only happens at iteration 0
			System.Diagnostics.Debug.Assert(Iteration == 0);

			var elements = Random.Shuffle(_iterations.ToArray());
			_iterations.Clear();
			_iterations.AddRange(elements);
		}
	}
}
