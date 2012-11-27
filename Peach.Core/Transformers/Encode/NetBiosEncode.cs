
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
using System.Text;
using Peach.Core.Dom;
using Peach.Core.IO;

namespace Peach.Core.Transformers.Encode
{
    [Description("Encode on output from a string to a binary NetBios representation.")]
    [Parameter("pad", typeof(bool), "Should the NetBios names be padded/trimmed to 32 bytes?", "false")]
    [Transformer("NetBiosEncode", true)]
    [Transformer("encode.NetBiosEncode")]
    [Serializable]
    public class NetBiosEncode : Transformer
    {
        Dictionary<string, Variant> m_args;

        public NetBiosEncode(Dictionary<string, Variant> args) : base(args)
        {
            m_args = args;
        }

        protected override BitStream internalEncode(BitStream data)
        {
            string name = System.Text.ASCIIEncoding.ASCII.GetString(data.Value).ToUpper();
            var sb = new System.Text.StringBuilder(32);

            if (m_args.ContainsKey("pad") && Boolean.Parse((string)m_args["pad"]))
                while (name.Length < 16)
                    name += " ";

            if (name.Length > 16)
                name = name.Substring(0, 16);

            foreach (char c in name)
            {
                var ascii = (int)c;
                sb.Append((Char)((ascii / 16) + 0x41));
                sb.Append((Char)((ascii - (ascii / 16 * 16) + 0x41)));
            }

            var sret = sb.ToString();

            if (m_args.ContainsKey("pad") && Boolean.Parse((string)m_args["pad"]))
            {
                if (sret.Length > 30)
                    sret = sret.Substring(0, 30);

                sret += "AA";
            }

            return new BitStream(System.Text.ASCIIEncoding.ASCII.GetBytes(sret));
        }

        protected override BitStream internalDecode(BitStream data)
        {
            if (data.LengthBytes % 2 != 0)
                throw new Exception("NetBiosDecode transformer internalEncode failed: Length must be divisible by two.");

            var sb = new System.Text.StringBuilder((int)data.LengthBytes / 2);

            var nbs = System.Text.ASCIIEncoding.ASCII.GetString(data.Value);

            for (int i = 0; i < nbs.Length; i += 2)
            {
                char c1 = nbs[i];
                char c2 = nbs[i + 1];

                var part1 = (c1 - 0x41) * 16;
                var part2 = (c2 - 0x41);

                sb.Append((Char)(part1 + part2));
            }

            return new BitStream(System.Text.ASCIIEncoding.ASCII.GetBytes(sb.ToString()));
        }
    }
}

// end
