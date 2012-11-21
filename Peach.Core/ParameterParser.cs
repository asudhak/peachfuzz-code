using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Net;

namespace Peach.Core
{
	public static class ParameterParser
	{
		public static void Parse<T>(T obj, Dictionary<string, Variant> args) where T: class
		{
			foreach (var attr in obj.GetType().GetAttributes<ParameterAttribute>(null))
			{
				Variant value;

				if (args.TryGetValue(attr.name, out value))
					ApplyProperty(obj, attr, (string)value);
				else if (!attr.required && attr.defaultVaue != null)
					ApplyProperty(obj, attr, attr.defaultVaue);
				else if (attr.required)
					RaiseError(obj, "is missing required parameter '{0}'.", attr.name);
			}
		}

		private static void ApplyProperty<T>(T obj, ParameterAttribute attr, string value) where T : class
		{
			object val = null;
			try
			{
				if (attr.type == typeof(IPAddress))
					val = IPAddress.Parse(value);
				else if (attr.type.IsEnum)
					val = Enum.Parse(attr.type, value);
				else
					val = ChangeType(obj, value, attr.type);
			}
			catch (Exception ex)
			{
				RaiseError(obj, "could not set parameter '{0}'.  {1}", attr.name, ex.Message);
			}

			BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
			var prop = obj.GetType().GetProperty(attr.name, bindingAttr, null, attr.type, new Type[0], null);
			if (prop == null)
				RaiseError(obj, "has no property for parameter '{0}'.", attr.name);
			else if (!prop.CanWrite)
				RaiseError(obj, "has no settable property for parameter '{0}'.", attr.name);
			else
				prop.SetValue(obj, val, null);
		}

		private static object ChangeType<T>(T obj, string value, Type type) where T : class
		{
			try
			{
				return Convert.ChangeType(value, type);
			}
			catch (InvalidCastException)
			{
			}

			// Find a converter on this type with the signature:
			// static void ParseParam(string str, out "type" val)

			BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
			Type[] types = new Type[] { typeof(string), type.MakeByRefType() };
			var method = obj.GetType().GetMethod("Parse", bindingAttr, Type.DefaultBinder, types, null);
			if (method == null || method.ReturnType != typeof(void) || !method.GetParameters()[1].IsOut)
				throw new InvalidCastException("No suitable method exists for converting a string to " + type.Name + ".");

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

		private static void RaiseError<T>(T obj, string fmt, params string[] args) where T : class
		{
			var attrs = obj.GetType().GetAttributes<PluginAttribute>(null);
			var attr = attrs.FirstOrDefault(a => a.IsDefault == true);
			if (attr == null) attr = attrs.First();

			string msg = string.Format("{0} '{1}' {2}", attr.Type.Name, attr.Name, string.Format(fmt, args));
			throw new PeachException(msg);
		}

	}
}