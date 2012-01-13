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
    [TransformerAttribute("UnsignedNumberToString", "Transforms unsigned numbers to strings.")]
    public class UnsignedNumberToString : Transformer
    {
        public UnsignedNumberToString(Dictionary<string,Variant> args) : base(args)
		{
		}

		protected override BitStream internalEncode(BitStream data)
		{
            StringBuilder sb = new StringBuilder((int)data.LengthBytes * 2);

            foreach (byte b in data.Value)
                sb.Append(b);

            return new BitStream(ASCIIEncoding.ASCII.GetBytes(sb.ToString()));
		}

		protected override BitStream internalDecode(BitStream data)
		{
            throw new NotImplementedException();
		}
    }
}

// end
