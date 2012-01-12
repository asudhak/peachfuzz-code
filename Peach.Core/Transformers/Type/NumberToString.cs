using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Compression;
using System.IO;
using Peach.Core.Dom;
using Peach.Core.IO;

namespace Peach.Core.Transformers.Type
{
    [TransformerAttribute("NumberToString", "Transforms any type of number to a string.")]
    public class NumberToString : Transformer
    {
        public NumberToString(Dictionary<string,Variant> args) : base(args)
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
