using System;
using System.Collections.Generic;
using System.Text;

namespace Peach.Core
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public class ParameterAttribute : Attribute
	{
		public string name { get; private set; }
		public Type type { get; private set; }
		public string description { get; private set; }
		public bool required { get; private set; }
		public string defaultValue { get; private set; }

		/// <summary>
		/// Constructs a REQUIRED parameter.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="type"></param>
		/// <param name="description"></param>
		/// <param name="defaultValue"></param>
		public ParameterAttribute(string name, Type type, string description)
		{
			this.name = name;
			this.type = type;
			this.description = description;
			this.required = true;
			this.defaultValue = null;
		}

		/// <summary>
		/// Constructs an OPTIONAL parameter.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="type"></param>
		/// <param name="description"></param>
		/// <param name="defaultValue"></param>
		public ParameterAttribute(string name, Type type, string description, string defaultValue)
		{
			if (defaultValue == null)
				throw new ArgumentNullException("defaultValue");

			this.name = name;
			this.type = type;
			this.description = description;
			this.required = false;
			this.defaultValue = defaultValue;
		}
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class DescriptionAttribute : Attribute
	{
		public string Description { get; private set; }

		public DescriptionAttribute(string description)
		{
			this.Description = description;
		}
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public abstract class PluginAttribute : Attribute
	{
		public string Name { get; private set; }
		public Type Type { get; private set; }
		public bool IsDefault { get; private set; }

		protected PluginAttribute(Type type, string name, bool isDefault)
		{
			this.Name = name;
			this.Type = type;
			this.IsDefault = isDefault;
		}
	}
}
