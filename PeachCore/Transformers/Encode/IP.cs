
//
// Copyright (c) Michael Eddington
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in	
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

// Authors:
//   Mikhail Davidov (sirus@haxsys.net)

// $Id$

using System;
using System.Collections.Generic;
using System.Text;
using PeachCore.Dom;
namespace PeachCore.Transformers.Encode
{
    [TransformerAttribute("Ipv4StringToOctet", "Encode on output from a dot notation string to a 4 byte octet reprisentaiton.")]
    class Ipv4StringToOctet : Transformer
    {
        public Ipv4StringToOctet(Dictionary<string,Variant>  args) : base(args)
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
        public Ipv4StringToNetworkOctet(Dictionary<string,Variant>  args) : base(args)
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
        public Ipv6StringToOctet(Dictionary<string,Variant> args)
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
