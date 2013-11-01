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
			var bs = new BitStream();

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

			bs.SetLengthBits(20);

			Assert.AreEqual(2, bs.Length);
			Assert.AreEqual(2, bs.Position);
			Assert.AreEqual(20, bs.LengthBits);
			Assert.AreEqual(20, bs.PositionBits);
			Assert.AreEqual(3, bs.BaseStream.Length);
			Assert.AreEqual(2, bs.BaseStream.Position);

			try
			{
				bs.Position = -1;
				Assert.Fail("Should throw");
			}
			catch (ArgumentOutOfRangeException ex)
			{
				Assert.AreEqual("Non-negative number required." + Environment.NewLine + "Parameter name: value", ex.Message);
			}

			try
			{
				bs.PositionBits = -1;
				Assert.Fail("Should throw");
			}
			catch (ArgumentOutOfRangeException ex)
			{
				Assert.AreEqual("Non-negative number required." + Environment.NewLine + "Parameter name: value", ex.Message);
			}
		}

		[Test]
		public void CopyTo()
		{
			var bs1 = new BitStream();
			bs1.Write(Encoding.ASCII.GetBytes("Hello"), 0, 5);
			bs1.WriteBits(0x2, 4);
			bs1.SeekBits(0, SeekOrigin.Begin);

			Assert.AreEqual(5, bs1.Length);
			Assert.AreEqual(44, bs1.LengthBits);
			Assert.AreEqual(0, bs1.Position);
			Assert.AreEqual(0, bs1.PositionBits);

			var dst = new MemoryStream();
			bs1.CopyTo(dst);

			Assert.AreEqual(5, dst.Length);
			Assert.AreEqual(5, dst.Position);
			dst.Seek(0, SeekOrigin.Begin);
			var rdr = new StreamReader(dst);
			var final = rdr.ReadToEnd();

			Assert.AreEqual("Hello", final);
		}

		[Test]
		public void CopyToStream()
		{
			// Extra bits need to be copied off to a real stream
			var bs1 = new BitStream();
			bs1.Write(Encoding.ASCII.GetBytes("Hello"), 0, 5);
			bs1.WriteBits(0x3, 6);
			bs1.SeekBits(4, SeekOrigin.Begin);
		}

		[Test]
		public void TestSlice()
		{
			var ms = new MemoryStream();
			for (int i = 0; i < 256; ++i)
				ms.WriteByte((byte)i);
			ms.Seek(0, SeekOrigin.Begin);

			var bs1 = new BitStream(ms);
			Assert.AreEqual(0, bs1.Position);
			Assert.AreEqual(256, bs1.Length);

			bs1.Seek(10, SeekOrigin.Begin);
			Assert.AreEqual(10, ms.Position);

			var bs2 = bs1.SliceBits(800);

			Assert.AreEqual(110, bs1.Position);
			Assert.AreEqual(880, bs1.PositionBits);
			Assert.AreEqual(0, bs2.Position);
			Assert.AreEqual(0, bs2.PositionBits);
			Assert.AreEqual(100, bs2.Length);
			Assert.AreEqual(800, bs2.LengthBits);

			var bs3 = bs1.SliceBits(800);
			Assert.AreEqual(210, bs1.Position);
			Assert.AreEqual(1680, bs1.PositionBits);
			Assert.AreEqual(0, bs2.Position);
			Assert.AreEqual(0, bs2.PositionBits);
			Assert.AreEqual(0, bs3.Position);
			Assert.AreEqual(0, bs3.PositionBits);
			Assert.AreEqual(100, bs3.Length);
			Assert.AreEqual(800, bs3.LengthBits);

			for (int i = 0; i < 100; ++i)
			{
				int b2 = bs2.ReadByte();
				Assert.AreEqual(10 + i, b2);
				Assert.AreEqual(i + 1, bs2.Position);
				Assert.AreEqual(i, bs3.Position);
				Assert.AreEqual(210, bs1.Position);
				Assert.AreEqual(10 + i + 1, ms.Position);

				int b3 = bs3.ReadByte();
				Assert.AreEqual(110 + i, b3);
				Assert.AreEqual(i + 1, bs2.Position);
				Assert.AreEqual(i + 1, bs3.Position);
				Assert.AreEqual(210, bs1.Position);
				Assert.AreEqual(110 + i + 1, ms.Position);
			}
		}

		[Test]
		public void SetLength()
		{
			var bs = new BitStream();
			bs.SetLengthBits(21);
			Assert.AreEqual(0, bs.Position);
			Assert.AreEqual(0, bs.PositionBits);
			Assert.AreEqual(2, bs.Length);
			Assert.AreEqual(21, bs.LengthBits);

			bs.Seek(0, SeekOrigin.End);
			Assert.AreEqual(2, bs.Position);
			Assert.AreEqual(21, bs.PositionBits);

			bs.WriteBits(0x0f, 4);
			Assert.AreEqual(3, bs.Position);
			Assert.AreEqual(25, bs.PositionBits);
			Assert.AreEqual(3, bs.Length);
			Assert.AreEqual(25, bs.LengthBits);

			bs.SetLengthBits(17);
			Assert.AreEqual(2, bs.Position);
			Assert.AreEqual(17, bs.PositionBits);
			Assert.AreEqual(2, bs.Length);
			Assert.AreEqual(17, bs.LengthBits);
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
		public void BadRead()
		{
			var bs = new BitStream();
			bs.PositionBits = 21;
			Assert.AreEqual(2, bs.Position);
			Assert.AreEqual(21, bs.PositionBits);

			ulong tmp;
			int len = bs.ReadBits(out tmp, 64);
			Assert.AreEqual(0, len);
			Assert.AreEqual(0, bs.Length);
			Assert.AreEqual(0, bs.LengthBits);
			Assert.AreEqual(2, bs.Position);
			Assert.AreEqual(21, bs.PositionBits);

			var buf = new byte[10];
			len = bs.Read(buf, 0, buf.Length);
			Assert.AreEqual(0, len);
			Assert.AreEqual(0, bs.Length);
			Assert.AreEqual(0, bs.LengthBits);
			Assert.AreEqual(2, bs.Position);
			Assert.AreEqual(21, bs.PositionBits);
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
			bs.WriteBits(0x0f, 4);
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
			Assert.AreNotEqual(-1, val);

			bs.SeekBits(-1, System.IO.SeekOrigin.End);
			val = bs.ReadByte();

			bs = new BitStream();
		}

		[Test]
		public void ReadBits()
		{
			var bs = new BitStream();

			bs.WriteBits(1, 1);
			Assert.AreEqual(1, bs.LengthBits);
			Assert.AreEqual(0, bs.Length);
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
			Assert.AreEqual(0, ret);
			Assert.AreEqual(new byte[] { 0x00, 0x00 }, buf);
			Assert.AreEqual(1, bs.LengthBits);
			Assert.AreEqual(0, bs.Length);
			Assert.AreEqual(0, bs.PositionBits);
			Assert.AreEqual(0, bs.Position);

			bs.SeekBits(0, SeekOrigin.End);
			bs.WriteBits(0x3, 2);
			bs.SeekBits(-2, SeekOrigin.Current);
			Assert.AreEqual(3, bs.LengthBits);
			Assert.AreEqual(0, bs.Length);
			Assert.AreEqual(1, bs.PositionBits);
			Assert.AreEqual(0, bs.Position);
			bs.SetLengthBits(2);
			Assert.AreEqual(2, bs.LengthBits);
			Assert.AreEqual(0, bs.Length);
			Assert.AreEqual(1, bs.PositionBits);
			Assert.AreEqual(0, bs.Position);

			buf[0] = 0;
			ret = bs.Read(buf, 0, buf.Length);
			Assert.AreEqual(0, ret);
			Assert.AreEqual(new byte[] { 0x00, 0x00 }, buf);
			Assert.AreEqual(1, bs.PositionBits);
			Assert.AreEqual(0, bs.Position);

			bs.SeekBits(3, SeekOrigin.Begin);
			bs.WriteBits(0x3f, 6);
			Assert.AreEqual(9, bs.LengthBits);
			Assert.AreEqual(1, bs.Length);
			Assert.AreEqual(9, bs.PositionBits);
			Assert.AreEqual(1, bs.Position);
			bs.SeekBits(-6, SeekOrigin.Current);
			Assert.AreEqual(3, bs.PositionBits);
			Assert.AreEqual(0, bs.Position);

			buf[0] = 0;
			ret = bs.Read(buf, 0, buf.Length);
			Assert.AreEqual(0, ret);
			Assert.AreEqual(new byte[] { 0x00, 0x00 }, buf);
			Assert.AreEqual(3, bs.PositionBits);
			Assert.AreEqual(0, bs.Position);

			bs.SeekBits(1, SeekOrigin.Begin);
			Assert.AreEqual(1, bs.PositionBits);
			Assert.AreEqual(0, bs.Position);
			ret = bs.Read(buf, 0, buf.Length);
			Assert.AreEqual(1, ret);
			Assert.AreEqual(new byte[] { 0xff, 0x00 }, buf);
			Assert.AreEqual(9, bs.PositionBits);
			Assert.AreEqual(1, bs.Position);

		}

		[Test]
		public void StringTest()
		{
			string test = "Hello World";
			byte[] buf = Encoding.UTF32.GetBytes(test);

			var bs = new BitStream();
			bs.WriteBits(0x1, 2);
			bs.Write(buf, 0, buf.Length);
			bs.WriteBits(0x1, 2);

			Assert.AreEqual(buf.Length, bs.Length);
			Assert.AreEqual(buf.Length * 8 + 4, bs.LengthBits);
			Assert.AreEqual(bs.Length, bs.Position);
			Assert.AreEqual(bs.LengthBits, bs.PositionBits);

			bs.SeekBits(2, System.IO.SeekOrigin.Begin);

			var dest = new byte[50];
			int len = bs.Read(dest, 0, dest.Length);
			Assert.AreEqual(44, len);

			bs.SeekBits(2, System.IO.SeekOrigin.Begin);
			StreamReader rdr = new StreamReader(bs, System.Text.Encoding.UTF32, false);
			string read = rdr.ReadToEnd();

			Assert.AreEqual(test, read);

			ulong extra;
			int remain = bs.ReadBits(out extra, 2);

			Assert.AreEqual(0x1, extra);
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

			ms.Write(buf, 0, buf.Length);
			Assert.AreEqual(15, ms.Position);
			Assert.AreEqual(15, ms.Length);

			var bs = new BitStream();
			bs.PositionBits = 4;
			ret = bs.Read(buf, 0, buf.Length);
			Assert.AreEqual(0, ret);
			bs.Write(new byte[] { 0xab, 0xcd, 0xef }, 0, 3);
			Assert.AreEqual(3, bs.Position);
			Assert.AreEqual(3, bs.Length);
			Assert.AreEqual(28, bs.PositionBits);
			Assert.AreEqual(28, bs.LengthBits);
			bs.WriteBits(0xf, 4);

			bs.PositionBits = 0;
			ret = bs.Read(buf, 0, buf.Length);
			Assert.AreEqual(4, ret);
			Assert.AreEqual(0x0a, buf[0]);
			Assert.AreEqual(0xbc, buf[1]);
			Assert.AreEqual(0xde, buf[2]);
			Assert.AreEqual(0xff, buf[3]);
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
			Assert.AreEqual(3, len); // 2bit + 7bit + 18bit = 27bit = 3 bytes
			Assert.AreEqual(37, lst.LengthBits);
			Assert.AreEqual(34, lst.PositionBits);
			Assert.AreEqual(4, lst.Length);
			Assert.AreEqual(4, lst.Position);

			// 00 00 10 10 10 10 11 10 11 10 00 00 00 00 10 00 01 01 0
			//                10 11 10 11 10 00 00 00 00 10 00 01 
			//                0xbb        0x80        0x21
			Assert.AreEqual(new byte[] { 0xbb, 0x80, 0x21, 0x00 }, buf);

			lst.SeekBits(0, SeekOrigin.Begin);
			int bits = (int)lst[0].LengthBits + 1;
			ulong tmp;
			int cnt = lst.ReadBits(out tmp, bits);

			Assert.AreEqual(bits, cnt);
		}

		[Test]
		public void TestFind()
		{
			var str = Bits.Fmt("{0}", "aabcd");
			var token = Bits.Fmt("{0}", "abc");

			var idx = str.IndexOf(token, 0);
			Assert.AreEqual(idx, 8);
		}

	}
}