
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
using System.Linq;
using System.Text;
using PeachCore.Dom;

namespace PeachCore
{
	/// <summary>
	/// This stream is how all data is stored and read by
	/// Peach data elements.  It supports padded and unpadded
	/// reading/writing and accessing data stream as bits and
	/// bytes.
	/// </summary>
	public class BitStream
	{
		/// <summary>
		/// Data element positions in stream.  Key
		/// is element name (full to data model) and
		/// value = [0] start bit position, [1] length in bits.
		/// </summary>
		protected Dictionary<string, byte[]> _elementPositions = new Dictionary<string, byte[]>();
		
		protected List<byte> buff;
		protected ulong pos = 0;
		protected ulong len = 0;
		protected bool _isLittleEndian = true;
		protected bool isNormalRead = true;
		protected bool _padding = true;
		protected bool _readLeftToRight = false;

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
			buff = new List<byte>(buff);
			len = buff.Length * 8;
			LittleEndian();
		}

		/// <summary>
		/// Clear contents of stream.  After calling
		/// position will be 0 and length is also 0.
		/// </summary>
		public void Clear()
		{
			_elementPositions = new Dictionary<string, byte[]>();
			buff = new List<byte>();
			pos = 0;
			len = 0;
			LittleEndian();
		}

		protected BitStream(byte [] buff, ulong pos, ulong len,
			bool isLittleEndian, bool isNormalRead,
			bool padding, bool readLeftToRight,
			Dictionary<string, byte[]> _elementPositions)
		{
			this.buff = new List<bute>(buff);
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
			return BitStream(buff.ToArray(), pos, len, _isLittleEndian, 
				isNormalRead, _padding, _readLeftToRight);
		}

		/// <summary>
		/// Length in bits of buffer
		/// </summary>
		public ulong LengthBits
		{
			get { return len; }
		}

		/// <summary>
		/// Length in bytes of buffer.  Size is
		/// badded out to 8 bit boundry.
		/// </summary>
		public ulong LengthBytes
		{
			get
			{
				return (len/8) + (len % 0 == 0 ? 0 : 1);
			}
		}

		/// <summary>
		/// Current position in bits
		/// </summary>
		/// <returns>Returns current bit position</returns>
		public ulong TellBits()
		{
			return pos;
		}

		/// <summary>
		/// Current position in bytes
		/// </summary>
		/// <returns>Returns current byte position</returns>
		public ulong TellBytes()
		{
			return pos / 8;
		}

		/// <summary>
		/// Seek to a position in our stream.  We 
		/// will will expand the stream as needed.
		/// </summary>
		/// <param name="offset">Offset from origion to seek to</param>
		/// <param name="origin">Origin to seek from</param>
		public void SeekBits(long offset, SeekOrigin origin)
		{
			switch (origin)
			{
				case SeekOrigin.Begin:
					pos = offset;
					break;
				case SeekOrigin.Current:
					pos = pos + offset;
					break;
				case SeekOrigin.End:
					pos = len - offset;
					break;
			}
		}

		/// <summary>
		/// Seek to a position in our stream.  We 
		/// will will expand the stream as needed.
		/// </summary>
		/// <param name="offset">Offset from origion to seek to</param>
		/// <param name="origin">Origin to seek from</param>
		public void SeekBytes(ulong offset, SeekOrigin origin)
		{
			SeekBits(offset * 8, origin);
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
			ReadRightToLeft();
		}

		/// <summary>
		/// Pack/unpack as little endian values.
		/// </summary>
		public void LittleEndian()
		{
			_isLittleEndian = true;
			ReadRightToLeft();
		}

		/// <summary>
		/// Pack/Unack bits from left to right.  Normally
		/// big endian is left to right and little endian
		/// is right to left.
		/// 
		/// Changing endianness via LittleEndian() or BigEndian()
		/// will reset this to default method.
		/// </summary>
		public void ReadLeftToRight()
		{
			_readLeftToRight = true;
		}

		/// <summary>
		/// Pack/Unack bits from right to left.  Normally
		/// big endian is left to right and little endian
		/// is right to left.
		/// 
		/// Changing endianness via LittleEndian() or BigEndian()
		/// will reset this to default method.
		/// </summary>
		public void ReadRightToLeft()
		{
			_readLeftToRight = false;
		}

		#endregion

		#region DataElements

		public uint DataElementLength(DataElement e)
		{
			if (e == null)
				throw new ApplicationException("DataElement 'e' is null");

			if (!_elementPositions.Contains[e])
				throw new ApplicationException("Unknown DataElement");

			return _elementPositions[e.fullName][1];
		}

		public uint DataElementPosition(DataElement e)
		{
			if (e == null)
				throw new ApplicationException("DataElement 'e' is null");

			if (!_elementPositions.Contains[e])
				throw new ApplicationException("Unknown DataElement");

			return _elementPositions[e.fullName][0];
		}

		public void MarkStartOfElement(DataElement e, uint lengthInBits)
		{
			if (e == null)
				throw new ApplicationException("DataElement 'e' is null");

			if (_elementPositions.Contains(e.fullName))
				_elementPositions[e.fullName][0] = pos;
			else
				_elementPositions.Add(e.fullName, new byte[] { pos, lengthInBits });
		}

		public void MarkStartOfElement(DataElement e)
		{
			if (e == null)
				throw new ApplicationException("DataElement 'e' is null");

			if (_elementPositions.Contains(e.fullName))
				_elementPositions[e.fullName][0] = pos;
			else
				_elementPositions.Add(e.fullName, new byte[] { pos, 0 });
		}

		public void MarkEndOfElement(DataElement e)
		{
			if (e == null)
				throw new ApplicationException("DataElement 'e' is null");

			if(!_elementPositions.Contains(e.fullName))
				throw new ApplicationException(
					String.Format("Element position list does not contain DataElement {0}.", e.fullName));

			_elementPositions[e.fullName][1] = pos;
		}

		#endregion

		#region Writing Methods

		public void WriteSByte(sbyte value)
		{
			WriteBits(value, 8);
		}
		public void WriteInt8(sbyte value)
		{
			WriteBits(value, 8);
		}
		public void WriteByte(byte value)
		{
			WriteBits(value, 8);
		}
		public void WriteUInt8(byte value)
		{
			WriteBits(value, 8);
		}
		public void WriteShort(short value)
		{
			WriteBits(value, 16);
		}
		public void WriteInt16(short value)
		{
			WriteBits(value, 16);
		}
		public void WriteUShort(ushort value)
		{
			WriteBits(value, 16);
		}
		public void WriteUInt16(ushort value)
		{
			WriteBits(value, 16);
		}
		public void WriteWORD(ushort value)
		{
			WriteBits(value, 16);
		}
		public void WriteInt(int value)
		{
			WriteBits(value, 32);
		}
		public void WriteInt32(int value)
		{
			WriteBits(value, 32);
		}
		public void WriteUInt(uint value)
		{
			WriteBits(value, 32);
		}
		public void WriteUInt32(uint value)
		{
			WriteBits(value, 32);
		}
		public void WriteDWORD(uint value)
		{
			WriteBits(value, 32);
		}
		public void WriteLong(long value)
		{
			WriteBits(value, 64);
		}
		public void WriteInt64(long value)
		{
			WriteBits(value, 64);
		}
		public void WriteULong(ulong value)
		{
			WriteBits(value, 64);
		}
		public void WriteUInt64(ulong value)
		{
			WriteBits(value, 64);
		}

		#endregion

		#region Writing Methods with DataElement

		public void WriteSByte(sbyte value, DataElement element)
		{
			MarkStartOfElement(element, 8);
			WriteBits(value, 8);
		}
		public void WriteInt8(sbyte value, DataElement element)
		{
			MarkStartOfElement(element, 8);
			WriteBits(value, 8);
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
			WriteBits(value, 16);
		}
		public void WriteInt16(short value, DataElement element)
		{
			MarkStartOfElement(element, 16);
			WriteBits(value, 16);
		}
		public void WriteUShort(ushort value, DataElement element)
		{
			MarkStartOfElement(element, 16);
			WriteBits(value, 16);
		}
		public void WriteUInt16(ushort value, DataElement element)
		{
			MarkStartOfElement(element, 16);
			WriteBits(value, 16);
		}
		public void WriteWORD(ushort value, DataElement element)
		{
			MarkStartOfElement(element, 16);
			WriteBits(value, 16);
		}
		public void WriteInt(int value, DataElement element)
		{
			MarkStartOfElement(element, 32);
			WriteBits(value, 32);
		}
		public void WriteInt32(int value, DataElement element)
		{
			MarkStartOfElement(element, 32);
			WriteBits(value, 32);
		}
		public void WriteUInt(uint value, DataElement element)
		{
			MarkStartOfElement(element, 32);
			WriteBits(value, 32);
		}
		public void WriteUInt32(uint value, DataElement element)
		{
			MarkStartOfElement(element, 32);
			WriteBits(value, 32);
		}
		public void WriteDWORD(uint value, DataElement element)
		{
			MarkStartOfElement(element, 32);
			WriteBits(value, 32);
		}
		public void WriteLong(long value, DataElement element)
		{
			MarkStartOfElement(element, 64);
			WriteBits(value, 64);
		}
		public void WriteInt64(long value, DataElement element)
		{
			MarkStartOfElement(element, 64);
			WriteBits(value, 64);
		}
		public void WriteULong(ulong value, DataElement element)
		{
			MarkStartOfElement(element, 64);
			WriteBits(value, 64);
		}
		public void WriteUInt64(ulong value, DataElement element)
		{
			MarkStartOfElement(element, 64);
			WriteBits(value, 64);
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

			ulong bytesToWrite = bits.LengthBits/8;
			ulong extraBits = bits.LengthBits - (bytesToWrite*8);
			ulong origionalPos = pos;

			bits.SeekBits(0, SeekOrigin.Begin);
			WriteBytes(bits.ReadBytes(bytesToWrite));
			WriteBits(bits.ReadBits(extraBits));

			// Copy over DataElement positions, replace
			// existing entries if they exist.
			foreach (String key in bits._elementPositions.Keys)
			{
				if (!_elementPositions.Contains(key))
					_elementPositions.Add(key, bits._elementPositions[key]);
				else
					_elementPositions[key] = bits._elementPositions[key];

				_elementPositions[key][0] += origionalPos;
			}
		}

		public void WriteBits(ulong value, uint bits, DataElement element)
		{
			MarkStartOfElement(element, bits);
			WriteBits(value, bits);
		}

		/// <summary>
		/// Write bits using bitfield encoding.
		/// </summary>
		/// <param name="value">Value to write</param>
		/// <param name="bits">Number of bits to write</param>
		public void WriteBits(ulong value, uint bits)
		{
			if(bits == 0 || bits > 64)
				throw new ApplicationException("bits is invalid value, but be > 0 and < 64");

			uint newpos = pos + bitlen;

			uint startBPos = pos % 8;
			uint startBlock = pos / 8;

			uint endBPos = newpos % 8;
			uint endBlock = newpos / 8 + (endBPos != 0);

			// Grow buffer if needed
			while (buff.Count < endBlock)
			{
				len += 8;
				buff.Add(0);
			}

			curpos = startBPos;
			uint bitlen = bits;
			uint bitsLeft = 0;
			ulong mask;
			ulong shift;
			byte b;
			ulong n = value;
			n &= (1L << bitlen) - 1;

			if (_readLeftToRight)
			{
				while (bitlen > 0)
				{
					bitsLeft = 8 - (curpos % 8);

					if (bitsLeft > bitlen)
						bitsLeft = bitlen;

					mask = (1 << bitsLeft) - 1;

					buff[startBlock + (pos / 8)] ^= buff[startBlock + (pos / 8)] & (mask << (curpos % 8));
					buff[startBlock + (pos / 8)] |= (n & mask) << (pos % 8);

					n >>= bitsLeft;
					bitlen -= bitsLeft;
					curpos += bitsLeft;
				}
			}
			else
			{
				while (bitlen > 0)
				{
					bitsLeft = 8 - (curpos % 8);

					if (bitsLeft > bitlen)
						bitsLeft = bitlen;

					mask = (1 << bitsLeft) - 1;
					shift = (8 - BitLength(mask, 8)) - (pos - (pos / 8 * 8));
					b = n >> BitLength - BitLength(mask, 8);

					buff[startBlock + (pos / 8)] |= ((buff & mask) << shift);

					bitlen -= bitsLeft;
					curpos += bitsLeft;
				}
			}

			pos = newpos;
			if (pos > len)
				len = pos;
		}

		/// <summary>
		/// Number of bits required to store value.
		/// </summary>
		/// <param name="value">Value to calc bit length of</param>
		/// <returns>Number of bits required to store number.</returns>
		protected int BitLength(ulong value)
		{
			return BitLength(value, 64);
		}

		/// <summary>
		/// Number of bits required to store value
		/// </summary>
		/// <param name="value">Value to calc bit length of</param>
		/// <param name="maxBits">Max length in bits</param>
		/// <returns>Returns number of bits required to store number.</returns>
		protected int BitLength(ulong value, int maxBits)
		{
			int blen = 0;
			for (int i = 0; i < maxBits; i++)
				if (((value >> 1) & 1) == 1)
					blen = i;

			return blen;
		}

		public void WriteBytes(byte[] value)
		{
			foreach (byte b in value)
				WriteBits(b, 8);
		}

		public void WriteBytes(byte[] value, int offset, int length)
		{
			for (int i = offset; i < length && i < value.Count; i++)
				WriteBits(value[i], 8);
		}

		#region Reading methods

		public sbyte ReadSByte()
		{
			return (sbyte)ReadBits(8);
		}
		public sbyte ReadInt8()
		{
			return (sbyte)ReadBits(8);
		}
		public byte ReadByte()
		{
			return (byte)ReadBits(8);
		}
		public byte ReadUInt8()
		{
			return (byte)ReadBits(8);
		}
		public short ReadShort()
		{
			return (short)ReadBits(16);
		}
		public short ReadInt16()
		{
			return (short)ReadBits(16);
		}
		public ushort ReadUShort()
		{
			return (ushort)ReadBits(16);
		}
		public ushort ReadUInt16()
		{
			return (ushort)ReadBits(16);
		}
		public ushort ReadWORD()
		{
			return (ushort)ReadBits(16);
		}
		public int ReadInt()
		{
			return (int)ReadBits(32);
		}
		public int ReadInt32()
		{
			return (int)ReadBits(32);
		}
		public uint ReadUInt()
		{
			return (uint)ReadBits(32);
		}
		public uint ReadUInt32()
		{
			return (uint)ReadBits(32);
		}
		public uint ReadDWORD()
		{
			return (uint)ReadBits(32);
		}
		public long ReadLong()
		{
			return (long)ReadBits(64);
		}
		public long ReadInt64()
		{
			return (long)ReadBits(64);
		}
		public ulong ReadULong()
		{
			return (ulong)ReadBits(64);
		}
		public ulong ReadUInt64()
		{
			return (ulong)ReadBits(64);
		}

		#endregion

		public ulong ReadBits(uint bits)
		{
			uint newpos = pos + bits;
			uint orig_bitlen = bits;
			uint bitlen = bits;
			uint startBPos = pos % 8;
			uint startBlock = pos / 8;
			uint endBPos = newpos % 8;
			uint endBlock = newpos / 8 + (endBPos != 0 ? 1 : 0);
			ulong ret = 0;
			uint curpos = startBPos;
			uint bitsLeft = 0;
			ulong mask = 0;
			byte b = 0;
			ulong shift = 0;

			if (_readLeftToRight)
			{
				while (bitlen > 0)
				{
					bitsLeft = 8 - (curpos % 8);
					bitsToLeft = pos - (curpos / 8 * 8);

					if (bitsLeft > bitlen)
						bitsLeft = bitlen;

					mask = (1 << bitsLeft) - 1;

					b = buff[startBlock + (curpos / 8)];
					b = b >> (8 - bitsLeft) - bitsToLeft;

					shift = BitLength(mask, 8);
					ret = ret << shift;
					ret |= b & mask;

					shift += bitsLeft;
					bitlen -= bitsLeft;
					curpos += bitsLeft;
				}
			}
			else
			{
				while (bitlen > 0)
				{
					bitsLeft = 8 - (curpos - 8);

					if (bitsLeft > bitlen)
						bitsLeft = bitlen;

					bitsToLeft = curpos - (curpos / 8 * 8);
					mask = (1 << bitsLeft) - 1;

					b = buff[startBlock + (curpos / 8)];
					b = b >> (8 - bitsLeft) - bitsToLeft;

					ret |= (b & mask) << shift;

					shift += bitsLeft;
					bitlen -= bitsLeft;
					curpos += bitsLeft;
				}
			}

			pos = newpos;
			return ret;
		}

		public byte[] ReadBytes(uint count)
		{
			if (count == 0)
				throw new ApplicationException("Asking for zero bytes");
			if ((pos + count) > buff.Count)
				throw new ApplicationException("Count overruns buffer");

			byte[] ret = new byte[count];

			for (int i = 0; i<count; i++)
				ret[i] = ReadByte();

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
		public void Truncate(uint sizeInBits)
		{
			if (sizeInBits > len)
				throw new ApplicationException("sizeInbits larger then length of data");

			if (pos > sizeInBits)
				pos = sizeInBits;

			len = sizeInBits;
			uint startBlock = sizeInBits / 8 + (sizeInBits % 8 == 0 ? 0 : 1);
			buff.RemoveRange(startBlock, buff.Count - startBlock);

			// Remove element entries that were truncated off.

			List<String> keysToRemove = new List<string>();
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
			uint currentBlock = pos / 8;

			// If both streams are on an 8 bit boundry
			// this is the quick 'n easy method.
			if (pos % 8 == 0 && bits.LengthBits % 8 == 0)
			{
				buff.InsertRange(currentBlock, bits.Value);
				len += bits.LengthBits;
				pos += bits.LengthBits;
				return;
			}

			uint curpos = pos;
			uint curlen = len;
			uint retpos = pos;
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

			pos = retpos;
		}

		/// <summary>
		/// Byte array of stream.
		/// </summary>
		public byte[] Value
		{
			get { return buff; }
		}
	}
}

// end
