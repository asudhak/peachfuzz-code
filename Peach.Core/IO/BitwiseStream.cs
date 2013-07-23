using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Peach.Core.IO
{
	[Serializable]
	public abstract class BitwiseStream : Stream
	{
		#region Const Members

		public const int BlockCopySize = 4 * 1024 * 1024;

		#endregion

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

		#region Element Positions

		public string Name { get; set; }

		public virtual bool TryGetPosition(string name, out long position)
		{
			throw new NotSupportedException("Stream does not support element positions.");
		}

		#endregion

		#region Stream Specializations

		public void CopyTo(BitwiseStream destination)
		{
			CopyTo(destination, BlockCopySize);
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

		#region Helpers

		/// <summary>
		/// Reports the position of the first occurrence of the specified BitStream in this
		/// instance. The search starts at a specified BitStream position.
		/// </summary>
		/// <param name="value">The BitStream to seek.</param>
		/// <param name="offsetBits">The search starting position.</param>
		/// <returns>
		/// The zero-based index position of value if that BitStream is found, or -1 if it is not.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="value"/> is <null/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="offsetBits"/> specifies a position not within this instance.</exception>
		public long IndexOf(BitwiseStream value, long offsetBits)
		{
			if (value == null)
				throw new ArgumentNullException("value");
			if (value.LengthBits % 8 != 0)
				throw new ArgumentOutOfRangeException("value");
			if (offsetBits < 0 || offsetBits > LengthBits)
				throw new ArgumentOutOfRangeException("offsetBits");

			var needle = new byte[value.Length];
			long pos = value.PositionBits;
			value.SeekBits(0, SeekOrigin.Begin);
			int len = value.Read(needle, 0, needle.Length);
			System.Diagnostics.Debug.Assert(len == needle.Length);
			value.SeekBits(pos, SeekOrigin.Begin);

			pos = PositionBits;
			SeekBits(offsetBits, SeekOrigin.Begin);

			int idx = 0;
			long ret = -1;
			long end = (LengthBits - PositionBits) / 8;

			for (long i = 0; i < end; ++i)
			{
				int b = ReadByte();

				if (b != needle[idx])
				{
					SeekBits(idx * -8, SeekOrigin.Current);
					i -= idx;
				}
				else if (++idx == needle.Length)
				{
					ret = 8 * (i - idx + 1) + offsetBits;
					break;
				}
			}

			SeekBits(pos, SeekOrigin.Begin);
			return ret;
		}

		/// <summary>
		/// Reads the next bit from the BitwiseStream.
		/// </summary>
		/// <returns>Returns the next bit, or -1 if there are no bits left.</returns>
		public int ReadBit()
		{
			ulong bits;
			int len = ReadBits(out bits, 1);

			if (len == 0)
				return -1;

			return (int)bits;
		}

		/// <summary>
		/// Write a bit into the BitwiseStream
		/// </summary>
		/// <param name="value">The bit value to write.</param>
		public void WriteBit(int value)
		{
			if (value != 0 && value != 1)
				throw new ArgumentOutOfRangeException("value");

			WriteBits((ulong)value, 1);
		}

		#endregion
	}
}
