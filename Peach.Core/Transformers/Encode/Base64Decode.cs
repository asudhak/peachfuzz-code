using System;
using System.Collections.Generic;
using System.Text;
using Peach.Core.Dom;
using Peach.Core.IO;

namespace Peach.Core.Transformers.Encode
{
    [TransformerAttribute("Base64Decode", "Decode on output from Base64.")]
    [TransformerAttribute("encode.Base64Decode", "Decode on output from Base64.")]
    public class Base64Decode : Transformer
    {
        public Base64Decode(Dictionary<string, Variant> args) : base(args)
        {
        }

        protected override BitStream internalEncode(BitStream data)
        {
            var b64s = System.Text.ASCIIEncoding.ASCII.GetString(data.Value);
            return new BitStream(System.Convert.FromBase64String(b64s));
        }

        protected override BitStream internalDecode(BitStream data)
        {
            var b64s = System.Convert.ToBase64String(data.Value);
            var bytes = System.Text.ASCIIEncoding.ASCII.GetBytes(b64s);
            return new BitStream(bytes);
        }
    }
}

// end
