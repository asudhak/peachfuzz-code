
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
//   Michael Eddington (mike@phed.org)

// $Id$

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Peach.Core.Dom;

using System;
using System.IO;
using System.Text;
using System.Collections.Specialized;
using System.Resources;
using System.Globalization;
using System.ComponentModel;
using System.Reflection;
using System.Net.Sockets;
using System.Security.Cryptography;

using Peach.Core.IO.Conversion;

namespace Peach.Core.IO
{
	/// <summary>
	/// This stream is how all data is stored and read by
	/// Peach data elements.  It supports padded and unpadded
	/// reading/writing and accessing data stream as bits and
	/// bytes.
	/// </summary>
	[Serializable]
	public class BitStream
	{
		/// <summary>
		/// Data element positions in stream.  Key
		/// is element name (full to data model) and
		/// value = [0] start bit position, [1] length in bits.
		/// </summary>
		protected Dictionary<string, int[]> _elementPositions = new Dictionary<string, int[]>();
		
		public List<byte> buff;
		protected int pos = 0;
		protected int len = 0;
		protected bool _isLittleEndian = true;
		protected bool isNormalRead = true;
		protected bool _padding = true;
		protected bool _readLeftToRight = false;
		protected EndianBitConverter bitConverter = null;

		/// <summary>
		/// Default constructor
		/// </summary>
		public BitStream()
		{
			buff = new List<byte>();
			LittleEndian();
		}

		/// <summary>
		/// Constructor for BitStream class
		/// </summary>
		/// <param name="buff">Use buff as initial stream data.</param>
		public BitStream(byte[] buff)
		{
			this.buff = new List<byte>(buff);
			len = buff.Length * 8;
			LittleEndian();
		}

		/// <summary>
		/// Clear contents of stream.  After calling
		/// position will be 0 and length is also 0.
		/// </summary>
		public void Clear()
		{
			_elementPositions = new Dictionary<string, int[]>();
			buff = new List<byte>();
			pos = 0;
			len = 0;
			LittleEndian();
		}

		protected BitStream(byte [] buff, int pos, int len,
			bool isLittleEndian, bool isNormalRead,
			bool padding, bool readLeftToRight,
			Dictionary<string, int[]> _elementPositions)
		{
			this.buff = new List<byte>(buff);
			this.pos = pos;
			this.len = len;
			this._isLittleEndian = isLittleEndian;
			this.isNormalRead = isNormalRead;
			this._padding = padding;
			this._readLeftToRight = readLeftToRight;
			this._elementPositions = _elementPositions;
		}

		/// <summary>
		/// Create exact copy of this BitStream
		/// </summary>
		/// <returns>Returns exact copy of this BitStream</returns>
		public BitStream Clone()
		{
			return new BitStream(buff.ToArray(), pos, len, _isLittleEndian, 
				isNormalRead, _padding, _readLeftToRight, _elementPositions);
		}

		/// <summary>
		/// Length in bits of buffer
		/// </summary>
		public int LengthBits
		{
			get { return len; }
		}

		/// <summary>
		/// Length in bytes of buffer.  size is
		/// badded out to 8 bit boundry.
		/// </summary>
		public int LengthBytes
		{
			get
			{
				return (len/8) + (len % 8 == 0 ? 0 : 1);
			}
		}

		/// <summary>
		/// Current position in bits
		/// </summary>
		/// <returns>Returns current bit position</returns>
		public int TellBits()
		{
			return pos;
		}

		/// <summary>
		/// Current position in bytes
		/// </summary>
		/// <returns>Returns current byte position</returns>
		public int TellBytes()
		{
			return pos / 8;
		}

		/// <summary>
		/// Seek to a position in our stream.  We 
		/// will will expand the stream as needed.
		/// </summary>
		/// <param name="offset">Offset from origion to seek to</param>
		/// <param name="origin">Origin to seek from</param>
		public void SeekBits(int offset, SeekOrigin origin)
		{
			switch (origin)
			{
				case SeekOrigin.Begin:
					pos = (int)offset;
					break;
				case SeekOrigin.Current:
					pos = (int)(((long)pos) + offset);
					break;
				case SeekOrigin.End:
					pos = (int)(((long)len) - offset);
					break;
			}
		}

		/// <summary>
		/// Seek to a position in our stream.  We 
		/// will will expand the stream as needed.
		/// </summary>
		/// <param name="offset">Offset from origion to seek to</param>
		/// <param name="origin">Origin to seek from</param>
		public void SeekBytes(int offset, SeekOrigin origin)
		{
			SeekBits(offset * 8, origin);
		}

		public void SeekToDataElement(DataElement elem)
		{
			SeekToDataElement(elem.fullName);
		}

		protected bool HasDataElement(string name)
		{
			foreach (string key in _elementPositions.Keys)
				if (key == name)
					return true;

			return false;
		}

		public void SeekToDataElement(string name)
		{
			if (name == null)
				throw new ApplicationException("name is null");

			if (!HasDataElement(name))
				throw new ApplicationException(
					string.Format("DataElement {0} does not exist in collection", name));

			pos = _elementPositions[name][0];
		}

		#region BitControl

		/// <summary>
		/// Is byte padding enabled.
		/// </summary>
		public bool Padding
		{
			get { return _padding; }
			set { _padding = value; }
		}

		/// <summary>
		/// Pack/unpack as big endian values.
		/// </summary>
		public void BigEndian()
		{
			_isLittleEndian = false;
			bitConverter = new BigEndianBitConverter();
		}

		/// <summary>
		/// Pack/unpack as little endian values.
		/// </summary>
		public void LittleEndian()
		{
			_isLittleEndian = true;
			bitConverter = new LittleEndianBitConverter();
		}

		///// <summary>
		///// Pack/Unack bits from left to right.  Normally
		///// big endian is left to right and little endian
		///// is right to left.
		///// 
		///// Changing endianness via LittleEndian() or BigEndian()
		///// will reset this to default method.
		///// </summary>
		//public void ReadLeftToRight()
		//{
		//    _readLeftToRight = true;
		//}

		///// <summary>
		///// Pack/Unack bits from right to left.  Normally
		///// big endian is left to right and little endian
		///// is right to left.
		///// 
		///// Changing endianness via LittleEndian() or BigEndian()
		///// will reset this to default method.
		///// </summary>
		//public void ReadRightToLeft()
		//{
		//    _readLeftToRight = false;
		//}

		#endregion

		#region DataElements

		/// <summary>
		/// Length of DataElement by bits
		/// </summary>
		/// <param name="e">DataElement that has already been written to stream</param>
		/// <returns>Returns size in bits of DataElement</returns>
		public int DataElementLength(DataElement e)
		{
			if (e == null)
				throw new ApplicationException("DataElement 'e' is null");

			return DataElementLength(e.fullName);
		}

		/// <summary>
		/// Length of DataElement by bits
		/// </summary>
		/// <param name="fullName">Fullname of DataElement that has already been written to stream</param>
		/// <returns>Returns size in bits of DataElement</returns>
		public int DataElementLength(string fullName)
		{
			if (fullName == null)
				throw new ApplicationException("fullName is null");

			if (!HasDataElement(fullName))
				throw new ApplicationException(string.Format("Unknown DataElement {0}", fullName));

			return _elementPositions[fullName][1];
		}

		/// <summary>
		/// position in stream of DataElement
		/// </summary>
		/// <param name="e">DataElement that has already been written to the stream</param>
		/// <returns>Returns bit position of DataElement</returns>
		public int DataElementPosition(DataElement e)
		{
			if (e == null)
				throw new ApplicationException("DataElement 'e' is null");

			return DataElementPosition(e.fullName);
		}

		/// <summary>
		/// position in stream of DataElement
		/// </summary>
		/// <param name="fullName">DataElement that has already been written to the stream</param>
		/// <returns>Returns bit position of DataElement</returns>
		public int DataElementPosition(string fullName)
		{
			if (fullName == null)
				throw new ApplicationException("fullName is null");

			if (!HasDataElement(fullName))
				throw new ApplicationException(string.Format("Unknown DataElement {0}", fullName));

			return _elementPositions[fullName][0];
		}

		/// <summary>
		/// Mark the starting position of a DataElement in the stream.
		/// </summary>
		/// <param name="e">DataElement to mark the position of</param>
		/// <param name="lengthInBits">Length of DataElement in stream</param>
		public void MarkStartOfElement(DataElement e, int lengthInBits)
		{
			if (e == null)
				throw new ApplicationException("DataElement 'e' is null");

			if (HasDataElement(e.fullName))
				_elementPositions[e.fullName][0] = pos;
			else
				_elementPositions.Add(e.fullName, new int[] { pos, lengthInBits });
		}

		/// <summary>
		/// Mark the starting position of a DataElement in the stream.
		/// </summary>
		/// <param name="e">DataElement to mark the position of</param>
		public void MarkStartOfElement(DataElement e)
		{
			if (e == null)
				throw new ApplicationException("DataElement 'e' is null");

			if (HasDataElement(e.fullName))
				_elementPositions[e.fullName][0] = pos;
			else
				_elementPositions.Add(e.fullName, new int[] { pos, 0 });
		}

		/// <summary>
		/// Mark the ending position of DataElement.  If you have
		/// already specified a length with MarkStartOfElement you
		/// do not need to call this method.
		/// </summary>
		/// <param name="e">DataElement to mark the position of</param>
		public void MarkEndOfElement(DataElement e)
		{
			if (e == null)
				throw new ApplicationException("DataElement 'e' is null");

			if(!HasDataElement(e.fullName))
				throw new ApplicationException(
					string.Format("Element position list does not contain DataElement {0}.", e.fullName));

			_elementPositions[e.fullName][1] = pos;
		}

		#endregion

		#region Writing Methods

		public void WriteSByte(sbyte value)
		{
			WriteByte((byte)value);
		}
		public void WriteInt8(sbyte value)
		{
			WriteByte((byte)value);
		}
		public void WriteByte(byte value)
		{
			WriteBits(value, 8);
		}
		public void WriteUInt8(byte value)
		{
			WriteByte(value);
		}
		public void WriteShort(short value)
		{
			WriteBytes(bitConverter.GetBytes(value));
		}
		public void WriteInt16(short value)
		{
			WriteBytes(bitConverter.GetBytes(value));
		}
		public void WriteUShort(ushort value)
		{
			WriteBytes(bitConverter.GetBytes(value));
		}
		public void WriteUInt16(ushort value)
		{
			WriteBytes(bitConverter.GetBytes(value));
		}
		public void WriteWORD(ushort value)
		{
			WriteBytes(bitConverter.GetBytes(value));
		}
		public void WriteInt(int value)
		{
			WriteBytes(bitConverter.GetBytes(value));
		}
		public void WriteInt32(int value)
		{
			WriteBytes(bitConverter.GetBytes(value));
		}
		public void WriteUInt(uint value)
		{
			WriteBytes(bitConverter.GetBytes(value));
		}
		public void WriteUInt32(uint value)
		{
			WriteBytes(bitConverter.GetBytes(value));
		}
		public void WriteDWORD(uint value)
		{
			WriteBytes(bitConverter.GetBytes(value));
		}
		public void WriteLong(long value)
		{
			WriteBytes(bitConverter.GetBytes(value));
		}
		public void WriteInt64(long value)
		{
			WriteBytes(bitConverter.GetBytes(value));
		}
		public void WriteULong(ulong value)
		{
			WriteBytes(bitConverter.GetBytes(value));
		}
		public void WriteUInt64(ulong value)
		{
			WriteBytes(bitConverter.GetBytes(value));
		}

		#endregion

		#region Writing Methods with DataElement

		public void WriteSByte(sbyte value, DataElement element)
		{
			MarkStartOfElement(element, 8);
			WriteBits((byte)value, 8);
		}
		public void WriteInt8(sbyte value, DataElement element)
		{
			MarkStartOfElement(element, 8);
			WriteBits((byte)value, 8);
		}
		public void WriteByte(byte value, DataElement element)
		{
			MarkStartOfElement(element, 8);
			WriteBits(value, 8);
		}
		public void WriteUInt8(byte value, DataElement element)
		{
			MarkStartOfElement(element, 8);
			WriteBits(value, 8);
		}
		public void WriteShort(short value, DataElement element)
		{
			MarkStartOfElement(element, 16);
			WriteBytes(bitConverter.GetBytes(value));
		}
		public void WriteInt16(short value, DataElement element)
		{
			MarkStartOfElement(element, 16);
			WriteBytes(bitConverter.GetBytes(value));
		}
		public void WriteUShort(ushort value, DataElement element)
		{
			MarkStartOfElement(element, 16);
			WriteBytes(bitConverter.GetBytes(value));
		}
		public void WriteUInt16(ushort value, DataElement element)
		{
			MarkStartOfElement(element, 16);
			WriteBytes(bitConverter.GetBytes(value));
		}
		public void WriteWORD(ushort value, DataElement element)
		{
			MarkStartOfElement(element, 16);
			WriteBytes(bitConverter.GetBytes(value));
		}
		public void WriteInt(int value, DataElement element)
		{
			MarkStartOfElement(element, 32);
			WriteBytes(bitConverter.GetBytes(value));
		}
		public void WriteInt32(int value, DataElement element)
		{
			MarkStartOfElement(element, 32);
			WriteBytes(bitConverter.GetBytes(value));
		}
		public void WriteUInt(uint value, DataElement element)
		{
			MarkStartOfElement(element, 32);
			WriteBytes(bitConverter.GetBytes(value));
		}
		public void WriteUInt32(uint value, DataElement element)
		{
			MarkStartOfElement(element, 32);
			WriteBytes(bitConverter.GetBytes(value));
		}
		public void WriteDWORD(uint value, DataElement element)
		{
			MarkStartOfElement(element, 32);
			WriteBytes(bitConverter.GetBytes(value));
		}
		public void WriteLong(long value, DataElement element)
		{
			MarkStartOfElement(element, 64);
			WriteBytes(bitConverter.GetBytes(value));
		}
		public void WriteInt64(long value, DataElement element)
		{
			MarkStartOfElement(element, 64);
			WriteBytes(bitConverter.GetBytes(value));
		}
		public void WriteULong(ulong value, DataElement element)
		{
			MarkStartOfElement(element, 64);
			WriteBytes(bitConverter.GetBytes(value));
		}
		public void WriteUInt64(ulong value, DataElement element)
		{
			MarkStartOfElement(element, 64);
			WriteBytes(bitConverter.GetBytes(value));
		}

		#endregion

		public void Write(BitStream bits, DataElement element)
		{
			MarkStartOfElement(element, bits.LengthBits);
			Write(bits);
		}

		/// <summary>
		/// Write the contents of another BitStream into
		/// this BitStream.
		/// </summary>
		/// <param name="bits">BitStream to write data from.</param>
		public void Write(BitStream bits)
		{
			if(bits == null)
				throw new ApplicationException("bits parameter is null");
			
			if(bits.LengthBits == 0)
				return;

			int bytesToWrite = bits.LengthBits / 8;
			int extraBits = bits.LengthBits - (bytesToWrite * 8);
			int origionalPos = pos;

			bits.SeekBits(0, SeekOrigin.Begin);
			WriteBytes(bits.ReadBytes(bytesToWrite));
			if(extraBits > 0)
				WriteBits(bits.ReadBits(extraBits), extraBits);

			// Copy over DataElement positions, replace
			// existing entries if they exist.
			foreach (string key in bits._elementPositions.Keys)
			{
				if (!HasDataElement(key))
					_elementPositions.Add(key, bits._elementPositions[key]);
				else
					_elementPositions[key] = bits._elementPositions[key];

				_elementPositions[key][0] += origionalPos;
			}
		}

		public void WriteBits(ulong value, int bits, DataElement element)
		{
			MarkStartOfElement(element, bits);
			WriteBits(value, bits);
		}

		protected byte[] ClearingMasks = new byte[] { 0x7f, 0xBF, 0xDF, 0xef, 0xf7, 0xfb, 0xfd, 0xfe };

		public void WriteBit(byte bit)
		{
			if (bit > 1)
				throw new ApplicationException("WriteBit only takes values of 0 or 1.");

			// Index into buff[] array
			int byteIndex = 0;
			// Index into byte from buff[] array
			int bitIndex = 0;

			// Get byte position in buff
			byteIndex = pos / 8;

			// Calc position in byte to set
			bitIndex = pos - (byteIndex * 8);

			// Do we need to grow buff?
			if (byteIndex >= buff.Count)
				buff.Add(0);

			//if (_isLittleEndian)
			//{
			//    if (bit == 0)
			//        // clear bit
			//        buff[byteIndex] = (byte)(buff[byteIndex] & ClearingMasks[7 - bitIndex]);
			//    else
			//        // Set bit
			//        buff[byteIndex] = (byte)(buff[byteIndex] | (bit << bitIndex));
			//}
			//else
			//{
				if (bit == 0)
					// clear bit
					buff[byteIndex] = (byte)(buff[byteIndex] & ClearingMasks[bitIndex]);
				else
					// Set bit
					buff[byteIndex] = (byte)(buff[byteIndex] | (bit << (7 - bitIndex)));
			//}

			// Increment our current position
			pos++;
			if (pos >= len)
				len++;
		}

		/// <summary>
		/// Write bits using bitfield encoding.
		/// </summary>
		/// <param name="value">Value to write</param>
		/// <param name="bits">Number of bits to write</param>
		public void WriteBits(ulong value, int bits)
		{
			if(bits == 0 || bits > 64)
				throw new ApplicationException("bits is invalid value, but be > 0 and < 64");

			for (int cnt = 0; cnt < (int)bits; cnt++ )
			{
				WriteBit( (byte) ((value >> ((bits-1) - cnt)) & 1) );
			}
		}

		/// <summary>
		/// Number of bits required to store value.
		/// </summary>
		/// <param name="value">Value to calc bit length of</param>
		/// <returns>Number of bits required to store number.</returns>
		protected ulong BitLength(ulong value)
		{
			return BitLength(value, 64);
		}

		/// <summary>
		/// Number of bits required to store value
		/// </summary>
		/// <param name="value">Value to calc bit length of</param>
		/// <param name="maxBits">Max length in bits</param>
		/// <returns>Returns number of bits required to store number.</returns>
		protected ulong BitLength(ulong value, ulong maxBits)
		{
			ulong blen = 0;
			for (ulong i = 0; i < maxBits; i++)
				if (((value >> 1) & 1) == 1)
					blen = i;

			return blen+1;
		}

		public void WriteBytes(byte[] value)
		{
			foreach (byte b in value)
				WriteBits(b, 8);
		}

		public void WriteBytes(byte[] value, int offset, int length)
		{
			for (int i = offset; i < length && i < value.Length; i++)
				WriteBits(value[i], 8);
		}

		#region Reading methods

		public sbyte ReadSByte()
		{
			return (sbyte)ReadByte();
		}
		public sbyte ReadInt8()
		{
			return (sbyte)ReadByte();
		}
		public byte ReadByte()
		{
			return (byte)ReadBits(8);
		}
		public byte ReadUInt8()
		{
			return ReadByte();
		}
		public short ReadShort()
		{
			return bitConverter.ToInt16(ReadBytes(2));
		}
		public short ReadInt16()
		{
			return bitConverter.ToInt16(ReadBytes(2));
		}
		public ushort ReadUShort()
		{
			return bitConverter.ToUInt16(ReadBytes(2));
		}
		public ushort ReadUInt16()
		{
			return bitConverter.ToUInt16(ReadBytes(2));
		}
		public ushort ReadWORD()
		{
			return bitConverter.ToUInt16(ReadBytes(2));
		}
		public int ReadInt()
		{
			return  bitConverter.ToInt32(ReadBytes(4));
		}
		public int ReadInt32()
		{
			return bitConverter.ToInt32(ReadBytes(4));
		}
		public uint ReadUInt()
		{
			return bitConverter.ToUInt32(ReadBytes(4));
		}
		public uint ReadUInt32()
		{
			return bitConverter.ToUInt32(ReadBytes(4));
		}
		public uint ReadDWORD()
		{
			return bitConverter.ToUInt32(ReadBytes(4));
		}
		public long ReadLong()
		{
			return bitConverter.ToInt64(ReadBytes(8));
		}
		public long ReadInt64()
		{
			return bitConverter.ToInt64(ReadBytes(8));
		}
		public ulong ReadULong()
		{
			return bitConverter.ToUInt64(ReadBytes(8));
		}
		public ulong ReadUInt64()
		{
			return bitConverter.ToUInt64(ReadBytes(8));
		}

		#endregion

		/// <summary>
		/// Read a single bit from our bitstream
		/// </summary>
		/// <returns>Return a byte containing 0/1</returns>
		public byte ReadBit()
		{
			try
			{
				// Index into buff[] array
				int byteIndex = pos / 8;
				// Index into byte from buff[] array
				int bitIndex = pos - (byteIndex * 8);

				//if (_isLittleEndian)
				//    // Shift and mask off 1 byte
				//    return (byte)((buff[byteIndex] >> (byte)bitIndex) & 1);
				//else
			    
                // Shift and mask off 1 byte
                return (byte)((buff[byteIndex] >> (byte)(7 - bitIndex)) & 1);
			}
			finally
			{
				pos++;
			}
		}

		protected byte ReadBit(byte b, int pos)
		{
			// Index into byte from buff[] array
			int bitIndex = pos;

			if (_isLittleEndian)
				// Shift and mask off 1 byte
				return (byte)((b >> (byte)bitIndex) & 1);
			else
				// Shift and mask off 1 byte
				return (byte)((b >> (byte)(7 - bitIndex)) & 1);
		}

		public ulong ReadBits(int bits)
		{
			if (bits < 0 || bits > 64)
				throw new ArgumentOutOfRangeException("Must be between 0 and 64");

			ulong ret = 0;

			for (int cnt = 0; cnt < bits; cnt++)
			{
				ret |= (byte)(ReadBit() << (int)((bits - 1) - cnt));
			}

			return ret;
		}


		protected static string Byte2String(byte b)
		{
			string ret = "";

			for (int i = 0; i < 8; i++)
			{
				int bit = (b >> 7 - i) & 1;
				ret += bit == 0 ? "0" : "1";
			}

			return ret;
		}
		public byte[] ReadBytes(int count)
		{
			if (count == 0)
				throw new ApplicationException("Asking for zero bytes");
			if (((pos/8) + count) > buff.Count)
				throw new ApplicationException("Count overruns buffer");

			byte[] ret = new byte[count];

			//Console.WriteLine("ReadBytes()");
			for (int i = 0; i < count; i++)
			{
				ret[i] = ReadByte();
				//Console.Write(Byte2String(ret[i]));
			}
			//Console.WriteLine("");

			return ret;
		}

		public byte[] FixEndian(byte[] b)
		{
			byte tmp;
			if (_isLittleEndian)
			{
				// Reverse array
				for (int cnt = 0; cnt < (b.Length/2); cnt++)
				{
					tmp = b[cnt];
					b[cnt] = b[(b.Length-1) - cnt];
					b[(b.Length-1) - cnt] = tmp;
				}
			}

			return b;
		}

		public ulong ByteArrayToLong(byte[] b)
		{
			FixEndian(b);

			ulong ret = 0;
			for (int cnt = 0; cnt < b.Length; cnt++)
			{
				//ret = ret << 7;
				ret |= ((ulong)b[cnt]) << (8*cnt);
			}

			return ret;
		}

		/// <summary>
		/// Truncate stream from current position.
		/// </summary>
		public void Truncate()
		{
			Truncate(pos);
		}

		/// <summary>
		/// Truncate stream to specific length in bits.
		/// </summary>
		/// <param name="sizeInBits">Length in bits of stream</param>
		public void Truncate(int sizeInBits)
		{
			if (sizeInBits > len)
				throw new ApplicationException("sizeInbits larger then length of data");

			if (pos > sizeInBits)
				pos = sizeInBits;

			len = sizeInBits;
			int startBlock = sizeInBits / 8 + (sizeInBits % 8 == 0 ? 0 : 1);
			buff.RemoveRange((int)startBlock, buff.Count - (int)startBlock);

			// Remove element entries that were truncated off.

			List<string> keysToRemove = new List<string>();
			foreach (string key in _elementPositions.Keys)
			{
				if (_elementPositions[key][0] > len)
					keysToRemove.Add(key);
				else if (_elementPositions[key][0] + _elementPositions[key][1] > len)
					_elementPositions[key][1] = len - _elementPositions[key][0];
			}

			foreach (string key in keysToRemove)
				_elementPositions.Remove(key);
		}

		/// <summary>
		/// Insert a BitStream at current position.  This
		/// will cause length of stream to increase by the
		/// size of "bits".  New position will be after
		/// inserted "bits".
		/// </summary>
		/// <param name="bits">BitStream to insert.</param>
		public void Insert(BitStream bits)
		{
			int currentBlock = pos / 8;
			int curpos = pos;
			int curlen = len;
			int retpos = pos;
			int[] vals = null;

			// If both streams are on an 8 bit boundry
			// this is the quick 'n easy method.
			if (pos % 8 == 0 && bits.LengthBits % 8 == 0)
			{
				buff.InsertRange((int)currentBlock, bits.Value);
				len += bits.LengthBits;
				pos += bits.LengthBits;

				// Move existing DataElement positions

				foreach (string key in _elementPositions.Keys)
				{
					vals = _elementPositions[key];

					if (vals[0] >= curpos)
						vals[0] += bits.LengthBits;
				}

				// Copy over the new DataElement positions

				foreach (string key in bits._elementPositions.Keys)
				{
					if(HasDataElement(key))
						throw new ApplicationException(
							string.Format("Dictionary already contains a key called {0}", key));

					vals = bits._elementPositions[key];
					vals[0] += curpos;
					_elementPositions.Add(key, vals);
				}

				return;
			}

			BitStream tmp = Clone();
			Truncate();

			bits.SeekBits(0, SeekOrigin.Begin);
			WriteBytes(bits.ReadBytes(bits.LengthBits / 8));
			if(bits.LengthBits % 8 != 0)
				WriteBits(bits.ReadBits(bits.LengthBits % 8), bits.LengthBits % 8);

			retpos = pos;

			tmp.SeekBits(curpos, SeekOrigin.Begin);
			WriteBytes(tmp.ReadBytes((curlen - curpos) / 8));
			if ((curlen - curpos) % 8 != 0)
				WriteBits(tmp.ReadBits((curlen - curpos) % 8), (curlen - curpos) % 8);

			// Copy over the DataElement positions
			foreach (string key in tmp._elementPositions.Keys)
			{
				vals = tmp._elementPositions[key];
				if (vals[0] >= curpos)
				{
					vals[0] += retpos - curpos;
					if (!tmp.HasDataElement(key))
						tmp._elementPositions.Add(key, vals);
					else
						throw new ApplicationException(
							string.Format("DataElement {0} already exists!", key));
				}
			}

			pos = retpos;
		}

		/// <summary>
		/// Byte array of stream.
		/// </summary>
		public byte[] Value
		{
			get { return buff.ToArray(); }
		}
	}



}

// end
