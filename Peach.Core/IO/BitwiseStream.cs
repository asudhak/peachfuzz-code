using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Peach.Core.IO.New
{
	public static class BitwiseStreamExtensions
	{
		public static ulong ReadBits(this BitStream bs, int count)
		{
			ulong bits;
			int len = bs.ReadBits(out bits, count);
			if (len != count)
				throw new IOException();

			return bits;
		}

		public static long SeekBytes(this BitStream bs, long offset, SeekOrigin origin)
		{
			return bs.Seek(offset, origin);
		}

		public static long TellBits(this BitStream bs)
		{
			return bs.PositionBits;
		}

		public static byte[] ReadBytes(this BitStream bs, long count)
		{
			var buf = new byte[count];
			int len = bs.Read(buf, 0, (int)count);
			if (count != len)
				throw new IOException();

			return buf;
		}

		public static void WriteBytes(this BitStream bs, byte[] buffer)
		{
			bs.Write(buffer, 0, buffer.Length);
		}

		public static long TellBytes(this BitStream bs)
		{
			return bs.Position;
		}

		public static void Write(this BitStream bs, BitwiseStream other)
		{
			long pos = other.PositionBits;
			other.SeekBits(0, SeekOrigin.Begin);
			other.CopyTo(bs);
			other.SeekBits(pos, SeekOrigin.Begin);
		}

		public static void WriteBit(this BitStream bs, byte bit)
		{
			bs.WriteBits(bit, 1);
		}

		public static byte ReadBit(this BitStream bs)
		{
			return (byte)ReadBits(bs, 1);
		}

		public static void Truncate(this BitStream bs)
		{
			bs.SetLengthBits(bs.PositionBits);
		}

		public static BitStream ReadBitsAsBitStream(this BitStream bs, long bits)
		{
			var ret = new BitStream();
			const int bufferSize = 16 * 1024;
			var buffer = new byte[bufferSize];
			long remain = bits / 8;
			long pos = bs.PositionBits;

			while (remain > 0)
			{
				int readSize = (int)Math.Min(remain, bufferSize);
				int len = bs.Read(buffer, 0, readSize);

				// Why??
				if (len == 0)
					throw new IOException();

				ret.Write(buffer, 0, len);
				remain -= len;
			}

			ulong tmp;
			int bitLen = bs.ReadBits(out tmp, (int)(bits % 8));
			ret.WriteBits(tmp, bitLen);

			bs.SeekBits(pos, SeekOrigin.Begin);
			ret.SeekBits(0, SeekOrigin.Begin);

			return ret;
		}

		public static BitStream Clone(this BitStream bs)
		{
			var ret = new BitStream();
			long pos = bs.PositionBits;
			bs.SeekBits(0, SeekOrigin.Begin);
			bs.CopyTo(ret);
			bs.SeekBits(pos, SeekOrigin.Begin);
			return ret;
		}
	}

	public abstract class BitwiseStream : Stream
	{
		#region Constructor

		protected BitwiseStream() { }

		#endregion

		#region Bitwise Interface

		public abstract long LengthBits { get; }
		public abstract long PositionBits { get; set; }
		public abstract long SeekBits(long offset, SeekOrigin origin);
		public abstract int ReadBits(out ulong bits, int count);
		public abstract void SetLengthBits(long value);
		public abstract void WriteBits(ulong bits, int count);

		#endregion

		#region Stream Specializations

		public void CopyTo(BitwiseStream destination)
		{
			CopyTo(destination, 16 * 1024);
		}

		public void CopyTo(BitwiseStream destination, int bufferSize)
		{
			if (destination == null)
				throw new ArgumentNullException("destination");
			if (!CanRead)
				throw new NotSupportedException("This stream does not support reading");
			if (!destination.CanWrite)
				throw new NotSupportedException("This destination stream does not support writing");
			if (bufferSize <= 0)
				throw new ArgumentOutOfRangeException("bufferSize");

			var buffer = new byte[bufferSize];
			int nread;
			while ((nread = Read(buffer, 0, bufferSize)) != 0)
				destination.Write(buffer, 0, nread);

			ulong bits;
			nread = ReadBits(out bits, 7);
			destination.WriteBits(bits, nread);
		}

		#endregion

		#region Legacy Compatibility

		[Obsolete]
		public byte[] Value
		{
			get
			{
				var dest = new MemoryStream();
				long pos = PositionBits;
				SeekBits(0, SeekOrigin.Begin);
				CopyTo(dest);
				SeekBits(pos, SeekOrigin.Begin);
				return dest.ToArray();
			}
		}

		[Obsolete]
		public Stream Stream
		{
			get
			{
				return this;
			}
		}

		[Obsolete]
		public long LengthBytes
		{
			get
			{
				return Length;
			}
		}

		#endregion
	}

	[DebuggerDisplay("{Progress}")]
	public class BitStream : BitwiseStream
	{
		#region Private Members

		private Stream _stream;
		private long _position;
		private long _length;

		#endregion

		#region Constructor

		public BitStream()
			: this(new MemoryStream())
		{
		}

		[Obsolete]
		public BitStream(byte[] buffer)
			: this(new MemoryStream(buffer))
		{
		}

		public BitStream(Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");

			_stream = stream;
			_position = stream.Position * 8;
			_length = stream.Length * 8;
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

		#region Utility Functions

		public string Progress
		{
			get
			{
				return "Bytes: {0}/{1}, Bits: {2}/{3}".Fmt(Position, Length, PositionBits, LengthBits);
			}
		}

		public void WantBytes(long bytes)
		{
			if (bytes < 0)
				throw new ArgumentOutOfRangeException("bytes", "Non-negative number required.");

			if (bytes == 0)
				return;

			Publisher pub = _stream as Publisher;
			if (pub != null)
				pub.WantBytes(bytes);
		}

		#endregion

		#region Stream Interface

		public override bool CanRead
		{
			get { return _stream.CanRead; }
		}

		public override bool CanSeek
		{
			get { return _stream.CanSeek; }
		}

		public override bool CanWrite
		{
			get { return _stream.CanWrite; }
		}

		public override void Flush()
		{
			_stream.Flush();
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
				if (value < 0)
					throw new ArgumentOutOfRangeException("value", "Non-negative number required.");

				_stream.Position = value;
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

			if (avail <= 0)
				return 0;

			int pos = (int)(_position & 0x7);

			if (pos == 0)
			{
				// If we are aligned on stream, just read
				avail = _stream.Read(buffer, offset, avail);
			}
			else
			{
				// If we are unaligned on stream, need to combine two bytes
				int shift = 8 - pos;

				// First read the high bits
				int cur = _stream.ReadByte();
				System.Diagnostics.Debug.Assert(cur != 1);

				int end = offset + avail;
				for (int i = offset; i < end; ++i)
				{
					// Shift the high bits into place
					byte next = (byte)cur;
					next &= BitsMask[shift];
					next <<= pos;

					// Read the low bits from the next byte
					cur = _stream.ReadByte();
					System.Diagnostics.Debug.Assert(cur != -1);

					// Shift the low bits into place
					next |= (byte)((cur >> shift) & BitsMask[pos]);

					// Save the combined byte
					buffer[i] = next;
				}

				// Since our position is not aligned, we need to back up
				// the stream by a single byte so subsequent reads/writes work
				_stream.Seek(-1, SeekOrigin.Current);
			}

			// Advance position
			_position += (avail * 8);

			return avail;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			long pos = 0;

			switch (origin)
			{
				case SeekOrigin.Begin:
					pos = (offset * 8);
					break;
				case SeekOrigin.End:
					pos = _length + (offset * 8);
					break;
				case SeekOrigin.Current:
					pos = _position + (offset * 8);
					break;
			}

			if (pos < 0)
				throw new IOException("An attempt was made to move the position before the beginning of the stream.");

			_position = pos;
			pos = _stream.Seek(offset, origin);

			return pos;
		}

		public override void SetLength(long value)
		{
			_stream.SetLength(value);
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
				_stream.Write(buffer, offset, count);
			}
			else
			{
				int shift = 8 - pos;
				byte next = (byte)(buffer[offset] >> pos);
				int cur = _stream.ReadByte();
				if (cur != -1)
				{
					_stream.Seek(-1, SeekOrigin.Current);
					cur &= KeepMask[pos];
					next |= (byte)cur;
				}
				_stream.WriteByte(next);

				int last = offset + count - 1;
				for (int i = offset; i < last; ++i)
				{
					next = (byte)((buffer[i] << shift) | (buffer[i+1] >> pos));
					_stream.WriteByte(next);
				}

				next = (byte)(buffer[last] << shift);
				cur = _stream.ReadByte();
				if (cur != -1)
				{
					_stream.Seek(-1, SeekOrigin.Current);
					cur &= BitsMask[shift];
					next |= (byte)cur;
				}

				_stream.WriteByte(next);
				_stream.Seek(-1, SeekOrigin.Current);
			}

			_position += (count * 8);
			if (_position > _length)
				_length = _position;
		}

		#endregion

		#region BitwiseStream Interface

		public override long LengthBits
		{
			get { return _length; }
		}

		public override long PositionBits
		{
			get
			{
				return _position;
			}
			set
			{
				if (value < 0)
					throw new ArgumentOutOfRangeException("value", "Non-negative number required.");

				_position = value;
				_stream.Position = value / 8;
			}
		}

		public override int ReadBits(out ulong bits, int count)
		{
			if (count > 64 || count < 0)
				throw new ArgumentOutOfRangeException("count");

			bits = 0;

			if (_length < _position)
				return 0;

			int pos = (int)(_position & 0x7);
			int avail = (int)Math.Min(_length - _position, count);
			int remain = avail;

			while (remain > 0)
			{
				int len = Math.Min(8 - pos, remain);
				int cur = _stream.ReadByte();
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
				_stream.Seek(-1, SeekOrigin.Current);

			return avail;
		}

		public override long SeekBits(long offset, SeekOrigin origin)
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

			if (pos < 0)
				throw new IOException("An attempt was made to move the position before the beginning of the stream.");

			_position = pos;
			_stream.Seek(pos / 8, SeekOrigin.Begin);

			return pos;
		}

		public override void SetLengthBits(long value)
		{
			_stream.SetLength((value + 7) / 8);
			_length = value;
		}

		public override void WriteBits(ulong bits, int count)
		{
			if (count > 64 || count < 0)
				throw new ArgumentOutOfRangeException("count");

			if (count == 0)
				return;

			if (count < 64 && bits >= ((ulong)1 << count))
				throw new ArgumentOutOfRangeException("bits");

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

					int cur = _stream.ReadByte();
					if (cur != -1)
					{
						_stream.Seek(-1, SeekOrigin.Current);
						int mask = ~(BitsMask[len] << shift);
						cur &= mask;
						next |= (byte)cur;
					}

					pos = 0;
				}

				_stream.WriteByte(next);
				remain -= len;
			}

			_position += count;

			if ((_position & 0x7) != 0)
				_stream.Seek(-1, SeekOrigin.Current);

			if (_position > _length)
				_length = _position;
		}

		private static readonly byte[] KeepMask = new byte[] { 0x00, 0x80, 0xc0, 0xe0, 0xf0, 0xf8, 0xfc, 0xfe };
		private static readonly byte[] BitsMask = new byte[] { 0x00, 0x01, 0x03, 0x07, 0x0f, 0x1f, 0x3f, 0x7f, 0xff };

		#endregion
	}

	[DebuggerDisplay("Count = {Count}")]
	public class BitStreamList : BitwiseStream, IList<BitStream>
	{
		#region Private Members

		private List<BitStream> _streams;
		private long _position;

		#endregion

		#region Constructor

		public BitStreamList()
		{
			_streams = new List<BitStream>();
		}

		public BitStreamList(int capacity)
		{
			_streams = new List<BitStream>(capacity);
		}

		public BitStreamList(IEnumerable<BitStream> collection)
		{
			_streams = new List<BitStream>(collection);
		}

		#endregion

		#region IDisposable

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			foreach (var item in _streams)
				item.Dispose();

			_streams.Clear();
		}

		#endregion

		#region BitwiseStream Interface

		public override long LengthBits
		{
			get { return this.Sum(a => a.LengthBits); }
		}

		public override long PositionBits
		{
			get
			{
				return _position;
			}
			set
			{
				_position = value;
			}
		}

		public override long SeekBits(long offset, SeekOrigin origin)
		{
			long pos = 0;

			switch (origin)
			{
				case SeekOrigin.Begin:
					pos = offset;
					break;
				case SeekOrigin.End:
					pos = LengthBits + offset;
					break;
				case SeekOrigin.Current:
					pos = PositionBits + offset;
					break;
			}

			if (pos < 0)
				throw new IOException("An attempt was made to move the position before the beginning of the stream.");

			PositionBits = pos;
			return PositionBits;
		}

		public override int ReadBits(out ulong bits, int count)
		{
			if (count > 64 || count < 0)
				throw new ArgumentOutOfRangeException("count");

			bits = 0;

			int needed = count;
			long pos = 0;

			foreach (var item in this)
			{
				long next = pos + item.LengthBits;

				if (next >= PositionBits)
				{
					long offset = item.PositionBits;
					item.PositionBits = PositionBits - pos;
					ulong tmp;
					int len = item.ReadBits(out tmp, count);
					item.PositionBits = offset;

					bits <<= len;
					bits |= tmp;
					PositionBits += len;
					needed -= len;

					if (needed == 0)
						break;
				}

				pos = next;
			}

			return count - needed;
		}

		public override void SetLengthBits(long value)
		{
			throw new NotSupportedException("Stream does not support writing.");
		}

		public override void WriteBits(ulong bits, int count)
		{
			throw new NotSupportedException("Stream does not support writing.");
		}

		#endregion

		#region Stream Interface

		public override bool CanRead
		{
			get { return true; }
		}

		public override bool CanSeek
		{
			get { return true; }
		}

		public override bool CanWrite
		{
			get { return false; }
		}

		public override void Flush()
		{
		}

		public override long Length
		{
			get { return LengthBits / 8; }
		}

		public override long Position
		{
			get
			{
				return PositionBits / 8;
			}
			set
			{
				PositionBits = value * 8;
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

			int bits = 0;
			int needed = count;
			long pos = 0;
			ulong tmp = 0;
			byte glue = 0;

			foreach (var item in this)
			{
				long next = pos + item.LengthBits;

				if (next >= PositionBits)
				{
					long restore = item.PositionBits;
					item.PositionBits = PositionBits - pos;

					// If we are not aligned reading into buffer, get back aligned
					if (bits != 0)
					{
						int len = item.ReadBits(out tmp, 8 - bits);
						glue |= (byte)(tmp << (8 - bits - len));
						PositionBits += len;
						bits += len;

						// Advance offset once buffer is aligned again
						if (bits == 8)
						{
							buffer[offset] = glue;
							++offset;
							--needed;
							bits = 0;
							glue = 0;
						}
					}

					// If we are aligned, read directly into the buffer
					if (bits == 0)
					{
						int len = item.Read(buffer, offset, needed);

						offset += len;
						needed -= len;
						PositionBits += (len * 8);

						// Ensure we read any leftover bits
						if (needed > 0)
						{
							bits = item.ReadBits(out tmp, 7);
							glue = (byte)(tmp << (8 - bits));
							PositionBits += bits;
						}
					}

					item.PositionBits = restore;

					if (bits == 0 && needed == 0)
						break;
				}

				pos = next;
			}

			// If we have partial bits we failed to glue into a whole byte
			// we need to back up our position
			PositionBits -= bits;

			return count - needed;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return SeekBits(offset * 8, origin) / 8;
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException("Stream does not support writing.");
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException("Stream does not support writing.");
		}

		#endregion

		#region IList<BitStream> Members

		public int IndexOf(BitStream item)
		{
			return _streams.IndexOf(item);
		}

		public void Insert(int index, BitStream item)
		{
			_streams.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			_streams.RemoveAt(index);
		}

		public BitStream this[int index]
		{
			get
			{
				return _streams[index];
			}
			set
			{
				_streams[index] = value;
			}
		}

		public void Add(BitStream item)
		{
			_streams.Add(item);
		}

		public void Clear()
		{
			_streams.Clear();
		}

		public bool Contains(BitStream item)
		{
			return _streams.Contains(item);
		}

		public void CopyTo(BitStream[] array, int arrayIndex)
		{
			_streams.CopyTo(array, arrayIndex);
		}

		public int Count
		{
			get { return _streams.Count; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public bool Remove(BitStream item)
		{
			return _streams.Remove(item);
		}

		public IEnumerator<BitStream> GetEnumerator()
		{
			return _streams.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return _streams.GetEnumerator();
		}

		#endregion
	}
}
