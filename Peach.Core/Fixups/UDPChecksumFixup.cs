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
//  Mick Ayzenberg (mick@dejavusecurity.com)
//  Jordyn Puryear (jordyn@dejavusecurity.com)

using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using Peach.Core.Fixups.Libraries;
using Peach.Core.Dom;

namespace Peach.Core.Fixups
{
    [Description("Standard UDP checksum.")]
    [Fixup("UDPChecksumFixup", true)]
    [Fixup("checksums.UDPChecksumFixup")]
    [Parameter("ref", typeof(DataElement), "Reference to data element")]
    [Parameter("src", typeof(string), "Reference to data element")]
    [Parameter("dst", typeof(string), "Reference to data element")]
    [Serializable]
    public class UDPChecksumFixup : Fixup
    {
        public UDPChecksumFixup(DataElement parent, Dictionary<string, Variant> args)
            : base(parent, args, "ref")
        {
            if (!args.ContainsKey("src"))
                throw new PeachException("Error, UDPChecksumFixup requires a 'src' argument!");
            if (!args.ContainsKey("dst"))
                throw new PeachException("Error, UDPChecksumFixup requires a 'dst' argument!");
        }

        protected override Variant fixupImpl()
        {
			var elem = elements["ref"];
			byte[] data = elem.Value.Value;

			InternetFixup fixup = new InternetFixup();
			fixup.ChecksumAddAddress((string)args["src"]);
			fixup.ChecksumAddAddress((string)args["dst"]);

			byte[] protocol;
			byte[] tcpLength;
			if (fixup.isIPv6())
			{
				protocol = new byte[] { 0, 0, 0, 0x11 };
				tcpLength = BitConverter.GetBytes((uint)data.Length);
			}
			else
			{
				protocol = new byte[] { 0, 0x11 };
				tcpLength = BitConverter.GetBytes((ushort)data.Length);
			}

			if (System.BitConverter.IsLittleEndian)
				System.Array.Reverse(tcpLength);

			fixup.ChecksumAddPseudoHeader(protocol);
			fixup.ChecksumAddPseudoHeader(tcpLength);
			fixup.ChecksumAddPayload(data);

			return new Variant(fixup.ChecksumFinal());
        }
    }
}
