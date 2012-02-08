using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Compression;
using System.IO;
using Peach.Core.Dom;
using Peach.Core.IO;

namespace Peach.Core.Transformers.Type
{
    [TransformerAttribute("AsInt32", "Changes the size of a number.")]
    [TransformerAttribute("type.AsInt32", "Changes the size of a number.")]
    [ParameterAttribute("isSigned", typeof(int), "Signed/Unsigned", false)]
    [ParameterAttribute("isLittleEndian", typeof(int), "Big/Little Endian", false)]
    [Serializable]
    public class AsInt32 : Transformer
    {
        Dictionary<string, Variant> m_args;

        public AsInt32(Dictionary<string, Variant> args) : base(args)
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

            byte[] final;
            int len = data.Value.Length;
            int sz = sizeof(Int32);

            if (len == sz)
            {
                final = data.Value;
            }
            else if (len < sz)
            {
                int remaining = sz - len;
                List<byte> tmp = new List<byte>();

                while (remaining > 0)
                {
                    tmp.Add(0x00);
                    --remaining;
                }

                final = ArrayExtensions.Combine(data.Value, tmp.ToArray());
            }
            else
            {
                final = ArrayExtensions.Slice(data.Value, 0, sz);
            }

            return new BitStream(final);
        }

        protected override BitStream internalDecode(BitStream data)
        {
            throw new NotImplementedException();
        }
    }
}

// end
