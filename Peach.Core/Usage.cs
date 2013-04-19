//
// Copyright (c) Michael Eddington
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in	
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

// Authors:
//   Michael Eddington (mike@dejavusecurity.com)

// $Id$

using System;
using System.Linq;
using System.Collections.Generic;

namespace Peach.Core
{
	public static class Usage
	{
		private class TypeComparer : IComparer<Type>
		{
			public int Compare(Type x, Type y)
			{
				return x.Name.CompareTo(y.Name);
			}
		}

		private class PluginComparer : IComparer<PluginAttribute>
		{
			public int Compare(PluginAttribute x, PluginAttribute y)
			{
				if (x.IsDefault == y.IsDefault)
					return x.Name.CompareTo(y.Name);

				if (x.IsDefault)
					return -1;

				return 1;
			}
		}

		private class ParamComparer : IComparer<ParameterAttribute>
		{
			public int Compare(ParameterAttribute x, ParameterAttribute y)
			{
				if (x.required == y.required)
					return x.name.CompareTo(y.name);

				if (x.required)
					return -1;

				return 1;
			}
		}

		public static void Print()
		{
			var color = Console.ForegroundColor;

			var domTypes = new SortedDictionary<string, Type>();

			foreach (var type in ClassLoader.GetAllByAttribute<Peach.Core.Dom.DataElementAttribute>(null))
			{
				if (domTypes.ContainsKey(type.Key.elementName))
				{
					PrintDuplicate("Data element", type.Key.elementName, domTypes[type.Key.elementName], type.Value);
					continue;
				}

				domTypes.Add(type.Key.elementName, type.Value);
			}

			var pluginsByName = new SortedDictionary<string, Type>();
			var plugins = new SortedDictionary<Type, SortedDictionary<Type, SortedSet<PluginAttribute>>>(new TypeComparer());

			foreach (var type in ClassLoader.GetAllByAttribute<Peach.Core.PluginAttribute>(null))
			{
				if (type.Key.IsTest)
					continue;

				var pluginType = type.Key.Type;

				string fullName = type.Key.Type.Name + ": " + type.Key.Name;
				if (pluginsByName.ContainsKey(fullName))
				{
					PrintDuplicate(type.Key.Type.Name, type.Key.Name, pluginsByName[fullName], type.Value);
					continue;
				}

				pluginsByName.Add(fullName, type.Value);

				if (!plugins.ContainsKey(pluginType))
					plugins.Add(pluginType, new SortedDictionary<Type, SortedSet<PluginAttribute>>(new TypeComparer()));

				var plugin = plugins[pluginType];

				if (!plugin.ContainsKey(type.Value))
					plugin.Add(type.Value, new SortedSet<PluginAttribute>(new PluginComparer()));

				var attrs = plugin[type.Value];

				bool added = attrs.Add(type.Key);
				System.Diagnostics.Debug.Assert(added);
			}

			Console.WriteLine("----- Data Elements --------------------------------------------");
			foreach (var elem in domTypes)
			{
				Console.WriteLine();
				Console.WriteLine("  {0}", elem.Key);
				PrintParams(elem.Value);
			}

			foreach (var kv in plugins)
			{
				Console.WriteLine();
				Console.WriteLine();
				Console.WriteLine("----- {0}s --------------------------------------------", kv.Key.Name);

				foreach (var plugin in kv.Value)
				{
					Console.WriteLine();
					Console.Write(" ");

					foreach (var attr in plugin.Value)
					{
						Console.Write(" ");
						if (attr.IsDefault)
							Console.ForegroundColor = ConsoleColor.White;
						Console.Write(attr.Name);
						Console.ForegroundColor = color;
					}

					Console.WriteLine();

					var desc = plugin.Key.GetAttributes<DescriptionAttribute>(null).FirstOrDefault();
					if (desc != null)
						Console.WriteLine("    [{0}]", desc.Description);

					PrintParams(plugin.Key);
				}
			}
		}

		private static void PrintDuplicate(string category, string name, Type type1, Type type2)
		{
			var color = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Red;

			if (type1 == type2)
			{
				// duplicate name on same type
				Console.WriteLine("{0} '{1}' declared more than once in assembly '{2}' class '{3}'.",
					category, name, type1.Assembly.Location, type1.FullName);
			}
			else
			{
				// duplicate name on different types
				Console.WriteLine("{0} '{1}' declared in assembly '{2}' class '{3}' and in assembly {4} and class '{5}'.",
					category, name, type1.Assembly.Location, type1.FullName, type2.Assembly.Location, type2.FullName);
			}

			Console.ForegroundColor = color;
			Console.WriteLine();
		}

		private static void PrintParams(Type elem)
		{
			var properties = new SortedSet<ParameterAttribute>(elem.GetAttributes<ParameterAttribute>(null), new ParamComparer());

			foreach (var prop in properties)
			{
				string value = "";
				if (!prop.required)
					value = string.Format(" default=\"{0}\"", prop.defaultValue.Replace("\r", "\\r").Replace("\n", "\\n"));

				string type;
				if (prop.type.IsGenericType && prop.type.GetGenericTypeDefinition() == typeof(Nullable<>))
					type = string.Format("({0}?)", prop.type.GetGenericArguments()[0].Name);
				else
					type = string.Format("({0})", prop.type.Name);

				Console.WriteLine("    {0} {1} {2} {3}.{4}", prop.required ? "*" : "-",
					prop.name.PadRight(24), type.PadRight(14), prop.description, value);
			}
		}
	}
}
