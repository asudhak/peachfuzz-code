using System;
using Peach.Core.IO.New;
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
			var bs = new BitStream();

			Assert.AreEqual(0, bs.Length);
			Assert.AreEqual(0, bs.Position);
			Assert.AreEqual(0, bs.LengthBits);
			Assert.AreEqual(0, bs.PositionBits);

			bs.Position = 10;

			Assert.AreEqual(0, bs.Length);
			Assert.AreEqual(10, bs.Position);
			Assert.AreEqual(0, bs.LengthBits);
			Assert.AreEqual(80, bs.PositionBits);

			bs.PositionBits = 26;

			Assert.AreEqual(0, bs.Length);
			Assert.AreEqual(3, bs.Position);
			Assert.AreEqual(0, bs.LengthBits);
			Assert.AreEqual(26, bs.PositionBits);

			bs.SetLength(2);

			Assert.AreEqual(2, bs.Length);
			Assert.AreEqual(2, bs.Position);
			Assert.AreEqual(16, bs.LengthBits);
			Assert.AreEqual(16, bs.PositionBits);

			bs.Position = 10;

			Assert.AreEqual(2, bs.Length);
			Assert.AreEqual(10, bs.Position);
			Assert.AreEqual(16, bs.LengthBits);
			Assert.AreEqual(80, bs.PositionBits);

			try
			{
				bs.Position = -1;
				Assert.Fail("Should throw");
			}
			catch (ArgumentOutOfRangeException ex)
			{
				Assert.AreEqual(ex.Message, "Non-negative number required.\r\nParameter name: value");
			}

			try
			{
				bs.PositionBits = -1;
				Assert.Fail("Should throw");
			}
			catch (ArgumentOutOfRangeException ex)
			{
				Assert.AreEqual(ex.Message, "Non-negative number required.\r\nParameter name: value");
			}
		}

		[Test]
		public void Seek()
		{
			var bs = new BitStream();

			Assert.AreEqual(0, bs.Length);
			Assert.AreEqual(0, bs.Position);
			Assert.AreEqual(0, bs.LengthBits);
			Assert.AreEqual(0, bs.PositionBits);

			long ret = bs.Seek(10, SeekOrigin.Begin);
			Assert.AreEqual(10, ret);
			Assert.AreEqual(10, bs.Position);
			Assert.AreEqual(80, bs.PositionBits);
			Assert.AreEqual(0, bs.Length);
			Assert.AreEqual(0, bs.LengthBits);

			ret = bs.SeekBits(12, SeekOrigin.Current);
			Assert.AreEqual(92, ret);
			Assert.AreEqual(11, bs.Position);
			Assert.AreEqual(92, bs.PositionBits);
			Assert.AreEqual(0, bs.Length);
			Assert.AreEqual(0, bs.LengthBits);

			ret = bs.Seek(1, SeekOrigin.Current);
			Assert.AreEqual(12, ret);
			Assert.AreEqual(12, bs.Position);
			Assert.AreEqual(100, bs.PositionBits);
			Assert.AreEqual(0, bs.Length);
			Assert.AreEqual(0, bs.LengthBits);

			try
			{
				bs.Seek(-1, SeekOrigin.Begin);
				Assert.Fail("Should throw");
			}
			catch (IOException ex)
			{
				Assert.AreEqual(ex.Message, "An attempt was made to move the position before the beginning of the stream.");
			}

			try
			{
				bs.Seek(-1, SeekOrigin.Begin);
				Assert.Fail("Should throw");
			}
			catch (IOException ex)
			{
				Assert.AreEqual(ex.Message, "An attempt was made to move the position before the beginning of the stream.");
			}
		}

		[Test]
		public void WriteBits()
		{
			var bs = new BitStream();

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
			Assert.AreEqual(6, len);

			bs.SeekBits(-4, System.IO.SeekOrigin.End);
			len = bs.ReadBits(out bits, 8);
			Assert.AreEqual(4, len);

			bs.SeekBits(34, System.IO.SeekOrigin.Begin);

			int val = bs.ReadByte();

			bs.SeekBits(-1, System.IO.SeekOrigin.End);
			val = bs.ReadByte();
		}

		[Test]
		public void ReadBits()
		{
			var bs = new BitStream();

			bs.WriteBits(1, 1);
			Assert.AreEqual(1, bs.LengthBits);
			Assert.AreEqual(1, bs.Length);
			Assert.AreEqual(0, bs.Position);
			Assert.AreEqual(1, bs.PositionBits);

			bs.Position = 0;
			Assert.AreEqual(0, bs.Position);
			Assert.AreEqual(0, bs.PositionBits);

			ulong bits;
			int ret = bs.ReadBits(out bits, 3);
			Assert.AreEqual(1, ret);
			Assert.AreEqual(1, bits);
			Assert.AreEqual(1, bs.PositionBits);
			Assert.AreEqual(0, bs.Position);

			bs.Position = 0;
			Assert.AreEqual(0, bs.Position);
			Assert.AreEqual(0, bs.PositionBits);

			byte[] buf = new byte[2];
			ret = bs.Read(buf, 0, buf.Length);
			Assert.AreEqual(1, ret);
			Assert.AreEqual(new byte[] { 0x80, 0x00 }, buf);
			Assert.AreEqual(1, bs.PositionBits);
			Assert.AreEqual(0, bs.Position);

			bs.WriteBits(0x3, 2);
			bs.SeekBits(-2, SeekOrigin.Current);
			Assert.AreEqual(3, bs.LengthBits);
			Assert.AreEqual(1, bs.Length);
			Assert.AreEqual(1, bs.PositionBits);
			Assert.AreEqual(0, bs.Position);
			bs.SetLengthBits(2);
			Assert.AreEqual(2, bs.LengthBits);
			Assert.AreEqual(1, bs.Length);
			Assert.AreEqual(1, bs.PositionBits);
			Assert.AreEqual(0, bs.Position);

			buf[0] = 0;
			ret = bs.Read(buf, 0, buf.Length);
			Assert.AreEqual(1, ret);
			Assert.AreEqual(new byte[] { 0x80, 0x00 }, buf);
			Assert.AreEqual(2, bs.PositionBits);
			Assert.AreEqual(0, bs.Position);

			bs.WriteBits(0xff, 6);
			Assert.AreEqual(8, bs.LengthBits);
			Assert.AreEqual(1, bs.Length);
			Assert.AreEqual(8, bs.PositionBits);
			Assert.AreEqual(1, bs.Position);
			bs.SeekBits(-6, SeekOrigin.Current);
			Assert.AreEqual(2, bs.PositionBits);
			Assert.AreEqual(0, bs.Position);

			buf[0] = 0;
			ret = bs.Read(buf, 0, buf.Length);
			Assert.AreEqual(1, ret);
			Assert.AreEqual(new byte[] { 0xfc, 0x00 }, buf);
			Assert.AreEqual(8, bs.PositionBits);
			Assert.AreEqual(1, bs.Position);
		}

		[Test]
		public void StringTest()
		{
			string test = "Hello World";
			byte[] buf = Encoding.UTF32.GetBytes(test);

			var bs = new BitStream();
			bs.WriteBits(0x3, 2);
			bs.Write(buf, 0, buf.Length);
			bs.WriteBits(0x3, 2);

			Assert.AreEqual(buf.Length + 1, bs.Length);
			Assert.AreEqual(buf.Length * 8 + 4, bs.LengthBits);
			Assert.AreEqual(bs.Length - 1, bs.Position);
			Assert.AreEqual(bs.LengthBits, bs.PositionBits);

			bs.SeekBits(2, System.IO.SeekOrigin.Begin);

			StreamReader rdr = new StreamReader(bs, System.Text.Encoding.UTF32, false);
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

		[Test]
		public void TestBadWrite()
		{
			MemoryStream ms = new MemoryStream();
			ms.Position = 5;
			var buf = new byte[10];
			int ret = ms.Read(buf, 0, buf.Length);
			Assert.AreEqual(0, ret);
		}

		[Test]
		public void TestList()
		{
			BitStreamList lst = new BitStreamList();
			lst.Add(new BitStream(new MemoryStream(Encoding.ASCII.GetBytes("Hello"))));
			lst.Add(new BitStream(new MemoryStream(Encoding.ASCII.GetBytes("World"))));

			Assert.AreEqual(0, lst.Position);
			Assert.AreEqual(0, lst.PositionBits);

			Assert.AreEqual(10, lst.Length);
			Assert.AreEqual(80, lst.LengthBits);

			long ret = lst.SeekBits(12, SeekOrigin.Begin);
			Assert.AreEqual(12, ret);
			Assert.AreEqual(1, lst.Position);
			Assert.AreEqual(12, lst.PositionBits);

			ret = lst.Seek(2, SeekOrigin.Current);
			Assert.AreEqual(3, ret);
			Assert.AreEqual(3, lst.Position);
			Assert.AreEqual(28, lst.PositionBits);

			ret = lst.SeekBits(-12, SeekOrigin.End);
			Assert.AreEqual(68, ret);
			Assert.AreEqual(8, lst.Position);
			Assert.AreEqual(68, lst.PositionBits);

			ulong bits;
			int b = lst.ReadBits(out bits, 15);
			Assert.AreEqual(12, b);
			Assert.AreEqual(0xc64, bits);
			Assert.AreEqual(lst.Length, lst.Position);
			Assert.AreEqual(lst.LengthBits, lst.PositionBits);

			lst.Add(new BitStream(new MemoryStream(Encoding.ASCII.GetBytes("!"))));
			Assert.AreEqual(10, lst.Position);
			Assert.AreEqual(80, lst.PositionBits);
			Assert.AreEqual(11, lst.Length);
			Assert.AreEqual(88, lst.LengthBits);

			ret = lst.SeekBits(-12, SeekOrigin.End);
			Assert.AreEqual(76, ret);
			Assert.AreEqual(9, lst.Position);
			Assert.AreEqual(76, lst.PositionBits);

			b = lst.ReadBits(out bits, 15);
			Assert.AreEqual(12, b);
			Assert.AreEqual(lst.Length, lst.Position);
			Assert.AreEqual(lst.LengthBits, lst.PositionBits);
			Assert.AreEqual(0x421, bits);

			var buf = new byte[3];
			b = lst.Read(buf, 0, buf.Length);
			Assert.AreEqual(0, b);
			Assert.AreEqual(lst.Length, lst.Position);
			Assert.AreEqual(lst.LengthBits, lst.PositionBits);

			lst.Seek(-3, SeekOrigin.End);
			b = lst.Read(buf, 0, buf.Length);
			Assert.AreEqual(3, b);
			Assert.AreEqual(Encoding.ASCII.GetBytes("ld!"), buf);
			Assert.AreEqual(lst.Length, lst.Position);
			Assert.AreEqual(lst.LengthBits, lst.PositionBits);
		}

		[Test]
		public void TestUnalignedRead()
		{
			BitStreamList lst = new BitStreamList();
			var bs1 = new BitStream();
			bs1.WriteBits(0xaa, 12);
			lst.Add(bs1);
			var bs2a = new BitStream();
			bs2a.WriteBits(0x7, 3);
			lst.Add(bs2a);
			var bs2b = new BitStream();
			bs2b.WriteBits(0x7, 4);
			lst.Add(bs2b);
			var bs3 = new BitStream();
			bs3.WriteBits(0x10a, 18);
			lst.Add(bs3);

			lst.SeekBits(10, SeekOrigin.Begin);
			Assert.AreEqual(37, lst.LengthBits);

			var buf = new byte[4];
			int len = lst.Read(buf, 0, buf.Length);
			Assert.AreEqual(4, len); // 2bit + 7bit + 18bit = 27bit = 4 bytes
			Assert.AreEqual(37, lst.LengthBits);
			Assert.AreEqual(37, lst.PositionBits);
			Assert.AreEqual(5, lst.Length);
			Assert.AreEqual(4, lst.Position);

			// 00 00 10 10 10 10 11 10 11 10 00 00 00 00 10 00 01 01 0
			//                10 11 10 11 10 00 00 00 00 10 00 01 01 00 00 00
			//                0xbb        0x80        0x21        0x40
			Assert.AreEqual(new byte[] { 0xbb, 0x80, 0x21, 0x40 }, buf);
		}
	}
}