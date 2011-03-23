using System;
using System.Collections.Generic;
using System.Text;

namespace PeachCore.Transformers.Encode
{

    [Parameter("pad",typeof(bool),"Should the NetBios names be padded/trimmed to 32 bytes?",false)]
    [TransformerAttribute("NetBiosDecode", "Deocode on output from binary reprisentation of a NetBios name to a string.")]
    class NetBiosDecode : Transformer
    {
        Dictionary<string, string> m_args;
        public NetBiosDecode(Dictionary<string,string> args) : base(args)
		{
            m_args = args;
		}


        protected override BitStream internalEncode(BitStream data)
        {
            if (data.LengthBytes % 2 != 0)
                throw new Exception("NetBiosDecode transformer internalEncode failed: Length must be divisible by two.");

            var sb = new System.Text.StringBuilder((int)data.LengthBytes / 2);

            var nbs = System.Text.ASCIIEncoding.ASCII.GetString(data.Value);

            for(int i=0;i<nbs.Length;i+=2)
            {
                char c1 = nbs[i];
                char c2 = nbs[i + 1];

                var part1 = (c1 - 0x41) * 16;
                var part2 = (c2 - 0x41);

                sb.Append((Char)(part1 + part2));
            }

            return new BitStream(System.Text.ASCIIEncoding.ASCII.GetBytes(sb.ToString()));

        }

        protected override BitStream internalDecode(BitStream data)
        {

            string name = System.Text.ASCIIEncoding.ASCII.GetString(data.Value).ToUpper();
            var sb = new System.Text.StringBuilder(32);

            if (m_args.ContainsKey("pad") && Boolean.Parse(m_args["pad"]))
                while (name.Length < 16)
                    name += " ";

            if(name.Length > 16)
                name = name.Substring(0, 16);


            foreach (char c in name)
            {
                var ascii = (int)c;
                sb.Append((Char)((ascii / 16) + 0x41));
                sb.Append((Char)((ascii - (ascii/16 * 16) + 0x41)));
            }

            var sret = sb.ToString();

            if (m_args.ContainsKey("pad") && Boolean.Parse(m_args["pad"]))
            {
                if (sret.Length > 30)
                    sret = sret.Substring(0, 30);

                sret += "AA";
            }

            return new BitStream(System.Text.ASCIIEncoding.ASCII.GetBytes(sret));
        }
    }

    [Parameter("pad", typeof(bool), "Should the NetBios names be padded/trimmed to 32 bytes?", false)]
    [TransformerAttribute("NetBiosEncode", "Encode on output from a string to a binary NetBios reprisentation.")]
    class NetBiosEncode : Transformer
    {
        Dictionary<string, string> m_args;
        public NetBiosEncode(Dictionary<string, string> args)
            : base(args)
		{
            m_args = args;
		}

        protected override BitStream internalEncode(BitStream data)
        {
            string name = System.Text.ASCIIEncoding.ASCII.GetString(data.Value).ToUpper();
            var sb = new System.Text.StringBuilder(32);

            if (m_args.ContainsKey("pad") && Boolean.Parse(m_args["pad"]))
                while (name.Length < 16)
                    name += " ";

            if (name.Length > 16)
                name = name.Substring(0, 16);


            foreach (char c in name)
            {
                var ascii = (int)c;
                sb.Append((Char)((ascii / 16) + 0x41));
                sb.Append((Char)((ascii - (ascii / 16 * 16) + 0x41)));
            }

            var sret = sb.ToString();

            if (m_args.ContainsKey("pad") && Boolean.Parse(m_args["pad"]))
            {
                if (sret.Length > 30)
                    sret = sret.Substring(0, 30);

                sret += "AA";
            }

            return new BitStream(System.Text.ASCIIEncoding.ASCII.GetBytes(sret));
        }

        protected override BitStream internalDecode(BitStream data)
        {
            if (data.LengthBytes % 2 != 0)
                throw new Exception("NetBiosDecode transformer internalEncode failed: Length must be divisible by two.");

            var sb = new System.Text.StringBuilder((int)data.LengthBytes / 2);

            var nbs = System.Text.ASCIIEncoding.ASCII.GetString(data.Value);

            for (int i = 0; i < nbs.Length; i += 2)
            {
                char c1 = nbs[i];
                char c2 = nbs[i + 1];

                var part1 = (c1 - 0x41) * 16;
                var part2 = (c2 - 0x41);

                sb.Append((Char)(part1 + part2));
            }

            return new BitStream(System.Text.ASCIIEncoding.ASCII.GetBytes(sb.ToString()));

        }
    }
}
