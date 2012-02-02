using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Compression;
using System.IO;
using Peach.Core.Dom;
using Peach.Core.IO;

namespace Peach.Core.Transformers.Compress
{
    [TransformerAttribute("GzipDecompress", "Decompress on output using gzip.")]
    [TransformerAttribute("compress.GzipDecompress", "Decompress on output using gzip.")]
    [Serializable]
    public class GzipDecompress : Transformer
    {
        public GzipDecompress(Dictionary<string, Variant> args) : base(args)
        {
        }

        protected override BitStream internalEncode(BitStream data)
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

        protected override BitStream internalDecode(BitStream data)
        {
            byte[] buff = new byte[1024];
            int ret;

            MemoryStream sin = new MemoryStream(data.Value);
            MemoryStream sout = new MemoryStream();

            GZipStream gzip = new GZipStream(sin, CompressionMode.Compress);

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
