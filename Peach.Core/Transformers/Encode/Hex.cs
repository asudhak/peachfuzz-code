
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
using Peach.Core.IO;

namespace Peach.Core.Transformers.Encode
{
	[Serializable]
    [TransformerAttribute("Hex", "Encode on output as a hex string.")]
    [TransformerAttribute("encode.Hex", "Encode on output as a hex string.")]
    public class Hex : Transformer
    {
        public Hex(Dictionary<string,Variant>  args) : base(args)
		{
		}

        protected override BitStream internalEncode(BitStream data)
        {
            StringBuilder sb = new StringBuilder(((int)data.LengthBytes*2));

            foreach (byte b in data.Value)
                sb.AppendFormat("{0:x2}", b);

            return new BitStream(System.Text.ASCIIEncoding.ASCII.GetBytes(sb.ToString()));
        }

        protected override BitStream internalDecode(BitStream data)
        {
            if (data.Value.Length % 2 != 0)
                //TODO: Transformer soft exception?
                throw new Exception("Hex transfromer internalDecode failed: Invalid length.");

            byte[] ret = new byte[data.LengthBytes/2];

            for (int i = 0; i < data.LengthBytes; i += 2)
            {
                int high = (data.Value[i] > 0x40 ? data.Value[i] - 0x37 : data.Value[i] - 0x30) << 4;
                int low = data.Value[i + 1] > 0x40 ? data.Value[i + 1] - 0x37 : data.Value[i] - 0x30;

                ret[i/2] = Convert.ToByte(high + low);

            }

            return new BitStream(ret);
        }
    }
}

// end
