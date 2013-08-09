
//
// Copyright (c) Michael Eddington
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in	
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

// Authors:
//   Michael Eddington (mike@dejavusecurity.com)

// $Id$


using System;
using System.IO;
using System.Diagnostics;

namespace Peach.Core.IO
{
	[Serializable]
	[DebuggerDisplay("{Progress}")]
	public class BitStream : BitwiseStream
	{
		#region Private Members

		private Stream _stream;
		private long _position;
		private long _length;
		private long _offset;
		private bool _canWrite;

		#endregion

		#region Constructor

		public BitStream()
			: this(new MemoryStream())
		{
		}

		public BitStream(byte[] buffer)
			: this(new MemoryStream())
		{
			Write(buffer, 0, buffer.Length);
			Seek(0, SeekOrigin.Begin);
		}

		public BitStream(Stream stream)
			: this(stream, stream.Position * 8, stream.Length * 8, 0, true)
		{
		}

		private BitStream(Stream stream, long position, long length, long offset, bool canWrite)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");

			_stream = stream;
			_position = position;
			_length = length;
			_offset = offset;
			_canWrite = canWrite;
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

		public Stream BaseStream
		{
			get
			{
				return _stream;
			}
		}

		public string Progress
		{
			get
			{
				return "Bytes: {0}/{1}, Bits: {2}/{3}".Fmt(Position, Length, PositionBits, LengthBits);
			}
		}

		public void WantBytes(long bytes)
		{
			// If we are a slice, out length is fixed and can't change
			if (bytes <= 0 || !_canWrite)
				return;

			Publisher pub = _stream as Publisher;
			if (pub != null)
			{
				pub.WantBytes(bytes);
				_length = pub.Length * 8;
			}
		}

		public BitStream SliceBits(long length)
		{
			if (length < 0 || (_position + length) > _length)
				throw new ArgumentOutOfRangeException("lengthInBits");

			var ret = new BitStream(_stream, 0, length, _position + _offset, false);

			SeekBits(length, SeekOrigin.Current);

			return ret;
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
			get { return _canWrite && _stream.CanWrite; }
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

				_position = value * 8;
				_stream.Position = value;
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

			int pos = (int)((_position + _offset) & 0x7);

			// Ensure stream is in the right place
			_stream.Seek((_position + _offset) / 8, SeekOrigin.Begin);

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

			_stream.Seek((_offset + pos) / 8, SeekOrigin.Begin);

			return pos / 8;
		}

		public override void SetLength(long value)
		{
			if (!CanWrite)
				throw new NotSupportedException("Stream does not support writing.");

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

			if (!CanWrite)
				throw new NotSupportedException("Stream does not support writing.");

			if (count == 0)
				return;

			int pos = (int)(_position & 0x7);

			// Ensure stream is in the right place
			_stream.Seek((_position + _offset) / 8, SeekOrigin.Begin);

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
					next = (byte)((buffer[i] << shift) | (buffer[i + 1] >> pos));
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

			int pos = (int)((_position + _offset) & 0x7);
			int avail = (int)Math.Min(_length - _position, count);
			int remain = avail;

			// Ensure stream is in the right place
			_stream.Seek((_position + _offset) / 8, SeekOrigin.Begin);

			while (remain > 0)
			{
				int len = Math.Min(8 - pos, remain);
				int cur = _stream.ReadByte();
				System.Diagnostics.Debug.Assert(cur != -1);

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

			// Ensure stream is in the right place
			_stream.Seek((_position + _offset) / 8, SeekOrigin.Begin);

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

			_stream.Seek((_offset + pos) / 8, SeekOrigin.Begin);

			return pos;
		}

		public override void SetLengthBits(long value)
		{
			if (!CanWrite)
				throw new NotSupportedException("Stream does not support writing.");

			_stream.SetLength((value + 7) / 8);
			_length = value;

			if (_position > _length)
			{
				_position = value;
				_stream.Position = _position / 8;
			}
		}

		public override void WriteBits(ulong bits, int count)
		{
			if (count > 64 || count < 0)
				throw new ArgumentOutOfRangeException("count");

			if (count < 64 && bits >= ((ulong)1 << count))
				throw new ArgumentOutOfRangeException("bits");

			if (!CanWrite)
				throw new NotSupportedException("Stream does not support writing.");

			if (count == 0)
				return;

			int pos = (int)(_position & 0x7);
			int remain = count;

			// Ensure stream is in the right place
			_stream.Seek((_position + _offset) / 8, SeekOrigin.Begin);

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

			// Ensure stream is in the right place
			_stream.Seek((_position + _offset) / 8, SeekOrigin.Begin);

			if (_position > _length)
				_length = _position;
		}

		private static readonly byte[] KeepMask = new byte[] { 0x00, 0x80, 0xc0, 0xe0, 0xf0, 0xf8, 0xfc, 0xfe };
		private static readonly byte[] BitsMask = new byte[] { 0x00, 0x01, 0x03, 0x07, 0x0f, 0x1f, 0x3f, 0x7f, 0xff };

		#endregion
	}
}
