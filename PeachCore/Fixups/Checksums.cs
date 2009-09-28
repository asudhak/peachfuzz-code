
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
using System.Linq;
using System.Text;
using PeachCore.Dom;

namespace PeachCore.Fixups
{
	[FixupAttribute("Crc32Fixup", "Standard CRC32 as defined by ISO 3309.")]
	[ParameterAttribute("ref", DataElement, "Reference to data element", true)]
	public class Crc32Fixup : Fixup
	{
		public override Variant fixup(DataElement obj)
		{
			string objRef = args["ref"] as string;
			DataElement from = obj.find(objRef);
			Variant data = from.Value;

			// Todo: Calc crc32

			throw new NotImplementedException();
		}
	}

	[FixupAttribute("Crc32DualFixup", "Standard CRC32 as defined by ISO 3309.")]
	[ParameterAttribute("ref1", DataElement, "Reference to data element", true)]
	[ParameterAttribute("ref2", DataElement, "Reference to data element", true)]
	public class Crc32DualFixup : Fixup
	{
		public override Variant fixup(DataElement obj)
		{
			string objRef1 = args["ref1"] as string;
			string objRef2 = args["ref2"] as string;
			Variant data1 = obj.find(objRef1).Value;
			Variant data2 = obj.find(objRef2).Value;

			// Todo: Calc crc32

			throw new NotImplementedException();
		}
	}
}

// end
