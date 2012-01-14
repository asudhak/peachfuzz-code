using System;
using System.Collections.Generic;
using System.Text;
using Peach.Core.Dom;
using Peach.Core.IO;

namespace Peach.Core.Transformers.Encode
{
    [TransformerAttribute("UrlEncodePlus", "Encode on output as a URL with spaces turned to pluses.")]
    [TransformerAttribute("encode.UrlEncodePlus", "Encode on output as a URL with spaces turned to pluses.")]
    public class UrlEncodePlus : Transformer
    {
        public UrlEncodePlus(Dictionary<string, Variant> args) : base(args)
        {
        }

        protected override BitStream internalEncode(BitStream data)
        {
            return new BitStream(System.Web.HttpUtility.UrlEncodeToBytes(data.Value));
        }

        protected override BitStream internalDecode(BitStream data)
        {
            return new BitStream(System.Web.HttpUtility.UrlDecodeToBytes(data.Value));
        }
    }
}

// end
