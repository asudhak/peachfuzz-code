using System;
using System.Collections.Generic;
using System.Text;

namespace PeachCore.Transformers.Encode
{
    [TransformerAttribute("Base64Encode", "Encode on output as Base64.")]
    class Base64Encode : Transformer
    {
        public Base64Encode(Dictionary<string,string> args) : base(args)
		{
		}


        protected override BitStream internalEncode(BitStream data)
        {
            var b64s = System.Convert.ToBase64String(data.Value);
            var bytes = System.Text.ASCIIEncoding.ASCII.GetBytes(b64s);
            return new BitStream(bytes);
        }
        
        protected override BitStream internalDecode(BitStream data)
        {
            var b64s = System.Text.ASCIIEncoding.ASCII.GetString(data.Value);
            return new BitStream(System.Convert.FromBase64String(b64s));
        }
    }
    [TransformerAttribute("Base64Decode", "Decode on output from Base64.")]
    class Base64Decode : Transformer
    {
        public Base64Decode(Dictionary<string, string> args)
            : base(args)
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
