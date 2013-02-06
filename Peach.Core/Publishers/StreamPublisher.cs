using System;
using System.Collections.Generic;
using System.IO;

namespace Peach.Core.Publishers
{
	/// <summary>
	/// Helper class for creating stream based publishers.
	/// Derived classes should only need to override OnOpen and OnClose
	/// </summary>
	public abstract class StreamPublisher : Publisher
	{
		protected Stream stream = null;

		public StreamPublisher(Dictionary<string, Variant> args)
			: base(args)
		{
		}

		protected override void OnInput()
		{
		}

		protected override void OnOutput(byte[] buffer, int offset, int count)
		{
			stream.Write(buffer, offset, count);
		}

		#region Stream

		public override bool CanRead
		{
			get
			{
				return stream.CanRead;
			}
		}

		public override bool CanSeek
		{
			get
			{
				return stream.CanSeek;
			}
		}

		public override void Flush()
		{
			stream.Flush();
		}

		public override long Length
		{
			get
			{
				return stream.Length;
			}
		}

		public override long Position
		{
			get
			{
				return stream.Position;
			}
			set
			{
				stream.Position = value;
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return stream.Read(buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return stream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			stream.SetLength(value);
		}

		#endregion

	}
}
