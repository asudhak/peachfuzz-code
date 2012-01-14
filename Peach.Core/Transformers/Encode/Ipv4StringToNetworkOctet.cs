using System;
using System.Collections.Generic;
using System.Text;
using Peach.Core.Dom;
using Peach.Core.IO;

namespace Peach.Core.Transformers.Encode
{
    [TransformerAttribute("Ipv4StringToNetworkOctet", "Encode on output from a dot notation string to a 4 byte octet reprisentaiton.")]
    [TransformerAttribute("encode.Ipv4StringToNetworkOctet", "Encode on output from a dot notation string to a 4 byte octet reprisentaiton.")]
    public class Ipv4StringToNetworkOctet : Transformer
    {
        public Ipv4StringToNetworkOctet(Dictionary<string, Variant> args) : base(args)
        {
        }

        protected override BitStream internalEncode(BitStream data)
        {
            string sip = System.Text.ASCIIEncoding.ASCII.GetString(data.Value);

            var ip = System.Net.IPAddress.Parse(sip);
            var ipb = ip.GetAddressBytes();

            int ipaddr = BitConverter.ToInt32(ipb, 0);
            int ipaddr_network = System.Net.IPAddress.HostToNetworkOrder(ipaddr);

            return new BitStream(BitConverter.GetBytes(ipaddr_network));
        }

        protected override BitStream internalDecode(BitStream data)
        {
            throw new NotImplementedException();
        }
    }
}

// end
