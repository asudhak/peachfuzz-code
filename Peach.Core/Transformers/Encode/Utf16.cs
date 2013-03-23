
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
    //TODO: Validate claims same for C#.
    [Description("Encode on output a string as UTF-16. String is prefixed with a BOM. Supports surrogate pair	encoding of values larger then 0xFFFF.")]
    [Transformer("Utf16", true)]
    [Transformer("encode.Utf16")]
    [Serializable]
    public class Utf16 : Transformer
    {
        static byte[] _preamble = System.Text.Encoding.Unicode.GetPreamble();

        public Utf16(Dictionary<string, Variant> args) : base(args)
        {
        }

        protected override BitStream internalEncode(BitStream data)
        {
            string value = System.Text.Encoding.ASCII.GetString(data.Value);
            int len = System.Text.Encoding.Unicode.GetByteCount(value);
            byte[] buf = new byte[_preamble.Length + len];
            Buffer.BlockCopy(_preamble, 0, buf, 0, _preamble.Length);
            System.Text.Encoding.Unicode.GetBytes(value, 0, value.Length, buf, _preamble.Length);
            return new BitStream(buf);
        }

        protected override BitStream internalDecode(BitStream data)
        {
            string value = Encoding.Unicode.GetString(data.Value);
            byte[] buf = Encoding.ASCII.GetBytes(value);
            return new BitStream(buf);
        }
    }
}

// end
