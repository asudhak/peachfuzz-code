using System;

namespace Peach.Core.IO
{
	public class BitWriter : IDisposable
	{
		Endian endian = Endian.Little;
		bool leaveOpen;

		public BitWriter(BitwiseStream stream)
			: this(stream, false)
		{
		}

		public BitWriter(BitwiseStream stream, bool leaveOpen)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");

			BaseStream = stream;
			this.leaveOpen = leaveOpen;
		}

		public void Dispose()
		{
			if (!leaveOpen)
				BaseStream.Close();

			BaseStream = null;
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

		public void WriteBit(int value)
		{
			BaseStream.WriteBit(value);
		}

		public void WriteBits(ulong bits, int count)
		{
			BaseStream.WriteBits(bits, count);
		}

		public void WriteBytes(byte[] buffer)
		{
			BaseStream.Write(buffer, 0, buffer.Length);
		}

		public void WriteSByte(sbyte value)
		{
			WriteBytes(endian.GetBytes(value, 8));
		}

		public void WriteByte(byte value)
		{
			WriteBytes(endian.GetBytes(value, 8));
		}

		public void WriteInt16(short value)
		{
			WriteBytes(endian.GetBytes(value, 16));
		}

		public void WriteUInt16(ushort value)
		{
			WriteBytes(endian.GetBytes(value, 16));
		}

		public void WriteInt32(int value)
		{
			WriteBytes(endian.GetBytes(value, 32));
		}

		public void WriteUInt32(uint value)
		{
			WriteBytes(endian.GetBytes(value, 32));
		}

		public void WriteInt64(long value)
		{
			WriteBytes(endian.GetBytes(value, 64));
		}

		public void WriteUInt64(ulong value)
		{
			WriteBytes(endian.GetBytes(value, 64));
		}

		public void WriteString(string value)
		{
			WriteString(value, Encoding.ASCII);
		}

		public void WriteString(string value, Encoding encoding)
		{
			WriteBytes(encoding.GetBytes(value));
		}
	}
}
