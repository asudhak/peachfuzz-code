using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Compression;
using System.IO;
using Peach.Core.Dom;
using Peach.Core.IO;
using Ionic.BZip2;

namespace Peach.Core.Transformers
{
    [TransformerAttribute("Bz2Compress", "Compress on output using bz2.")]
    public class Bz2Compress : Transformer
    {
        public Bz2Compress(Dictionary<string,Variant> args) : base(args)
		{
		}

		protected override BitStream internalEncode(BitStream data)
		{
			byte[] buff = new byte[1024];
			int ret;

			MemoryStream sin = new MemoryStream(data.Value);
			MemoryStream sout = new MemoryStream();

            return new BitStream();
		}

		protected override BitStream internalDecode(BitStream data)
		{
			byte[] buff = new byte[1024];
			int ret;

			MemoryStream sin = new MemoryStream(data.Value);
			MemoryStream sout = new MemoryStream();

            return new BitStream();
		}
    }

    [TransformerAttribute("Bz2Decompress", "Decompress on output using bz2.")]
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

            return new BitStream();
		}

		protected override BitStream internalDecode(BitStream data)
		{
			byte[] buff = new byte[1024];
			int ret;

			MemoryStream sin = new MemoryStream(data.Value);
			MemoryStream sout = new MemoryStream();

            return new BitStream();
		}
    }
}

// end
