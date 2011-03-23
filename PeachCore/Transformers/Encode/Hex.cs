using System;
using System.Collections.Generic;
using System.Text;

namespace PeachCore.Transformers.Encode
{
    [TransformerAttribute("Hex", "Encode on output as a hex string.")]
    class Hex : Transformer
    {
        public Hex(Dictionary<string,string> args) : base(args)
		{
		}

        protected override BitStream internalEncode(BitStream data)
        {
            StringBuilder sb = new StringBuilder(((int)data.LengthBytes*2));
            foreach (byte b in data.Value)
                sb.AppendFormat("{0:x2}", b);

            return new BitStream(System.Text.ASCIIEncoding.ASCII.GetBytes(sb.ToString()));
        }

        protected override BitStream internalDecode(BitStream data)
        {
            

            if (data.Value.Length % 2 != 0)
                //TODO: Transformer soft exception?
                throw new Exception("Hex transfromer internalDecode failed: Invalid length.");

            byte[] ret = new byte[data.LengthBytes/2];

            for (ulong i = 0; i < data.LengthBytes; i += 2)
            {
                int high = (data.Value[i] > 0x40 ? data.Value[i] - 0x37 : data.Value[i] - 0x30) << 4;
                int low = data.Value[i + 1] > 0x40 ? data.Value[i + 1] - 0x37 : data.Value[i] - 0x30;

                ret[i/2] = Convert.ToByte(high + low);

            }

            return new BitStream(ret);
        }
    }


    [Parameter("resolution", typeof(int), "Number of nibbles between separator. (Must be a positive, even int.)", false)]
    [Parameter("prefix", typeof(string), "A value to prepend each chunk with. (defaults to ' ')", false)]
    [TransformerAttribute("HexString", "Transforms a string of bytes into the specified hex format.")]
    class HexString : Transformer
    {
        Dictionary<string, string> m_args;
        public HexString(Dictionary<string, string> args)
            : base(args)
		{
            m_args = args;
		}

        protected override BitStream internalEncode(BitStream data)
        {
            int resolution = 1;
            string prefix = " ";
            if (m_args.ContainsKey("resolution"))
                resolution = Int32.Parse(m_args["resolution"]);

            if (resolution % 2 != 0 && resolution != 1)
                throw new Exception("HexString transformer internalEncode failed: Resolution must be 1 or a multiple of two.");

            if(data.LengthBytes % (ulong)resolution != 0)
                throw new Exception("HexString transformer internalEncode failed: Data length must be divisible by resolution.");

            if (m_args.ContainsKey("prefix"))
                prefix = m_args["prefix"];

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
