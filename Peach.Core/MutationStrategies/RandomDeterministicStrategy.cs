
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
	[MutationStrategy("RandomDeterministic", true)]
	[Serializable]
	public class RandomDeterministicStrategy : Sequential
	{
		uint _mapping = 0;
		SequenceGenerator sequence = null;

		public RandomDeterministicStrategy(Dictionary<string, Variant> args)
			: base(args)
		{
		}

		public override void Initialize(RunContext context, Engine engine)
		{
			base.Initialize(context, engine);
		}

		public override uint Iteration
		{
			get
			{
				return _mapping;
			}
			set
			{
				_mapping = value;

				if (!_context.controlIteration)
					base.Iteration = sequence.Get(value);
				else
					base.Iteration = value;
			}
		}

		protected override void OnDataModelRecorded()
		{
			// This strategy should randomize the order of mutators
			// that would be performed by the sequential mutation strategy.
			// The shuffle should always use the same seed.
			var rng = new Random(Seed);
			var elements = rng.Shuffle(_iterations.ToArray());
			_iterations.Clear();
			_iterations.AddRange(elements);

			if (this.Count > 0)
				sequence = new SequenceGenerator(this.Count);
		}
	}
}
