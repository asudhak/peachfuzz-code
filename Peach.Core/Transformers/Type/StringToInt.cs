using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Compression;
using System.IO;
using Peach.Core.Dom;
using Peach.Core.IO;

namespace Peach.Core.Transformers.Type
{
    [TransformerAttribute("StringToInt", "Transforms a string into an integer.")]
    [TransformerAttribute("encode.StringToInt", "Transforms a string into an integer.")]
    [Serializable]
    public class StringToInt : Transformer
    {
        public StringToInt(Dictionary<string,Variant> args) : base(args)
		{
		}

		protected override BitStream internalEncode(BitStream data)
		{
            string dataAsString = Encoding.ASCII.GetString(data.Value);
            Int32 dataAsInt32 = Int32.Parse(dataAsString);
            return new BitStream(BitConverter.GetBytes(dataAsInt32));
		}

		protected override BitStream internalDecode(BitStream data)
		{
            throw new NotImplementedException();
		}
    }
}

// end
