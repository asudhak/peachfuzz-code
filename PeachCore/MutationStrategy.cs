using System;
using System.Collections.Generic;
using System.Text;
using Peach.Core.MutationStrategies;

namespace Peach.Core
{
	/// <summary>
	/// Mutation strategies drive the fuzzing
	/// that Peach performs.  Creating a fuzzing
	/// strategy allows one to fully control which elements
	/// are mutated, by which mutators, and when.
	/// </summary>
	[Serializable]
	public abstract class MutationStrategy
	{
		public MutationStrategy(Dictionary<string, string> args)
		{
		}

		public virtual void Initialize(RunContext context, Engine engine)
		{
		}

		public bool isFinite
		{
			get { return false; }
		}

		public abstract uint count
		{
			get;
		}

		public abstract Mutator currentMutator();

		public abstract void next();
	}

	public class DefaultMutationStrategyAttribute : Attribute
	{
	}

	public class MutationStrategyAttribute : Attribute
	{
		public string name = null;
		public MutationStrategyAttribute(string name)
		{
			this.name = name;
		}
	}
}

// end
