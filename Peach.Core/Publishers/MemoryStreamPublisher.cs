
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
using System.Collections.Generic;
using System.Text;
using Peach.Core.Dom;

namespace Peach.Core.Publishers
{
	[Publisher("MemoryStream", true)]
	[Publisher("MemoryStreamPublisher")]
	[ParameterAttribute("Stream", typeof(MemoryStream), "MemoryStream to receive or send data.", true)]
  [NotPitParsable]
	public class MemoryStreamPublisher : Publisher
	{
		protected Stream stream = null;

		public MemoryStreamPublisher(Dictionary<string, Variant> args)
			: base(args)
		{
			throw new PeachException("Error, MemoryStreamPublisher is for API use only.");
		}

		public MemoryStreamPublisher(MemoryStream stream) 
			: base(new Dictionary<string,Variant>())
		{
			this.stream = stream;
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
			if (stream == null)
				open(action);

			OnOutput(action, data);

			byte[] buff = (byte[])data;
			stream.Write(buff, 0, buff.Length);
		}

		#region Stream

		public override bool CanRead
		{
			get { return stream.CanRead; }
		}

		public override bool CanSeek
		{
			get { return stream.CanSeek; }
		}

		public override bool CanWrite
		{
			get { return stream.CanWrite; }
		}

		public override void Flush()
		{
			stream.Flush();
		}

		public override long Length
		{
			get { return stream.Length; }
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
			OnInput(currentAction, count);

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

		public override void Write(byte[] buffer, int offset, int count)
		{
			OnOutput(currentAction, new Variant(buffer));

			stream.Write(buffer, offset, count);
		}

		#endregion
	}
}

// END
