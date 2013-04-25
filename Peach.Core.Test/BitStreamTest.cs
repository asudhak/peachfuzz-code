
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
			BitStream bs = new BitStream(new byte[] { 0x41, 0x41 });
			//bs.LittleEndian();

			//Assert.AreEqual(1, bs.ReadBit()); // 0
			//Assert.AreEqual(0, bs.ReadBit()); // 1
			//Assert.AreEqual(0, bs.ReadBit()); // 2
			//Assert.AreEqual(0, bs.ReadBit()); // 3
			//Assert.AreEqual(0, bs.ReadBit()); // 4
			//Assert.AreEqual(0, bs.ReadBit()); // 5
			//Assert.AreEqual(1, bs.ReadBit()); // 6
			//Assert.AreEqual(0, bs.ReadBit()); // 7

			//Assert.AreEqual(1, bs.ReadBit()); // 0
			//Assert.AreEqual(0, bs.ReadBit()); // 1
			//Assert.AreEqual(0, bs.ReadBit()); // 2
			//Assert.AreEqual(0, bs.ReadBit()); // 3
			//Assert.AreEqual(0, bs.ReadBit()); // 4
			//Assert.AreEqual(0, bs.ReadBit()); // 5
			//Assert.AreEqual(1, bs.ReadBit()); // 6
			//Assert.AreEqual(0, bs.ReadBit()); // 7

			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			bs.BigEndian();

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
			bs.LittleEndian();

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
			bs.LittleEndian();

			//Max
			bs.WriteInt8(sbyte.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);

			Console.WriteLine(Byte2String((byte)sbyte.MaxValue));
			foreach (byte b in bs.Value)
				Console.WriteLine(Byte2String(b));
			Console.WriteLine(Byte2String((byte)bs.ReadInt8()));

			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(sbyte.MaxValue, bs.ReadInt8());

			bs.Clear();
			bs.WriteInt16(short.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);

			Console.WriteLine(Short2String(short.MaxValue));
			foreach (byte b in bs.Value)
				Console.Write(Byte2String(b));
			Console.WriteLine("\n" + Short2String(bs.ReadInt16()));

			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(short.MaxValue, bs.ReadInt16());

			bs.Clear();
			bs.WriteInt32(67305985);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);

			Console.WriteLine(Int2String(67305985));
			foreach (byte b in bs.Value)
				Console.Write(Byte2String(b));
			Console.WriteLine("\n" + Int2String(bs.ReadInt32()));

			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(67305985, bs.ReadInt32());

			bs.Clear();
			bs.WriteInt32(Int32.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int32.MaxValue, bs.ReadInt32());

			bs.Clear();
			bs.WriteInt64(Int64.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int64.MaxValue, bs.ReadInt64());

			bs.Clear();
			bs.WriteUInt8(byte.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(byte.MaxValue, bs.ReadUInt8());

			bs.Clear();
			bs.WriteUInt16(ushort.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(ushort.MaxValue, bs.ReadUInt16());

			bs.Clear();
			bs.WriteUInt32(UInt32.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(UInt32.MaxValue, bs.ReadUInt32());

			bs.Clear();
			bs.WriteUInt64(UInt64.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(UInt64.MaxValue, bs.ReadUInt64());


			//Min
			bs.Clear();
			bs.WriteInt8(sbyte.MinValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(sbyte.MinValue, bs.ReadInt8());

			bs.Clear();
			bs.WriteInt16(short.MinValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);

			Console.WriteLine(Short2String(short.MinValue));
			foreach (byte b in bs.Value)
				Console.Write(Byte2String(b));
			Console.WriteLine("\n" + Short2String(bs.ReadInt16()));

			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(short.MinValue, bs.ReadInt16());

			bs.Clear();
			bs.WriteInt32(Int32.MinValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int32.MinValue, bs.ReadInt32());

			bs.Clear();
			bs.WriteInt64(Int64.MinValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int64.MinValue, bs.ReadInt64());

			// BIG ENDIAN //////////////////////////////////////////

			bs = new BitStream();
			bs.BigEndian();

			//Max
			bs.WriteInt8(sbyte.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(sbyte.MaxValue, bs.ReadInt8());

			bs.Clear();
			bs.WriteInt16(short.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(short.MaxValue, bs.ReadInt16());

			bs.Clear();
			bs.WriteInt32(67305985);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(67305985, bs.ReadInt32());

			bs.Clear();
			bs.WriteInt32(Int32.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int32.MaxValue, bs.ReadInt32());

			bs.Clear();
			bs.WriteInt64(Int64.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int64.MaxValue, bs.ReadInt64());

			bs.Clear();
			bs.WriteUInt8(byte.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(byte.MaxValue, bs.ReadUInt8());

			bs.Clear();
			bs.WriteUInt16(ushort.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(ushort.MaxValue, bs.ReadUInt16());

			bs.Clear();
			bs.WriteUInt32(UInt32.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(UInt32.MaxValue, bs.ReadUInt32());

			bs.Clear();
			bs.WriteUInt64(UInt64.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(UInt64.MaxValue, bs.ReadUInt64());

			//Min
			bs.Clear();
			bs.WriteInt8(sbyte.MinValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(sbyte.MinValue, bs.ReadInt8());

			bs.Clear();
			bs.WriteInt16(short.MinValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(short.MinValue, bs.ReadInt16());

			bs.Clear();
			bs.WriteInt32(Int32.MinValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int32.MinValue, bs.ReadInt32());

			bs.Clear();
			bs.WriteInt64(Int64.MinValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int64.MinValue, bs.ReadInt64());
		}

		[Test]
		public void ReadWriteNumbersOddOffset()
		{
			BitStream bs = new BitStream();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);

			//bs.LittleEndian();

			Console.WriteLine("After 3");
			foreach (byte b in bs.Value)
				Console.Write(Byte2String(b));
			Console.WriteLine("");

			//Max
			bs.WriteInt8(sbyte.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);

			Console.WriteLine(Byte2String((byte)sbyte.MaxValue));
			foreach (byte b in bs.Value)
				Console.Write(Byte2String(b));

			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Console.WriteLine("\n" + Byte2String((byte)bs.ReadInt8()));

			bs.SeekBits(3, System.IO.SeekOrigin.Begin);

			Assert.AreEqual(0, bs.ReadBit());
			Assert.AreEqual(1, bs.ReadBit());
			Assert.AreEqual(1, bs.ReadBit());
			Assert.AreEqual(1, bs.ReadBit());
			Assert.AreEqual(1, bs.ReadBit());
			Assert.AreEqual(1, bs.ReadBit());
			Assert.AreEqual(1, bs.ReadBit());

			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(sbyte.MaxValue, bs.ReadInt8());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteInt16(short.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(short.MaxValue, bs.ReadInt16());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteInt32(67305985);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(67305985, bs.ReadInt32());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteInt32(Int32.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int32.MaxValue, bs.ReadInt32());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteInt64(Int64.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int64.MaxValue, bs.ReadInt64());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteUInt8(byte.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(byte.MaxValue, bs.ReadUInt8());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteUInt16(ushort.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(ushort.MaxValue, bs.ReadUInt16());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteUInt32(UInt32.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(UInt32.MaxValue, bs.ReadUInt32());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteUInt64(UInt64.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(UInt64.MaxValue, bs.ReadUInt64());


			//Min
			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteInt8(sbyte.MinValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(sbyte.MinValue, bs.ReadInt8());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteInt16(short.MinValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(short.MinValue, bs.ReadInt16());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteInt32(Int32.MinValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int32.MinValue, bs.ReadInt32());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteInt64(Int64.MinValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int64.MinValue, bs.ReadInt64());

			// BIG ENDIAN //////////////////////////////////////////

			bs = new BitStream();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.LittleEndian();

			//Max
			bs.WriteInt8(sbyte.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(sbyte.MaxValue, bs.ReadInt8());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteInt16(short.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(short.MaxValue, bs.ReadInt16());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteInt32(67305985);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(67305985, bs.ReadInt32());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteInt32(Int32.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int32.MaxValue, bs.ReadInt32());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteInt64(Int64.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int64.MaxValue, bs.ReadInt64());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteUInt8(byte.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(byte.MaxValue, bs.ReadUInt8());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteUInt16(ushort.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(ushort.MaxValue, bs.ReadUInt16());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteUInt32(UInt32.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(UInt32.MaxValue, bs.ReadUInt32());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteUInt64(UInt64.MaxValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(UInt64.MaxValue, bs.ReadUInt64());

			//Min
			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteInt8(sbyte.MinValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(sbyte.MinValue, bs.ReadInt8());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteInt16(short.MinValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(short.MinValue, bs.ReadInt16());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteInt32(Int32.MinValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int32.MinValue, bs.ReadInt32());

			bs.Clear();
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteInt64(Int64.MinValue);
			bs.SeekBits(3, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int64.MinValue, bs.ReadInt64());
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
			Assert.AreEqual(0, bs.LengthBytes);
			Assert.AreEqual(0, bs.TellBits());
			Assert.AreEqual(0, bs.TellBytes());
			Assert.AreEqual(0, bs.Stream.Length);
			Assert.AreEqual(0, bs.Stream.Position);

			bs.SeekBits(10, SeekOrigin.Begin);
			Assert.AreEqual(0, bs.LengthBits);
			Assert.AreEqual(0, bs.LengthBytes);
			Assert.AreEqual(10, bs.TellBits());
			Assert.AreEqual(1, bs.TellBytes());
			Assert.AreEqual(0, bs.Stream.Length);
			Assert.AreEqual(1, bs.Stream.Position);

			bs.WriteBit(1);
			Assert.AreEqual(11, bs.LengthBits);
			Assert.AreEqual(2, bs.LengthBytes);
			Assert.AreEqual(11, bs.TellBits());
			Assert.AreEqual(1, bs.TellBytes());
			Assert.AreEqual(2, bs.Stream.Length);
			Assert.AreEqual(2, bs.Stream.Position);
		}

		class BitStreamWriter<T> where T : IEndian, new()
		{
			// For computing bit shift offsets
			private static T offset = new T();

			// Stores the output
			public MemoryStream ms = new MemoryStream();

			// How many bits of the last byte are unset
			private int remain = 8;

			// Mask representing the set bits in the last byte
			private int mask = 0;

			// Length in bits
			private long length = 0;

			// Returns the next byte in the memory stream
			private byte NextByte
			{
				get
				{
					if (ms.Position == ms.Length)
						return 0;

					byte ret = (byte)ms.ReadByte();
					ms.Seek(-1, SeekOrigin.Current);
					return ret;
				}
			}

			public long LengthBits
			{
				get
				{
					return length;
				}
			}

			public MemoryStream Stream
			{
				get
				{
					return ms;
				}
			}

			public void Write(ulong value, int bits)
			{
				if (bits < 0 || bits > 64)
					throw new ArgumentOutOfRangeException();

				byte pending = NextByte;

				int written = 0;
				int todo = bits;

				while (todo > 0)
				{
					System.Diagnostics.Debug.Assert(written + todo == bits);

					if (todo < remain)
					{
						remain -= todo;
						todo = 0;

						byte lowMask = (byte)((1 << remain) - 1);
						mask |= lowMask;

						// LE: written, BE: zero
						int shift = offset.ShiftBy(written, todo);
						byte next = (byte)(value >> shift);
						next <<= remain;

						pending = (byte)((pending & mask) | (next & ~mask));
						ms.WriteByte(pending);
						ms.Seek(-1, SeekOrigin.Current);

						mask = ~lowMask;
					}
					else
					{
						todo -= remain;

						// LE: written, BE: todo
						int shift = offset.ShiftBy(written, todo);
						byte next = (byte)(value >> shift);

						pending = (byte)((pending & mask) | (next & ~mask));
						ms.WriteByte(pending);
						pending = NextByte;

						written += remain;
						remain = 8;
						mask = 0;
					}
				}

				length += bits;
			}

			public void Write(long value, int bits)
			{
				Write((ulong)value, bits);
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

			val = BitWriter<BigEndian>.GetBytes(0xABC, 0);
			Assert.AreEqual(new byte[] { }, val);
			Assert.AreEqual(0, BigBitWriter.GetUInt64(val, 0));
			Assert.AreEqual(0, BigBitWriter.GetInt64(val, 0));

			val = BitWriter<BigEndian>.GetBytes(0xABC, 12);
			Assert.AreEqual(new byte[] { 0xab, 0xc0 }, val);
			Assert.AreEqual(2748, BigBitWriter.GetUInt64(val, 12));
			Assert.AreEqual(-1348, BigBitWriter.GetInt64(val, 12));

			val = BitWriter<LittleEndian>.GetBytes(0xABC, 0);
			Assert.AreEqual(new byte[] { }, val);
			Assert.AreEqual(0, LittleBitWriter.GetUInt64(val, 0));
			Assert.AreEqual(0, LittleBitWriter.GetInt64(val, 0));

			val = BitWriter<LittleEndian>.GetBytes(0xABC, 12);
			Assert.AreEqual(new byte[] { 0xbc, 0xa0 }, val);
			Assert.AreEqual(2748, LittleBitWriter.GetUInt64(val, 12));
			Assert.AreEqual(-1348, LittleBitWriter.GetInt64(val, 12));

			ulong bits = 0;

			bits = BitWriter<BigEndian>.GetBits(0xABC, 12);
			Assert.AreEqual(0xABC, bits);
			Assert.AreEqual(2748, BigBitWriter.GetUInt64(bits, 12));
			Assert.AreEqual(-1348, BigBitWriter.GetInt64(bits, 12));

			bits = BitWriter<LittleEndian>.GetBits(0xABC, 12);
			Assert.AreEqual(0xBCA, bits);
			Assert.AreEqual(2748, LittleBitWriter.GetUInt64(bits, 12));
			Assert.AreEqual(-1348, LittleBitWriter.GetInt64(bits, 12));
		}

		[Test]
		public void BitStreamBits()
		{
			byte[] expected = new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89 };
			BitStream bs = new BitStream(expected);
			ulong test = bs.ReadBits(40);
			Assert.AreEqual(0x0123456789, test);

			bs = new BitStream();
			bs.WriteBits(test, 40);
			Assert.AreEqual(40, bs.LengthBits);
			MemoryStream ms = bs.Stream as MemoryStream;
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

			var w = new BitStreamWriter<BigEndian>();
			w.Write(0, 32);
			w.Write(0x00, 1);
			w.Write(0x01, 1);
			w.Write(0x00, 1);
			w.Write(-1, 5);
			w.Write(0x0f, 4);
			w.Write(0x123456789A, 40);

			Assert.AreEqual(84, w.LengthBits);
			Assert.AreEqual(11, w.Stream.Length);
			byte[] exp1 = new byte[] { 0x0, 0x0, 0x0, 0x0, 0x5f, 0xf1, 0x23, 0x45, 0x67, 0x89, 0xa0 };
			byte[] act1 = new byte[11];
			Buffer.BlockCopy(w.Stream.GetBuffer(), 0, act1, 0, 11);
			Assert.AreEqual(exp1, act1);

			var w1 = new BitStreamWriter<LittleEndian>();
			w1.Write(0x12345678, 32);
			w1.Write(0xabc, 12);

			Assert.AreEqual(44, w1.LengthBits);
			Assert.AreEqual(6, w1.Stream.Length);
			byte[] exp2 = new byte[] { 0x78, 0x56, 0x34, 0x12, 0xbc, 0xa0 };
			byte[] act2 = new byte[6];
			Buffer.BlockCopy(w1.Stream.GetBuffer(), 0, act2, 0, 6);
			Assert.AreEqual(exp2, act2);

			var w2 = new BitStreamWriter<LittleEndian>();
			w2.Write(1, 1);
			w2.Write(0, 1);
			w2.Write(0xffff, 6);

			Assert.AreEqual(8, w2.LengthBits);
			Assert.AreEqual(1, w2.Stream.Length);
			byte[] exp3 = new byte[] { 0xbf };
			byte[] act3 = new byte[1];
			Buffer.BlockCopy(w2.Stream.GetBuffer(), 0, act3, 0, 1);
			Assert.AreEqual(exp3, act3);
		}

		[Test]
		public void ReadBitStream()
		{
			var bs = new BitStream();
			bs.WriteBytes(new byte[] { 0x11, 0x27, 0x33, 0x44, 0x55 });
			bs.SeekBits(0, SeekOrigin.Begin);
			BitStream in1 = bs.ReadBitsAsBitStream(8 + 4);
			BitStream in2 = bs.ReadBitsAsBitStream(2);
			BitStream in3 = bs.ReadBitsAsBitStream(2 + 16 + 4);

			Assert.AreEqual(new byte[] { 0x11, 0x20 }, in1.Value);
			Assert.AreEqual(new byte[] { 0x40 }, in2.Value);
			Assert.AreEqual(new byte[] { 0xcc, 0xd1, 0x14 }, in3.Value);
		}

	}
}
