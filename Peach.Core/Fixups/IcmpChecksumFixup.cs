
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
//   Ross Salpino (rsal42@gmail.com)
//   Mikhail Davidov (sirus@haxsys.net)

// $Id$

using System;
using System.Collections.Generic;
using System.Text;
using Peach.Core.Dom;

namespace Peach.Core.Fixups
{
    [FixupAttribute("IcmpChecksumFixup", "Standard ICMP checksum.", true)]
    [FixupAttribute("checksums.IcmpChecksumFixup", "Standard ICMP checksum.")]
    [ParameterAttribute("ref", typeof(DataElement), "Reference to data element", true)]
    [Serializable]
    public class IcmpChecksumFixup : Fixup
    {
        public IcmpChecksumFixup(Dictionary<string, Variant> args) : base(args)
        {
            if (!args.ContainsKey("ref"))
                throw new PeachException("Error, IcmpChecksumFixup requires a 'ref' argument!");
        }

        protected override Variant fixupImpl(DataElement obj)
        {
            string objRef = (string)args["ref"];
            DataElement from = obj.find(objRef);

            if (from == null)
                throw new PeachException(string.Format("IcmpChecksumFixup could not find ref element '{0}'", objRef));

            byte[] data = from.Value.Value;
            uint chcksm = 0;
            int idx = 0;

            // add a byte if not divisible by 2
            if (data.Length % 2 != 0)
                data = ArrayExtensions.Combine(data, new byte[] { 0x00 });

            // calculate checksum
            while (idx < data.Length)
            {
                chcksm += Convert.ToUInt32(BitConverter.ToUInt16(data, idx));
                idx += 2;
            }

            chcksm = (chcksm >> 16) + (chcksm & 0xFFFF);
            chcksm += (chcksm >> 16);

            return new Variant((ushort)(~chcksm));
        }
    }
}

// end
