
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

	public class HexString
	{
		public byte[] Value { get; private set; }

		private HexString(byte[] value)
		{
			this.Value = value;
		}

		public static HexString Parse(string s)
		{
			if (s.Length % 2 == 0)
			{
				var array = Utilities.HexStringToArray(s);
				if (array != null)
					return new HexString(array);
			}

			throw new FormatException("An invalid hex string was specified.");
		}
	}

	/// <summary>
	/// Some utility methods that be usefull
	/// </summary>
	public class Utilities
	{
		private static Encoding _extendedAscii = Encoding.GetEncoding(1252);
		public static Encoding ExtendedASCII { get { return _extendedAscii; } }

		/// <summary>
		/// Returns the minimum number of bytes needed to decode
		/// a character of the specified encodinf
		/// </summary>
		/// <param name="enc">String encoding</param>
		/// <returns>Minimum bytes</returns>
		public static int EncodingMinBytes(Encoding enc)
		{
			if (enc is UnicodeEncoding)
				return 2;
			else if (enc is UTF32Encoding)
				return 4;
			else
				return 1;
		}

		/// <summary>
		/// Converts a string to a byte array of the specified encoding.
		/// Works around inconsistencies between Microsoft .NET and Mono so
		/// errors are always provided
		/// </summary>
		/// <param name="str">String to convert</param>
		/// <param name="enc">String encoding</param>
		/// <returns>Byte array</returns>
		public static byte[] StringToBytes(string str, Encoding enc)
		{
			enc = Encoding.GetEncoding(enc.BodyName, new EncoderExceptionFallback(), new DecoderExceptionFallback());
			return enc.GetBytes(str.ToCharArray());
		}

		/// <summary>
		/// Checks if a string is valid for the specified encoding.
		/// Works around inconsistencies between Microsoft .NET and Mono so
		/// errors are always provided
		/// </summary>
		/// <param name="str">String to check</param>
		/// <param name="enc">String encoding</param>
		public static bool StringIsValid(string str, Encoding enc)
		{
			if (!enc.IsSingleByte)
				return true;

			try
			{
				StringToBytes(str, enc);
				return true;
			}
			catch (EncoderFallbackException)
			{
				return false;
			}
		}

		/// <summary>
		/// Converts a byte array to a string of the specified encoding.
		/// Works around inconsistencies between Microsoft .NET and Mono so
		/// errors are always provided
		/// </summary>
		/// <param name="str">Byte array to convert</param>
		/// <param name="enc">String encoding</param>
		/// <returns>String value</returns>
		public static string BytesToString(byte[] buf, Encoding enc)
		{
			int min = EncodingMinBytes(enc);

			if ((buf.Length % min) != 0)
				throw new DecoderFallbackException();

			enc = Encoding.GetEncoding(enc.BodyName, new EncoderExceptionFallback(), new DecoderExceptionFallback());
			var chars = enc.GetChars(buf);

			if (enc is UTF7Encoding)
			{
				if (!buf.IsSame(enc.GetBytes(chars)))
					throw new DecoderFallbackException();
			}

			return new string(chars);
		}

		public static byte[] HexStringToArray(string s)
		{
			if (s.Length % 2 != 0)
				throw new ArgumentException("s");

			byte[] ret = new byte[s.Length / 2];

			for (int i = 0; i < s.Length; i += 2)
			{
				int nibble1 = GetNibble(s[i]);
				int nibble2 = GetNibble(s[i + 1]);

				if (nibble1 < 0 || nibble1 > 0xF || nibble2 < 0 | nibble2 > 0xF)
					return null;

				ret[i / 2] = (byte)((nibble1 << 4) | nibble2);
			}

			return ret;
		}

		private static int GetNibble(char c)
		{
			if (c >= 'a')
				return 0xA + (int)(c - 'a');
			else if (c >= 'A')
				return 0xA + (int)(c - 'A');
			else
				return (int)(c - '0');
		}

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
				byte[] buf = System.Text.Encoding.ASCII.GetBytes(line);
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
