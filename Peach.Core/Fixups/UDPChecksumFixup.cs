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
using System.Net.Sockets;

namespace Peach.Core.Fixups
{
	[Description("Standard UDP checksum.")]
	[Fixup("UDPChecksumFixup", true)]
	[Fixup("checksums.UDPChecksumFixup")]
	[Parameter("ref", typeof(DataElement), "Reference to data element")]
	[Parameter("src", typeof(IPAddress), "Source IP address")]
	[Parameter("dst", typeof(IPAddress), "Destination IP address")]
	[Serializable]
	public class UDPChecksumFixup : InternetFixup
	{
		public UDPChecksumFixup(DataElement parent, Dictionary<string, Variant> args)
			: base(parent, args, "ref")
		{
		}

		protected override ushort Protocol
		{
			get { return (ushort)ProtocolType.Udp; }
		}

		protected override bool AddLength
		{
			get { return true; }
		}
	}
}
