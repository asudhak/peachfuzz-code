
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Peach.Core.IO;

namespace Peach.Core.IO.Conversion
{
	/// <summary>
	/// Implementation of EndianBitConverter which converts to/from big-endian
	/// byte arrays.
	/// </summary>
	[Serializable]
	public class BigEndianBitConverter : EndianBitConverter
	{
		/// <summary>
		/// Indicates the byte order ("endianess") in which data is converted using this class.
		/// </summary>
		/// <remarks>
		/// Different computer architectures store data using different byte orders. "Big-endian"
		/// means the most significant byte is on the left end of a word. "Little-endian" means the 
		/// most significant byte is on the right end of a word.
		/// </remarks>
		/// <returns>true if this converter is little-endian, false otherwise.</returns>
		public sealed override bool IsLittleEndian()
		{
			return false;
		}

		/// <summary>
		/// Indicates the byte order ("endianess") in which data is converted using this class.
		/// </summary>
		public sealed override Endianness Endianness
		{
			get { return Endianness.BigEndian; }
		}

		/// <summary>
		/// Copies the specified number of bytes from value to buffer, starting at index.
		/// </summary>
		/// <param name="value">The value to copy</param>
		/// <param name="bytes">The number of bytes to copy</param>
		/// <param name="buffer">The buffer to copy the bytes into</param>
		/// <param name="index">The index to start at</param>
		protected override void CopyBytesImpl(long value, int bytes, byte[] buffer, int index)
		{
			int endOffset = index + bytes - 1;
			for (int i = 0; i < bytes; i++)
			{
				buffer[endOffset - i] = unchecked((byte)(value & 0xff));
				value = value >> 8;
			}
		}

		/// <summary>
		/// Returns a value built from the specified number of bytes from the given buffer,
		/// starting at index.
		/// </summary>
		/// <param name="buffer">The data in byte array format</param>
		/// <param name="startIndex">The first index to use</param>
		/// <param name="bytesToConvert">The number of bytes to use</param>
		/// <returns>The value built from the given bytes</returns>
		protected override long FromBytes(byte[] buffer, int startIndex, int bytesToConvert)
		{
			long ret = 0;
			for (int i = 0; i < bytesToConvert; i++)
			{
				ret = unchecked((ret << 8) | buffer[startIndex + i]);
			}
			return ret;
		}
	}
}
