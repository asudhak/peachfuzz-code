using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Compression;
using System.IO;
using Peach.Core.Dom;
using Peach.Core.IO;

namespace Peach.Core.Transformers.Type
{
    [TransformerAttribute("AsInt64", "Changes the size of a number.")]
    [TransformerAttribute("type.AsInt64", "Changes the size of a number.")]
    [ParameterAttribute("isSigned", typeof(int), "Signed/Unsigned", false)]
    [ParameterAttribute("isLittleEndian", typeof(int), "Big/Little Endian", false)]
    [Serializable]
    public class AsInt64 : Transformer
    {
        Dictionary<string, Variant> m_args;

        public AsInt64(Dictionary<string, Variant> args) : base(args)
        {
            m_args = args;
        }

        protected override BitStream internalEncode(BitStream data)
        {
            int signed = 1;
            int littleEndian = 1;

            if (m_args.ContainsKey("isSigned"))
                signed = Int32.Parse((string)m_args["isSigned"]);

            if (m_args.ContainsKey("isLittleEndian"))
                littleEndian = Int32.Parse((string)m_args["isLittleEndian"]);


            List<byte> tmp = new List<byte>();

            for (int i = 0; i < data.Value.Length; ++i)
            {
                if (i >= 8)
                    break;
                tmp.Add(data.Value[i]);
            }

            if (tmp.Count < 8)
            {
                int remaining = 8 - tmp.Count;
                for (int i = 0; i < remaining; ++i)
                    tmp.Add(0x00);
            }

            return new BitStream(tmp.ToArray());
        }

        protected override BitStream internalDecode(BitStream data)
        {
            throw new NotImplementedException();
        }
    }
}

// end
