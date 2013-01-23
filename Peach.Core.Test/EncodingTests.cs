using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;

using Enc = System.Text.Encoding;

namespace Peach.Core.Test
{
	[TestFixture]
	class EncodingTests
	{
		static byte[] AppendByte(byte[] buf, int val = -1)
		{
			var list = buf.ToList();
			if (val == -1)
				list.Add(buf[0]);
			else
				list.Add((byte)val);
			return list.ToArray();
		}

		static Enc latin1 = Enc.GetEncoding(1252);

		[Test]
		public void TestBaseEncodings()
		{
			Assert.True(Enc.ASCII.IsSingleByte);
			Assert.True(latin1.IsSingleByte);
			Assert.False(Enc.BigEndianUnicode.IsSingleByte);
			Assert.False(Enc.Unicode.IsSingleByte);
			Assert.False(Enc.UTF7.IsSingleByte);
			Assert.False(Enc.UTF8.IsSingleByte);
			Assert.False(Enc.UTF32.IsSingleByte);

			// Why???
			if (Platform.GetOS() == Platform.OS.Windows)
			{
				Assert.AreEqual(2, Enc.ASCII.GetMaxByteCount(1));
				Assert.AreEqual(2, latin1.GetMaxByteCount(1));
				Assert.AreEqual(4, Enc.BigEndianUnicode.GetMaxByteCount(1));
				Assert.AreEqual(4, Enc.Unicode.GetMaxByteCount(1));
				Assert.AreEqual(5, Enc.UTF7.GetMaxByteCount(1));
				Assert.AreEqual(6, Enc.UTF8.GetMaxByteCount(1));
				Assert.AreEqual(8, Enc.UTF32.GetMaxByteCount(1));
			}
			else
			{
				Assert.AreEqual(1, Enc.ASCII.GetMaxByteCount(1));
				Assert.AreEqual(1, latin1.GetMaxByteCount(1));
				Assert.AreEqual(2, Enc.BigEndianUnicode.GetMaxByteCount(1));
				Assert.AreEqual(2, Enc.Unicode.GetMaxByteCount(1));
				Assert.AreEqual(5, Enc.UTF7.GetMaxByteCount(1));
				Assert.AreEqual(4, Enc.UTF8.GetMaxByteCount(1));
				Assert.AreEqual(4, Enc.UTF32.GetMaxByteCount(1));
			}
		}

		[Test]
		public void TestConvert()
		{
			Assert.Throws<EncoderFallbackException>(delegate()
			{
				Encoding.ASCII.GetBytes("\u00abX");
			});

			Assert.Throws<EncoderFallbackException>(delegate()
			{
				Encoding.ASCII.GetBytes("\x80");
			});

			var bufD = Encoding.ISOLatin1.GetBytes("\x80");
			Assert.AreEqual(1, bufD.Length);
			Assert.AreEqual(0x80, bufD[0]);

			var buf = Encoding.ASCII.GetBytes("Hello");
			Assert.AreEqual(5, buf.Length);
			var buf16 = Encoding.Unicode.GetBytes("\u00abX");
			Assert.AreEqual(4, buf16.Length);
			var buf16be = Encoding.BigEndianUnicode.GetBytes("\u00abX");
			Assert.AreEqual(4, buf16be.Length);
			var buf32 = Encoding.UTF32.GetBytes("\u00abX");
			Assert.AreEqual(8, buf32.Length);
			var buf8 = Encoding.UTF8.GetBytes("\u00abX");
			Assert.AreEqual(3, buf8.Length);
			var buf7 = Encoding.UTF7.GetBytes("\u00abX");
			Assert.AreEqual(6, buf7.Length);

			string str;

			str = Encoding.ASCII.GetString(buf);
			Assert.AreEqual("Hello", str);
			str = Encoding.ISOLatin1.GetString(bufD);
			Assert.AreEqual("\x80", str);
			str = Encoding.Unicode.GetString(buf16);
			Assert.AreEqual("\u00abX", str);
			str = Encoding.BigEndianUnicode.GetString(buf16be);
			Assert.AreEqual("\u00abX", str);
			str = Encoding.UTF8.GetString(buf8);
			Assert.AreEqual("\u00abX", str);
			str = Encoding.UTF7.GetString(buf7);
			Assert.AreEqual("\u00abX", str);
			str = Encoding.UTF32.GetString(buf32);
			Assert.AreEqual("\u00abX", str);

			Assert.Throws<DecoderFallbackException>(delegate()
			{
				Encoding.ASCII.GetString(AppendByte(buf, 0xff));
			});

			Assert.Throws<DecoderFallbackException>(delegate()
			{
				Encoding.Unicode.GetString(AppendByte(buf16));
			});

			Assert.Throws<DecoderFallbackException>(delegate()
			{
				Encoding.BigEndianUnicode.GetString(AppendByte(buf16be));
			});

			Assert.Throws<DecoderFallbackException>(delegate()
			{
				Encoding.UTF32.GetString(AppendByte(buf32));
			});

			Assert.Throws<DecoderFallbackException>(delegate()
			{
				Encoding.UTF8.GetString(AppendByte(buf8));
			});

			Assert.Throws<DecoderFallbackException>(delegate()
			{
				Encoding.UTF7.GetString(AppendByte(buf7));
			});
		}

		[Test]
		public void PartialUTF7()
		{
			var buf7 = Encoding.UTF7.GetBytes("\u00abX");

			var dec = Encoding.UTF7.GetDecoder();
			char[] chars = new char[100];

			int conv1 = dec.GetChars(buf7, 0, buf7.Length, chars, 0);
			int conv2 = dec.GetChars(buf7, 0, 1, chars, conv1);
			int conv3 = dec.GetChars(buf7, 1, buf7.Length - 1, chars, conv1 + conv2);

			string str = new string(chars, 0, conv1 + conv2 + conv3);

			Assert.AreEqual("\u00abX\u00abX", str);

		}

		[Test]
		[Ignore]
		public void EncodeBadUtf32()
		{
			string bad = "\ud860";
			int val = (int)bad[0];
			Assert.AreEqual(val, 0xd860);
			var bytes = Encoding.UTF32.GetBytes(bad);
			var expected = new byte[] { 0x60, 0xd8, 0, 0 };
			Assert.AreEqual(expected, bytes);
		}

		[Test]
		[Ignore]
		public void EncodeBadUtf16()
		{
			string bad = "\ud860";
			int val = (int)bad[0];
			Assert.AreEqual(val, 0xd860);
			var bytes = Encoding.Unicode.GetBytes(bad);
			var expected = new byte[] { 0x60, 0xd8 };
			Assert.AreEqual(expected, bytes);
		}

		[Test]
		[Ignore]
		public void EncodeBadUtf16BE()
		{
			string bad = "\ud860";
			int val = (int)bad[0];
			Assert.AreEqual(val, 0xd860);
			var bytes = Encoding.BigEndianUnicode.GetBytes(bad);
			var expected = new byte[] { 0xd8, 0x60 };
			Assert.AreEqual(expected, bytes);
		}

		[Test]
		[Ignore]
		public void EncodeBadUtf8()
		{
			// 0xd860
			// 1110xxxx 10xxxxxx 10xxxxxx
			//     1101   100001   100000
			// 0xed     0xa1     0xa0
			string bad = "\ud860";
			int val = (int)bad[0];
			Assert.AreEqual(val, 0xd860);
			var bytes = Encoding.UTF8.GetBytes(bad);
			var expected = new byte[] { 0xed, 0xa1, 0xa0 };
			Assert.AreEqual(expected, bytes);
		}
	}
}
