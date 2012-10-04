
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

namespace Peach.Core
{
	/// <summary>
	/// Helper class to determine the OS/Platform we are on.  The built in 
	/// method returns incorrect results.
	/// </summary>
	public static class Platform
	{
		[DllImport("libc")]
		static extern int uname(IntPtr buf);
		static private bool mIsWindows;
		static private bool mIsMac;
		
		public enum OS { Windows, Mac, Linux, unknown };
		
		static public OS GetOS()
		{
			if (mIsWindows = (System.IO.Path.DirectorySeparatorChar == '\\')) return OS.Windows;
			if (mIsMac = (!mIsWindows && IsRunningOnMac())) return OS.Mac;
			if (!mIsMac && System.Environment.OSVersion.Platform == PlatformID.Unix) return OS.Linux;
			return OS.unknown;
		}
		//From Managed.Windows.Forms/XplatUI
		static bool IsRunningOnMac()
		{
			IntPtr buf = IntPtr.Zero;
			try
			{
				buf = Marshal.AllocHGlobal(8192);
				// This is a hacktastic way of getting sysname from uname ()
				if (uname(buf) == 0)
				{
					string os = Marshal.PtrToStringAnsi(buf);
					if (os == "Darwin") return true;
				}
			}
			catch
			{
			}
			finally
			{
				if (buf != IntPtr.Zero) Marshal.FreeHGlobal(buf);
			}
			return false;
		}
	}

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

		static ClassLoader()
		{
			string[] searchPath = new string[] {
				Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
				Directory.GetCurrentDirectory(),
			};

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
						Assembly asm = Assembly.LoadFile(file);
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

	/// <summary>
	/// Some utility methods that be usefull
	/// </summary>
	public class Utilities
	{
		public bool TcpPortAvailable(int port)
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

	}
}

// end
