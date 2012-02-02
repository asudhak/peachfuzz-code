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
    [TransformerAttribute("Bz2Decompress", "Decompress on output using bz2.")]
    [TransformerAttribute("compress.Bz2Decompress", "Decompress on output using bz2.")]
    [Serializable]
    public class Bz2Decompress : Transformer
    {
        public Bz2Decompress(Dictionary<string, Variant> args) : base(args)
        {
        }

        protected override BitStream internalEncode(BitStream data)
        {
            byte[] buff = new byte[1024];
            int ret;

            MemoryStream sin = new MemoryStream(data.Value);
            MemoryStream sout = new MemoryStream();

            BZip2InputStream bzip2 = new BZip2InputStream(sin);

            do
            {
                ret = bzip2.Read(buff, 0, buff.Length);
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

            BZip2OutputStream bzip2 = new BZip2OutputStream(sout);

            do
            {
                ret = sin.Read(buff, 0, buff.Length);
                bzip2.Write(buff, 0, ret);
            }
            while (ret != 0);

            return new BitStream(bzip2);
        }
    }
}

// end
