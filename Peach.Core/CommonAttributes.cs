using System;
using System.Collections.Generic;
using System.Text;

namespace Peach.Core
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public class ParameterAttribute : Attribute
	{
		public string name;
		public Type type;
		public string description;
		public bool required;

		public ParameterAttribute(string name, Type type, string description, bool required)
		{
			this.name = name;
			this.type = type;
			this.description = description;
			this.required = required;
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class NoParametersAttribute : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	public class PluginAttribute : Attribute
	{
		public string Name { get; set; }
		public PluginAttribute(string name)
		{
			this.Name = name;
		}
	}
}
