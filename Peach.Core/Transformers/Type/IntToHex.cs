using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Compression;
using System.IO;
using Peach.Core.Dom;
using Peach.Core.IO;

namespace Peach.Core.Transformers.Type
{
    [TransformerAttribute("IntToHex", "Transforms an integer into hex.")]
    [TransformerAttribute("type.IntToHex", "Transforms an integer into hex.")]
    public class IntToHex : Transformer
    {
        public IntToHex(Dictionary<string,Variant> args) : base(args)
		{
		}

		protected override BitStream internalEncode(BitStream data)
		{
            StringBuilder sb = new StringBuilder(((int)data.LengthBytes * 2));

            foreach (byte b in data.Value)
                sb.AppendFormat("{0:x2}", b);

            return new BitStream(System.Text.ASCIIEncoding.ASCII.GetBytes(sb.ToString()));
		}

		protected override BitStream internalDecode(BitStream data)
		{
            throw new NotImplementedException();
		}
    }
}

// end
