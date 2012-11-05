
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
			Assert.AreEqual(1, bs.Stream.Position);


		}
	}
}
