
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
using System.Globalization;
using System.Collections.Generic;
using System.Text;
using System.IO.Compression;
using System.IO;
using Peach.Core.Dom;
using Peach.Core.IO;

namespace Peach.Core.Transformers.Type
{
	[Description("Transforms an integer into hex.")]
	[Transformer("IntToHex", true)]
	[Transformer("type.IntToHex")]
	[Serializable]
	public class IntToHex : Transformer
	{
		public IntToHex(Dictionary<string, Variant> args)
			: base(args)
		{
		}

		protected override BitwiseStream internalEncode(BitwiseStream data)
		{
			string dataAsStr = new BitReader(data).ReadString();
			int dataAsInt = Int32.Parse(dataAsStr);
			string dataAsHexStr = dataAsInt.ToString("X");
			var ret = new BitStream();
			var writer = new BitWriter(ret);
			writer.WriteString(dataAsHexStr);
			ret.Seek(0, System.IO.SeekOrigin.Begin);
			return ret;
		}

		protected override BitStream internalDecode(BitStream data)
		{
			string dataAsHexStr = new BitReader(data).ReadString();
			int dataAsInt = Int32.Parse(dataAsHexStr, NumberStyles.HexNumber);
			string dataAsStr = dataAsInt.ToString();
			var ret = new BitStream();
			var writer = new BitWriter(ret);
			writer.WriteString(dataAsStr);
			ret.Seek(0, System.IO.SeekOrigin.Begin);
			return ret;
		}
	}
}

// end
