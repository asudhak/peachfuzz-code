using System;

namespace Peach.Core.IO
{
	public interface IEndian
	{
		int ShiftBy(int written, int todo);
		ulong GetBits(ulong value, int bitlen);
		ulong GetValue(ulong bits, int bitlen);
	}

	public class LittleEndian : IEndian
	{
		public int ShiftBy(int written, int todo) { return written; }

		public ulong GetBits(ulong value, int bitlen)
		{
			ulong ret = 0;

			while (bitlen > 0)
			{
				bitlen =  Math.Max(0, bitlen - 8);
				ret |= (value & 0xff) << bitlen;
				value >>= 8;
			}

			return ret;
		}

		public ulong GetValue(ulong bits, int bitlen)
		{
			ulong ret = 0;
			int shift = bitlen % 8;

			while (bitlen > 0)
			{
				bitlen -= shift;
				ret |= (bits & ((ulong)0xff >> (8 - shift))) << bitlen;
				bits >>= shift;
				shift = 8;
			}

			return ret;
		}
	}

	public class BigEndian : IEndian
	{
		public int ShiftBy(int written, int todo) { return Math.Max(0, todo); }

		public ulong GetBits(ulong value, int bitlen)
		{
			return value;
		}

		public ulong GetValue(ulong bits, int bitlen)
		{
			return bits;
		}
	}

	public class LittleBitWriter : BitWriter<LittleEndian> { }

	public class BigBitWriter : BitWriter<BigEndian> { }

	public class BitWriter<T> where T : IEndian, new()
	{
		#region Helpers

		private static T endian = new T();

		private static long SignExpand(ulong value, int bitlen)
		{
			ulong mask = ((ulong)1 << (bitlen - 1));
			if ((value & mask) != 0)
				value |= ~(mask - 1);

			return (long)value;
		}

		#endregion

		#region Value -> byte[]

		public static byte[] GetBytes(sbyte value, int bitlen)
		{
			return GetBytes((ulong)value, bitlen, 8);
		}

		public static byte[] GetBytes(byte value, int bitlen)
		{
			return GetBytes((ulong)value, bitlen, 8);
		}

		public static byte[] GetBytes(short value, int bitlen)
		{
			return GetBytes((ulong)value, bitlen, 16);
		}

		public static byte[] GetBytes(ushort value, int bitlen)
		{
			return GetBytes((ulong)value, bitlen, 16);
		}

		public static byte[] GetBytes(int value, int bitlen)
		{
			return GetBytes((ulong)value, bitlen, 32);
		}

		public static byte[] GetBytes(uint value, int bitlen)
		{
			return GetBytes((ulong)value, bitlen, 32);
		}

		public static byte[] GetBytes(long value, int bitlen)
		{
			return GetBytes((ulong)value, bitlen, 64);
		}

		public static byte[] GetBytes(ulong value, int bitlen)
		{
			return GetBytes((ulong)value, bitlen, 64);
		}

		#endregion

		#region Value -> Bits

		public static ulong GetBits(sbyte value, int bitlen)
		{
			return GetBits((ulong)value, bitlen, 8);
		}

		public static ulong GetBits(byte value, int bitlen)
		{
			return GetBits((ulong)value, bitlen, 8);
		}

		public static ulong GetBits(short value, int bitlen)
		{
			return GetBits((ulong)value, bitlen, 16);
		}

		public static ulong GetBits(ushort value, int bitlen)
		{
			return GetBits((ulong)value, bitlen, 16);
		}

		public static ulong GetBits(int value, int bitlen)
		{
			return GetBits((ulong)value, bitlen, 32);
		}

		public static ulong GetBits(uint value, int bitlen)
		{
			return GetBits((ulong)value, bitlen, 32);
		}

		public static ulong GetBits(long value, int bitlen)
		{
			return GetBits((ulong)value, bitlen, 64);
		}

		public static ulong GetBits(ulong value, int bitlen)
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

		#region Bits to Value

		public static sbyte GetInt8(ulong bits, int bitlen)
		{
			return (sbyte)GetSigned(bits, bitlen, 8);
		}

		public static byte GetUInt8(ulong bits, int bitlen)
		{
			return (byte)GetUnsigned(bits, bitlen, 8);
		}

		public static short GetInt16(ulong bits, int bitlen)
		{
			return (short)GetSigned(bits, bitlen, 16);
		}

		public static ushort GetUInt16(ulong bits, int bitlen)
		{
			return (ushort)GetUnsigned(bits, bitlen, 16);
		}

		public static int GetInt32(ulong bits, int bitlen)
		{
			return (int)GetSigned(bits, bitlen, 32);
		}

		public static uint GetUInt32(ulong bits, int bitlen)
		{
			return (uint)GetUnsigned(bits, bitlen, 32);
		}

		public static long GetInt64(ulong bits, int bitlen)
		{
			return (long)GetSigned(bits, bitlen, 64);
		}

		public static ulong GetUInt64(ulong bits, int bitlen)
		{
			return (ulong)GetUnsigned(bits, bitlen, 64);
		}

		#endregion

		#region Bit Conversion

		private static ulong GetBits(ulong value, int bitlen, int maxlen)
		{
			if (bitlen < 0 || bitlen > maxlen)
				throw new ArgumentOutOfRangeException("bitlen");

			if (bitlen < 64)
				value &= ((ulong)1 << bitlen) - 1;

			return endian.GetBits(value, bitlen);
		}

		private static ulong GetUnsigned(ulong bits, int bitlen, int maxlen)
		{
			if (bitlen < 0 || bitlen > maxlen)
				throw new ArgumentOutOfRangeException("bitlen");

			if (bitlen < 64)
				bits &= ((ulong)1 << bitlen) - 1;

			return endian.GetValue(bits, bitlen);
		}

		private static long GetSigned(ulong bits, int bitlen, int maxlen)
		{
			ulong ret = GetUnsigned(bits, bitlen, maxlen);

			return SignExpand(ret, bitlen);
		}

		#endregion

		#region Byte Conversion

		private static byte[] GetBytes(ulong value, int bitlen, int maxlen)
		{
			if (bitlen < 0 || bitlen > maxlen)
				throw new ArgumentOutOfRangeException("bitlen");

			byte[] ret = new byte[(bitlen + 7) / 8];
			int index = 0;
			int written = 0;

			while (bitlen > 0)
			{
				bitlen -= 8;

				// LE: written, BE: bitlen
				int shift = endian.ShiftBy(written, bitlen);
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
			if (bitlen < 0 || bitlen > maxlen)
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
				int shift = endian.ShiftBy(written, bitlen);

				ret |= ((ulong)next << shift);
				written += 8;
			}

			return ret;
		}

		private static long GetSigned(byte[] buf, int bitlen, int maxlen)
		{
			ulong ret = GetUnsigned(buf, bitlen, maxlen);

			return SignExpand(ret, bitlen);
		}

		#endregion
	}
}
