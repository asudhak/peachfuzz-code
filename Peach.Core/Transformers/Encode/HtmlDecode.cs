using System;
using System.Collections.Generic;
using System.Text;
using Peach.Core.IO;
using Peach.Core.Dom;

namespace Peach.Core.Transformers.Encode
{
    [Transformer("HtmlDecode", "Decode on output from HTML encoding.")]
    [Transformer("encode.HtmlDecode", "Decode on output from HTML encoding.")]
    [Serializable]
    public class HtmlDecode : Transformer
    {
        public HtmlDecode(Dictionary<string, Variant> args) : base(args)
        {
        }

        protected override BitStream internalEncode(BitStream data)
        {
            var s = System.Text.ASCIIEncoding.ASCII.GetString(data.Value);
            var ds = System.Web.HttpUtility.HtmlDecode(s);

            return new BitStream(System.Text.ASCIIEncoding.ASCII.GetBytes(ds));
        }

        protected override BitStream internalDecode(BitStream data)
        {
            var s = System.Text.ASCIIEncoding.ASCII.GetString(data.Value);
            var es = System.Web.HttpUtility.HtmlAttributeEncode(s);

            return new BitStream(System.Text.ASCIIEncoding.ASCII.GetBytes(es));
        }
    }
}

// end
