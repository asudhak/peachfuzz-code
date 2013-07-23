
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
    [Description("Encode on output as Javascript string.")]
    [Transformer("JsEncode", true)]
    [Transformer("encode.JsEncode")]
    [Serializable]
    public class JsEncode : Transformer
    {
        public JsEncode(Dictionary<string, Variant> args) : base(args)
        {
        }

        protected override BitwiseStream internalEncode(BitwiseStream data)
        {
            BitStream ret = new BitStream();
            BitWriter writer = new BitWriter(ret);
            int b;

            while ((b = data.ReadByte()) != -1)
            {
                if ((b >= 97 && b <= 122) ||
                    (b >= 65 && b <= 90) ||
                    (b >= 48 && b <= 57) ||
                    b == 32 || b == 44 || b == 46)
                    writer.WriteByte((byte)b);
                else if (b <= 127)
                    writer.WriteString(string.Format("\\x{0:X2}", b));
                else
                    //NOTE: Doing at ASCII byte level.. might not not be necesarry here as the string is not typed...
                    writer.WriteString(string.Format("\\u{0:X4}", b));
            }

            ret.Seek(0, System.IO.SeekOrigin.Begin);
            return ret;
        }

        protected override BitStream internalDecode(BitStream data)
        {
            throw new NotImplementedException();
        }
    }
}

// end
