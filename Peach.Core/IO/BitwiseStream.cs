using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Peach.Core.IO.New
{
	public abstract class BitwiseStream : Stream
	{
		protected BitwiseStream() { }

		public abstract long LengthBits { get; }
		public abstract long PositionBits { get; set; }
		public abstract long SeekBits(long offset, SeekOrigin origin);
		public abstract int ReadBits(out ulong bits, int count);
		public abstract void SetLengthBits(long value);
		public abstract void WriteBits(ulong bits, int count);
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
			get { return (_length + 7) / 8; }
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

			int avail = (int)Math.Min((_length + 7 - _position) / 8, count);

			if (avail == 0)
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

					// If there is another byte, read the low bits
					if (_stream.Position < _stream.Length)
					{
						cur = _stream.ReadByte();
						System.Diagnostics.Debug.Assert(cur != -1);

						// Shift the low bits into place
						next |= (byte)((cur >> shift) & BitsMask[pos]);
					}

					// Save the combined byte
					buffer[i] = next;
				}
			}

			// If LengthBits=1, and we were asked to read a byte, we
			// need to mask off the unread low bits and ensure
			// our PositionBits matches our LengthBits
			_position += (avail * 8);
			if (_position > _length)
			{
				int bits = (int)(_position - _length);
				System.Diagnostics.Debug.Assert(bits < 8);
				buffer[offset + avail - 1] &= KeepMask[8 - bits];
				_position = _length;
			}

			// If our position is not aligned, we need to back up
			// the stream by a single byte so subsequent reads/writes work
			int remain = (int)(_position & 0x7);
			if (remain != 0)
				_stream.Seek(-1, SeekOrigin.Current);

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
				System.Diagnostics.Debug.Assert(cur != 1);
				_stream.Seek(-1, SeekOrigin.Current);
				cur &= KeepMask[pos];
				next |= (byte)cur;
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
			get { return (LengthBits + 7) / 8; }
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
						ulong tmp;
						int len = item.ReadBits(out tmp, 8 - bits);
						int shift = 8 - bits - len;
						buffer[offset] |= (byte)(tmp << shift);
						PositionBits += len;
						bits += len;

						// Advance offset once buffer is aligned again
						if (bits == 8)
						{
							++offset;
							--needed;
							bits = 0;
						}
					}

					// If we are aligned, read directly into the buffer
					if (bits == 0)
					{
						long start = item.PositionBits;
						int len = item.Read(buffer, offset, needed);

						long read = item.PositionBits - start;
						bits = (int)(read % 8);

						if (bits != 0)
							--len;

						offset += len;
						needed -= len;
						PositionBits += read;
					}

					item.PositionBits = restore;

					if (bits == 0 && needed == 0)
						break;
				}

				pos = next;
			}

			// If we have leftover bits at the end, we have written into the next byte
			if (bits > 0)
				--needed;

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
