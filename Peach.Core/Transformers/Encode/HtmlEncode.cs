using System;
using System.Collections.Generic;
using System.Text;
using Peach.Core.Dom;
using Peach.Core.IO;

namespace Peach.Core.Transformers.Encode
{
    [Transformer("HtmlEncode", "Encode on output as HTML (encoding < > & and \")")]
    [Transformer("encode.HtmlEncode", "Encode on output as HTML (encoding < > & and \")")]
    public class HtmlEncode : Transformer
    {
        public HtmlEncode(Dictionary<string, Variant> args) : base(args)
        {
        }

        protected override BitStream internalEncode(BitStream data)
        {
            var s = System.Text.ASCIIEncoding.ASCII.GetString(data.Value);
            var es = System.Web.HttpUtility.HtmlAttributeEncode(s);

            return new BitStream(System.Text.ASCIIEncoding.ASCII.GetBytes(es));
        }

        protected override BitStream internalDecode(BitStream data)
        {
            var s = System.Text.ASCIIEncoding.ASCII.GetString(data.Value);
            var ds = System.Web.HttpUtility.HtmlDecode(s);

            return new BitStream(System.Text.ASCIIEncoding.ASCII.GetBytes(ds));
        }
    }
}

// end
