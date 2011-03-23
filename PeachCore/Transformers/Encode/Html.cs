using System;
using System.Collections.Generic;
using System.Text;

namespace PeachCore.Transformers.Encode
{
    [TransformerAttribute("HtmlEncodeAgressive", "Encode on output as as HTML agressively.  Only alphanums will not be encoded.")]
    class HtmlEncodeAgressive :Transformer
    {
        public HtmlEncodeAgressive(Dictionary<string,string> args) : base(args)
		{
		}

        protected override BitStream internalEncode(BitStream data)
        {
            var s = System.Text.ASCIIEncoding.ASCII.GetString(data.Value);
            var es = System.Web.HttpUtility.HtmlEncode(s);

            return new BitStream(System.Text.ASCIIEncoding.ASCII.GetBytes(es));
        }

        protected override BitStream internalDecode(BitStream data)
        {
            var s = System.Text.ASCIIEncoding.ASCII.GetString(data.Value);
            var ds = System.Web.HttpUtility.HtmlDecode(s);

            return new BitStream(System.Text.ASCIIEncoding.ASCII.GetBytes(ds));
        }
    }
    [TransformerAttribute("HtmlEncode", "Encode on output as HTML (encoding < > & and \")")]
    class HtmlEncode : Transformer
    {
        public HtmlEncode(Dictionary<string,string> args) : base(args)
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
    [TransformerAttribute("JsEncode", "Encode on output as Javascript string.")]
    class JsEncode : Transformer
    {
        public JsEncode(Dictionary<string,string> args) : base(args)
		{
		}

        protected override BitStream internalEncode(BitStream data)
        {
            StringBuilder sb = new StringBuilder((int)data.LengthBytes);

            foreach (byte b in data.Value)
            {
                if ((b >= 97 && b <= 122) ||
                    (b >= 65 && b <= 90) ||
                    (b >= 48 && b <= 57) ||
                    b == 32 || b == 44 || b == 46)
                    sb.Append((Char)b);
                else if (b <= 127)
                    sb.AppendFormat("\\x{0:X2}",b);
                else
                    //NOTE: Doing at ASCII byte level.. might not not be necesarry here as the string is not typed...
                    sb.AppendFormat("\\u{0:X4}",b); 

            }
            
           return new  BitStream(System.Text.ASCIIEncoding.ASCII.GetBytes(sb.ToString()));

        }

        protected override BitStream internalDecode(BitStream data)
        {
            throw new NotImplementedException();
        }
    }
    [TransformerAttribute("HtmlDecode", "Decode on output from HTML encoding.")]
    class HtmlDecode : Transformer
    {
        public HtmlDecode(Dictionary<string, string> args)
            : base(args)
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
