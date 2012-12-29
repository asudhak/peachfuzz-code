
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
using System.Net;
using System.Net.NetworkInformation;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using NLog;
using System.Security;
using System.Security.Policy;

namespace Peach.Core
{
	/// <summary>
	/// Helper class to add a debug listener so asserts get written to the console.
	/// </summary>
	public class AssertWriter : System.Diagnostics.TraceListener
	{
		public static void Register()
		{
			System.Diagnostics.Debug.Listeners.Insert(0, new AssertWriter());
		}

		public override void Write(string message)
		{
			Console.Write(message);
		}

		public override void WriteLine(string message)
		{
			Console.WriteLine("Assertion {0}", message);
			Console.WriteLine(new System.Diagnostics.StackTrace(2, true));
		}
	}

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

			// http://mikehadlow.blogspot.com/2011/07/detecting-and-changing-files-internet.html
			var zone = Zone.CreateFromUrl(fullPath);
			if (zone.SecurityZone > SecurityZone.MyComputer)
				throw new SecurityException("The assemly is part of the " + zone.SecurityZone + " Security Zone and loading has been blocked.");

			Assembly asm = Assembly.LoadFrom(fullPath);
			return asm;
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

    /// <summary>
    /// A simple number generation class.
    /// </summary>
    public static class NumberGenerator
    {
        /// <summary>
        /// Generate a list of numbers around size edge cases.
        /// </summary>
        /// <param name="size">The size (in bits) of the data</param>
        /// <param name="n">The +/- range number</param>
        /// <returns>Returns a list of all sizes to be used</returns>
        public static long[] GenerateBadNumbers(int size, int n = 50)
        {
            if (size == 8)
                return BadNumbers8(n);
            else if (size == 16)
                return BadNumbers16(n);
            else if (size == 24)
                return BadNumbers24(n);
            else if (size == 32)
                return BadNumbers32(n);
            else if (size == 64)
                return BadNumbers64(n);
            else
                throw new ArgumentOutOfRangeException("size");
        }

        public static long[] GenerateBadPositiveNumbers(int size = 16, int n = 50)
        {
            if (size == 16)
                return BadPositiveNumbers16(n);
            else
                return null;
        }

        public static ulong[] GenerateBadPositiveUInt64(int n = 50)
        {
            ulong[] edgeCases = new ulong[] { 50, 127, 255, 32767, 65535, 2147483647, 4294967295, 9223372036854775807, 18446744073709551615 };
            List<ulong> temp = new List<ulong>();

            ulong start;
            ulong end;
            for (int i = 0; i < edgeCases.Length - 1; ++i)
            {
                start = edgeCases[i] - (ulong)n;
                end = edgeCases[i] + (ulong)n;

                for (ulong j = start; j <= end; ++j)
                    temp.Add(j);
            }

            start = edgeCases[8] - (ulong)n;
            end = edgeCases[8];
            for (ulong i = start; i < end; ++i)
                temp.Add(i);
            temp.Add(end);

            return temp.ToArray();
        }

        private static long[] BadNumbers8(int n)
        {
            long[] edgeCases = new long[] { 0, -128, 127, 255 };
            return Populate(edgeCases, n);
        }

        private static long[] BadNumbers16(int n)
        {
            long[] edgeCases = new long[] { 0, -128, 127, 255, -32768, 32767, 65535 };
            return Populate(edgeCases, n);
        }

        private static long[] BadNumbers24(int n)
        {
            long[] edgeCases = new long[] { 0, -128, 127, 255, -32768, 32767, 65535, -8388608, 8388607, 16777215 };
            return Populate(edgeCases, n);
        }

        private static long[] BadNumbers32(int n)
        {
            long[] edgeCases = new long[] { 0, -128, 127, 255, -32768, 32767, 65535, -2147483648, 2147483647, 4294967295 };
            return Populate(edgeCases, n);
        }

        private static long[] BadNumbers64(int n)
        {
            long[] edgeCases = new long[] { 0, -128, 127, 255, -32768, 32767, 65535, -2147483648, 2147483647, 4294967295, -9223372036854775808, 9223372036854775807 };    // UInt64.Max = 18446744073709551615;
            return Populate(edgeCases, n);
        }

        private static long[] BadPositiveNumbers16(int n)
        {
            long[] edgeCases = new long[] { 50, 127, 255, 32767, 65535 };
            return Populate(edgeCases, n);
        }

        private static long[] Populate(long[] values, int n)
        {
            List<long> temp = new List<long>();

            for (int i = 0; i < values.Length; ++i)
            {
                long start = values[i] - n;
                long end = values[i] + n;

                for (long j = start; j <= end; ++j)
                    temp.Add(j);
            }

            return temp.ToArray();
        }
    }

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

	/// <summary>
	/// Some utility methods that be usefull
	/// </summary>
	public class Utilities
	{
        public static string FormatAsPrettyHex(byte[] data, int startPos = 0, int length = -1)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder rightSb = new StringBuilder();
            int lineLength = 15;
            int groupLength = 7;
            string gap = "  ";
            byte b;

            if (length == -1)
                length = data.Length;

            int cnt = 0;
            for (int i = startPos; i<data.Length && i<length; i++)
            {
                b = data[i];

                sb.Append(b.ToString("X2"));

                if (b >= 32 && b < 127)
                    rightSb.Append(ASCIIEncoding.ASCII.GetString(new byte[] {b}));
                else
                    rightSb.Append(".");


                if (cnt == groupLength)
                {
                    sb.Append("  ");
                }
                else if (cnt == lineLength)
                {
                    sb.Append(gap);
                    sb.Append(rightSb);
                    sb.Append("\n");
                    rightSb.Clear();

                    cnt = -1; // (+1 happens later)
                }
                else
                {
                    sb.Append(" ");
                }

                cnt++;
            }

            for (; cnt <= lineLength; cnt++)
            {
                sb.Append("  ");

                if (cnt == groupLength)
                    sb.Append(" ");
                else if (cnt < lineLength)
                {
                    sb.Append(" ");
                }
            }

            sb.Append(gap);
            sb.Append(rightSb);
            sb.Append("\n");
            rightSb.Clear();

            return sb.ToString();
        }

		public static bool TcpPortAvailable(int port)
		{
			bool isAvailable = true;

			IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
			TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

			foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
			{
				if (tcpi.LocalEndPoint.Port == port)
				{
					isAvailable = false;
					break;
				}
			}

			IPEndPoint[] objEndPoints = ipGlobalProperties.GetActiveTcpListeners();

			foreach (IPEndPoint endp in objEndPoints)
			{
				if (endp.Port == port)
				{
					isAvailable = false;
					break;
				}
			}

			return isAvailable;
		}

		public static Encoding GetXmlEncoding(string xml, Encoding def)
		{
			// Look for <?xml encoding="xxx"?> - return def if not found

			try
			{
				var re = new Regex("^<\\?xml.+?encoding=[\"']([^\"']+)[\"'].*?\\?>");
				var m = re.Match(xml);
				if (m.Success)
				{
					string enc = m.Groups[1].Value;
					def = Encoding.GetEncoding(enc);
				}
			}
			catch
			{
			}

			return def;
		}

		// Slightly tweaked from:
		// http://www.codeproject.com/Articles/36747/Quick-and-Dirty-HexDump-of-a-Byte-Array
		private delegate void HexOutputFunc(char[] line);

		private static void HexDump(Stream data, HexOutputFunc output, int bytesPerLine = 16)
		{
			System.Diagnostics.Debug.Assert(data != null);
			long pos = data.Position;
			long bytesLength = data.Length - pos;
			byte[] bytes = new byte[bytesPerLine];

			char[] HexChars = "0123456789ABCDEF".ToCharArray();

			int firstHexColumn =
				  8                   // 8 characters for the address
				+ 3;                  // 3 spaces

			int firstCharColumn = firstHexColumn
				+ bytesPerLine * 3       // - 2 digit for the hexadecimal value and 1 space
				+ (bytesPerLine - 1) / 8 // - 1 extra space every 8 characters from the 9th
				+ 2;                  // 2 spaces 

			int lineLength = firstCharColumn
				+ bytesPerLine           // - characters to show the ascii value
				+ Environment.NewLine.Length; // Carriage return and line feed (should normally be 2)

			char[] line = (new System.String(' ', lineLength - Environment.NewLine.Length) + Environment.NewLine).ToCharArray();

			for (int i = 0; i < bytesLength; i += bytesPerLine)
			{
				line[0] = HexChars[(i >> 28) & 0xF];
				line[1] = HexChars[(i >> 24) & 0xF];
				line[2] = HexChars[(i >> 20) & 0xF];
				line[3] = HexChars[(i >> 16) & 0xF];
				line[4] = HexChars[(i >> 12) & 0xF];
				line[5] = HexChars[(i >> 8) & 0xF];
				line[6] = HexChars[(i >> 4) & 0xF];
				line[7] = HexChars[(i >> 0) & 0xF];

				int hexColumn = firstHexColumn;
				int charColumn = firstCharColumn;

				int readLen = data.Read(bytes, 0, bytesPerLine);

				for (int j = 0; j < bytesPerLine; j++)
				{
					if (j > 0 && (j & 7) == 0) hexColumn++;
					if (j >= readLen)
					{
						line[hexColumn] = ' ';
						line[hexColumn + 1] = ' ';
						line[charColumn] = ' ';
					}
					else
					{
						byte b = bytes[j];
						line[hexColumn] = HexChars[(b >> 4) & 0xF];
						line[hexColumn + 1] = HexChars[b & 0xF];
						line[charColumn] = (b < 32 ? '·' : (char)b);
					}
					hexColumn += 3;
					charColumn++;
				}

				output(line);
			}

			data.Seek(pos, SeekOrigin.Begin);
		}

		public static void HexDump(Stream data, Stream output, int bytesPerLine = 16)
		{
			HexOutputFunc func = delegate(char[] line)
			{
				byte[] buf = Encoding.ASCII.GetBytes(line);
				output.Write(buf, 0, buf.Length);
			};

			HexDump(data, func, bytesPerLine);
		}

		public static string HexDump(Stream data, int bytesPerLine = 16)
		{
			StringBuilder sb = new StringBuilder();
			HexOutputFunc func = delegate(char[] line) { sb.Append(line); };
			HexDump(data, func, bytesPerLine);
			return sb.ToString();
		}
	}

	/// <summary>
	/// Extention of WebClient that supports cookies
	/// </summary>
	class WebClientEx : WebClient
	{
		public WebClientEx()
		{
			this.container = new CookieContainer();
		}

		public WebClientEx(CookieContainer container)
		{
			this.container = container;
		}

		private readonly CookieContainer container = new CookieContainer();

		protected override WebRequest GetWebRequest(Uri address)
		{
			WebRequest r = base.GetWebRequest(address);
			var request = r as HttpWebRequest;
			if (request != null)
			{
				request.CookieContainer = container;
			}
			return r;
		}

		protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
		{
			WebResponse response = base.GetWebResponse(request, result);
			ReadCookies(response);
			return response;
		}

		protected override WebResponse GetWebResponse(WebRequest request)
		{
			WebResponse response = base.GetWebResponse(request);
			ReadCookies(response);
			return response;
		}

		private void ReadCookies(WebResponse r)
		{
			var response = r as HttpWebResponse;
			if (response != null)
			{
				CookieCollection cookies = response.Cookies;
				container.Add(cookies);
			}
		}
	}

	/// <summary>
	/// Required for coping non-DataElements.
	/// </summary>
	/// <remarks>
	/// Reference Article http://www.codeproject.com/KB/tips/SerializedObjectCloner.aspx
	/// Provides a method for performing a deep copy of an object.
	/// Binary Serialization is used to perform the copy.
	/// </remarks>
	public static class ObjectCopier
	{
		/// <summary>
		/// Perform a deep Copy of the object.
		/// </summary>
		/// <typeparam name="T">The type of object being copied.</typeparam>
		/// <param name="source">The object instance to copy.</param>
		/// <returns>The copied object.</returns>
		public static T Clone<T>(T source)
		{
			if (!typeof(T).IsSerializable)
			{
				throw new ArgumentException("The type must be serializable.", "source");
			}

			// Don't serialize a null object, simply return the default for that object
			if (Object.ReferenceEquals(source, null))
			{
				return default(T);
			}

			IFormatter formatter = new BinaryFormatter();
			Stream stream = new MemoryStream();
			using (stream)
			{
				formatter.Serialize(stream, source);
				stream.Seek(0, SeekOrigin.Begin);
				return (T)formatter.Deserialize(stream);
			}
		}
	}    
}

// end
