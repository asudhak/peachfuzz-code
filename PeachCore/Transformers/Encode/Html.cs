
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
using Peach.Core.Dom;
namespace Peach.Core.Transformers.Encode
{
    [TransformerAttribute("HtmlEncodeAgressive", "Encode on output as as HTML agressively.  Only alphanums will not be encoded.")]
    class HtmlEncodeAgressive :Transformer
    {
        public HtmlEncodeAgressive(Dictionary<string,Variant>  args) : base(args)
		{
		}

        protected override BitStream internalEncode(BitStream data)
        {
            var s = System.Text.ASCIIEncoding.ASCII.GetString(data.Value);
            var es = System.Web.HttpUtility.HtmlEncode(s);

            return new BitStream(System.Text.ASCIIEncoding.ASCII.GetBytes(es));
        }

        protected override BitStream internalDecode(BitStream data)
        {
            var s = System.Text.ASCIIEncoding.ASCII.GetString(data.Value);
            var ds = System.Web.HttpUtility.HtmlDecode(s);

            return new BitStream(System.Text.ASCIIEncoding.ASCII.GetBytes(ds));
        }
    }
    [TransformerAttribute("HtmlEncode", "Encode on output as HTML (encoding < > & and \")")]
    class HtmlEncode : Transformer
    {
        public HtmlEncode(Dictionary<string,Variant>  args) : base(args)
		{
		}

        protected override BitStream internalEncode(BitStream data)
        {
            var s = System.Text.ASCIIEncoding.ASCII.GetString(data.Value);
            var es = System.Web.HttpUtility.HtmlAttributeEncode(s);

            return new BitStream(System.Text.ASCIIEncoding.ASCII.GetBytes(es));
        }

        protected override BitStream internalDecode(BitStream data)
        {
            var s = System.Text.ASCIIEncoding.ASCII.GetString(data.Value);
            var ds = System.Web.HttpUtility.HtmlDecode(s);

            return new BitStream(System.Text.ASCIIEncoding.ASCII.GetBytes(ds));
        }
    }
    [TransformerAttribute("JsEncode", "Encode on output as Javascript string.")]
    class JsEncode : Transformer
    {
        public JsEncode(Dictionary<string,Variant>  args) : base(args)
		{
		}

        protected override BitStream internalEncode(BitStream data)
        {
            StringBuilder sb = new StringBuilder((int)data.LengthBytes);

            foreach (byte b in data.Value)
            {
                if ((b >= 97 && b <= 122) ||
                    (b >= 65 && b <= 90) ||
                    (b >= 48 && b <= 57) ||
                    b == 32 || b == 44 || b == 46)
                    sb.Append((Char)b);
                else if (b <= 127)
                    sb.AppendFormat("\\x{0:X2}",b);
                else
                    //NOTE: Doing at ASCII byte level.. might not not be necesarry here as the string is not typed...
                    sb.AppendFormat("\\u{0:X4}",b); 

            }
            
           return new  BitStream(System.Text.ASCIIEncoding.ASCII.GetBytes(sb.ToString()));

        }

        protected override BitStream internalDecode(BitStream data)
        {
            throw new NotImplementedException();
        }
    }
    [TransformerAttribute("HtmlDecode", "Decode on output from HTML encoding.")]
    class HtmlDecode : Transformer
    {
        public HtmlDecode(Dictionary<string,Variant> args)
            : base(args)
		{
		}


        protected override BitStream internalEncode(BitStream data)
        {
            var s = System.Text.ASCIIEncoding.ASCII.GetString(data.Value);
            var ds = System.Web.HttpUtility.HtmlDecode(s);

            return new BitStream(System.Text.ASCIIEncoding.ASCII.GetBytes(ds));
        }

        protected override BitStream internalDecode(BitStream data)
        {
            var s = System.Text.ASCIIEncoding.ASCII.GetString(data.Value);
            var es = System.Web.HttpUtility.HtmlAttributeEncode(s);

            return new BitStream(System.Text.ASCIIEncoding.ASCII.GetBytes(es));
        }
    }

}
