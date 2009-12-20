using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PeachCore
{
	public class BitStream
	{
		protected List<byte> buff;
		protected ulong pos = 0;
		protected ulong len = 0;
		protected bool _isLittleEndian = true;
		protected bool isNormalRead = true;
		protected bool _padding = true;
		protected bool _readLeftToRight = false;

		public BitStream()
		{
			buff = new List<byte>();
			LittleEndian();
		}

		public BitStream(byte[] buff)
		{
			buff = new List<byte>(buff);
			len = buff.Length * 8;
			LittleEndian();
		}

		public void Clear()
		{
			buff = new List<byte>();
			pos = 0;
			len = 0;
			LittleEndian();
		}

		/// <summary>
		/// Current position in bits
		/// </summary>
		/// <returns>Returns current bit position</returns>
		public ulong TellBits()
		{
			return pos;
		}

		/// <summary>
		/// Current position in bytes
		/// </summary>
		/// <returns>Returns current byte position</returns>
		public ulong TellBytes()
		{
			return pos / 8;
		}

		/// <summary>
		/// Seek to a position in our stream.  We 
		/// will will expand the stream as needed.
		/// </summary>
		/// <param name="offset">Offset from origion to seek to</param>
		/// <param name="origin">Origin to seek from</param>
		public void SeekBits(long offset, SeekOrigin origin)
		{
			switch (origin)
			{
				case SeekOrigin.Begin:
					pos = offset;
					break;
				case SeekOrigin.Current:
					pos = pos + offset;
					break;
				case SeekOrigin.End:
					pos = len - offset;
					break;
			}
		}

		/// <summary>
		/// Seek to a position in our stream.  We 
		/// will will expand the stream as needed.
		/// </summary>
		/// <param name="offset">Offset from origion to seek to</param>
		/// <param name="origin">Origin to seek from</param>
		public void SeekBytes(ulong offset, SeekOrigin origin)
		{
			SeekBits(offset * 8, origin);
		}

		/// <summary>
		/// Is byte padding enabled.
		/// </summary>
		public bool Padding
		{
			get { return _padding; }
			set { _padding = value; }
		}

		/// <summary>
		/// Pack/unpack as big endian values.
		/// </summary>
		public void BigEndian()
		{
			_isLittleEndian = false;
			ReadRightToLeft();
		}

		/// <summary>
		/// Pack/unpack as little endian values.
		/// </summary>
		public void LittleEndian()
		{
			_isLittleEndian = true;
			ReadRightToLeft();
		}

		/// <summary>
		/// Pack/Unack bits from left to right.  Normally
		/// big endian is left to right and little endian
		/// is right to left.
		/// 
		/// Changing endianness via LittleEndian() or BigEndian()
		/// will reset this to default method.
		/// </summary>
		public void ReadLeftToRight()
		{
			_readLeftToRight = true;
		}

		/// <summary>
		/// Pack/Unack bits from right to left.  Normally
		/// big endian is left to right and little endian
		/// is right to left.
		/// 
		/// Changing endianness via LittleEndian() or BigEndian()
		/// will reset this to default method.
		/// </summary>
		public void ReadRightToLeft()
		{
			_readLeftToRight = false;
		}

		public void WriteSByte(sbyte value);
		public void WriteInt8(sbyte value);
		public void WriteByte(byte value);
		public void WriteUInt8(byte value);
		public void WriteShort(short value);
		public void WriteInt16(short value);
		public void WriteUShort(ushort value);
		public void WriteUInt16(ushort value);
		public void WriteWORD(ushort value);
		public void WriteInt(int value);
		public void WriteInt32(int value);
		public void WriteUInt(uint value);
		public void WriteUInt32(uint value);
		public void WriteDWORD(uint value);
		public void WriteLong(long value);
		public void WriteInt64(long value);
		public void WriteULong(ulong value);
		public void WriteUInt64(ulong value);

		public void Write(BitStream bits);
		public void WriteBits(uint value, uint bits);
		public void WriteBytes(byte[] value);
		public void WriteBytes(byte[] value, int offset, int length);

		public sbyte ReadSByte();
		public sbyte ReadInt8();
		public byte ReadByte();
		public byte ReadUInt8();
		public short ReadShort();
		public short ReadInt16();
		public ushort ReadUShort();
		public ushort ReadUInt16();
		public ushort ReadWORD();
		public int ReadInt();
		public int ReadInt32();
		public uint ReadUInt();
		public uint ReadUInt32();
		public uint ReadDWORD();
		public long ReadLong();
		public long ReadInt64();
		public ulong ReadULong();
		public ulong ReadUInt64();

		public ulong ReadBits(uint bits);
		public byte[] ReadBytes();
	}
}
