using System;

namespace Peach.Core.IO
{
	public interface IEndian
	{
		int ShiftBy(int written, int todo);
	}

	public class LittleEndian : IEndian
	{
		public int ShiftBy(int written, int todo) { return written; }
	}

	public class BigEndian : IEndian
	{
		public int ShiftBy(int written, int todo) { return Math.Max(0, todo); }
	}

	public class LittleBitWriter : BitWriter<LittleEndian> { }

	public class BigBitWriter : BitWriter<BigEndian> { }

	public class BitWriter<T> where T : IEndian, new()
	{
		private static T offset = new T();

		#region Value -> byte[]

		public static byte[] GetBits(sbyte value, int bitlen)
		{
			return GetBits((ulong)value, bitlen, 8);
		}

		public static byte[] GetBits(byte value, int bitlen)
		{
			return GetBits((ulong)value, bitlen, 8);
		}

		public static byte[] GetBits(short value, int bitlen)
		{
			return GetBits((ulong)value, bitlen, 16);
		}

		public static byte[] GetBits(ushort value, int bitlen)
		{
			return GetBits((ulong)value, bitlen, 16);
		}

		public static byte[] GetBits(int value, int bitlen)
		{
			return GetBits((ulong)value, bitlen, 32);
		}

		public static byte[] GetBits(uint value, int bitlen)
		{
			return GetBits((ulong)value, bitlen, 32);
		}

		public static byte[] GetBits(long value, int bitlen)
		{
			return GetBits((ulong)value, bitlen, 64);
		}

		public static byte[] GetBits(ulong value, int bitlen)
		{
			return GetBits((ulong)value, bitlen, 64);
		}

		#endregion

		#region byte[] to Value

		public static sbyte GetInt8(byte[] buf, int bitlen)
		{
			return (sbyte)GetSigned(buf, bitlen, 8);
		}

		public static byte GetUInt8(byte[] buf, int bitlen)
		{
			return (byte)GetUnsigned(buf, bitlen, 8);
		}

		public static short GetInt16(byte[] buf, int bitlen)
		{
			return (short)GetSigned(buf, bitlen, 16);
		}

		public static ushort GetUInt16(byte[] buf, int bitlen)
		{
			return (ushort)GetUnsigned(buf, bitlen, 16);
		}

		public static int GetInt32(byte[] buf, int bitlen)
		{
			return (int)GetSigned(buf, bitlen, 32);
		}

		public static uint GetUInt32(byte[] buf, int bitlen)
		{
			return (uint)GetUnsigned(buf, bitlen, 32);
		}

		public static long GetInt64(byte[] buf, int bitlen)
		{
			return (long)GetSigned(buf, bitlen, 64);
		}

		public static ulong GetUInt64(byte[] buf, int bitlen)
		{
			return (ulong)GetUnsigned(buf, bitlen, 64);
		}

		#endregion

		#region Bit Conversion

		private static byte[] GetBits(ulong value, int bitlen, int maxlen)
		{
			if (bitlen <= 0 || bitlen > maxlen)
				throw new ArgumentOutOfRangeException("bitlen");

			byte[] ret = new byte[(bitlen + 7) / 8];
			int index = 0;
			int written = 0;

			while (bitlen > 0)
			{
				bitlen -= 8;

				// LE: written, BE: bitlen
				int shift = offset.ShiftBy(written, bitlen);
				byte next = (byte)(value >> shift);

				if (bitlen < 0)
					next <<= -bitlen;

				ret[index++] = next;
				written += 8;
			}

			return ret;
		}

		private static ulong GetUnsigned(byte[] buf, int bitlen, int maxlen)
		{
			if (bitlen <= 0 || bitlen > maxlen)
				throw new ArgumentOutOfRangeException("bitlen");
			if (buf.Length != ((bitlen + 7) / 8))
				throw new ArgumentOutOfRangeException("buf");

			ulong ret = 0;
			int index = 0;
			int written = 0;

			while (bitlen > 0)
			{
				bitlen -= 8;

				byte next = buf[index++];

				if (bitlen < 0)
					next >>= -bitlen;

				// LE: written, BE: bitlen
				int shift = offset.ShiftBy(written, bitlen);

				ret |= ((ulong)next << shift);
				written += 8;
			}

			return ret;
		}

		private static long GetSigned(byte[] buf, int bitlen, int maxlen)
		{
			ulong ret = GetUnsigned(buf, bitlen, maxlen);

			// Handle sign expansion
			ulong mask = ((ulong)1 << (bitlen - 1));
			if ((ret & mask) != 0)
				ret |= ~(mask - 1);

			return (long)ret;
		}

		#endregion
	}
}
