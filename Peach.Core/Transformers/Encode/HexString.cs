
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
using System.Linq;
using System.Text;
using Peach.Core.Dom;
using Peach.Core.IO;

namespace Peach.Core.Transformers.Encode
{
    [Serializable]
    [Description("Transforms a string of bytes into the specified hex format.")]
    [Transformer("HexString", true)]
    [Transformer("encode.HexString")]
    [Parameter("resolution", typeof(int), "Number of nibbles between separator. Must be a positive, even int.", "1")]
    [Parameter("prefix", typeof(string), "A value to prepend each chunk with.", " ")]
    public class HexString : Transformer
    {
        Dictionary<string, Variant> m_args;

        public HexString(Dictionary<string, Variant> args) : base(args)
        {
            m_args = args;
        }

        protected override BitStream internalEncode(BitStream data)
        {
            int resolution = 1;
            string prefix = " ";
            if (m_args.ContainsKey("resolution"))
                resolution = Int32.Parse((string)m_args["resolution"]);

            if (resolution % 2 != 0 && resolution != 1)
                throw new Exception("HexString transformer internalEncode failed: Resolution must be 1 or a multiple of two.");

            if (data.LengthBytes % resolution != 0)
                throw new Exception("HexString transformer internalEncode failed: Data length must be divisible by resolution.");

            if (m_args.ContainsKey("prefix"))
                prefix = (string)m_args["prefix"];

            var ret = new System.Text.StringBuilder();
            var tmp = new System.Text.StringBuilder();

            foreach (byte b in data.Value)
            {
                tmp.AppendFormat("{0:x2}", b);

                if (tmp.Length / 2 == resolution)
                {
                    tmp.Append(prefix);
                    ret.Append(tmp.ToString());
                    tmp = new StringBuilder();
                }
            }


            var rets = ret.ToString().Trim();

            return new BitStream(System.Text.ASCIIEncoding.ASCII.GetBytes(rets));
        }

        protected override BitStream internalDecode(BitStream data)
        {
            throw new NotImplementedException();
        }
    }
}

// end
