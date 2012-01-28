using System;
using System.Collections.Generic;
using System.Text;
using Peach.Core.Dom;
using Peach.Core.Fixups.Libraries;

namespace Peach.Core.Fixups
{
    [FixupAttribute("EthernetChecksumFixup", "Standard ethernet checksum.")]
    [FixupAttribute("checksums.EthernetChecksumFixup", "Standard ethernet checksum.")]
    [ParameterAttribute("ref", typeof(DataElement), "Reference to data element", true)]
    [Serializable]
    public class EthernetChecksumFixup : Fixup
    {
        public EthernetChecksumFixup(Dictionary<string, Variant> args) : base(args)
        {
            if (!args.ContainsKey("ref"))
                throw new PeachException("Error, Crc32Fixup requires a 'ref' argument!");
        }

        protected override Variant fixupImpl(DataElement obj)
        {
            string objRef = (string)args["ref"];
            DataElement from = obj.find(objRef);
            byte[] data = from.Value.Value;

            //CRCTool crcTool = new CRCTool();
            //crcTool.Init(CRCTool.CRCCode.CRC32);
            //return new Variant((uint)crcTool.crctablefast(data, 0x04C11DB7));

            CRC32 crc = new CRC32();
            //crc.ComputeHash(new byte[] { 0x04, 0xC1, 0x1D, 0xB7 });
            crc.ComputeHash(data);

            return new Variant(crc.GetHashCode());
        }
    }
}

// end
