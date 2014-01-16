using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Peach.Core
{
	// Mark a class as a Peach Mutator
	public class MutatorAttribute : PluginAttribute
	{
		public MutatorAttribute(string name)
			: base(typeof(Mutator), name, true)
		{
		}
	}
}
