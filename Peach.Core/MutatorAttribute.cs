using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Peach.Core
{
	// Mark a class as a Peach Mutator
	public class MutatorAttribute : Attribute
	{
		public string description = null;

		public MutatorAttribute()
		{
			description = "Unknown";
		}

		public MutatorAttribute(string description)
		{
			this.description = description;
		}
	}
}
