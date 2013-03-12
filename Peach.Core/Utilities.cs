
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

    [Serializable]
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
				var array = ToArray(s);
				if (array != null)
					return new HexString(array);
			}

			throw new FormatException("An invalid hex string was specified.");
		}

		public static byte[] ToArray(string s)
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

		/// <summary>
		/// Compute the subrange resulting from diving a range into equal parts
		/// </summary>
		/// <param name="begin">Inclusive range begin</param>
		/// <param name="end">Inclusive range end</param>
		/// <param name="curSlice">The 1 based index of the current slice</param>
		/// <param name="numSlices">The total number of slices</param>
		/// <returns>Range of the current slice</returns>
		public static Tuple<uint, uint> SliceRange(uint begin, uint end, uint curSlice, uint numSlices)
		{
			if (begin > end)
				throw new ArgumentOutOfRangeException("begin");
			if (curSlice == 0 || curSlice > numSlices)
				throw new ArgumentOutOfRangeException("curSlice");

			uint total = end - begin + 1;

			if (numSlices == 0 || numSlices > total)
				throw new ArgumentOutOfRangeException("numSlices");

			uint slice = total / numSlices;

			end = curSlice * slice + begin - 1;
			begin = end - slice + 1;

			if (curSlice == numSlices)
				end += total % numSlices;

			return new Tuple<uint, uint>(begin, end);
		}

		// Slightly tweaked from:
		// http://www.codeproject.com/Articles/36747/Quick-and-Dirty-HexDump-of-a-Byte-Array
		private delegate void HexOutputFunc(char[] line);
		private delegate int HexInputFunc(byte[] buf, int max);

		private static void HexDump(HexInputFunc input, HexOutputFunc output, int bytesPerLine = 16)
		{
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

			for (int i = 0; ; i += bytesPerLine)
			{
				int readLen = input(bytes, bytesPerLine);
				if (readLen == 0)
					break;

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

		}

		public static void HexDump(Stream input, Stream output, int bytesPerLine = 16)
		{
			long pos = input.Position;

			HexInputFunc inputFunc = delegate(byte[] buf, int max)
			{
				return input.Read(buf, 0, max);
			};

			HexOutputFunc outputFunc = delegate(char[] line)
			{
				byte[] buf = System.Text.Encoding.ASCII.GetBytes(line);
				output.Write(buf, 0, buf.Length);
			};

			HexDump(inputFunc, outputFunc, bytesPerLine);

			input.Seek(pos, SeekOrigin.Begin);
		}

		public static void HexDump(byte[] buffer, int offset, int count, Stream output, int bytesPerLine = 16)
		{
			HexInputFunc inputFunc = delegate(byte[] buf, int max)
			{
				int len = Math.Min(count, max);
				Buffer.BlockCopy(buffer, offset, buf, 0, len);
				offset += len;
				count -= len;
				return len;
			};

			HexOutputFunc outputFunc = delegate(char[] line)
			{
				byte[] buf = System.Text.Encoding.ASCII.GetBytes(line);
				output.Write(buf, 0, buf.Length);
			};

			HexDump(inputFunc, outputFunc, bytesPerLine);
		}

		public static string HexDump(Stream input, int bytesPerLine = 16)
		{
			StringBuilder sb = new StringBuilder();
			long pos = input.Position;

			HexInputFunc inputFunc = delegate(byte[] buf, int max)
			{
				return input.Read(buf, 0, max);
			};

			HexOutputFunc outputFunc = delegate(char[] line)
			{
				sb.Append(line);
			};

			HexDump(inputFunc, outputFunc, bytesPerLine);

			input.Seek(pos, SeekOrigin.Begin);

			return sb.ToString();
		}

		public static string HexDump(byte[] buffer, int offset, int count, int bytesPerLine = 16)
		{
			StringBuilder sb = new StringBuilder();

			HexInputFunc inputFunc = delegate(byte[] buf, int max)
			{
				int len = Math.Min(count, max);
				Buffer.BlockCopy(buffer, offset, buf, 0, len);
				offset += len;
				count -= len;
				return len;
			};

			HexOutputFunc outputFunc = delegate(char[] line)
			{
				sb.Append(line);
			};

			HexDump(inputFunc, outputFunc, bytesPerLine);

			return sb.ToString();
		}

		public static string PrettyBytes(long bytes)
		{
			if (bytes < 0)
				throw new ArgumentOutOfRangeException("bytes");

			if (bytes > (1024 * 1024 * 1024))
				return (bytes / (1024 * 1024 * 1024.0)).ToString("0.###") + " Gbytes";
			if (bytes > (1024 * 1024))
				return (bytes / (1024 * 1024.0)).ToString("0.###") + " Mbytes";
			if (bytes > 1024)
				return (bytes / 1024.0).ToString("0.###") + " Kbytes";
			return bytes.ToString() + " Bytes";
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
