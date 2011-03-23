using System;
using System.Collections.Generic;
using System.Text;

namespace PeachCore.Transformers.Encode
{
    [TransformerAttribute("WideChar", "Encode on output a string as wchar string.")] 
    class WideChar : Transformer
    {
        public WideChar(Dictionary<string, string> args)
            : base(args)
		{
		}

        protected override BitStream internalEncode(BitStream data)
        {
            byte[] ret = new byte[data.LengthBytes * 2];
            for (ulong i = 0; i < data.LengthBytes; i++)
            {
                ret[i * 2] = data.Value[i];
                ret[i * 2 + 1] = (Byte)0; 
            }

            return new BitStream(ret);
        }
        protected override BitStream internalDecode(BitStream data)
        {
            if (data.LengthBytes % 2 != 0)
                //TODO: transformer soft exception?
                throw new Exception("WideChar transfromer internalDecode failed: Invalid length.");

            byte[] ret = new byte[data.LengthBytes / 2];

            for (ulong i = 0; i < data.LengthBytes; i += 2)
                ret[i / 2] = data.Value[i];

            return new BitStream(ret);
        }
    }
}
