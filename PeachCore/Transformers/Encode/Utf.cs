
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
//   Mikhail Davidov (sirus@haxsys.net)

// $Id$

using System;
using System.Collections.Generic;
using System.Text;
using PeachCore.Dom;
namespace PeachCore.Transformers.Encode
{
    [TransformerAttribute("Utf8", "Encode on output a string as UTF-8.")]
    class Utf8 : Transformer
    {
        public Utf8(Dictionary<string,Variant>  args) : base(args)
		{
		}


        protected override BitStream internalEncode(BitStream data)
        {
            return new BitStream(System.Text.UTF8Encoding.UTF8.GetBytes(System.Text.ASCIIEncoding.ASCII.GetString(data.Value)));
        }

        protected override BitStream internalDecode(BitStream data)
        {

            return new BitStream(System.Text.ASCIIEncoding.ASCII.GetBytes(System.Text.UTF8Encoding.UTF8.GetString(data.Value)));
        }
    }
    //TODO: Validate claims same for C#.
    [TransformerAttribute("Utf16", "Encode on output a string as UTF-16. String is prefixed with a BOM. Supports surrogate pair	encoding of values larger then 0xFFFF.")] 
    class Utf16 : Transformer
    {
        public Utf16(Dictionary<string,Variant>  args) : base(args)
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

    //TODO: Validate claims same for C#.
    [TransformerAttribute("Utf16Le", "Encode on output a string as UTF-16LE.  Supports surrogate pair encoding of values larger then 0xFFFF." )]
    class Utf16Le : Transformer 
    {
        public Utf16Le(Dictionary<string,Variant>  args) : base(args)
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

    //TODO: Validate claims same for C#.
    [TransformerAttribute("Utf16Be", "Encode on output a string as UTF-16BE. Supports surrogate pair encoding of values larger then 0xFFFF.")]
    class Utf16Be : Transformer
    {
        public Utf16Be(Dictionary<string,Variant> args)
            : base(args)
		{
		}


        protected override BitStream internalEncode(BitStream data)
        {
            return new BitStream(System.Text.Encoding.BigEndianUnicode.GetBytes(System.Text.ASCIIEncoding.ASCII.GetString(data.Value)));
        }

        protected override BitStream internalDecode(BitStream data)
        {
            return new BitStream(System.Text.ASCIIEncoding.ASCII.GetBytes(System.Text.Encoding.BigEndianUnicode.GetString(data.Value)));
        }
    }

}
