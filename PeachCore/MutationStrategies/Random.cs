using System;
using System.Collections.Generic;
using System.Text;

namespace Peach.Core.MutationStrategies
{
	public class Random : MutationStrategy
	{
		public Random(Dictionary<string, string> args)
			: base(args)
		{
		}

		public override uint count
		{
			get { throw new NotImplementedException(); }
		}

		public override Mutator currentMutator()
		{
			throw new NotImplementedException();
		}

		public override void next()
		{
			throw new NotImplementedException();
		}
	}
}

// end
