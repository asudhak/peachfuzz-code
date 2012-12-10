
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
    [Description("Changes the size of a number.")]
    [Transformer("AsInt16", true)]
    [Transformer("type.AsInt16")]
    [Parameter("isSigned", typeof(int), "Signed/Unsigned", "1")]
    [Parameter("isLittleEndian", typeof(int), "Big/Little Endian", "1")]
    [Serializable]
    public class AsInt16 : Transformer
    {
        //Dictionary<string, Variant> m_args;

        public AsInt16(Dictionary<string, Variant> args) : base(args)
        {
            //m_args = args;
        }

        protected override BitStream internalEncode(BitStream data)
        {
            //int signed = 1;
            //int littleEndian = 1;

            //if (m_args.ContainsKey("isSigned"))
            //    signed = Int32.Parse((string)m_args["isSigned"]);

            //if (m_args.ContainsKey("isLittleEndian"))
            //    littleEndian = Int32.Parse((string)m_args["isLittleEndian"]);

            // TODO: FIXME: This doesn't honor signedness or endianness
            byte[] final = new byte[sizeof(Int16)];
            int count = Math.Min(data.Value.Length, final.Length);
            int offset = final.Length - count;
            Buffer.BlockCopy(data.Value, 0, final, offset, count);

            return new BitStream(final);
        }

        protected override BitStream internalDecode(BitStream data)
        {
            throw new NotImplementedException();
        }
    }
}

// end
