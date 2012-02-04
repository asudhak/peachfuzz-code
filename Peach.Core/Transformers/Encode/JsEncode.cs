using System;
using System.Collections.Generic;
using System.Text;
using Peach.Core.Dom;
using Peach.Core.IO;

namespace Peach.Core.Transformers.Encode
{
    [Transformer("JsEncode", "Encode on output as Javascript string.")]
    [Transformer("encode.JsEncode", "Encode on output as Javascript string.")]
    [Serializable]
    class JsEncode : Transformer
    {
        public JsEncode(Dictionary<string, Variant> args) : base(args)
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
                    sb.AppendFormat("\\x{0:X2}", b);
                else
                    //NOTE: Doing at ASCII byte level.. might not not be necesarry here as the string is not typed...
                    sb.AppendFormat("\\u{0:X4}", b);

            }

            return new BitStream(System.Text.ASCIIEncoding.ASCII.GetBytes(sb.ToString()));
        }

        protected override BitStream internalDecode(BitStream data)
        {
            throw new NotImplementedException();
        }
    }
}

// end
