using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;

using Enc = System.Text.Encoding;
using System.Globalization;

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
				Encoding.ASCII.GetBytes("\u08abX");
			});

			if (Platform.GetOS() == Platform.OS.Windows)
			{
				Assert.Throws<EncoderFallbackException>(delegate()
				{
					Encoding.Unicode.GetBytes("\ud860");
				});
			}
			else
			{
				Encoding.Unicode.GetBytes("\ud860");
			}

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
		public void TestHighSurrogate()
		{
			string high = char.ConvertFromUtf32(0x10ffff);
			Assert.AreEqual(2, high.Length);

			var it = StringInfo.GetTextElementEnumerator(high);
			int len = 0;
			while (it.MoveNext())
				++len;
			Assert.AreEqual(1, len);

			byte[] utf32 = System.Text.Encoding.UTF32.GetBytes(high);

			// Why is this different?
			if (Platform.GetOS() == Platform.OS.Windows)
				Assert.AreEqual(4, utf32.Length);
			else
				Assert.AreEqual(8, utf32.Length);

			byte[] utf16 = System.Text.Encoding.Unicode.GetBytes(high);
			Assert.AreEqual(4, utf16.Length);
		}

		[Test]
		public void EncodeBadAscii()
		{
			string str = "\xff";
			var bytes = Encoding.ASCII.GetRawBytes(str);
			var expected = new byte[] { 0xff };
			Assert.AreEqual(expected, bytes);
		}

		[Test]
		public void EncodeBadUtf32()
		{
			string str = "\ud860";
			var bytes = Encoding.UTF32.GetRawBytes(str);
			var expected = new byte[] { 0x60, 0xd8, 0, 0 };
			Assert.AreEqual(expected, bytes);

			// 0x64321
			str = "\uD950\uDF21";
			bytes = Encoding.UTF32.GetRawBytes(str);
			expected = new byte[] { 0x21, 0x43, 0x6, 0 };
			Assert.AreEqual(expected, bytes);
		}

		[Test]
		public void EncodeBadUtf16()
		{
			string str = "\ud860";
			var bytes = Encoding.Unicode.GetRawBytes(str);
			var expected = new byte[] { 0x60, 0xd8 };
			Assert.AreEqual(expected, bytes);

			str = "\uD950\uDF21";
			bytes = Encoding.Unicode.GetRawBytes(str);
			expected = new byte[] { 0x50, 0xd9, 0x21, 0xdf };
			Assert.AreEqual(expected, bytes);
		}

		[Test]
		public void EncodeBadUtf16BE()
		{
			string str = "\ud860";
			var bytes = Encoding.BigEndianUnicode.GetRawBytes(str);
			var expected = new byte[] { 0xd8, 0x60};
			Assert.AreEqual(expected, bytes);

			str = "\uD950\uDF21";
			bytes = Encoding.BigEndianUnicode.GetRawBytes(str);
			expected = new byte[] { 0xd9, 0x50, 0xdf, 0x21 };
			Assert.AreEqual(expected, bytes);
		}

		[Test]
		public void EncodeBadUtf8()
		{
			// 0xd860
			// 1110xxxx 10xxxxxx 10xxxxxx
			//     1101   100001   100000
			// 0xed     0xa1     0xa0
			string str = "\ud860";
			var bytes = Encoding.UTF8.GetRawBytes(str);
			var expected = new byte[] { 0xed, 0xa1, 0xa0 };
			Assert.AreEqual(expected, bytes);

			// 0x64321
			// 11110xxx 10xxxxxx 10xxxxxx 10xxxxxx
			//      001   100100   001100   100001
			// 0xf1     0xa4     0x8c     0xa1
			str = "\uD950\uDF21";
			bytes = Encoding.UTF8.GetRawBytes(str);
			expected = new byte[] { 0xf1, 0xa4, 0x8c, 0xa1 };
			Assert.AreEqual(expected, bytes);
		}

		[Test]
		public void EncodeBadUtf7()
		{
			//   0xd860
			//   1101 1000 0110 0000
			//   110110 000110 0000**
			//   0x36   0x06   0x00
			//   '2'    'G'    'A'
			string str = "\ud860";
			var bytes = Encoding.UTF7.GetRawBytes(str);
			var expected = new byte[] { (byte)'+', (byte)'2', (byte)'G', (byte)'A', (byte)'-' };
			Assert.AreEqual(expected, bytes);

			//   0xd950              0xdf21
			//   1101 1001 0101 0000 1101 1111 0010 0001
			//   110110 010101 000011 011111 001000 01****
			//   0x36   0x15   0x03   0x1f   0x08   0x10
			//   '2'    'V'    'D'    'f'    'I'    'Q'
			str = "\uD950\uDF21";
			bytes = Encoding.UTF7.GetRawBytes(str);
			expected = new byte[] { (byte)'+', (byte)'2', (byte)'V', (byte)'D', (byte)'f', (byte)'I', (byte)'Q', (byte)'-' };
			Assert.AreEqual(expected, bytes);
		}
	}
}
