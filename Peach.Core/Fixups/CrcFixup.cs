
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
	[Description("Standard CRC32 as defined by ISO 3309.")]
	[Fixup("CrcFixup", true)]
	[Fixup("checksums.CrcFixup")]
	[Parameter("ref", typeof(DataElement), "Reference to data element")]
	[Parameter("type", typeof(string), "Type of CRC to run [32, 16, CCITT]: Default is 32", "32")]
	[Serializable]
	public class CrcFixup : Fixup
	{
		static void Parse(string str, out DataElement val)
		{
			val = null;
		}

		protected DataElement _ref { get; set; }
		protected string type { get; set; }

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
			if (type.Equals("32") || type.ToUpper().Equals("CRC32"))
			{
				crcTool.Init(CRCTool.CRCCode.CRC32);
			}
			else if(type.Equals("16") || type.ToUpper().Equals("CRC16"))
			{
				crcTool.Init(CRCTool.CRCCode.CRC16);
			}
			else if(type.ToUpper().Equals("CCITT") || type.ToUpper().Equals("CRC_CCITT"))
			{
				crcTool.Init(CRCTool.CRCCode.CRC_CCITT);
			}
			else
			{
				throw new PeachException("CrcFixup does not recognize Crc type: '" + type + "'.");
			}
			return new Variant((uint)crcTool.crctablefast(data));
		}
	}
}

// end
