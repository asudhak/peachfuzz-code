
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
    [TransformerAttribute("SidStringToBytes", "Encode on output from a string reprisentation of a SID to bytes. (Format: S-1-5-21-2127521184-1604012920-1887927527-1712781)")]
    class SidStringToBytes : Transformer
    {
        public SidStringToBytes(Dictionary<string,Variant> args)
            : base(args)
		{
		}

        protected override BitStream internalEncode(BitStream data)
        {
            var sids = System.Text.ASCIIEncoding.ASCII.GetString(data.Value);
            
            //Hopefully this is in mono...
            var sid = new System.Security.Principal.SecurityIdentifier(sids);
            byte[] bsid = new byte[sid.BinaryLength];
            sid.GetBinaryForm(bsid, 0);
            return new BitStream();


        }

        protected override BitStream internalDecode(BitStream data)
        {
            throw new NotImplementedException();
        }
    }
}
