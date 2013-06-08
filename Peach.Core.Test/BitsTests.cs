using System;
using System.IO;
using System.Text.RegularExpressions;
using Peach.Core;
using Peach.Core.IO;
using NUnit.Framework;

namespace Peach
{
	public static class Bits
	{
		#region BitStreamFormatInfo

		private class BitStreamFormatInfo : IFormatProvider, ICustomFormatter
		{
			private BitStream bs;

			public BitStreamFormatInfo(BitStream bs)
			{
				if (bs == null)
					throw new ArgumentNullException("bs");

				this.bs = bs;
			}

			public object GetFormat(Type formatType)
			{
				if (typeof(ICustomFormatter).Equals(formatType))
					return this;

				return null;
			}

			public string Format(string format, object arg, IFormatProvider formatProvider)
			{
				if (arg == null)
					throw new ArgumentNullException("arg");

				if (arg is byte[])
				{
					if (format != null)
						throw new FormatException("Format not allowed for byte[] arguments.");

					var buf = (byte[])arg;
					bs.Write(buf, 0, buf.Length);
					return "";
				}

				if (arg is string)
				{
					if (string.IsNullOrEmpty(format))
						format = "ascii";

					try
					{
						var enc = Encoding.GetEncoding(format);
						var buf = enc.GetBytes((string)arg);
						bs.Write(buf, 0, buf.Length);
						return "";
					}
					catch (ArgumentException ex)
					{
						throw new FormatException(ex.Message, ex);
					}
				}

				if (string.IsNullOrEmpty(format))
					format = "";

				// Valid format is Lxxx or Bxxx where L/B is little/big and xxx is the bit length
				var re = new Regex(@"^([lb]?)(\d*)$");
				var m = re.Match(format.ToLower().Trim());
				if (!m.Success)
					throw new FormatException("Invalid format specified.");

				int bitlen;
				if (!string.IsNullOrEmpty(m.Groups[2].Value))
				{
					bitlen = int.Parse(m.Groups[2].Value);
				}
				else
				{
					switch (Type.GetTypeCode(arg.GetType()))
					{
						case TypeCode.SByte:
						case TypeCode.Byte:
							bitlen = 8;
							break;
						case TypeCode.Int16:
						case TypeCode.UInt16:
							bitlen = 16;
							break;
						case TypeCode.Int32:
						case TypeCode.UInt32:
							bitlen = 32;
							break;
						case TypeCode.Int64:
						case TypeCode.UInt64:
							bitlen = 64;
							break;

						default:
							throw new FormatException("Only string and numeric types are formattable.");
					}
				}

				Endian endian = m.Groups[1].Value == "b" ? Endian.Big : Endian.Little;
				bool signed;


				switch (Type.GetTypeCode(arg.GetType()))
				{
					case TypeCode.Byte:
					case TypeCode.UInt16:
					case TypeCode.UInt32:
					case TypeCode.UInt64:
						signed = false;
						break;

					case TypeCode.SByte:
					case TypeCode.Int16:
					case TypeCode.Int32:
					case TypeCode.Int64:
						signed = true;
						break;

					default:
						throw new FormatException("Only string and numeric types are formattable.");
				}

				ulong bits;

				if (signed)
				{
					ulong max = (ulong)((ulong)1 << ((int)bitlen - 1)) - 1;
					long min = 0 - (long)((ulong)1 << ((int)bitlen - 1));
					long value = Convert.ToInt64(arg);

					if (value < min || (ulong)value > max)
						throw new FormatException("Number overflows bitlen.");

					bits = endian.GetBits(value, bitlen);
				}
				else
				{
					ulong max = (ulong)((ulong)1 << ((int)bitlen - 1));
					max += (max - 1);
					ulong value = Convert.ToUInt64(arg);

					if (value > max)
						throw new FormatException("Number overflows bitlen.");

					bits = endian.GetBits(value, bitlen);
				}

				bs.WriteBits(bits, bitlen);

				return "";
			}
		}

		#endregion

		public static byte[] ToArray(this BitStream bs)
		{
			var ms = bs.BaseStream as MemoryStream;
			Assert.NotNull(ms);
			return ms.ToArray();
		}

		/// <summary>
		/// Formats a BitStream according to the format string. Supported
		/// arguments are byte[], numbers and strings.
		/// 
		/// Valid formatting options are:
		/// 
		/// byte[] - none
		/// string - ascii,utf16,utf16be,utf8,utf7,utf32 (defaults is ascii)
		/// numers - [L,B][bitlen]
		/// 
		/// If no formatting options are given, defaults to little endian
		/// and the size of the argument.
		/// </summary>
		/// <param name="fmt">Format string</param>
		/// <param name="args">Objects to format</param>
		/// <returns></returns>
		public static BitStream Fmt(string fmt, params object[] args)
		{
			BitStream bs = new BitStream();
			string.Format(new BitStreamFormatInfo(bs), fmt, args);
			bs.SeekBits(0, SeekOrigin.Begin);
			return bs;
		}
	}

	[TestFixture]
	class BitsTests
	{
		[Test]
		public void TestFmt()
		{
			var testBuf = new byte[] { 1, 2, 3, 4, 5 };
			var testStr = "Test";
			var testStrAscii = Encoding.ASCII.GetBytes(testStr);
			var testStrUtf8 = Encoding.UTF8.GetBytes(testStr);
			var testStrUtf16 = Encoding.Unicode.GetBytes(testStr);
			var testStrUtf16be = Encoding.BigEndianUnicode.GetBytes(testStr);
			var testStrUtf32 = Encoding.UTF32.GetBytes(testStr);
			var testStrUtf7 = Encoding.UTF7.GetBytes(testStr);

			// Little, Big and Width
			Assert.AreEqual(new byte[] { 0xb8, 0x0b }, Bits.Fmt("{0:L16}", 0xbb8).ToArray());
			Assert.AreEqual(new byte[] { 0x0b, 0xb8 }, Bits.Fmt("{0:B16}", 0xbb8).ToArray());
			Assert.AreEqual(new byte[] { 0xf0 }, Bits.Fmt("{0:L4}", (uint)0xf).ToArray());

			Assert.Throws<FormatException>(delegate()
			{
				Bits.Fmt("{0:L8}", 0x100);
			});

			Assert.Throws<FormatException>(delegate()
			{
				Bits.Fmt("{0:L4}", 0x10);
			});


			// Defaults to Little
			Assert.AreEqual(new byte[] { 0xb8, 0x0b }, Bits.Fmt("{0:16}", 0xbb8).ToArray());

			// Defaults to size of param
			Assert.AreEqual(new byte[] { 0x0b, 0xb8 }, Bits.Fmt("{0:B}", (short)0xbb8).ToArray());

			// Defaults to little and size of param
			Assert.AreEqual(new byte[] { 0xb8, 0x0b, 0x00, 0x00 }, Bits.Fmt("{0}", 0xbb8).ToArray());
			Assert.AreEqual(new byte[] { 0xb8, 0x0b }, Bits.Fmt("{0}", (short)0xbb8).ToArray());
			Assert.AreEqual(new byte[] { 0xb8 }, Bits.Fmt("{0}", (byte)0xb8).ToArray());

			// Defaults to ascii for string
			Assert.AreEqual(testStrAscii, Bits.Fmt("{0}", testStr).ToArray());

			// String formats
			Assert.AreEqual(testStrAscii, Bits.Fmt("{0:ascii}", testStr).ToArray());
			Assert.AreEqual(testStrUtf8, Bits.Fmt("{0:utf8}", testStr).ToArray());
			Assert.AreEqual(testStrUtf16, Bits.Fmt("{0:utf16}", testStr).ToArray());
			Assert.AreEqual(testStrUtf16be, Bits.Fmt("{0:utf16be}", testStr).ToArray());
			Assert.AreEqual(testStrUtf7, Bits.Fmt("{0:utf7}", testStr).ToArray());
			Assert.AreEqual(testStrUtf32, Bits.Fmt("{0:utf32}", testStr).ToArray());

			// Numeric format options are not allowed on string
			Assert.Throws<FormatException>(delegate()
			{
				Bits.Fmt("{0:L4}", testStr);
			});

			// Can format a byte[]
			Assert.AreEqual(testBuf, Bits.Fmt("{0}", testBuf).ToArray());

			// Numeric format options are not allowed on byte[]
			Assert.Throws<FormatException>(delegate()
			{
				Bits.Fmt("{0:L4}", testBuf);
			});

			// String format options are not allowed on byte[]
			Assert.Throws<FormatException>(delegate()
			{
				Bits.Fmt("{0:utf16}", testBuf);
			});
		}
	}
}
