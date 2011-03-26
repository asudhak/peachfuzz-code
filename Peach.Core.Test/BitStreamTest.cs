using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core;

namespace Peach.Core.Test
{
	[TestFixture]
	public class BitStreamTest
	{
		[Test]
		public void ReadWriteBits()
		{
			BitStream bs = new BitStream();
			bs.BigEndian();

			bs.WriteBits(0, 1);
			bs.WriteBits(0, 1);
			bs.WriteBits(0, 1);
			bs.WriteBits(1, 1);
			bs.WriteBits(0, 1);
			bs.WriteBits(1, 1);

			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(0, bs.ReadBits(1));
			Assert.AreEqual(0, bs.ReadBits(1));
			Assert.AreEqual(0, bs.ReadBits(1));
			Assert.AreEqual(1, bs.ReadBits(1));
			Assert.AreEqual(0, bs.ReadBits(1));
			Assert.AreEqual(1, bs.ReadBits(1));

			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual('(', (char)bs.ReadUInt8());
		}

		[Test]
		public void ReadWriteNumbers()
		{
			BitStream bs = new BitStream();

			//Max
			bs.WriteInt8(sbyte.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(sbyte.MaxValue, bs.ReadInt8());

			bs.WriteInt16(short.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(short.MaxValue, bs.ReadInt16());

			bs.WriteInt32(Int32.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int32.MaxValue, bs.ReadInt32());

			bs.WriteInt64(Int64.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int64.MaxValue, bs.ReadInt64());

			bs.WriteUInt8(byte.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(byte.MaxValue, bs.ReadUInt8());

			bs.WriteUInt16(ushort.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(ushort.MaxValue, bs.ReadUInt16());

			bs.WriteUInt32(UInt32.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(UInt32.MaxValue, bs.ReadUInt32());

			bs.WriteUInt64(UInt64.MaxValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(UInt64.MaxValue, bs.ReadUInt64());


			//Min
			bs.WriteInt8(sbyte.MinValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(sbyte.MinValue, bs.ReadInt8());

			bs.WriteInt16(short.MinValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(short.MinValue, bs.ReadInt16());

			bs.WriteInt32(Int32.MinValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int32.MinValue, bs.ReadInt32());

			bs.WriteInt64(Int64.MinValue);
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.AreEqual(Int64.MinValue, bs.ReadInt64());
		}
	}
}
