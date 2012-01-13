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
