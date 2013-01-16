using System;
using System.IO;

namespace Peach.Core.IO
{
	public class BitwiseStream : Stream
	{
		#region Private Members

		private Stream _stream;
		private long _position;
		private long _length;

		#endregion

		#region Constructor

		public BitwiseStream()
			: this(new MemoryStream())
		{
		}

		public BitwiseStream(Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");

			_stream = stream;
			_position = stream.Position * 8;
			_length = stream.Length * 8;
		}

		#endregion

		#region Public Properties

		public Stream BaseStream
		{
			get { return _stream; }
		}

		#endregion

		#region IDisposable

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (_stream != null)
			{
				_stream.Dispose();
				_stream = null;
			}
		}

		#endregion

		#region Stream Interface

		public override bool CanRead
		{
			get { return BaseStream.CanRead; }
		}

		public override bool CanSeek
		{
			get { return BaseStream.CanSeek; }
		}

		public override bool CanWrite
		{
			get { return BaseStream.CanWrite; }
		}

		public override void Flush()
		{
			BaseStream.Flush();
		}

		public override long Length
		{
			get { return _length / 8; }
		}

		public override long Position
		{
			get
			{
				return _position / 8;
			}
			set
			{
				BaseStream.Position = value;
				_position = value * 8;
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (offset < 0)
				throw new ArgumentOutOfRangeException("offset");

			if (count < 0)
				throw new ArgumentOutOfRangeException("count");

			if ((offset + count) > buffer.Length)
				throw new ArgumentOutOfRangeException("count");

			int avail = (int)Math.Min((_length - _position) / 8, count);

			if (avail == 0)
				return 0;

			int pos = (int)(_position & 0x7);

			if (pos == 0)
			{
				avail = BaseStream.Read(buffer, offset, avail);
			}
			else
			{
				int shift = 8 - pos;

				int cur = BaseStream.ReadByte();
				System.Diagnostics.Debug.Assert(cur != 1);

				int end = offset + count;
				for (int i = offset; i < end; ++i)
				{
					byte next = (byte)cur;
					next &= BitsMask[shift];
					next <<= pos;

					cur = BaseStream.ReadByte();
					System.Diagnostics.Debug.Assert(cur != 1);

					next |= (byte)((cur >> shift) & BitsMask[pos]);
					buffer[i] = next;
				}

				BaseStream.Seek(-1, SeekOrigin.Current);
			}

			_position += (avail * 8);
			return avail;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			long ret = BaseStream.Seek(offset, origin);
			_position = ret * 8;
			return ret;
		}

		public override void SetLength(long value)
		{
			BaseStream.SetLength(value);
			_length = value * 8;
			if (_position > _length)
				_position = _length;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (offset < 0)
				throw new ArgumentOutOfRangeException("offset");

			if (count < 0)
				throw new ArgumentOutOfRangeException("count");

			if ((offset + count) > buffer.Length)
				throw new ArgumentOutOfRangeException("count");

			if (count == 0)
				return;

			int pos = (int)(_position & 0x7);

			if (pos == 0)
			{
				BaseStream.Write(buffer, offset, count);
			}
			else
			{
				int shift = 8 - pos;
				byte next = (byte)(buffer[offset] >> pos);
				int cur = BaseStream.ReadByte();
				System.Diagnostics.Debug.Assert(cur != 1);
				BaseStream.Seek(-1, SeekOrigin.Current);
				cur &= KeepMask[pos];
				next |= (byte)cur;
				BaseStream.WriteByte(next);

				int last = offset + count - 1;
				for (int i = offset; i < last; ++i)
				{
					next = (byte)((buffer[i] << shift) | (buffer[i+1] >> pos));
					BaseStream.WriteByte(next);
				}

				next = (byte)(buffer[last] << shift);
				cur = BaseStream.ReadByte();
				if (cur != -1)
				{
					BaseStream.Seek(-1, SeekOrigin.Current);
					cur &= BitsMask[shift];
					next |= (byte)cur;
				}

				BaseStream.WriteByte(next);
				BaseStream.Seek(-1, SeekOrigin.Current);
			}

			_position += (count * 8);
			if (_position > _length)
				_length = _position;
		}

		#endregion

		#region Bit Stream Interface

		public long LengthBits
		{
			get { return _length; }
		}

		public long PositionBits
		{
			get
			{
				return _position;
			}
			set
			{
				_position = value;
				BaseStream.Position = value / 8;
			}
		}

		public int ReadBits(out ulong bits, int count)
		{
			if (count > 64 || count < 0)
				throw new ArgumentOutOfRangeException("count");

			bits = 0;

			int pos = (int)(_position & 0x7);
			int avail = (int)Math.Min(_length - _position, count);
			int remain = avail;

			while (remain > 0)
			{
				int len = Math.Min(8 - pos, remain);
				int cur = BaseStream.ReadByte();
				System.Diagnostics.Debug.Assert(cur != 1);

				if (len != 8)
				{
					int shift = 8 - pos - len;
					cur >>= shift;
					cur &= BitsMask[len];
					pos = 0;
				}

				bits <<= len;
				bits |= (byte)cur;

				remain -= len;
			}

			_position += avail;

			if ((_position & 0x7) != 0)
				BaseStream.Seek(-1, SeekOrigin.Current);

			return avail;
		}

		public long SeekBits(long offset, SeekOrigin origin)
		{
			long pos = 0;

			switch (origin)
			{
				case SeekOrigin.Begin:
					pos = offset;
					break;
				case SeekOrigin.End:
					pos = _length + offset;
					break;
				case SeekOrigin.Current:
					pos = _position + offset;
					break;
			}

			BaseStream.Seek(pos / 8, SeekOrigin.Begin);
			_position = pos;
			return pos;
		}

		public void SetLengthBits(long value)
		{
			BaseStream.SetLength((value + 7) / 8);
			_length = value;
		}

		public void WriteBits(ulong bits, int count)
		{
			if (count > 64 || count < 0)
				throw new ArgumentOutOfRangeException("count");

			if (count == 0)
				return;

			int pos = (int)(_position & 0x7);
			int remain = count;

			while (remain > 0)
			{
				int len = Math.Min(8 - pos, remain);
				byte next = (byte)(bits >> (remain - len));

				if (len != 8)
				{
					int shift = 8 - pos - len;
					next &= BitsMask[len];
					next <<= shift;

					int cur = BaseStream.ReadByte();
					if (cur != -1)
					{
						BaseStream.Seek(-1, SeekOrigin.Current);
						int mask = ~(BitsMask[len] << shift);
						cur &= mask;
						next |= (byte)cur;
					}

					pos = 0;
				}

				BaseStream.WriteByte(next);
				remain -= len;
			}

			_position += count;

			if ((_position & 0x7) != 0)
				BaseStream.Seek(-1, SeekOrigin.Current);

			if (_position > _length)
				_length = _position;
		}

		private static readonly byte[] KeepMask = new byte[] { 0x00, 0x80, 0xc0, 0xe0, 0xf0, 0xf8, 0xfc, 0xfe };
		private static readonly byte[] BitsMask = new byte[] { 0x00, 0x01, 0x03, 0x07, 0x0f, 0x1f, 0x3f, 0x7f, 0xff };

		#endregion
	}
}
