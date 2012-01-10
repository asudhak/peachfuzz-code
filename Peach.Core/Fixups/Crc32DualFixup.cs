
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
//   Michael Eddington (mike@phed.org)

// $Id$

using System;
using System.Collections.Generic;
using System.Text;
using Peach.Core.Dom;
using Peach.Core.Fixups.Libraries;

namespace Peach.Core.Fixups
{
	[FixupAttribute("Crc32DualFixup", "Standard CRC32 as defined by ISO 3309.")]
	[ParameterAttribute("ref1", typeof(DataElement), "Reference to data element", true)]
	[ParameterAttribute("ref2", typeof(DataElement), "Reference to data element", true)]
	[Serializable]
	public class Crc32DualFixup : Fixup
	{
		public Crc32DualFixup(Dictionary<string, Variant> args)
			: base(args)
		{
			if (!args.ContainsKey("ref1"))
				throw new PeachException("Error, Crc32DualFixup requires a 'ref1' argument!");

            if (!args.ContainsKey("ref2"))
                throw new PeachException("Error, Crc32DualFixup requires a 'ref2' arguement!");
		}

		protected override Variant fixupImpl(DataElement obj)
		{
			string objRef1 = (string)args["ref1"];
			string objRef2 = (string)args["ref2"];
			byte[] data1 = obj.find(objRef1).Value.Value;
			byte[] data2 = obj.find(objRef2).Value.Value;
			byte[] data3 = new byte[data1.Length + data2.Length];

			int cnt = 0;
			for (int cnt1 = 0; cnt1 < data1.Length; cnt1++, cnt++)
				data3[cnt] = data1[cnt1];
			for (int cnt2 = 0; cnt2 < data2.Length; cnt2++, cnt++)
				data3[cnt] = data2[cnt2];

			CRCTool crcTool = new CRCTool();
			crcTool.Init(CRCTool.CRCCode.CRC32);

			return new Variant((uint)crcTool.crctablefast(data3));
		}
	}
}

// end
