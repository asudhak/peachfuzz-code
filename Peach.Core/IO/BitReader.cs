using System;
using System.IO;

namespace Peach.Core.IO
{
	public class BitReader
	{
		Endian endian = Endian.Little;

		public BitReader(BitwiseStream stream)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");

			BaseStream = stream;
		}

		public BitwiseStream BaseStream
		{
			get;
			private set;
		}

		public void LittleEndian()
		{
			endian = Endian.Little;
		}

		public void BigEndian()
		{
			endian = Endian.Big;
		}

		public int ReadBit()
		{
			return BaseStream.ReadBit();
		}

		public ulong ReadBits(int count)
		{
			ulong bits;
			int len = BaseStream.ReadBits(out bits, count);
			if (len != count)
				throw new IOException("Not enough data available.");
			return bits;
		}

		public byte[] ReadBytes(int count)
		{
			byte[] buf = new byte[count];
			int len = BaseStream.Read(buf, 0, count);
			if (len != count)
				throw new IOException("Not enough data available.");
			return buf;
		}

		public sbyte ReadSByte()
		{
			return endian.GetSByte(ReadBits(8), 8);
		}

		public byte ReadByte()
		{
			return endian.GetByte(ReadBits(8), 8);
		}

		public short ReadInt16()
		{
			return endian.GetInt16(ReadBits(16), 16);
		}

		public ushort ReadUInt16()
		{
			return endian.GetUInt16(ReadBits(16), 16);
		}

		public int ReadInt32()
		{
			return endian.GetInt32(ReadBits(32), 32);
		}

		public uint ReadUInt32()
		{
			return endian.GetUInt32(ReadBits(32), 32);
		}

		public long ReadInt64()
		{
			return endian.GetInt64(ReadBits(64), 64);
		}

		public ulong ReadUInt64()
		{
			return endian.GetUInt64(ReadBits(64), 64);
		}
	}
}
