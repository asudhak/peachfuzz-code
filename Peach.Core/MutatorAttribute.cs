using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Peach.Core
{
	/// <summary>
	/// Used to indicate a class is a valid Mutator and 
	/// provide it's invoking name used in the Pit XML file.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class MutatorAttribute : PluginAttribute
	{
		public MutatorAttribute(string name)
			: base(typeof(Mutator), name, true)
		{
		}
	}
}
