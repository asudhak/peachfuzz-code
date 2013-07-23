using System;

namespace Peach.Core.IO
{
	[Serializable]
	public abstract class Endian
	{
		#region Abstract Functions

		protected abstract int ShiftBy(int written, int todo);
		protected abstract ulong PackBits(ulong value, int bitlen);
		protected abstract ulong UnpackBits(ulong bits, int bitlen);

		#endregion

		#region Helpers

		private static long SignExpand(ulong value, int bitlen)
		{
			ulong mask = ((ulong)1 << (bitlen - 1));
			if ((value & mask) != 0)
				value |= ~(mask - 1);

			return (long)value;
		}

		#endregion

		#region Value -> byte[]

		public byte[] GetBytes(sbyte value, int bitlen)
		{
			return GetBytes((ulong)value, bitlen, 8);
		}

		public byte[] GetBytes(byte value, int bitlen)
		{
			return GetBytes((ulong)value, bitlen, 8);
		}

		public byte[] GetBytes(short value, int bitlen)
		{
			return GetBytes((ulong)value, bitlen, 16);
		}

		public byte[] GetBytes(ushort value, int bitlen)
		{
			return GetBytes((ulong)value, bitlen, 16);
		}

		public byte[] GetBytes(int value, int bitlen)
		{
			return GetBytes((ulong)value, bitlen, 32);
		}

		public byte[] GetBytes(uint value, int bitlen)
		{
			return GetBytes((ulong)value, bitlen, 32);
		}

		public byte[] GetBytes(long value, int bitlen)
		{
			return GetBytes((ulong)value, bitlen, 64);
		}

		public byte[] GetBytes(ulong value, int bitlen)
		{
			return GetBytes((ulong)value, bitlen, 64);
		}

		#endregion

		#region Value -> Bits

		public ulong GetBits(sbyte value, int bitlen)
		{
			return GetBits((ulong)value, bitlen, 8);
		}

		public ulong GetBits(byte value, int bitlen)
		{
			return GetBits((ulong)value, bitlen, 8);
		}

		public ulong GetBits(short value, int bitlen)
		{
			return GetBits((ulong)value, bitlen, 16);
		}

		public ulong GetBits(ushort value, int bitlen)
		{
			return GetBits((ulong)value, bitlen, 16);
		}

		public ulong GetBits(int value, int bitlen)
		{
			return GetBits((ulong)value, bitlen, 32);
		}

		public ulong GetBits(uint value, int bitlen)
		{
			return GetBits((ulong)value, bitlen, 32);
		}

		public ulong GetBits(long value, int bitlen)
		{
			return GetBits((ulong)value, bitlen, 64);
		}

		public ulong GetBits(ulong value, int bitlen)
		{
			return GetBits((ulong)value, bitlen, 64);
		}

		#endregion

		#region byte[] to Value

		public sbyte GetSByte(byte[] buf, int bitlen)
		{
			return (sbyte)GetSigned(buf, bitlen, 8);
		}

		public byte GetByte(byte[] buf, int bitlen)
		{
			return (byte)GetUnsigned(buf, bitlen, 8);
		}

		public short GetInt16(byte[] buf, int bitlen)
		{
			return (short)GetSigned(buf, bitlen, 16);
		}

		public ushort GetUInt16(byte[] buf, int bitlen)
		{
			return (ushort)GetUnsigned(buf, bitlen, 16);
		}

		public int GetInt32(byte[] buf, int bitlen)
		{
			return (int)GetSigned(buf, bitlen, 32);
		}

		public uint GetUInt32(byte[] buf, int bitlen)
		{
			return (uint)GetUnsigned(buf, bitlen, 32);
		}

		public long GetInt64(byte[] buf, int bitlen)
		{
			return (long)GetSigned(buf, bitlen, 64);
		}

		public ulong GetUInt64(byte[] buf, int bitlen)
		{
			return (ulong)GetUnsigned(buf, bitlen, 64);
		}

		#endregion

		#region Bits to Value

		public sbyte GetSByte(ulong bits, int bitlen)
		{
			return (sbyte)GetSigned(bits, bitlen, 8);
		}

		public byte GetByte(ulong bits, int bitlen)
		{
			return (byte)GetUnsigned(bits, bitlen, 8);
		}

		public short GetInt16(ulong bits, int bitlen)
		{
			return (short)GetSigned(bits, bitlen, 16);
		}

		public ushort GetUInt16(ulong bits, int bitlen)
		{
			return (ushort)GetUnsigned(bits, bitlen, 16);
		}

		public int GetInt32(ulong bits, int bitlen)
		{
			return (int)GetSigned(bits, bitlen, 32);
		}

		public uint GetUInt32(ulong bits, int bitlen)
		{
			return (uint)GetUnsigned(bits, bitlen, 32);
		}

		public long GetInt64(ulong bits, int bitlen)
		{
			return (long)GetSigned(bits, bitlen, 64);
		}

		public ulong GetUInt64(ulong bits, int bitlen)
		{
			return (ulong)GetUnsigned(bits, bitlen, 64);
		}

		#endregion

		#region Bit Conversion

		private ulong GetBits(ulong value, int bitlen, int maxlen)
		{
			if (bitlen < 0 || bitlen > maxlen)
				throw new ArgumentOutOfRangeException("bitlen");

			if (bitlen < 64)
				value &= ((ulong)1 << bitlen) - 1;

			return PackBits(value, bitlen);
		}

		private ulong GetUnsigned(ulong bits, int bitlen, int maxlen)
		{
			if (bitlen < 0 || bitlen > maxlen)
				throw new ArgumentOutOfRangeException("bitlen");

			if (bitlen < 64)
				bits &= ((ulong)1 << bitlen) - 1;

			return UnpackBits(bits, bitlen);
		}

		private long GetSigned(ulong bits, int bitlen, int maxlen)
		{
			ulong ret = GetUnsigned(bits, bitlen, maxlen);

			return SignExpand(ret, bitlen);
		}

		#endregion

		#region Byte Conversion

		private byte[] GetBytes(ulong value, int bitlen, int maxlen)
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
				int shift = ShiftBy(written, bitlen);
				byte next = (byte)(value >> shift);

				if (bitlen < 0)
					next <<= -bitlen;

				ret[index++] = next;
				written += 8;
			}

			return ret;
		}

		private ulong GetUnsigned(byte[] buf, int bitlen, int maxlen)
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
				int shift = ShiftBy(written, bitlen);

				ret |= ((ulong)next << shift);
				written += 8;
			}

			return ret;
		}

		private long GetSigned(byte[] buf, int bitlen, int maxlen)
		{
			ulong ret = GetUnsigned(buf, bitlen, maxlen);

			return SignExpand(ret, bitlen);
		}

		#endregion

		#region Static Properties

		private static Endian big = new BigEndian();
		private static Endian little = new LittleEndian();

		public static Endian Big
		{
			get { return big; }
		}

		public static Endian Little
		{
			get { return little; }
		}

		#endregion
	}

	[Serializable]
	public class LittleEndian : Endian
	{
		protected override int ShiftBy(int written, int todo)
		{
			return written;
		}

		protected override ulong PackBits(ulong value, int bitlen)
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

		protected override ulong UnpackBits(ulong bits, int bitlen)
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

	[Serializable]
	public class BigEndian : Endian
	{
		protected override int ShiftBy(int written, int todo)
		{
			return Math.Max(0, todo);
		}

		protected override ulong PackBits(ulong value, int bitlen)
		{
			return value;
		}

		protected override ulong UnpackBits(ulong bits, int bitlen)
		{
			return bits;
		}
	}
}
