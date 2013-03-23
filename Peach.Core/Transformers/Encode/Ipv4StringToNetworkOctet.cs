
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
//   Michael Eddington (mike@dejavusecurity.com)

// $Id$

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Peach.Core.Dom;
using Peach.Core.IO;

namespace Peach.Core.Transformers.Encode
{
    [Description("Encode on output from a dot notation string to a 4 byte octet representaiton.")]
    [Transformer("Ipv4StringToNetworkOctet", true)]
    [Transformer("encode.Ipv4StringToNetworkOctet")]
    [Serializable]
    public class Ipv4StringToNetworkOctet : Transformer
    {
        public Ipv4StringToNetworkOctet(Dictionary<string, Variant> args) : base(args)
        {
        }

        protected override BitStream internalEncode(BitStream data)
        {
            string sip = System.Text.ASCIIEncoding.ASCII.GetString(data.Value);

            try
            {
                var ip = IPAddress.Parse(sip);
                var ipb = ip.GetAddressBytes();
                System.Array.Reverse(ipb);

                int ipaddr = BitConverter.ToInt32(ipb, 0);
                int ipaddr_network = IPAddress.HostToNetworkOrder(ipaddr);
                return new BitStream(BitConverter.GetBytes(ipaddr_network));
            }
            catch(Exception ex)
            {
                throw new PeachException("Error, could not convert IP address " + sip, ex);
            }

        }

        protected override BitStream internalDecode(BitStream data)
        {
            throw new NotImplementedException();
        }
    }
}

// end
