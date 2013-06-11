
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
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core;
using Peach.Core.IO;
using System.IO;

namespace Peach.Core.Test
{
	[TestFixture]
	public class BitStreamTest
	{
		[Test]
		public void Length()
		{
			BitStream bs = new BitStream();

			Assert.AreEqual(0, bs.LengthBits);

			bs.WriteBit(0);
			Assert.AreEqual(1, bs.LengthBits);

			bs = new BitStream();
			for (int i = 1; i < 10000; i++)
			{
				bs.WriteBit(0);
				Assert.AreEqual(i, bs.LengthBits);
			}

			bs = new BitStream();
			bs.WriteByte(1);
			Assert.AreEqual(8, bs.LengthBits);
			Assert.AreEqual(1, bs.LengthBytes);

			bs = new BitStream(new byte[] { 1, 2, 3, 4, 5 });
			Assert.AreEqual(5, bs.LengthBytes);
			Assert.AreEqual(5 * 8, bs.LengthBits);
		}

		[Test]
		public void ReadingBites()
		{
			var bs = new BitStream(new byte[] { 0x41, 0x41 });

			bs.SeekBits(0, System.IO.SeekOrigin.Begin);

			Assert.AreEqual(0, bs.ReadBit()); // 0
			Assert.AreEqual(1, bs.ReadBit()); // 1
			Assert.AreEqual(0, bs.ReadBit()); // 2
			Assert.AreEqual(0, bs.ReadBit()); // 3
			Assert.AreEqual(0, bs.ReadBit()); // 4
			Assert.AreEqual(0, bs.ReadBit()); // 5
			Assert.AreEqual(0, bs.ReadBit()); // 6
			Assert.AreEqual(1, bs.ReadBit()); // 7

			Assert.AreEqual(0, bs.ReadBit()); // 0
			Assert.AreEqual(1, bs.ReadBit()); // 1
			Assert.AreEqual(0, bs.ReadBit()); // 2
			Assert.AreEqual(0, bs.ReadBit()); // 3
			Assert.AreEqual(0, bs.ReadBit()); // 4
			Assert.AreEqual(0, bs.ReadBit()); // 5
			Assert.AreEqual(0, bs.ReadBit()); // 6
			Assert.AreEqual(1, bs.ReadBit()); // 7
		}

		[Test]
		public void ReadWriteBits()
		{
			BitStream bs = new BitStream();

			bs.WriteBit(0);
			bs.WriteBit(0);
			bs.WriteBit(0);
			bs.WriteBit(1);
			bs.WriteBit(0);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);

			bs.WriteBit(0);
			bs.WriteBit(0);
			bs.WriteBit(0);
			bs.WriteBit(1);
			bs.WriteBit(0);
			bs.WriteBit(1);

			bs.SeekBits(0, System.IO.SeekOrigin.Begin);

			Assert.AreEqual(0, bs.ReadBit());
			Assert.AreEqual(0, bs.ReadBit());
			Assert.AreEqual(0, bs.ReadBit());
			Assert.AreEqual(1, bs.ReadBit());
			Assert.AreEqual(0, bs.ReadBit());
			Assert.AreEqual(1, bs.ReadBit());
			Assert.AreEqual(1, bs.ReadBit());
			Assert.AreEqual(1, bs.ReadBit());
			Assert.AreEqual(1, bs.ReadBit());

			Assert.AreEqual(0, bs.ReadBit());
			Assert.AreEqual(0, bs.ReadBit());
			Assert.AreEqual(0, bs.ReadBit());
			Assert.AreEqual(1, bs.ReadBit());
			Assert.AreEqual(0, bs.ReadBit());
			Assert.AreEqual(1, bs.ReadBit());
		}
		protected static string Int2String(int b)
		{
			string ret = "";

			for (int i = 0; i < 32; i++)
			{
				int bit = (b >> 31 - i) & 1;
				ret += bit == 0 ? "0" : "1";
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

		protected static string Short2String(short b)
		{
			string ret = "";

			for (int i = 0; i < 16; i++)
			{
				int bit = (b >> 15 - i) & 1;
				ret += bit == 0 ? "0" : "1";
			}

			return ret;
		}

		[Test]
		public void ReadWriteNumbers()
		{
			BitStream bs = new BitStream();
			BitWriter w = new BitWriter(bs);
			BitReader r = new BitReader(bs);

			w.LittleEndian();
			r.LittleEndian();

			//Max
			w.WriteSByte(sbyte.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(sbyte.MaxValue, r.ReadSByte());

			bs.SetLength(0);
			w.WriteInt16(short.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(short.MaxValue, r.ReadInt16());

			bs.SetLength(0);
			w.WriteInt32(67305985);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(67305985, r.ReadInt32());

			bs.SetLength(0);
			w.WriteInt32(Int32.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int32.MaxValue, r.ReadInt32());

			bs.SetLength(0);
			w.WriteInt64(Int64.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int64.MaxValue, r.ReadInt64());

			bs.SetLength(0);
			w.WriteByte(byte.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(byte.MaxValue, r.ReadByte());

			bs.SetLength(0);
			w.WriteUInt16(ushort.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(ushort.MaxValue, r.ReadUInt16());

			bs.SetLength(0);
			w.WriteUInt32(UInt32.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(UInt32.MaxValue, r.ReadUInt32());

			bs.SetLength(0);
			w.WriteUInt64(UInt64.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(UInt64.MaxValue, r.ReadUInt64());

			//Min
			bs.SetLength(0);
			w.WriteSByte(sbyte.MinValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(sbyte.MinValue, r.ReadSByte());

			bs.SetLength(0);
			w.WriteInt16(short.MinValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(short.MinValue, r.ReadInt16());

			bs.SetLength(0);
			w.WriteInt32(Int32.MinValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int32.MinValue, r.ReadInt32());

			bs.SetLength(0);
			w.WriteInt64(Int64.MinValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int64.MinValue, r.ReadInt64());

			// BIG ENDIAN //////////////////////////////////////////

			bs.SetLength(0);
			w.LittleEndian();
			r.LittleEndian();

			//Max
			w.WriteSByte(sbyte.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(sbyte.MaxValue, r.ReadSByte());

			bs.SetLength(0);
			w.WriteInt16(short.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(short.MaxValue, r.ReadInt16());

			bs.SetLength(0);
			w.WriteInt32(67305985);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(67305985, r.ReadInt32());

			bs.SetLength(0);
			w.WriteInt32(Int32.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int32.MaxValue, r.ReadInt32());

			bs.SetLength(0);
			w.WriteInt64(Int64.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int64.MaxValue, r.ReadInt64());

			bs.SetLength(0);
			w.WriteByte(byte.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(byte.MaxValue, r.ReadByte());

			bs.SetLength(0);
			w.WriteUInt16(ushort.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(ushort.MaxValue, r.ReadUInt16());

			bs.SetLength(0);
			w.WriteUInt32(UInt32.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(UInt32.MaxValue, r.ReadUInt32());

			bs.SetLength(0);
			w.WriteUInt64(UInt64.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(UInt64.MaxValue, r.ReadUInt64());

			//Min
			bs.SetLength(0);
			w.WriteSByte(sbyte.MinValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(sbyte.MinValue, r.ReadSByte());

			bs.SetLength(0);
			w.WriteInt16(short.MinValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(short.MinValue, r.ReadInt16());

			bs.SetLength(0);
			w.WriteInt32(Int32.MinValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int32.MinValue, r.ReadInt32());

			bs.SetLength(0);
			w.WriteInt64(Int64.MinValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int64.MinValue, r.ReadInt64());
		}

		[Test]
		public void ReadWriteNumbersOddOffset()
		{
			BitStream bs = new BitStream();
			BitWriter w = new BitWriter(bs);
			BitReader r = new BitReader(bs);

			w.LittleEndian();
			r.LittleEndian();

			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteBit(1);

			Assert.AreEqual(3, bs.LengthBits);
			Assert.AreEqual(3, bs.PositionBits);

			//Max
			w.WriteSByte(sbyte.MaxValue);

			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(0, r.ReadBit());
			Assert.AreEqual(1, r.ReadBit());
			Assert.AreEqual(1, r.ReadBit());
			Assert.AreEqual(1, r.ReadBit());
			Assert.AreEqual(1, r.ReadBit());
			Assert.AreEqual(1, r.ReadBit());
			Assert.AreEqual(1, r.ReadBit());

			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(sbyte.MaxValue, r.ReadSByte());

			bs.SetLength(0);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteInt16(short.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(short.MaxValue, r.ReadInt16());

			bs.SetLength(0);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteInt32(67305985);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(67305985, r.ReadInt32());

			bs.SetLength(0);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteInt32(Int32.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int32.MaxValue, r.ReadInt32());

			bs.SetLength(0);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteInt64(Int64.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int64.MaxValue, r.ReadInt64());

			bs.SetLength(0);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteByte(byte.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(byte.MaxValue, r.ReadByte());

			bs.SetLength(0);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteUInt16(ushort.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(ushort.MaxValue, r.ReadUInt16());

			bs.SetLength(0);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteUInt32(UInt32.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(UInt32.MaxValue, r.ReadUInt32());

			bs.SetLength(0);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteUInt64(UInt64.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(UInt64.MaxValue, r.ReadUInt64());


			//Min
			bs.SetLength(0);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteSByte(sbyte.MinValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(sbyte.MinValue, r.ReadSByte());

			bs.SetLength(0);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteInt16(short.MinValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(short.MinValue, r.ReadInt16());

			bs.SetLength(0);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteInt32(Int32.MinValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int32.MinValue, r.ReadInt32());

			bs.SetLength(0);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteInt64(Int64.MinValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int64.MinValue, r.ReadInt64());

			// BIG ENDIAN //////////////////////////////////////////

			bs.SetLength(0);
			r.BigEndian();
			w.BigEndian();

			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteBit(1);

			//Max
			w.WriteSByte(sbyte.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(sbyte.MaxValue, r.ReadSByte());

			bs.SetLength(0);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteInt16(short.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(short.MaxValue, r.ReadInt16());

			bs.SetLength(0);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteInt32(67305985);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(67305985, r.ReadInt32());

			bs.SetLength(0);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteInt32(Int32.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int32.MaxValue, r.ReadInt32());

			bs.SetLength(0);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteInt64(Int64.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int64.MaxValue, r.ReadInt64());

			bs.SetLength(0);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteByte(byte.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(byte.MaxValue, r.ReadByte());

			bs.SetLength(0);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteUInt16(ushort.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(ushort.MaxValue, r.ReadUInt16());

			bs.SetLength(0);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteUInt32(UInt32.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(UInt32.MaxValue, r.ReadUInt32());

			bs.SetLength(0);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteUInt64(UInt64.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(UInt64.MaxValue, r.ReadUInt64());

			//Min
			bs.SetLength(0);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteSByte(sbyte.MinValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(sbyte.MinValue, r.ReadSByte());

			bs.SetLength(0);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteInt16(short.MinValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(short.MinValue, r.ReadInt16());

			bs.SetLength(0);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteInt32(Int32.MinValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int32.MinValue, r.ReadInt32());

			bs.SetLength(0);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteBit(1);
			w.WriteInt64(Int64.MinValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int64.MinValue, r.ReadInt64());
		}

		[Test]
		public void TestSeek()
		{
			MemoryStream ms = new MemoryStream();
			Assert.AreEqual(0, ms.Length);
			Assert.AreEqual(0, ms.Position);
			ms.Seek(10, SeekOrigin.Begin);
			Assert.AreEqual(0, ms.Length);
			Assert.AreEqual(10, ms.Position);
			ms.WriteByte(1);
			Assert.AreEqual(11, ms.Length);
			Assert.AreEqual(11, ms.Position);

			BitStream bs = new BitStream();
			Assert.AreEqual(0, bs.LengthBits);
			Assert.AreEqual(0, bs.Length);
			Assert.AreEqual(0, bs.PositionBits);
			Assert.AreEqual(0, bs.Position);
			Assert.AreEqual(0, bs.BaseStream.Length);
			Assert.AreEqual(0, bs.BaseStream.Position);

			bs.SeekBits(10, SeekOrigin.Begin);
			Assert.AreEqual(0, bs.LengthBits);
			Assert.AreEqual(0, bs.Length);
			Assert.AreEqual(10, bs.PositionBits);
			Assert.AreEqual(1, bs.Position);
			Assert.AreEqual(0, bs.BaseStream.Length);
			Assert.AreEqual(1, bs.BaseStream.Position);

			bs.WriteBit(1);
			Assert.AreEqual(11, bs.LengthBits);
			Assert.AreEqual(1, bs.Length);
			Assert.AreEqual(11, bs.PositionBits);
			Assert.AreEqual(1, bs.Position);
			Assert.AreEqual(2, bs.BaseStream.Length);
			Assert.AreEqual(1, bs.BaseStream.Position);
		}

		class BitStreamWriter
		{
			Endian endian;

			public BitStream Stream = new BitStream();

			public BitStreamWriter(Endian endian)
			{
				this.endian = endian;
			}

			public void Write(long value, int bits)
			{
				Stream.WriteBits(endian.GetBits(value, bits), bits);
			}

		}

		[Test]
		public void BitConverter()
		{
			/*
			 * Unsigned, BE, 12bit "A B C" -> 0x0ABC ->  2748
			 * Signed  , BE, 12bit "A B C" -> 0xFABC -> -1348
			 * Unsigned, LE, 12bit "B C A" -> 0x0ABC ->  2748
			 * Signed  , LE, 12bit "B C A" -> 0xFABC -> -1348
			 */

			byte[] val = null;

			val = Endian.Big.GetBytes(0xABC, 0);
			Assert.AreEqual(new byte[] { }, val);
			Assert.AreEqual(0, Endian.Big.GetUInt64(val, 0));
			Assert.AreEqual(0, Endian.Big.GetInt64(val, 0));

			val = Endian.Big.GetBytes(0xABC, 12);
			Assert.AreEqual(new byte[] { 0xab, 0xc0 }, val);
			Assert.AreEqual(2748, Endian.Big.GetUInt64(val, 12));
			Assert.AreEqual(-1348, Endian.Big.GetInt64(val, 12));

			val = Endian.Little.GetBytes(0xABC, 0);
			Assert.AreEqual(new byte[] { }, val);
			Assert.AreEqual(0, Endian.Little.GetUInt64(val, 0));
			Assert.AreEqual(0, Endian.Little.GetInt64(val, 0));

			val = Endian.Little.GetBytes(0xABC, 12);
			Assert.AreEqual(new byte[] { 0xbc, 0xa0 }, val);
			Assert.AreEqual(2748, Endian.Little.GetUInt64(val, 12));
			Assert.AreEqual(-1348, Endian.Little.GetInt64(val, 12));

			ulong bits = 0;

			bits = Endian.Big.GetBits(0xABC, 12);
			Assert.AreEqual(0xABC, bits);
			Assert.AreEqual(2748, Endian.Big.GetUInt64(bits, 12));
			Assert.AreEqual(-1348, Endian.Big.GetInt64(bits, 12));

			bits = Endian.Little.GetBits(0xABC, 12);
			Assert.AreEqual(0xBCA, bits);
			Assert.AreEqual(2748, Endian.Little.GetUInt64(bits, 12));
			Assert.AreEqual(-1348, Endian.Little.GetInt64(bits, 12));
		}

		[Test]
		public void BitStreamBits()
		{
			byte[] expected = new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89 };
			BitStream bs = new BitStream(expected);
			ulong test;
			bs.ReadBits(out test, 40);
			Assert.AreEqual(0x0123456789, test);

			bs = new BitStream();
			bs.WriteBits(test, 40);
			Assert.AreEqual(40, bs.LengthBits);
			MemoryStream ms = bs.BaseStream as MemoryStream;
			Assert.NotNull(ms);
			Assert.AreEqual(ms.Length, 5);

			for (int i = 0; i < expected.Length; ++i)
				Assert.AreEqual(expected[i], ms.GetBuffer()[i]);
		}

		[Test]
		public void BitwiseNumbers()
		{
			/*
			 * Unsigned, BE, 12bit "A B C" -> 0x0ABC ->  2748
			 * Signed  , BE, 12bit "A B C" -> 0xFABC -> -1348
			 * Unsigned, LE, 12bit "B C A" -> 0x0ABC ->  2748
			 * Signed  , LE, 12bit "B C A" -> 0xFABC -> -1348
			 */

			var w = new BitStreamWriter(Endian.Big);
			w.Write(0, 32);
			w.Write(0x00, 1);
			w.Write(0x01, 1);
			w.Write(0x00, 1);
			w.Write(-1, 5);
			w.Write(0x0f, 4);
			w.Write(0x123456789A, 40);

			Assert.AreEqual(84, w.Stream.LengthBits);
			Assert.AreEqual(10, w.Stream.Length);
			Assert.AreEqual(11, w.Stream.BaseStream.Length);
			byte[] exp1 = new byte[] { 0x0, 0x0, 0x0, 0x0, 0x5f, 0xf1, 0x23, 0x45, 0x67, 0x89, 0xa0 };
			Assert.AreEqual(exp1, w.Stream.ToArray());

			var w1 = new BitStreamWriter(Endian.Little);
			w1.Write(0x12345678, 32);
			w1.Write(0xabc, 12);

			Assert.AreEqual(44, w1.Stream.LengthBits);
			Assert.AreEqual(5, w1.Stream.Length);
			Assert.AreEqual(6, w1.Stream.BaseStream.Length);
			byte[] exp2 = new byte[] { 0x78, 0x56, 0x34, 0x12, 0xbc, 0xa0 };
			Assert.AreEqual(exp2, w1.Stream.ToArray());

			var w2 = new BitStreamWriter(Endian.Little);
			w2.Write(1, 1);
			w2.Write(0, 1);
			w2.Write(0xffff, 6);

			Assert.AreEqual(8, w2.Stream.LengthBits);
			Assert.AreEqual(1, w2.Stream.Length);
			Assert.AreEqual(1, w2.Stream.BaseStream.Length);
			byte[] exp3 = new byte[] { 0xbf };
			Assert.AreEqual(exp3, w2.Stream.ToArray());
		}

		[Test]
		public void ReadBitStream()
		{
			var bs = new BitStream();
			bs.Write(new byte[] { 0x11, 0x27, 0x33, 0x44, 0x55 }, 0, 5);
			bs.SeekBits(0, SeekOrigin.Begin);
			BitStream in1 = bs.SliceBits(8 + 4);
			BitStream in2 = bs.SliceBits(2);
			BitStream in3 = bs.SliceBits(2 + 16 + 4);
			BitStream in4 = in3.SliceBits(16);

			Assert.AreEqual(new byte[] { 0x11, 0x20 }, in1.Value);
			Assert.AreEqual(new byte[] { 0x40 }, in2.Value);
			Assert.AreEqual(new byte[] { 0xcc, 0xd1, 0x14 }, in3.Value);
			Assert.AreEqual(new byte[] { 0xcc, 0xd1 }, in4.Value);
		}

	}
}
