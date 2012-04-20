
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

namespace Peach.Core.Transformers.Compress
{
	[TransformerAttribute("GzipCompress", "Compress on output using gzip.")]
    [TransformerAttribute("compress.GzipCompress", "Compress on output using gzip.")]
    [Serializable]
	public class GzipCompress : Transformer
	{
		public GzipCompress(Dictionary<string,Variant> args) : base(args)
		{
		}

		protected override BitStream internalEncode(BitStream data)
		{
            //byte[] buff = new byte[1024];
            //int ret;

            //MemoryStream sin = new MemoryStream(data.Value);
            //MemoryStream sout = new MemoryStream();

            //GZipStream gzip = new GZipStream(sout, CompressionMode.Compress);

            //do
            //{
            //    ret = sin.Read(buff, 0, buff.Length);
            //    gzip.Write(buff, 0, ret);
            //}
            //while (ret != 0);

            //return new BitStream(sout.ToArray());

            //MemoryStream sin = new MemoryStream(data.Value);

            //MemoryStream sout = new MemoryStream();
            //GZipStream zs = new GZipStream(sout, CompressionMode.Compress, true);

            //zs.Write(data.Value, 0, data.Value.Length);

            //byte[] ret;

            string dataAsStrASCII = ASCIIEncoding.ASCII.GetString(data.Value);
            byte[] dataAsStrUTF8Buff = Encoding.UTF8.GetBytes(dataAsStrASCII);

            MemoryStream ms = new MemoryStream();
            
            using (GZipStream gzip = new GZipStream(ms, CompressionMode.Compress))
            {
                gzip.Write(data.Value, 0, data.Value.Length);
            }

            //ret = ms.ToArray();

            //ms.Position = 0;
            var compressedData = new byte[ms.Length];
            ms.Read(compressedData, 0, compressedData.Length);

            var gZipBuffer = new byte[compressedData.Length + 4];
            Buffer.BlockCopy(compressedData, 0, gZipBuffer, 4, compressedData.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(data.Value.Length), 0, gZipBuffer, 0, 4);
            var x = Convert.ToString(gZipBuffer);
            var xx = Convert.ToBase64String(gZipBuffer);

            return new BitStream();
            //return new BitStream(sout.ToArray());
		}

		protected override BitStream internalDecode(BitStream data)
		{
			byte[] buff = new byte[1024];
			int ret;

			MemoryStream sin = new MemoryStream(data.Value);
			MemoryStream sout = new MemoryStream();

			GZipStream gzip = new GZipStream(sin, CompressionMode.Decompress);

			do
			{
				ret = gzip.Read(buff, 0, buff.Length);
				sout.Write(buff, 0, ret);
			}
			while (ret != 0);

			return new BitStream(sout.ToArray());
		}
	}
}

// end
