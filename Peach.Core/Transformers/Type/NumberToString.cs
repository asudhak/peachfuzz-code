using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Compression;
using System.IO;
using Peach.Core.Dom;
using Peach.Core.IO;

namespace Peach.Core.Transformers.Type
{
    [TransformerAttribute("NumberToString", "Transforms any type of number to a string.")]
    [TransformerAttribute("type.NumberToString", "Transforms any type of number to a string.")]
    [Serializable]
    public class NumberToString : Transformer
    {
        public NumberToString(Dictionary<string,Variant> args) : base(args)
		{
		}

		protected override BitStream internalEncode(BitStream data)
		{
            string dataAsStr = ASCIIEncoding.ASCII.GetString(data.Value);
            return new BitStream(Encoding.ASCII.GetBytes(dataAsStr));
		}

		protected override BitStream internalDecode(BitStream data)
		{
            throw new NotImplementedException();
		}
    }
}

// end
