using System;
using System.Collections.Generic;
using System.Text;

namespace PeachCore.MutationStrategies
{
	[DefaultMutationStrategy]
	[MutationStrategy("Sequencial")]
	public class Sequencial : MutationStrategy
	{
		public Sequencial(Dictionary<string,string> args)
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
