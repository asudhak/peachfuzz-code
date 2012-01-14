using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Peach.Core.Dom;
using Peach.Core.IO;

namespace Peach.Core.Transformers.Encode
{
    [Serializable]
    [TransformerAttribute("HexString", "Transforms a string of bytes into the specified hex format.")]
    [TransformerAttribute("encode.HexString", "Transforms a string of bytes into the specified hex format.")]
    [Parameter("resolution", typeof(int), "Number of nibbles between separator. (Must be a positive, even int.)", false)]
    [Parameter("prefix", typeof(string), "A value to prepend each chunk with. (defaults to ' ')", false)]
    public class HexString : Transformer
    {
        Dictionary<string, Variant> m_args;

        public HexString(Dictionary<string, Variant> args) : base(args)
        {
            m_args = args;
        }

        protected override BitStream internalEncode(BitStream data)
        {
            int resolution = 1;
            string prefix = " ";
            if (m_args.ContainsKey("resolution"))
                resolution = Int32.Parse((string)m_args["resolution"]);

            if (resolution % 2 != 0 && resolution != 1)
                throw new Exception("HexString transformer internalEncode failed: Resolution must be 1 or a multiple of two.");

            if (data.LengthBytes % resolution != 0)
                throw new Exception("HexString transformer internalEncode failed: Data length must be divisible by resolution.");

            if (m_args.ContainsKey("prefix"))
                prefix = (string)m_args["prefix"];

            var ret = new System.Text.StringBuilder();
            var tmp = new System.Text.StringBuilder();

            foreach (byte b in data.Value)
            {
                tmp.AppendFormat("{0:x2}", b);

                if (tmp.Length / 2 == resolution)
                {
                    ret.Append(tmp.ToString());
                    tmp = new StringBuilder();
                }
            }


            var rets = ret.ToString().Trim();

            return new BitStream(System.Text.ASCIIEncoding.ASCII.GetBytes(rets));
        }

        protected override BitStream internalDecode(BitStream data)
        {
            throw new NotImplementedException();
        }
    }
}

// end
