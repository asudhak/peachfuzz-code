
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

using NLog;

namespace Peach.Core.Transformers.Compress
{
	[Description("Decompress on output using gzip.")]
	[Transformer("GzipDecompress", true)]
	[Transformer("compress.GzipDecompress")]
	[Serializable]
	public class GzipDecompress : Transformer
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		public GzipDecompress(Dictionary<string, Variant> args)
			: base(args)
		{
		}

		protected override BitStream internalEncode(BitStream compressedData)
		{
			logger.Debug("internalDecode");

			var data = new MemoryStream();
			compressedData.SeekBits(0, SeekOrigin.Begin);

            try
            {
                using (GZipStream compressionStream = new GZipStream(compressedData.Stream, CompressionMode.Decompress))
                {
                    compressionStream.CopyTo(data);
                }

                return new BitStream(data.ToArray());
            }
            catch (Exception ex)
            {
                throw new PeachException("Error, could not GZip decompress data", ex);
            }
		}

		protected override BitStream internalDecode(BitStream data)
		{
			logger.Debug("internalDecode");

			var compressedData = new MemoryStream();
			data.SeekBits(0, SeekOrigin.Begin);

            try
            {
                using (GZipStream compressionStream = new GZipStream(compressedData, CompressionMode.Compress))
                {
                    data.Stream.CopyTo(compressionStream);
                }

                return new BitStream(compressedData.ToArray());
            }
            catch(Exception ex)
            {
               throw new PeachException("Error, could not GZip dceompress data", ex); 
            }
		}
	}
}

// end
