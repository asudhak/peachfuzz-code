
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
//   Mikhail Davidov (sirus@haxsys.net)
//	 Mick Ayzenberg	(mick@dejavusecurity.com)
//
// $Id$

using System;
using System.Collections.Generic;
using System.Text;
using Peach.Core.Dom;
using Peach.Core.Fixups.Libraries;
using System.Runtime.Serialization;

namespace Peach.Core.Fixups
{
	[Description("CRC Fixup library including CRC32 as defined by ISO 3309.")]
	[Fixup("CrcFixup", true)]
	[Fixup("checksums.CrcFixup")]
	[Fixup("Crc32Fixup")]
	[Fixup("checksums.Crc32Fixup")]
	[Parameter("ref", typeof(DataElement), "Reference to data element")]
	[Parameter("type", typeof(CRCTool.CRCCode), "Type of CRC to run [CRC32, CRC16, CRC_CCITT]", "CRC32")]
	[Serializable]
	public class CrcFixup : Fixup
	{
		static void Parse(string str, out DataElement val)
		{
			val = null;
		}

		protected DataElement _ref { get; set; }
		protected CRCTool.CRCCode type { get; set; }

		public CrcFixup(DataElement parent, Dictionary<string, Variant> args)
			: base(parent, args, "ref")
		{
			ParameterParser.Parse(this, args);
		}

		protected override Variant fixupImpl()
		{
			var elem = elements["ref"];
			byte[] data = elem.Value.Value;

			CRCTool crcTool = new CRCTool();
			crcTool.Init(type);
		
			return new Variant((uint)crcTool.crctablefast(data));
		}
	}
}

// end
