
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
			catch { }
			finally
			{
				if (buf != IntPtr.Zero) Marshal.FreeHGlobal(buf);
			}
			return false;
		}
	}

	/// <summary>
	/// Provides a method for performing a deep copy of an object.
	/// Binary Serialization is used to perform the copy.
	/// </summary>
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
				T obj = (T)formatter.Deserialize(stream);

				return obj;
			}
		}
	}

	/// <summary>
	/// Methods for finding and creating instances of 
	/// classes.
	/// </summary>
	public static class ClassLoader
	{
		public static List<string> AssemblyFilenames = new List<string>();
		public static List<string> SearchPaths = new List<string>();

		static ClassLoader()
		{
			SearchPaths.Add(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
			SearchPaths.Add(Directory.GetCurrentDirectory());
		}

		/// <summary>
		/// Look through our search paths for assemblies to load.
		/// </summary>
		public static void UpdateAssemblyCache()
		{
			foreach (string path in SearchPaths)
			{
				foreach (string file in Directory.GetFiles(path, "*.exe"))
				{
					if (!AssemblyFilenames.Contains(file))
					{
						AssemblyFilenames.Add(file);
						try
						{
							Assembly asm = Assembly.LoadFile(file);
						}
						catch
						{
						}
					}
				}
				foreach (string file in Directory.GetFiles(path, "*.dll"))
				{
					if (!AssemblyFilenames.Contains(file))
					{
						AssemblyFilenames.Add(file);
						try
						{
							Assembly asm = Assembly.LoadFile(file);
						}
						catch
						{
						}
					}
				}
			}
		}

		/// <summary>
		/// Try to create instance of a class based on an attribute type
		/// and name.
		/// </summary>
		/// <param name="type">Attribute type</param>
		/// <param name="name">Class name</param>
		/// <returns>Returns new instance of found class, or null.</returns>
		public static object FindAndCreateByAttributeAndName(Type type, string name)
		{
			UpdateAssemblyCache();

			foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
			{
				Type found = a.GetType(name, false, false);
				if (!found.IsClass)
					continue;

				object [] attrs = found.GetCustomAttributes(type, true);
				if (attrs.Length == 0)
					continue;

				ConstructorInfo cinfo = found.GetConstructor(new Type[0]);
				return cinfo.Invoke(new object[0]);
			}

			return null;
		}

		/// <summary>
		/// Find and create and instance of class by parent type and 
		/// name.
		/// </summary>
		/// <param name="type">Parent type</param>
		/// <param name="name">Name of class</param>
		/// <returns>Returns new instance of found class, or null.</returns>
		public static object FindAndCreateByTypeAndName(Type type, string name)
		{
			UpdateAssemblyCache();

			foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (a.IsDynamic)
					continue;

				Type found = a.GetType(name);
				if (found == null)
					continue;

				if (!found.IsClass)
					continue;

				if (!found.IsSubclassOf(type))
					continue;

				ConstructorInfo cinfo = found.GetConstructor(new Type[0]);
				return cinfo.Invoke(new object[0]);
			}

			return null;
		}
	}

    /// <summary>
    /// Additional methods for array functionality.
    /// </summary>
    public static class ArrayExtensions
    {
        /// <summary>
        /// Slice an array into a sub-array.
        /// </summary>
        /// <param name="source">The source array</param>
        /// <param name="start">The starting index for slicing</param>
        /// <param name="end">The ending index for slicing</param>
        /// <returns>Returns newly sliced array.</returns>
        public static T[] Slice<T>(this T[] source, int start, int end)
        {
            // catch invalid ends
            if (end < 0)
            {
                end += source.Length;
            }
            else if (end > source.Length)
            {
                end = source.Length;
            }
            else if (start < 0)
            {
                start = 0;
            }
            else if (start > source.Length)
            {
                return new T[0];
            }
            int len = end - start;

            // create new array
            T[] ret = new T[len];
            for (int i = 0; i < len; i++)
            {
                ret[i] = source[i + start];
            }
            return ret;
        }

        /// <summary>
        /// Combine multiple arrays into one array.
        /// </summary>
        /// <param name="arrays">The list of arrays to be combined</param>
        /// <returns>Returns newly combined array.</returns>
        public static T[] Combine<T>(params T[][] arrays)
        {
            T[] ret = new T[arrays.Sum(a => a.Length)];
            int offset = 0;

            foreach (T[] array in arrays)
            {
                Buffer.BlockCopy(array, 0, ret, offset, array.Length);
                offset += array.Length;
            }

            return ret;
        }

        /// <summary>
        /// Combine multiple arrays into one array.
        /// </summary>
        /// <param name="arrays">The list of arrays to be combined</param>
        /// <returns>Returns newly combined array.</returns>
        public static int[] Range(int start, int stop, int step)
        {
            if (step == 0)
                return null;

            List<int> ret = new List<int>();
            int value = start + step * ret.Count;

            if (step > 0)
            {
                while (value < stop)
                {
                    ret.Add(value);
                    value = start + step * ret.Count;
                }
            }
            else
            {
                while (value > stop)
                {
                    ret.Add(value);
                    value = start + step * ret.Count;
                }
            }

            return ret.ToArray();
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
	}
}

// end
