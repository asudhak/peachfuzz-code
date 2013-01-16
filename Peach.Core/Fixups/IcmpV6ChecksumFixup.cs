
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
//   Mick Ayzenberg (mick@dejavusecurity.com)

// $Id$

using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Linq;
using Peach.Core.Fixups.Libraries;
using Peach.Core.Dom;

namespace Peach.Core.Fixups
{
	[Description("Standard ICMPv6 checksum.")]
	[Fixup("IcmpV6ChecksumFixup", true)]
	[Fixup("checksums.IcmpV6ChecksumFixup")]
	[Parameter("ref", typeof(DataElement), "Reference to data element")]
	[Parameter("src", typeof(string), "Reference to data element")]
	[Parameter("dst", typeof(string), "Reference to data element")]
	[Serializable]
	public class IcmpV6ChecksumFixup : Fixup
	{
		public IcmpV6ChecksumFixup(DataElement parent, Dictionary<string, Variant> args)
			: base(parent, args, "ref")
		{
			if (!args.ContainsKey("src"))
				throw new PeachException("Error, IcmpV6ChecksumFixup requires a 'src' argument!");
			if (!args.ContainsKey("dst"))
				throw new PeachException("Error, IcmpV6ChecksumFixup requires a 'dest' argument!");
		}

		protected override Variant fixupImpl()
		{
			var elem = elements["ref"];
			byte[] data = elem.Value.Value;

			byte[] length = System.BitConverter.GetBytes((uint)data.Length);
			if (System.BitConverter.IsLittleEndian)
				System.Array.Reverse(length);
			byte[] next = new byte[] { 0, 0, 0, 58 };

			InternetFixup fixup = new InternetFixup();
			fixup.ChecksumAddAddress((string)args["src"]);
			fixup.ChecksumAddAddress((string)args["dst"]);
			fixup.ChecksumAddPseudoHeader(length);
			fixup.ChecksumAddPseudoHeader(next);
			fixup.ChecksumAddPayload(data);

			return new Variant(fixup.ChecksumFinal());

		}
	}
}

// end