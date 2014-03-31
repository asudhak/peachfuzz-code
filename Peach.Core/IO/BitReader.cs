using System;
using System.IO;
using System.Text;

namespace Peach.Core.IO
{
	public class BitReader : IDisposable
	{
		Endian endian = Endian.Little;
		bool leaveOpen;

		public BitReader(BitwiseStream stream)
			: this(stream, false)
		{
		}

		public BitReader(BitwiseStream stream, bool leaveOpen)
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

		public string ReadString()
		{
			return ReadString(Encoding.ASCII);
		}

		public string ReadString(Encoding encoding)
		{
			var sb = new StringBuilder();
			var dec = encoding.GetDecoder();
			var chars = new char[2];
			var buf = new byte[1];
			var idx = 0;

			while (true)
			{
				int len = BaseStream.Read(buf, 0, buf.Length);

				if (len == 0)
				{
					if (idx != 0)
						throw new IOException("Couldn't convert last " + idx + " bytes into string.");

					long bits = BaseStream.LengthBits - BaseStream.PositionBits;
					if (bits != 0)
						throw new IOException("Couldn't convert last " + bits + " bits into string.");

					return sb.ToString();
				}

				len = dec.GetChars(buf, 0, buf.Length, chars, 0);

				if (len == 0)
				{
					idx++;
				}
				else
				{
					for (int i = 0; i < len; ++i)
						sb.Append(chars[i]);

					idx = 0;
				}
			}
		}
	}
}
