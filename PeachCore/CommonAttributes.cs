using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PeachCore
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ParameterAttribute : Attribute
	{
		public string name;
		public Type type;
		public string description;

		public ParameterAttribute(string name, Type type, string description)
		{
			this.name = name;
			this.type = type;
			this.description = description;
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class NoParametersAttribute : Attribute
	{
	}
}
