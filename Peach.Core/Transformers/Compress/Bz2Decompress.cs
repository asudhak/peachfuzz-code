
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

		protected override BitwiseStream internalEncode(BitwiseStream data)
		{
			BitStream ret = new BitStream();

			using (var bzip2 = new BZip2InputStream(data, true))
			{
				try
				{
					// For some reason, Ionic decided the BZip2InputStream
					// should return -1 from Read() when EOF is reached.  This
					// breaks Stream.CopyTo() as it expects 0 on EOF.
					// We need to use ReadByte() instead.
					int val;
					while ((val = bzip2.ReadByte()) != -1)
						ret.WriteByte((byte)val);
				}
				catch (Exception ex)
				{
					throw new SoftException("Could not BZip decompress data.", ex);
				}
			}

			ret.Seek(0, SeekOrigin.Begin);
			return ret;
		}

		protected override BitStream internalDecode(BitStream data)
		{
			BitStream ret = new BitStream();

			using (var bzip2 = new BZip2OutputStream(ret, true))
			{
				data.CopyTo(bzip2);
			}

			ret.Seek(0, SeekOrigin.Begin);
			return ret;
		}
	}
}

// end
