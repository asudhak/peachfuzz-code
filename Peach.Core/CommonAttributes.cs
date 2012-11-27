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
		public string defaultVaue;

		[Obsolete("This constructor is obsolete")]
		public ParameterAttribute(string name, Type type, string description, bool required)
		{
			this.name = name;
			this.type = type;
			this.description = description;
			this.required = required;
			this.defaultVaue = null;
		}

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
			this.defaultVaue = null;
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
			this.defaultVaue = defaultValue;
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
	public class PluginAttribute : Attribute
	{
		public string Name { get; private set; }
		public Type Type { get; private set; }
		public bool IsDefault { get; private set; }

		public PluginAttribute(Type type, string name, bool isDefault)
		{
			this.Name = name;
			this.Type = type;
			this.IsDefault = isDefault;
		}
	}
}
