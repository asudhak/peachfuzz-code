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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using NLog;
using System.Security;
using System.Security.Policy;

namespace Peach.Core
{
	/// <summary>
	/// Methods for finding and creating instances of 
	/// classes.
	/// </summary>
	public static class ClassLoader
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		public static Dictionary<string, Assembly> AssemblyCache = new Dictionary<string, Assembly>();
		static string[] searchPath = GetSearchPath();

		static string[] GetSearchPath()
		{
			var ret = new List<string> {
				Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
				Directory.GetCurrentDirectory(),
			};

			string devpath = Environment.GetEnvironmentVariable("DEVPATH");
			if (!string.IsNullOrEmpty(devpath))
				ret.AddRange(devpath.Split(Path.PathSeparator));

			string mono_path = Environment.GetEnvironmentVariable("MONO_PATH");
			if (!string.IsNullOrEmpty(mono_path))
				ret.AddRange(mono_path.Split(Path.PathSeparator));

			return ret.ToArray();
		}

		static ClassLoader()
		{
			foreach (string path in searchPath)
			{
				foreach (string file in Directory.GetFiles(path))
				{
					if (!file.EndsWith(".exe") && !file.EndsWith(".dll"))
						continue;

					if (AssemblyCache.ContainsKey(file))
						continue;

					try
					{
						Assembly asm = Load(file);
						asm.GetExportedTypes(); // make sure we can load exported types.
						AssemblyCache.Add(file, asm);
					}
					catch (Exception ex)
					{
						logger.Debug("ClassLoader skipping \"{0}\", {1}", file, ex.Message);
					}
				}
			}
		}

		static Assembly Load(string fullPath)
		{
			if (!File.Exists(fullPath))
				throw new FileNotFoundException("The file \"" + fullPath + "\" does not exist.");

			try
			{
				// Always try and load the assembly first. It will succeed regardless of security
				// zone if it is directly referenced or loadFromRemoteSources is true.
				Assembly asm = Assembly.LoadFrom(fullPath);
				return asm;
			}
			catch (Exception ex)
			{
				// http://mikehadlow.blogspot.com/2011/07/detecting-and-changing-files-internet.html
				var zone = Zone.CreateFromUrl(fullPath);
				if (zone.SecurityZone > SecurityZone.MyComputer)
					throw new SecurityException("The assemly is part of the " + zone.SecurityZone + " Security Zone and loading has been blocked.", ex);

				throw;
			}
		}

		static bool TryLoad(string fullPath)
		{
			if (!File.Exists(fullPath))
				return false;

			if (!AssemblyCache.ContainsKey(fullPath))
			{
				var asm = Load(fullPath);
				asm.GetExportedTypes(); // make sure we can load exported types.
				AssemblyCache.Add(fullPath, asm);
			}

			return true;
		}

		public static string FindFile(string fileName)
		{
			if (Path.IsPathRooted(fileName))
			{
				if (File.Exists(fileName))
					return fileName;
			}
			else
			{
				foreach (string path in searchPath)
				{
					string fullPath = Path.Combine(path, fileName);

					if (File.Exists(fullPath))
						return fullPath;
				}
			}

			throw new FileNotFoundException();
		}

		public static void LoadAssembly(string fileName)
		{
			if (Path.IsPathRooted(fileName))
			{
				if (TryLoad(fileName))
					return;
			}
			else
			{
				foreach (string path in searchPath)
				{
					if (TryLoad(Path.Combine(path, fileName)))
						return;
				}
			}

			throw new FileNotFoundException();
		}

		/// <summary>
		/// Extension to the Type class. Return all attributes matching the specified type and predicate.
		/// </summary>
		/// <typeparam name="A">Attribute type to find.</typeparam>
		/// <param name="type">Type in which the search should run over.</param>
		/// <param name="predicate">Returns an attribute if the predicate returns true or the predicate itself is null.</param>
		/// <returns>A generator which yields the attributes specified.</returns>
		public static IEnumerable<A> GetAttributes<A>(this Type type, Func<Type, A, bool> predicate)
			where A : Attribute
		{
			foreach (var attr in type.GetCustomAttributes(true))
			{
				var concrete = attr as A;
				if (concrete != null && (predicate == null || predicate(type, concrete)))
				{
					yield return concrete;
				}
			}
		}

		/// <summary>
		/// Finds all types that are decorated with the specified Attribute type and matches the specified predicate.
		/// </summary>
		/// <typeparam name="A">Attribute type to find.</typeparam>
		/// <param name="predicate">Returns a value if the predicate returns true or the predicate itself is null.</param>
		/// <returns>A generator which yields KeyValuePair elements of custom attribute and type found.</returns>
		public static IEnumerable<KeyValuePair<A, Type>> GetAllByAttribute<A>(Func<Type, A, bool> predicate)
			where A : Attribute
		{
			foreach (var asm in ClassLoader.AssemblyCache.Values)
			{
				if (asm.IsDynamic)
					continue;

				foreach (var type in asm.GetExportedTypes())
				{
					if (!type.IsClass)
						continue;

					foreach (var x in type.GetAttributes<A>(predicate))
					{
						yield return new KeyValuePair<A, Type>(x, type);
					}
				}
			}
		}

		/// <summary>
		/// Finds all types that are decorated with the specified Attribute type and matches the specified predicate.
		/// </summary>
		/// <typeparam name="A">Attribute type to find.</typeparam>
		/// <param name="predicate">Returns a value if the predicate returns true or the predicate itself is null.</param>
		/// <returns>A generator which yields elements of the type found.</returns>
		public static IEnumerable<Type> GetAllTypesByAttribute<A>(Func<Type, A, bool> predicate)
			where A : Attribute
		{
			return GetAllByAttribute<A>(predicate).Select(x => x.Value);
		}

		/// <summary>
		/// Finds the first type that matches the specified query.
		/// </summary>
		/// <typeparam name="A">Attribute type to find.</typeparam>
		/// <param name="predicate">Returns a value if the predicate returns true or the predicate itself is null.</param>
		/// <returns>KeyValuePair of custom attribute and type found.</returns>
		public static KeyValuePair<A, Type> FindByAttribute<A>(Func<Type, A, bool> predicate)
			where A : Attribute
		{
			return GetAllByAttribute<A>(predicate).FirstOrDefault();
		}

		/// <summary>
		/// Finds the first type that matches the specified query.
		/// </summary>
		/// <typeparam name="A">Attribute type to find.</typeparam>
		/// <param name="predicate">Returns a value if the predicate returns true or the predicate itself is null.</param>
		/// <returns>Returns only the Type found.</returns>
		public static Type FindTypeByAttribute<A>(Func<Type, A, bool> predicate)
			where A : Attribute
		{
			return GetAllByAttribute<A>(predicate).FirstOrDefault().Value;
		}

		/// <summary>
		/// Find and create and instance of class by parent type and 
		/// name.
		/// </summary>
		/// <typeparam name="T">Return Type.</typeparam>
		/// <param name="name">Name of type.</param>
		/// <returns>Returns a new instance of found type, or null.</returns>
		public static T FindAndCreateByTypeAndName<T>(string name)
			where T : class
		{
			foreach (var asm in ClassLoader.AssemblyCache.Values)
			{
				if (asm.IsDynamic)
					continue;

				Type type = asm.GetType(name);
				if (type == null)
					continue;

				if (!type.IsClass)
					continue;

				if (!type.IsSubclassOf(type))
					continue;

				return Activator.CreateInstance(type) as T;
			}

			return null;
		}
	}
}
