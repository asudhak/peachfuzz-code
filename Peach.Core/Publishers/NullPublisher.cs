using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Peach.Core.Publishers
{
	[Publisher("Null", true)]
	[NoParametersAttribute()]
	public class NullPublisher : Publisher
	{
		public NullPublisher(Dictionary<string, Variant> args)
			: base(args)
		{
		}

		public override void open(Core.Dom.Action action)
		{
			OnOpen(action);
		}

		public override void close(Core.Dom.Action action)
		{
			OnClose(action);
		}

		public override void output(Core.Dom.Action action, Variant data)
		{
			OnOutput(action, data);
		}

		#region Stream

		public override bool CanRead
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override bool CanSeek
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override bool CanWrite
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override void Flush()
		{
			throw new NotImplementedException();
		}

		public override long Length
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override long Position
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotImplementedException();
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
