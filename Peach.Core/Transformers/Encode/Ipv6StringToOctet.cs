using System;
using System.Collections.Generic;
using System.Text;
using Peach.Core.Dom;
using Peach.Core.IO;

namespace Peach.Core.Transformers.Encode
{
    [TransformerAttribute("Ipv6StringToOctet", "Encode on output from a colon notation ipv6 address into a 16 byte octect representation.")]
    [TransformerAttribute("encode.Ipv6StringToOctet", "Encode on output from a colon notation ipv6 address into a 16 byte octect representation.")]
    [Serializable]
    public class Ipv6StringToOctet : Transformer
    {
        public Ipv6StringToOctet(Dictionary<string, Variant> args) : base(args)
        {
        }

        protected override BitStream internalEncode(BitStream data)
        {
            string ipstr = System.Text.ASCIIEncoding.ASCII.GetString(data.Value);
            var ip = System.Net.IPAddress.Parse(ipstr);
            return new BitStream(ip.GetAddressBytes());
        }

        protected override BitStream internalDecode(BitStream data)
        {
            throw new NotImplementedException();
        }
    }
}

// end
