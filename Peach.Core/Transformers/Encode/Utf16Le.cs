
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
    [Description("Encode on output a string as UTF-16LE.  Supports surrogate pair encoding of values larger then 0xFFFF.")]
    [Transformer("Utf16Le", true)]
    [Transformer("encode.Utf16Le")]
    [Serializable]
    public class Utf16Le : Transformer
    {
        public Utf16Le(Dictionary<string, Variant> args) : base(args)
        {
        }

        protected override BitStream internalEncode(BitStream data)
        {
            return new BitStream(System.Text.UnicodeEncoding.Unicode.GetBytes(System.Text.ASCIIEncoding.ASCII.GetString(data.Value)));
        }

        protected override BitStream internalDecode(BitStream data)
        {
            return new BitStream(System.Text.ASCIIEncoding.ASCII.GetBytes(System.Text.UnicodeEncoding.Unicode.GetString(data.Value)));
        }
    }
}

// end
