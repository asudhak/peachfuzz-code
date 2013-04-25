
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
using System.Collections.Generic;
using System.Text;
using Peach.Core.Dom;

using System.Collections.Specialized;
using System.Resources;
using System.Globalization;
using System.ComponentModel;
using System.Reflection;
using System.Net.Sockets;
using System.Security.Cryptography;

#if PEACH
using Peach.Core.IO.Conversion;
using System.Diagnostics;
#endif

namespace Peach.Core.IO
{
	/// <summary>
	/// This stream is how all data is stored and read by
	/// Peach data elements.  It supports padded and unpadded
	/// reading/writing and accessing data stream as bits and
	/// bytes.
	/// </summary>
	[Serializable]
	[DebuggerDisplay("{Progress}")]
	public class BitStream : IDisposable
	{
#if PEACH
		protected Dictionary<string, long[]> _elementPositions = new Dictionary<string, long[]>();
#endif
		protected Stream stream;
		protected long pos = 0;
		protected long len = 0;
		public EndianBitConverter bitConverter = null;
		protected bool isDisposed = false;

		/// <summary>
		/// Default constructor
		/// </summary>
		public BitStream()
		{
			stream = new MemoryStream();
			LittleEndian();
		}

		/// <summary>
		/// Constructor for BitStream class
		/// </summary>
		/// <param name="stream">Use stream as initial stream data.</param>
		public BitStream(Stream stream)
		{
			this.stream = stream;
			pos = stream.Position * 8;
			len = stream.Length * 8;
			LittleEndian();
		}

		/// <summary>
		/// Constructor for BitStream class
		/// </summary>
		/// <param name="buff">Use buff as initial stream data.</param>
		public BitStream(byte[] buff)
		{
			stream = new MemoryStream();
			stream.Write(buff, 0, buff.Length);
			stream.Position = 0;
			len = stream.Length * 8;
			LittleEndian();
		}

		/// <summary>
		/// Constructor for BitStream class
		/// </summary>
		/// <param name="buff">Use buff as initial stream data.</param>
		/// <param name="offset">Offset to start.</param>
		/// <param name="length">Length to use.</param>
		public BitStream(byte[] buff, int offset, int length)
		{
			stream = new MemoryStream();
			stream.Write(buff, offset, length);
			len = stream.Length * 8;
			LittleEndian();
		}

		/// <summary>
		/// Clear contents of stream.  After calling
		/// position will be 0 and length is also 0.
		/// </summary>
		public void Clear()
		{
			if (isDisposed)
				throw new ObjectDisposedException("BitStream already disposed");

			if (!(stream is MemoryStream))
				throw new ApplicationException("Error, unable to reset when stream is not a MemoryStream.");

			stream = new MemoryStream();
			_elementPositions = new Dictionary<string, long[]>();
			pos = 0;
			len = 0;
			LittleEndian();
		}

#if PEACH
		public void ClearElementPositions()
		{
			_elementPositions = new Dictionary<string, long[]>();
		}
#endif

		protected BitStream(Stream stream, long pos, long len,
			EndianBitConverter bitConverter, Dictionary<string, long[]> _elementPositions)
		{
			this.stream = stream;
			this.pos = pos;
			this.len = len;
			this._elementPositions = _elementPositions;
			this.bitConverter = bitConverter;

			this.stream.Seek(pos / 8, SeekOrigin.Begin);
		}

		/// <summary>
		/// Create exact copy of this BitStream
		/// </summary>
		/// <returns>Returns exact copy of this BitStream</returns>
		public BitStream Clone()
		{
			if (isDisposed)
				throw new ObjectDisposedException("BitStream already disposed");

			if (stream is MemoryStream)
			{
				Dictionary<string, long[]> copyOfElementPositions = new Dictionary<string, long[]>(_elementPositions);
				return new BitStream(new MemoryStream(((MemoryStream)stream).ToArray()), pos, len, bitConverter,
					copyOfElementPositions);
			}

			throw new ApplicationException("Error, unable to clone stream.");
		}

		public string Progress
		{
			get
			{
				return "Bytes: {0}/{1}, Bits: {2}/{3}".Fmt(TellBytes(), LengthBytes, TellBits(), LengthBits);
			}
		}

		/// <summary>
		/// Length in bits of buffer
		/// </summary>
		public long LengthBits
		{
			get
			{
				if (stream is Publisher)
				{
					//long l = len / 8 + (len % 8 == 0 ? 0 : 1);
					if (stream.Length > 1)
					{
						len = stream.Length * 8;
					}
				}

				return len;
			}
		}

		/// <summary>
		/// Length in bytes of buffer.  size is
		/// badded out to 8 bit boundry.
		/// </summary>
		public long LengthBytes
		{
			get
			{
				if (stream is Publisher)
				{
					//long l = len / 8 + (len % 8 == 0 ? 0 : 1);
					if (stream.Length > 1)
					{
						len = stream.Length * 8;
					}
				}

				return (len / 8) + (len % 8 == 0 ? 0 : 1);
			}
		}

		/// <summary>
		/// Current position in bits
		/// </summary>
		/// <returns>Returns current bit position</returns>
		public long TellBits()
		{
			return pos;
		}

		/// <summary>
		/// Current position in bytes
		/// </summary>
		/// <returns>Returns current byte position</returns>
		public long TellBytes()
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
			if (isDisposed)
				throw new ObjectDisposedException("BitStream already disposed");

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

			stream.Position = pos / 8;
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

		public long IndexOf(BitStream bits, long offsetBits)
		{
			if (offsetBits % 8 != 0)
				throw new NotImplementedException("Need to implement this!");
			if (this.LengthBits % 8 != 0)
				throw new NotImplementedException("Need to implement this!");
			if (bits.LengthBits % 8 != 0)
				throw new NotImplementedException("Need to implement this!");


			long tgtLen = bits.LengthBytes;
			long start = offsetBits / 8;
			long end = this.LengthBytes - tgtLen;

			for (long i = start; i <= end; ++i)
			{
				int j = 0;
				for (j = 0; j < tgtLen; ++j)
				{
					if (this.Value[i + j] != bits.Value[j])
						break;
				}
				if (j == tgtLen)
					return i * 8;
			}
			return -1;
		}

		public long IndexOf(BitStream bits)
		{
			return IndexOf(bits, 0);
		}

#if PEACH
		public void SeekToDataElement(DataElement elem)
		{
			SeekToDataElement(elem.fullName);
		}

		public bool HasDataElement(string name)
		{
			return _elementPositions.ContainsKey(name);
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
#endif

		#region BitControl

		/// <summary>
		/// Pack/unpack as big endian values.
		/// </summary>
		public void BigEndian()
		{
			if (isDisposed)
				throw new ObjectDisposedException("BitStream already disposed");

			bitConverter = new BigEndianBitConverter();
		}

		/// <summary>
		/// Pack/unpack as little endian values.
		/// </summary>
		public void LittleEndian()
		{
			if (isDisposed)
				throw new ObjectDisposedException("BitStream already disposed");

			bitConverter = new LittleEndianBitConverter();
		}

		#endregion

		#region DataElements

		/// <summary>
		/// Length of DataElement by bits
		/// </summary>
		/// <param name="e">DataElement that has already been written to stream</param>
		/// <returns>Returns size in bits of DataElement</returns>
		public long DataElementLength(DataElement e)
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
		public long DataElementLength(string fullName)
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
		public long DataElementPosition(DataElement e)
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
		public long DataElementPosition(string fullName)
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
		public void MarkStartOfElement(DataElement e, long lengthInBits)
		{
			if (e == null)
				throw new ApplicationException("DataElement 'e' is null");

			if (HasDataElement(e.fullName))
				_elementPositions[e.fullName][0] = pos;
			else
				_elementPositions.Add(e.fullName, new long[] { pos, lengthInBits });
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
				_elementPositions.Add(e.fullName, new long[] { pos, 0 });
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

#if PEACH

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


		public void Write(BitStream bits, DataElement element)
		{
			long currentPos = TellBits();
			foreach (var elem in bits._elementPositions)
			{
				_elementPositions.Add(elem.Key, new long[] { elem.Value[0] + currentPos, elem.Value[1] });
			}

			MarkStartOfElement(element, bits.LengthBits);
			Write(bits);
		}

#endif

		#endregion

		public static void CopyTo(Stream sin, Stream sout)
		{
			int len = (int)Math.Min(sin.Length, 1024*1024);
			sin.CopyTo(sout, len);
		}

		/// <summary>
		/// Write the contents of another BitStream into
		/// this BitStream.
		/// </summary>
		/// <param name="bits">BitStream to write data from.</param>
		public void Write(BitStream bits)
		{
			if (isDisposed)
				throw new ObjectDisposedException("BitStream already disposed");

			if(bits == null)
				throw new ApplicationException("bits parameter is null");
			
			if(bits.LengthBits == 0)
				return;

			// Are we starting from 0?
			if (pos == 0 && bits.LengthBits % 8 == 0)
			{
				long oPos = bits.stream.Position;
				bits.stream.Position = 0;
				CopyTo(bits.stream, stream);
				bits.stream.Position = oPos;

				pos += bits.LengthBits;
				if (pos > len)
					len = pos;

				return;
			}

			// Are we working in bytes?
			if (pos % 8 == 0 && bits.LengthBits % 8 == 0)
			{
				long oPos = bits.stream.Position;

				bits.stream.Position = 0;
				CopyTo(bits.stream, stream);
				bits.stream.Position = oPos;

				pos += bits.LengthBits;
				if (pos > len)
					len = pos;

				return;
			}

			long bytesToWrite = bits.LengthBits / 8;
			long extraBits = bits.LengthBits - (bytesToWrite * 8);
			long origionalPos = pos;

			bits.SeekBits(0, SeekOrigin.Begin);
            if (bytesToWrite != 0)
			    WriteBytes(bits.ReadBytes(bytesToWrite));
			if(extraBits > 0)
				WriteBits(bits.ReadBits((int)extraBits), (int)extraBits);

#if PEACH
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
#endif
		}

		public void WriteBits(ulong value, int bits, DataElement element)
		{
			MarkStartOfElement(element, bits);
			WriteBits(value, bits);
		}

		protected byte[] ClearingMasks = new byte[] { 0x7f, 0xBF, 0xDF, 0xef, 0xf7, 0xfb, 0xfd, 0xfe };

		public void WriteBit(byte bit)
		{
			if (isDisposed)
				throw new ObjectDisposedException("BitStream already disposed");

			if (bit > 1)
				throw new ApplicationException("WriteBit only takes values of 0 or 1.");

			// Index into buff[] array
			long byteIndex = 0;
			// Index into byte from buff[] array
			int bitIndex = 0;

			// Get byte position in buff
			byteIndex = pos / 8;

			// Calc position in byte to set
			bitIndex = (int) (pos - (byteIndex * 8));

			// Do we need to grow buff?
			if (byteIndex >= stream.Length)
				stream.WriteByte(0);

			stream.Seek(byteIndex, SeekOrigin.Begin);
			byte value = (byte)stream.ReadByte();

			if (bit == 0)
					// clear bit
				value = (byte)(value & ClearingMasks[bitIndex]);
			else
					// Set bit
				value = (byte)(value | (bit << (7 - bitIndex)));

			stream.Seek(byteIndex, SeekOrigin.Begin);
			stream.WriteByte(value);

			// Increment our current position
			pos++;
			if (pos > len)
				len = pos;
		}

		/// <summary>
		/// Write bits using bitfield encoding.
		/// </summary>
		/// <param name="value">Value to write</param>
		/// <param name="bits">Number of bits to write</param>
		public void WriteBits(ulong value, int bits)
		{
			if (isDisposed)
				throw new ObjectDisposedException("BitStream already disposed");

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
			if (isDisposed)
				throw new ObjectDisposedException("BitStream already disposed");

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
			if (isDisposed)
				throw new ObjectDisposedException("BitStream already disposed");

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

		public byte ReadBit()
		{
			if (isDisposed)
				throw new ObjectDisposedException("BitStream already disposed");

			byte b = (byte)stream.ReadByte();
			byte bitIndex = (byte)(pos++ % 8);

			if (bitIndex != 7)
				stream.Seek(-1, SeekOrigin.Current);

			return (byte)((b >> (byte)(7 - bitIndex)) & 1);
		}

		public ulong ReadBits(int bits)
		{
			if (isDisposed)
				throw new ObjectDisposedException("BitStream already disposed");

			if (bits < 0 || bits > 64)
				throw new ArgumentOutOfRangeException("Must be between 0 and 64");

			ulong ret = 0;

			for (int cnt = 0; cnt < bits; cnt++)
			{
				ret |= ((ulong)ReadBit() << (int)((bits - 1) - cnt));
			}

			return ret;
		}

		/// <summary>
		/// Read from our stream into a new BitStream.  This call
		/// is optimized for large reads.
		/// </summary>
		/// <param name="bits"></param>
		/// <returns></returns>
		public BitStream ReadBitsAsBitStream(long bits)
		{
			if (bits < 0)
				throw new ArgumentOutOfRangeException("Should not be negative!");

			if (bits % 8 == 0)
				return RealReadBytesAsBitStream(bits / 8);

			return RealReadBitsAsBitStream(bits);
		}

		/// <summary>
		/// Bit copies are fairly slow.  We need to optimize
		/// this somehow.
		/// </summary>
		/// <param name="bits"></param>
		/// <returns></returns>
		protected BitStream RealReadBitsAsBitStream(long bits)
		{
			BitStream newStream = new BitStream();

			while (pos % 8 > 0 && bits > 0)
			{
				bits--;
				newStream.WriteBit(ReadBit());
			}

			byte[] buff = new byte[1024];
			long streamPosition = stream.Position;
			long streamCount = 0;
			int len;

			while (bits / (8 * 1024) > 0)
			{
				len = this.stream.Read(buff, 0, 1024);
				newStream.WriteBytes(buff, 0, len);
				bits -= (8 * len);
			}

			while (bits / 8 > 0)
			{
				bits -= 8;
				newStream.WriteByte((byte)stream.ReadByte());
			}

			streamCount = stream.Position - streamPosition;
			stream.Position = streamPosition;
			if (streamCount > 0)
				SeekBytes((int)streamCount, SeekOrigin.Current);

			if (bits % 8 > 0)
				newStream.WriteBits(ReadBits((int)(bits % 8)), (int)bits % 8);

			newStream.SeekBits(0, SeekOrigin.Begin);

			return newStream;
		}

		/// <summary>
		/// Optimized reading of bytes from our stream.
		/// </summary>
		/// <param name="bytes">Number of bytes to copy</param>
		/// <returns>Returns BitStream instance with our data.</returns>
		protected BitStream RealReadBytesAsBitStream(long bytes)
		{
			if (bytes > (LengthBytes - TellBytes()))
				return null;

			// Assume our internal state is correct
			stream.Position = this.pos / 8;

			MemoryStream sin = new MemoryStream();

			long totalBytes = bytes;

			// Are we copying entire stream over?
			if ((bytes * 8) == LengthBits && pos == 0)
			{
				CopyTo(stream, sin);
				sin.Position = 0;
				stream.Position = 0;

				SeekBits(0, SeekOrigin.End);

				return new BitStream(sin);
			}

			// Do a fast copy if we are byte aligned
			if (pos % 8 == 0)
			{
				byte[] buff = new byte[32768];
				long streamPosition = stream.Position;
				int len;

				while (bytes / 32768 > 0)
				{
					len = this.stream.Read(buff, 0, 32768);
					sin.Write(buff, 0, len);

					bytes -= len;
				}

				len = stream.Read(buff, 0, (int)bytes);
				if (len != bytes)
					throw new ApplicationException("Read failed");

				sin.Write(buff, 0, (int)bytes);

				stream.Position = streamPosition;
				sin.Position = 0;

				SeekBits((totalBytes * 8), SeekOrigin.Current);

				return new BitStream(sin);
			}

			// Perform slow bit copy if we are not byte alligned.
			return RealReadBitsAsBitStream(bytes * 8);
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

		public byte[] ReadBitsAsBytes(long sizeInBits)
		{
			byte[] buf = new byte[(sizeInBits + 7) / 8];

			int i = 0;

			while (sizeInBits >= 8)
			{
				buf[i++] = ReadByte();
				sizeInBits -= 8;
			}

			if (sizeInBits > 0)
			{
				buf[i] = (byte)(ReadBits((int)sizeInBits) << (int)(8 - sizeInBits));
			}

			return buf;
		}

		public byte[] ReadBytes(long count)
		{
			if (count == 0)
				return new byte[0];
			if (((pos/8) + count) > stream.Length)
				throw new ApplicationException("Count overruns buffer");
			if (count < 0)
				throw new ArgumentOutOfRangeException("negative count");

			byte[] ret = new byte[count];

			for (int i = 0; i < count; i++)
			{
				ret[i] = ReadByte();
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
		public void Truncate(long sizeInBits)
		{
			if (isDisposed)
				throw new ObjectDisposedException("BitStream already disposed");

			if (sizeInBits > len)
				throw new ApplicationException("sizeInbits larger then length of data");

			if (!(stream is MemoryStream))
				throw new ApplicationException("Error, unable to truncate stream that is not a MemoryStream.");

			if (pos > sizeInBits)
				pos = sizeInBits;

			List<byte> buff = new List<byte>(((MemoryStream)stream).ToArray());

			len = sizeInBits;
			long startBlock = sizeInBits / 8 + (sizeInBits % 8 == 0 ? 0 : 1);
			buff.RemoveRange((int)startBlock, buff.Count - (int)startBlock);

			stream = new MemoryStream(buff.ToArray());

#if PEACH
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
#endif
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
			if (isDisposed)
				throw new ObjectDisposedException("BitStream already disposed");

			if (!(stream is MemoryStream))
				throw new ApplicationException("Error, unable to insert into non-MemoryStream");

			long currentBlock = pos / 8;
			long curpos = pos;
			long curlen = len;
			long retpos = pos;
			long[] vals;

			// If both streams are on an 8 bit boundry
			// this is the quick 'n easy method.
			if (pos % 8 == 0 && bits.LengthBits % 8 == 0)
			{
				List<byte> buff = new List<byte>(((MemoryStream)stream).ToArray());
				buff.InsertRange((int)currentBlock, bits.Value);
				len += bits.LengthBits;
				pos += bits.LengthBits;

				stream = new MemoryStream(buff.ToArray());
				stream.Seek(pos / 8, SeekOrigin.Begin);

#if PEACH
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
#endif

				return;
			}

			BitStream tmp = Clone();
			Truncate();

			bits.SeekBits(0, SeekOrigin.Begin);
			WriteBytes(bits.ReadBytes(bits.LengthBits / 8));
			if(bits.LengthBits % 8 != 0)
				WriteBits(bits.ReadBits((int)(bits.LengthBits % 8)), (int)(bits.LengthBits % 8));

			retpos = pos;

			tmp.SeekBits(curpos, SeekOrigin.Begin);
			WriteBytes(tmp.ReadBytes((curlen - curpos) / 8));
			if ((curlen - curpos) % 8 != 0)
				WriteBits(tmp.ReadBits((int)((curlen - curpos) % 8)),(int) ((curlen - curpos) % 8));

#if PEACH
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
#endif

			pos = retpos;
		}

		/// <summary>
		/// Byte array of stream.
		/// </summary>
		public byte[] Value
		{
			get
			{
				if (isDisposed)
					throw new ObjectDisposedException("BitStream already disposed");

				if(stream is MemoryStream)
					return ((MemoryStream)stream).ToArray();

				byte [] buff = new byte[stream.Length];
				stream.Seek(0, SeekOrigin.Begin);
				stream.Read(buff, 0, buff.Length);
				stream.Seek(TellBytes(), SeekOrigin.Begin);

				return buff;
		}
	}

		/// <summary>
		/// Locate the first occurance of data and return index.
		/// </summary>
		/// <param name="data">Data to search for</param>
		/// <returns>Returns index or -1 if not found.</returns>
		public long IndexOf(byte[] data)
		{
			if (isDisposed)
				throw new ObjectDisposedException("BitStream already disposed");

			long currentPosition = TellBits();
			bool found = false;
			long foundAt = -1;

			while (TellBits() < LengthBits)
			{
				foundAt = TellBytes();

				if (ReadByte() == data[0])
				{
					found = true;
					for (int i = 1; found && i < data.Length; i++)
						if (data[i] != ReadByte())
							found = false;
				}

				if (found)
				{
					SeekBits(currentPosition, SeekOrigin.Begin);
					return foundAt;
			}
			}

			SeekBits(currentPosition, SeekOrigin.Begin);
			return -1;
		}

		public Stream Stream
		{
			get { return stream; }
		}

		public void WantBytes(long bytes)
		{
			if (bytes <= 0)
				return;

			Publisher pub = stream as Publisher;
			if (pub != null)
				pub.WantBytes(bytes);
		}

		#region IDisposable Members

		public void Dispose()
		{
			isDisposed = true;

			if (stream != null)
			stream.Dispose();

			stream = null;
		}

		#endregion
	}

}

// end
