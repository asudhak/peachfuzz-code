
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
	[Description("Encode on output as a hex string.")]
	[Transformer("Hex", true)]
	[Transformer("encode.Hex")]
	[Serializable]
	public class Hex : Transformer
	{
		public Hex(Dictionary<string, Variant> args)
			: base(args)
		{
		}

		protected override BitStream internalEncode(BitStream data)
		{
			var buf = data.Value;
			StringBuilder sb = new StringBuilder(buf.Length * 2);

			foreach (byte b in buf)
				sb.AppendFormat("{0:x2}", b);

			return new BitStream(Encoding.ASCII.GetBytes(sb.ToString()));
		}

		protected override BitStream internalDecode(BitStream data)
		{
			var buf = data.Value;

			if (buf.Length % 2 != 0)
				//TODO: Transformer soft exception?
				throw new Exception("Hex transfromer internalDecode failed: Invalid length.");

			byte[] ret = new byte[buf.Length / 2];

			for (int i = 0; i < buf.Length; i += 2)
			{
				int nibble1 = GetNibble(buf[i]);
				int nibble2 = GetNibble(buf[i + 1]);

				if (nibble1 < 0 || nibble1 > 0xF || nibble2 < 0 | nibble2 > 0xF)
					//TODO: Transformer soft exception?
					throw new Exception("Hex transfromer internalDecode failed: Invalid bytes.");

				ret[i / 2] = (byte)((nibble1 << 4) | nibble2);
			}

			return new BitStream(ret);
		}

		private static int GetNibble(byte c)
		{
			if (c >= 'a')
				return 0xA + (int)(c - 'a');
			else if (c >= 'A')
				return 0xA + (int)(c - 'A');
			else
				return (int)(c - '0');
		}
	}
}

// end
