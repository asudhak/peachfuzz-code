
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
using System.IO.Compression;
using System.IO;
using Peach.Core.Dom;
using Peach.Core.IO;

namespace Peach.Core.Transformers.Type
{
    [TransformerAttribute("AsInt32", "Changes the size of a number.")]
    [TransformerAttribute("type.AsInt32", "Changes the size of a number.")]
    [ParameterAttribute("isSigned", typeof(int), "Signed/Unsigned", false)]
    [ParameterAttribute("isLittleEndian", typeof(int), "Big/Little Endian", false)]
    [Serializable]
    public class AsInt32 : Transformer
    {
        Dictionary<string, Variant> m_args;

        public AsInt32(Dictionary<string, Variant> args) : base(args)
        {
            m_args = args;
        }

        protected override BitStream internalEncode(BitStream data)
        {
            int signed = 1;
            int littleEndian = 1;

            if (m_args.ContainsKey("isSigned"))
                signed = Int32.Parse((string)m_args["isSigned"]);

            if (m_args.ContainsKey("isLittleEndian"))
                littleEndian = Int32.Parse((string)m_args["isLittleEndian"]);

            byte[] final;
            int len = data.Value.Length;
            int sz = sizeof(Int32);

            if (len == sz)
            {
                final = data.Value;
            }
            else if (len < sz)
            {
                int remaining = sz - len;
                List<byte> tmp = new List<byte>();

                while (remaining > 0)
                {
                    tmp.Add(0x00);
                    --remaining;
                }

                final = ArrayExtensions.Combine(data.Value, tmp.ToArray());
            }
            else
            {
                final = ArrayExtensions.Slice(data.Value, 0, sz);
            }

            return new BitStream(final);
        }

        protected override BitStream internalDecode(BitStream data)
        {
            throw new NotImplementedException();
        }
    }
}

// end
