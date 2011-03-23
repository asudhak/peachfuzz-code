using System;
using System.Collections.Generic;
using System.Text;

namespace PeachCore.Transformers.Encode
{
    [TransformerAttribute("Ipv4StringToOctet", "Encode on output from a dot notation string to a 4 byte octet reprisentaiton.")]
    class Ipv4StringToOctet : Transformer
    {
        public Ipv4StringToOctet(Dictionary<string,string> args) : base(args)
		{
		}


        protected override BitStream internalEncode(BitStream data)
        {

            string sip = System.Text.ASCIIEncoding.ASCII.GetString(data.Value);
            var ip = System.Net.IPAddress.Parse(sip);

            return new BitStream(ip.GetAddressBytes());
        }

        protected override BitStream internalDecode(BitStream data)
        {
            throw new NotImplementedException();
        }
    }
    [TransformerAttribute("Ipv4StringToNetworkOctet", "Encode on output from a dot notation string to a 4 byte octet reprisentaiton.")]
    class Ipv4StringToNetworkOctet : Transformer
    {
        public Ipv4StringToNetworkOctet(Dictionary<string,string> args) : base(args)
		{
		}


        protected override BitStream internalEncode(BitStream data)
        {
            string sip = System.Text.ASCIIEncoding.ASCII.GetString(data.Value);

            var ip = System.Net.IPAddress.Parse(sip);
            var ipb = ip.GetAddressBytes();


            int ipaddr = BitConverter.ToInt32(ipb,0);
            int ipaddr_network = System.Net.IPAddress.HostToNetworkOrder(ipaddr);

            return new BitStream(BitConverter.GetBytes(ipaddr_network));


        }

        protected override BitStream internalDecode(BitStream data)
        {
            throw new NotImplementedException();
        }
    }


    [TransformerAttribute("Ipv6StringToOctet", "Encode on output from a collen notiation ipv6 address into a 16 byte octect representation.")]
    class Ipv6StringToOctet : Transformer
    {
        public Ipv6StringToOctet(Dictionary<string, string> args)
            : base(args)
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
