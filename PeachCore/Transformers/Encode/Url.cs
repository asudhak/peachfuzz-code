using System;
using System.Collections.Generic;
using System.Text;

namespace PeachCore.Transformers.Encode
{
    [TransformerAttribute("UrlEncode", "Encode on output as a URL without pluses.")]
    class UrlEncode : Transformer
    {
        public UrlEncode(Dictionary<string,string> args) : base(args)
		{
		}

        protected override BitStream internalEncode(BitStream data)
        {
            string dataString = System.Text.ASCIIEncoding.ASCII.GetString(data.Value);
            string ue = System.Web.HttpUtility.UrlPathEncode(dataString);

            return new BitStream(System.Text.ASCIIEncoding.ASCII.GetBytes(ue));
        }

        protected override BitStream internalDecode(BitStream data)
        {
            return new BitStream(System.Web.HttpUtility.UrlDecodeToBytes(data.Value));
        }
    }
     [TransformerAttribute("UrlEncodePlus", "Encode on output as a URL with spaces turned to pluses.")]
    class UrlEncodePlus : Transformer
    {
         public UrlEncodePlus(Dictionary<string, string> args)
             : base(args)
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
