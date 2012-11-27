using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Net;

namespace Peach.Core
{
	public static class ParameterParser
	{
		/// <summary>
		/// Parses a dictionary of arguments, similiar to python kwargs.
		/// For each parameter attribute on 'T', the appropriate property
		/// on 'obj' will be set. Eg, given integer parameter 'option1':
		/// obj.option1 = int.Parse(args["option1"])
		/// </summary>
		/// <typeparam name="T">Class type</typeparam>
		/// <param name="obj">Instance of class T</param>
		/// <param name="args">Dictionary of arguments</param>
		public static void Parse<T>(T obj, Dictionary<string, Variant> args) where T: class
		{
			foreach (var attr in obj.GetType().GetAttributes<ParameterAttribute>(null))
			{
				Variant value;

				if (args.TryGetValue(attr.name, out value))
					ApplyProperty(obj, attr, (string)value);
				else if (!attr.required)
					ApplyProperty(obj, attr, attr.defaultVaue);
				else if (attr.required)
					RaiseError(obj.GetType(), "is missing required parameter '{0}'.", attr.name);
			}
		}

		/// <summary>
		/// Will convert a string value to the type described in the ParameterAttribute.
		/// If an appropriate conversion function can not be found, this function will
		/// look for a static method on 'type' to perform the conversion.  For example,
		/// if the attribute type was class 'SomeClass', the function signature would be:
		/// static void ParseParam(string str, out SomeClass val)
		/// </summary>
		/// <param name="type">Object type that is decorated with the Parameter attribute.</param>
		/// <param name="attr">Parameter attribute describing the destination type.</param>
		/// <param name="value">String value to convert.</param>
		/// <returns></returns>
		public static object FromString(Type type, ParameterAttribute attr, string value)
		{
			object val = null;

			if (value == null)
			{
				if (attr.type.IsValueType)
					RaiseError(type, "could not set value type parameter '{0}' to 'null'.", attr.name);
			}
			else
			{
				try
				{
					if (attr.type == typeof(IPAddress))
						val = IPAddress.Parse(value);
					else if (attr.type.IsEnum)
						val = Enum.Parse(attr.type, value, true);
					else
						val = ChangeType(type, value, attr.type);
				}
				catch (Exception ex)
				{
					RaiseError(type, "could not set parameter '{0}'.  {1}", attr.name, ex.Message);
				}
			}

			return val;
		}

		private static void ApplyProperty<T>(T obj, ParameterAttribute attr, string value) where T : class
		{
			Type type = obj.GetType();

			object val = FromString(type, attr, value);

			BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
			var prop = obj.GetType().GetProperty(attr.name, bindingAttr, null, attr.type, new Type[0], null);
			if (prop == null)
				RaiseError(type, "has no property for parameter '{0}'.", attr.name);
			else if (!prop.CanWrite)
				RaiseError(type, "has no settable property for parameter '{0}'.", attr.name);
			else
				prop.SetValue(obj, val, null);
		}

		private static object ChangeType(Type ownerType, string value, Type destType)
		{
			try
			{
				return Convert.ChangeType(value, destType);
			}
			catch (InvalidCastException)
			{
			}

			// Find a converter on this type with the signature:
			// static void ParseParam(string str, out "type" val)

			BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
			Type[] types = new Type[] { typeof(string), destType.MakeByRefType() };
			var method = ownerType.GetMethod("Parse", bindingAttr, Type.DefaultBinder, types, null);
			if (method == null || method.ReturnType != typeof(void) || !method.GetParameters()[1].IsOut)
				throw new InvalidCastException("No suitable method exists for converting a string to " + destType.Name + ".");

			try
			{
				object[] parameters = new object[] { value, null };
				method.Invoke(null, parameters);
				System.Diagnostics.Debug.Assert(parameters[1] != null);
				return parameters[1];
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException;
			}
			catch (Exception)
			{
				throw;
			}
		}

		private static void RaiseError(Type type, string fmt, params string[] args)
		{
			var attrs = type.GetAttributes<PluginAttribute>(null);
			var attr = attrs.FirstOrDefault(a => a.IsDefault == true);
			if (attr == null) attr = attrs.First();

			string msg = string.Format("{0} '{1}' {2}", attr.Type.Name, attr.Name, string.Format(fmt, args));
			throw new PeachException(msg);
		}

	}
}