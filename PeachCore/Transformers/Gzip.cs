
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
//   Michael Eddington (mike@phed.org)

// $Id$

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Compression;
using System.IO;

namespace PeachCore.Transformers
{
	[TransformerAttribute("GzipCompress", "Compress on output using gzip.")]
	public class GzipCompress : Transformer
	{
		protected override PeachCore.Dom.Variant internalEncode(PeachCore.Dom.Variant data)
		{
			byte[] buff = new byte[1024];
			uint ret;

			MemoryStream sin = new MemoryStream((byte[])data);
			MemoryStream sout = new MemoryStream();

			GZipStream gzip = new GZipStream(data, CompressionMode.Compress);

			do
			{
				ret = gzip.Read(buff, 0, buff.Length);
				sout.Write(buff, 0, ret);
			}
			while (ret != 0);

			return sout.ToArray();
		}

		protected override PeachCore.Dom.Variant internalDecode(PeachCore.Dom.Variant data)
		{
			byte[] buff = new byte[1024];
			uint ret;

			MemoryStream sin = new MemoryStream((byte[])data);
			MemoryStream sout = new MemoryStream();

			GZipStream gzip = new GZipStream(data, CompressionMode.Decompress);

			do
			{
				ret = gzip.Read(buff, 0, buff.Length);
				sout.Write(buff, 0, ret);
			}
			while (ret != 0);

			return sout.ToArray();
		}
	}

	[TransformerAttribute("GzipDecompress", "Decompress on output using gzip.")]
	public class GzipCompress : Transformer
	{
		protected override PeachCore.Dom.Variant internalEncode(PeachCore.Dom.Variant data)
		{
			byte[] buff = new byte[1024];
			uint ret;

			MemoryStream sin = new MemoryStream((byte[])data);
			MemoryStream sout = new MemoryStream();

			GZipStream gzip = new GZipStream(data, CompressionMode.Decompress);

			do
			{
				ret = gzip.Read(buff, 0, buff.Length);
				sout.Write(buff, 0, ret);
			}
			while (ret != 0);

			return sout.ToArray();
		}

		protected override PeachCore.Dom.Variant internalDecode(PeachCore.Dom.Variant data)
		{
			byte[] buff = new byte[1024];
			uint ret;

			MemoryStream sin = new MemoryStream((byte[])data);
			MemoryStream sout = new MemoryStream();

			GZipStream gzip = new GZipStream(data, CompressionMode.Compress);

			do
			{
				ret = gzip.Read(buff, 0, buff.Length);
				sout.Write(buff, 0, ret);
			}
			while (ret != 0);

			return sout.ToArray();
		}
	}
}

// end
