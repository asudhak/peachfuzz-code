using System;
using Peach.Core.IO;
using NUnit.Framework;
using System.Text;
using System.IO;

namespace Peach.Core.Test
{
	[TestFixture]
	public class BitwiseStreamTest
	{
		[Test]
		public void Position()
		{
			var bs = new BitwiseStream();

			Assert.AreEqual(0, bs.Length);
			Assert.AreEqual(0, bs.Position);
			Assert.AreEqual(0, bs.LengthBits);
			Assert.AreEqual(0, bs.PositionBits);
			Assert.AreEqual(0, bs.BaseStream.Length);
			Assert.AreEqual(0, bs.BaseStream.Position);

			bs.Position = 10;

			Assert.AreEqual(0, bs.Length);
			Assert.AreEqual(10, bs.Position);
			Assert.AreEqual(0, bs.LengthBits);
			Assert.AreEqual(80, bs.PositionBits);
			Assert.AreEqual(0, bs.BaseStream.Length);
			Assert.AreEqual(10, bs.BaseStream.Position);

			bs.PositionBits = 26;

			Assert.AreEqual(0, bs.Length);
			Assert.AreEqual(3, bs.Position);
			Assert.AreEqual(0, bs.LengthBits);
			Assert.AreEqual(26, bs.PositionBits);
			Assert.AreEqual(0, bs.BaseStream.Length);
			Assert.AreEqual(3, bs.BaseStream.Position);

			bs.SetLength(2);

			Assert.AreEqual(2, bs.Length);
			Assert.AreEqual(2, bs.Position);
			Assert.AreEqual(16, bs.LengthBits);
			Assert.AreEqual(16, bs.PositionBits);
			Assert.AreEqual(2, bs.BaseStream.Length);
			Assert.AreEqual(2, bs.BaseStream.Position);

			bs.Position = 10;

			Assert.AreEqual(2, bs.Length);
			Assert.AreEqual(10, bs.Position);
			Assert.AreEqual(16, bs.LengthBits);
			Assert.AreEqual(80, bs.PositionBits);
			Assert.AreEqual(2, bs.BaseStream.Length);
			Assert.AreEqual(10, bs.BaseStream.Position);
		}

		[Test]
		public void Seek()
		{
			var bs = new BitwiseStream();

			Assert.AreEqual(0, bs.Length);
			Assert.AreEqual(0, bs.Position);
			Assert.AreEqual(0, bs.LengthBits);
			Assert.AreEqual(0, bs.PositionBits);
			Assert.AreEqual(0, bs.BaseStream.Length);
			Assert.AreEqual(0, bs.BaseStream.Position);
		}

		[Test]
		public void WriteBits()
		{
			var bs = new BitwiseStream();

			bs.PositionBits = 2;
			bs.WriteBits(1, 1);
			bs.WriteBits(0, 1);
			bs.WriteBits(0x70f0f00a, 32);
			bs.WriteByte(0x22);
			bs.WriteBits(0xff, 4);
			bs.SeekBits(-4, System.IO.SeekOrigin.Current);
			bs.Write(new byte[] { 0, 0xab, 0xab, 0xab, 0xff }, 1, 3);
			bs.SeekBits(2, System.IO.SeekOrigin.Begin);
			bs.WriteBits(0, 1);
			bs.WriteBits(1, 1);

			ulong bits;

			int len = bs.ReadBits(out bits, 6);

			bs.SeekBits(-4, System.IO.SeekOrigin.End);
			len = bs.ReadBits(out bits, 8);

			bs.SeekBits(34, System.IO.SeekOrigin.Begin);

			int val = bs.ReadByte();

			bs.SeekBits(-1, System.IO.SeekOrigin.End);
			val = bs.ReadByte();
		}

		[Test]
		public void StringTest()
		{
			string test = "Hello World";
			byte[] buf = Encoding.UTF32.GetBytes(test);

			var bs = new BitwiseStream();
			bs.WriteBits(0x3, 2);
			bs.Write(buf, 0, buf.Length);
			bs.WriteBits(0x3, 2);

			Assert.AreEqual(buf.Length, bs.Length);
			Assert.AreEqual(buf.Length * 8 + 4, bs.LengthBits);
			Assert.AreEqual(bs.Length, bs.Position);
			Assert.AreEqual(bs.LengthBits, bs.PositionBits);
			Assert.AreEqual(bs.Position, bs.BaseStream.Position);
			Assert.AreEqual(bs.Length + 1, bs.BaseStream.Length);

			bs.SeekBits(2, System.IO.SeekOrigin.Begin);

			StreamReader rdr = new StreamReader(bs, Encoding.UTF32, false);
			string read = rdr.ReadToEnd();

			Assert.AreEqual(test, read);

			ulong extra;
			int remain = bs.ReadBits(out extra, 2);

			Assert.AreEqual(0x3, extra);
			Assert.AreEqual(2, remain);

			remain = bs.ReadBits(out extra, 1);
			Assert.AreEqual(0, remain);
			remain = bs.ReadByte();
			Assert.AreEqual(-1, remain);
		}
	}
}