
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
using System.Collections.Generic;
using System.Text;
using System.IO.Compression;
using System.IO;
using Peach.Core.Dom;
using Peach.Core.IO;
using Ionic.BZip2;

namespace Peach.Core.Transformers.Compress
{
	[Description("Decompress on output using bz2.")]
	[Transformer("Bz2Decompress", true)]
	[Transformer("compress.Bz2Decompress")]
	[Serializable]
	public class Bz2Decompress : Transformer
	{
		public Bz2Decompress(Dictionary<string, Variant> args)
			: base(args)
		{
		}

		protected override BitStream internalEncode(BitStream data)
		{
			MemoryStream sin = new MemoryStream((byte[])data.Value);
			MemoryStream sout = new MemoryStream();

			try
			{
				BZip2InputStream bzip2 = new BZip2InputStream(sin);
				bzip2.CopyTo(sout);
			}
			catch
			{
			}

			return new BitStream(sout.ToArray());
		}

		protected override BitStream internalDecode(BitStream data)
		{
			MemoryStream sin = new MemoryStream((byte[])data.Value);
			MemoryStream sout = new MemoryStream();
			BZip2OutputStream bzip2 = new BZip2OutputStream(sout);
			sin.CopyTo(bzip2);
			bzip2.Dispose();

			return new BitStream(sout.ToArray());
		}
	}
}

// end
